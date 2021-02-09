using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class ExamentijdspanneService: IExamentijdspanneService
    {
        private readonly TihfDbContext _database;

        public ExamentijdspanneService(TihfDbContext database)
        {
            _database = database;
        }
        public Examentijdspanne Get(int id)
        {
            return _database.Examentijdspanne.SingleOrDefault(e => e.tijdspanneID == id);
        }

        public IList<Examentijdspanne> Find()
        {
            return _database.Examentijdspanne.ToList();
        }

        public Examentijdspanne Create(Examentijdspanne examentijdspanne)
        {
            _database.Examentijdspanne.Add(examentijdspanne);
            _database.SaveChanges();
            return examentijdspanne;
        }

        public Examentijdspanne Update(int id, Examentijdspanne examentijdspanne)
        {
            {
                var dbExamentijdspanne = Get(id);
                if (dbExamentijdspanne == null)
                {
                    return examentijdspanne;
                }
                _database.Examentijdspanne.Update(dbExamentijdspanne);
                _database.SaveChanges();
                return examentijdspanne;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbExamentijdspanne = Get(id);
                if (dbExamentijdspanne == null)
                {
                    return false;
                }
                _database.Examentijdspanne.Remove(dbExamentijdspanne);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
