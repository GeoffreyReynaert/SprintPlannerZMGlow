using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerlingverdelingService
    {
        Leerlingverdeling Get(int id);
        IList<Leerlingverdeling> Find();
        Leerlingverdeling Create(Leerlingverdeling leerlingverdeling);
        Leerlingverdeling Update(int id, Leerlingverdeling leerlingverdeling);
        bool Delete(int id);
    }
}
