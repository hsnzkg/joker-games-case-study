using System;
using System.Collections;
using Project.Scripts.EventBus;
using Project.Scripts.EventBus.Events.GUI;
using Project.Scripts.EventBus.Events.Replay;
using Unity.Cinemachine;
using UnityEngine;

namespace Project.Scripts.Camera
{
    public class CameraFocusController
    {
        private readonly GameCamera m_owner;
        private readonly CinemachineCamera m_camera;
        private readonly Transform m_betFocusPoint;
        private readonly Transform m_rouletteFocusPoint;
        private readonly float m_cameraFocusDuration;

        private Coroutine m_focusRoutine;
        
        private EventBind<EPlayPress> m_playPressedBind;
        private EventBind<EReplayEnd>  m_replayEndedBind;

        public event Action<FocusType> FocusComplete;

        public CameraFocusController(GameCamera owner, CinemachineCamera camera, Transform betFocusPoint, Transform rouletteFocusPoint, float cameraFocusDuration)
        {
            m_owner = owner;
            m_camera = camera;
            m_betFocusPoint = betFocusPoint;
            m_rouletteFocusPoint = rouletteFocusPoint;
            m_cameraFocusDuration = Mathf.Max(0f, cameraFocusDuration);
        }

        public void Initialize()
        {
            m_playPressedBind = new EventBind<EPlayPress>(OnPlayPressed);
            m_replayEndedBind = new EventBind<EReplayEnd>(OnReplayEnded);
            SnapTo(m_betFocusPoint);
        }

        public void Enable()
        {
            EventBus<EPlayPress>.Register(m_playPressedBind);
            EventBus<EReplayEnd>.Register(m_replayEndedBind);
        }

        public void Disable()
        {
            EventBus<EPlayPress>.Unregister(m_playPressedBind);
            EventBus<EReplayEnd>.Unregister(m_replayEndedBind);
            StopFocusRoutine();
        }

        private void OnPlayPressed()
        {
         
        }

        private void OnReplayEnded()
        {
            FocusTo(FocusType.Bet);
        }

        public void FocusTo(FocusType focusType)
        {
            Transform targetFocusPoint = focusType == FocusType.Bet ? m_betFocusPoint : m_rouletteFocusPoint;
            
            StopFocusRoutine();

            if (m_camera == null || targetFocusPoint == null)
            {
                FocusComplete?.Invoke(focusType);
                return;
            }

            if (m_cameraFocusDuration <= 0f)
            {
                SnapTo(targetFocusPoint);
                FocusComplete?.Invoke(focusType);
                return;
            }

            m_focusRoutine = m_owner.StartCoroutine(FocusRoutine(focusType));
        }

        private IEnumerator FocusRoutine(FocusType focusType)
        {
            Transform targetFocusPoint = focusType == FocusType.Bet ? m_betFocusPoint : m_rouletteFocusPoint;
            Vector3 startPosition = m_camera.transform.position;
            Quaternion startRotation = m_camera.transform.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < m_cameraFocusDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / m_cameraFocusDuration);
                ApplyCameraPose(Vector3.Lerp(startPosition, targetFocusPoint.position, t), Quaternion.Slerp(startRotation, targetFocusPoint.rotation, t));
                yield return null;
            }

            ApplyCameraPose(targetFocusPoint.position, targetFocusPoint.rotation);
            m_focusRoutine = null;

            FocusComplete?.Invoke(focusType);
        }

        private void SnapTo(Transform targetFocusPoint)
        {
            if (m_camera == null || targetFocusPoint == null) return;
            ApplyCameraPose(targetFocusPoint.position, targetFocusPoint.rotation);
        }

        private void ApplyCameraPose(Vector3 position, Quaternion rotation)
        {
            m_camera.ForceCameraPosition(position, rotation);
        }

        private void StopFocusRoutine()
        {
            if (m_focusRoutine == null || m_owner == null)
            {
                return;
            }

            m_owner.StopCoroutine(m_focusRoutine);
            m_focusRoutine = null;
        }
    }
}