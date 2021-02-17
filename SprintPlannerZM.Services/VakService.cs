using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class VakService: IVakService
    {
        private readonly TihfDbContext _database;

        public VakService(TihfDbContext database)
        {
            _database = database;
        }
        public Vak Get(int id)
        {
            return _database.Vak.SingleOrDefault(v => v.vakID == id);
        }

        public Vak GetBySubString(string vakNaam, int klasID)
        {
            return _database.Vak.Where(v => v.klasID == klasID).SingleOrDefault(v => v.vaknaam.Contains(vakNaam.Substring(0, 3)));
        }

        public IList<Vak> Find()
        {
            return _database.Vak.ToList();
        }

        public IList<Vak> FindBySubstring(string vakNaam, int klasID)
        {
            return _database.Vak.Where(v => v.vaknaam.Contains(vakNaam.Substring(0, 3))).ToList();
        }
        public Vak Create(Vak vak)
        {
            _database.Vak.Add(vak);
            _database.SaveChanges();
            return vak;
        }

        public Vak Update(int id, Vak vak)
        {
            {
                var dbVak = Get(id);
                if (dbVak == null)
                {
                    return vak;
                }
                _database.Vak.Update(dbVak);
                _database.SaveChanges();
                return vak;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbVak = Get(id);
                if (dbVak == null)
                {
                    return false;
                }
                _database.Vak.Remove(dbVak);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
