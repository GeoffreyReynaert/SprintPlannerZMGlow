using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Leerling Get(long id);
        Leerling GetToImport(long id);
        Leerling GetFullLeerling(long id);
        IList<Leerling> Find();
        Task<IQueryable<Leerling>> FindAsyncPagingQueryable();
        IList<Leerling> FindByKlasID(int klasid);
        Leerling Create(Leerling leerling);
        Leerling Update(long id, Leerling leerling);
        bool Delete(int id);
    }
}
