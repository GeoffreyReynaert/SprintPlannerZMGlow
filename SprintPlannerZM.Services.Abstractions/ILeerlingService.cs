using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingService
    {
        Leerling Get(int id);
        IList<Leerling> Find();
        Leerling Create(Leerling leerling);
        Leerling Update(int id, Leerling leerling);
        bool Delete(int id);
    }
}
