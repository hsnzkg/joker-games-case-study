using System;
using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;

namespace Project.Scripts.GUI.Desk
{
    public partial class DeskView
    {
        [Serializable]
        public class BetAreaGroup
        {
            public string GroupId;
            public List<BetArea> BetData = new();
        }
    }
}