using System;

namespace FleetManager.Models
{
    public class Kilometerallas
    {
        public int KilometerID { get; set; }

        public int GepjarmuID { get; set; }

        public DateTime Datum { get; set; }

        public int Kilometer { get; set; }

        public Gepjarmu Gepjarmu { get; set; } = null!;
    }
}