using System;
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
                .Include(l => l.Lokaal)
                .Include(l => l.Leerkracht)
                .Include(l => l.Leerlingverdelingen)
                .ThenInclude(i=>i.Hulpleerling)
                .ThenInclude(l=>l.Leerling)
                .ThenInclude(l=>l.Klas)
                .Include(l=>l.Examen)
                .ThenInclude(e=>e.Vak)
                .SingleOrDefaultAsync();
        }

        public async Task<IList<Sprintlokaalreservatie>> Find()
        {
            return await _database.Sprintlokaalreservatie
                .Include(l=>l.Lokaal)
                .Include(l=>l.Leerkracht)
                .Include(l=>l.Leerlingverdelingen)
                .Include(l=>l.Examen)
                .ThenInclude(e=>e.Vak)
                .ThenInclude(k=>k.klas)
                .ToListAsync();
        }

        public async Task<IList<Sprintlokaalreservatie>> FindByDate(DateTime date)
        {
            return await _database.Sprintlokaalreservatie
                .Where(s => s.datum.Date.Equals(date.Date))
                .Include(l => l.Lokaal)
                .Include(l => l.Leerkracht)
                .Include(l => l.Leerlingverdelingen)
                .Include(l => l.Examen)
                .ThenInclude(e => e.Vak)
                .ThenInclude(k => k.klas)
                .ToListAsync();
        }
        public async Task<IList<Sprintlokaalreservatie>> FindAantalBySprintreservatieIdAndType(int reservatieID, string type)
        {
            return await _database.Sprintlokaalreservatie
                .Where(s => s.sprintlokaalreservatieID == reservatieID)
                .Where(s => s.reservatietype.Equals(type))
                .ToListAsync();
        }

        public async Task<IList<Sprintlokaalreservatie>> FindByExamIDAndType(int examid, string type)
        {
            return await _database.Sprintlokaalreservatie
                .Where(s => s.examenID == examid)
                .Where(s => s.reservatietype.Equals(type))
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
        public async Task<bool> DeleteAllFromDate(DateTime date)
        {
            {
                var dbSprintlokaalreservatie = await FindByDate(date);

                foreach (var reservatie in dbSprintlokaalreservatie)
                {
                    _database.Sprintlokaalreservatie.Remove(reservatie);
                    await _database.SaveChangesAsync();
                }
              
                return true;
            }
        }
    }
}
