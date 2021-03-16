using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services
{
    public class HulpleerlingService: IHulpleerlingService
    {
        private readonly TihfDbContext _database;

        public HulpleerlingService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Hulpleerling> Get(long? id)
        {
            var hulpLln= await _database.Hulpleerling
                .Where(v => v.hulpleerlingID == id)
                .Include(v=>v.Klas)
                .ThenInclude(v=>v.Vakken)
                .ThenInclude(l=>l.Leerkracht)
                .Include(v=>v.Sprintvakkeuzes)
                .Include(h=>h.Leerling)
                .SingleOrDefaultAsync();
        
            return hulpLln;
        }

        public async Task<Hulpleerling> GetHulpAsync(long? id)
        {
            var hulpLln = await _database.Hulpleerling
                .Where(v => v.hulpleerlingID == id)
                .SingleOrDefaultAsync();

            return hulpLln;
        }

        public async Task<Hulpleerling> GetbyLeerlingId(long leerlingID)
        {
            var hulpLln =await _database.Hulpleerling.FirstOrDefaultAsync(l => l.leerlingID == leerlingID);

            return hulpLln;
        }

        public async Task<IList<Hulpleerling>> Find()
        {
           var  hulpleerlingen = await _database.Hulpleerling
               .Include(l=>l.Leerling)
               .Include(h => h.Klas)
               .ThenInclude(k=>k.Vakken)
               .Include(h => h.Sprintvakkeuzes)
               .ThenInclude(s=>s.Vak)/*.OrderBy(h=>h.Klas)*/
               .ToListAsync();

           return hulpleerlingen;
        }

        public async Task<Hulpleerling> Create(Hulpleerling hulpleerling)
        {
            await _database.Hulpleerling.AddAsync(hulpleerling); 
            await _database.SaveChangesAsync();
            var dbLeerling = await _database.Hulpleerling.SingleOrDefaultAsync(h=> h.hulpleerlingID == hulpleerling.hulpleerlingID);
            return dbLeerling;
        }

        public async Task<Hulpleerling> Update(long? id, Hulpleerling hulpleerling)
        {
            {
                var dbHulpleerling = await Get(id);
                if (dbHulpleerling == null)
                {
                    return hulpleerling;
                }
                _database.Hulpleerling.Update(dbHulpleerling);
                await _database.SaveChangesAsync();
                return hulpleerling;
            }
        }

        public async Task<bool> DeleteByAsync(long? id)
        {
            {
                //var dbHulpleerling = await GetHulpAsync(id);
                var dbHulpleerling = await _database.Hulpleerling.Where(h => h.hulpleerlingID == id).SingleOrDefaultAsync();
                if (dbHulpleerling == null)
                {
                    return false;
                }

                _database.Hulpleerling.Remove(dbHulpleerling);
                await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}