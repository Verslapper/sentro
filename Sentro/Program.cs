using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Match = Sentro.Models.Match;

namespace Sentro
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialise with cookie from args (e.g. __cfduid=dd3567adc0bc7998fde01064741f2789a1456980891; _ga=GA1.2.309934278.1456987010; PHPSESSID=s73psg9qotrds7elqbisto0447)
            var cookie = args.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cookie))
            {
                Console.WriteLine("Where's your cookie brah?");
                return;
            }
            //var cookie = CreateCookie(cookieString);
            var lastStats = GetLatestStats(cookie);

            while (true)
            {
                // get latest stats
                var latestStats = GetLatestStats(cookie);
                if (latestStats != lastStats) // and are bets open etc
                {
                    Console.WriteLine("pick side to bet on");
                    // place bet
                    lastStats = latestStats;
                }

                Thread.Sleep(5000);
            }
        }

        private static Match GetLatestStats(string cookie)
        {
            var url = string.Format("http://www.saltybet.com/ajax_get_stats.php");
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Headers["Cookie"] = cookie;
            //var cookieContainer = new CookieContainer();
            //cookieContainer.Add();
            //request.CookieContainer = new CookieContainer().Add(cookie);
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            if (response.ContentLength <= 0)
            {
                Console.WriteLine("Crap");
            }
            using (Stream respStream = response.GetResponseStream())
            {
                if (respStream != null)
                {
                    var encoding = Encoding.GetEncoding("utf-8");
                    var streamReader = new StreamReader(respStream, encoding);

                    var content = streamReader.ReadToEnd();
                    // decode
                    // serialize JSON to MatchDTO
                    // convert MatchDTO to Match
                }
            }

            return new Match();
        }
    }
}
