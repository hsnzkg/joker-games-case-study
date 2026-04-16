using Project.Scripts.Physic;
using Project.Scripts.Physic.State;
using UnityEngine;

namespace Project.Scripts.RouletteDesk
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
