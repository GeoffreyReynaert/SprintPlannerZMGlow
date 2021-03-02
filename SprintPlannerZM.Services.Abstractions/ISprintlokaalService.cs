using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintlokaalService
    {
        Task<Sprintlokaal> Get(int id);
        Task<IList<Sprintlokaal>> Find();
        Task<IList<Sprintlokaal>> FindByExamID(int examID);
        Task<Sprintlokaal> Create(Sprintlokaal sprintlokaal);
        Task<Sprintlokaal> Update(int id, Sprintlokaal sprintlokaal);
        Task<bool> Delete(int id);
    }
}
