using UnityEngine;

namespace StickyFabric.Demo
{
    /// <summary>
    /// Tints the object while it is touching anything on the glue layers.
    /// Uses a <see cref="MaterialPropertyBlock"/>, so the shared material is never duplicated.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class GlueContactHighlighter : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private LayerMask glueLayers;
        [SerializeField] private Color gluedColor = new Color(1f, 0.45f, 0.85f);
        [SerializeField] private Color freeColor = Color.white;

        private Renderer cachedRenderer;
        private MaterialPropertyBlock propertyBlock;
        private int glueContacts;

        public bool IsTouchingGlue => glueContacts > 0;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            ApplyColor();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsGlue(collision.gameObject))
            {
                glueContacts++;
                ApplyColor();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (IsGlue(collision.gameObject))
            {
                glueContacts = Mathf.Max(0, glueContacts - 1);
                ApplyColor();
            }
        }

        private bool IsGlue(GameObject other) => (glueLayers.value & (1 << other.layer)) != 0;

        private void ApplyColor()
        {
            Color color = IsTouchingGlue ? gluedColor : freeColor;
            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
