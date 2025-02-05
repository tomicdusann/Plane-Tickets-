using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebProject1.Models;
using WebProject1.Repo;


namespace WebProject1.Controllers
{
    [RoutePrefix("api/Recenzije")]
    public class RecenzijeController : ApiController
    {
        private RepozitorijumRecenzija _repozitorijumRecenzija = new RepozitorijumRecenzija();
        private RepozitorijumKorisnika _repozitorijumKorisnika = new RepozitorijumKorisnika();

        [HttpGet]
        [Route("OneSaZavrsenim")]
        public IHttpActionResult OneKojeImajuZavrseneLetove()
        {
            var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

            if (trenutniKorisnik == null)
            {
                return Unauthorized();
            }

            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajla));
            var trazeniKorisnik = sviKorisnici.FirstOrDefault(k => k.KorisnickoIme == trenutniKorisnik.KorisnickoIme);
            List<Rezervacija> ZavrseniLetoviKorisnika = new List<Rezervacija>();    

            if(trazeniKorisnik.ListaRezervacija.Any(rez => rez.Let.StatusLeta.ToString().Equals("Zavrsen", StringComparison.OrdinalIgnoreCase)))
            {
                ZavrseniLetoviKorisnika = trazeniKorisnik.ListaRezervacija.Where(rez => rez.Let.StatusLeta.ToString().Equals("Zavrsen", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!ZavrseniLetoviKorisnika.Any())
            {
                AviokompanijeResult imenaTih = new AviokompanijeResult();
                return Ok(imenaTih);
            }

            var aviokompanijeResult = new AviokompanijeResult
            {
                Aviokompanije = ZavrseniLetoviKorisnika.Select(rez => rez.Let.Aviokompanija).ToList()
            };

            return Ok(aviokompanijeResult);
        }


        /*[HttpPost]
        [Route("kreiraj")]
        public IHttpActionResult KreirajRecenziju()
        {
            // Proverite da li je zahtev validan
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Nepodržan format zahteva.");
            }

            // Definišite direktorijum za privremeno čuvanje fajlova
            var uploadPath = HttpContext.Current.Server.MapPath("~/Images");
            var provider = new MultipartFormDataStreamProvider(uploadPath);

            try
            {
                // Očitajte podatke iz zahteva
                Request.Content.ReadAsMultipartAsync(provider).Wait();

                // Preuzmite podatke iz forme
                var aviokompanija = provider.FormData["Aviokompanija"];
                var naslov = provider.FormData["Naslov"];
                var sadrzaj = provider.FormData["Sadrzaj"];

                // Ako postoji slika, preuzmite je
                var slika = provider.FileData.FirstOrDefault();
                string slikaPutanja = null;
                if (slika != null)
                {
                    //vraća informacije o "Content-Disposition" zaglavlju u HTTP zahtevu. Ovo zaglavlje često sadrži metapodatke o fajlu,
                    //kao što su naziv fajla koji je poslat.
                    //slikaPutanja = slika.Headers.ContentDisposition.FileName.Trim('"');

                    try
                    {
                        slikaPutanja = Path.Combine(uploadPath, Path.GetFileName(slika.LocalFileName));
                        File.Move(slika.LocalFileName, slikaPutanja);
                        // Loguj uspesan upload
                        System.Diagnostics.Debug.WriteLine($"Slika uspešno smeštena: {slikaPutanja}");
                    }
                    catch (Exception ex)
                    {
                        // Loguj grešku
                        System.Diagnostics.Debug.WriteLine($"Greška pri smeštanju slike: {ex.Message}");
                        return InternalServerError(new Exception("Greška pri čuvanju slike.", ex));
                    }
                }

                var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;
                // Kreirajte instancu recenzije
                var recenzija = new Recenzija
                {
                    RecenzijaID = _repozitorijumRecenzija.GetNextId(),
                    Recezent = trenutniKorisnik.KorisnickoIme,
                    Aviokompanija = aviokompanija,
                    Naslov = naslov,
                    Sadrzaj = sadrzaj,
                    Slika = slikaPutanja,
                    StatusRecenzije = StatusRecenzije.Kreirana
                };

                var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));
                var kompanijaZaIzmenu = sveAviokompanije.FirstOrDefault(k => k.Naziv == recenzija.Aviokompanija);

                kompanijaZaIzmenu.ListaRecenzija.Add(recenzija);// kad budem kao admin pravio aviokompanije odmah zakucati inicijalnu vrednost liste na prazna, da se ne buni za null ako slucajno stavim za null, zato bolje prazna lista odmah
                File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented)); //azuriranje aviokompanija

                var putanjaFajlaRec = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                var sveRec = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajla));

                sveRec.Add(recenzija);
                File.WriteAllText(putanjaFajlaRec, JsonConvert.SerializeObject(sveRec, Formatting.Indented)); //azuriranje recenzija generalno

                // Sačuvajte recenziju u bazi podataka ili u JSON fajlu
                // Na primer: 
                // _recenzijeService.SacuvajRecenziju(recenzija);

                return Ok("Recenzija uspešno kreirana!");
            }
            catch (Exception ex)
            {
                // Obradite grešku
                return InternalServerError(ex);
            }
        }*/


        [HttpPost]
        [Route("kreiraj")]
        public async Task<IHttpActionResult> KrerirajRecenziju()
        {
            // Proverite da li je zahtev validan
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Nepodržan format zahteva.");
            }

            // Definišite direktorijum za privremeno čuvanje fajlova
            var uploadPath = HttpContext.Current.Server.MapPath("~/Images");
            var provider = new MultipartFormDataStreamProvider(uploadPath);

            try
            {
                // Očitajte podatke iz zahteva
                await Request.Content.ReadAsMultipartAsync(provider);

                // Preuzmite podatke iz forme
                var aviokompanija = provider.FormData["Aviokompanija"];
                var naslov = provider.FormData["Naslov"];
                var sadrzaj = provider.FormData["Sadrzaj"];

                // Ako postoji slika, preuzmite je
                var slika = provider.FileData.FirstOrDefault();
                string slikaPutanja = null;
                if (slika != null)
                {
                    slikaPutanja = slika.Headers.ContentDisposition.FileName.Trim('"');
                    // Definišite punu putanju za sliku
                    var fullPath = Path.Combine(uploadPath, slikaPutanja);

                    // Premeštanje slike iz privremenog mesta u odredište
                    File.Move(slika.LocalFileName, fullPath);

                    // Podesite putanju slike kao relativnu putanju
                    slikaPutanja = "Images/" + slikaPutanja;
                }

                var trenutniKorisnik = HttpContext.Current.Session["Korisnik"] as Korisnik;

                // Definišite putanju gde ćete sačuvati sliku
                //var fullPath = Path.Combine(uploadPath, slikaPutanja);

                // Kreirajte instancu recenzije
                //var imeFajla = Path.GetFileName(fullPath);
                Recenzija recenzija = new Recenzija
                {
                    RecenzijaID = _repozitorijumRecenzija.GetNextId(),
                    Recezent = trenutniKorisnik.KorisnickoIme,
                    Aviokompanija = aviokompanija,
                    Naslov = naslov,
                    Sadrzaj = sadrzaj,
                    Slika = slikaPutanja, // Relativna putanja
                    StatusRecenzije = StatusRecenzije.Kreirana
                };

                var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));
                Aviokompanija kompanijaZaIzmenu = new Aviokompanija();
                
