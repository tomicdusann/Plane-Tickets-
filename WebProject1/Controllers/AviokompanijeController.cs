using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebProject1.Models;
using WebProject1.Repo;

namespace WebProject1.Controllers
{
    [RoutePrefix("api/Aviokompanije")]
    public class AviokompanijeController : ApiController
    {
        private RepozitorijumAviokompanija _repozitorijumAviokompanija = new RepozitorijumAviokompanija();

        // GET: api/Aviokompanije/naziv/{naziv}
        [HttpGet]
        [Route("naziv/{naziv}")]
        public IHttpActionResult GetAviokompanijaByNaziv(string naziv)
        {
            // Logovanje unesenog naziva
            System.Diagnostics.Debug.WriteLine("Primljen naziv: " + naziv);

            var aviokompanija = _repozitorijumAviokompanija.GetAllAviokompanije()
                                .FirstOrDefault(a => a.Naziv.Equals(naziv, StringComparison.OrdinalIgnoreCase));

            if (aviokompanija == null)
            {
                return NotFound();
            }

            // Kreiraj novu instancu tipa Aviokompanija i dodeli vrednosti iz postojeće
            Aviokompanija zaPrikaz = new Aviokompanija
            {
                AviokompanijaID = aviokompanija.AviokompanijaID,
                Naziv = aviokompanija.Naziv,
                Adresa = aviokompanija.Adresa,
                KontaktInformacije = aviokompanija.KontaktInformacije,
                ListaLetova = aviokompanija.ListaLetova,
                ListaRecenzija = new List<Recenzija>() // Kreiramo praznu listu za filtrirane recenzije
            };

            // Filtriranje odobrenih recenzija
            if (aviokompanija.ListaRecenzija.Any(rec => rec.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (Recenzija temp in aviokompanija.ListaRecenzija)
                {
                    if (temp.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase))
                    {
                        zaPrikaz.ListaRecenzija.Add(temp); // Dodaj samo odobrene recenzije
                    }
                }
            }

            return Ok(zaPrikaz);
        }


        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllAviokompanije()
        {
            var aviokompanije = _repozitorijumAviokompanija.GetAllAviokompanije();

            if (aviokompanije == null)
            {
                return NotFound();
            }

            return Ok(aviokompanije);
        }

        [HttpPut]
        [Route("updateKompaniju")]
        public IHttpActionResult AzurirajAviokompaniju([FromBody] AviokompanijaAzurirajModel noviPodaci)
        {
            // Učitaj postojeće aviokompanije iz JSON fajla
            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));

