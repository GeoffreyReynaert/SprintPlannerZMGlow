using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class LeerlingverdelingService: ILeerlingverdelingService
    {
        private readonly TihfDbContext _database;

        public LeerlingverdelingService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Leerlingverdeling> Get(int id)
        {
            return await _database.Leerlingverdeling
                .Where(l => l.leerlingverdelingID == id)
                .SingleOrDefaultAsync();
        }

        public async Task<IList<Leerlingverdeling>> Find()
        {
            return await _database.Leerlingverdeling
                .ToListAsync();
        }

        public async Task<IList<Leerlingverdeling>> FindBySprintLokaalReservatie(int sprintlokaalreservatieId)
        {
            return await _database.Leerlingverdeling
                .Include(l => l.Hulpleerling)
                .ThenInclude(h => h.Leerling)
                .ThenInclude(l => l.Klas)
                .Include(l => l.Examenrooster)
                .ThenInclude(e => e.Vak)
                .Where(l=>l.sprintlokaalreservatieID== sprintlokaalreservatieId)
                .ToListAsync();
        }

        public async Task<IList<Leerlingverdeling>> FindAantalBySprintLokaalId(int sprintlokaalreservatieId ,string type)
        {
            return await _database.Leerlingverdeling
                .Where(l => l.sprintlokaalreservatieID == sprintlokaalreservatieId)
                .Where(l => l.reservatietype.Equals(type))
                .ToListAsync();
        }



        public async Task<Leerlingverdeling> Create(Leerlingverdeling leerlingverdeling)
        {
           await _database.Leerlingverdeling.AddAsync(leerlingverdeling);
           await _database.SaveChangesAsync();
            return leerlingverdeling;
        }

        public async Task<Leerlingverdeling> Update(int id, Leerlingverdeling leerlingverdeling)
        {
            {
                var dbLeerlingverdeling = await Get(id);
                if (dbLeerlingverdeling == null)
                {
                    return leerlingverdeling;
                }
                _database.Leerlingverdeling.Update(dbLeerlingverdeling);
               await _database.SaveChangesAsync();
                return leerlingverdeling;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbLeerlingverdeling = await Get(id);
                if (dbLeerlingverdeling == null)
                {
                    return false;
                }
                _database.Leerlingverdeling.Remove(dbLeerlingverdeling);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
