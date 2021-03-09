using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IVakService
    {
        Task<Vak> GetAsync(int id);
        Task<Vak> GetBySubString(string vakNaam, int klasID);
        Task<IList<Vak>> GetByKlasId(int id);
        Task<IList<Vak>> Find();
        Task<IQueryable<Vak>> FindAsyncPagingQueryable();
        Task<Vak> Create(Vak vak);
        Task<Vak> UpdateAsync(int id, Vak vak);
        Task<bool> Delete(int id);
    }
}
