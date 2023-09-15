using MongoDB.Bson.Serialization.Attributes;

namespace DinnerPlaner.Storage.Models
{
    public class GeneratedContry
    {

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string ContryName { get; set; }
        public int NumberOfRecepies { get; set; }
    }
}
