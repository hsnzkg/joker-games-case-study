using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.Replay.Core
{
    public interface ISimulationReplayAdapter<in TState>
    {
        Vector3 GetPosition(TState state);
        Quaternion GetRotation(TState state);
        void ApplyState(TState state);
        void ApplyInterpolatedState(TState fromState, TState toState, float alpha);
    }
}
