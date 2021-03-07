using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Task<Leerling> Get(long id);
        Task<Leerling> GetToImport(long id);
        Task<IList<Leerling>> Find();
        Task<IList<Leerling>> FindOnlyWithHulpLln();
        IQueryable<Leerling> FindAsyncPagingQueryable();
        Task<IList<Leerling>> FindByKlasID(int klasid);
        Task<Leerling> Create(Leerling leerling);
        Task<Leerling> Update(long id, Leerling leerling);
        Task<bool> Delete(int id);
    }
}
