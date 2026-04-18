using Project.Scripts.Camera;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
    using Project.Scripts.StateManagement.Data;

    public sealed class GameStateContext
    {
        public RouletteGame Game { get; }
        public GameCamera Camera { get; }

        public readonly GameRuntimeData GameData;
        public PostGameData CurrentPostGameData;
        public bool ShouldResumeFromPostGameData;

        public GameStateContext(RouletteGame game, GameCamera camera)
        {
            Game = game;
            Camera = camera;
            GameData = new GameRuntimeData();
            CurrentPostGameData = PostGameData.Empty;
            ShouldResumeFromPostGameData = false;
        }
    }
}