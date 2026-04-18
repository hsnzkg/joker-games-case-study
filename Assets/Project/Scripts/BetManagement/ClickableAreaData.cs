using System;
using Newtonsoft.Json;
using Project.Scripts.GUI.Desk;

namespace Project.Scripts.BetManagement
{
    [Serializable]
    public class ClickableAreaData
    {
        [JsonIgnore] public ClickableAreaHandler ClickHandler;
        public string AreaId;
    }
}