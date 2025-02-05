using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public class Prijava
    {
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }

        public TipKorisnika TipKorisnika { get; set; }
    }
}