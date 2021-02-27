using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class LokaalService: ILokaalService
    {
        private readonly TihfDbContext _database;

        public LokaalService(TihfDbContext database)
        {
            _database = database;
        }

        public Lokaal Get(int id)
        {
            return _database.Lokaal.SingleOrDefault(l => l.lokaalID == id);
        }

        public Lokaal GetByName(string lokaalnaam)
        {
            return _database.Lokaal.SingleOrDefault(l => l.lokaalnaam.Equals(lokaalnaam));
        }

        public IList<Lokaal> Find()
        {
            return _database.Lokaal.ToList();
        }

        public IList<Lokaal> FindForSprint()
        {
            return _database.Lokaal.Where(l => l.lokaaltype.Equals("sprint")).ToList();
        }

        public Lokaal Create(Lokaal lokaal)
        {
            _database.Lokaal.Add(lokaal);
            _database.SaveChanges();
            return lokaal;
        }

        public Lokaal Update(int id, Lokaal lokaal)
        {
            {
                var lokaalToUpd = _database.Lokaal.SingleOrDefault(l => l.lokaalID == id);
                lokaalToUpd.lokaalnaam = lokaal.lokaalnaam;
                lokaalToUpd.naamafkorting = lokaal.naamafkorting;
                lokaalToUpd.sprintlokaal = lokaal.sprintlokaal;
                lokaalToUpd.capaciteit = lokaal.capaciteit;
                lokaalToUpd.lokaaltype = lokaal.lokaaltype;
                _database.Lokaal.Update(lokaalToUpd);
                _database.SaveChanges();
                return lokaalToUpd;
            }
        }

        public bool Delete(int id)
        {
            {
                var dblokaal = Get(id);
                if (dblokaal == null)
                {
                    return false;
                }
                _database.Lokaal.Remove(dblokaal);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
