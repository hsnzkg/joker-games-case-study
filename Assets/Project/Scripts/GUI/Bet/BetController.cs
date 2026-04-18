using Project.Scripts.GUI.Core;
using UnityEngine;

namespace Project.Scripts.GUI.Bet
{
    public class BetController : ControllerBase<BetView, BetModel>
    {
        public BetController(BetView view, BetModel model) : base(view, model)
        {
        }

        public override void Enable()
        {
            View.BetAreaPressed += OnBetAreaPressed;
        }

        public override void Disable()
        {
            View.BetAreaPressed -= OnBetAreaPressed;
        }

        private void OnBetAreaPressed(BetArea betArea)
        {
            var targetName = betArea.Target != null ? betArea.Target.name : "Unknown";
            Debug.Log($"Tried To Bet On {targetName}. Type: {betArea.Type}");
        }
    }
}