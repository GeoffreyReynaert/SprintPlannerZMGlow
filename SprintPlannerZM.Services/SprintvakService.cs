using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class SprintvakService : ISprintvakService
    {
        private readonly TihfDbContext _database;

        public SprintvakService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Sprintvak> GetAsync(int id)
        {
            var sprintvak = await _database.Sprintvak.SingleOrDefaultAsync(s => s.sprintvakID == id);
            sprintvak.Vak = await _database.Vak.SingleOrDefaultAsync(v => v.vakID == sprintvak.vakID);
            sprintvak.Hulpleerling = await _database.Hulpleerling.SingleOrDefaultAsync(h => h.leerlingID == sprintvak.hulpleerlingID);
            return sprintvak;
        }

        public async Task<IList<Sprintvak>> FindAsync()
        {
           var sprintvakken =await _database.Sprintvak.ToListAsync();
           foreach (var sprintvak in sprintvakken)
           {
               sprintvak.Vak = _database.Vak.SingleOrDefault(v => v.vakID == sprintvak.vakID);
               sprintvak.Hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.leerlingID == sprintvak.hulpleerlingID);
           }
           return sprintvakken;
        }

        public async Task<Sprintvak> CreateAsync(Sprintvak sprintvak)
        {
            _database.Sprintvak.Add(sprintvak);
           await _database.SaveChangesAsync();
            return sprintvak;
        }

        public async Task<Sprintvak> UpdateAsync(int id, Sprintvak sprintvak)
        {
            {
                var dbSprintvak = await GetAsync(id);
                if (dbSprintvak == null)
                {
                    return sprintvak;
                }
                _database.Sprintvak.Update(dbSprintvak);
               await _database.SaveChangesAsync();
                return sprintvak;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            {
                var dbSprintvak =await GetAsync(id);
                if (dbSprintvak == null)
                {
                    return false;
                }
                _database.Sprintvak.Remove(dbSprintvak);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}