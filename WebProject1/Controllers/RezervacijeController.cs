using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebProject1.Models;
using WebProject1.Repo;

namespace WebProject1.Controllers
{
    [RoutePrefix("api/Rezervacije")]
    public class RezervacijeController : ApiController
    {
        private RepozitorijumKorisnika _repozitorijumKorisnika = new RepozitorijumKorisnika();
        private RepozitorijumLetova _repozitorijumLetova = new RepozitorijumLetova();
        private RepozitorijumRezervacija _repozitorijumRezervacija = new RepozitorijumRezervacija();
        private RepozitorijumAviokompanija _repozitorijumAviokompanija = new RepozitorijumAviokompanija();

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var rezervacije = _repozitorijumRezervacija.GetAllRezervacije();
            return Ok(rezervacije);
        }

        [HttpGet]
        [Route("KreiraneAdmin")]
        public IHttpActionResult PosaljiKreirane(string statusFilter)
        {
            string filter = statusFilter;

            var sveRez = _repozitorijumRezervacija.GetAllRezervacije();
            List<Rezervacija> kreirane = new List<Rezervacija>();    

            if(sveRez.Any(rez=>rez.Status.Equals("Kreirana", StringComparison.OrdinalIgnoreCase)))
            {
                kreirane = sveRez.Where(rez => rez.Status.Equals("Kreirana", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(kreirane);
        }

        [HttpGet]
        [Route("CekajuAdmin")]
        public IHttpActionResult PosaljiKojeCekaju(string statusFilter)
        {
            string filter = statusFilter;

            var sveRez = _repozitorijumRezervacija.GetAllRezervacije();
            List<Rezervacija> cekaju = new List<Rezervacija>();

            if (sveRez.Any(rez => rez.Status.Equals("CekaNaOtkazivanje", StringComparison.OrdinalIgnoreCase)))
            {
                cekaju = sveRez.Where(rez => rez.Status.Equals("CekaNaOtkazivanje", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(cekaju);
        }

        //ZavrseneAdmin
        [HttpGet]
        [Route("ZavrseneAdmin")]
        public IHttpActionResult PosaljiZavrsene(string statusFilter)
        {
            string filter = statusFilter;

            var sveRez = _repozitorijumRezervacija.GetAllRezervacije();
            List<Rezervacija> zavrsene = new List<Rezervacija>();

            if (sveRez.Any(rez => rez.Status.Equals("Zavrsena", StringComparison.OrdinalIgnoreCase)))
            {
                zavrsene = sveRez.Where(rez => rez.Status.Equals("Zavrsena", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(zavrsene);
        }

        [HttpGet]
        [Route("OtkazaneAdmin")]
        public IHttpActionResult PosaljiOtkazane(string statusFilter)
        {
            string filter = statusFilter;

            var sveRez = _repozitorijumRezervacija.GetAllRezervacije();
            List<Rezervacija> otkazane = new List<Rezervacija>();

            if (sveRez.Any(rez => rez.Status.Equals("Otkazana", StringComparison.OrdinalIgnoreCase)))
            {
                otkazane = sveRez.Where(rez => rez.Status.Equals("Otkazana", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(otkazane);
        }

        [HttpGet]
        [Route("OdobreneAdmin")]
        public IHttpActionResult PosaljiOdobrene(string statusFilter)
        {
            string filter = statusFilter;

            var sveRez = _repozitorijumRezervacija.GetAllRezervacije();
            List<Rezervacija> odobrene = new List<Rezervacija>();

            if (sveRez.Any(rez => rez.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase)))
            {
                odobrene = sveRez.Where(rez => rez.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(odobrene);
        }

        [HttpPost]
        [Route("kreiraj")]
        public IHttpActionResult KreirajRezervaciju([FromBody] RezervacijaFormat rezervacijaFormat)
        {
            if (rezervacijaFormat == null)
            {
                return BadRequest("Podaci o rezervaciji nisu prosleđeni.");
            }

            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));
            Let let = new Let();

            if(sviLetovi.Any(l => l.LetID == rezervacijaFormat.LetID))
            {
                let = sviLetovi.FirstOrDefault(l => l.LetID == rezervacijaFormat.LetID);

                if (rezervacijaFormat.BrojPutnika > let.BrojSlobodnihMesta)
                {
                    return BadRequest("Nema dovoljno slobodnih mesta na ovom letu.");
                }

                var rezervacija = new Rezervacija
                {
                    RezervacijaID = _repozitorijumRezervacija.GetNextId(),
                    Korisnik = trenutniKorisnik.KorisnickoIme,
                    Let = let,
                    BrojPutnika = rezervacijaFormat.BrojPutnika,
                    UkupnaCena = let.Cena * rezervacijaFormat.BrojPutnika,
                    StatusRezervacije = StatusRezervacije.Kreirana // inicijalno dobija status kreirana
                };

                rezervacija.Let.BrojSlobodnihMesta -= rezervacija.BrojPutnika;
                rezervacija.Let.BrojZauzetihMesta += rezervacija.BrojPutnika;

                var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                var sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

                sveRezervacije.Add(rezervacija);
                File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));

                // Ažuriraj broj slobodnih i zauzetih mesta na letu
                let.BrojSlobodnihMesta -= rezervacija.BrojPutnika;
                let.BrojZauzetihMesta += rezervacija.BrojPutnika;

                File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));

                var putanjaFajlaAviokompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaAviokompanije));

                foreach (Aviokompanija av in sveKompanije)
                {
                    if (av.ListaLetova.Any(l => l.LetID == let.LetID))
                    {
                        Let tajLet = av.ListaLetova.FirstOrDefault(ll => ll.LetID == let.LetID);

                        tajLet.BrojSlobodnihMesta = let.BrojSlobodnihMesta;
                        tajLet.BrojZauzetihMesta = let.BrojZauzetihMesta;
                    }
                }
                File.WriteAllText(putanjaFajlaAviokompanije, JsonConvert.SerializeObject(sveKompanije, Formatting.Indented));

                var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
                var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));
                Korisnik trenutni = null;

                if (sviKorisnici.Any(k => k.KorisnickoIme.Equals(trenutniKorisnik.KorisnickoIme)))
                {
                    trenutni = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme.Equals(trenutniKorisnik.KorisnickoIme));
                    trenutni.ListaLetova.Add(let);
                    trenutni.ListaRezervacija.Add(rezervacija);
                }

                File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));

                return Ok(rezervacija);
            }


            // Upis u repozitorijum za recenzije
            /*var recenzija = new Recenzija
            {
                RecenzijaID = _repoRecenzija.GetNextId(),
                Rezervacija = rezervacija
                // Ostali atributi...
            };
            _repoRecenzija.AddRecenzija(recenzija);*/

            return NotFound();
        }

