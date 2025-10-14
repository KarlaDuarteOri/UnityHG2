using UnityEngine;

namespace Scripts.SafeZone
{
    /// <summary>
    /// Optional helper to scale a cylindrical mesh so it matches the SafeZoneController radius.
    /// Attach this to the wall mesh (e.g., ProBuilder tube) and assign the controller reference.
    /// </summary>
    public class SafeZoneWallVisual : MonoBehaviour
    {
        [SerializeField] private SafeZoneController controller;
        [SerializeField] private float wallHeight = 30f;
        [SerializeField] private Vector3 baseScale = Vector3.one;

        private Transform cachedTransform;

        private void Awake()
        {
            cachedTransform = transform;
            if (controller == null)
            {
                controller = GetComponentInParent<SafeZoneController>();
            }
        }

        private void LateUpdate()
        {
            if (controller == null)
            {
                return;
            }

            float radius = Mathf.Max(0.01f, controller.CurrentRadius);
            Vector3 targetScale = baseScale;
            targetScale.x = radius * 2f / Mathf.Max(0.01f, baseScale.x);
            targetScale.z = radius * 2f / Mathf.Max(0.01f, baseScale.z);
            targetScale.y = wallHeight / Mathf.Max(0.01f, baseScale.y);

            cachedTransform.localScale = targetScale;
            cachedTransform.position = controller.CurrentCenter + new Vector3(0f, wallHeight * 0.5f, 0f);
        }
    }
}
