using UnityEngine;

namespace Flipit.CityTerrain
{
    /// <summary>
    /// Adds a silhouette effect to characters that renders them visible through walls.
    /// Uses a second material pass with ZTest Greater to draw behind occluding geometry.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class CharacterSilhouette : MonoBehaviour
    {
        [SerializeField] private Color _silhouetteColor = new Color(1f, 1f, 1f, 0.5f);
        
        private Material _silhouetteMaterial;
        private MeshRenderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer == null) return;

            // Create silhouette material that renders behind geometry
            _silhouetteMaterial = new Material(Shader.Find("Flipit/CharacterSilhouette"));
            if (_silhouetteMaterial.shader.name == "Hidden/InternalErrorShader")
            {
                // Fallback: use a simple unlit shader with ZTest Greater
                _silhouetteMaterial = CreateFallbackSilhouetteMaterial();
            }
            _silhouetteMaterial.color = _silhouetteColor;

            // Add silhouette material as additional material on the renderer
            var mats = _renderer.sharedMaterials;
            var newMats = new Material[mats.Length + 1];
            for (int i = 0; i < mats.Length; i++)
                newMats[i] = mats[i];
            newMats[mats.Length] = _silhouetteMaterial;
            _renderer.sharedMaterials = newMats;
        }

        private Material CreateFallbackSilhouetteMaterial()
        {
            // Create a material that always renders behind other objects
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetFloat("_Surface", 1); // Transparent
            mat.SetFloat("_Blend", 0); // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Greater);
            mat.renderQueue = 3100; // After transparent
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            return mat;
        }

        public void SetSilhouetteColor(Color color)
        {
            _silhouetteColor = color;
            if (_silhouetteMaterial != null)
                _silhouetteMaterial.color = color;
        }
    }
}
