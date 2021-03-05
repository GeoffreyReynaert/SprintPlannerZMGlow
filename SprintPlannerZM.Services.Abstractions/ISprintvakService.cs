using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintvakService
    {
        Task<Sprintvak> GetAsync(long id);
        Task<Sprintvak> GetByVakAndHulpleerlingID(int vakId, long hulpleerlingId);
        Task<IList<Sprintvak>> FindAsync();
        Task<Sprintvak> CreateAsync(Sprintvak sprintvak);
        Task<Sprintvak> UpdateAsync(int id, Sprintvak sprintvak);
        Task<bool> DeleteAsync(int id);
    }
}
