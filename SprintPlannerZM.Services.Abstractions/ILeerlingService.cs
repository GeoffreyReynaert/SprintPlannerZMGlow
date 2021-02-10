using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Leerling Get(long id);
        IList<Leerling> Find();
        IList<Leerling> FindByKlasID(int klasid);
        Leerling Create(Leerling leerling);
        Leerling Update(long id, Leerling leerling);
        bool Delete(int id);
    }
}
