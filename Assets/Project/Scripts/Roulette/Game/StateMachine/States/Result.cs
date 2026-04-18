using Project.Scripts.Roulette.Desk;
using Project.Scripts.Roulette.Game.StateMachine.Core;

namespace Project.Scripts.Roulette.Game.StateMachine.States
{
    public class Result : GameStateBase
    {
        public Result(GameStateContext context) : base(context)
        {
        }

        protected override void OnEnter()
        {
            SlotInfo finalSlotInfo = Context.GameData.LastSlotInfo;
            UnityEngine.Debug.Log($"Final slot index: [{finalSlotInfo.Index}], final slot number: [{finalSlotInfo.Number}], final slot color: [{finalSlotInfo.Color}].");
            RouletteGame.StateMachine.ChangeState<Bet>();
        }
    }
}