            // Pronađi aviokompaniju koja treba da se ažurira
            var aviokompanijaZaIzmenu = sveAviokompanije.FirstOrDefault(a => a.Naziv.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase));

            if (aviokompanijaZaIzmenu != null)
            {
                // Ažuriraj samo polja koja su promenjena
                aviokompanijaZaIzmenu.Naziv = noviPodaci.NoviNaziv ?? aviokompanijaZaIzmenu.Naziv;
                aviokompanijaZaIzmenu.Adresa = noviPodaci.Adresa ?? aviokompanijaZaIzmenu.Adresa;
                aviokompanijaZaIzmenu.KontaktInformacije = noviPodaci.KontaktInformacije ?? aviokompanijaZaIzmenu.KontaktInformacije;

                // Očuvaj postojeće liste letova i recenzija, ukoliko su oni prisutni u strukturi podataka
                //aviokompanijaZaIzmenu.ListaLetova = noviPodaci.ListaLetova ?? aviokompanijaZaIzmenu.ListaLetova;
                //aviokompanijaZaIzmenu.ListaRecenzija = noviPodaci.ListaRecenzija ?? aviokompanijaZaIzmenu.ListaRecenzija;
                foreach(Let templet in aviokompanijaZaIzmenu.ListaLetova)
                {
                    templet.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                }

                foreach(Recenzija temrez in aviokompanijaZaIzmenu.ListaRecenzija)
                {
                    temrez.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                }

                // Sačuvaj promene u JSON fajl
                File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented));


                var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
                List<Korisnik> sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));

                var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
                List<Let> sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

                var putanjaFajlaRecenzije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                List<Recenzija> sveRecenzije = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajlaRecenzije));

                var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                List<Rezervacija> sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

                //update treba uraditi za sve ove entitete

                foreach (Korisnik temp in sviKorisnici)
                {
                    List<Let> LetoviOveKompanije = new List<Let>();
                    if(temp.ListaLetova.Any(let => let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)))
                    {
                        LetoviOveKompanije = temp.ListaLetova.Where(let => let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    foreach (Let tempLet in LetoviOveKompanije)
                    {
                        tempLet.Aviokompanija = aviokompanijaZaIzmenu.Naziv.ToString();
                    }

                    List<Rezervacija> RezervacijeOveAviokompanije = new List<Rezervacija>();
                    if(temp.ListaRezervacija.Any(rez => rez.Let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)))
                    {
                        RezervacijeOveAviokompanije = temp.ListaRezervacija.Where(rez => rez.Let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    foreach(Rezervacija tempRez in RezervacijeOveAviokompanije)
                    {
                        tempRez.Let.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                    }
                }
                File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));


                List<Let> LetoviZaIzmenu = new List<Let>();
                if (sviLetovi.Any(let => let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)))
                {
                    LetoviZaIzmenu = sviLetovi.Where(let => let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)).ToList();
                }


                foreach (Let temp in LetoviZaIzmenu)
                {
                    temp.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                }
                File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));


                List<Recenzija> RecenzijeZaIzmenu = new List<Recenzija>();
                if(sveRecenzije.Any(rec => rec.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)))
                {
                    RecenzijeZaIzmenu = sveRecenzije.Where(rec => rec.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                

                foreach(Recenzija temp in RecenzijeZaIzmenu)
                {
                    temp.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                }
                File.WriteAllText(putanjaFajlaRecenzije, JsonConvert.SerializeObject(sveRecenzije, Formatting.Indented));


                List<Rezervacija> rezervacijeLetovaTeKompanije = new List<Rezervacija>();
                if(sveRezervacije.Any(rez => rez.Let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)))
                {
                    rezervacijeLetovaTeKompanije = sveRezervacije.Where(rez => rez.Let.Aviokompanija.Equals(noviPodaci.StariNaziv, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                foreach(Rezervacija temp in rezervacijeLetovaTeKompanije)
                {
                    temp.Let.Aviokompanija = aviokompanijaZaIzmenu.Naziv;
                }
                File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));

                return Ok(new { success = true, redirectUrl = "kompanijeAdmin.html" });
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("delete/{naziv}")]
        public IHttpActionResult DeleteAviokompanija(string naziv)
        {
            if (string.IsNullOrEmpty(naziv))
            {
                return BadRequest("Naziv aviokompanije nije validan.");
            }

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));
            var trazena = sveKompanije.FirstOrDefault(k => k.Naziv.Equals(naziv, StringComparison.OrdinalIgnoreCase));

            if (trazena == null)
            {
                return NotFound();
            }

            if(trazena.ListaLetova.Any(let => let.StatusLeta.ToString().Equals("Aktivan", StringComparison.OrdinalIgnoreCase)))
            {
                trazena.IsDeleted = false;
            }
            else
            {
                trazena.IsDeleted = true;
            }

            File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKompanije, Formatting.Indented));

            if (trazena.IsDeleted == true)
            {
                return Ok("Aviokompanija je logički obrisana.");
            }
            else
            {
                return BadRequest("Aviokompanija ima aktivne letove, ne moze se obrisati.");
            }
        }

        [HttpGet]
        [Route("search")]
        public IHttpActionResult PretraziAviokompanije(string naziv = null, string adresa = null, string kontaktInformacije = null)
        {
            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var filtriraneAviokompanije = sveKompanije.Where(a =>
            (string.IsNullOrEmpty(naziv) || (a.Naziv != null && a.Naziv.ToLower().Contains(naziv.ToLower()))) &&
            (string.IsNullOrEmpty(adresa) || (a.Adresa != null && a.Adresa.ToLower().Contains(adresa.ToLower()))) &&
            (string.IsNullOrEmpty(kontaktInformacije) || (a.KontaktInformacije != null && a.KontaktInformacije.ToLower().Contains(kontaktInformacije.ToLower())))
            ).ToList();


            return Ok(filtriraneAviokompanije);
        }

        [HttpPost]
        [Route("create")]
        public IHttpActionResult DodajNovuKompaniju([FromBody] AviokompanijaCreateModel noviPodaci)
        {
            if (noviPodaci == null)
            {
                return BadRequest("Podaci o aviokompaniji su nevalidni.");
            }

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var novaKompanija = new Aviokompanija
            {
                AviokompanijaID = _repozitorijumAviokompanija.GetNextId(),
                Naziv = noviPodaci.Naziv,
                Adresa = noviPodaci.Adresa,
                KontaktInformacije = noviPodaci.KontaktInformacije,
                ListaLetova = new List<Let>(),
                ListaRecenzija = new List<Recenzija>(),
                IsDeleted = false
            };

            sveKompanije.Add(novaKompanija);

            File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKompanije, Formatting.Indented));

            return Ok();
        }

    }
}
