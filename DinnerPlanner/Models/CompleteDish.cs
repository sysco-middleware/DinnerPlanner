using MongoDB.Bson.Serialization.Attributes;

namespace DinnerPlanner.Models
{
    public class CompleteDish: DishIdea
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Difficulty { get; set; }
        public string GrandMaNotes { get; set; }
        public List<string> Ingredient { get; set; }
        public NutritionalValue NutritionalValuePer100g { get; set; }
        public List<InstructionStep> Instructions { get; set; }

    }
}
