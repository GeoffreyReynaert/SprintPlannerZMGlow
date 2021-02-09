using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintlokaalService
    {
        Sprintlokaal Get(int id);
        IList<Sprintlokaal> Find();
        Sprintlokaal Create(Sprintlokaal sprintlokaal);
        Sprintlokaal Update(int id, Sprintlokaal sprintlokaal);
        bool Delete(int id);
    }
}
