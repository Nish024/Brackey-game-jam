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

    void Awake()
    {
        ResolveVariant();
    }

    private void ResolveVariant()
    {
        if (data == null) return;
        
        ResolvedState = data.gunState;

        // If the gun is supposed to be Fake AND has the LogoChange reason, we swap the logo to a fake one.
        // Otherwise, we do nothing and let the prefab keep its original legitimate logo!
        if (ResolvedState == GunState.Fake && data.fakeReason.HasFlag(FakeReason.LogoChange))
        {
            if (data.fakeLogoIndices != null && data.fakeLogoIndices.Length > 0)
            {
                int picked = data.fakeLogoIndices[Random.Range(0, data.fakeLogoIndices.Length)];
                
                if (data.manufacturerLogos != null && picked < data.manufacturerLogos.Length)
                {
                    ApplyLogo(data.manufacturerLogos[picked]);
                    Debug.Log($"[GunDataHolder] '{data.gunModelName}' resolved variant → State: {ResolvedState}, Swapped to Fake Logo: #{picked}");
                }
            }
        }
        else
        {
            Debug.Log($"[GunDataHolder] '{data.gunModelName}' resolved variant → State: {ResolvedState}, Kept original prefab logo");
        }
    }

    private void ApplyLogo(Sprite logo)
    {
        if (logo == null || logoRenderer == null) return;
        logoRenderer.sprite = logo;
    }
}
