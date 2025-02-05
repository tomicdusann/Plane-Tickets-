using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public class LetCreateModel
    {
        public string Aviokompanija { get; set; }
        public string PolaznaDestinacija { get; set; }
        public string OdredisnaDestinacija { get; set; }   
        public DateTime DatumVremePolaska { get; set; }
        public DateTime DatumVremeDolaska { get; set; }
        public int BrojSlobodnihMesta { get; set; }
        public decimal Cena { get; set; }
    }
}