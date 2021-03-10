using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class VakService : IVakService
    {
        private readonly TihfDbContext _database;

        public VakService(TihfDbContext database)
        {
            _database = database;
        }


        public async Task<Vak> GetAsync(int id)
        {
            var vak = await _database.Vak
                .Include(v => v.klas)
                .Include(v => v.Leerkracht)
                .Include(v => v.Sprintvakkeuzes)
                .Include(v => v.Examenroosters)
                .SingleOrDefaultAsync(v => v.vakID == id);

            return vak;
        }

        public async Task<IList<Vak>> GetByKlasId(int id)
        {
            var vakken = await _database.Vak
                .Where(v => v.klasID == id).ToListAsync();
            return vakken;
        }


        //enkel voor importeren examens gebruikt
        public async Task<Vak> GetBySubString(string vakNaam, int klasID)
        {

            var vak = await _database.Vak
                .Where(v => v.vaknaam.Substring(0, 3).ToLower().Equals(vakNaam.Substring(0, 3).ToLower()))
                .Where(v => v.klasID == klasID)
                .Include(v => v.klas)
                .Include(v => v.Leerkracht)
                .FirstOrDefaultAsync();

            return vak;
        }


        public async Task<IList<Vak>> Find()
        {
            var vakken = await _database.Vak
                .Include(v => v.klas)
                .Include(v => v.Leerkracht)
                .Include(v => v.Sprintvakkeuzes)
                .Include(v => v.Examenroosters)
                .ToListAsync();

            return vakken;
        }


        public async Task<IQueryable<Vak>> FindAsyncPagingQueryable()
        {
            var vakken = _database.Vak
                .Include(v => v.klas)
                .Include(v => v.Leerkracht)
                .Include(v => v.Sprintvakkeuzes)
                .AsQueryable();

            return vakken;
        }


        public async Task<Vak> Create(Vak vak)
        {
            await _database.Vak.AddAsync(vak);
            await _database.SaveChangesAsync();
            return vak;
        }

        public async Task<Vak> UpdateAsync(int id, Vak vak)
        {
            {
                var vakToUpd = await _database.Vak.SingleOrDefaultAsync(i => i.vakID == id);
                vakToUpd.vaknaam = vak.vaknaam;
                vakToUpd.vakID = vak.vakID;
                vakToUpd.leerkrachtID = vak.leerkrachtID;
                _database.Vak.Update(vakToUpd);
                await _database.SaveChangesAsync();
                return vak;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbVak = await GetAsync(id);
                if (dbVak == null)
                {
                    return false;
                }
                _database.Vak.Remove(dbVak);
                await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}

