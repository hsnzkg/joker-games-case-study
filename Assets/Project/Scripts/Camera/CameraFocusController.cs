using Project.Scripts.Event;
using Project.Scripts.Event.Events.Camera;
using Project.Scripts.Event.Events.GUI;
using Project.Scripts.Event.Events.Replay;
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
            SnapTo(m_betFocusPoint);
        }

        public void Enable()
        {
            EventBus.Subscribe<EPlayPress>(OnPlayPressed);
            EventBus.Subscribe<EReplayEnd>(OnReplayEnded);
        }

        public void Disable()
        {
            EventBus.Unsubscribe<EPlayPress>(OnPlayPressed);
            EventBus.Unsubscribe<EReplayEnd>(OnReplayEnded);
            StopFocusRoutine();
        }

        private void OnPlayPressed()
        {
            FocusTo(m_rouletteFocusPoint, true);
        }

        private void OnReplayEnded()
        {
            FocusTo(m_betFocusPoint, false);
        }

        private void FocusTo(Transform targetFocusPoint, bool publishPlayStarted)
        {
            StopFocusRoutine();

            if (m_camera == null || targetFocusPoint == null)
            {
                if (publishPlayStarted)
                {
                    EventBus.Publish<ECameraFocusEnd>();
                }

                return;
            }

            if (m_cameraFocusDuration <= 0f)
            {
                SnapTo(targetFocusPoint);

                if (publishPlayStarted)
                {
                    EventBus.Publish<ECameraFocusEnd>();
                }

                return;
            }

            m_focusRoutine = m_owner.StartCoroutine(FocusRoutine(targetFocusPoint, publishPlayStarted));
        }

        private System.Collections.IEnumerator FocusRoutine(Transform targetFocusPoint, bool publishPlayStarted)
        {
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

            if (publishPlayStarted)
            {
                EventBus.Publish<ECameraFocusEnd>();
            }
        }

        private void SnapTo(Transform targetFocusPoint)
        {
            if (m_camera == null || targetFocusPoint == null)
            {
                return;
            }

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