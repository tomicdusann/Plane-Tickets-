using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum StatusRezervacijePromena
    {
        Odobri,
        Otkazi
    }

    public class PromeniStatusRezervacijeRequest
    {
        public StatusRezervacijePromena StatusRezervacijePromena { get; set; }
    }
}