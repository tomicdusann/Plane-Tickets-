using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebProject1.Models;

namespace WebProject1.Repo
{
    public class RepozitorijumRecenzija
    {
        private readonly string _filePath = "C:\\Users\\dusan\\source\\repos\\web-project-1\\WebProject1\\RepoFajlovi\\recenzije.json";

        private List<Recenzija> _recenzije;

        public RepozitorijumRecenzija()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _recenzije = JsonConvert.DeserializeObject<List<Recenzija>>(json) ?? new List<Recenzija>();
            }
            else
            {
                _recenzije = new List<Recenzija>();
            }
        }

        public IEnumerable<Recenzija> GetAllRecenzije()
        {
            return _recenzije;
        }

        public int GetNextId()
        {
            int newId;
            var rand = new Random();
            do
            {
                newId = rand.Next(10, int.MaxValue);
            } while (_recenzije.Any(r => r.RecenzijaID == newId));
            return newId;
        }
    }
}