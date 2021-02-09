using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class DagdeelService: IDagdeelService
    {
        private readonly TihfDbContext _database;

        public DagdeelService(TihfDbContext database)
        {
            _database = database;
        }
        public Dagdeel Get(int id)
        {
            return _database.Dagdeel.SingleOrDefault(d => d.dagdeelID == id);
        }

        public IList<Dagdeel> Find()
        {
            return _database.Dagdeel.ToList();
        }

        public Dagdeel Create(Dagdeel dagdeel)
        {
            _database.Dagdeel.Add(dagdeel);
            _database.SaveChanges();
            return dagdeel;
        }

        public Dagdeel Update(int id, Dagdeel dagdeel)
        {
            {
                var dbDagdeel = Get(id);
                if (dbDagdeel == null)
                {
                    return dagdeel;
                }
                _database.Dagdeel.Update(dbDagdeel);
                _database.SaveChanges();
                return dagdeel;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbDagdeel = Get(id);
                if (dbDagdeel == null)
                {
                    return false;
                }
                _database.Dagdeel.Remove(dbDagdeel);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
