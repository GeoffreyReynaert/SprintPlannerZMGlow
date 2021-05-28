using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
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
            var examenrooster = await _database.Examenrooster
                .Where(e => e.examenID == id)
                .Include(e => e.Vak)
                .SingleOrDefaultAsync();

            return examenrooster;
        }

        public async Task<IList<Examenrooster>> GetByLeerkracht(long leerkrachtId)
        {
            var vakken = await _database.Examenrooster.Where(l => l.Vak.leerkrachtID == leerkrachtId)
                .Include(v => v.Vak)
                .ThenInclude(k => k.klas)
                .ToListAsync();

            return vakken;
        }

        //Enkel voor de leerlingenverdeling
        public async Task<IList<Examenrooster>> FindByDatum(DateTime date)
        {
            var examenroosters = await _database.Examenrooster
                .Where(e => e.datum.Date.Equals(date.Date))
                .Include(e => e.Vak)
                .OrderBy(e => e.datum.Hour)
                .ToListAsync();

            return examenroosters;
        }

        //ADMIN leerlingverdeling
        public async Task<IList<Examenrooster>> FindDistinct()
        {
            var examenRoosters = await _database.Examenrooster
                .Select(e => e.datum.Date).Distinct()
                .OrderBy(e => e.Date)
                .ToListAsync();

            IList<Examenrooster> examens = new List<Examenrooster>();

            //Objecten aanmaken om lijst van examenroosters weer te geven en geen lijst van datetimes
            foreach (var datum in examenRoosters)
            {
                var leerlingen = await _database.Leerlingverdeling
                    .Where(e => e.Examenrooster.datum.Date.Equals(datum.Date)).ToListAsync();
                var rooster = new Examenrooster
                {
                    datum = datum,
                    Leerlingverdelingen = leerlingen
                };
                examens.Add(rooster);
            }

            return examens;
        }

        public async Task<IList<Examenrooster>> Find()
        {
            var examenRoosters = await _database.Examenrooster.ToListAsync();
            foreach (var rooster in examenRoosters)
                rooster.Vak = await _database.Vak.SingleOrDefaultAsync(v => v.vakID == rooster.vakID);
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
            var dbExamenrooster = await Get(id);
            if (dbExamenrooster == null) return examenrooster;

            _database.Examenrooster.Update(dbExamenrooster);
            await _database.SaveChangesAsync();
            return examenrooster;
        }

        public async Task<Examenrooster> UpdateDocument(int id, Examenrooster examenrooster)
        {
            var examenroosterToUpd = await _database.Examenrooster.SingleOrDefaultAsync(s => s.examenID == id);
            examenroosterToUpd.examendoc = examenrooster.examendoc;
            _database.Examenrooster.Update(examenroosterToUpd);
            await _database.SaveChangesAsync();

            return examenrooster;
        }

        public async Task<bool> Delete(int id)
        {
            var dbExamenrooster = await Get(id);
            if (dbExamenrooster == null) return false;

            _database.Examenrooster.Remove(dbExamenrooster);
            await _database.SaveChangesAsync();
            return true;
        }
    }
}