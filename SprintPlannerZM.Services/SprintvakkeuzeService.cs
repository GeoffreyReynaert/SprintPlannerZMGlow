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
            var sprintvakkeuze = await _database.Sprintvakkeuze
                .Where(s => s.sprintvakkeuzeID == id)
                .Include(s=>s.Vak)
                .Include(s=>s.Hulpleerling)
                .SingleOrDefaultAsync();
            return sprintvakkeuze;
        }

        public async Task<IList<Sprintvakkeuze>> GetHulpVakAsync(long? hulpleerlingID)
        {
            var sprintvakkeuzes = await _database.Sprintvakkeuze
                .Where(h=> h.hulpleerlingID == hulpleerlingID)
                .Include(s => s.Vak)
                .Include(s => s.Hulpleerling)
                .ToListAsync();
            return sprintvakkeuzes;
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
           var sprintvakkeuzes = await _database.Sprintvakkeuze
               .Include(s=>s.Vak)
               .Include(s=>s.Hulpleerling)
               .ToListAsync();
           return sprintvakkeuzes;
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
            var dbSprintvak = await GetAsync(id);
                if (dbSprintvak == null)
                {
                    return false;
                }
                _database.Sprintvakkeuze.Remove(dbSprintvak);
               await _database.SaveChangesAsync();
                return true;
        }

        public async Task<bool> DeleteByHulpAsync(long? hulpleerlingID)
        {
            var dbSprintvakken = await GetHulpVakAsync(hulpleerlingID);
            foreach (var dbvakkeuze in dbSprintvakken)
            {
                if (dbvakkeuze == null)
                {
                    return false;
                }
                _database.Sprintvakkeuze.Remove(dbvakkeuze);
                await _database.SaveChangesAsync();
            }
            return true;
        }
    }
}