                if(sveAviokompanije.Any(k => k.Naziv == recenzija.Aviokompanija))
                {
                    kompanijaZaIzmenu = sveAviokompanije.FirstOrDefault(k => k.Naziv == recenzija.Aviokompanija);
                }

                kompanijaZaIzmenu.ListaRecenzija.Add(recenzija);// kad budem kao admin pravio aviokompanije odmah zakucati inicijalnu vrednost liste na prazna, da se ne buni za null ako slucajno stavim za null, zato bolje prazna lista odmah
                File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sveAviokompanije, Formatting.Indented)); //azuriranje aviokompanija

                var putanjaFajlaRec = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                var sveRec = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajla));

                sveRec.Add(recenzija);
                File.WriteAllText(putanjaFajlaRec, JsonConvert.SerializeObject(sveRec, Formatting.Indented)); //azuriranje recenzija generalno

                return Ok("Recenzija uspešno kreirana!");
            }
            catch (Exception ex)
            {
                // Obradite grešku
                return BadRequest("Kreiranje recenzije nije uspelo.");
            }
        }

        [HttpGet]
        [Route("KreiraneAdmin")]
        public IHttpActionResult DobaviKreiraneZaAdmina()
        {
            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));
            List<Recenzija> sveKreirane = new List<Recenzija>();
            List<Recenzija> sveKreiraneUKompaniji = new List<Recenzija>();

            foreach(Aviokompanija av in sveAviokompanije)
            {
                if(av.ListaRecenzija.Any(rec => rec.Status.Equals("Kreirana", StringComparison.OrdinalIgnoreCase)))
                {
                    sveKreiraneUKompaniji = av.ListaRecenzija.Where(rec => rec.Status.Equals("Kreirana", StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (Recenzija rec in sveKreiraneUKompaniji)
                    {
                        sveKreirane.Add(rec);   
                    }
                }
            }

            return Ok(sveKreirane);
        }

        [HttpGet]
        [Route("OdbijeneAdmin")]
        public IHttpActionResult DobaviOdbijeneZaAdmina()
        {
            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));
            List<Recenzija> sveOdbijene = new List<Recenzija>();
            List<Recenzija> sveOdbijeneUKompaniji = new List<Recenzija>();

            foreach (Aviokompanija av in sveAviokompanije)
            {
                if (av.ListaRecenzija.Any(rec => rec.Status.Equals("Odbijena", StringComparison.OrdinalIgnoreCase)))
                {
                    sveOdbijeneUKompaniji = av.ListaRecenzija.Where(rec => rec.Status.Equals("Odbijena", StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (Recenzija rec in sveOdbijeneUKompaniji)
                    {
                        sveOdbijene.Add(rec);
                    }
                }
            }

            return Ok(sveOdbijene);
        }

        [HttpGet]
        [Route("OdobreneAdmin")]
        public IHttpActionResult DobaviOdobreneZaAdmina()
        {
            var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveAviokompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajla));
            List<Recenzija> sveOdobrene = new List<Recenzija>();
            List<Recenzija> sveOdobreneUKompaniji = new List<Recenzija>();

            foreach (Aviokompanija av in sveAviokompanije)
            {
                if (av.ListaRecenzija.Any(rec => rec.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase)))
                {
                    sveOdobreneUKompaniji = av.ListaRecenzija.Where(rec => rec.Status.Equals("Odobrena", StringComparison.OrdinalIgnoreCase)).ToList();

                    foreach (Recenzija rec in sveOdobreneUKompaniji)
                    {
                        sveOdobrene.Add(rec);
                    }
                }
            }

            return Ok(sveOdobrene);
        }

        [HttpPut]
        [Route("{recenzijaID}/status")]
        public IHttpActionResult UpdateStatusRecenzije(int recenzijaID, [FromBody] PromeniStatusRecenzijeRequest updatedStatus)
        {
            if(updatedStatus == null)
            {
                return BadRequest("Podaci o promeni statusa nisu uspesno prosledjeni.");
            }

            if(updatedStatus.StatusRecenzijePromena.ToString().Equals("Odobri", StringComparison.OrdinalIgnoreCase)) 
            {
                var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                var sveRecenzije = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajla));
                Recenzija trazena = new Recenzija();

                if(sveRecenzije.Any(rec => rec.RecenzijaID == recenzijaID))
                {
                    trazena = sveRecenzije.FirstOrDefault(rec => rec.RecenzijaID == recenzijaID);

                    string nazivAviokompanije = trazena.Aviokompanija;
                    trazena.StatusRecenzije = StatusRecenzije.Odobrena;
                    File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sveRecenzije, Formatting.Indented));

                    var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                    var sveKomp = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));
                    Aviokompanija trazenaKomp = new Aviokompanija();

                    if(sveKomp.Any(a => a.Naziv.Equals(nazivAviokompanije)))
                    {
                        trazenaKomp = sveKomp.FirstOrDefault(a => a.Naziv.Equals(nazivAviokompanije));

                        if (trazenaKomp.ListaRecenzija.Any(r => r.RecenzijaID == recenzijaID))
                        {
                            Recenzija trazenaUKomp = trazenaKomp.ListaRecenzija.FirstOrDefault(r => r.RecenzijaID == recenzijaID);
                            trazenaUKomp.StatusRecenzije = StatusRecenzije.Odobrena;
                            File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKomp, Formatting.Indented));
                        }
                    }
                }

                return Ok();
            }

            if (updatedStatus.StatusRecenzijePromena.ToString().Equals("Odbij", StringComparison.OrdinalIgnoreCase))
            {
                var putanjaFajla = HttpContext.Current.Server.MapPath("~/RepoFajlovi/recenzije.json");
                var sveRecenzije = JsonConvert.DeserializeObject<List<Recenzija>>(File.ReadAllText(putanjaFajla));
                Recenzija trazena = new Recenzija();

                if (sveRecenzije.Any(rec => rec.RecenzijaID == recenzijaID))
                {
                    trazena = sveRecenzije.FirstOrDefault(rec => rec.RecenzijaID == recenzijaID);

                    string nazivAviokompanije = trazena.Aviokompanija;
                    trazena.StatusRecenzije = StatusRecenzije.Odbijena;
                    File.WriteAllText(putanjaFajla, JsonConvert.SerializeObject(sveRecenzije, Formatting.Indented));

                    var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
                    var sveKomp = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));
                    Aviokompanija trazenaKomp = new Aviokompanija();

                    if (sveKomp.Any(a => a.Naziv.Equals(nazivAviokompanije)))
                    {
                        trazenaKomp = sveKomp.FirstOrDefault(a => a.Naziv.Equals(nazivAviokompanije));

                        if (trazenaKomp.ListaRecenzija.Any(r => r.RecenzijaID == recenzijaID))
                        {
                            Recenzija trazenaUKomp = trazenaKomp.ListaRecenzija.FirstOrDefault(r => r.RecenzijaID == recenzijaID);
                            trazenaUKomp.StatusRecenzije = StatusRecenzije.Odbijena;
                            File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKomp, Formatting.Indented));
                        }
                    }
                }

                return Ok();
            }

            return BadRequest("Problem pri menjanju statusa recenzije.");
        }
    }
}
