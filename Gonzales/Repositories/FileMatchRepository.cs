using System;
using Gonzales.Interfaces;
using Gonzales.Models;

namespace Gonzales.Repositories
{
    internal class FileMatchRepository : IMatchRepository
    {
        public void Save(Match match)
        {
            Console.WriteLine("Save this {0} vs {1} match to a file please", match.Red, match.Blue);
        }
    }
}
