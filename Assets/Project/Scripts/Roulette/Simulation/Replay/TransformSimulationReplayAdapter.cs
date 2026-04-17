using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.Replay
{
    public abstract class TransformSimulationReplayAdapter<TState> : ISimulationReplayAdapter<TState>
    {
        private readonly Transform m_targetTransform;

        protected TransformSimulationReplayAdapter(Transform targetTransform)
        {
            m_targetTransform = targetTransform;
        }

        public abstract Vector3 GetPosition(TState state);
        public abstract Quaternion GetRotation(TState state);

        public virtual void ApplyState(TState state)
        {
            m_targetTransform.SetPositionAndRotation(GetPosition(state), GetRotation(state));
        }

        public virtual void ApplyInterpolatedState(TState fromState, TState toState, float alpha)
        {
            Vector3 position = Vector3.Lerp(GetPosition(fromState), GetPosition(toState), alpha);
            Quaternion rotation = Quaternion.Slerp(GetRotation(fromState), GetRotation(toState), alpha);
            m_targetTransform.SetPositionAndRotation(position, rotation);
        }
    }
}
