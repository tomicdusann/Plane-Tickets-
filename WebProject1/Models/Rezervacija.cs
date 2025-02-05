using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum StatusRezervacije
    {
        Kreirana,
        Odobrena,
        Otkazana,
        Zavrsena,
        CekaNaOtkazivanje //dodatno stanje kad ceka da bude otkazana
    }

    public class Rezervacija
    {
        public int RezervacijaID { get; set; }
        public string Korisnik { get; set; } //username korisnika
        public Let Let { get; set; } 
        public int BrojPutnika { get; set; }
        public decimal UkupnaCena { get; set; }
        public string Status
        {
            get { return StatusRezervacije.ToString(); }
        }

        public StatusRezervacije StatusRezervacije { get; set; }
    }
}