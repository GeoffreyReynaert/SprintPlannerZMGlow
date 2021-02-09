using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Lokaal
    {
        public int lokaalID { get; set; }
        public string lokaalnaam { get; set; }
        public int capaciteit { get; set; }
        public string lokaaltype { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
    }
}
