using Project.Scripts.BetManagement;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.Currency;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.GUI.Desk;
using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.SessionManagement.Data;
using UnityEngine;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Bet : GameStateBase
    {
        private readonly EventBind<EPlayPress> m_playPressedBind;
        protected override GameSessionStateType StateType => GameSessionStateType.Bet;

        public Bet(GameStateContext context) : base(context)
        {
            m_playPressedBind = new EventBind<EPlayPress>(OnPlayPressed);
        }

        protected override void OnEnter()
        {
            EventBus<EPlayPress>.Register(m_playPressedBind);

            if (Context.ShouldResumeFromPostGameData)
            {
                Context.ShouldResumeFromPostGameData = false;
                Context.Game.StartGame();
            }
            else
            {
                Context.Game.ClearSessionSimulationState(StateType);
            }
        }

        protected override void OnExit()
        {
            EventBus<EPlayPress>.Unregister(m_playPressedBind);
            EventBus<EBetExit>.Raise(new EBetExit());
        }

        private void OnPlayPressed()
        {
            if (!DeskController.TryGetCurrentBoardData(out BoardData boardData))
            {
                boardData = new BoardData(new System.Collections.Generic.List<BetManagement.Bet.Bet>());
            }

            int totalInvested = BetResultCalculator.CalculateTotalInvested(boardData);
            if (totalInvested > 0 && !CurrencyManager.Instance.TryRemove(totalInvested, out _))
            {
                UnityEngine.Debug.LogWarning($"Not enough currency to start the game. Total bet amount: [{totalInvested}].");
                return;
            }

            Context.GameData.CurrentRoundBoardData = boardData;
            Context.Game.StartGame();
        }
    }
}
