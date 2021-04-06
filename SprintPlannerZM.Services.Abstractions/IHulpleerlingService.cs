using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IHulpleerlingService
    {
        Task<Hulpleerling> Get(long? id);
        Task<Hulpleerling> GetHulpAsync(long? id);
        Task<Hulpleerling> GetbyLeerlingId(long id);
        Task<Hulpleerling> GetOverzicht(long id);
        Task<IList<Hulpleerling>> FindOverzicht();
        Task<IList<Hulpleerling>> simpleFind();
        Task<IList<Hulpleerling>> Find();
        Task<Hulpleerling> Create(Hulpleerling hulpleerling);
        Task<Hulpleerling> Update(long? id, Hulpleerling hulpleerling);
        Task<bool> DeleteByAsync(long? id);
    }
}
