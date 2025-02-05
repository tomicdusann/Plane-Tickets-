using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumLetova
    {
        //private readonly string _filePath = @":\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\letovi.json";
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\letovi.json";

        private List<Let> _letovi;

        public RepozitorijumLetova()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _letovi = JsonConvert.DeserializeObject<List<Let>>(json) ?? new List<Let>();
            }
            else
            {
                _letovi = new List<Let>();
            }
        }

        public IEnumerable<Let> GetAllLetovi()
        {
            return _letovi;
        }

        public IEnumerable<Let> GetAllAktivniLetovi() => _letovi.Where(l => l.StatusLeta.ToString().Equals("Aktivan", StringComparison.OrdinalIgnoreCase));
        
        public Let GetLetById(int id)
        {
            return _letovi.FirstOrDefault(l => l.LetID == id);
        }

        public IEnumerable<Let> SearchLetovi(string polaznaDestinacija, string odredisnaDestinacija, DateTime? datumPolaska, DateTime? datumDolaska, string aviokompanija)
        {
            return _letovi.Where(l =>
                l.Status.ToString().Equals("Aktivan", StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrEmpty(polaznaDestinacija) || (l.PolaznaDestinacija != null && l.PolaznaDestinacija.ToLower().Contains(polaznaDestinacija.ToLower()))) &&
                (string.IsNullOrEmpty(odredisnaDestinacija) || (l.OdredisnaDestinacija != null && l.OdredisnaDestinacija.ToLower().Contains(odredisnaDestinacija.ToLower()))) &&
                (!datumPolaska.HasValue || l.DatumVremePolaska.Date == datumPolaska.Value.Date) &&
                (!datumDolaska.HasValue || l.DatumVremeDolaska.Date == datumDolaska.Value.Date) &&
                (string.IsNullOrEmpty(aviokompanija) || (l.Aviokompanija != null && l.Aviokompanija.ToLower().Contains(aviokompanija.ToLower())))
            );
        }

        public IEnumerable<Let> SortLetovi(string sortOrder)
        {
            var sortedLetovi = _letovi.Where(l => l.StatusLeta.ToString().Equals("Aktivan", StringComparison.OrdinalIgnoreCase));

            switch (sortOrder)
            {
                case "cena_asc":
                    sortedLetovi = sortedLetovi.OrderBy(l => l.Cena);
                    break;
                case "cena_desc":
                    sortedLetovi = sortedLetovi.OrderByDescending(l => l.Cena);
                    break;
                default:
                    sortedLetovi = sortedLetovi.OrderBy(l => l.Cena);
                    break;
            }

            return sortedLetovi;
        }

        public void UpdateLet(Let noviLet)
        {
            Let staraVerzijaLeta = GetLetById(noviLet.LetID);
            /*_letovi.Remove(staraVerzijaLata);

            _letovi.Add(noviLet);*/

            if (staraVerzijaLeta != null)
            {
                // Ažurirajte svojstva stare verzije leta sa vrednostima iz nove verzije
                staraVerzijaLeta.LetID = noviLet.LetID;
                staraVerzijaLeta.Aviokompanija = noviLet.Aviokompanija;
                staraVerzijaLeta.PolaznaDestinacija = noviLet.PolaznaDestinacija;
                staraVerzijaLeta.OdredisnaDestinacija = noviLet.OdredisnaDestinacija;
                staraVerzijaLeta.DatumVremePolaska = noviLet.DatumVremePolaska;
                staraVerzijaLeta.DatumVremeDolaska = noviLet.DatumVremeDolaska;
                staraVerzijaLeta.BrojSlobodnihMesta = noviLet.BrojSlobodnihMesta;
                staraVerzijaLeta.BrojZauzetihMesta = noviLet.BrojZauzetihMesta;
                staraVerzijaLeta.Cena = noviLet.Cena;
                staraVerzijaLeta.StatusLeta = noviLet.StatusLeta;

                // Sačuvajte promene
                SaveChanges();
            }
        }

        public void SaveChanges()
        {
            var json = JsonConvert.SerializeObject(_letovi, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public int GetNextId()
        {
            int newId;
            var rand = new Random();
            do
            {
                newId = rand.Next(10, int.MaxValue);
            } while (_letovi.Any(r => r.LetID == newId));
            return newId;
        }

    }
}