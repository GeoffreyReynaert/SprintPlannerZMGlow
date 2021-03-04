using System;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Examenrooster> Get(int id)
        {
            var examenrooster =await _database.Examenrooster
                .Where(e => e.examenID == id)
                .Include(e=>e.Vak)
                .SingleOrDefaultAsync();
            
            return examenrooster;
        }



        //Enkel voor de leerlingenverdeling
        public async Task<IList<Examenrooster>> FindByDatum(DateTime date)
        {
            var examenroosters =await _database.Examenrooster
                .Where(e => e.datum.Equals(date))
                .Include(e=>e.Vak)
                .ToListAsync();

            return examenroosters;
        }

        //ADMIN leerlingverdeling
        public async Task<IList<Examenrooster>> FindDistinct()
        {
            IList<Examenrooster> examens = new List<Examenrooster>();
            var examenRoosters = await _database.Examenrooster
                .Select(e => e.datum).Distinct()
                .OrderBy(e => e.Date)
                .ToListAsync();

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

        public async Task<IList<Examenrooster>> Find()
        {
            var examenRoosters = await _database.Examenrooster.ToListAsync();
            foreach (var rooster in examenRoosters)
            {
                rooster.Vak =await _database.Vak.SingleOrDefaultAsync(v => v.vakID == rooster.vakID);
            }
            return examenRoosters;
        }


        public async Task<Examenrooster> Create(Examenrooster examenrooster)
        {
           await _database.Examenrooster.AddAsync(examenrooster);
           await _database.SaveChangesAsync();
            return examenrooster;
        }

        public async Task<Examenrooster> Update(int id, Examenrooster examenrooster)
        {
            {
                var dbExamenrooster =await Get(id);
                if (dbExamenrooster == null)
                {
                    return examenrooster;
                }

                _database.Examenrooster.Update(dbExamenrooster);
                await _database.SaveChangesAsync();
                return examenrooster;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbExamenrooster =await Get(id);
                if (dbExamenrooster == null)
                {
                    return false;
                }

                _database.Examenrooster.Remove(dbExamenrooster);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
