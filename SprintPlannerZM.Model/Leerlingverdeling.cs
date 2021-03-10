namespace SprintPlannerZM.Model
{
    public class Leerlingverdeling
    {
        public int leerlingverdelingID { get; set; }
        public long hulpleerlingID { get; set; }
        public int sprintlokaalID { get; set; }
        public int examenID { get; set; }
        public string reservatietype { get; set; }
        public Hulpleerling Hulpleerling { get; set; }
        public Sprintlokaalreservatie Sprintlokaalreservatie { get; set; }
        public Examenrooster Examenrooster { get; set; }
    }
}
