using System;
using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintlokaalreservatieService
    {
        Task<Sprintlokaalreservatie> Get(int id);
        Task<List<Sprintlokaalreservatie>> GetTime(DateTime datum);
        Task<IList<Sprintlokaalreservatie>> Find();
        Task<IList<Sprintlokaalreservatie>> FindDul();
        Task<List<DateTime>> FindDistinctDatums();
        Task<IList<Sprintlokaalreservatie>> FindByDate(DateTime date);
        Task<IList<Sprintlokaalreservatie>> FindByDateOnly(DateTime date);
        Task<IList<Sprintlokaalreservatie>> FindAantalBySprintreservatieIdAndType(int sprintreservatieid, string type);
        Task<IList<Sprintlokaalreservatie>> FindByExamIDAndType(int examID, string type);
        Task<Sprintlokaalreservatie> Create(Sprintlokaalreservatie sprintlokaalreservatie);
        Task<Sprintlokaalreservatie> Update(int id, Sprintlokaalreservatie sprintlokaalreservatie);
        Task<bool> Delete(int id);
        Task<bool> DeleteAllFromDate(DateTime date);
    }
}
