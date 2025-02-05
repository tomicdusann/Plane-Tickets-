using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum StatusRecenzije
    {
        Kreirana,
        Odobrena,
        Odbijena
    }

    public class Recenzija
    {
        public int RecenzijaID { get; set; }
        public string Recezent { get; set; } // neka recenzent bude string korisnickog imena
        public string Aviokompanija { get; set; }
        public string Naslov { get; set; }
        public string Sadrzaj { get; set; }
        public string Slika { get; set; } // Opciono
                                          //public StatusRecenzije Status { get; set; }

        // Ovde konvertujemo enum u string
        public string Status
        {
            get { return StatusRecenzije.ToString(); }
        }

        public StatusRecenzije StatusRecenzije { get; set; }
    }
}