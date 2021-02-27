using System.Collections.Generic;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Leerling Get(long id);
        Leerling GetToImport(long id);
        IList<Leerling> Find();
        Task<IList<Leerling>> FindAsync();
        IList<Leerling> FindByKlasID(int klasid);
        Leerling Create(Leerling leerling);
        Leerling Update(long id, Leerling leerling);
        bool Delete(int id);
    }
}
