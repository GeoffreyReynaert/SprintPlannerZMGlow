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
            return _database.Klas.SingleOrDefault(k => k.klasID == id);
        }

        public Klas Get(string name)
        {
            return _database.Klas.SingleOrDefault(k => k.klasnaam == name);
        }

        public Klas GetBySubString(string klasnaam)
        {
            return _database.Klas.SingleOrDefault(l => l.klasnaam.Substring(0,3) == klasnaam.Substring(0,3));
        }

        public IList<Klas> Find()
        {
            return _database.Klas.OrderBy(k => k.klasnaam).ToList();
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
                var klasToUpd = _database.Klas.SingleOrDefault(l => l.klasID == id);
                klasToUpd.klasnaam = klas.klasnaam;
                klasToUpd.titularisID = klas.titularisID;
                _database.Klas.Update(klasToUpd);
                _database.SaveChanges();
                return klasToUpd;
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
