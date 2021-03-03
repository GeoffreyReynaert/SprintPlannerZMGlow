using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IBeheerderService
    {
        Task<Beheerder> GetAsync(int id);
        Task<IList<Beheerder>> FindAsync();
        Task<Beheerder> CreateAsync(Beheerder beheerder);
        Task<Beheerder> UpdateAsync(int id, Beheerder beheerder);
        Task<bool> DeleteAsync(int id);
    }
}
