using Project.Scripts.Camera;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public sealed class GameStateContext
    {
        public GameCamera Camera;

        public GameStateContext(GameCamera camera)
        {
            Camera = camera;
        }
    }
}