using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintvakkeuzeService
    {
        Task<Sprintvakkeuze> GetAsync(long id);
        Task<IList<Sprintvakkeuze>> GetHulpVakAsync(long? hulpleerlingID);
        Task<Sprintvakkeuze> GetByVakAndHulpleerlingID(int vakId, long hulpleerlingId);
        Task<IList<Sprintvakkeuze>> FindAsync();
        Task<Sprintvakkeuze> CreateAsync(Sprintvakkeuze sprintvakkeuze);
        Task<Sprintvakkeuze> UpdateAsync(int id, Sprintvakkeuze sprintvakkeuze);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByHulpAsync(long? hulpleerlingID);
    }
}
