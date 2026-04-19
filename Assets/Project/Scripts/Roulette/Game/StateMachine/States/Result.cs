using System.Collections.Generic;
using Project.Scripts.BetManagement;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.Command;
using Project.Scripts.Currency;
using Project.Scripts.GUI.Desk;
using Project.Scripts.Roulette.Desk;
using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Result : GameStateBase
    {
        protected override GameSessionStateType StateType => GameSessionStateType.Result;

        public Result(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            SlotInfo finalSlotInfo = Context.GameData.LastSlotInfo;
            BoardData roundBoardData = GetRoundBoardData();
            BetRoundResult roundResult = BetResultCalculator.Calculate(roundBoardData, finalSlotInfo, DeskController.ResolveBetArea);

            Context.GameData.LastBetRoundResult = roundResult;

            if (roundResult.TotalReturned > 0)
            {
                CurrencyManager.Instance.Add(roundResult.TotalReturned);
                OnWin(finalSlotInfo, roundResult);
            }
            else
            {
                OnLose(finalSlotInfo, roundResult);
            }

            DeskController.ClearCurrentBoard();
            CommandManager.ForceClear();
            Context.GameData.CurrentRoundBoardData = new BoardData(new List<BetManagement.Bet.Bet>());
            Context.Game.ClearSessionSimulationState(StateType);
            RouletteGame.StateMachine.ChangeState<Bet>();
        }

        private BoardData GetRoundBoardData()
        {
            if (Context.GameData.CurrentRoundBoardData.Bets != null && Context.GameData.CurrentRoundBoardData.Bets.Count > 0)
            {
                return Context.GameData.CurrentRoundBoardData;
            }

            return DeskController.TryGetCurrentBoardData(out BoardData boardData) ? boardData : new BoardData(new List<BetManagement.Bet.Bet>());
        }

        private void OnWin(SlotInfo finalSlotInfo, BetRoundResult roundResult)
        {
            // TODO: Add win-specific UI, VFX, SFX, and result presentation here.
            UnityEngine.Debug.Log($"Win. Slot: [{finalSlotInfo.Number}], returned: [{roundResult.TotalReturned}], profit: [{roundResult.NetProfit}].");
        }

        private void OnLose(SlotInfo finalSlotInfo, BetRoundResult roundResult)
        {
            // TODO: Add lose-specific UI, VFX, SFX, and result presentation here.
            UnityEngine.Debug.Log($"Lose. Slot: [{finalSlotInfo.Number}], invested: [{roundResult.TotalInvested}], returned: [{roundResult.TotalReturned}].");
        }
    }
}
