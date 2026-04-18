using System.Collections.Generic;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;

namespace Project.Scripts.GUI.Desk
{
    public class DeskModel : IModel
    {
        public readonly Observable<Chip> SelectedChip;
        public Dictionary<string, BetManagement.Bet.Bet> Bets;

        public DeskModel()
        {
            SelectedChip = new Observable<Chip>();
            Bets = new Dictionary<string, BetManagement.Bet.Bet>();
        }
    }
}