using System;
using System.Collections.Generic;

namespace Project.Scripts.BetManagement.Bet
{
    [Serializable]
    public struct Bet
    {
        public string AreaId;
        public Chip.Chip Chip;

        public Bet(string areaId, Chip.Chip chip)
        {
            AreaId = areaId;
            Chip = chip;
        }
    }
}
