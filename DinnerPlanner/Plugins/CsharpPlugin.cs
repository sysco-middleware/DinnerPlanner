using DinnerPlaner.Storage.Repositories;
using DinnerPlanner.Models;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Newtonsoft.Json;

namespace DinnerPlanner.Pugins
{
    public class CsharpPlugin
    {
        private readonly ReciepeRepository _reciepeRepository;
        private readonly ILogger _logger;   

        public CsharpPlugin(ReciepeRepository reciepeRepository, ILogger logger)
        {
            _reciepeRepository = reciepeRepository;
            _logger = logger;
        }

        [SKFunction("Check that generated Dish is not in the database")]
        [SKFunctionName("GenerateValidDish")]
        [SKFunctionContextParameter(Name = "dishType", Description = "type of dish to generate")]
        [SKFunctionContextParameter(Name = "number", Description = "number of dishes to generate")]
        public async Task<string> GenerateValidDish(SKContext context)
        {
            var country = context["idea"];
            var dishIdeas = context.Skills.GetSemanticFunction("CookingPlugin", "DishIdeaSemanticFunction");

            DishIdea dishResult = new DishIdea();
            var existingDishes = new List<string>();
            var newDish = false;
            while (newDish == false)
            {
                var result = await dishIdeas.InvokeWithCustomInputAsync(context.Variables, null, null, null, new CancellationToken());
                _logger.LogInformation($"Generated Dish idea {result} ");
                dishResult = JsonConvert.DeserializeObject<DishIdea>(result.Result);
                if (!await _reciepeRepository.ExistInCountry(country, dishResult.DishName))
                {
                    newDish = true;

                }
                else
                {
                    _logger.LogInformation("Dish Already stored");
                    existingDishes.Add(dishResult.DishName);
                    context.Variables.Set("storedDishes", JsonConvert.SerializeObject(existingDishes));
                    _logger.LogInformation($"Stored dishes {JsonConvert.SerializeObject(existingDishes)}");

                }
            }

            return JsonConvert.SerializeObject(dishResult);
        }
    }
}
