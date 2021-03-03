using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IHulpleerlingService
    {
        Task<Hulpleerling> Get(int id);
        Task<Hulpleerling> GetbyLeerlingId(long id);
        Task<IList<Hulpleerling>> Find();
        Task<Hulpleerling> Create(Hulpleerling hulpleerling);
        Task<Hulpleerling> Update(int id, Hulpleerling hulpleerling);
        Task<bool> Delete(int id);
    }
}
