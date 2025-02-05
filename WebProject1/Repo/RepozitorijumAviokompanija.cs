using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumAviokompanija
    {
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\aviokompanije.json";

        private List<Aviokompanija> _akomp;

        public RepozitorijumAviokompanija()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _akomp = JsonConvert.DeserializeObject<List<Aviokompanija>>(json) ?? new List<Aviokompanija>();
            }
            else
            {
                _akomp = new List<Aviokompanija>();
            }
        }

        public IEnumerable<Aviokompanija> GetAllAviokompanije()
        {
            return _akomp.Where(komp => komp.IsDeleted == false);
        }

        public Aviokompanija GetKompByNaziv(string naziv)
        {
            return _akomp.FirstOrDefault(a => a.Naziv.Equals(naziv, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateLetKompanija(Let azuriraniLet, string nazivKompanije)
        {
            Aviokompanija kompanijaZaAzuriranje = GetKompByNaziv(nazivKompanije);
            _akomp.Remove(kompanijaZaAzuriranje);// brisemo staru verziju

            Let staraVerzijaLeta = kompanijaZaAzuriranje.ListaLetova.FirstOrDefault(l => l.LetID == azuriraniLet.LetID);// pronadji bajati let


            if (staraVerzijaLeta == null) //ako ne postoji
            {
                kompanijaZaAzuriranje.ListaLetova.Add(azuriraniLet);
            }
            else
            {
                kompanijaZaAzuriranje.ListaLetova.Remove(staraVerzijaLeta);
                kompanijaZaAzuriranje.ListaLetova.Add(azuriraniLet);
            }

            _akomp.Add(kompanijaZaAzuriranje); // dodajem je ponovo nakon update-a

            SaveChanges();
        }

        public void SaveChanges()
        {
            var json = JsonConvert.SerializeObject(_akomp, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public int GetNextId()
        {
            int newId;
            var rand = new Random();
            do
            {
                newId = rand.Next(10, int.MaxValue);
            } while (_akomp.Any(r => r.AviokompanijaID == newId));
            return newId;
        }
    }
}