using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class LokaalService: ILokaalService
    {
        private readonly TihfDbContext _database;

        public LokaalService(TihfDbContext database)
        {
            _database = database;
        }

        public async Task<Lokaal> GetAsync(int id)
        {
            return await _database.Lokaal.SingleOrDefaultAsync(l => l.lokaalID == id);
        }

        public async Task<Lokaal> GetByNameAsync(string lokaalnaam)
        {
            return await _database.Lokaal.SingleOrDefaultAsync(l => l.lokaalnaam.Equals(lokaalnaam));
        }

        public async Task<IList<Lokaal>> FindAsync()
        {
            return await _database.Lokaal.ToListAsync();
        }

        public async Task<IList<Lokaal>> FindForSprintAsync()
        {
            return await _database.Lokaal.Where(l => l.lokaaltype.Equals("sprint")).ToListAsync();
        }


        public  IQueryable<Lokaal> FindAsyncPagingQueryable()
        {
            var lokalen =  _database.Lokaal
                .Include(l => l.Sprintlokalen)
                .AsQueryable();

            return lokalen;
        }


        public async Task<Lokaal> CreateAsync(Lokaal lokaal)
        {
            await _database.Lokaal.AddAsync(lokaal);
            await _database.SaveChangesAsync();
            return lokaal;
        }

        public async Task<Lokaal> UpdateAsync(int id, Lokaal lokaal)
        {
            {
                var lokaalToUpd = await _database.Lokaal.SingleOrDefaultAsync(l => l.lokaalID == id);
                lokaalToUpd.lokaalnaam = lokaal.lokaalnaam;
                lokaalToUpd.naamafkorting = lokaal.naamafkorting;
                lokaalToUpd.sprintlokaal = lokaal.sprintlokaal;
                lokaalToUpd.capaciteit = lokaal.capaciteit;
                lokaalToUpd.lokaaltype = lokaal.lokaaltype;
                _database.Lokaal.Update(lokaalToUpd);
               await _database.SaveChangesAsync();
                return lokaalToUpd;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dblokaal = await GetAsync(id);
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
