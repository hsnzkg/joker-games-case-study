using System;
using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;

namespace Project.Scripts.BetManagement
{
    [Serializable]
    public struct BoardData
    {
        public List<BetArea> Areas;
        public BoardData(List<BetArea> areas)
        {
            Areas = areas ?? new List<BetArea>();
        }
    }
}
