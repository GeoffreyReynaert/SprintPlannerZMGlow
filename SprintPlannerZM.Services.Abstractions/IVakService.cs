using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IVakService
    {
        Vak Get(int id);
        IList<Vak> Find();
        Vak Create(Vak vak);
        Vak Update(int id, Vak vak);
        bool Delete(int id);
    }
}
