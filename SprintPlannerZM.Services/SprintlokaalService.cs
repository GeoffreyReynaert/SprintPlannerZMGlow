using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class SprintlokaalService: ISprintlokaalService
    {
        private readonly TihfDbContext _database;

        public SprintlokaalService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Sprintlokaal> Get(int id)
        {
            return await _database.Sprintlokaal
                .Where(s => s.sprintlokaalID == id)
                .SingleOrDefaultAsync();
        }

        public async Task<IList<Sprintlokaal>> Find()
        {
            return await _database.Sprintlokaal
                .ToListAsync();
        }


        public async Task<IList<Sprintlokaal>> FindByExamID(int examid)
        {
            return await _database.Sprintlokaal
                .Where(s=>s.examenID == examid)
                .ToListAsync();
        }


        public async Task<Sprintlokaal> Create(Sprintlokaal sprintlokaal)
        {
           await _database.Sprintlokaal.AddAsync(sprintlokaal);
           await _database.SaveChangesAsync();
            return sprintlokaal;
        }

        public async Task<Sprintlokaal> Update(int id, Sprintlokaal sprintlokaal)
        {
            {
                var dbSprintlokaal =await Get(id);
                if (dbSprintlokaal == null)
                {
                    return sprintlokaal;
                }
                _database.Sprintlokaal.Update(dbSprintlokaal);
                await _database.SaveChangesAsync();
                return sprintlokaal;
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
