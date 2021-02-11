using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SprintPlannerZM.Model
{
    public class Examentijdspanne
    {
        [Key]
        public byte tijdspanneID { get; set; }
        public string tijdsduur { get; set; }
        public DateTime datum { get; set; }
        public IList<Examenrooster> Examenroosters { get; set; }
        public IList<Sprintlokaal> Sprintlokalen { get; set; }
    }
}
