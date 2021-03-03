using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Lokaal
    {
        public  Lokaal()
        {
            Sprintlokalen =new List<Sprintlokaal>();
        }

        public int lokaalID { get; set; }
        public string lokaalnaam { get; set; }
        public string naamafkorting { get; set; }
        public byte capaciteit { get; set; }
        public string lokaaltype { get; set; }
        public bool sprintlokaal { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
    }
}
