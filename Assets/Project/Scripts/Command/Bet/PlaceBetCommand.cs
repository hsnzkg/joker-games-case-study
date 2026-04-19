using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.GUI.Desk;

namespace Project.Scripts.Command.Bet
{
    public sealed class PlaceBetCommand : ICommand
    {
        private readonly DeskModel m_model;
        private readonly string m_areaId;
        private readonly Chip m_chip;

        public PlaceBetCommand(DeskModel model, string areaId, Chip chip)
        {
            m_model = model;
            m_areaId = areaId;
            m_chip = chip;
        }

        public bool Execute()
        {
            if (m_model == null || string.IsNullOrWhiteSpace(m_areaId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(m_chip.Id) || m_chip.Value <= 0)
            {
                return false;
            }

            m_model.AddBet(m_areaId, m_chip);
            return true;
        }

        public void Undo()
        {
            if (m_model == null || string.IsNullOrWhiteSpace(m_areaId))
            {
                return;
            }

            if (!m_model.RemoveLastBet(m_areaId, m_chip))
            {
                return;
            }
        }
    }
}
