using Project.Scripts.Roulette.Simulation.State;
using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.Replay
{
    public class DeskReplayAdapter : TransformSimulationReplayAdapter<DeskState>
    {
        public DeskReplayAdapter(Transform targetTransform) : base(targetTransform)
        {
        }

        public override Vector3 GetPosition(DeskState state)
        {
            return state.Position;
        }

        public override Quaternion GetRotation(DeskState state)
        {
            return state.Rotation;
        }
    }
}
