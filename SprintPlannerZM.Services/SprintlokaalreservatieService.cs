using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class SprintlokaalreservatieService : ISprintlokaalreservatieService
    {
        private readonly TihfDbContext _database;

        public SprintlokaalreservatieService(TihfDbContext database)
        {
            _database = database;
        }
        public async Task<Sprintlokaalreservatie> Get(int id)
        {
            return await _database.Sprintlokaalreservatie
                .Where(s => s.sprintlokaalreservatieID == id)
                .SingleOrDefaultAsync();
        }

        public async Task<IList<Sprintlokaalreservatie>> Find()
        {
            return await _database.Sprintlokaalreservatie
                .ToListAsync();
        }


        public async Task<IList<Sprintlokaalreservatie>> FindByExamID(int examid)
        {
            return await _database.Sprintlokaalreservatie
                .Where(s => s.examenID == examid)
                .ToListAsync();
        }


        public async Task<Sprintlokaalreservatie> Create(Sprintlokaalreservatie sprintlokaalreservatie)
        {
            await _database.Sprintlokaalreservatie.AddAsync(sprintlokaalreservatie);
            await _database.SaveChangesAsync();
            return sprintlokaalreservatie;
        }

        public async Task<Sprintlokaalreservatie> Update(int id, Sprintlokaalreservatie sprintlokaalreservatie)
        {
            {
                var dbSprintlokaalreservatie = await Get(id);
                if (dbSprintlokaalreservatie == null)
                {
                    return sprintlokaalreservatie;
                }
                _database.Sprintlokaalreservatie.Update(dbSprintlokaalreservatie);
                await _database.SaveChangesAsync();
                return sprintlokaalreservatie;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbSprintlokaalreservatie = await Get(id);
                if (dbSprintlokaalreservatie == null)
                {
                    return false;
                }
                _database.Sprintlokaalreservatie.Remove(dbSprintlokaalreservatie);
                await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}
