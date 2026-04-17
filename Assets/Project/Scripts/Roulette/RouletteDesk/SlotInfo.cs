namespace Project.Scripts.Roulette.RouletteDesk
{
    public struct SlotInfo
    {
        public readonly int Index;
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