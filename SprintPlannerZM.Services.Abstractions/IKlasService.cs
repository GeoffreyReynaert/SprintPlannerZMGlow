using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IKlasService
    {
        Klas Get(int id);
        Klas GetByKlasName(string id);
        Klas GetBySubString(string klasnaam);
        IList<Klas> Find();
        Klas Create(Klas klas);
        Klas Update(int id, Klas klas);
        bool Delete(int id);
    }
}
