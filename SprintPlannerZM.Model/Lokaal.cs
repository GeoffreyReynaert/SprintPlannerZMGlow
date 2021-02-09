using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Lokaal
    {
        public int lokaalID { get; set; }
        public string lokaalnaam { get; set; }
        public byte capaciteit { get; set; }
        public string lokaaltype { get; set; }
        public byte sprintlokaal { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; } 

    }
}
