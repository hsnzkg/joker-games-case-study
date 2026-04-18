using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.GUI.Core;
using UnityEngine;

namespace Project.Scripts.GUI.Desk
{
    public class DeskController : ControllerBase<DeskView, DeskModel>
    {
        private readonly EventBind<EBetExit> m_betExitBind;

        public DeskController(DeskView view, DeskModel model) : base(view, model)
        {
            m_betExitBind = new EventBind<EBetExit>(OnBetExited);
        }

        public override void Enable()
        {
            View.BetAreaPressed += OnBetAreaPressed;
            View.ChipAreaPressed += OnChipAreaPressed;
            EventBus<EBetExit>.Register(m_betExitBind);
        }

        public override void Disable()
        {
            View.BetAreaPressed -= OnBetAreaPressed;
            View.ChipAreaPressed -= OnChipAreaPressed;
            EventBus<EBetExit>.Unregister(m_betExitBind);
        }

        private void OnBetAreaPressed(string id)
        {
            if (!View.TryGetBetArea(id, out BetArea betArea)) return;
            Debug.Log($"BetArea {betArea.AreaId}");
        }

        private void OnChipAreaPressed(string id)
        {
            if (!View.TryGetChipArea(id, out ChipArea chipArea)) return;
            if (Model.SelectedChip.Value.Equals(chipArea.Chip))
            {
                View.ReleaseChip(chipArea);
                Model.SelectedChip.Value = default;
            }
            else
            {
                View.ReleaseChip(Model.SelectedChip.Value.Id);
                Model.SelectedChip.Value = chipArea.Chip;
                View.SelectChip(chipArea);
            }
        }

        private void OnBetExited()
        {
            View.ReleaseChip(Model.SelectedChip.Value.Id);
        }
    }
}