using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProject1.Models
{
    public enum Pol
    {
        Muski,
        Zenski
    }

    public enum TipKorisnika
    {
        Putnik,
        Administrator
    }

    public class Korisnik
    {
        public int KorisnikID { get; set; }
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public DateTime DatumRodjenja { get; set; }

        public string Pol
        {
            get { return PolKorisnika.ToString(); }
        }

        public Pol PolKorisnika { get; set; }

        public string TipKorisnika
        {
            get { return Tip.ToString(); }
        }
        public TipKorisnika Tip{ get; set; }
        public virtual ICollection<Rezervacija> ListaRezervacija { get; set; }

        //nadogradnja modela korisnik, treba da ima listu svojih letova
        public virtual ICollection<Let> ListaLetova { get; set; }
    }
}