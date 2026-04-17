using UnityEngine;

namespace Project.Scripts.Camera
{
    [RequireComponent(typeof(Unity.Cinemachine.CinemachineCamera))]
    public class GameCamera : MonoBehaviour
    {
        [SerializeField] private Transform m_betFocusPoint;
        [SerializeField] private Transform m_rouletteFocusPoint;
        [SerializeField] private float m_cameraFocusDuration = 1f;

        private CameraFocusController m_cameraFocusController;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            m_cameraFocusController?.Enable();
        }

        private void OnDisable()
        {
            m_cameraFocusController?.Disable();
        }

        private void Initialize()
        {
            Unity.Cinemachine.CinemachineCamera cinemachineCamera = GetComponent<Unity.Cinemachine.CinemachineCamera>();
            m_cameraFocusController = new CameraFocusController(this, cinemachineCamera, m_betFocusPoint, m_rouletteFocusPoint, m_cameraFocusDuration);
            m_cameraFocusController.Initialize();
        }
    }
}
