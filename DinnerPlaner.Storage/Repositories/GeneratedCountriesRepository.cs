using DinnerPlaner.Storage.Models;
using DinnerPlaner.Storage.MongoDb;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace DinnerPlaner.Storage.Repositories
{
    public class GeneratedCountriesRepository
    {
        private readonly IMongoCollection<GeneratedContry> _generatedCountryCollection;

        public GeneratedCountriesRepository(DbClient dbClient)
        {
            _generatedCountryCollection = dbClient.GetGeneratedCountriesDataCollection();
        }

        public async Task AddCountry(GeneratedContry contry)
        {
            await _generatedCountryCollection.InsertOneAsync(contry);
        }

        public async Task<GeneratedContry> GetCountry(string dishType)
        {
           return  await  _generatedCountryCollection.FindSync(c => c.ContryName == dishType).FirstOrDefaultAsync();
        }

        public async Task<List<GeneratedContry>> GetGeneratedContriesAsync()
        {
            var engagements = await _generatedCountryCollection.Find(c => true).ToListAsync();
            return engagements.OfType<GeneratedContry>().ToList();
        }

        public async Task UpdateCountryAsync(GeneratedContry country)
        {

            var upCountry = country;

            var filter = Builders<GeneratedContry>.Filter
            .Eq(c => c.NumberOfRecepies, country.NumberOfRecepies);
            var update = Builders<GeneratedContry>.Update
                .Set(c => c.NumberOfRecepies, country.NumberOfRecepies + 1);
            await _generatedCountryCollection.UpdateOneAsync(filter, update);
        }
    }
}
