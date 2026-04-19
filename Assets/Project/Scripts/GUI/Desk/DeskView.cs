using System;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.BetManagement.Bet;
using Project.Scripts.BetManagement.Chip;
using Project.Scripts.GUI.Core;
using Project.Scripts.Utility.Easing;
using UnityEngine;

namespace Project.Scripts.GUI.Desk
{
    public class DeskView : ViewBase
    {
        [Serializable]
        public struct ChipMaterialBinding
        {
            public int Id;
            public Material Material;
        }

        [SerializeField] private List<BetArea> m_betData = new();
        [SerializeField] private List<ChipArea> m_chipData = new();
        [SerializeField] private GameObject m_betChipPrefab;
        [SerializeField] private List<ChipMaterialBinding> m_chipMaterialBindings = new();
        [SerializeField] private float m_betChipStackYOffset = 0.01f;

        [SerializeField] private float m_chipSelectScaleDuration = 0.2f;
        [SerializeField] private float m_chipReleaseScaleDuration = 0.15f;

        private readonly Dictionary<ChipArea, Coroutine> m_chipScaleRoutines = new();
        private readonly Dictionary<ChipArea, Vector3> m_chipTargetScales = new();

        public event Action<string> BetAreaPressed;
        public event Action<string> ChipAreaPressed;

        public GameObject BetChipPrefab => m_betChipPrefab;
        public float BetChipStackYOffset => m_betChipStackYOffset;


        protected override void OnEnable()
        {
            Register();
            foreach (ChipArea chipArea in m_chipData)
            {
                InitializeChipVisual(chipArea);
            }
        }

        protected override void OnDisable()
        {
            Unregister();
            StopAllChipScaleRoutines();
        }

        private void Register()
        {
            for (int index = 0; index < m_betData.Count; index++)
            {
                BetArea betArea = m_betData[index];
                betArea.ClickHandler.SetId(betArea.AreaId);
                betArea.ClickHandler.Clicked += OnBetAreaClicked;
            }

            for (int index = 0; index < m_chipData.Count; index++)
            {
                ChipArea chipData = m_chipData[index];
                chipData.ClickHandler.SetId(chipData.AreaId);
                chipData.ClickHandler.Clicked += OnChipAreaClicked;
            }
        }

        private void Unregister()
        {
            for (int index = 0; index < m_betData.Count; index++)
            {
                BetArea betArea = m_betData[index];
                betArea.ClickHandler.Clicked -= OnBetAreaClicked;
            }

            for (int index = 0; index < m_chipData.Count; index++)
            {
                ChipArea chipArea = m_chipData[index];
                chipArea.ClickHandler.Clicked -= OnChipAreaClicked;
            }
        }

        public bool TryGetBetArea(string id, out BetArea betArea)
        {
            betArea = null;
            foreach (BetArea area in m_betData)
            {
                if (area.AreaId != id) continue;
                betArea = area;
                return true;
            }

            return false;
        }

        public bool TryGetBetAreaTransform(string id, out Transform areaTransform)
        {
            areaTransform = null;

            if (!TryGetBetArea(id, out BetArea betArea) || betArea?.ClickHandler == null)
            {
                return false;
            }

            areaTransform = betArea.ClickHandler.transform;
            return true;
        }

        public bool TryGetChipArea(string id, out ChipArea chipArea)
        {
            chipArea = null;
            foreach (ChipArea area in m_chipData)
            {
                if (area.AreaId != id) continue;
                chipArea = area;
                return true;
            }

            return false;
        }

        public void SelectChip(ChipArea chipArea)
        {
            AnimateChipScale(chipArea, true);
        }

        public void ReleaseChip(ChipArea chipArea)
        {
            AnimateChipScale(chipArea, false);
        }

        public void ReleaseChip(string id)
        {
            foreach (ChipArea chipArea in m_chipData)
            {
                if (chipArea.AreaId != id) continue;
                AnimateChipScale(chipArea, false);
            }
        }

        public bool TryGetChipMaterial(Chip chip, out Material material)
        {
            material = null;

            if (!int.TryParse(chip.Id, out int chipId))
            {
                return false;
            }

            for (int index = 0; index < m_chipMaterialBindings.Count; index++)
            {
                ChipMaterialBinding binding = m_chipMaterialBindings[index];
                if (binding.Id != chipId)
                {
                    continue;
                }

                material = binding.Material;
                return material != null;
            }

            return false;
        }

        private void InitializeChipVisual(ChipArea chipArea)
        {
            if (chipArea?.SelectedVisualEffect == null)
            {
                return;
            }

            RectTransform selectedVisualEffect = chipArea.SelectedVisualEffect;
            if (!m_chipTargetScales.ContainsKey(chipArea))
            {
                m_chipTargetScales.Add(chipArea, selectedVisualEffect.localScale);
            }

            selectedVisualEffect.localScale = Vector3.zero;
            selectedVisualEffect.gameObject.SetActive(false);
        }

        private void AnimateChipScale(ChipArea chipArea, bool isSelecting)
        {
            if (chipArea?.SelectedVisualEffect == null)
            {
                return;
            }

            if (!m_chipTargetScales.TryGetValue(chipArea, out Vector3 targetScale))
            {
                targetScale = chipArea.SelectedVisualEffect.localScale;
                m_chipTargetScales[chipArea] = targetScale == Vector3.zero ? Vector3.one : targetScale;
            }

            StopChipScaleRoutine(chipArea);
            m_chipScaleRoutines[chipArea] = StartCoroutine(AnimateChipScaleRoutine(chipArea, chipArea.SelectedVisualEffect.localScale, isSelecting ? targetScale : Vector3.zero, isSelecting ? Mathf.Max(0f, m_chipSelectScaleDuration) : Mathf.Max(0f, m_chipReleaseScaleDuration), isSelecting));
        }

        private IEnumerator AnimateChipScaleRoutine(ChipArea chipArea, Vector3 startScale, Vector3 endScale, float duration, bool isSelecting)
        {
            RectTransform selectedVisualEffect = chipArea.SelectedVisualEffect;
            selectedVisualEffect.gameObject.SetActive(true);

            if (duration <= 0f)
            {
                selectedVisualEffect.localScale = endScale;
                selectedVisualEffect.gameObject.SetActive(isSelecting);
                m_chipScaleRoutines.Remove(chipArea);
                yield break;
            }

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);
                float easedTime = isSelecting ? EaseUtility.EaseOutBack(normalizedTime) : EaseUtility.EaseInBack(normalizedTime);
                selectedVisualEffect.localScale = Vector3.LerpUnclamped(startScale, endScale, easedTime);
                yield return null;
            }

            selectedVisualEffect.localScale = endScale;
            selectedVisualEffect.gameObject.SetActive(isSelecting);
            m_chipScaleRoutines.Remove(chipArea);
        }

        private void StopChipScaleRoutine(ChipArea chipArea)
        {
            if (!m_chipScaleRoutines.TryGetValue(chipArea, out Coroutine routine) || routine == null)
            {
                return;
            }

            StopCoroutine(routine);
            m_chipScaleRoutines.Remove(chipArea);
        }

        private void StopAllChipScaleRoutines()
        {
            foreach (Coroutine routine in m_chipScaleRoutines.Values)
            {
                if (routine == null)
                {
                    continue;
                }

                StopCoroutine(routine);
            }

            m_chipScaleRoutines.Clear();
        }

        private void OnBetAreaClicked(string betAreaId)
        {
            BetAreaPressed?.Invoke(betAreaId);
        }

        private void OnChipAreaClicked(string obj)
        {
            ChipAreaPressed?.Invoke(obj);
        }
    }
}
