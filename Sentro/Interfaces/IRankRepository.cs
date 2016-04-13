using System.Collections.Generic;
using Sentro.Models;

namespace Sentro.Interfaces
{
    public interface IRankRepository
    {
        Dictionary<string, int> GetAllRanks();
    }
}
