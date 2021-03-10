using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintlokaalreservatieService
    {
        Task<SprintlokaalReservatie> Get(int id);
        Task<IList<SprintlokaalReservatie>> Find();
        Task<IList<SprintlokaalReservatie>> FindByExamIDAndType(int examID, string type);
        Task<SprintlokaalReservatie> Create(SprintlokaalReservatie sprintlokaalReservatie);
        Task<SprintlokaalReservatie> Update(int id, SprintlokaalReservatie sprintlokaalReservatie);
        Task<bool> Delete(int id);
    }
}
