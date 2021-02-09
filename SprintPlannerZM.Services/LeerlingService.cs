using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class LeerlingService: ILeerlingService
    {
        private readonly TihfDbContext _database;

        public LeerlingService(TihfDbContext database)
        {
            _database = database;
        }

        public Leerling Get(int id)
        {
            return _database.Leerling.SingleOrDefault(l => l.leerlingID == id);
        }

        public IList<Leerling> Find()
        {
            return _database.Leerling.ToList();
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

        public Leerling Update(int id, Leerling leerling)
        {
            {
                var dbLeerling = Get(id);
                if (dbLeerling == null)
                {
                    return leerling;
                }
                _database.Leerling.Update(dbLeerling);
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
