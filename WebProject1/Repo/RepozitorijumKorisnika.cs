using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumKorisnika
    {
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\korisnici.json";

        private List<Korisnik> _korisnici;

        public RepozitorijumKorisnika()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _korisnici = JsonConvert.DeserializeObject<List<Korisnik>>(json) ?? new List<Korisnik>();
            }
            else
            {
                _korisnici = new List<Korisnik>();
            }
        }

        public IEnumerable<Korisnik> GetAllKorisnici()
        {
            return _korisnici;
        }

        public bool AddKorisnik(Korisnik noviKorisnik)
        {
            // Proveri da li korisničko ime već postoji
            if (_korisnici.Any(k => k.KorisnickoIme.Equals(noviKorisnik.KorisnickoIme, StringComparison.OrdinalIgnoreCase)))
            {
                return false; // Korisničko ime već postoji
            }

            // Dodaj novog korisnika u listu

            /*    **inicijalizacijom listi na null eskiviram error kasnije, jer ce se buniti nesto oko null greske,
                    pa da budu odmah u fajlu zapisane kao [] i [] da se zna da su prazne strukture  */
            noviKorisnik.ListaLetova = new List<Let>();
            noviKorisnik.ListaRezervacija = new List<Rezervacija>();
            
            _korisnici.Add(noviKorisnik);

            // Sačuvaj promene u JSON fajlu
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_korisnici, Formatting.Indented));

            return true; // Uspešno dodavanje korisnika
        }

        /*public Korisnik GetKorisnikKIme(string username)
        {
            return _korisnici.FirstOrDefault(k => k.KorisnickoIme.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateKorisnikRezervacije(Korisnik azuriraniKorisnik)
        { 

            Korisnik korisnikZaAzuriranje = GetKorisnikKIme(azuriraniKorisnik.KorisnickoIme);
            korisnikZaAzuriranje.ListaLetova = azuriraniKorisnik.ListaLetova ?? korisnikZaAzuriranje.ListaLetova;
            

            SaveChanges();
        }*/

        public void SaveChanges()
        {
            var json = JsonConvert.SerializeObject(_korisnici, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<Korisnik> SearchKorisnici(string ime, string prezime, DateTime? datumRodjenjaOd, DateTime? datumRodjenjaDo)
        {
            return _korisnici.Where(k =>
                (string.IsNullOrEmpty(ime) || (k.Ime != null && k.Ime.ToLower().Contains(ime.ToLower()))) &&
                (string.IsNullOrEmpty(prezime) || (k.Prezime != null && k.Prezime.ToLower().Contains(prezime.ToLower()))) &&
                (!datumRodjenjaOd.HasValue || k.DatumRodjenja >= datumRodjenjaOd.Value) &&
                (!datumRodjenjaDo.HasValue || k.DatumRodjenja <= datumRodjenjaDo.Value)
            );
        }
    }
}