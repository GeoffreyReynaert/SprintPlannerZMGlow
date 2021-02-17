using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class KlasService: IKlasService
    {

        private readonly TihfDbContext _database;

        public KlasService(TihfDbContext database)
        {
            _database = database;
        }
        public Klas Get(int id)
        {
            return _database.Klas.SingleOrDefault(l => l.klasID == id);
        }

        public Klas Get(string name)
        {
            return _database.Klas.SingleOrDefault(l => l.klasnaam == name);
        }

        public Klas GetBySubString(string klasnaam)
        {
            return _database.Klas.SingleOrDefault(l => l.klasnaam.Substring(0,3) == klasnaam.Substring(0,3));
        }

        public IList<Klas> Find()
        {
            return _database.Klas.ToList();
        }

        public Klas Create(Klas klas)
        {
            var dbLeerkracht = _database.Klas.SingleOrDefault(l => l.klasID == klas.klasID);
            if (dbLeerkracht == null)
            {
                _database.Klas.Add(klas);
                _database.SaveChanges();
            }
            return klas;
        }

        public Klas Update(int id, Klas klas)
        {
            {
                var dbKlas = Get(id);
                if (dbKlas == null)
                {
                    return klas;
                }
                _database.Klas.Update(dbKlas);
                _database.SaveChanges();
                return klas;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbKlas = Get(id);
                if (dbKlas == null)
                {
                    return false;
                }
                _database.Klas.Remove(dbKlas);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
