using System;
using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingverdelingService
    {
        Task<Leerlingverdeling> Get(int id);
        Task<IList<Leerlingverdeling>> Find();
        Task<IList<Leerlingverdeling>> FindAantalBySprintLokaalId(int sprintlokaalreservatieId, string type);
        Task<IList<Leerlingverdeling>> FindBySprintLokaalReservatie(int sprintlokaalreservatieId);
        Task<Leerlingverdeling> Create(Leerlingverdeling leerlingverdeling);
        Task<Leerlingverdeling> Update(int id, Leerlingverdeling leerlingverdeling);
        Task<bool> Delete(int id);
        Task<bool> DeleteAllFromDate(DateTime date);
    }
}
