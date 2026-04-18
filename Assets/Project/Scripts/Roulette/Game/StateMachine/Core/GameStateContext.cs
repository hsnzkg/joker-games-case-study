using Project.Scripts.Camera;
using Project.Scripts.SessionManagement.Data;

namespace Project.Scripts.Roulette.Game.StateMachine.Core
{
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
            CurrentPostGameData = new PostGameData();
            ShouldResumeFromPostGameData = false;
        }
    }
}