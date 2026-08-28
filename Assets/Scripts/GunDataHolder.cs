using UnityEngine;

/// <summary>
/// Attached to the root of a Gun Prefab.
/// At runtime, randomly picks one logo from GunData's logo array.
/// If the picked logo is NOT the legitimate one, the gun is automatically Fake.
/// </summary>
public class GunDataHolder : MonoBehaviour
{
    [SerializeField] private GunData data;

    [Header("Logo Swapping")]
    [Tooltip("The SpriteRenderer that displays the logo on the grip.")]
    [SerializeField] private SpriteRenderer logoRenderer;

    // ── Resolved at runtime ──
    public GunData Data => data;
    public GunState ResolvedState { get; private set; }
    public string SelectedManufacturerName { get; private set; }

    void Awake()
    {
        ResolveVariant();
    }

    private void ResolveVariant()
    {
        if (data == null) return;
        if (data.manufacturerLogos == null || data.manufacturerLogos.Length == 0) return;

        // Randomly pick one logo from the array
        int picked = Random.Range(0, data.manufacturerLogos.Length);

        // Determine state: if it's not the legit logo, it's fake
        ResolvedState = (picked == data.legitimateLogoIndex) ? GunState.Legit : GunState.Fake;

        // Resolve manufacturer name
        if (data.manufacturerNames != null && picked < data.manufacturerNames.Length)
            SelectedManufacturerName = data.manufacturerNames[picked];
        else
            SelectedManufacturerName = "Unknown";

        // Apply the picked logo texture to the mesh
        ApplyLogo(data.manufacturerLogos[picked]);

        Debug.Log($"[GunDataHolder] '{data.gunModelName}' spawned with logo #{picked} " +
                  $"({SelectedManufacturerName}) → State: {ResolvedState}");
    }

    private void ApplyLogo(Sprite logo)
    {
        if (logo == null || logoRenderer == null) return;
        logoRenderer.sprite = logo;
    }
}
