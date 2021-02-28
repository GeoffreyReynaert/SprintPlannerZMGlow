using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Query;
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

        //Geoffrey
        public Leerling Get(long id)
        {
            var leerling = _database.Leerling
                .Where(l => l.leerlingID == id)
                .Include(l => l.Klas)
                .Include(l => l.hulpleerling)
                .ThenInclude(h => h.Sprintvakken)
                .SingleOrDefault();

            return leerling;
        }

        public Leerling GetFullLeerling(long id)
        {
            var leerling = _database.Leerling.SingleOrDefault(l => l.leerlingID == id);
            leerling.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerling.KlasID);
            leerling.hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.leerlingID == leerling.leerlingID);
            if (leerling.hulpleerling != null)
            {
                leerling.hulpleerling.Sprintvakken = _database.Sprintvak.Where(s => s.hulpleerlingID == leerling.hulpleerling.hulpleerlingID).ToList();
                foreach (var sprint in leerling.hulpleerling.Sprintvakken)
                {
                    sprint.Vak = _database.Vak.SingleOrDefault(v => v.vakID == sprint.vakID);
                }
            }
            //niet mogelijk bij import niewe get odig met hulpleerling inbegrepen en null check

            return leerling;
        }


        public Leerling GetToImport(long id)
        {
            var leerling = _database.Leerling.SingleOrDefault(l => l.leerlingID == id);

            return leerling;
        }


        public IList<Leerling> Find()
        {
            var leerlingen = _database.Leerling.OrderBy(l => l.familieNaam).ToList();
            foreach (var leerling in leerlingen)
            {
                leerling.hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.hulpleerlingID == leerling.leerlingID);
                leerling.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerling.KlasID);
            }
            return leerlingen;
        }



        //Geoffrey
        //Voor Paging Queryable Leerling beheer
        public async Task<IQueryable<Leerling>> FindAsyncPagingQueryable()
        {

            var leerlings =  _database.Leerling
                .Join(_database.Klas,l=> l.KlasID, klas => klas.klasID,(leerling,klas) => new Leerling
                {
                    Klas = klas,
                    familieNaam = leerling.familieNaam,
                    voorNaam = leerling.voorNaam,
                    leerlingID = leerling.leerlingID
                })
                .AsQueryable();

            return leerlings;
        }



        public IList<Leerling> FindByKlasID(int klasid)
        {
            var leerlingenPerKlas =
                _database.Leerling.Where(l => l.KlasID == klasid).OrderBy(l => l.familieNaam).ToList();
            foreach (var leerlingperklas in leerlingenPerKlas)
            {
                leerlingperklas.hulpleerling =
                    _database.Hulpleerling.SingleOrDefault(h => h.hulpleerlingID == leerlingperklas.leerlingID);
                leerlingperklas.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerlingperklas.KlasID);
            }

            return leerlingenPerKlas;
        }

        //public IList<Leerling> FindByKlasID(int klasid)
            //{
            //    var leerlingenPerKlas = _database.Leerling.Where(l => l.KlasID == klasid)
            //        .Include(l => _database.Hulpleerling.SingleOrDefault(h => h.hulpleerlingID == l.leerlingID))
            //        .Include(l => _database.Klas.SingleOrDefault(k => k.klasID == l.KlasID))
            //        .OrderBy(l => l.familieNaam).ToList();

            //    return leerlingenPerKlas;
            //}

            public Leerling Create(Leerling leerling)
        {
            var dbLeerling = _database.Leerling.SingleOrDefault(l => l.leerlingID == leerling.leerlingID);
            if (dbLeerling == null)
            {
                _database.Leerling.Add(leerling);
                _database.SaveChanges();
            }
            return leerling;
        }

        public Leerling Update(long id, Leerling leerling)
        {
            {
                var leerlingToUpd = _database.Leerling.SingleOrDefault(l => l.leerlingID == id);
                leerlingToUpd.sprinter = leerling.sprinter;
                leerlingToUpd.mklas = leerling.mklas;
                leerlingToUpd.typer = leerling.typer;
                _database.Leerling.Update(leerlingToUpd);
                _database.SaveChanges();
                return leerling;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbLeerling = Get(id);
                if (dbLeerling == null)
                {
                    return false;
                }
                //_database.Leerling.Remove(dbLeerling);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
