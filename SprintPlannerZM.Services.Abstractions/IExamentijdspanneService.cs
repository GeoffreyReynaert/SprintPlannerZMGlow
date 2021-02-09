using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IExamentijdspanneService
    {
        Examentijdspanne Get(int id);
        IList<Examentijdspanne> Find();
        Examentijdspanne Create(Examentijdspanne examentijdspanne);
        Examentijdspanne Update(int id, Examentijdspanne examentijdspanne);
        bool Delete(int id);
    }
}
