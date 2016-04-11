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
using Sentro.Services;
using Match = Sentro.Models.Match;
using System.Configuration;
using Sentro.DTOs.Chat;

namespace Sentro
{
    class Program
    {
        private static readonly StreakService _streakService = new StreakService();
        private static readonly NameBasedRecommendationService _nameBasedRecommendationService = new NameBasedRecommendationService();
        private static readonly StatBasedRecommendationService _statBasedRecommendationService = new StatBasedRecommendationService(_streakService);
        private static readonly BetModifierService _betModifierService = new BetModifierService();

        static void Main(string[] args)
        {
            var cookieArg = args.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cookieArg))
            {
                Console.WriteLine("Where's your cookie brah?");
                Console.WriteLine("e.g. __cfduid=dd3567adc0bc7998fde01064741f2789a1456980891; _ga=GA1.2.309934278.1456987010; PHPSESSID=s73psg9qotrds7elqbisto0446");
                return;
            }
            var cookieContainer = CreateCookieContainer(args);
            var lastMatch = GetLatestStats(cookieContainer) ?? GetLatestMatch(cookieContainer);
            var lastStreak = string.Empty;

            int BASE_WAGER;
            if (!int.TryParse(ConfigurationManager.AppSettings["baseWager"], out BASE_WAGER))
            {
                BASE_WAGER = 1;
            }
            
