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
            return _database.Sprintvak.SingleOrDefault(s => s.sprintvakID == id);
        }

        public IList<Sprintvak> Find()
        {
            return _database.Sprintvak.ToList();
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
