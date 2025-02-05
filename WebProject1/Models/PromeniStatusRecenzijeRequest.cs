using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum StatusRecenzijePromena
    {
        Odobri,
        Odbij
    }
    public class PromeniStatusRecenzijeRequest
    {
        public StatusRecenzijePromena StatusRecenzijePromena { get; set; }    
    }
}