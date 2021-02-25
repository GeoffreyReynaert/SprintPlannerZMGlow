using System;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class ExamenroosterService : IExamenroosterService

    {
        private readonly TihfDbContext _database;

        public ExamenroosterService(TihfDbContext database)
        {
            _database = database;
        }

        public Examenrooster Get(int id)
        {
            var examenrooster = _database.Examenrooster.SingleOrDefault(e => e.examenID == id);
            var vak = _database.Vak.SingleOrDefault(v => v.vakID == examenrooster.vakID);
            examenrooster.Vak = vak;
            return examenrooster;
        }



        //Enkel voor de leerlingenverdeling
        public IList<Examenrooster> FindByDatum(DateTime date)
        {
            var examenroosters = _database.Examenrooster.Where(e => e.datum == date).ToList();

            foreach (var rooster in examenroosters)
            {
                rooster.Vak = _database.Vak.SingleOrDefault(v => v.vakID == rooster.vakID); ;
            }
            return examenroosters;
        }

        public IList<Examenrooster> FindDistinct()
        {
            var examenRoosters = _database.Examenrooster.Select(e => e.datum).Distinct().OrderBy(e => e.Date).ToList();
            IList<Examenrooster> examens = new List<Examenrooster>();
            foreach (var String in examenRoosters)
            {
                var rooster = new Examenrooster()
                {
                    datum = String
                };
                examens.Add(rooster);
            }
            return examens;
        }

        public IList<Examenrooster> Find()
        {
            var examenRoosters = _database.Examenrooster.ToList();
            foreach (var rooster in examenRoosters)
            {
                rooster.Vak = _database.Vak.SingleOrDefault(v => v.vakID == rooster.vakID);
            }
            return examenRoosters;
        }

        public Examenrooster Create(Examenrooster examenrooster)
        {
            _database.Examenrooster.Add(examenrooster);
            _database.SaveChanges();
            return examenrooster;
        }

        public Examenrooster Update(int id, Examenrooster examenrooster)
        {
            {
                var dbExamenrooster = Get(id);
                if (dbExamenrooster == null)
                {
                    return examenrooster;
                }

                _database.Examenrooster.Update(dbExamenrooster);
                _database.SaveChanges();
                return examenrooster;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbExamenrooster = Get(id);
                if (dbExamenrooster == null)
                {
                    return false;
                }

                _database.Examenrooster.Remove(dbExamenrooster);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
