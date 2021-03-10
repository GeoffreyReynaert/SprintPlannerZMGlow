using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class LeerkrachtService: ILeerkrachtService
    {
        private readonly TihfDbContext _database;

        public LeerkrachtService(TihfDbContext database)
        {
            _database = database;
        }

        public async Task<Leerkracht> Get(long id)
        {
            var leerkracht = await _database.Leerkracht
                .Where(l => l.leerkrachtID == id)
                .Include(l=>l.Klassen)
                .Include(l => l.Vakken)


                .SingleOrDefaultAsync();
            //leerkracht.Klassen = await _database.Klas.Where(k => k.titularisID == leerkracht.leerkrachtID).ToListAsync();
            //leerkracht.Vakken = await _database.Vak.Where(v => v.leerkrachtID == leerkracht.leerkrachtID).ToListAsync();
            //leerkracht.Sprintlokalen = await _database.Sprintlokaal.Where(s => s.leerkrachtID == leerkracht.leerkrachtID).ToListAsync();
            return leerkracht;
        }

        public async Task<IList<Leerkracht>> Find()
        {
            var leerkrachten =await _database.Leerkracht.ToListAsync();
            foreach (var leerkracht in leerkrachten)
            {
                leerkracht.Klassen =await _database.Klas.Where(k => k.titularisID == leerkracht.leerkrachtID).ToListAsync();
                leerkracht.Vakken =await _database.Vak.Where(v => v.leerkrachtID == leerkracht.leerkrachtID).ToListAsync();
                leerkracht.Sprintlokaalreservaties =await _database.Sprintlokaalreservatie.Where(s => s.leerkrachtID == leerkracht.leerkrachtID).ToListAsync();
            }
            return await _database.Leerkracht.OrderBy(l => l.achternaam).ToListAsync();
        }


        public async Task<IQueryable<Leerkracht>> FindAsyncPagingQueryable()
        {
            var leerkrachten =  _database.Leerkracht
                .Include( l => l.Vakken)
                .AsQueryable();

            return  leerkrachten;
        }


        public async Task<Leerkracht> Create(Leerkracht leerkracht)
        {
            var dbLeerkracht = await _database.Leerkracht.SingleOrDefaultAsync(l => l.leerkrachtID == leerkracht.leerkrachtID);
            if (dbLeerkracht==null)
            {
                await _database.Leerkracht.AddAsync(leerkracht);
                await _database.SaveChangesAsync();
            }
            return leerkracht;
        }


        public async Task<Leerkracht> Update(long id, Leerkracht leerkracht)
        {
            {
                var leerkrachtToUpd =await _database.Leerkracht.SingleOrDefaultAsync(i => i.leerkrachtID == id);
                leerkrachtToUpd.achternaam = leerkracht.achternaam;
                leerkrachtToUpd.voornaam = leerkracht.voornaam;
                leerkrachtToUpd.status = leerkracht.status;
                leerkrachtToUpd.email = leerkracht.email;
                leerkrachtToUpd.kluisNr = leerkracht.kluisNr;
                leerkrachtToUpd.sprintToezichter = leerkracht.sprintToezichter;
                leerkrachtToUpd.rol = leerkracht.rol;
                _database.Leerkracht.Update(leerkrachtToUpd);
               await _database.SaveChangesAsync();
                return leerkracht;
            }
        }

        public async Task<bool> Delete(long id)
        {
            {
                var dbLeerkracht =await Get(id);
                if (dbLeerkracht == null)
                {
                    return false;
                }
                _database.Leerkracht.Remove(dbLeerkracht);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
