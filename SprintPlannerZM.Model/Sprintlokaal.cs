using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Sprintlokaal
    {
        public int sprintlokaalID { get; set; }
        public int tijdspanneID { get; set; }
        public int dagdeelID { get; set; }
        public int lokaalID { get; set; }
        public int leerkrachtID { get; set; }
        public Lokaal Lokaal { get; set; }
        public Leerkracht Leerkracht { get; set; }
        public Dagdeel Dagdeel { get; set; }
        public Examentijdspanne Examentijdspanne { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}
