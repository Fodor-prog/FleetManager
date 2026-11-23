namespace FleetManager.Models
{
    public class Gepjarmu
    {
        public int GepjarmuID { get; set; }

        public int? DolgozoID { get; set; }

        public string Rendszam { get; set; } = string.Empty;

        public string Marka { get; set; } = string.Empty;

        public string Modell { get; set; } = string.Empty;

        public int? Evjarat { get; set; }

        public string? Alvazszam { get; set; }

        public string? Uzemanyag { get; set; }

        public int Kilometerora { get; set; }

        public DateTime? MuszakiLejarat { get; set; }

        public DateTime? BiztositasLejarat { get; set; }

        public string Allapot { get; set; } = "Aktív";

        public Munkavallalo? Munkavallalo { get; set; }
    }
}
