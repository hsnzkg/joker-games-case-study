using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.Command;
using Project.Scripts.Command.Bet;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GameState;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.GUI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.GUI.Desk
{
    public class DeskController : ControllerBase<DeskView, DeskModel>
    {
        private readonly EventBind<EBetExit> m_betExitBind;
        private readonly EventBind<EUndoPressed> m_undoPressedBind;
        private readonly EventBind<EResetPress> m_resetPressedBind;
        private readonly EventBind<EGameOpened> m_gameOpenedBind;
        private readonly EventBind<EGameClosed> m_gameClosedBind;
        private readonly Dictionary<string, List<GameObject>> m_activeBetChips = new();
        private readonly Stack<GameObject> m_chipPool = new();
        private readonly Material m_defaultBetChipMaterial;

        public DeskController(DeskView view, DeskModel model) : base(view, model)
        {
            m_betExitBind = new EventBind<EBetExit>(OnBetExited);
            m_undoPressedBind = new EventBind<EUndoPressed>(OnUndoPressed);
            m_resetPressedBind = new EventBind<EResetPress>(OnResetPressed);
            m_gameOpenedBind = new EventBind<EGameOpened>(OnGameLifecycleReset);
            m_gameClosedBind = new EventBind<EGameClosed>(OnGameLifecycleReset);
            Renderer renderer = view.BetChipPrefab != null ? view.BetChipPrefab.GetComponentInChildren<Renderer>(true) : null;
            m_defaultBetChipMaterial = renderer != null ? renderer.sharedMaterial : null;
        }

        public override void Enable()
        {
            View.BetAreaPressed += OnBetAreaPressed;
            View.ChipAreaPressed += OnChipAreaPressed;
            EventBus<EBetExit>.Register(m_betExitBind);
            EventBus<EUndoPressed>.Register(m_undoPressedBind);
            EventBus<EResetPress>.Register(m_resetPressedBind);
            EventBus<EGameOpened>.Register(m_gameOpenedBind);
            EventBus<EGameClosed>.Register(m_gameClosedBind);
        }

        public override void Disable()
        {
            View.BetAreaPressed -= OnBetAreaPressed;
            View.ChipAreaPressed -= OnChipAreaPressed;
            EventBus<EBetExit>.Unregister(m_betExitBind);
            EventBus<EUndoPressed>.Unregister(m_undoPressedBind);
            EventBus<EResetPress>.Unregister(m_resetPressedBind);
            EventBus<EGameOpened>.Unregister(m_gameOpenedBind);
            EventBus<EGameClosed>.Unregister(m_gameClosedBind);
            ReleaseAllPooledBetChips();
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

            bool hasExecuted = CommandManager.Execute(new PlaceBetCommand(Model, this, betArea, selectedChip));
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

        public bool TrySpawnPooledBetChip(string areaId, Chip chip, out GameObject chipObject)
        {
            chipObject = null;

            if (View.BetChipPrefab == null)
            {
                Debug.LogWarning("Bet chip prefab is missing on DeskView.");
                return false;
            }

            if (!View.TryGetBetAreaTransform(areaId, out Transform betAreaTransform))
            {
                return false;
            }

            chipObject = GetOrCreatePooledChipObject();
            if (chipObject == null)
            {
                return false;
            }

            if (!m_activeBetChips.TryGetValue(areaId, out List<GameObject> chips))
            {
                chips = new List<GameObject>();
                m_activeBetChips.Add(areaId, chips);
            }

            chips.Add(chipObject);
            PreparePooledChipObject(chipObject, chip, betAreaTransform, chips.Count - 1);
            return true;
        }

        public void ReleasePooledBetChip(string areaId, GameObject chipObject)
        {
            if (chipObject == null)
            {
                return;
            }

            if (m_activeBetChips.TryGetValue(areaId, out List<GameObject> chips))
            {
                chips.Remove(chipObject);
                if (chips.Count == 0)
                {
                    m_activeBetChips.Remove(areaId);
                }
            }

            chipObject.SetActive(false);
            chipObject.transform.SetParent(View.transform, true);
            m_chipPool.Push(chipObject);
        }

        private void OnUndoPressed()
        {
            CommandManager.Undo();
        }

        private void OnResetPressed()
        {
            CommandManager.Clear();
            Model.ClearBets();
            ReleaseAllPooledBetChips();
        }

        private void OnGameLifecycleReset()
        {
            Model.ClearBets();
            ReleaseAllPooledBetChips();
        }

        private GameObject GetOrCreatePooledChipObject()
        {
            if (m_chipPool.Count > 0)
            {
                return m_chipPool.Pop();
            }

            GameObject chipObject = Object.Instantiate(View.BetChipPrefab, View.transform);
            SetPooledChipInteractivity(chipObject, false);
            return chipObject;
        }

        private void PreparePooledChipObject(GameObject chipObject, Chip chip, Transform betAreaTransform, int stackIndex)
        {
            chipObject.transform.SetParent(View.transform, true);
            chipObject.transform.SetPositionAndRotation(betAreaTransform.position + (Vector3.up * (View.BetChipStackYOffset * stackIndex)), betAreaTransform.rotation);
            chipObject.SetActive(true);

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
            foreach (List<GameObject> chips in m_activeBetChips.Values)
            {
                for (int index = 0; index < chips.Count; index++)
                {
                    GameObject chipObject = chips[index];
                    if (chipObject == null)
                    {
                        continue;
                    }

                    chipObject.SetActive(false);
                    chipObject.transform.SetParent(View.transform, true);
                    m_chipPool.Push(chipObject);
                }
            }

            m_activeBetChips.Clear();
        }
    }
}
