using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class HulpleerlingService: IHulpleerlingService
    {
        private readonly TihfDbContext _database;

        public HulpleerlingService(TihfDbContext database)
        {
            _database = database;
        }
        public Hulpleerling Get(int id)
        {
            return _database.Hulpleerling.SingleOrDefault(v => v.hulpleerlingID == id);
        }

        public IList<Hulpleerling> Find()
        {
            return _database.Hulpleerling.ToList();
        }

        public Hulpleerling Create(Hulpleerling hulpleerling)
        {
            _database.Hulpleerling.Add(hulpleerling);
            _database.SaveChanges();
            return hulpleerling;
        }

        public Hulpleerling Update(int id, Hulpleerling hulpleerling)
        {
            {
                var dbHulpleerling = Get(id);
                if (dbHulpleerling == null)
                {
                    return hulpleerling;
                }
                _database.Hulpleerling.Update(dbHulpleerling);
                _database.SaveChanges();
                return hulpleerling;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbHulpleerling = Get(id);
                if (dbHulpleerling == null)
                {
                    return false;
                }
                _database.Hulpleerling.Remove(dbHulpleerling);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
