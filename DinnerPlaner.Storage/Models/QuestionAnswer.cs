using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinnerPlaner.Storage.Models
{
    public class QuestionAnswer
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime Inserted { get; set; }
    }
}
