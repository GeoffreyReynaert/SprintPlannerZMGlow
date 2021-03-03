using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class HulpleerlingService: IHulpleerlingService
    {
        private readonly TihfDbContext _database;

        public HulpleerlingService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Hulpleerling> Get(int id)
        {
            var hulpLln= await _database.Hulpleerling
                .Where(v => v.hulpleerlingID == id)
                .Include(v=>v.Klas)
                .Include(v=>v.Sprintvakken)
                .Include(h=>h.Leerling)
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
               .Include(h=>h.Leerling)
               .Include(h => h.Klas)
               .ThenInclude(k=>k.Vakken)
               .Include(h => h.Sprintvakken)
               .ThenInclude(s=>s.Vak)
               .ToListAsync();

           return hulpleerlingen;
        }

        public async Task<Hulpleerling> Create(Hulpleerling hulpleerling)
        {
            await _database.Hulpleerling.AddAsync(hulpleerling);
            await _database.SaveChangesAsync();
            return hulpleerling;
        }

        public async Task<Hulpleerling> Update(int id, Hulpleerling hulpleerling)
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

        public async Task<bool> Delete(int id)
        {
            {
                var dbHulpleerling = await Get(id);
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
