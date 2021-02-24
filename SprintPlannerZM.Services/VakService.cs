using System;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class VakService : IVakService
    {
        private readonly TihfDbContext _database;

        public VakService(TihfDbContext database)
        {
            _database = database;
        }


        public Vak Get(int id)
        {
            var vak = _database.Vak.SingleOrDefault(v => v.vakID == id);
            vak.klas = _database.Klas.SingleOrDefault(k => k.klasID == vak.klasID);
            vak.Examenroosters = _database.Examenrooster.Where(e => e.vakID == vak.vakID).ToList();
            vak.Sprintvakken = _database.Sprintvak.Where(s => s.vakID == vak.vakID).ToList();
            vak.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == vak.leerkrachtID);
            return vak;
        }


      //enkel voor importeren examens gebruikt
        public Vak GetBySubString(string vakNaam, int klasID)
        {
            var klas = _database.Klas.SingleOrDefault(k => k.klasID == klasID);

            var vakkenPerKlas = _database.Vak.Where(v => v.klasID == klasID).ToList();
            foreach (var vak in vakkenPerKlas )
            {
                if (vak.vaknaam.Substring(0,3).ToLower().Equals(vakNaam.Substring(0, 3).ToLower()))
                {
                    vak.klas = klas;
                    return vak;
                }
            }

            return null;
        }

        public IList<Vak> Find()
        {
            var vakken = _database.Vak.ToList();
            foreach (var vak in vakken)
            {
                vak.klas = _database.Klas.SingleOrDefault(k => k.klasID == vak.klasID);
                vak.Examenroosters = _database.Examenrooster.Where(e => e.vakID == vak.vakID).ToList();
                vak.Sprintvakken = _database.Sprintvak.Where(s => s.vakID == vak.vakID).ToList();
                vak.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == vak.leerkrachtID);
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
