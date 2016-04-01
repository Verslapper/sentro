using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Sentro.DTOs;
using Sentro.Enums;
using Sentro.Models;
using Match = Sentro.Models.Match;

namespace Sentro
{
    class Program
    {
        const int BASE_WAGER = 1;
        const int MAX_LOOPS = 1000000;
        const int CLEAR_FAVOURITE_DIFFERENCE = 40;
        const int UPSET_POTENTIAL_DIFFERENCE = 5;
        const int MINES_ALL_IN_UNTIL = 20000;
        const int TOURNEY_ALL_IN_UNTIL = 20000;

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
            var lastMatch = GetLatestMatch(cookieContainer);

            var i = 0;
            while (i++ < MAX_LOOPS)
            {
                var latestMatch = GetLatestMatch(cookieContainer);
                if (latestMatch.CompareTo(lastMatch) != 0)
                {
                    var latestStats = GetLatestStats(cookieContainer);
                    var red = latestMatch.Red.Players.First();
                    var blue = latestMatch.Blue.Players.First();
                    Console.WriteLine("{0} vs {1} in {2} tier", red.Name, blue.Name, red.Tier);
                    var mode = GetMode(cookieContainer);
                    var balance = GetBalance(cookieContainer);
                    var bet = GetBet(latestMatch, mode, BASE_WAGER, balance);
                    var betOnRed = bet.Team == latestMatch.Red;
                    var itsOn = PlaceBet(cookieContainer, betOnRed, bet.Wager, mode);
                    if (itsOn)
                    {
                        Console.WriteLine("{0}, I choose you!", betOnRed ? red.Name : blue.Name);
                        lastMatch = latestMatch;
                    }
                }

                Console.WriteLine("{0}: ResidentSleeper", DateTime.Now);
                Thread.Sleep(36000);
            }
        }

        private static bool PlaceBet(CookieContainer cookieContainer, bool betOnRed, int wager, Mode mode)
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

