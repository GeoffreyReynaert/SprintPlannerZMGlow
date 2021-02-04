using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;

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
            return _database.Vak.SingleOrDefault(v => v.VakID == id);
        }

        public IList<Vak> Find()
        {
            return _database.Vak.ToList();
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
