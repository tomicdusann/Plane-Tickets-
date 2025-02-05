using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumAdministratora
    {
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\administratori.json";

        private List<Korisnik> _admini;

        public RepozitorijumAdministratora()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _admini = JsonConvert.DeserializeObject<List<Korisnik>>(json) ?? new List<Korisnik>();
            }
            else
            {
                _admini = new List<Korisnik>();
            }
        }

        public IEnumerable<Korisnik> GetAllKorisnici()
        {
            return _admini;
        }

        public IEnumerable<Korisnik> SearchAdministratori(string ime, string prezime, DateTime? datumRodjenjaOd, DateTime? datumRodjenjaDo)
        {
            return _admini.Where(k =>
                (string.IsNullOrEmpty(ime) || (k.Ime != null && k.Ime.ToLower().Contains(ime.ToLower()))) &&
                (string.IsNullOrEmpty(prezime) || (k.Prezime != null && k.Prezime.ToLower().Contains(prezime.ToLower()))) &&
                (!datumRodjenjaOd.HasValue || k.DatumRodjenja >= datumRodjenjaOd.Value) &&
                (!datumRodjenjaDo.HasValue || k.DatumRodjenja <= datumRodjenjaDo.Value)
            );
        }
    }
}