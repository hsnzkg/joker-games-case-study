using Project.Scripts.Roulette.RouletteBall;
using Project.Scripts.Roulette.Simulation.Replay.Core;
using Project.Scripts.Roulette.Simulation.State;
using UnityEngine;

namespace Project.Scripts.Roulette.Simulation.Replay
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
