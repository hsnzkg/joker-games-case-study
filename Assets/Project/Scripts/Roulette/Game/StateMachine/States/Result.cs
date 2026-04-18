using Project.Scripts.Roulette.Desk;
using Project.Scripts.Roulette.Game.StateMachine.Core;
using Project.Scripts.StateManagement.Data;

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
            UnityEngine.Debug.Log($"Final slot index: [{finalSlotInfo.Index}], final slot number: [{finalSlotInfo.Number}], final slot color: [{finalSlotInfo.Color}].");
            Context.Game.ClearSessionSimulationState(StateType);
            RouletteGame.StateMachine.ChangeState<Bet>();
        }
    }
}
