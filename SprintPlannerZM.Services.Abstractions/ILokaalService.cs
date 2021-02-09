using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILokaalService
    {
        Lokaal Get(int id);
        IList<Lokaal> Find();
        Lokaal Create(Lokaal lokaal);
        Lokaal Update(int id, Lokaal lokaal);
        bool Delete(int id);
    }
}
