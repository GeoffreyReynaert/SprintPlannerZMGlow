using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SprintPlannerZM.Model
{
    public class Examenrooster
    {
        [Key]
        public int examenID { get; set; }
        public int vakID { get; set; }
        public byte[] examendoc { get; set; }
        public byte[] examendoc2 { get; set; }
        public string tijd { get; set; }
        public DateTime datum { get; set; }
        public Vak Vak { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}