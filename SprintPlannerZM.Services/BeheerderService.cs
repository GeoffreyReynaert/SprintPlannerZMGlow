using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class BeheerderService: IBeheerderService
    {
        private readonly TihfDbContext _database;

        public BeheerderService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Beheerder> GetAsync(int id)
        {
            return  _database.Beheerder.SingleOrDefault(d => d.beheerderID == id);
        }

        public async Task<IList<Beheerder>> FindAsync()
        {
            return _database.Beheerder.ToList();
        }

        public async Task<Beheerder> CreateAsync(Beheerder beheerder)
        {
            _database.Beheerder.Add(beheerder);
            _database.SaveChanges();
            return beheerder;
        }

        public async Task<Beheerder> UpdateAsync(int id, Beheerder beheerder)
        {
            {
                var dBeheerder = await GetAsync(id);
                if (dBeheerder == null)
                {
                    return beheerder;
                }
                _database.Beheerder.Update(dBeheerder);
                _database.SaveChanges();
                return beheerder;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            {
                var dBeheerder = await GetAsync(id);
                if (dBeheerder == null)
                {
                    return false;
                }
                _database.Beheerder.Remove(dBeheerder);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
