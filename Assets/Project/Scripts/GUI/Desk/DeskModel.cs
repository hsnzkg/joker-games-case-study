using System.Collections.Generic;
using Project.Scripts.BetManagement;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.GUI.Core;
using Project.Scripts.Observable;

namespace Project.Scripts.GUI.Desk
{
    public class DeskModel : IModel
    {
        public readonly Observable<Chip> SelectedChip;
        public readonly Observable<BoardData> BoardState;

        public DeskModel()
        {
            SelectedChip = new Observable<Chip>();
            BoardState = new Observable<BoardData>(new BoardData(new List<Bet>()));
        }

        public void AddBet(string areaId, Chip chip)
        {
            List<Bet> bets = GetBetsCopy();
            bets.Add(new Bet(areaId, chip));
            BoardState.Value = new BoardData(bets);
        }

        public bool RemoveLastBet(string areaId, Chip chip)
        {
            List<Bet> bets = GetBetsCopy();

            for (int index = bets.Count - 1; index >= 0; index--)
            {
                Bet bet = bets[index];
                if (bet.AreaId != areaId || !bet.Chip.Equals(chip))
                {
                    continue;
                }

                bets.RemoveAt(index);
                BoardState.Value = new BoardData(bets);
                return true;
            }

            return false;
        }

        public void SetBoardData(BoardData boardData)
        {
            BoardState.Value = new BoardData(boardData.Bets);
        }

        public void ClearBets()
        {
            BoardState.Value = new BoardData(new List<Bet>());
        }

        private List<Bet> GetBetsCopy()
        {
            return BoardState.Value.Bets != null ? new List<Bet>(BoardState.Value.Bets) : new List<Bet>();
        }
    }
}
