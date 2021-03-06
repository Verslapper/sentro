﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Match = Gonzales.Models.Match;

namespace Gonzales
{
    class Program
    {
        private static readonly MatchService _matchService = new MatchService();

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

            var tournamentId = 8871;

            while (tournamentId >= 70) // sketchy data and IRL players before 70 (Shaker Classic #2)
            {
                var matches = GetMatches(cookieContainer, tournamentId);
                foreach (var match in matches)
                {
                    _matchService.Save(match);
                }
                --tournamentId;
            }
        }

        private static CookieContainer CreateCookieContainer(IEnumerable<string> cookieArgs)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookieCollectionPart in cookieArgs)
            {
                var cookieParts = cookieCollectionPart.Replace(";", "").Split('=');
                var target = new Uri("http://www.saltybet.com/");
                var cookie = new Cookie(cookieParts[0], cookieParts[1]) { Domain = target.Host };
                cookieContainer.Add(cookie);
            }
            return cookieContainer;
        }

        private static List<Match> GetMatches(CookieContainer cookieContainer, int tournamentId)
        {
            var matches = new List<Match>();
            var content = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://www.saltybet.com/stats?tournament_id=" + tournamentId);
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
                Console.WriteLine("Error around GetMatches {0} {1} {2}", e.Message, e.InnerException, e.StackTrace);
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                var nodes = doc.DocumentNode.SelectNodes(".//*/table/tbody/tr");
                foreach (var node in nodes)
                {
                    var red = node.SelectSingleNode("//span[@class='redtext']").InnerText;
                    var blue = node.SelectSingleNode("//span[@class='bluetext']").InnerText;
                    var regex = new Regex(@"- \$\d+");
                    var betTotals = regex.Matches(node.SelectSingleNode("//td[1]").InnerText);
                    var winner = node.SelectSingleNode("//td[2]").InnerText;
                    var startTime = DateTime.Parse(node.SelectSingleNode("//td[3]").InnerText);
                    var endTime = DateTime.Parse(node.SelectSingleNode("//td[4]").InnerText);
                    var duration = endTime - startTime;
                    var bettors = int.Parse(node.SelectSingleNode("//td[5]").InnerText);
                    //<tr class="odd">
                    //    < td class=" "><a href = "stats?match_id=510935" >< span class="redtext">Existence-less</span> - $1194657, <span class="bluetext">Guts</span> - $437387</a></td>
                    //    <td class=" "><span class="bluetext">Guts</span></td>
                    //    <td class=" ">5:51am</td>
                    //    <td class=" ">5:57am</td>
                    //    <td class=" ">229</td>
                    //</tr>

                    var match = new Match
                    {
                        Red = red,
                        Blue = blue,
                        Winner = winner,
                        Duration = duration,
                        Bettors = bettors,
                    };

                    if (betTotals.Count == 2)
                    {
                        match.RedTotalBetted = int.Parse(Regex.Replace(betTotals[0].Value, @"[^\d]", ""));
                        match.BlueTotalBetted = int.Parse(Regex.Replace(betTotals[1].Value, @"[^\d]", ""));
                    }

                    Console.WriteLine("{0} (${1}) vs {2} (${3}) among {6} bettors => won by {4} in {5} minutes", red, match.RedTotalBetted, blue, match.BlueTotalBetted, winner, duration.Minutes, bettors);

                    matches.Add(match);
                }
            }
            return matches;
        }
    }
}
