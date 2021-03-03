using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Leerkracht
    {
        public Leerkracht()
        {
            Klassen = new List<Klas>();
            Vakken = new List<Vak>();
            Sprintlokalen = new List<Sprintlokaal>();
        }
        public long leerkrachtID { get; set; }
        public string voornaam { get; set; }
        public string achternaam { get; set; }
        public string email { get; set; }
        public string telefoonNr { get; set; }
        public short kluisNr { get; set; }
        public bool sprintToezichter { get; set; }
        public bool status { get; set; }
        public byte rol { get; set; }
        public IList<Klas> Klassen { get; set; }
        public IList<Vak> Vakken { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Leerkracht objAsPart = obj as Leerkracht;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return (int)leerkrachtID;
        }

        public bool Equals(Leerkracht other)
        {
            if (other == null) return false;
            return (this.achternaam.Equals(other.achternaam) && (this.voornaam.Equals(other.voornaam)));
        }

    }
}
