using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;

namespace Project.Scripts.GUI.Desk
{
    public class DeskModel : IModel
    {
        public readonly Observable<Chip> SelectedChip;
        public readonly Dictionary<string, Bet> Bets;

        public DeskModel()
        {
            SelectedChip = new Observable<Chip>();
            Bets = new Dictionary<string, Bet>();
        }

        public void AddChipToBet(string areaId, Chip chip)
        {
            if (!Bets.TryGetValue(areaId, out Bet bet))
            {
                bet = new Bet
                {
                    Chips = new List<Chip>()
                };
            }
            else if (bet.Chips == null)
            {
                bet.Chips = new List<Chip>();
            }
            bet.Chips.Add(chip);
            Bets[areaId] = bet;
        }

        public bool RemoveLastChipFromBet(string areaId)
        {
            if (!Bets.TryGetValue(areaId, out Bet bet) || bet.Chips == null || bet.Chips.Count == 0)
            {
                return false;
            }

            int lastChipIndex = bet.Chips.Count - 1;
            bet.Chips.RemoveAt(lastChipIndex);

            if (bet.Chips.Count == 0)
            {
                Bets.Remove(areaId);
                return true;
            }

            Bets[areaId] = bet;
            return true;
        }

        public void ClearBets()
        {
            Bets.Clear();
        }
    }
}
