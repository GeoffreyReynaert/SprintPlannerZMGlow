using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class LokaalService: ILokaalService
    {
        private readonly TihfDbContext _database;

        public LokaalService(TihfDbContext database)
        {
            _database = database;
        }

        public Lokaal Get(int id)
        {
            return _database.Lokaal.SingleOrDefault(l => l.lokaalID == id);
        }

        public IList<Lokaal> Find()
        {
            return _database.Lokaal.ToList();
        }

        public Lokaal Create(Lokaal lokaal)
        {
            _database.Lokaal.Add(lokaal);
            _database.SaveChanges();
            return lokaal;
        }

        public Lokaal Update(int id, Lokaal lokaal)
        {
            {
                var dblokaal = Get(id);
                if (dblokaal == null)
                {
                    return lokaal;
                }
                _database.Lokaal.Update(dblokaal);
                _database.SaveChanges();
                return lokaal;
            }
        }

        public bool Delete(int id)
        {
            {
                var dblokaal = Get(id);
                if (dblokaal == null)
                {
                    return false;
                }
                _database.Lokaal.Remove(dblokaal);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
