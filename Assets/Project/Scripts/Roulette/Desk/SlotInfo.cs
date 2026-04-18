namespace Project.Scripts.Roulette.Desk
{
    public struct SlotInfo
    {
        public int Index;
        public int Number;
        public SlotColor Color;

        public SlotInfo(int index, int number, SlotColor color)
        {
            Index = index;
            Number = number;
            Color = color;
        }
    }
}
