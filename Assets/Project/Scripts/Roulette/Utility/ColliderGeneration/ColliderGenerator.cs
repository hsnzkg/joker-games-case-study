using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Roulette.Utility.ColliderGeneration
{
    public class ColliderGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform m_center;

        [Header("Circle Settings")]
        [SerializeField] private int m_count = 8;
        [SerializeField] private float m_radius = 5f;
        [SerializeField] private Vector3 m_offset = Vector3.zero;

        [Header("Sphere Settings")]
        [SerializeField] private float m_sphereScale = 1f;
        [SerializeField] private string m_sphereNamePrefix = "CircleSphere";

        public Transform Center => m_center;
        public int Count => m_count;
        public float Radius => m_radius;
        public Vector3 Offset => m_offset;
        public float SphereScale => m_sphereScale;
        public string SphereNamePrefix => m_sphereNamePrefix;

#if UNITY_EDITOR
        public void Generate()
        {
            if (m_center == null)
            {
                Debug.LogError("Center transform is not assigned.", this);
                return;
            }

            ClearChildren();

            if (m_count <= 0)
            {
                Debug.LogWarning("Count must be greater than 0.", this);
                return;
            }

            Vector3 centerPosition = m_center.position + m_offset;
            float angleStep = 360f / m_count;

            for (int i = 0; i < m_count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;

                Vector3 position = centerPosition + new Vector3(
                    Mathf.Cos(angle) * m_radius,
                    0f,
                    Mathf.Sin(angle) * m_radius
                );

                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"{m_sphereNamePrefix}_{i}";
                Undo.RegisterCreatedObjectUndo(sphere, "Create Circle Sphere");

                sphere.transform.SetParent(transform);
                sphere.transform.position = position;
                sphere.transform.localScale = Vector3.one * m_sphereScale;
            }
        }

        public void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_center == null)
                return;

            Vector3 centerPosition = m_center.position + m_offset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPosition, 0.2f);

            Gizmos.color = Color.cyan;
            const int segments = 64;
            Vector3 prevPoint = centerPosition + new Vector3(m_radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = t * Mathf.PI * 2f;

                Vector3 nextPoint = centerPosition + new Vector3(
                    Mathf.Cos(angle) * m_radius,
                    0f,
                    Mathf.Sin(angle) * m_radius
                );

                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
#endif
    }
}