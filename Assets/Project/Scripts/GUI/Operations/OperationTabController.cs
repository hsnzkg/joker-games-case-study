using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.EventBus.Events.Replay;
using Project.Scripts.GUI.Core;

namespace Project.Scripts.GUI.Operations
{
    public class OperationTabController : ControllerBase<OperationTabView, OperationTabModel>
    {
        private const string k_emptyDeterministicNumber = "";
        private readonly EventBind<EReplayEnd> m_replayEndedBind;
        private readonly EventBind<EBetExit> m_betExitBind;

        public OperationTabController(OperationTabView view, OperationTabModel model) : base(view, model)
        {
            m_replayEndedBind = new EventBind<EReplayEnd>(OnReplayEnded);
            m_betExitBind = new EventBind<EBetExit>(OnBetExited);
        }

        public override void Enable()
        {
            View.PlayPressed += OnPlayPressed;
            View.UndoPressed += OnUndoPressed;
            View.ResetPressed += OnResetPressed;
            View.DeterministicNumberChanged += OnDeterministicNumberChanged;
            Model.DeterministicNumberText.Subscribe(OnDeterministicNumberTextUpdated, true);
            EventBus<EReplayEnd>.Register(m_replayEndedBind);
            EventBus<EBetExit>.Register(m_betExitBind);
        }

        public override void Disable()
        {
            View.PlayPressed -= OnPlayPressed;
            View.UndoPressed -= OnUndoPressed;
            View.ResetPressed -= OnResetPressed;
            View.DeterministicNumberChanged -= OnDeterministicNumberChanged;
            Model.DeterministicNumberText.Unsubscribe(OnDeterministicNumberTextUpdated);
            EventBus<EReplayEnd>.Unregister(m_replayEndedBind);
            EventBus<EBetExit>.Unregister(m_betExitBind);
        }

        private void OnPlayPressed()
        {
            if (TryGetDeterministicNumber(out int deterministicNumber))
            {
                EventBus<EPlayPress>.Raise(new EPlayPress(deterministicNumber));
                return;
            }

            EventBus<EPlayPress>.Raise(new EPlayPress());
        }

        private void OnUndoPressed()
        {
            EventBus<EUndoPressed>.Raise(new EUndoPressed());
        }

        private void OnResetPressed()
        {
            EventBus<EResetPress>.Raise(new EResetPress());
        }

        private void OnReplayEnded()
        {
            ClearDeterministicNumber();
            View.SetOperationInteractivity(true);
        }

        private void OnBetExited()
        {
            View.SetOperationInteractivity(false);
        }

        private void OnDeterministicNumberChanged(string value)
        {
            Model.DeterministicNumberText.Value = value ?? k_emptyDeterministicNumber;
        }

        private void OnDeterministicNumberTextUpdated(string value)
        {
            if (HasInvalidCharacters(value))
            {
                ClearDeterministicNumber();
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                View.ClearDeterministicNumberText();
                return;
            }

            View.SetDeterministicNumberText(value);
        }

        private void ClearDeterministicNumber()
        {
            Model.DeterministicNumberText.Value = k_emptyDeterministicNumber;
            View.ClearDeterministicNumberText();
        }

        private bool TryGetDeterministicNumber(out int deterministicNumber)
        {
            deterministicNumber = default;
            string deterministicNumberText = Model.DeterministicNumberText.Value;
            return !string.IsNullOrEmpty(deterministicNumberText) && int.TryParse(deterministicNumberText, out deterministicNumber);
        }

        private static bool HasInvalidCharacters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                if (!char.IsDigit(value[index]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}