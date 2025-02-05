using Newtonsoft.Json;
using System;
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
    [RoutePrefix("api/Korisnici")]
    public class KorisniciController : ApiController
    {
        private RepozitorijumKorisnika _repozitorijumKorisnika = new RepozitorijumKorisnika();
        private RepozitorijumAdministratora _repozitorijumAdministratora = new RepozitorijumAdministratora();

        [HttpGet]
        [Route("proveriKorisnickoIme/{korisnickoIme}")]
        public IHttpActionResult ProveriKorisnickoIme(string korisnickoIme)
        {
            var korisnik = _repozitorijumKorisnika.GetAllKorisnici()
                              .FirstOrDefault(k => k.KorisnickoIme == korisnickoIme);
            if (korisnik != null)
            {
                return Ok(true); // korisničko ime postoji
            }
            return Ok(false); // korisničko ime ne postoji
        }

        /*[HttpGet]
        [Route("provera/{korisnickoIme}")]
        public IHttpActionResult ProveraKorisnickogImena(string korisnickoIme)
        {
            var korisnik = _repozitorijumKorisnika.GetAllKorisnici()
                            .FirstOrDefault(k => k.KorisnickoIme.Equals(korisnickoIme, StringComparison.OrdinalIgnoreCase));
            if (korisnik != null)
            {
                return Ok(new { exists = true });
            }
            return Ok(new { exists = false });
        }*/

        [HttpPost]
        [Route("registracija")]
        public IHttpActionResult RegistrujKorisnika([FromBody] Korisnik korisnik)
        {
            if (_repozitorijumKorisnika.GetAllKorisnici().Any(k => k.KorisnickoIme == korisnik.KorisnickoIme))
            {
                return BadRequest("Korisničko ime već postoji.");
            }

            _repozitorijumKorisnika.AddKorisnik(korisnik);

            if (korisnik != null)
            {
                HttpContext.Current.Session["Korisnik"] = korisnik;
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }


        /*[HttpPost]
        [Route("prijava")]
        public IHttpActionResult PrijaviKorisnika([FromBody] Prijava prijava)
        {
            // Proverite da li korisnik postoji u bazi
            var korisnik = _repozitorijumKorisnika.GetAllKorisnici()
                .FirstOrDefault(k => k.KorisnickoIme == prijava.KorisnickoIme);

            if (korisnik == null || korisnik.Lozinka != prijava.Lozinka)
            {
                return Unauthorized(); // Ako korisnik ne postoji ili lozinka nije tačna
            }

            // Ako je prijava uspešna, možete postaviti sesiju ako želite
            HttpContext.Current.Session["Korisnik"] = korisnik;

            return Ok(); // Uspesna prijava
        }*/

        [HttpPost]
        [Route("prijava")]
        public IHttpActionResult Prijava([FromBody] Prijava model)
        {
            if (model == null)
            {
                return BadRequest("Podaci nisu prosleđeni.");
            }

            List<Korisnik> sviKorisnici;
            List<Korisnik> sviAdministratori;

            var putanjaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var putanjaAdministratori = HttpContext.Current.Server.MapPath("~/RepoFajlovi/administratori.json");

            // Učitaj korisnike i administratore
            sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaKorisnici));
            sviAdministratori = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaAdministratori));

            Korisnik korisnik = null;
            Korisnik administrator = null;

            if (model.TipKorisnika.ToString().Equals("Putnik"))
            {
                korisnik = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == model.KorisnickoIme && k.Lozinka == model.Lozinka);
                if (korisnik == null)
                {
                    return Unauthorized();
                }

                // Postavite korisnika u sesiju
                HttpContext.Current.Session["Korisnik"] = korisnik;
            }
            else if (model.TipKorisnika.ToString().Equals("Administrator"))
            {
                administrator = sviAdministratori.FirstOrDefault(a => a.KorisnickoIme == model.KorisnickoIme && a.Lozinka == model.Lozinka);
                if (administrator == null)
                {
                    return Unauthorized();
                }

                // Postavite administratora u sesiju
                HttpContext.Current.Session["Korisnik"] = administrator;
            }
            else
            {
                return BadRequest("Nepoznat tip korisnika.");
            }

            return Ok(new { success = true, redirectUrl = model.TipKorisnika.ToString().Equals("Administrator") ? "indexAdmin.html" : "indexRegular.html" });
        }

        [HttpGet]
        [Route("proveri-sesiju")]
        public IHttpActionResult ProveriSesiju()
        {
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;
            if (korisnik != null)
            {
                return Ok(korisnik);
            }
            else
            {
                return Unauthorized();
            }
        }

        /*[HttpGet]
        [Route("filtriraj")]
        public IHttpActionResult FiltrirajLetoveKorisnika(string filterOrder)
        {
            // Dohvatanje ulogovanog korisnika iz sesije
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (korisnik == null)
            {
                return Unauthorized();
            }

            string filter = filterOrder;

            // Filtriranje letova po statusu
            var filtriraniLetovi = korisnik.ListaLetova
                .Where(let => let.StatusLeta.ToString().Equals(filterOrder, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filtriraniLetovi);
        }*/

        [HttpGet]
        [Route("filtriraj")]
        public IHttpActionResult FiltrirajLetoveKorisnika(string filterOrder)
        {
            // Dohvatanje ulogovanog korisnika iz sesije
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (korisnik == null)
            {
                return Unauthorized();
            }

            var putanjaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaKorisnici));

            string filter = filterOrder;
            Korisnik trazeni = new Korisnik();
            if(sviKorisnici.Any(k => k.KorisnickoIme == korisnik.KorisnickoIme))
            {
                trazeni = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == korisnik.KorisnickoIme);
            }


            List<Let> filtrirano = new List<Let>();

            switch (filterOrder)
            {
                case "aktivan":
                    if (trazeni.ListaLetova.Any(let => let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase)))
                    {
                        filtrirano = trazeni.ListaLetova.Where(l => l.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    break;
                case "otkazan":
                    if (trazeni.ListaLetova.Any(let => let.Status.Equals("Otkazan", StringComparison.OrdinalIgnoreCase)))
                    {
                        filtrirano = trazeni.ListaLetova.Where(l => l.Status.Equals("Otkazan", StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    break;
                case "zavrsen":
                    if (trazeni.ListaLetova.Any(let => let.Status.Equals("Zavrsen", StringComparison.OrdinalIgnoreCase)))
                    {
                        filtrirano = trazeni.ListaLetova.Where(l => l.Status.Equals("Zavrsen", StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    break;
                default:
                    if (trazeni.ListaLetova.Any(let => let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase)))
                    {
                        filtrirano = trazeni.ListaLetova.Where(l => l.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    break;

            }

            return Ok(filtrirano);
        }

        [HttpGet]
        [Route("filtrirajRezervacije")]
        public IHttpActionResult FiltrirajRezervacijeKorisnika(string filterStatus)
        {
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (korisnik == null)
            {
                return Unauthorized();
            }

            string IME = korisnik.KorisnickoIme;

            string statusFilter = filterStatus;
            List<Rezervacija> filtriraneRezervacije = null;

            var putanjaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaKorisnici));

            if(sviKorisnici.Any(k => k.KorisnickoIme == korisnik.KorisnickoIme))
            {
                korisnik = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == IME);
            }

            if (korisnik.ListaRezervacija != null) 
            {
                filtriraneRezervacije = korisnik.ListaRezervacija.Where
                (r => r.StatusRezervacije.ToString().Equals(filterStatus, StringComparison.OrdinalIgnoreCase))
                .ToList();
            }
            else
            {
                filtriraneRezervacije = new List<Rezervacija>();
            }


            return Ok(filtriraneRezervacije);
        }


        [HttpGet]
        [Route("profilUcitaj")]
        public IHttpActionResult UcitajProfil()
        {
            // Dohvatanje ulogovanog korisnika iz sesije
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (korisnik == null)
            {
                return Unauthorized(); // Vraća status 401 ako korisnik nije ulogovan
            }

            // Vraćamo podatke o korisniku
            // Kreiranje anonimnog objekta sa potrebnim poljima
            var korisnickiProfil = new
            {
                korisnik.KorisnickoIme,
                korisnik.Lozinka,
                korisnik.Ime,
                korisnik.Prezime,
                korisnik.Email,
                korisnik.Pol,
                korisnik.DatumRodjenja
            };

            // Vraćanje podataka o korisniku kao odgovor na zahtev
            return Ok(korisnickiProfil);
        }

        [HttpPost]
        [Route("azurirajProfil")]
        public IHttpActionResult AzurirajProfil([FromBody] Korisnik noviPodaci)
        {
            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajla));

            string staroIme = trenutniKorisnik.KorisnickoIme;
            //trenutni korisnik je onaj korisnik koje je aktivan u aplikaciji u ovom momentu, 
            //korisnikzaIzmenu je verzija tog korisnika koja je procitana iz JSON fajla
            //ovi podaci treba da budu sinhronizovani tako da ce se bilo koja izmena u aplikaciji
            //namestiti tako da se sacuva i u fajlu cim se izvrsi u aplikaciji da bi imali sveze podatke

            /*
                kako funkcionise ?? - pa na taj nacin sto se gleda sta nije null sa leve i desne strane operatora, pa se to uzima kao izbor
                a prioritet ima leva strana, dakle ako ona nije null ona ce se uzeti svakako
             */
            /*if(sviKorisnici.Any(k => k.KorisnickoIme.Equals(noviPodaci.KorisnickoIme)))
            {
                return BadRequest("Korisnicko ime je zauzeto.");
            }*/
            

            // Samo ažuriraj polja koja su promenjena
            trenutniKorisnik.KorisnickoIme = noviPodaci.KorisnickoIme ?? trenutniKorisnik.KorisnickoIme;
            trenutniKorisnik.Lozinka = noviPodaci.Lozinka ?? trenutniKorisnik.Lozinka; // Ako se lozinka menja
            trenutniKorisnik.Ime = noviPodaci.Ime ?? trenutniKorisnik.Ime;
            trenutniKorisnik.Prezime = noviPodaci.Prezime ?? trenutniKorisnik.Prezime;
            trenutniKorisnik.Email = noviPodaci.Email ?? trenutniKorisnik.Email;
            trenutniKorisnik.DatumRodjenja = (noviPodaci.DatumRodjenja != null) ? noviPodaci.DatumRodjenja : trenutniKorisnik.DatumRodjenja;
            //trenutniKorisnik.PolKorisnika = (noviPodaci.PolKorisnika != null) ? noviPodaci.PolKorisnika : trenutniKorisnik.PolKorisnika;
            trenutniKorisnik.PolKorisnika = noviPodaci.PolKorisnika;
          
            // Sačuvaj promene u JSON fajl
            var korisnikZaIzmenu = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == staroIme);

            if (korisnikZaIzmenu != null)
            {
                // Ažuriraj samo polja koja su promenjena
                korisnikZaIzmenu.KorisnickoIme = trenutniKorisnik.KorisnickoIme;
                korisnikZaIzmenu.Ime = trenutniKorisnik.Ime;
                korisnikZaIzmenu.Prezime = trenutniKorisnik.Prezime;
                korisnikZaIzmenu.Email = trenutniKorisnik.Email;
                korisnikZaIzmenu.PolKorisnika = trenutniKorisnik.PolKorisnika;
                korisnikZaIzmenu.DatumRodjenja = trenutniKorisnik.DatumRodjenja;
                korisnikZaIzmenu.Lozinka = trenutniKorisnik.Lozinka;

                // Očuvaj postojeće liste
                korisnikZaIzmenu.ListaLetova = trenutniKorisnik.ListaLetova;
                korisnikZaIzmenu.ListaRezervacija = trenutniKorisnik.ListaRezervacija;

                foreach(Rezervacija rez in korisnikZaIzmenu.ListaRezervacija)
                {
                    rez.Korisnik = korisnikZaIzmenu.KorisnickoIme;
                }

                File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));

                var putanjaFajlaRecenzije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                var sveRecenzije = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajlaRecenzije));

                if(sveRecenzije.Any(rec => rec.Recezent.Equals(staroIme)))
                {
                    foreach(Recenzija rec in sveRecenzije)
                    {
                        if(rec.Recezent.Equals(staroIme))
                        {
                            rec.Recezent = korisnikZaIzmenu.KorisnickoIme;
                        }
                    }
                }
                File.WriteAllText(putanjaFajlaRecenzije, JsonConvert.SerializeObject(sveRecenzije, Formatting.Indented));


                var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                var sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

                if(sveRezervacije.Any(rez => rez.Korisnik.Equals(staroIme)))
                {
                    foreach(Rezervacija rec in sveRezervacije)
                    {
                        rec.Korisnik = korisnikZaIzmenu.KorisnickoIme;
                    }
                }
                File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRecenzije, Formatting.Indented));


                var putanjaFajlaAviokompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaAviokompanije));

                foreach(Aviokompanija av in sveAviokompanije)
                {
                    if(av.ListaRecenzija.Any(rec => rec.Recezent.Equals(staroIme)))
                    {
                        foreach(Recenzija rec in av.ListaRecenzija)
                        {
                            if(rec.Recezent.Equals(staroIme))
                            {
                                rec.Recezent = korisnikZaIzmenu.KorisnickoIme;
                            }
                        }
                    }
                }
                File.WriteAllText(putanjaFajlaAviokompanije, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented));
            }

            return Ok();
        }

        [HttpPost]
        [Route("azurirajProfilAdmin")]
        public IHttpActionResult AzurirajProfilAdmin([FromBody] Korisnik noviPodaci)
        {
            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            string adminStaroIme = trenutniKorisnik.KorisnickoIme;

            // Samo ažuriraj polja koja su promenjena
            trenutniKorisnik.KorisnickoIme = noviPodaci.KorisnickoIme ?? trenutniKorisnik.KorisnickoIme;
            trenutniKorisnik.Lozinka = noviPodaci.Lozinka ?? trenutniKorisnik.Lozinka; // Ako se lozinka menja
            trenutniKorisnik.Ime = noviPodaci.Ime ?? trenutniKorisnik.Ime;
            trenutniKorisnik.Prezime = noviPodaci.Prezime ?? trenutniKorisnik.Prezime;
            trenutniKorisnik.Email = noviPodaci.Email ?? trenutniKorisnik.Email;
            trenutniKorisnik.DatumRodjenja = (noviPodaci.DatumRodjenja != null) ? noviPodaci.DatumRodjenja : trenutniKorisnik.DatumRodjenja;
            //trenutniKorisnik.PolKorisnika = (noviPodaci.PolKorisnika != null) ? noviPodaci.PolKorisnika : trenutniKorisnik.PolKorisnika;
            trenutniKorisnik.PolKorisnika = noviPodaci.PolKorisnika;

            // Sačuvaj promene u JSON fajl
            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/administratori.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajla));
            Korisnik administratorZaIzmenu = null;

            if(sviKorisnici.Any(k => k.KorisnickoIme.Equals(adminStaroIme)))
            {
                administratorZaIzmenu = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == adminStaroIme);
            }

            if (administratorZaIzmenu != null)
            {
                // Ažuriraj samo polja koja su promenjena
                administratorZaIzmenu.KorisnickoIme = trenutniKorisnik.KorisnickoIme;
                administratorZaIzmenu.Ime = trenutniKorisnik.Ime;
                administratorZaIzmenu.Prezime = trenutniKorisnik.Prezime;
                administratorZaIzmenu.Email = trenutniKorisnik.Email;
                administratorZaIzmenu.PolKorisnika = trenutniKorisnik.PolKorisnika;
                administratorZaIzmenu.DatumRodjenja = trenutniKorisnik.DatumRodjenja;
                administratorZaIzmenu.Lozinka = trenutniKorisnik.Lozinka;

                // Očuvaj postojeće liste
                administratorZaIzmenu.ListaLetova = trenutniKorisnik.ListaLetova;
                administratorZaIzmenu.ListaRezervacija = trenutniKorisnik.ListaRezervacija;

                File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));
            }

            return Ok();
        }

        [HttpGet]
        [Route("filtrirajAdmin")]
        public IHttpActionResult FiltrirajKorisnikeSistema(string filterOrder)
        {
            // Dohvatanje ulogovanog korisnika iz sesije
            var korisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (korisnik == null)
            {
                return Unauthorized();
            }

            string filter = filterOrder;

            var putanjaFajlaAdmin = HttpContext.Current.Server.MapPath("~/RepoFajlovi/administratori.json");
            var sviAdministratori = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaAdmin));

            var putanjaFajlaPutnik = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviPutnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaPutnik));

            List<Korisnik> filtrirano = new List<Korisnik>();

            switch (filterOrder)
            {
                case "administratorNameSurnameRastuce":
                    filtrirano = sviAdministratori.OrderBy(a => a.Ime + a.Prezime).ToList();
                    break;
                case "administratorNameSurnameOpadajuce":
                    filtrirano = sviAdministratori.OrderByDescending(a => a.Ime + a.Prezime).ToList();
                    break;
                case "administratorDatumRastuce":
                    filtrirano = sviAdministratori.OrderBy(a => a.DatumRodjenja).ToList();
                    break;
                case "administratorDatumOpadajuce":
                    filtrirano = sviAdministratori.OrderByDescending(a => a.DatumRodjenja).ToList();
                    break;
                case "putnikNameSurnameRastuce":
                    filtrirano = sviPutnici.OrderBy(p => p.Ime + p.Prezime).ToList();
                    break;
                case "putnikNameSurnameOpadajuce":
                    filtrirano = sviPutnici.OrderByDescending(p => p.Ime + p.Prezime).ToList();
                    break;
                case "putnikDatumRastuce":
                    filtrirano = sviPutnici.OrderBy(p => p.DatumRodjenja).ToList();
                    break;
                case "putnikDatumOpadajuce":
                    filtrirano = sviPutnici.OrderByDescending(p => p.DatumRodjenja).ToList();
                    break;
                default:
                    filtrirano = sviPutnici.OrderBy(p => p.Ime + p.Prezime).ToList();
                    break;

            }

            return Ok(filtrirano);
        }

        [HttpGet]
        [Route("Search")]
        public IHttpActionResult SearchKorisnici(string ime = null, string prezime = null, DateTime? datumRodjenjaOd = null, DateTime? datumRodjenjaDo = null)
        {
            List<Korisnik> korisnici = _repozitorijumKorisnika.SearchKorisnici(ime, prezime, datumRodjenjaOd, datumRodjenjaDo).ToList();
            List<Korisnik> administratori = _repozitorijumAdministratora.SearchAdministratori(ime, prezime, datumRodjenjaOd, datumRodjenjaDo).ToList();

            List<Korisnik> rezultat = administratori.Concat(korisnici).ToList();
            return Ok(rezultat);
        }

    }
}