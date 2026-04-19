using System.Collections.Generic;
using Project.Scripts.BetManagement;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.Roulette.Desk;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public class GameRuntimeData
    {
        public SlotInfo LastSlotInfo { get; set; }
        public BoardData CurrentRoundBoardData { get; set; } = new(new List<Bet>());
        public BetRoundResult LastBetRoundResult { get; set; }
    }
}
