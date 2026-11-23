namespace FleetManager.Models
{
    public class Munkavallalo
    {
        public int DolgozoID { get; set; }

        public string Nev { get; set; } = string.Empty;

        public string? Reszleg { get; set; }

        public string? Email { get; set; }

        public string? Telefonszam { get; set; }
    }
}