using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Dagdeel
    {
        public byte dagdeelID { get; set; }
        public string dagdeelNaam { get; set; }
        public IList<Examenrooster> Examenroosters { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
    }
}