        [HttpGet]
        [Route("MoguSeOtkazati")]
        public IHttpActionResult SpremneZaOtkazivanje()
        {
            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            var odgovarajuceRez = _repozitorijumRezervacija.GetAllZaOtkaz(trenutniKorisnik.KorisnickoIme); //vraca samo rezervacije koje mogu da se otkazu

            if (odgovarajuceRez == null)
            {
                odgovarajuceRez = new List<Rezervacija>();
            }

            return Ok(odgovarajuceRez);
        }

        [HttpPost]
        [Route("smestiZaOtkaz")]
        public IHttpActionResult OtkaziRezervaciju([FromBody] OtkazanaFormat otkazanaFormat)
        {
            // ovde samo  staljamo sve rezervacije koje se otkazuju u bazu podataka za otkazivanje, jer je admin duzan da ih otkaze a ne obican korisnik
            //tek kad admin njih prekrsti u OTAKZANA, onda se vracaju i sva podesavanja letova na staro u smislu na broj mesta i ostale promene
            if (otkazanaFormat == null)
            {
                return BadRequest("Podaci o rezervaciji nisu prosleđeni.");
            }

            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            Rezervacija otkazujeSe = new Rezervacija();
            if (trenutniKorisnik.ListaRezervacija.Any(r => r.RezervacijaID == otkazanaFormat.RezervacijaID))
            {
                otkazujeSe = trenutniKorisnik.ListaRezervacija.FirstOrDefault(r => r.RezervacijaID == otkazanaFormat.RezervacijaID);

                TimeSpan razlikaVremena = otkazujeSe.Let.DatumVremePolaska - DateTime.Now;

                if (razlikaVremena.TotalHours < 24)
                {
                    return BadRequest("Ne moze se otkazati rezervacija za let ciji polazak je za manje od 24h.");
                }

                otkazujeSe.StatusRezervacije = StatusRezervacije.CekaNaOtkazivanje;

                var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
                var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));
                var korisnikZaIzmenu = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme.Equals(trenutniKorisnik.KorisnickoIme));

                if (korisnikZaIzmenu.ListaRezervacija.Any(rez => rez.RezervacijaID == otkazujeSe.RezervacijaID))
                {
                    var RezUKorisniku = korisnikZaIzmenu.ListaRezervacija.FirstOrDefault(r => r.RezervacijaID == otkazanaFormat.RezervacijaID);
                    RezUKorisniku.StatusRezervacije = StatusRezervacije.CekaNaOtkazivanje;
                    File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));
                }


                var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                var rezervacijeSve = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajla));

                if (rezervacijeSve.Any(rez => rez.RezervacijaID == otkazujeSe.RezervacijaID))
                {
                    Rezervacija otkazujeSe1 = rezervacijeSve.FirstOrDefault(rez => rez.RezervacijaID == otkazanaFormat.RezervacijaID);
                    otkazujeSe1.StatusRezervacije = StatusRezervacije.CekaNaOtkazivanje;
                    File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(rezervacijeSve, Formatting.Indented)); // stavljamo ih u listu cekanja na o
                }

                return Ok("Rezervacija je uspešno stavljena u red za otkazivanje.");
            }
            else
            {
                return BadRequest("Nije uspesno prosledjeno administratoru.");
            }

        
        }

        [HttpPut]
        [Route("{id}/status")]
        public IHttpActionResult UpdateStatusRezervacije(int id, [FromBody] PromeniStatusRezervacijeRequest request)
        {
            var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
            var rezervacijeSve = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));
            Rezervacija trazena = new Rezervacija();

            string statusIZRequesta = request.StatusRezervacijePromena.ToString();

            if (rezervacijeSve.Any(r => r.RezervacijaID == id))
            {
                trazena = rezervacijeSve.FirstOrDefault(r => r.RezervacijaID == id);

                if (trazena.Status.Equals("Kreirana", StringComparison.OrdinalIgnoreCase) && request.StatusRezervacijePromena.ToString().Equals("Odobri", StringComparison.OrdinalIgnoreCase))
                {
                    trazena.StatusRezervacije = StatusRezervacije.Odobrena;

                    //update svega gde imamo podatke o rezervaciji
                    var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
                    var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));

                    foreach (Korisnik k in sviKorisnici)
                    {
                        if (k.ListaRezervacija.Any(rez => rez.RezervacijaID == trazena.RezervacijaID))
                        {
                            var trazenaUKorisnicima = k.ListaRezervacija.FirstOrDefault(rez => rez.RezervacijaID == trazena.RezervacijaID);
                            trazenaUKorisnicima.StatusRezervacije = StatusRezervacije.Odobrena;
                        }
                    }

                    File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented)); // stavljamo ih u listu cekanja na o
                    File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(rezervacijeSve, Formatting.Indented));
                }
                if (trazena.Status.Equals("CekaNaOtkazivanje", StringComparison.OrdinalIgnoreCase) && request.StatusRezervacijePromena.ToString().Equals("Otkazi", StringComparison.OrdinalIgnoreCase))
                {
                    TimeSpan razlikaVremena = trazena.Let.DatumVremePolaska - DateTime.Now;
                    if (razlikaVremena.TotalHours < 24)
                    {
                        return BadRequest("Rezervacija se može otkazati najkasnije 24h pre polaska leta.");
                    }

                    trazena.StatusRezervacije = StatusRezervacije.Otkazana;
                    trazena.Let.StatusLeta = StatusLeta.Otkazan;
                    trazena.Let.BrojSlobodnihMesta += trazena.BrojPutnika;
                    trazena.Let.BrojZauzetihMesta -= trazena.BrojPutnika;

                    var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
                    var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));
                    var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
                    var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));
                    var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                    var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

                    foreach (Korisnik k in sviKorisnici)
                    {
                        if (k.ListaRezervacija.Any(rez => rez.RezervacijaID == trazena.RezervacijaID))
                        {
                            var trazenaUKorisnicima = k.ListaRezervacija.FirstOrDefault(rez => rez.RezervacijaID == trazena.RezervacijaID);
                            trazenaUKorisnicima.StatusRezervacije = StatusRezervacije.Otkazana;
                            trazenaUKorisnicima.Let.BrojZauzetihMesta -= trazena.BrojPutnika;
                            trazenaUKorisnicima.Let.BrojSlobodnihMesta += trazena.BrojPutnika;
                            //ovo dodajem jer mislim da treba:
                            trazenaUKorisnicima.Let.StatusLeta = StatusLeta.Otkazan;
                        }

                        if (k.ListaLetova.Any(let => let.LetID == trazena.Let.LetID))
                        {
                            var trazeniLet = k.ListaLetova.FirstOrDefault(let => let.LetID == trazena.Let.LetID);
                            trazeniLet.BrojZauzetihMesta -= trazena.BrojPutnika;
                            trazeniLet.BrojSlobodnihMesta += trazena.BrojPutnika;
                            // i ovde isto moramo azurirati za korisnika status leta:
                            trazeniLet.StatusLeta = StatusLeta.Otkazan;
                        }
                    }

                    if (sviLetovi.Any(let => let.LetID == trazena.Let.LetID))
                    {
                        var trazeniLet = sviLetovi.FirstOrDefault(let => let.LetID == trazena.Let.LetID);
                        trazeniLet.BrojZauzetihMesta -= trazena.BrojPutnika;
                        trazeniLet.BrojSlobodnihMesta += trazena.BrojPutnika;
                    }

                    foreach (Aviokompanija av in sveKompanije)
                    {
                        if (av.ListaLetova.Any(let => let.LetID == trazena.Let.LetID))
                        {
                            var trazeniLet = av.ListaLetova.FirstOrDefault(let => let.LetID == trazena.Let.LetID);
                            {
                                trazeniLet.BrojZauzetihMesta -= trazena.BrojPutnika;
                                trazeniLet.BrojSlobodnihMesta += trazena.BrojPutnika;
                            }
                        }
                    }

                    File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented)); // stavljamo ih u listu cekanja na o
                    File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(rezervacijeSve, Formatting.Indented));
                    File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));
                    File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKompanije, Formatting.Indented));

                }
            }
            return Ok();
        }

    }
}
