using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Configuration;
using System.Reflection;
using DinnerPlanner.Models;
using Microsoft.SemanticKernel.Orchestration;
using Newtonsoft.Json;
using DinnerPlanner.Pugins;
using DinnerPlaner.Storage.Repositories;
using Microsoft.SemanticKernel.Memory;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using DinnerPlaner.Storage.Models;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DinnerPlanner.Services
{
    public class SemanticKernel
    {
        private readonly IConfiguration _config;
        private readonly IKernel _kernel;
        private readonly string _pluginDirectory;
        private readonly ReciepeRepository _reciepeRepository;
        private readonly ILogger _logger;
        private readonly string MEMORYNAME = "Reciepe";

        public SemanticKernel(IConfiguration config, ReciepeRepository reciepeRepository, ILogger<SemanticKernel> logger)
        {
            _reciepeRepository = reciepeRepository;
            _config = config;
            _kernel = Kernel.Builder.Configure( c =>
            {
                    c.AddAzureOpenAITextCompletion("davinci", _config["model"], _config["endpoint"], _config["apiKey"]);
                    c.AddAzureOpenAIEmbeddingGeneration("ada", "text-embedding-ada-002", _config["endpoint"], _config["apiKey"]);
            }
                ).WithMemoryStorage(new VolatileMemoryStore()).Build();




            _pluginDirectory = Path.Combine(GetProjectDirectory(), "Plugins");
            _logger = logger;   
        }
        public async Task<QuestionAnswer> AnswerQuestion(string question)
        {
            var questionAnswerSkill = _kernel.ImportSemanticSkillFromDirectory(_pluginDirectory, "QuestionAnswer");
            var myContext1 = new ContextVariables();
            myContext1.Set("question", question);
            var result = await _kernel.RunAsync(myContext1, questionAnswerSkill["AnswerQuestion"]);
            var qa = new QuestionAnswer()
            {
                Question = question,
                Answer = result.Result,
                Inserted = DateTime.UtcNow
            };
            return qa;
        }

        public async Task<CompleteDish> GenerateDishIdeas(string dishType)
        {
            //load skills and native functions
            var dishIdeaSkill = _kernel.ImportSemanticSkillFromDirectory(_pluginDirectory, "CookingPlugin");
            var nativePlugin = _kernel.ImportSkill(new CsharpPlugin(_reciepeRepository, _logger ), "CsharpPlugin");

            //set context variables
            var myContext1 = new ContextVariables();
            myContext1.Set("idea", dishType);
            var result = await _kernel.RunAsync(myContext1, nativePlugin["GenerateValidDish"]);
            _logger.LogInformation($"Generated dishIdea {result}");
            _logger.LogInformation($"Fixing possible JSON fromat errors");
            myContext1.Set("json", result.Result);
            result = await _kernel.RunAsync(myContext1, dishIdeaSkill["FixJsonFormat"]);

            var dishWithDifficulties = new List<CompleteDish>();
            var dishIdea = JsonConvert.DeserializeObject<DishIdea>(result.Result);

            var thisDishWithDifficulty = new CompleteDish()
            {
                DishName = dishIdea.DishName,
                DishSummary = dishIdea.DishSummary,
            };
            var myContext2 = new ContextVariables();
            myContext2.Set("originCountry", dishType);
            myContext2.Set("DishIdea", JsonConvert.SerializeObject(dishIdea));
            var gradMaAnswers = @"###";
            //GRANDMA
            for (int i = 0; i < 4; i++)
            {

                gradMaAnswers = gradMaAnswers + @$"ANSWER {i}";
                result = await _kernel.RunAsync(myContext2, dishIdeaSkill["GrandMaDifficultySemanticFunction"]);
                gradMaAnswers = gradMaAnswers + result;
            }
            
            _logger.LogInformation($"Generated grandMaAnswers {gradMaAnswers}");

            //GRANDMA JUDGE
            var myContext3 = new ContextVariables();
            myContext3.Set("type", dishType);
            myContext3.Set("DishIdea", JsonConvert.SerializeObject(dishIdea));
            myContext3.Set("answers", gradMaAnswers);
            result = await _kernel.RunAsync(myContext3, dishIdeaSkill["GrandMaLogicJudge"]);
            var difficulty = JsonConvert.DeserializeObject<DishDifficulty>(result.ToString());
            thisDishWithDifficulty.Difficulty = difficulty.Difficulty;
            thisDishWithDifficulty.GrandMaNotes = difficulty.Notes;
            _logger.LogInformation($"Judged grandMaAnswers to be best {result.ToString()}");

            //CHEF AND NUTRITIONAL EXPERT
            var myContext4 = new ContextVariables();
            myContext4.Set("dishdescription", JsonConvert.SerializeObject(thisDishWithDifficulty));
            result = await _kernel.RunAsync(myContext4, dishIdeaSkill["NutritionalChef"]);
            var chefOutPut = JsonConvert.DeserializeObject<ChefOutput>(result.ToString());
            thisDishWithDifficulty.Ingredient = chefOutPut.Ingredients;
            thisDishWithDifficulty.NutritionalValuePer100g = chefOutPut.NutritionalValuePer100g;
            thisDishWithDifficulty.Instructions = chefOutPut.Instructions;
            _logger.LogInformation($"generated recipes and nutritional values");
            return thisDishWithDifficulty;
        }

        public async Task InitializeMemory()
        {
            var files = await _reciepeRepository.GetAllReceipes();
            foreach(var file in files)
            {
                await _kernel.Memory.SaveInformationAsync(MEMORYNAME, JsonConvert.SerializeObject(file), file.Id);
            }
        }

        public async Task SaveToMemory(Reciepe reciepe)
        {
            await _kernel.Memory.SaveInformationAsync(MEMORYNAME, reciepe.Id, JsonConvert.SerializeObject(reciepe));   
        }

        public async Task<List<Reciepe>> SearchMemoryAsync(string searchKeywords)
        {
            var memories =  _kernel.Memory.SearchAsync(MEMORYNAME, searchKeywords, limit: 5, minRelevanceScore: 0.77);
            var reciepes = new List<Reciepe>();
            await foreach(var mem in memories)
            {
                reciepes.Add(await _reciepeRepository.GetGeneratedReceipeById(mem.Id));
            }

            return reciepes;
        }

        private string GetProjectDirectory()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string projectDirectory = Path.GetDirectoryName(assemblyPath);
            return projectDirectory;
        }

    }
}
