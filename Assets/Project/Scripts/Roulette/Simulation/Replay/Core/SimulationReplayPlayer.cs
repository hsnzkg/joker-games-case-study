using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.Replay.Core
{
    public class SimulationReplayPlayer<TState>
    {
        private readonly ISimulationReplayAdapter<TState> m_adapter;
        private TState[] m_states;
        private float m_tickDuration;
        private float m_elapsedTime;
        private int m_frameCount;
        private int m_lastAppliedTick;

        public event System.Action OnReplayStarted;
        public event System.Action OnReplayEnded;
        public bool IsPlaying { get; private set; }

        public SimulationReplayPlayer(ISimulationReplayAdapter<TState> adapter)
        {
            m_adapter = adapter;
            m_lastAppliedTick = -1;
        }

        public void Play(TState[] states, int frameCount, float tickDuration)
        {
            m_states = states;
            m_frameCount = Mathf.Clamp(frameCount, 0, states?.Length ?? 0);
            m_tickDuration = Mathf.Max(tickDuration, Mathf.Epsilon);
            m_elapsedTime = 0f;
            m_lastAppliedTick = -1;
            IsPlaying = m_frameCount > 0;

            if (!IsPlaying)
            {
                return;
            }

            ApplyTick(0);
            OnReplayStarted?.Invoke();
        }

        public void Stop()
        {
            if (!IsPlaying)
            {
                return;
            }

            IsPlaying = false;
            OnReplayEnded?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (!IsPlaying || m_frameCount == 0)
            {
                return;
            }

            if (m_frameCount == 1)
            {
                ApplyTick(0);
                Stop();
                return;
            }

            m_elapsedTime += deltaTime;

            float replayTick = m_elapsedTime / m_tickDuration;
            int fromIndex = Mathf.Clamp(Mathf.FloorToInt(replayTick), 0, m_frameCount - 1);
            int toIndex = Mathf.Min(fromIndex + 1, m_frameCount - 1);

            if (fromIndex != m_lastAppliedTick)
            {
                ApplyTick(fromIndex);
            }

            if (fromIndex >= m_frameCount - 1)
            {
                m_adapter.ApplyState(m_states[m_frameCount - 1]);
                Stop();
                return;
            }

            float alpha = replayTick - fromIndex;
            m_adapter.ApplyInterpolatedState(m_states[fromIndex], m_states[toIndex], alpha);
        }

        private void ApplyTick(int index)
        {
            m_lastAppliedTick = index;
            m_adapter.ApplyState(m_states[index]);
        }
    }
}
