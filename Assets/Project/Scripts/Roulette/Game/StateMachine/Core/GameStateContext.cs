using Project.Scripts.Camera;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    public sealed class GameStateContext
    {
        public RouletteGame Game { get; }
        public GameCamera Camera { get; }

        public GameRuntimeData GameData;

        public GameStateContext(RouletteGame game, GameCamera camera)
        {
            Game = game;
            Camera = camera;
            GameData = new GameRuntimeData();
        }
    }
}
