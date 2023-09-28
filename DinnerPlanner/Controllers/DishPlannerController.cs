using DinnerPlaner.Storage.Models;
using DinnerPlaner.Storage.Repositories;
using DinnerPlanner.Dto;
using DinnerPlanner.Models;
using DinnerPlanner.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace DinnerPlanner.Controllers
{
    [ApiController]
    [Route("DinnerPlanner")]
    public class DishPlannerController : ControllerBase
    {
        
        private readonly ILogger<DishPlannerController> _logger;
        private readonly SemanticKernel _semanticKernel;
        private readonly GeneratedCountriesRepository _countriesRepository;
        private readonly ReciepeRepository _reciepeRepository;


        public DishPlannerController(ILogger<DishPlannerController> logger, SemanticKernel semanticKernel, GeneratedCountriesRepository generatedCountries, ReciepeRepository reciepeRepository)
        {
            _logger = logger;
            _semanticKernel = semanticKernel;
            _countriesRepository = generatedCountries;
            _reciepeRepository = reciepeRepository;
        }

        [HttpPost]
        [Route("question")]
        [ProducesResponseType(typeof(IEnumerable<CompleteDish>), StatusCodes.Status200OK)]
        public async Task<ActionResult<QuestionAnswer>> AnswerQuestion(string question)
        {

            _logger.LogInformation($"Requested question with test {question}");
            var qa = await _semanticKernel.AnswerQuestion(question);
            return Ok(qa);
        }


        [HttpPost]
        [Route("dishIdeas")]
        [ProducesResponseType(typeof(IEnumerable<CompleteDish>), StatusCodes.Status200OK)]
        public async Task<ActionResult<CompleteDish>> GenerateDishIdeasAsync(string dishType)
        {
            _logger.LogInformation($"Requested Generation of a dish from {dishType}");
            var dish = await _semanticKernel.GenerateDishIdeas(dishType );

            var newReciepe = new Reciepe()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                DishName = dish.DishName,
                CountryName = dishType,
                DishSummary = dish.DishSummary,
                Difficulty = dish.Difficulty,
                GrandMaNotes = dish.GrandMaNotes,
                StepByStepReciepe = dish.Instructions.ToJson(),
                NutritionalValue = dish.NutritionalValuePer100g.ToJson(),
                Ingridients = dish.Ingredient.ToJson()
            };

            await _reciepeRepository.AddReciepe(newReciepe);
            _logger.LogInformation($"Generated dish {dish}");
            _logger.LogInformation("Saving to memory");
            _semanticKernel.SaveToMemory(newReciepe);
            return Ok(dish);
        }


        [HttpGet]
        [Route("countries")]
        [ProducesResponseType(typeof(IEnumerable<GeneratedContry>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GeneratedContry>>> GetGeneratedCountryNames()
        {
            var storedReciepes = await _reciepeRepository.GetAllReceipes();
            var storedCountries = new List<GeneratedContry>();
            foreach(var storedReciepe in storedReciepes)
            {
                var matchedCountry = storedCountries.FirstOrDefault(c => c.ContryName == storedReciepe.CountryName);
                if ( matchedCountry == null)
                {
                    storedCountries.Add(new GeneratedContry
                    {
                        ContryName = storedReciepe.CountryName,
                        NumberOfRecepies = 1
                    });
                }
                else
                {
                   matchedCountry.NumberOfRecepies ++;
                }
            }

            return Ok(storedCountries);
        }


        [HttpGet]
        [Route("storedReciepes")]
        [ProducesResponseType(typeof(IEnumerable<CompleteDish>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompleteDish>>> GetStoredReciepiesByCountry(string country)
        {
            var storeddishes = await _reciepeRepository.GetGeneratedReceipesByCountry(country);
            var dishes = new List<CompleteDish>();
            foreach (var dish in storeddishes)
            {
                dishes.Add(new CompleteDish()
                {
                    DishName = dish.DishName,
                    DishSummary = dish.DishSummary,
                    Difficulty = dish.Difficulty,
                    GrandMaNotes = dish.GrandMaNotes,
                    Ingredient = JsonConvert.DeserializeObject<List<string>>(dish.Ingridients),
                    NutritionalValuePer100g = JsonConvert.DeserializeObject<NutritionalValue>(dish.NutritionalValue),
                    Instructions = JsonConvert.DeserializeObject<List<InstructionStep>>(dish.StepByStepReciepe)
                });
            }
            return Ok(dishes);
        }

        [HttpGet]
        [Route("searchReciepes")]
        [ProducesResponseType(typeof(IEnumerable<CompleteDish>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompleteDish>>> GetStoredReciepiesBySearchPhrase(string searchquery)
        {
            var reciepes = await _semanticKernel.SearchMemoryAsync(searchquery);
            var dishes = new List<CompleteDish>();
            foreach (var dish in reciepes)
            {
                dishes.Add(new CompleteDish()
                {
                    DishName = dish.DishName,
                    DishSummary = dish.DishSummary,
                    Difficulty = dish.Difficulty,
                    GrandMaNotes = dish.GrandMaNotes,
                    Ingredient = JsonConvert.DeserializeObject<List<string>>(dish.Ingridients),
                    NutritionalValuePer100g = JsonConvert.DeserializeObject<NutritionalValue>(dish.NutritionalValue),
                    Instructions = JsonConvert.DeserializeObject<List<InstructionStep>>(dish.StepByStepReciepe)
                });
            }
            return Ok(dishes);
        }

        [HttpGet]
        [Route("initializeMemory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> InitializeMemory()
        {
            await _semanticKernel.InitializeMemory();
            return Ok();
        }

    }
}