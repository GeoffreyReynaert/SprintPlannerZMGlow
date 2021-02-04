using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoapSSMvc.Model;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerkrachtService

    {
        Leerkracht Get(int id);
        IList<Leerkracht> Find();
        Leerkracht Create(Leerkracht leerkracht);
        Leerkracht Update(int id, Leerkracht leerkracht);
        bool Delete(int id);
    }
}
