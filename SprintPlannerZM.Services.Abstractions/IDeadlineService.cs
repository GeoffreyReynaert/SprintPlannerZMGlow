using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IDeadlineService
    {
        Deadline Get(int id);
        IList<Deadline> Find();
        Deadline Create(Deadline deadline);
        Deadline Update(int id, Deadline deadline);
        bool Delete(int id);
    }
}