            return mode != Mode.Tournament ? responseString == "1" : responseString != "0"; // Matchmaking returns 1 (success) or 0 (failure). Tourneys return a random number (4-5 digits) on success so dunno.
        }

        private static Bet GetBet(Match latestMatch, Mode mode, int baseWager, int? balance)
        {
            var red = latestMatch.Red.Players.First();
            var blue = latestMatch.Blue.Players.First();
            Team betOn;
            var wager = baseWager;

            if (red.Winrate - blue.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Red;
                wager = red.Winrate - blue.Winrate > CLEAR_FAVOURITE_DIFFERENCE ? baseWager * 10 : baseWager;
            }
            else if (blue.Winrate - red.Winrate > UPSET_POTENTIAL_DIFFERENCE)
            {
                betOn = latestMatch.Blue;
                wager = blue.Winrate - red.Winrate > CLEAR_FAVOURITE_DIFFERENCE ? baseWager * 10 : baseWager;
            }
            else if (red.Meter - blue.Meter >= 500)
            {
                betOn = latestMatch.Red;
            }
            else if (blue.Meter - red.Meter >= 500)
            {
                betOn = latestMatch.Blue;
            }
            else if (red.Life - blue.Life >= 800)
            {
                betOn = latestMatch.Red;
            }
            else if (blue.Life - red.Life >= 800) 
            {
                betOn = latestMatch.Blue;
            }
            else // this bets upset by default in close matches
            {
                betOn = blue.Winrate <= red.Winrate ? latestMatch.Blue : latestMatch.Red;
                wager = baseWager * 3;
            }

            // Go conservative for exhibs
            if (mode == Mode.Exhibitions)
            {
                wager = baseWager;
            }

            if (mode == Mode.Tournament && balance.HasValue && balance.Value < TOURNEY_ALL_IN_UNTIL)
            {
                wager = balance.Value;
            }
            else if (balance.HasValue && balance.Value < MINES_ALL_IN_UNTIL)
            {
                betOn = blue.Winrate <= red.Winrate ? latestMatch.Blue : latestMatch.Red; // bet underdog
                wager = balance.Value;
            }

            return new Bet { Team = betOn, Wager = wager };
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

        private static Match GetLatestStats(CookieContainer cookieContainer)
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
                        Console.WriteLine("Couldn't get stats. Are you a non-Illuminati or something?");
                    }
                    else
                    {
                        var redCaptain = new Player
                        {
                            Name = dto.p1name.Split('/').First().Trim(),
                            Winrate = Int32.Parse(dto.p1winrate.Split('/').First().Trim()),
                            Meter = Int32.Parse(dto.p1meter.Split('/').First().Trim()),
                            TotalMatches = Int32.Parse(dto.p1totalmatches.Split('/').First().Trim()),
                            Life = Int32.Parse(dto.p1life.Split('/').First().Trim()),
                            Tier = dto.p1tier.Split('/').First().Trim(),
                            Author = dto.p1author.Split('/').First().Trim(),
                            Palette = Int32.Parse(dto.p1palette.Split('/').First().Trim()),
                        };

                        var blueCaptain = new Player
                        {
                            Name = dto.p2name.Split('/').First().Trim(),
                            Winrate = Int32.Parse(dto.p2winrate.Split('/').First().Trim()),
                            Meter = Int32.Parse(dto.p2meter.Split('/').First().Trim()),
                            TotalMatches = Int32.Parse(dto.p2totalmatches.Split('/').First().Trim()),
                            Life = Int32.Parse(dto.p2life.Split('/').First().Trim()),
                            Tier = dto.p2tier.Split('/').First().Trim(),
                            Author = dto.p2author.Split('/').First().Trim(),
                            Palette = Int32.Parse(dto.p2palette.Split('/').First().Trim()),
                        };

                        match = new Match
                        {
                            Red = new Team { Players = new List<Player> { redCaptain } },
                            Blue = new Team { Players = new List<Player> { blueCaptain } },
                        };

                        Player redSidekick = null;
                        int winrate;
                        if (Int32.TryParse(dto.p2winrate.Split('/').LastOrDefault(), out winrate))
                        {
                            // 2+ player exhibition match
                            redSidekick = new Player
                            {
                                Name = dto.p1name.Split('/').Last().Trim(),
                                Winrate = Int32.Parse(dto.p1winrate.Split('/').Last().Trim()),
                                Meter = Int32.Parse(dto.p1meter.Split('/').Last().Trim()),
                                TotalMatches = Int32.Parse(dto.p1totalmatches.Split('/').Last().Trim()),
                                Life = Int32.Parse(dto.p1life.Split('/').Last().Trim()),
                                Tier = dto.p1tier.Split('/').Last().Trim(),
                                Author = dto.p1author.Split('/').Last().Trim(),
                                Palette = Int32.Parse(dto.p1palette.Split('/').Last().Trim()),
                            };
                        }

                        Player blueSidekick = null;
                        if (Int32.TryParse(dto.p2winrate.Split('/').LastOrDefault(), out winrate))
                        {
                            // 2+ player exhibition match
                            blueSidekick = new Player
                            {
                                Name = dto.p2name.Split('/').Last().Trim(),
                                Winrate = Int32.Parse(dto.p2winrate.Split('/').Last().Trim()),
                                Meter = Int32.Parse(dto.p2meter.Split('/').Last().Trim()),
                                TotalMatches = Int32.Parse(dto.p2totalmatches.Split('/').Last().Trim()),
                                Life = Int32.Parse(dto.p2life.Split('/').Last().Trim()),
                                Tier = dto.p2tier.Split('/').Last().Trim(),
                                Author = dto.p2author.Split('/').Last().Trim(),
                                Palette = Int32.Parse(dto.p2palette.Split('/').Last().Trim()),
                            };
                        }

                        if (redSidekick != null)
                        {
                            match.Red.Players.Add(redSidekick);
                        }

                        if (blueSidekick != null)
                        {
                            match.Blue.Players.Add(blueSidekick);
                        }
                    }
                }
            }

            return match;
        }

        private static Match GetLatestMatch(CookieContainer cookieContainer)
        {
            Match match = null;
            var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/state.json");
            request.CookieContainer = cookieContainer;
            var response = (HttpWebResponse)request.GetResponse();
            using (var respStream = response.GetResponseStream())
            {
                if (respStream != null)
                {
                    var encoding = Encoding.GetEncoding("utf-8");
                    var streamReader = new StreamReader(respStream, encoding);

                    var content = streamReader.ReadToEnd();
                    var dto = JsonConvert.DeserializeObject<ModeDTO>(content);
                    if (dto == null)
                    {
                        Console.WriteLine("Couldn't get stats. Are you a non-Illuminati or something?");
                    }
                    else
                    {
                        var redCaptain = new Player
                        {
                            Name = dto.p1name.Split('/').First().Trim(),
                        };

                        var blueCaptain = new Player
                        {
                            Name = dto.p2name.Split('/').First().Trim(),
                        };

                        match = new Match
                        {
                            Red = new Team { Players = new List<Player> { redCaptain } },
                            Blue = new Team { Players = new List<Player> { blueCaptain } },
                        };

                        Player redSidekick = null;
                        if (dto.p1name.Split('/').LastOrDefault() != null)
                        {
                            // 2+ player exhibition match
                            redSidekick = new Player
                            {
                                Name = dto.p1name.Split('/').Last().Trim(),
                            };
                        }

                        Player blueSidekick = null;
                        if (dto.p2name.Split('/').LastOrDefault() != null)
                        {
                            // 2+ player exhibition match
                            blueSidekick = new Player
                            {
                                Name = dto.p2name.Split('/').Last().Trim(),
                            };
                        }

                        if (redSidekick != null)
                        {
                            match.Red.Players.Add(redSidekick);
                        }

                        if (blueSidekick != null)
                        {
                            match.Blue.Players.Add(blueSidekick);
                        }
                    }
                }
            }
            return match;
        }

        private static Mode GetMode(CookieContainer cookieContainer)
        {
            var mode = Mode.Exhibitions; // get conservative if detection goes wrong
            var request = (HttpWebRequest) WebRequest.Create("http://www.saltybet.com/state.json");
            request.CookieContainer = cookieContainer;
            var response = (HttpWebResponse) request.GetResponse();
            using (var respStream = response.GetResponseStream())
            {
                if (respStream != null)
                {
                    var encoding = Encoding.GetEncoding("utf-8");
                    var streamReader = new StreamReader(respStream, encoding);

                    var content = streamReader.ReadToEnd();
                    var dto = JsonConvert.DeserializeObject<ModeDTO>(content);
                    if (dto.remaining.Contains("until the next tournament") || dto.remaining.Contains("Matchmaking mode has been"))
                    {
                        mode = Mode.Matchmaking;
                    }
                    else if (dto.remaining.Contains("characters left in the bracket") || dto.remaining.Contains("Tournament mode has been"))
                    {
                        mode = Mode.Tournament;
                    }
                }
            }
            return mode;
        }

        private static int? GetBalance(CookieContainer cookieContainer)
        {
            int? balance = null;
            var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com");
            request.CookieContainer = cookieContainer;
            var response = (HttpWebResponse)request.GetResponse();
            using (var respStream = response.GetResponseStream())
            {
                if (respStream != null)
                {
                    var encoding = Encoding.GetEncoding("utf-8");
                    var streamReader = new StreamReader(respStream, encoding);

                    var content = streamReader.ReadToEnd();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(content);

                    var balanceNode = doc.DocumentNode.SelectSingleNode(".//*/span[@id='balance']");
                    try
                    {
                        balance = Int32.Parse(Regex.Replace(balanceNode.InnerText, "[^0-9]", ""));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Whoa, dodgy balance mate. Can't parse that.");
                    }
                }
            }
            return balance;
        }
    }
}
