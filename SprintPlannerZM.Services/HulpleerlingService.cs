using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            var hulpLln= await _database.Hulpleerling.SingleOrDefaultAsync(v => v.hulpleerlingID == id);
            hulpLln.Klas =await _database.Klas.SingleOrDefaultAsync(k => k.klasID == hulpLln.klasID);
            hulpLln.Sprintvakken =await _database.Sprintvak.Where(s => s.hulpleerlingID == hulpLln.hulpleerlingID).ToListAsync();
            return hulpLln;
        }

        public async Task<Hulpleerling> GetbyLeerlingId(long leerlingID)
        {
            var hulpLln =await _database.Hulpleerling.FirstOrDefaultAsync(l => l.leerlingID == leerlingID);
            //hulpLln.Klas = _database.Klas.SingleOrDefault(k => k.klasID == hulpLln.klasID);
            //hulpLln.Sprintvakken = _database.Sprintvak.Where(s => s.hulpleerlingID == hulpLln.hulpleerlingID).ToList();
            return hulpLln;
        }

        public async Task<IList<Hulpleerling>> Find()
        {
           var  hulpleerlingen = await _database.Hulpleerling.ToListAsync();

            foreach (var hulpLln in hulpleerlingen)
            {
                hulpLln.Leerling = await _database.Leerling.SingleOrDefaultAsync(l => l.leerlingID == hulpLln.leerlingID);
                hulpLln.Klas = await _database.Klas.SingleOrDefaultAsync(k => k.klasID == hulpLln.klasID);
                hulpLln.Klas.Vakken = await _database.Vak.Where(v => v.klasID == hulpLln.klasID).ToListAsync();
                hulpLln.Sprintvakken = await _database.Sprintvak.Where(s => s.hulpleerlingID == hulpLln.hulpleerlingID).ToListAsync();
            }

            return hulpleerlingen;
        }

        public async Task<Hulpleerling> Create(Hulpleerling hulpleerling)
        {
            await _database.Hulpleerling.AddAsync(hulpleerling); 
            await _database.SaveChangesAsync();
            var dbLeerling = await _database.Hulpleerling.SingleOrDefaultAsync(h=> h.hulpleerlingID == hulpleerling.hulpleerlingID);
            return dbLeerling;
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
