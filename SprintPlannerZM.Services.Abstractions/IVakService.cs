using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IVakService
    {
        Task<Vak> GetAsync(int id);
        Vak GetBySubString(string vakNaam, int klasID);
        IList<Vak> Find();
        Task<IQueryable<Vak>> FindAsyncPagingQueryable();
        Vak Create(Vak vak);
        Vak Update(int id, Vak vak);
        Task<bool> Delete(int id);
    }
}
