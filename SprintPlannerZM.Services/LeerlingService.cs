using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class LeerlingService : ILeerlingService
    {
        private readonly TihfDbContext _database;

        public LeerlingService(TihfDbContext database)
        {
            _database = database;
        }

        public async Task<Leerling> Get(long id)
        {
            var leerling = await _database.Leerling
                .Where(l => l.leerlingID == id)
                .Include(l => l.Klas)
                .ThenInclude(k => k.Vakken)
                .Include(k => k.hulpleerling).DefaultIfEmpty()
                .SingleOrDefaultAsync();

            return leerling;
        }



        public async Task<Leerling> GetFullLeerling(long id)
        {
            var leerling = await _database.Leerling.SingleOrDefaultAsync(l => l.leerlingID == id);
            leerling.Klas = await _database.Klas.SingleOrDefaultAsync(k => k.klasID == leerling.KlasID);
            leerling.hulpleerling = await _database.Hulpleerling.SingleOrDefaultAsync(h => h.leerlingID == leerling.leerlingID);
            if (leerling.hulpleerling != null)
            {
                leerling.hulpleerling.Sprintvakken = await _database.Sprintvak.Where(s => s.hulpleerlingID == leerling.hulpleerling.hulpleerlingID).ToListAsync();
                foreach (var sprint in leerling.hulpleerling.Sprintvakken)
                {
                    sprint.Vak = await _database.Vak.SingleOrDefaultAsync(v => v.vakID == sprint.vakID);
                }
            }

            return leerling;
        }


        public async Task<Leerling> GetToImport(long id)
        {
            var leerling = await _database.Leerling.SingleOrDefaultAsync(l => l.leerlingID == id);

            return leerling;
        }


        public async Task<IList<Leerling>> Find()
        {
            var leerlingen = await _database.Leerling.OrderBy(l => l.familieNaam).ToListAsync();
            foreach (var leerling in leerlingen)
            {
                leerling.hulpleerling = await _database.Hulpleerling.SingleOrDefaultAsync(h => h.leerlingID == leerling.leerlingID);
                leerling.Klas = await _database.Klas.SingleOrDefaultAsync(k => k.klasID == leerling.KlasID);
            }
            return leerlingen;
        }



        //Voor Paging Queryable Leerling beheer
        public  IQueryable<Leerling> FindAsyncPagingQueryable()
        {
            var LeerlingenQuery = _database.Leerling
                .Include(l => l.Klas)
                .AsQueryable();

            return LeerlingenQuery;
        }


        public async Task<IList<Leerling>> FindByKlasID(int klasid)
        {
            var leerlingenPerKlas =
               await _database.Leerling.Where(l => l.KlasID == klasid).OrderBy(l => l.familieNaam).ToListAsync();
            foreach (var leerlingperklas in leerlingenPerKlas)
            {
                leerlingperklas.hulpleerling =
                   await _database.Hulpleerling.SingleOrDefaultAsync(h => h.hulpleerlingID == leerlingperklas.leerlingID);
                leerlingperklas.Klas = await _database.Klas.SingleOrDefaultAsync(k => k.klasID == leerlingperklas.KlasID);
            }

            return leerlingenPerKlas;
        }


        public async Task<Leerling> Create(Leerling leerling)
        {
            var dbLeerling = await _database.Leerling.SingleOrDefaultAsync(l => l.leerlingID == leerling.leerlingID);
            if (dbLeerling == null)
            {
                await _database.Leerling.AddAsync(leerling);
                await _database.SaveChangesAsync();
            }
            return leerling;
        }

        public async Task<Leerling> Update(long id, Leerling leerling)
        {
            {
                var leerlingToUpd = await _database.Leerling.SingleOrDefaultAsync(l => l.leerlingID == id);
                leerlingToUpd.sprinter = leerling.sprinter;
                leerlingToUpd.mklas = leerling.mklas;
                leerlingToUpd.typer = leerling.typer;
                _database.Leerling.Update(leerlingToUpd);
                await _database.SaveChangesAsync();
                return leerling;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbLeerling = await Get(id);
                if (dbLeerling == null)
                {
                    return false;
                }
                _database.Leerling.Remove(dbLeerling);
                await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}

//{
//leerling = _database.Leerling
//    .Where(l => l.leerlingID == id)
//    .Include(l => l.Klas)
//    .Include(l => l.hulpleerling)
//    .ThenInclude(h => h.Sprintvakken)
//    .ThenInclude(s => s.Vak)
//    .SingleOrDefault();
//}

