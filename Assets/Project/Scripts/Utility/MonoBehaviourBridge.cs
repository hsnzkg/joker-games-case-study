using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.Application;
using Project.Scripts.Singleton;
using UnityEngine;

namespace Project.Scripts.Utility
{
    public class MonoBehaviourBridge : MonoBehaviourSingleton<MonoBehaviourBridge>
    {
        protected override void OnAwake()
        {
            Application.runInBackground = true;
            Application.quitting += OnQuit;
            EventBus<EStart>.Raise(new EStart());
        }

        private void OnQuit()
        {
            EventBusCenter.DisposeAllBuses();
            EventBus<EQuit>.Raise(new EQuit());
        }
    }
}