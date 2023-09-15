
using DinnerPlaner.Storage.Models;
using DinnerPlaner.Storage.MongoDb;
using MongoDB.Driver;
using System;

namespace DinnerPlaner.Storage.Repositories
{
    public class ReciepeRepository
    {
        private readonly IMongoCollection<Reciepe> _reciepeCollection;
        public ReciepeRepository(DbClient dbClient)
        {
             _reciepeCollection = dbClient.GetReciepeDataCollection();
        }

        public async Task AddReciepe(Reciepe reciepe)
        {
            await _reciepeCollection.InsertOneAsync(reciepe);
        }

        public async Task<bool> ExistInCountry(string country, string dishName)
        {
           var reciepe = await _reciepeCollection.FindSync(r => r.CountryName == country && r.DishName == dishName).FirstOrDefaultAsync();
           if (reciepe == null)
            {
                return false;
            }
           else {
                return true;
            }
        }

        public async Task<IEnumerable<Reciepe>> GetAllReceipes()
        {
            var reciepes = await _reciepeCollection.Find(r => true).ToListAsync();
            return reciepes.ToList();
        }

        public async Task<Reciepe> GetGeneratedReceipeById(string id)
        {
            var reciepe = await _reciepeCollection.FindSync(r => r.Id == id).FirstOrDefaultAsync();
            return reciepe;
        }

        public async Task<List<Reciepe>> GetGeneratedReceipesByCountry(string country)
        {
            var engagements = await _reciepeCollection.Find(r => r.CountryName == country).ToListAsync();
            return engagements.OfType<Reciepe>().ToList();
        }
    }
}
