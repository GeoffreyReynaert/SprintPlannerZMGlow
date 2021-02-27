﻿using System;
using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Sprintlokaal
    {
        public int sprintlokaalID { get; set; }
        public string tijd { get; set; }
        public DateTime datum { get; set; }
        public int lokaalID { get; set; }
        public long? leerkrachtID { get; set; }
        public int examenID { get; set; }
        public Lokaal Lokaal { get; set; }
        public Examenrooster Examen { get; set; }
        public Leerkracht Leerkracht { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}
