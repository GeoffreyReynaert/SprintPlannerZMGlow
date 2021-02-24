using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class SprintvakService: ISprintvakService
    {
        private readonly TihfDbContext _database;

        public SprintvakService(TihfDbContext database)
        {
            _database = database;
        }
        public Sprintvak Get(int id)
        {
            var sprintvak = _database.Sprintvak.SingleOrDefault(s => s.sprintvakID == id);
            sprintvak.Vak = _database.Vak.SingleOrDefault(v => v.vakID == sprintvak.vakID);
            sprintvak.Hulpleerling =
                _database.Hulpleerling.SingleOrDefault(h => h.leerlingID == sprintvak.hulpleerlingID);
            return sprintvak;
        }

        public IList<Sprintvak> Find()
        {
           var sprintvakken =_database.Sprintvak.ToList();
           foreach (var sprintvak in sprintvakken)
           {
               sprintvak.Vak = _database.Vak.SingleOrDefault(v => v.vakID == sprintvak.vakID);
               sprintvak.Hulpleerling = _database.Hulpleerling.SingleOrDefault(h => h.leerlingID == sprintvak.hulpleerlingID);
            }
            return sprintvakken;
        }

        public Sprintvak Create(Sprintvak sprintvak)
        {
            _database.Sprintvak.Add(sprintvak);
            _database.SaveChanges();
            return sprintvak;
        }

        public Sprintvak Update(int id, Sprintvak sprintvak)
        {
            {
                var dbSprintvak = Get(id);
                if (dbSprintvak == null)
                {
                    return sprintvak;
                }
                _database.Sprintvak.Update(dbSprintvak);
                _database.SaveChanges();
                return sprintvak;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbSprintvak = Get(id);
                if (dbSprintvak == null)
                {
                    return false;
                }
                _database.Sprintvak.Remove(dbSprintvak);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