            while (true)
            {
                lastStreak = GetChatHighlights(lastStreak, lastMatch);

                var latestMatch = GetLatestStats(cookieContainer) ?? GetLatestMatch(cookieContainer); // REFACTOR: Have illum/non-illum in config + strategy pattern
                if (latestMatch.CompareTo(lastMatch) != 0)
                {
                    var mode = GetMode(cookieContainer);
                    var bet = GetBet(latestMatch, mode, BASE_WAGER, GetBalance(cookieContainer));
                    var betOnRed = bet.Team == latestMatch.Red;
                    var itsOn = PlaceBet(cookieContainer, betOnRed, bet.Wager, mode);
                    if (itsOn)
                    {
                        Console.WriteLine("{0}, I choose you!", betOnRed ? latestMatch.Red.Players.First().Name : latestMatch.Blue.Players.First().Name);
                        lastMatch = latestMatch;
                    }
                }
                
                Thread.Sleep(25000);
            }
        }

        private static string GetChatHighlights(string lastStreak, Match match)
        {
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.betterttv.net/2/channels/saltybet/history");
                var response = (HttpWebResponse)request.GetResponse();
                using (var respStream = response.GetResponseStream())
                {
                    if (respStream != null)
                    {
                        var encoding = Encoding.GetEncoding("utf-8");
                        var streamReader = new StreamReader(respStream, encoding);
                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting streak / chat history. {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
            }
            
            var dto = JsonConvert.DeserializeObject<ChatHistoryDTO>(content);
            if (dto == null)
            {
                Console.WriteLine("Couldn't parse streak / chat history");
            }
            else
            {
                //Team JaredKun wins! Payouts to Team Blue. 13 exhibition matches left!
                //Bets are locked. Zelos(you-ki_ai) (-1) - $297,013, Deathscythe (2) - $914,811
                var winMessages = dto.Messages.Where(m => m.User.Name == "waifu4u" && m.Message.Contains("wins!"));
                var streakMessages = dto.Messages.Where(m => m.User.Name == "waifu4u" && m.Message.Contains("are locked"));
                foreach (var message in winMessages)
                {
                    Console.WriteLine("{0}", message.Message);
                }

                foreach (var message in streakMessages)
                {
                    if (message.Message != lastStreak)
                    {
                        Console.WriteLine("{0}", message.Message);
                        lastStreak = message.Message;
                        var splits = message.Message.Split(new string[] { ") - $" }, StringSplitOptions.RemoveEmptyEntries);
                        if (splits.Length > 1)
                        {
                            var redStreak = splits[0].Split('(').Last();
                            var blueStreak = splits[1].Split('(').Last();
                            var redSubset = splits[0].Replace("Bets are locked. ", "");
                            var redName = redSubset.Substring(0, redSubset.LastIndexOf('(') - 1);
                            if (redName == match.Red.Players.First().Name) // confirmed that waifu4u message refers to last round of betting
                            {
                                _streakService.Save(new PlayerStreak { Player = match.Red.Players.First(), Streak = int.Parse(redStreak) });
                                _streakService.Save(new PlayerStreak { Player = match.Blue.Players.First(), Streak = int.Parse(blueStreak) });
                            }
                        }
                    }
                }
            }

            return lastStreak;
        }

        private static bool PlaceBet(CookieContainer cookieContainer, bool betOnRed, int wager, Mode mode)
        {
            try
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

                // Matchmaking returns 1 (success) or 0 (failure). Tourneys return a random number (4-5 digits) on success so dunno.
                if (mode == Mode.Tournament)
                {
                    Console.WriteLine(responseString);
                }
                return mode != Mode.Tournament ? responseString == "1" : responseString.Length < 6; 
            }
            catch (Exception e)
            {
                Console.WriteLine("Error placing bet. {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
                return false;
            }
        }

        private static Bet GetBet(Match latestMatch, Mode mode, int baseWager, int? balance)
        {
            var red = latestMatch.Red.Players.First();
            var blue = latestMatch.Blue.Players.First();
            Console.WriteLine("{0} {1}% vs {2} {3}%", red.Name, red.Winrate, blue.Name, blue.Winrate);
            var bet = new Bet { Wager = baseWager};

            if (red.Winrate == 0 && blue.Winrate == 0)
            {
                // Name analysis (non-illuminati)
                bet.Team = _nameBasedRecommendationService.GetTextOnlyRecommendedBet(latestMatch);
            }
            else
            {
                // Stats analysis (Illuminati)
                bet = _statBasedRecommendationService.GetRecommendedBet(latestMatch, baseWager, balance);
            }

            Console.WriteLine("Standard algorithm says bet {0}", bet.Team.Players.First().Name);

            return _betModifierService.ApplySituation(bet, mode, balance, latestMatch);
        }

        private static CookieContainer CreateCookieContainer(IEnumerable<string> cookieArgs)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookieCollectionPart in cookieArgs)
            {
                var cookieParts = cookieCollectionPart.Replace(";","").Split('=');
                var target = new Uri("http://www.saltybet.com/");
                var cookie = new Cookie(cookieParts[0], cookieParts[1]) { Domain = target.Host };
                cookieContainer.Add(cookie);
            }
            return cookieContainer;
        }

        private static Match GetLatestStats(CookieContainer cookieContainer)
        {
            Match match = null;
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/ajax_get_stats.php");
                request.CookieContainer = cookieContainer;
                var response = (HttpWebResponse) request.GetResponse();
                using (var respStream = response.GetResponseStream())
                {
                    if (respStream != null)
                    {
                        var encoding = Encoding.GetEncoding("utf-8");
                        var streamReader = new StreamReader(respStream, encoding);
                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error around GetLatestStats request {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var dto = JsonConvert.DeserializeObject<MatchDTO>(content);
                if (dto == null)
                {
                    Console.WriteLine("Couldn't get stats. Non-illuminati mode engage!");
                }
                else
                {
                    var redCaptain = new Player
                    {
                        Name = dto.p1name.Split('/').First().Trim(),
                        Winrate = int.Parse(dto.p1winrate.Split('/').First().Trim()),
                        Meter = int.Parse(dto.p1meter.Split('/').First().Trim()),
                        TotalMatches = int.Parse(dto.p1totalmatches.Split('/').First().Trim()),
                        Life = int.Parse(dto.p1life.Split('/').First().Trim()),
                        Tier = !string.IsNullOrWhiteSpace(dto.p1tier.Split('/').First().Trim()) ? (Tier)Enum.Parse(typeof(Tier), dto.p1tier.Split('/').First().Trim()) : Tier.Unknown,
                        Author = dto.p1author.Split('/').First().Trim(),
                        Palette = int.Parse(dto.p1palette.Split('/').First().Trim()),
                    };

                    var blueCaptain = new Player
                    {
                        Name = dto.p2name.Split('/').First().Trim(),
                        Winrate = int.Parse(dto.p2winrate.Split('/').First().Trim()),
                        Meter = int.Parse(dto.p2meter.Split('/').First().Trim()),
                        TotalMatches = int.Parse(dto.p2totalmatches.Split('/').First().Trim()),
                        Life = int.Parse(dto.p2life.Split('/').First().Trim()),
                        Tier = !string.IsNullOrWhiteSpace(dto.p2tier.Split('/').First().Trim()) ? (Tier)Enum.Parse(typeof(Tier), dto.p2tier.Split('/').First().Trim()) : Tier.Unknown,
                        Author = dto.p2author.Split('/').First().Trim(),
                        Palette = int.Parse(dto.p2palette.Split('/').First().Trim()),
                    };

                    match = new Match
                    {
                        Red = new Team {Players = new List<Player> {redCaptain}},
                        Blue = new Team {Players = new List<Player> {blueCaptain}},
                    };

                    Player redSidekick = null;
                    if (dto.p1winrate.Split('/').Length == 2)
                    {
                        // 2+ player exhibition match
                        redSidekick = new Player
                        {
                            Name = dto.p1name.Split('/').Last().Trim(),
                            Winrate = int.Parse(dto.p1winrate.Split('/').Last().Trim()),
                            Meter = int.Parse(dto.p1meter.Split('/').Last().Trim()),
                            TotalMatches = int.Parse(dto.p1totalmatches.Split('/').Last().Trim()),
                            Life = int.Parse(dto.p1life.Split('/').Last().Trim()),
                            Tier = !string.IsNullOrWhiteSpace(dto.p1tier.Split('/').Last().Trim()) ? (Tier)Enum.Parse(typeof(Tier), dto.p1tier.Split('/').Last().Trim()) : Tier.Unknown,
                            Author = dto.p1author.Split('/').Last().Trim(),
                            Palette = int.Parse(dto.p1palette.Split('/').Last().Trim()),
                        };
                    }

                    Player blueSidekick = null;
                    if (dto.p2winrate.Split('/').Length == 2)
                    {
                        // 2+ player exhibition match
                        blueSidekick = new Player
                        {
                            Name = dto.p2name.Split('/').Last().Trim(),
                            Winrate = int.Parse(dto.p2winrate.Split('/').Last().Trim()),
                            Meter = int.Parse(dto.p2meter.Split('/').Last().Trim()),
                            TotalMatches = int.Parse(dto.p2totalmatches.Split('/').Last().Trim()),
                            Life = int.Parse(dto.p2life.Split('/').Last().Trim()),
                            Tier = !string.IsNullOrWhiteSpace(dto.p2tier.Split('/').Last().Trim()) ? (Tier)Enum.Parse(typeof(Tier), dto.p2tier.Split('/').Last().Trim()) : Tier.Unknown,
                            Author = dto.p2author.Split('/').Last().Trim(),
                            Palette = int.Parse(dto.p2palette.Split('/').Last().Trim()),
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

            return match;
        }

        private static Match GetLatestMatch(CookieContainer cookieContainer)
        {
            Match match = null;
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/state.json");
                request.CookieContainer = cookieContainer;
                var response = (HttpWebResponse)request.GetResponse();
                using (var respStream = response.GetResponseStream())
                {
                    if (respStream != null)
                    {
                        var encoding = Encoding.GetEncoding("utf-8");
                        var streamReader = new StreamReader(respStream, encoding);

                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                // swallow rather than crash. We'll try again next time.
                Console.WriteLine("Error around GetLatestMatch requests {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
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
                        Red = new Team {Players = new List<Player> {redCaptain}},
                        Blue = new Team {Players = new List<Player> {blueCaptain}},
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
            return match;
        }

        private static Mode GetMode(CookieContainer cookieContainer)
        {
            var mode = Mode.Exhibitions; // get conservative if detection goes wrong
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest) WebRequest.Create("http://www.saltybet.com/state.json");
                request.CookieContainer = cookieContainer;
                var response = (HttpWebResponse) request.GetResponse();
                using (var respStream = response.GetResponseStream())
                {
                    if (respStream != null)
                    {
                        var encoding = Encoding.GetEncoding("utf-8");
                        var streamReader = new StreamReader(respStream, encoding);

                        content = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                // swallow rather than crash. We'll try again next time.
                Console.WriteLine("Error around GetLatestMatch requests {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var dto = JsonConvert.DeserializeObject<ModeDTO>(content);
                if (dto.remaining.Contains("until the next tournament") ||
                    dto.remaining.Contains("Matchmaking mode has been") ||
                    dto.remaining.Contains("Tournament mode will be"))
                {
                    mode = Mode.Matchmaking;
                }
                else if (dto.remaining.Contains("characters left in the bracket") ||
                         dto.remaining.Contains("Tournament mode has been") ||
                         dto.remaining.Contains("FINAL ROUND"))
                {
                    mode = Mode.Tournament;
                }
            }
            return mode;
        }

        private static int? GetBalance(CookieContainer cookieContainer)
        {
            int? balance = null;
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com");
                request.CookieContainer = cookieContainer;
                var response = (HttpWebResponse)request.GetResponse();
                using (var respStream = response.GetResponseStream())
                {
                    if (respStream != null)
                    {
                        var encoding = Encoding.GetEncoding("utf-8");
                        var streamReader = new StreamReader(respStream, encoding);
                        content = streamReader.ReadToEnd();

                    }
                }
            }
            catch (Exception e)
            {
                // swallow rather than crash. We'll try again next time.
                Console.WriteLine("Error around GetLatestMatch requests {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var balanceNode = doc.DocumentNode.SelectSingleNode(".//*/span[@id='balance']");
                try
                {
                    balance = int.Parse(Regex.Replace(balanceNode.InnerText, "[^0-9]", ""));
                }
                catch (Exception)
                {
                    Console.WriteLine("Whoa, dodgy balance mate. Can't parse that.");
                }
            }
            return balance;
        }
    }
}
