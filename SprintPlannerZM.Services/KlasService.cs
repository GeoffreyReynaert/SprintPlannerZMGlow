using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class KlasService : IKlasService
    {

        private readonly TihfDbContext _database;

        public KlasService(TihfDbContext database)
        {
            _database = database;
        }
        public Klas Get(int id)
        {
            var klas = _database.Klas.SingleOrDefault(k => k.klasID == id);
            klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
            klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
            klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();

            return klas;
        }

        public Klas GetSprintvakWithKlas(int id)
        {
            var klas = _database.Klas.SingleOrDefault(k => k.klasID == id);
            klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
            klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
            klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();
            foreach (var vak in klas.Vakken)
            {
                vak.Sprintvakken = _database.Sprintvak.Where(s => s.vakID == vak.vakID).ToList();
            }

            return klas;
        }

        public Klas GetByKlasName(string name)
        {
            var klas = _database.Klas.SingleOrDefault(k => k.klasnaam.Equals(name));
            klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
            return klas;
        }


        //gebruik voor myro import enkel
        public Klas GetBySubString(string klasnaam)
        {
            var klas = new Klas();
            if (klasnaam.Substring(0, 3).Equals("1OK"))
            {
                klas = _database.Klas.SingleOrDefault(l => l.klasnaam.Equals(klasnaam));
            }

            else if (klasnaam.Substring(0, 3).Equals("2AM") ||
                     klasnaam.Substring(0, 3).Equals("3BH") ||
                     klasnaam.Substring(0, 3).Equals("3KB") ||
                     klasnaam.Substring(0, 3).Equals("4BH") ||
                     klasnaam.Substring(0, 3).Equals("4BP") ||
                     klasnaam.Substring(0, 3).Equals("4KB"))
            {
                klas = _database.Klas.SingleOrDefault(l => l.klasnaam.Substring(0, 5).Equals(klasnaam.Substring(0, 5)));
            }

            else if (klasnaam.Substring(0, 3).Equals("3TS") ||
                     klasnaam.Substring(0, 3).Equals("4TS") ||
                     klasnaam.Substring(0, 3).Equals("5TS") ||
                     klasnaam.Substring(0, 3).Equals("5KT") ||
                     klasnaam.Substring(0, 3).Equals("6BH") ||
                     klasnaam.Substring(0, 3).Equals("6KT"))
            {
                if (klasnaam.Substring(0, 4).Equals("5TSV") ||
                    klasnaam.Substring(0, 4).Equals("6BHZ") ||
                    klasnaam.Substring(0, 4).Equals("6BHV"))
                {
                    klas = _database.Klas.SingleOrDefault(l =>
                        l.klasnaam.Substring(0, 4).Equals(klasnaam.Substring(0, 4)));
                }
                else
                {
                    klas = _database.Klas.SingleOrDefault(l =>
                    l.klasnaam.Substring(0, 6).Equals(klasnaam.Substring(0, 6)));
                }

            }
            else if (klasnaam.Substring(0, 3).Equals("5BH") ||
                     klasnaam.Substring(0, 3).Equals("5KA") ||
                     klasnaam.Substring(0, 3).Equals("6KA"))

            {
                //if (klasnaam.Substring(0, 4).Equals("5BHV") ||
                //    klasnaam.Substring(0, 4).Equals("5BHZ") ||
                //    klasnaam.Substring(0, 4).Equals("5KAB") ||
                //    klasnaam.Substring(0, 4).Equals("5KAV"))
                //{
                klas = _database.Klas.SingleOrDefault(l =>
                    l.klasnaam.Substring(0, 4).Equals(klasnaam.Substring(0, 4)));
            }

            else if (klasnaam.Substring(0, 3).Equals("6TS"))

            {
                if (klasnaam.Substring(0, 4).Equals("6TST"))
                {
                    klas = _database.Klas.SingleOrDefault(l =>
                        l.klasnaam.Substring(0, 6).Equals(klasnaam.Substring(0, 6)));
                }
                else if (klasnaam.Substring(0, 4).Equals("6TSV"))
                {
                    klas = _database.Klas.SingleOrDefault(l =>
                        l.klasnaam.Substring(0, 5).Equals(klasnaam.Substring(0, 5)));
                }
            }
            else
            {
                klas = _database.Klas.SingleOrDefault(l => l.klasnaam.Substring(0, 3).Equals(klasnaam.Substring(0, 3)));
            }

            return klas;
        }

        public IList<Klas> Find()
        {
            var klassen = _database.Klas.OrderBy(k => k.klasnaam).ToList();

            foreach (var klas in klassen)
            {
                klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
                klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
                klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();
            }
            return klassen;
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
