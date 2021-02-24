using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

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
            var leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == id);
            leerkracht.Klassen = _database.Klas.Where(k => k.titularisID == leerkracht.leerkrachtID).ToList();
            leerkracht.Vakken = _database.Vak.Where(v => v.leerkrachtID == leerkracht.leerkrachtID).ToList();
            leerkracht.Sprintlokalen = _database.Sprintlokaal.Where(s => s.leerkrachtID == leerkracht.leerkrachtID).ToList();
            return leerkracht;
        }

        public IList<Leerkracht> Find()
        {
            var leerkrachten = _database.Leerkracht.ToList();
            foreach (var leerkracht in leerkrachten)
            {
                leerkracht.Klassen = _database.Klas.Where(k => k.titularisID == leerkracht.leerkrachtID).ToList();
                leerkracht.Vakken = _database.Vak.Where(v => v.leerkrachtID == leerkracht.leerkrachtID).ToList();
                leerkracht.Sprintlokalen = _database.Sprintlokaal.Where(s => s.leerkrachtID == leerkracht.leerkrachtID).ToList();
            }
            return _database.Leerkracht.OrderBy(l => l.achternaam).ToList();
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


        public Leerkracht Update(long id, Leerkracht leerkracht)
        {
            {
                var leerkrachtToUpd = _database.Leerkracht.SingleOrDefault(i => i.leerkrachtID == id);
                leerkrachtToUpd.achternaam = leerkracht.achternaam;
                leerkrachtToUpd.voornaam = leerkracht.voornaam;
                leerkrachtToUpd.status = leerkracht.status;
                leerkrachtToUpd.email = leerkracht.email;
                leerkrachtToUpd.kluisNr = leerkracht.kluisNr;
                leerkrachtToUpd.sprintToezichter = leerkracht.sprintToezichter;
                leerkrachtToUpd.rol = leerkracht.rol;
                _database.Leerkracht.Update(leerkrachtToUpd);
                _database.SaveChanges();
                return leerkracht;
            }
        }

        public bool Delete(long id)
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
