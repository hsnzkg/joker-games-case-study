namespace Project.Scripts.BetManagement.Bet
{
    public struct BetRoundResult
    {
        public int TotalInvested;
        public int TotalPayout;
        public int ReturnedStake;
        public int TotalReturned;
        public int NetProfit;

        public BetRoundResult(int totalInvested, int totalPayout, int returnedStake)
        {
            TotalInvested = totalInvested;
            TotalPayout = totalPayout;
            ReturnedStake = returnedStake;
            TotalReturned = totalPayout + returnedStake;
            NetProfit = TotalReturned - totalInvested;
        }
    }
}
