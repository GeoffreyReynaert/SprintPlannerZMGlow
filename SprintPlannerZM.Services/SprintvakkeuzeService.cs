using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class SprintvakkeuzeService : ISprintvakkeuzeService
    {
        private readonly TihfDbContext _database;

        public SprintvakkeuzeService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Sprintvakkeuze> GetAsync(long id)
        {
            var sprintvak = await _database.Sprintvakkeuze
                .Where(s => s.sprintvakkeuzeID == id)
                .Include(s=>s.Vak)
                .Include(s=>s.Hulpleerling)
                .SingleOrDefaultAsync(); 

            return sprintvak;
        }

        public async Task<Sprintvakkeuze> GetByVakAndHulpleerlingID(int vakId, long hulpleerlingId)
        {
            var sprintvak = await _database.Sprintvakkeuze
                .Where(s => s.hulpleerlingID == hulpleerlingId)
                .Include(s => s.Vak)
                .Include(s => s.Hulpleerling)
                .SingleOrDefaultAsync(s=>s.vakID == vakId);

            return sprintvak;
        }

        public async Task<IList<Sprintvakkeuze>> FindAsync()
        {
           var sprintvakken =await _database.Sprintvakkeuze
               .Include(s=>s.Vak)
               .Include(s=>s.Hulpleerling)
               .ToListAsync();

           return sprintvakken;
        }

        public async Task<Sprintvakkeuze> CreateAsync(Sprintvakkeuze sprintvakkeuze)
        { 
            await _database.Sprintvakkeuze.AddAsync(sprintvakkeuze); 
            await _database.SaveChangesAsync();
            return sprintvakkeuze;
        }

        public async Task<Sprintvakkeuze> UpdateAsync(int id, Sprintvakkeuze sprintvakkeuze)
        {
            {
                var sprintvakToUpd = await _database.Sprintvakkeuze.SingleOrDefaultAsync(s => s.sprintvakkeuzeID == id);
                sprintvakToUpd.sprint = sprintvakkeuze.sprint;
                sprintvakToUpd.typer = sprintvakkeuze.typer;
                sprintvakToUpd.mklas = sprintvakkeuze.mklas;
                _database.Sprintvakkeuze.Update(sprintvakToUpd);
                await _database.SaveChangesAsync();
                return sprintvakkeuze;
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
                _database.Sprintvakkeuze.Remove(dbSprintvak);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}