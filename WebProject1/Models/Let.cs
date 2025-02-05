using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum StatusLeta
    {
        Aktivan,
        Otkazan,
        Zavrsen,
        Obrisan
    }

    public class Let
    {
        public int LetID { get; set; }
        public string Aviokompanija { get; set; } // cuva ime aviokompanije
        public string PolaznaDestinacija { get; set; }
        public string OdredisnaDestinacija { get; set; }
        public DateTime DatumVremePolaska { get; set; }
        public DateTime DatumVremeDolaska { get; set; }
        public int BrojSlobodnihMesta { get; set; }
        public int BrojZauzetihMesta { get; set; }
        public decimal Cena { get; set; }

        public string Status
        {
            get { return StatusLeta.ToString(); }
        }

        public StatusLeta StatusLeta { get; set; }
    }
}