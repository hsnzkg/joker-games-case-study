using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.Currency;
using Project.Scripts.GUI.Desk;
using UnityEngine;

namespace Project.Scripts.Command.Bet
{
    public sealed class PlaceBetCommand : ICommand
    {
        private readonly DeskModel m_model;
        private readonly DeskController m_controller;
        private readonly BetArea m_betArea;
        private readonly Chip m_chip;
        private GameObject m_spawnedChipObject;

        public PlaceBetCommand(DeskModel model, DeskController controller, BetArea betArea, Chip chip)
        {
            m_model = model;
            m_controller = controller;
            m_betArea = betArea;
            m_chip = chip;
        }

        public bool Execute()
        {
            if (m_model == null || m_betArea == null || string.IsNullOrWhiteSpace(m_betArea.AreaId))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(m_chip.Id) || m_chip.Value <= 0)
            {
                return false;
            }

            if (!CurrencyManager.Instance.TryRemove(m_chip.Value, out _))
            {
                return false;
            }

            m_model.AddChipToBet(m_betArea.AreaId, m_chip);

            if (!m_controller.TrySpawnPooledBetChip(m_betArea.AreaId, m_chip, out m_spawnedChipObject))
            {
                m_model.RemoveLastChipFromBet(m_betArea.AreaId);
                CurrencyManager.Instance.Add(m_chip.Value);
                return false;
            }

            return true;
        }

        public void Undo()
        {
            if (m_model == null || m_betArea == null || string.IsNullOrWhiteSpace(m_betArea.AreaId))
            {
                return;
            }

            if (!m_model.RemoveLastChipFromBet(m_betArea.AreaId))
            {
                return;
            }

            if (m_spawnedChipObject != null)
            {
                m_controller.ReleasePooledBetChip(m_betArea.AreaId, m_spawnedChipObject);
                m_spawnedChipObject = null;
            }

            CurrencyManager.Instance.Add(m_chip.Value);
        }
    }
}
