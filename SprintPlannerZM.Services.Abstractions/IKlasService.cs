using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IKlasService
    {
        Klas Get(int id);
        Klas GetByKlasName(string id);
        Klas GetBySubString(string klasnaam);
        IList<Klas> Find();
        Task<IQueryable<Klas>> FindAsyncPagingQueryable();
        Klas Create(Klas klas);
        Klas Update(int id, Klas klas);
        bool Delete(int id);
    }
}
