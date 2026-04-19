using System.Collections.Generic;
using System.IO;
using Project.Scripts.BetManagement;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.Command;
using Project.Scripts.Command.Bet;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.GUI.Core;
using Project.Scripts.SessionManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Project.Scripts.GUI.Desk
{
    public class DeskController : ControllerBase<DeskView, DeskModel>
    {
        private const int k_defaultPoolCapacity = 16;
        private const int k_maxPoolSize = 128;
        private const string k_betBoardDirectoryName = "BetBoardData";
        private const string k_betBoardFileName = "BetBoardData.json";

        private readonly EventBind<EBetExit> m_betExitBind;
        private readonly EventBind<EUndoPressed> m_undoPressedBind;
        private readonly EventBind<EResetPress> m_resetPressedBind;
        private readonly EventBind<EGameOpened> m_gameOpenedBind;
        private readonly EventBind<EGameClosed> m_gameClosedBind;
        private readonly List<GameObject> m_activeBetChips = new();
        private readonly ObjectPool<GameObject> m_chipPool;
        private readonly Material m_defaultBetChipMaterial;
        private static DeskController s_activeController;

        private string BetBoardDataFilePath => Path.Combine(Application.persistentDataPath, k_betBoardDirectoryName, k_betBoardFileName);

        public DeskController(DeskView view, DeskModel model) : base(view, model)
        {
            m_betExitBind = new EventBind<EBetExit>(OnBetExited);
            m_undoPressedBind = new EventBind<EUndoPressed>(OnUndoPressed);
            m_resetPressedBind = new EventBind<EResetPress>(OnResetPressed);
            m_gameOpenedBind = new EventBind<EGameOpened>(OnGameOpened);
            m_gameClosedBind = new EventBind<EGameClosed>(OnGameClosed);
            Renderer renderer = view.BetChipPrefab != null ? view.BetChipPrefab.GetComponentInChildren<Renderer>(true) : null;
            m_defaultBetChipMaterial = renderer != null ? renderer.sharedMaterial : null;
            m_chipPool = new ObjectPool<GameObject>(CreatePooledChipObject, OnGetPooledChipObject, OnReleasePooledChipObject, OnDestroyPooledChipObject, false, k_defaultPoolCapacity, k_maxPoolSize);
        }

        public override void Enable()
        {
            s_activeController = this;
            View.BetAreaPressed -= OnBetAreaPressed;
            View.BetAreaPressed += OnBetAreaPressed;
            View.ChipAreaPressed -= OnChipAreaPressed;
            View.ChipAreaPressed += OnChipAreaPressed;
            Model.BoardState.Unsubscribe(OnBoardStateChanged);
            Model.BoardState.Subscribe(OnBoardStateChanged);
            EventBus<EBetExit>.Register(m_betExitBind);
            EventBus<EUndoPressed>.Register(m_undoPressedBind);
            EventBus<EResetPress>.Register(m_resetPressedBind);
            EventBus<EGameOpened>.Register(m_gameOpenedBind);
            EventBus<EGameClosed>.Register(m_gameClosedBind);
            RestoreBoardDataFromStorage();
        }

        public override void Disable()
        {
            View.BetAreaPressed -= OnBetAreaPressed;
            View.ChipAreaPressed -= OnChipAreaPressed;
            Model.BoardState.Unsubscribe(OnBoardStateChanged);
            EventBus<EBetExit>.Unregister(m_betExitBind);
            EventBus<EUndoPressed>.Unregister(m_undoPressedBind);
            EventBus<EResetPress>.Unregister(m_resetPressedBind);
            EventBus<EGameOpened>.Unregister(m_gameOpenedBind);
            EventBus<EGameClosed>.Unregister(m_gameClosedBind);
            ReleaseAllPooledBetChips();
            Model.ClearBets();
            m_chipPool.Clear();

            if (ReferenceEquals(s_activeController, this))
            {
                s_activeController = null;
            }
        }

        private void OnBetAreaPressed(string id)
        {
            if (!View.TryGetBetArea(id, out BetArea betArea)) return;
            Chip selectedChip = Model.SelectedChip.Value;

            if (string.IsNullOrWhiteSpace(selectedChip.Id))
            {
                Debug.LogWarning("A chip must be selected before placing a bet.");
                return;
            }

            bool hasExecuted = CommandManager.Execute(new PlaceBetCommand(Model, betArea.AreaId, selectedChip));
            if (!hasExecuted)
            {
                Debug.LogWarning($"Failed to place bet on area [{betArea.AreaId}].");
            }
        }

        private void OnChipAreaPressed(string id)
        {
            if (!View.TryGetChipArea(id, out ChipArea chipArea)) return;
            if (Model.SelectedChip.Value.Equals(chipArea.Chip))
            {
                View.ReleaseChip(chipArea);
                Model.SelectedChip.Value = default;
            }
            else
            {
                View.ReleaseChip(Model.SelectedChip.Value.Id);
                Model.SelectedChip.Value = chipArea.Chip;
                View.SelectChip(chipArea);
            }
        }

        private void OnBetExited()
        {
            View.ReleaseChip(Model.SelectedChip.Value.Id);
        }

        private void OnUndoPressed()
        {
            CommandManager.Undo();
        }

        private void OnResetPressed()
        {
            CommandManager.Clear();
        }

        private void OnGameOpened()
        {
            RestoreBoardDataFromStorage();
        }

        private void OnGameClosed()
        {
        }

        private GameObject CreatePooledChipObject()
        {
            GameObject chipObject = Object.Instantiate(View.BetChipPrefab, View.transform);
            SetPooledChipInteractivity(chipObject, false);
            return chipObject;
        }

        private static void OnGetPooledChipObject(GameObject chipObject)
        {
            if (chipObject == null)
            {
                return;
            }

            chipObject.SetActive(true);
        }

        private void OnReleasePooledChipObject(GameObject chipObject)
        {
            if (chipObject == null)
            {
                return;
            }

            chipObject.SetActive(false);
            chipObject.transform.SetParent(View.transform, true);
        }

        private static void OnDestroyPooledChipObject(GameObject chipObject)
        {
            if (chipObject == null)
            {
                return;
            }

            Object.Destroy(chipObject);
        }

        private void PreparePooledChipObject(GameObject chipObject, Chip chip, Transform betAreaTransform, int stackIndex)
        {
            chipObject.transform.SetParent(View.transform, true);
            chipObject.transform.SetPositionAndRotation(betAreaTransform.position + (Vector3.up * (View.BetChipStackYOffset * stackIndex)), betAreaTransform.rotation);

            Renderer renderer = chipObject.GetComponentInChildren<Renderer>(true);
            if (renderer == null)
            {
                return;
            }

            if (View.TryGetChipMaterial(chip, out Material material))
            {
                renderer.sharedMaterial = material;
                return;
            }

            renderer.sharedMaterial = m_defaultBetChipMaterial;
        }

        private static void SetPooledChipInteractivity(GameObject chipObject, bool isEnabled)
        {
            if (chipObject == null)
            {
                return;
            }

            ClickableAreaHandler clickableAreaHandler = chipObject.GetComponentInChildren<ClickableAreaHandler>(true);
            if (clickableAreaHandler != null)
            {
                clickableAreaHandler.enabled = isEnabled;
            }

            Collider[] colliders = chipObject.GetComponentsInChildren<Collider>(true);
            for (int index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = isEnabled;
            }

            Graphic[] graphics = chipObject.GetComponentsInChildren<Graphic>(true);
            for (int index = 0; index < graphics.Length; index++)
            {
                graphics[index].gameObject.SetActive(isEnabled);
            }
        }

        private void ReleaseAllPooledBetChips()
        {
            for (int index = 0; index < m_activeBetChips.Count; index++)
            {
                GameObject chipObject = m_activeBetChips[index];
                if (chipObject == null)
                {
                    continue;
                }

                m_chipPool.Release(chipObject);
            }

            m_activeBetChips.Clear();
        }

        private void RestoreBoardDataFromStorage()
        {
            BoardData boardData = LoadBoardData();
            Model.SetBoardData(FilterInvalidBets(boardData));
            RebuildCommandHistory();
        }

        private void OnBoardStateChanged(BoardData boardData)
        {
            RenderBoardData(boardData);
            SaveBoardData(boardData);
        }

        private void RenderBoardData(BoardData boardData)
        {
            ReleaseAllPooledBetChips();

            if (boardData.Bets == null || boardData.Bets.Count == 0)
            {
                return;
            }

            Dictionary<string, int> areaStackIndices = new();

            for (int index = 0; index < boardData.Bets.Count; index++)
            {
                Bet bet = boardData.Bets[index];
                if (!View.TryGetBetAreaTransform(bet.AreaId, out Transform betAreaTransform))
                {
                    continue;
                }

                int stackIndex = areaStackIndices.TryGetValue(bet.AreaId, out int currentIndex) ? currentIndex : 0;
                areaStackIndices[bet.AreaId] = stackIndex + 1;

                GameObject chipObject = m_chipPool.Get();
                PreparePooledChipObject(chipObject, bet.Chip, betAreaTransform, stackIndex);
                m_activeBetChips.Add(chipObject);
            }
        }

        private void SaveBoardData(BoardData boardData)
        {
            if (boardData.Bets == null || boardData.Bets.Count == 0)
            {
                DataSerializer.Delete(BetBoardDataFilePath);
                return;
            }

            DataSerializer.Save(BetBoardDataFilePath, boardData);
        }

        private BoardData LoadBoardData()
        {
            if (!DataSerializer.TryLoad(BetBoardDataFilePath, out BoardData boardData))
            {
                return new BoardData(new List<Bet>());
            }

            return boardData;
        }

        private BoardData FilterInvalidBets(BoardData boardData)
        {
            if (boardData.Bets == null || boardData.Bets.Count == 0)
            {
                return new BoardData(new List<Bet>());
            }

            List<Bet> validBets = new();

            for (int index = 0; index < boardData.Bets.Count; index++)
            {
                Bet bet = boardData.Bets[index];
                if (!View.TryGetBetArea(bet.AreaId, out _))
                {
                    continue;
                }

                validBets.Add(bet);
            }

            return new BoardData(validBets);
        }

        private void RebuildCommandHistory()
        {
            CommandManager.ForceClear();
            BoardData boardData = Model.BoardState.Value;
            if (boardData.Bets == null || boardData.Bets.Count == 0)
            {
                return;
            }

            for (int index = 0; index < boardData.Bets.Count; index++)
            {
                Bet bet = boardData.Bets[index];
                CommandManager.Track(new PlaceBetCommand(Model, bet.AreaId, bet.Chip));
            }
        }

        public static bool TryGetCurrentBoardData(out BoardData boardData)
        {
            if (s_activeController == null)
            {
                boardData = new BoardData(new List<Bet>());
                return false;
            }

            boardData = s_activeController.Model.BoardState.Value;
            return true;
        }

        public static BetArea ResolveBetArea(string areaId)
        {
            if (s_activeController == null || string.IsNullOrWhiteSpace(areaId))
            {
                return null;
            }

            return s_activeController.View.TryGetBetArea(areaId, out BetArea betArea) ? betArea : null;
        }

        public static void ClearCurrentBoard()
        {
            if (s_activeController == null)
            {
                return;
            }

            s_activeController.Model.ClearBets();
        }
    }
}
