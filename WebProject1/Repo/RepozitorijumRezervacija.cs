using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumRezervacija
    {
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\rezervacije.json";

        private List<Rezervacija> _rezervacije;

        public RepozitorijumRezervacija()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _rezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(json) ?? new List<Rezervacija>();
            }
            else
            {
                _rezervacije = new List<Rezervacija>();
            }
        }

        public IEnumerable<Rezervacija> GetAllRezervacije()
        {
            return _rezervacije;
        }

        public int GetNextId()
        {
            int newId;
            var rand = new Random();
            do
            {
                newId = rand.Next(10, int.MaxValue);
            } while (_rezervacije.Any(r => r.RezervacijaID == newId));
            return newId;
        }

        public void AddRezervacija(Rezervacija novaRezervacija)
        {
            _rezervacije.Add(novaRezervacija);
            SaveChanges();
        }

        public void SaveChanges()
        {
            var json = JsonConvert.SerializeObject(_rezervacije, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<Rezervacija> GetAllZaOtkaz(string korisnickoIme) => 
            _rezervacije.Where(r => r.Korisnik.Equals(korisnickoIme, StringComparison.OrdinalIgnoreCase) && 
            (r.StatusRezervacije.ToString().Equals("Odobrena", StringComparison.OrdinalIgnoreCase) || r.StatusRezervacije.ToString().Equals("Kreirana", StringComparison.OrdinalIgnoreCase)));
    }
}