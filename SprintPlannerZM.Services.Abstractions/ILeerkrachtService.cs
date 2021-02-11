﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerkrachtService

    {
        Leerkracht Get(long id);
        Leerkracht GetByKlasId(int id);
        IList<Leerkracht> Find();
        Leerkracht Create(Leerkracht leerkracht);
        Leerkracht Update(int id, Leerkracht leerkracht);
        bool Delete(int id);
    }
}
