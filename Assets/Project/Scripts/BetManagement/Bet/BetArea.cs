using System;
using System.Collections.Generic;

namespace Project.Scripts.BetManagement.Bet
{
    [Serializable]
    public class BetArea : ClickableAreaData
    {
        public BetType Type;
        public int PayoutMultiplier;    
        public List<int> CoveredNumbers;
    }
}
