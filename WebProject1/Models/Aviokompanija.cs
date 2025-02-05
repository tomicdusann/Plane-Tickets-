using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public class Aviokompanija
    {
        public int AviokompanijaID { get; set; }
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public string KontaktInformacije { get; set; }
        public virtual ICollection<Let> ListaLetova { get; set; }
        public virtual ICollection<Recenzija> ListaRecenzija { get; set; }
        public bool IsDeleted { get; set; } 
    }
}