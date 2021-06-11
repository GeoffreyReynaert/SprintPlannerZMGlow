using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            return await _database.Beheerder.SingleOrDefaultAsync(d => d.beheerderID == id);
        }

        public async Task<IList<Beheerder>> FindAsync()
        {
            return await _database.Beheerder.ToListAsync();
        }

        public async Task<bool> FindMail(string mail)
        {
            var leerling = await _database.Beheerder
                .Where(b => b.email == mail)
                .SingleOrDefaultAsync();

            return leerling != null;
        }

        public async Task<Beheerder> CreateAsync(Beheerder beheerder)
        {
           await _database.Beheerder.AddAsync(beheerder);
           await _database.SaveChangesAsync();
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
               await _database.SaveChangesAsync();
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
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
