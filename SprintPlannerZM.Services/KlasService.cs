using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SprintPlannerZM.Services
{
    public class KlasService : IKlasService
    {

        private readonly TihfDbContext _database;

        public KlasService(TihfDbContext database)
        {
            _database = database;
        }

        public async Task<Klas> GetAsync(int id)
        {
            var klas = await _database.Klas
                .Where(k => k.klasID == id)
                .Include(k => k.Leerkracht)
                .Include(k => k.Leerlingen)
                .Include(k => k.Vakken)
                .ThenInclude(v => v.Leerkracht)
                .SingleOrDefaultAsync();
            return klas;
        }

        public Klas GetSprintvakWithKlas(int id)
        {
            var klas = _database.Klas.SingleOrDefault(k => k.klasID == id);
            klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
            klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
            klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();
            foreach (var vak in klas.Vakken)
            {
                vak.Sprintvakken = _database.Sprintvak.Where(s => s.vakID == vak.vakID).ToList();
            }

            return klas;
        }

        public async Task<Klas> GetByKlasName(string name)
        {
            var klas = await _database.Klas.Where(k => k.klasnaam.Equals(name))
                .Include(k => k.Leerlingen)
                .SingleOrDefaultAsync();
            return klas;
        }




        public async Task<IList<Klas>> Find()
        {
            var klassen = await _database.Klas
                .Include(k => k.Leerkracht)
                .Include(k => k.Leerlingen)
                .Include(k => k.Vakken)
                .OrderBy(k => k.klasnaam).ToListAsync();

            return klassen;
        }

        public async Task<IQueryable<Klas>> FindAsyncPagingQueryable()
        {
            var klassen = _database.Klas
                .Include(k => k.Leerkracht)
                .Include(k => k.Leerlingen)
                .Include(k => k.Vakken)
                .AsQueryable();

            return klassen;
        }

        public async Task<Klas> CreateAsync(Klas klas)
        {
            var dbLeerkracht = await _database.Klas.SingleOrDefaultAsync(l => l.klasID == klas.klasID);
            if (dbLeerkracht == null)
            {
                await _database.Klas.AddAsync(klas);
                await _database.SaveChangesAsync();
            }
            return klas;
        }


        public async Task<Klas> UpdateAsync(int id, Klas klas)
        {
            {
                var klasToUpd = await _database.Klas.SingleOrDefaultAsync(l => l.klasID == id);
                klasToUpd.klasnaam = klas.klasnaam;
                klasToUpd.titularisID = klas.titularisID;
                _database.Klas.Update(klasToUpd);
                await _database.SaveChangesAsync();
                return klasToUpd;
            }
        }

        public async Task<bool> Delete(int id)
        {
            {
                var dbKlas = await GetAsync(id);
                if (dbKlas == null)
                {
                    return false;
                }
                _database.Klas.Remove(dbKlas);
                await _database.SaveChangesAsync();
                return true;
            }
        }
    }
}



//public Klas OldGet(int id)
//{
//    var klas = _database.Klas.SingleOrDefault(k => k.klasID == id);
//    klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
//    klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
//    klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();

//    return klas;
//}

//public IList<Klas> Find()
//{
//var klassen = _database.Klas.OrderBy(k => k.klasnaam).ToList();

//    foreach (var klas in klassen)
//{
//    klas.Leerlingen = _database.Leerling.Where(l => klas != null && l.KlasID == klas.klasID).ToList();
//    klas.Leerkracht = _database.Leerkracht.SingleOrDefault(l => l.leerkrachtID == klas.titularisID);
//    klas.Vakken = _database.Vak.Where(v => v.klasID == klas.klasID).ToList();
//}
//return klassen;
//}
