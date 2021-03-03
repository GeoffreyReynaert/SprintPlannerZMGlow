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
                .Include(k => k.hulpleerling)
              //  .ThenInclude(h=>h.Sprintvakken)
                .DefaultIfEmpty()
                .SingleOrDefaultAsync();

            return leerling;
        }



        public async Task<Leerling> GetToImport(long id)
        {
            var leerling = await _database.Leerling
                .Where(l => l.leerlingID == id)
                .SingleOrDefaultAsync();

            return leerling;
        }


        public async Task<IList<Leerling>> Find()
        {
            var leerlingen = await _database.Leerling
                .Include(l => l.Klas)
                .ThenInclude(k => k.Vakken)
                .Include(k => k.hulpleerling)
                .OrderBy(l => l.familieNaam)
                .ToListAsync();

            return leerlingen;
        }



        //Voor Paging Queryable Leerling beheer
        public  IQueryable<Leerling> FindAsyncPagingQueryable()
        {
            var leerlingenQuery = _database.Leerling
                .Include(l => l.Klas)
                .AsQueryable();

            return leerlingenQuery;
        }


        public async Task<IList<Leerling>> FindByKlasID(int klasid)
        {
            var leerlingenPerKlas = await _database.Leerling
                .Where(l => l.KlasID == klasid)
                .Include(l=>l.hulpleerling)
                .Include(l=>l.Klas)
                .OrderBy(l => l.familieNaam)
                .ToListAsync();

            return leerlingenPerKlas;
        }


        public async Task<Leerling> Create(Leerling leerling)
        {
            var dbLeerling = 
                await _database.Leerling
                    .Where(l => l.leerlingID == leerling.leerlingID)
                    .SingleOrDefaultAsync();

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

