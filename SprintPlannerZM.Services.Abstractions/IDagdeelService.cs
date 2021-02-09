using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IDagdeelService
    {
        Dagdeel Get(int id);
        IList<Dagdeel> Find();
        Dagdeel Create(Dagdeel dagdeel);
        Dagdeel Update(int id, Dagdeel dagdeel);
        bool Delete(int id);
    }
}
