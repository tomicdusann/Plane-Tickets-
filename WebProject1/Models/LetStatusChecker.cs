using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;

namespace WebProject1.Models
{
    public class LetStatusChecker
    {
        private static Timer _timer;

        public static void Start()
        {
            // Postavljanje intervala za periodično ažuriranje (npr. svakih 60 minuta)
            _timer = new Timer(120000);
            _timer.Elapsed += async (sender, e) => await UpdateFlightStatusesAsync();
            _timer.Start();

            // Pozivanje metode odmah pri pokretanju aplikacije
            Task.Run(async () => await UpdateFlightStatusesAsync());
        }

        private static async Task UpdateFlightStatusesAsync()
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.PostAsync("http://localhost:62378/api/LetoviStatus/AzurirajStatuse", null);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Statusi letova su uspešno ažurirani.");
                    }
                    else
                    {
                        Console.WriteLine("Greška prilikom ažuriranja statusa letova.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Greška: " + ex.Message);
            }
        }

        public static void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
        }
    }
}