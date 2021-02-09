using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Leerkracht
    {
        public int leerkrachtID { get; set; }
        public string voornaam { get; set; }
        public string achternaam { get; set; }
        public string email { get; set; }
        public string telefoonNr { get; set; }
        public int kluisNr { get; set; }
        public bool sprintToezichter { get; set; }
        public bool status { get; set; }
        public int rol { get; set; }
        public IList<Klas> Klassen { get; set; }
        public IList<Vak> Vakken { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
    }
}
