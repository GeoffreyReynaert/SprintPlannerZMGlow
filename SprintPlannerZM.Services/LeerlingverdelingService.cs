using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class LeerlingverdelingService: ILeerlingverdelingService
    {
        private readonly TihfDbContext _database;

        public LeerlingverdelingService(TihfDbContext database)
        {
            _database = database;
        }
        public Leerlingverdeling Get(int id)
        {
            return _database.Leerlingverdeling.SingleOrDefault(l => l.leerlingverdelingID == id);
        }

        public IList<Leerlingverdeling> Find()
        {
            return _database.Leerlingverdeling.ToList();
        }

        public Leerlingverdeling Create(Leerlingverdeling leerlingverdeling)
        {
            _database.Leerlingverdeling.Add(leerlingverdeling);
            _database.SaveChanges();
            return leerlingverdeling;
        }

        public Leerlingverdeling Update(int id, Leerlingverdeling leerlingverdeling)
        {
            {
                var dbLeerlingverdeling = Get(id);
                if (dbLeerlingverdeling == null)
                {
                    return leerlingverdeling;
                }
                _database.Leerlingverdeling.Update(dbLeerlingverdeling);
                _database.SaveChanges();
                return leerlingverdeling;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbLeerlingverdeling = Get(id);
                if (dbLeerlingverdeling == null)
                {
                    return false;
                }
                _database.Leerlingverdeling.Remove(dbLeerlingverdeling);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
