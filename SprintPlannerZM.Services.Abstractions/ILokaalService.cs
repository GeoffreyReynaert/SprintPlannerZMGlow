﻿using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILokaalService
    {
        Lokaal Get(int id);
        Lokaal GetByName(string lokaalnaam);
        IList<Lokaal> Find();
        IList<Lokaal> FindForSprint();
        Lokaal Create(Lokaal lokaal);
        Lokaal Update(int id, Lokaal lokaal);
        bool Delete(int id);
    }
}
