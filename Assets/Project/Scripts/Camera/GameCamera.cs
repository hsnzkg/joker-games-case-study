using UnityEngine;

namespace Project.Scripts.Camera
{
    [RequireComponent(typeof(Unity.Cinemachine.CinemachineCamera))]
    public class GameCamera : MonoBehaviour
    {
        [SerializeField] private Transform m_betFocusPoint;
        [SerializeField] private Transform m_rouletteFocusPoint;
        [SerializeField] private float m_cameraFocusDuration = 1f;

        public CameraFocusController CameraFocusController { get; private set; }

        private void OnEnable()
        {
            CameraFocusController?.Enable();
        }

        private void OnDisable()
        {
            CameraFocusController?.Disable();
        }

        public void Initialize()
        {
            Unity.Cinemachine.CinemachineCamera cinemachineCamera = GetComponent<Unity.Cinemachine.CinemachineCamera>();
            CameraFocusController = new CameraFocusController(this, cinemachineCamera, m_betFocusPoint, m_rouletteFocusPoint, m_cameraFocusDuration);
            CameraFocusController.Initialize();
        }
    }
}