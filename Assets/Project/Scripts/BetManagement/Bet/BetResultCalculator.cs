using System;
using Project.Scripts.BetManagement;
using Project.Scripts.Roulette.Desk;

namespace Project.Scripts.BetManagement.Bet
{
    public static class BetResultCalculator
    {
        public static int CalculateTotalInvested(BoardData boardData)
        {
            if (boardData.Bets == null || boardData.Bets.Count == 0)
            {
                return 0;
            }

            int totalInvested = 0;

            for (int index = 0; index < boardData.Bets.Count; index++)
            {
                Bet bet = boardData.Bets[index];
                if (bet.Chip.Value <= 0)
                {
                    continue;
                }

                totalInvested += bet.Chip.Value;
            }

            return totalInvested;
        }

        public static BetRoundResult Calculate(BoardData boardData, SlotInfo finalSlotInfo, Func<string, BetArea> resolveBetArea)
        {
            int totalInvested = CalculateTotalInvested(boardData);
            if (totalInvested <= 0 || boardData.Bets == null || boardData.Bets.Count == 0 || resolveBetArea == null)
            {
                return new BetRoundResult(totalInvested, 0, 0);
            }

            int totalPayout = 0;
            int returnedStake = 0;

            for (int index = 0; index < boardData.Bets.Count; index++)
            {
                Bet bet = boardData.Bets[index];
                BetArea betArea = resolveBetArea(bet.AreaId);
                if (betArea == null || !IsWinningBet(betArea, finalSlotInfo))
                {
                    continue;
                }

                returnedStake += bet.Chip.Value;
                totalPayout += bet.Chip.Value * betArea.PayoutMultiplier;
            }

            return new BetRoundResult(totalInvested, totalPayout, returnedStake);
        }

        private static bool IsWinningBet(BetArea betArea, SlotInfo finalSlotInfo)
        {
            if (betArea.CoveredNumbers == null || betArea.CoveredNumbers.Count == 0)
            {
                return false;
            }

            return betArea.CoveredNumbers.Contains(finalSlotInfo.Number);
        }
    }
}
