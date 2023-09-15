using MongoDB.Bson.Serialization.Attributes;

namespace DinnerPlaner.Storage.Models
{
    public class Reciepe
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string DishName { get; set; }
        public string CountryName { get; set; }
        public string DishSummary { get; set; }
        public string Difficulty { get; set; }
        public string GrandMaNotes { get; set; }
        public string StepByStepReciepe { get; set; }
        public string NutritionalValue { get; set; }
        public string Ingridients { get; set; }
    }
}
