using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class ExamenroosterService: IExamenroosterService

    {
    private readonly TihfDbContext _database;

    public ExamenroosterService(TihfDbContext database)
    {
        _database = database;
    }

    public Examenrooster Get(int id)
    {
        return _database.Examenrooster.SingleOrDefault(e => e.examenID == id);
    }

    public IList<Examenrooster> Find()
    {
        return _database.Examenrooster.ToList();
    }

    public Examenrooster Create(Examenrooster examenrooster)
    {
        _database.Examenrooster.Add(examenrooster);
        _database.SaveChanges();
        return examenrooster;
    }

    public Examenrooster Update(int id, Examenrooster examenrooster)
    {
        {
            var dbExamenrooster = Get(id);
            if (dbExamenrooster == null)
            {
                return examenrooster;
            }

            _database.Examenrooster.Update(dbExamenrooster);
            _database.SaveChanges();
            return examenrooster;
        }
    }

    public bool Delete(int id)
    {
        {
            var dbExamenrooster = Get(id);
            if (dbExamenrooster == null)
            {
                return false;
            }

            _database.Examenrooster.Remove(dbExamenrooster);
            _database.SaveChanges();
            return true;
        }
    }
    }
}
