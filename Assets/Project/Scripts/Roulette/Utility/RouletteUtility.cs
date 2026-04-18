using Project.Scripts.Constants;
using Project.Scripts.Roulette.Desk;
using UnityEngine;

namespace Project.Scripts.Roulette.Utility
{
    public static class RouletteUtility
    {
        public static SlotInfo GetRandomSlotInfoByColor(this SlotColor color)
        {
            SlotColor[] colors = RouletteMap.Colors;
            int matchingSlotCount = 0;
            for (int index = 0; index < colors.Length; index++)
            {
                if (colors[index] == color)
                {
                    matchingSlotCount++;
                }
            }

            if (matchingSlotCount == 0)
            {
                return new SlotInfo(-1, -1, SlotColor.UNKNOWN);
            }

            int targetMatchIndex = Random.Range(0, matchingSlotCount);
            int currentMatchIndex = 0;

            for (int index = 0; index < colors.Length; index++)
            {
                if (colors[index] != color)
                {
                    continue;
                }

                if (currentMatchIndex == targetMatchIndex)
                {
                    return index.GetSlotInfo();
                }

                currentMatchIndex++;
            }

            return new SlotInfo(-1, -1, SlotColor.UNKNOWN);
        }

        public static SlotInfo GetSlotInfo(this int index)
        {
            int[] numbers = RouletteMap.Numbers;
            SlotColor[] colors = RouletteMap.Colors;
            if (index < 0 || index >= numbers.Length)
            {
                return new SlotInfo(index, -1, SlotColor.UNKNOWN);
            }

            return new SlotInfo(index, numbers[index], colors[index]);
        }

        public static SlotInfo GetSlotInfoBySlotNumber(this int number)
        {
            int[] numbers = RouletteMap.Numbers;
            for (int index = 0; index < numbers.Length; index++)
            {
                if (numbers[index] == number)
                {
                    return index.GetSlotInfo();
                }
            }

            return default;
        }
    }
}