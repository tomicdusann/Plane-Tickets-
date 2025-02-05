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

namespace WebProject1.Controllers
{
    public class LetoviStatusController : ApiController
    {
        [HttpPost]
        [Route("api/LetoviStatus/AzurirajStatuse")]
        public IHttpActionResult AzurirajStatuse()
        {
            var putanjaFajlaLetovi = HttpContext.Current.Server.MapPath("~/RepoFajlovi/letovi.json");
            var sviLetovi = JsonConvert.DeserializeObject<List<Let>>(File.ReadAllText(putanjaFajlaLetovi));

            var putanjaFajlaKompanije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/aviokompanije.json");
            var sveKompanije = JsonConvert.DeserializeObject<List<Aviokompanija>>(File.ReadAllText(putanjaFajlaKompanije));

            var putanjaFajlaKorisnici = HttpContext.Current.Server.MapPath("~/RepoFajlovi/korisnici.json");
            var sviKorisnici = JsonConvert.DeserializeObject<List<Korisnik>>(File.ReadAllText(putanjaFajlaKorisnici));

            var putanjaFajlaRezervacije = HttpContext.Current.Server.MapPath("~/RepoFajlovi/rezervacije.json");
            var sveRezervacije = JsonConvert.DeserializeObject<List<Rezervacija>>(File.ReadAllText(putanjaFajlaRezervacije));

            // Trenutni datum i vreme
            var trenutnoVreme = DateTime.Now;

            // Ažuriranje statusa letova
            foreach (Let temp in sviLetovi)
            {
                if(temp.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase) && trenutnoVreme > temp.DatumVremeDolaska)
                {
                    temp.StatusLeta = StatusLeta.Zavrsen;
                }
            }
            // Sačuvaj izmene u JSON fajl
            File.WriteAllText(putanjaFajlaLetovi, JsonConvert.SerializeObject(sviLetovi, Formatting.Indented));

            foreach(Rezervacija temp in sveRezervacije)
            {
                if(temp.Let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase) && trenutnoVreme > temp.Let.DatumVremeDolaska)
                {
                    temp.Let.StatusLeta = StatusLeta.Zavrsen;
                    temp.StatusRezervacije = StatusRezervacije.Zavrsena;
                }
            }
            File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));


            foreach(Korisnik temp in sviKorisnici)
            {
                foreach(Let tempLet in temp.ListaLetova)
                {
                    if(tempLet.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase) && trenutnoVreme > tempLet.DatumVremeDolaska)
                    {
                        tempLet.StatusLeta = StatusLeta.Zavrsen;
                    }
                }

                foreach (Rezervacija tempRez in temp.ListaRezervacija)
                {
                    if(tempRez.Let.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase) && trenutnoVreme > tempRez.Let.DatumVremeDolaska)
                    {
                        tempRez.Let.StatusLeta = StatusLeta.Zavrsen;
                        tempRez.StatusRezervacije = StatusRezervacije.Zavrsena;
                    }
                }
            }
            File.WriteAllText(putanjaFajlaKorisnici, JsonConvert.SerializeObject(sviKorisnici, Formatting.Indented));

            foreach(Aviokompanija temp in sveKompanije)
            {
                foreach(Let tempLet in temp.ListaLetova)
                {
                    if(tempLet.Status.Equals("Aktivan", StringComparison.OrdinalIgnoreCase) && trenutnoVreme > tempLet.DatumVremeDolaska)
                    {
                        tempLet.StatusLeta = StatusLeta.Zavrsen;
                    }
                }
            }
            File.WriteAllText(putanjaFajlaKompanije, JsonConvert.SerializeObject(sveKompanije, Formatting.Indented));

            /*foreach(Rezervacija reztemp in sveRezervacije)
            {
                if(reztemp.Let.Status.Equals("Zavrsen", StringComparison.OrdinalIgnoreCase))
                {
                    reztemp.StatusRezervacije = StatusRezervacije.Zavrsena;
                }
            }

            File.WriteAllText(putanjaFajlaRezervacije, JsonConvert.SerializeObject(sveRezervacije, Formatting.Indented));*/

            return Ok("Statusi letova su uspešno ažurirani.");
        }
    }
}
