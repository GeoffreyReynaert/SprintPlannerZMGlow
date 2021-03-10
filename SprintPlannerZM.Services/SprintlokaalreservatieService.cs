using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class SprintlokaalreservatieService: ISprintlokaalreservatieService
    {
        private readonly TihfDbContext _database;

        public SprintlokaalreservatieService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<SprintlokaalReservatie> Get(int id)
        {
            return await _database.Sprintlokaal
                .Where(s => s.sprintlokaalreservatieID == id)
                .SingleOrDefaultAsync();
        }

        public async Task<IList<SprintlokaalReservatie>> Find()
        {
            return await _database.Sprintlokaal
                .Include(s=>s.Lokaal)
                .Include(s=>s.Examen)
                .ThenInclude(e=>e.Vak)
                .ToListAsync();
        }


        public async Task<IList<SprintlokaalReservatie>> FindByExamIDAndType(int examid,string type)
        {
            return await _database.Sprintlokaal
                .Where(s=>s.examenID == examid)
                .Where(s=>s.reservatietype.Equals(type))
                .ToListAsync();
        }


        public async Task<SprintlokaalReservatie> Create(SprintlokaalReservatie sprintlokaalReservatie)
        {
           await _database.Sprintlokaal.AddAsync(sprintlokaalReservatie);
           await _database.SaveChangesAsync();
            return sprintlokaalReservatie;
        }

        public async Task<SprintlokaalReservatie> Update(int id, SprintlokaalReservatie sprintlokaalReservatie)
        {
            {
                var dbSprintlokaal =await Get(id);
                if (dbSprintlokaal == null)
                {
                    return sprintlokaalReservatie;
                }
                _database.Sprintlokaal.Update(dbSprintlokaal);
                await _database.SaveChangesAsync();
                return sprintlokaalReservatie;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbSprintlokaal =await Get(id);
                if (dbSprintlokaal == null)
                {
                    return false;
                }
                _database.Sprintlokaal.Remove(dbSprintlokaal);
               await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
