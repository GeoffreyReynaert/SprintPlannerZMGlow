using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using SoapSSMvc.Model;

namespace SprintPlannerZM.Services
{
    public class LeerkrachtService: ILeerkrachtService
    {
        private readonly TihfDbContext _database;

        public LeerkrachtService(TihfDbContext database)
        {
            _database = database;
        }

        public Leerkracht Get(long id)
        {
            return _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == id);
        }

        public Leerkracht GetByKlasId(int id)
        {
            return _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID== id);
        }

        public IList<Leerkracht> Find()
        {
            return _database.Leerkracht.ToList();
        }

        public Leerkracht Create(Leerkracht leerkracht)
        {
            var dbLeerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == leerkracht.leerkrachtID);
            if (dbLeerkracht==null)
            {
                _database.Leerkracht.Add(leerkracht);
                _database.SaveChanges();
            }
            return leerkracht;
        }


        public Leerkracht Update(int id, Leerkracht leerkracht)
        {
            {
                var dbLeerkracht = Get(id);
                if (dbLeerkracht == null)
                {
                    return leerkracht;
                }
                _database.Leerkracht.Update(dbLeerkracht);
                _database.SaveChanges();
                return leerkracht;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbLeerkracht = Get(id);
                if (dbLeerkracht == null)
                {
                    return false;
                }
                _database.Leerkracht.Remove(dbLeerkracht);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
