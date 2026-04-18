using System;
using UnityEngine;

namespace Project.Scripts.BetManagement.Chip
{
    [Serializable]
    public class ChipArea : ClickableAreaData
    {
        public Chip Chip;
        public RectTransform SelectedVisualEffect;
    }
}
