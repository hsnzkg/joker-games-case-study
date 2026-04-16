using Project.Scripts.Physic;
using UnityEngine;

namespace Project.Scripts.RouletteBall
{
    public class BallReplayAdapter : TransformSimulationReplayAdapter<BallState>
    {
        public BallReplayAdapter(Ball ball) : base(ball.transform)
        {
        }

        public override Vector3 GetPosition(BallState state)
        {
            return state.Position;
        }

        public override Quaternion GetRotation(BallState state)
        {
            return state.Rotation;
        }
    }
}
