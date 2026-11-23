using System;

namespace FleetManager.Models
{
    public class Szervizeles
    {
        public int SzervizID { get; set; }

        public int GepjarmuID { get; set; }

        public DateTime SzervizDatum { get; set; }

        public string SzervizTipus { get; set; } = string.Empty;

        public string? Leiras { get; set; }

        public decimal Koltseg { get; set; }

        public int? Kilometerora { get; set; }

        public Gepjarmu Gepjarmu { get; set; } = null!;
    }
}