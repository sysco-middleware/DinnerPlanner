using DinnerPlaner.Storage.Helpers;
using DinnerPlaner.Storage.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DinnerPlaner.Storage.MongoDb
{
    public class DbClient
    {
        //TODO Split into local and prod DbClient 
        private readonly IMongoCollection<GeneratedContry> _generatedCounteriesCollection;
        private readonly IMongoCollection<Reciepe> _reciepeCollection;

        public DbClient(IOptions<DbConfig> dbConfig)
        {
            //for local development
            var connectionString = dbConfig.Value.ConnectionString;
            var mongoUrl = new MongoUrl(connectionString);
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            settings.SslSettings = new SslSettings
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12,
            };
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            settings.UseTls = true;

            var client = new MongoClient(settings);
            var database = client.GetDatabase(dbConfig.Value.DatabaseName);
            _generatedCounteriesCollection = database.GetCollection<GeneratedContry>(DbCollectionTypes.GENERATEDCOUNTRIESCOLLECTION);
            _reciepeCollection = database.GetCollection<Reciepe>(DbCollectionTypes.RECIEPECOLLECTION);
        }

        public IMongoCollection<GeneratedContry> GetGeneratedCountriesDataCollection() => _generatedCounteriesCollection;
        public IMongoCollection<Reciepe> GetReciepeDataCollection() => _reciepeCollection;
    }

}
