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
          var vak = _database.Vak.SingleOrDefault(v => v.vakID == id);
          vak.Klas= _database.Klas.SingleOrDefault(k=>k.klasID == vak.klasID);
          return vak;
        }


        //Gebruik bij import examens nog niet zeker door lijst van antwoorden ipv van enkel een vak (meerdere door versch leerkrachten)
        public Vak GetBySubString(string vakNaam, int klasID)
        {
            var vak = _database.Vak.Where(v => v.klasID == klasID).First(v => v.vaknaam.Contains(vakNaam.Substring(0, 3)));
            vak.Klas = _database.Klas.SingleOrDefault(k => k.klasID == vak.klasID);
            return vak;
        }

        public IList<Vak> Find()
        {
            var vakken = _database.Vak.ToList();
            foreach (var vak in vakken)
            {
                vak.Klas = _database.Klas.SingleOrDefault(k => k.klasID == vak.klasID);
            }
            return vakken;
        }

        public IList<Vak> FindBySubstring(string vakNaam, int klasID)
        {
          var vakken =  _database.Vak.Where(v => v.vaknaam.Contains(vakNaam.Substring(0, 3))).ToList();
          foreach (var vak in vakken)
          {
              vak.Klas = _database.Klas.SingleOrDefault(k => k.klasID == vak.klasID);
          }

          return vakken;
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
                var vakToUpd = _database.Vak.SingleOrDefault(i => i.vakID == id);
                vakToUpd.vaknaam = vak.vaknaam;
                vakToUpd.vakID = vak.vakID;
                vakToUpd.leerkrachtID = vak.leerkrachtID;
                _database.Vak.Update(vakToUpd);
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
