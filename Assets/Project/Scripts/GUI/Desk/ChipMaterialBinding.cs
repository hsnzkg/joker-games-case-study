using System;
using UnityEngine;

namespace Project.Scripts.GUI.Desk
{
    public partial class DeskView
    {
        [Serializable]
        public struct ChipMaterialBinding
        {
            public int Id;
            public Material Material;
        }
    }
}