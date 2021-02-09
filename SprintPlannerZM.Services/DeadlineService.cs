using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class DeadlineService: IDeadlineService

    {
    private readonly TihfDbContext _database;

    public DeadlineService(TihfDbContext database)
    {
        _database = database;
    }

    public Deadline Get(int id)
    {
        return _database.Deadline.SingleOrDefault(d => d.deadlineID == id);
    }

    public IList<Deadline> Find()
    {
        return _database.Deadline.ToList();
    }

    public Deadline Create(Deadline deadline)
    {
        _database.Deadline.Add(deadline);
        _database.SaveChanges();
        return deadline;
    }

    public Deadline Update(int id, Deadline deadline)
    {
        {
            var dbDeadline = Get(id);
            if (dbDeadline == null)
            {
                return deadline;
            }

            _database.Deadline.Update(dbDeadline);
            _database.SaveChanges();
            return deadline;
        }
    }

    public bool Delete(int id)
    {
        {
            var dbDeadline = Get(id);
            if (dbDeadline == null)
            {
                return false;
            }

            _database.Deadline.Remove(dbDeadline);
            _database.SaveChanges();
            return true;
        }
    }
    }
}
