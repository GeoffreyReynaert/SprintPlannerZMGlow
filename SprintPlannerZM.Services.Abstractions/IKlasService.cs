using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IKlasService
    {
        Klas Get(int id);
        Klas Get(string id);
        IList<Klas> Find();
        Klas Create(Klas klas);
        Klas Update(int id, Klas klas);
        bool Delete(int id);
    }
}
