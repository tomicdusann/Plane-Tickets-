using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public class LetIzmena
    {
        // Prvo svojstvo tipa List<string>
        public List<int> AzuriraSeCena { get; set; }

        // Drugo svojstvo tipa List<int>
        public List<int> NeAzuriraSeCena { get; set; }

        // Konstruktor
        public LetIzmena()
        {
            // Inicijalizacija listi
            AzuriraSeCena = new List<int>(1);
            NeAzuriraSeCena = new List<int>(1);
        }
    }
}