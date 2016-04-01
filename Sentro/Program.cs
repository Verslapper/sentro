using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Sentro.DTOs;
using Sentro.Models;
using Match = Sentro.Models.Match;

namespace Sentro
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialise with cookie from args (e.g. __cfduid=dd3567adc0bc7998fde01064741f2789a1456980891; _ga=GA1.2.309934278.1456987010; PHPSESSID=s73psg9qotrds7elqbisto0447)
            var cookieArg = args.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cookieArg))
            {
                Console.WriteLine("Where's your cookie brah?");
                Console.WriteLine("e.g. __cfduid=dd3567adc0bc7998fde01064741f2789a1456980891; _ga=GA1.2.309934278.1456987010; PHPSESSID=s73psg9qotrds7elqbisto0446");
                return;
            }
            var cookieContainer = CreateCookieContainer(args);
            Match lastMatch = GetLatestMatch(cookieContainer);

            const int MAX_LOOPS = 1000000;
            int i = 0;
            while (i++ < MAX_LOOPS)
            {
                var latestMatch = GetLatestMatch(cookieContainer);
                if (latestMatch.CompareTo(lastMatch) != 0)
                {
                    // PHASE 2: Return wager or strength indicator
                    var betOn = GetSideToBetOn(latestMatch);
                    var betOnRed = betOn == latestMatch.Red;
                    var itsOn = PlaceBet(cookieContainer, betOnRed);
                    if (itsOn)
                    {
                        Console.WriteLine("Let's go {0}!", betOn.Players[0].Name);
                        lastMatch = latestMatch;
                    }
                    else
                    {
                        if (i > 1)
                        {
                            Console.WriteLine("Whoops, bet didn't go through. Too quick on the trigger?");
                            Thread.Sleep(10000);
                            var secondTimesACharm = PlaceBet(cookieContainer, betOnRed);
                            Console.WriteLine("That time was {0}", secondTimesACharm ? "much better!" : "just as potato");
                        }
                    }
                }

                Console.WriteLine("{0}: ResidentSleeper", DateTime.Now);
                Thread.Sleep(50000);
            }
        }

        private static bool PlaceBet(CookieContainer cookieContainer, bool betOnRed, int wager = 1)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/ajax_place_bet.php");
            request.CookieContainer = cookieContainer;

            var postData = "selectedplayer=player" + (betOnRed ? 1 : 2);
            postData += "&wager=" + wager;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString == "1"; // Salty success code
        }

        private static Team GetSideToBetOn(Match latestMatch)
        {
            var red = latestMatch.Red.Players.First();
            var blue = latestMatch.Blue.Players.First();

            // PHASE 2: If within a few %, check meter + life, bet underdog
            if (red.Winrate > blue.Winrate)
            {
                return latestMatch.Red;
            }
            if (blue.Winrate > red.Winrate)
            {
                return latestMatch.Blue;
            }

            if (red.Life > blue.Life)
            {
                return latestMatch.Red;
            }
            return latestMatch.Blue;
        }

        private static CookieContainer CreateCookieContainer(IEnumerable<string> cookieArgs)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookieCollectionPart in cookieArgs)
            {
                var cookieParts = cookieCollectionPart.Replace(";","").Split('=');
                if (cookieParts.Length != 2)
                {
                    Console.WriteLine("Missing a cookie key/value");
                }

                var target = new Uri("http://www.saltybet.com/");
                var cookie = new Cookie(cookieParts[0], cookieParts[1]) { Domain = target.Host };
                cookieContainer.Add(cookie);
            }
            return cookieContainer;
        }

        private static Match GetLatestMatch(CookieContainer cookieContainer)
        {
            Match match = null;
            var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/ajax_get_stats.php");
            request.CookieContainer = cookieContainer;
            var response = (HttpWebResponse) request.GetResponse();
            using (var respStream = response.GetResponseStream())
            {
                if (respStream != null)
                {
                    var encoding = Encoding.GetEncoding("utf-8");
                    var streamReader = new StreamReader(respStream, encoding);

                    var content = streamReader.ReadToEnd();
                    var dto = JsonConvert.DeserializeObject<MatchDTO>(content);
                    if (dto == null)
                    {
                        Console.WriteLine("Different DTO might mean bets aren't open");
                    }
                    else
                    {
                        match = new Match
                        {
                            Red = new Team
                            {
                                Players = new List<Player>
                                {
                                    new Player
                                    {
                                        Name = dto.p1name,
                                        Winrate = dto.p1winrate,
                                        Meter = dto.p1meter,
                                        TotalMatches = dto.p1totalmatches,
                                        Life = dto.p1life,
                                        Tier = dto.p1tier,
                                        Author = dto.p1author,
                                        Palette = dto.p1palette,
                                    }
                                }
                            },
                            Blue = new Team
                            {
                                Players = new List<Player>
                                {
                                    new Player
                                    {
                                        Name = dto.p2name,
                                        Winrate = dto.p2winrate,
                                        Meter = dto.p2meter,
                                        TotalMatches = dto.p2totalmatches,
                                        Life = dto.p2life,
                                        Tier = dto.p2tier,
                                        Author = dto.p2author,
                                        Palette = dto.p2palette,
                                    }
                                }
                            }
                        };
                    }
                }
            }

            return match;
        }
    }
}
