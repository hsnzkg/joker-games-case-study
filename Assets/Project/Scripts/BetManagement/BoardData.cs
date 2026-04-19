using System;
using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;

namespace Project.Scripts.BetManagement
{
    [Serializable]
    public struct BoardData
    {
        public List<Bet.Bet> Bets;

        public BoardData(List<Bet.Bet> bets)
        {
            Bets = bets != null ? new List<Bet.Bet>(bets) : new List<Bet.Bet>();
        }
    }
}