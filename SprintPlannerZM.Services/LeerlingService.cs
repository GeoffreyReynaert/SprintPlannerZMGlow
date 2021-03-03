using System.Collections.Generic;
using System.Linq;
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

        public Leerling Get(long id)
        {
            var leerling = _database.Leerling.SingleOrDefault(l => l.leerlingID == id);
            leerling.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerling.KlasID);
                        leerling.hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.leerlingID == leerling.leerlingID);
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
            var leerlingen =_database.Leerling.OrderBy(l => l.familieNaam).ToList();
            foreach (var leerling in leerlingen)
            {
                leerling.hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.hulpleerlingID == leerling.leerlingID);
                leerling.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerling.KlasID);
            }
            return leerlingen;
        }



        public IList<Leerling> FindByKlasID(int klasid)
        {
            var leerlingenPerKlas = _database.Leerling.Where(l => l.KlasID == klasid).OrderBy(l => l.familieNaam).ToList();
            foreach (var leerlingperklas in leerlingenPerKlas)
            {
                leerlingperklas.hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.hulpleerlingID == leerlingperklas.leerlingID);
                leerlingperklas.Klas = _database.Klas.SingleOrDefault(k => k.klasID == leerlingperklas.KlasID);
            }
            return leerlingenPerKlas;
        }

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

        public IQueryable<Leerling> FindAsyncPagingQueryable()
        {

            var leerlings = _database.Leerling
                .Include(l => l.Klas)
                .AsQueryable();

            return leerlings;
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
                _database.Leerling.Remove(dbLeerling);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
