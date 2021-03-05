using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
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
        public async Task<Sprintvak> GetAsync(long id)
        {
            var sprintvak = await _database.Sprintvak
                .Where(s => s.sprintvakID == id)
                .Include(s=>s.Vak)
                .Include(s=>s.Hulpleerling)
                .SingleOrDefaultAsync(); 

            return sprintvak;
        }

        public async Task<Sprintvak> GetByVakAndHulpleerlingID(int vakId, long hulpleerlingId)
        {
            var sprintvak = await _database.Sprintvak
                .Where(s => s.hulpleerlingID == hulpleerlingId)
                .Include(s => s.Vak)
                .Include(s => s.Hulpleerling)
                .SingleOrDefaultAsync(s=>s.vakID == vakId);

            return sprintvak;
        }

        public async Task<IList<Sprintvak>> FindAsync()
        {
           var sprintvakken =await _database.Sprintvak
               .Include(s=>s.Vak)
               .Include(s=>s.Hulpleerling)
               .ToListAsync();

           return sprintvakken;
        }

        public async Task<Sprintvak> CreateAsync(Sprintvak sprintvak)
        { 
            await _database.Sprintvak.AddAsync(sprintvak); 
            await _database.SaveChangesAsync();
            return sprintvak;
        }

        public async Task<Sprintvak> UpdateAsync(int id, Sprintvak sprintvak)
        {
            {
                var sprintvakToUpd = await _database.Sprintvak.SingleOrDefaultAsync(s => s.sprintvakID == id);
                sprintvakToUpd.sprint = sprintvak.sprint;
                sprintvakToUpd.typer = sprintvak.typer;
                sprintvakToUpd.mklas = sprintvak.mklas;
                _database.Sprintvak.Update(sprintvakToUpd);
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