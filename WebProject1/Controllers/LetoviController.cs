using Microsoft.IdentityModel.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebProject1.Models;
using WebProject1.Repo;
namespace WebProject1.Controllers
{
    [RoutePrefix("api/Letovi")]
    public class LetoviController : ApiController
    {
        private RepozitorijumLetova _repozitorijumLetova = new RepozitorijumLetova();

        // GET: api/Letovi
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetLetovi()
        {
            var aktivniLetovi = _repozitorijumLetova.GetAllAktivniLetovi(); //vraca samo aktivne letove
            return Ok(aktivniLetovi);
        }

        // GET: api/Letovi
        /*[ResponseType(typeof(Let))]
        public IHttpActionResult GetLet(int id)
        {
            Let let = _repozitorijumLetova.GetLetById(id);
            if (let == null)
            {
                return NotFound();
            }

            return Ok(let);
        }*/

        // GET: api/Letovi/Search
        [HttpGet]
        //[Route("api/Letovi/Search")]
        [Route("Search")]
        public IHttpActionResult SearchLetovi(string polaznaDestinacija, string odredisnaDestinacija, DateTime? datumPolaska, DateTime? datumDolaska, string aviokompanija)
        {
            var letovi = _repozitorijumLetova.SearchLetovi(polaznaDestinacija, odredisnaDestinacija, datumPolaska, datumDolaska, aviokompanija);
            return Ok(letovi);
        }

        // GET: api/Letovi/Sort
        [HttpGet]
        //[Route("api/Letovi/Sort")]
        [Route("Sort")]
        public IHttpActionResult SortLetovi(string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
            {
                return BadRequest("sortOrder parametar je obavezan.");
            }

            var letovi = _repozitorijumLetova.SortLetovi(sortOrder);

            if (letovi == null || !letovi.Any())
            {
                return NotFound();
            }

            return Ok(letovi);
        }

        [HttpGet]
        [Route("filtrirajAdmin")]
        public IHttpActionResult FiltrirajKorisnikeSistema(string filterOrder)
        {
            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

            IEnumerable<Let> filtrirano = null;

            switch (filterOrder)
            {
                case "Zavrsen":
                    filtrirano = sviLetovi.Where(let => let.Status.Equals("Zavrsen", StringComparison.OrdinalIgnoreCase));
                    if (filtrirano == null)
                    {
                        filtrirano = new List<Let>();
                    }
                    break;
                case "Aktivan":
                    filtrirano = sviLetovi.Where(let => let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase));
                    if (filtrirano == null)
                    {
                        filtrirano = new List<Let>();
                    }
                    break;
                case "Otkazan":
                    filtrirano = sviLetovi.Where(let => let.Status.Equals("Otkazan", StringComparison.OrdinalIgnoreCase));
                    if (filtrirano == null)
                    {
                        filtrirano = new List<Let>();
                    }
                    break;
                case "Obrisan":
                    filtrirano = sviLetovi.Where(let => let.Status.Equals("Obrisan", StringComparison.OrdinalIgnoreCase));
                    if (filtrirano == null)
                    {
                        filtrirano = new List<Let>();
                    }
                    break;
                default:
                    filtrirano = sviLetovi.Where(let => let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase));
                    if (filtrirano == null)
                    {
                        filtrirano = new List<Let>();
                    }
                    break;

            }

