using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IKlasService
    {
        Task<Klas> GetAsync(int id);
        Klas GetByKlasName(string id);
        Klas GetBySubString(string klasnaam);
        IList<Klas> Find();
        Task<IQueryable<Klas>> FindAsyncPagingQueryable();
        Task<Klas> CreateAsync(Klas klas);
        Task<Klas> UpdateAsync(int id, Klas klas);
        //Task<bool> Delete(int id);
    }
}
