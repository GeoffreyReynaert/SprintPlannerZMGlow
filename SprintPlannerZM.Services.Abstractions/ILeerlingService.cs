using System.Collections.Generic;
using System.Linq;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Leerling Get(long id);
        Leerling GetToImport(long id);
        IList<Leerling> Find();
        IList<Leerling> FindByKlasID(int klasid);
        IQueryable<Leerling> FindAsyncPagingQueryable();
        Leerling Create(Leerling leerling);
        Leerling Update(long id, Leerling leerling);
        bool Delete(int id);
    }
}
