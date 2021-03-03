using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IVakService
    {
        Vak Get(int id);
        IList<Vak> GetByKlasId(int id);
        Vak GetBySubString(string vakNaam, int klasID);
        IList<Vak> Find();
        //IList<Vak> FindBySubstring(string vakNaam, int klasID);
        Vak Create(Vak vak);
        Vak Update(int id, Vak vak);
        bool Delete(int id);
    }
}
