using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class SprintlokaalService: ISprintlokaalService
    {
        private readonly TihfDbContext _database;

        public SprintlokaalService(TihfDbContext database)
        {
            _database = database;
        }
        public Sprintlokaal Get(int id)
        {
            return _database.Sprintlokaal.SingleOrDefault(s => s.sprintlokaalID == id);
        }

        public IList<Sprintlokaal> Find()
        {
            return _database.Sprintlokaal.ToList();
        }

        public Sprintlokaal Create(Sprintlokaal sprintlokaal)
        {
            _database.Sprintlokaal.Add(sprintlokaal);
            _database.SaveChanges();
            return sprintlokaal;
        }

        public Sprintlokaal Update(int id, Sprintlokaal sprintlokaal)
        {
            {
                var dbSprintlokaal = Get(id);
                if (dbSprintlokaal == null)
                {
                    return sprintlokaal;
                }
                _database.Sprintlokaal.Update(dbSprintlokaal);
                _database.SaveChanges();
                return sprintlokaal;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbSprintlokaal = Get(id);
                if (dbSprintlokaal == null)
                {
                    return false;
                }
                _database.Sprintlokaal.Remove(dbSprintlokaal);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
