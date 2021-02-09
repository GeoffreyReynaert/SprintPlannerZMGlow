using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class BeheerderService: IBeheerderService
    {
        private readonly TihfDbContext _database;

        public BeheerderService(TihfDbContext database)
        {
            _database = database;
        }
        public Beheerder Get(int id)
        {
            return _database.Beheerder.SingleOrDefault(d => d.beheerderID == id);
        }

        public IList<Beheerder> Find()
        {
            return _database.Beheerder.ToList();
        }

        public Beheerder Create(Beheerder beheerder)
        {
            _database.Beheerder.Add(beheerder);
            _database.SaveChanges();
            return beheerder;
        }

        public Beheerder Update(int id, Beheerder beheerder)
        {
            {
                var dBeheerder = Get(id);
                if (dBeheerder == null)
                {
                    return beheerder;
                }
                _database.Beheerder.Update(dBeheerder);
                _database.SaveChanges();
                return beheerder;
            }
        }

        public bool Delete(int id)
        {
            {
                var dBeheerder = Get(id);
                if (dBeheerder == null)
                {
                    return false;
                }
                _database.Beheerder.Remove(dBeheerder);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
