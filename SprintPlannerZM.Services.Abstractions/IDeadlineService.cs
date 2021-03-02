using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IDeadlineService
    {
        Task<Deadline> Get(int id);
        Task<IList<Deadline>> Find();
        Task<Deadline> Create(Deadline deadline);
        Task<Deadline> Update(int id, Deadline deadline);
        Task<bool> Delete(int id);
    }
}
