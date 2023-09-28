using DinnerPlaner.Storage.Models;
using DinnerPlaner.Storage.MongoDb;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinnerPlaner.Storage.Repositories
{
    public class QuestionAnswerRepository
    {
        private readonly IMongoCollection<QuestionAnswer> _generatedQACollection;

        public QuestionAnswerRepository(DbClient dbClient)
        {
            _generatedQACollection = dbClient.GetQuestionAnswerDataCollection();
        }

        public async Task AddQuestionAnswers(QuestionAnswer qa)
        {
            await _generatedQACollection.InsertOneAsync(qa);
        }

        public async Task<QuestionAnswer> GetCountry(string id)
        {
           return  await  _generatedQACollection.FindSync(c => c.Id == id).FirstOrDefaultAsync();
        }
    }
}
