using UnityEngine;

namespace Scripts.SafeZone
{
    [RequireComponent(typeof(LineRenderer))]
    public class SafeZoneVisual : MonoBehaviour
    {
        private static Material sharedDefaultMaterial;

        [SerializeField] [Min(8)] private int circleSegments = 120;
        [SerializeField] private LineRenderer lineRenderer;

        private Color currentColor = Color.blue;
        private float currentWidth = 0.3f;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            ConfigureRenderer();
        }

        public void Configure(Color color, float width)
        {
            currentColor = color;
            currentWidth = Mathf.Max(0f, width);
            ConfigureRenderer();
        }

        public void Draw(Vector3 center, float radius, float heightOffset)
        {
            if (lineRenderer == null)
            {
                return;
            }

            if (radius <= Mathf.Epsilon)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            float y = center.y + heightOffset;
            EnsureCapacity();

            float fullCircle = Mathf.PI * 2f;
            for (int i = 0; i < circleSegments; i++)
            {
                float angle = fullCircle * i / circleSegments;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                Vector3 point = new Vector3(center.x + cos * radius, y, center.z + sin * radius);
                lineRenderer.SetPosition(i, point);
            }

            lineRenderer.loop = true;
            lineRenderer.enabled = true;
        }

        private void ConfigureRenderer()
        {
            if (lineRenderer == null)
            {
                return;
            }

            EnsureMaterial();
            lineRenderer.useWorldSpace = true;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = circleSegments;
            lineRenderer.widthMultiplier = currentWidth;
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth;
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            // Only set enabled state during play mode to avoid OnBecameInvisible warnings in OnValidate
            if (Application.isPlaying)
            {
                lineRenderer.enabled = false;
            }
        }

        private void EnsureCapacity()
        {
            if (lineRenderer.positionCount != circleSegments)
            {
                lineRenderer.positionCount = circleSegments;
            }
        }

        private void OnValidate()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            circleSegments = Mathf.Max(8, circleSegments);
            ConfigureRenderer();
        }

        private void EnsureMaterial()
        {
            if (lineRenderer == null || lineRenderer.sharedMaterial != null)
            {
                return;
            }

            if (sharedDefaultMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                {
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                }

                if (shader != null)
                {
                    sharedDefaultMaterial = new Material(shader)
                    {
                        name = "SafeZoneLine_Material"
                    };
                }
            }

            if (sharedDefaultMaterial != null)
            {
                lineRenderer.sharedMaterial = sharedDefaultMaterial;
            }
        }
    }
}
