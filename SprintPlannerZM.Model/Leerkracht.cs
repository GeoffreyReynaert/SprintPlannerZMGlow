using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Leerkracht
    {
        public long leerkrachtID { get; set; }
        public string voornaam { get; set; }
        public string achternaam { get; set; }
        public string email { get; set; }
        public string telefoonNr { get; set; }
        public int kluisnr { get; set; }
        public bool sprinttoezichter { get; set; }
        public bool status { get; set; }
        public int rol { get; set; }
        public IList<Klas> Klassen { get; set; }
        public IList<Vak> Vakken { get; set; }
    }
}