            return Ok(filtrirano);
        }

        [HttpGet]
        [Route("SearchAdmin")]
        public IHttpActionResult PretraziLetove(string polaznaDestinacija = null, string destinacijaDolaska = null, DateTime? datumPolaska = null, DateTime? datumDolaska = null, string statusLeta = null)
        {
            var putanjaFajlaLetova = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetova));

            var filtriraniLetovi = sviLetovi.Where(l =>
            (string.IsNullOrEmpty(polaznaDestinacija) || (l.PolaznaDestinacija != null && l.PolaznaDestinacija.ToLower().Contains(polaznaDestinacija.ToLower()))) &&
            (string.IsNullOrEmpty(destinacijaDolaska) || (l.OdredisnaDestinacija != null && l.OdredisnaDestinacija.ToLower().Contains(destinacijaDolaska.ToLower()))) &&
            (!datumPolaska.HasValue || l.DatumVremePolaska.Date == datumPolaska.Value.Date) &&
            (!datumDolaska.HasValue || l.DatumVremeDolaska.Date == datumDolaska.Value.Date)
            ).ToList();

            return Ok(filtriraniLetovi);
        }

        [HttpPost]
        [Route("dodajLet")]
        public IHttpActionResult DodajNovuKompaniju([FromBody] LetCreateModel noviPodaci)
        {
            if (noviPodaci == null)
            {
                return BadRequest("Podaci o letu su nevalidni.");
            }

            string polznaLokacija = noviPodaci.PolaznaDestinacija;

            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveKomp = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var noviLet = new Let
            {
                LetID = _repozitorijumLetova.GetNextId(),
                Aviokompanija = noviPodaci.Aviokompanija,
                PolaznaDestinacija = noviPodaci.PolaznaDestinacija,
                OdredisnaDestinacija = noviPodaci.OdredisnaDestinacija,
                DatumVremePolaska = noviPodaci.DatumVremePolaska,
                DatumVremeDolaska = noviPodaci.DatumVremeDolaska,
                BrojSlobodnihMesta = noviPodaci.BrojSlobodnihMesta,
                Cena = noviPodaci.Cena,
                BrojZauzetihMesta = 0,
                StatusLeta = StatusLeta.Aktivan
            };

            sviLetovi.Add(noviLet);//dodaje se u sisak svih letova zasebno
            Aviokompanija moraSeAzurira = null;

            if(sveKomp.Any(k => k.Naziv.Equals(noviLet.Aviokompanija, StringComparison.OrdinalIgnoreCase)))
            {
                moraSeAzurira = sveKomp.FirstOrDefault(k => k.Naziv.Equals(noviLet.Aviokompanija, StringComparison.OrdinalIgnoreCase));
                moraSeAzurira.ListaLetova.Add(noviLet);
                File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKomp, Formatting.Indented));
            }

            File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));

            return Ok();
        }

        [HttpGet] //url: 'api/Letovi/' + encodeURIComponent(letID),
        [Route("{id}")]
        public IHttpActionResult GetLetById(int id)
        {
            int ajdi = id;

            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));
            Let trazeni = new Let();

            if(sviLetovi.Any(let => let.LetID == id))
            {
                trazeni = sviLetovi.FirstOrDefault(let => (let.LetID == id && !let.Status.Equals("Obrisan", StringComparison.OrdinalIgnoreCase)));
            }

            return Ok(trazeni);
        }

        [HttpPut]
        [Route("updateLet/{id}")] //url: 'api/Letovi/updateLet/' + encodeURIComponent(letID),
        public IHttpActionResult UpdateLet(int id, [FromBody] Let updatedLet)
        {
            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));
            Let LetZaIzmenu = new Let();

            int brojSlobodnihMesta = updatedLet.BrojSlobodnihMesta;
            decimal cenaa = updatedLet.Cena;

            if(sviLetovi.Any(let => let.LetID == id))
            {
                LetZaIzmenu = sviLetovi.FirstOrDefault(let => let.LetID == id);

                LetIzmena letIzmena = new LetIzmena();

                if (LetZaIzmenu != null)
                {
                    LetZaIzmenu.LetID = id;
                    LetZaIzmenu.Aviokompanija = updatedLet.Aviokompanija ?? LetZaIzmenu.Aviokompanija;
                    LetZaIzmenu.DatumVremePolaska = (updatedLet.DatumVremePolaska != null) ? updatedLet.DatumVremePolaska : LetZaIzmenu.DatumVremePolaska;
                    LetZaIzmenu.DatumVremeDolaska = (updatedLet.DatumVremeDolaska != null) ? updatedLet.DatumVremeDolaska : LetZaIzmenu.DatumVremeDolaska;
                    //LetZaIzmenu.BrojSlobodnihMesta = (updatedLet.BrojSlobodnihMesta != null) ? updatedLet.BrojSlobodnihMesta : LetZaIzmenu.BrojSlobodnihMesta;
                    LetZaIzmenu.BrojSlobodnihMesta = updatedLet.BrojSlobodnihMesta;
                    //LetZaIzmenu.BrojZauzetihMesta = (updatedLet.BrojZauzetihMesta != null) ? updatedLet.BrojZauzetihMesta : LetZaIzmenu.BrojZauzetihMesta;
                    //LetZaIzmenu.BrojZauzetihMesta = updatedLet.BrojZauzetihMesta;

                    var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
                    var sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

                    if (sveRezervacije.Count == 0)
                    {
                        LetZaIzmenu.Cena = updatedLet.Cena;

                        foreach (Aviokompanija temp in sveAviokompanije)
                        {
                            if (temp.ListaLetova.Any(let => let.LetID == id))
                            {
                                var tajLet = temp.ListaLetova.FirstOrDefault(let => let.LetID == id);
                                tajLet.LetID = id;
                                tajLet.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                tajLet.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                tajLet.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                tajLet.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                tajLet.Cena = LetZaIzmenu.Cena;
                            }
                        }
                    }

                    bool imaLiRezervacijeZaOvajLet = sveRezervacije.Any(rez => rez.Let.LetID == id && rez.Let.Status != "Obrisan");

                    if (imaLiRezervacijeZaOvajLet)
                    {
                        List<Rezervacija> rezervacijeZaOvajLet = sveRezervacije.Where(rez => rez.Let.LetID == id && rez.Let.Status != "Obrisan").ToList();

                        if (rezervacijeZaOvajLet.Any(rez => (rez.Let.LetID == id && (rez.Status.Equals("Kreirana") || rez.Status.Equals("Odobrena")))))
                        {
                            letIzmena.NeAzuriraSeCena.Add(id);
                            List<Rezervacija> rezZaOvajLetNeMenjamoCenu = rezervacijeZaOvajLet.Where(rez => (rez.Let.LetID == id && (rez.Status.Equals("Kreirana") || rez.Status.Equals("Odobrena")))).ToList();

                            foreach (Rezervacija tempRez in rezZaOvajLetNeMenjamoCenu)
                            {
                                    tempRez.Let.LetID = id;
                                    tempRez.Let.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempRez.Let.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempRez.Let.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempRez.Let.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempRez.Let.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    //cenu necemo ni dirati
                            }
                        }
                        else
                        {
                            letIzmena.AzuriraSeCena.Add(id);
                            
                            foreach(Rezervacija tempRez in rezervacijeZaOvajLet)
                            {
                                if(tempRez.Let.LetID == id)
                                {
                                    tempRez.Let.LetID = id;
                                    tempRez.Let.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempRez.Let.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempRez.Let.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempRez.Let.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempRez.Let.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    tempRez.Let.Cena = updatedLet.Cena;
                                }
                            }
                        }

                        foreach (Korisnik tempKor in sviKorisnici)
                        {
                            foreach (Let tempLet in tempKor.ListaLetova)
                            {
                                if (letIzmena.NeAzuriraSeCena.Any(ID => ID == tempLet.LetID))
                                {
                                    tempLet.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempLet.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempLet.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempLet.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempLet.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                }
                                else if (letIzmena.AzuriraSeCena.Any(ID => ID == tempLet.LetID))
                                {
                                    tempLet.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempLet.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempLet.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempLet.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempLet.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    tempLet.Cena = updatedLet.Cena;
                                }
                            }

                            foreach (Rezervacija tempRez in tempKor.ListaRezervacija)
                            {
                                if (letIzmena.NeAzuriraSeCena.Any(ID => ID == tempRez.Let.LetID))
                                {
                                    tempRez.Let.LetID = id;
                                    tempRez.Let.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempRez.Let.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempRez.Let.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempRez.Let.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempRez.Let.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    //cenu necemo ni dirati
                                }
                                else if (letIzmena.AzuriraSeCena.Any(ID => ID == tempRez.Let.LetID))
                                {
                                    tempRez.Let.LetID = id;
                                    tempRez.Let.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempRez.Let.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempRez.Let.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempRez.Let.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempRez.Let.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    tempRez.Let.Cena = updatedLet.Cena;
                                }
                            }
                        }

                        foreach (Aviokompanija tempA in sveAviokompanije)
                        {
                            foreach (Let tempLet in tempA.ListaLetova)
                            {
                                if (letIzmena.NeAzuriraSeCena.Any(ID => ID == tempLet.LetID))
                                {
                                    tempLet.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempLet.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempLet.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempLet.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempLet.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                }
                                else if (letIzmena.AzuriraSeCena.Any(ID => ID == tempLet.LetID))
                                {
                                    tempLet.Aviokompanija = LetZaIzmenu.Aviokompanija;
                                    tempLet.DatumVremePolaska = LetZaIzmenu.DatumVremePolaska;
                                    tempLet.DatumVremeDolaska = LetZaIzmenu.DatumVremeDolaska;
                                    tempLet.BrojZauzetihMesta = LetZaIzmenu.BrojZauzetihMesta;
                                    tempLet.BrojSlobodnihMesta = LetZaIzmenu.BrojSlobodnihMesta;
                                    tempLet.Cena = updatedLet.Cena;
                                }
                            }
                        }

                    }
                    File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));
                    File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));
                    File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));
                    File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented));

                    return Ok(new { success = true, redirectUrl = "letoviAdmin.html" });
                }
            }

            return BadRequest();
        }

        [HttpDelete]
        [Route("delete/{id}")]
        public IHttpActionResult DeleteLet(int id)
        {
            var putanjaFajlaLetovi= HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

            var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
            var sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));


            var trazen = sviLetovi.FirstOrDefault(k => k.LetID == id);

            if(trazen == null)
            {
                return NotFound();
            }

            List<Rezervacija> daLiImaOdgRezervacija = new List<Rezervacija>(); //nadjemo sve rezervacije za taj let koje su otkazane i zavrsene - jer takve po uslovu mogu da se brisu
            if(sveRezervacije.Any(rez => (rez.Status.Equals("Otkazana", StringComparison.OrdinalIgnoreCase) || rez.Status.Equals("Zavrsena", StringComparison.OrdinalIgnoreCase)) && rez.Let.LetID == id))
            {
                daLiImaOdgRezervacija = sveRezervacije.Where(rez => (rez.Status.Equals("Otkazana", StringComparison.OrdinalIgnoreCase) || rez.Status.Equals("Zavrsena", StringComparison.OrdinalIgnoreCase)) && rez.Let.LetID == id).ToList();
            }

            List<Rezervacija> rezZaTajLetGeneralno = new List<Rezervacija>(); //nadjemo sve rezervacije za taj let bez obzira na status
            if(sveRezervacije.Any(rez => rez.Let.LetID == id))
            {
                rezZaTajLetGeneralno = sveRezervacije.Where(rez => rez.Let.LetID == id).ToList();
            }

            int brojRezZaLetUkupno = rezZaTajLetGeneralno.Count();
            int brojRezZaLetMSO = daLiImaOdgRezervacija.Count(); // MSO = mogu se otkazati
            

            if(brojRezZaLetUkupno - brojRezZaLetMSO == 0)
            {
                trazen.StatusLeta = StatusLeta.Obrisan;

                File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));

                Rezervacija RezSTimLetom = new Rezervacija();

                if(sveRezervacije.Any(rez => rez.Let.LetID == id))
                {
                    RezSTimLetom = sveRezervacije.FirstOrDefault(rez => rez.Let.LetID == id);
                    RezSTimLetom.Let.StatusLeta = StatusLeta.Obrisan;
                }
                
                File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));

                foreach (Aviokompanija temp in sveAviokompanije)
                {
                    foreach (Let tempLet in temp.ListaLetova)
                    {
                        if (tempLet.LetID == trazen.LetID)
                        {
                            tempLet.StatusLeta = StatusLeta.Obrisan;
                        }
                    }
                }
                File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented));


                foreach(Rezervacija temp in sveRezervacije)
                {
                    if (temp.Let.LetID == trazen.LetID)
                    {
                        temp.Let.StatusLeta = StatusLeta.Obrisan;
                    }
                }
                File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));

                foreach(Korisnik temp in sviKorisnici)
                {
                    foreach(Let tempLet in temp.ListaLetova)
                    {
                        if(tempLet.LetID == trazen.LetID)
                        {
                            tempLet.StatusLeta = StatusLeta.Obrisan;
                        }
                    }

                    foreach(Rezervacija tempRez in temp.ListaRezervacija)
                    {
                        if(tempRez.Let.LetID == trazen.LetID)
                        {
                            tempRez.Let.StatusLeta = StatusLeta.Obrisan;
                        }
                    }
                }

                return Ok();
            }

            return BadRequest("Let trenutno ne moze biti obrisan.");
        }
        
    }
}