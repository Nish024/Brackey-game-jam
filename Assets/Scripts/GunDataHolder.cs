using UnityEngine;

/// <summary>
/// Attached to the root of a Gun Prefab.
/// Holds the GunData (ScriptableObject) for this specific variant.
/// Automatically applies the logo texture when spawned.
/// </summary>
public class GunDataHolder : MonoBehaviour
{
    [SerializeField] private GunData data;
    
    [Header("Logo Swapping")]
    [Tooltip("The renderer that displays the logo.")]
    [SerializeField] private MeshRenderer logoRenderer;
    [Tooltip("The material index on the renderer that holds the logo.")]
    [SerializeField] private int logoMaterialIndex = 0;

    public GunData Data => data;

    void Start()
    {
        ApplyLogo();
    }

    private void ApplyLogo()
    {
        if (data == null || data.logoTexture == null) return;
        if (logoRenderer == null) return;

        // Apply the texture to the specific material slot
        var materials = logoRenderer.materials;
        if (logoMaterialIndex >= 0 && logoMaterialIndex < materials.Length)
        {
            materials[logoMaterialIndex].mainTexture = data.logoTexture;
            logoRenderer.materials = materials; // Reassign to apply changes
        }
    }
}
