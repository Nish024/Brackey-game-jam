using UnityEngine;

public enum CountryOfOrigin
{
    UnitedStates,
    Russia,
    China,
    Germany,
    Italy,
    UnitedKingdom,
    Israel,
    Spain,
    SouthKorea
}

[System.Flags]
public enum FakeReason
{
    None = 0,
    FakeData = 1 << 0,
    PhysicalModification = 1 << 1,
    LogoChange = 1 << 2
}

[CreateAssetMenu(fileName = "New Gun Data", menuName = "GunShop/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Basic Info")]
    public string gunModelName = "Pistol 1";
    public string serialNumber = "SN-123456789";
    public CountryOfOrigin madeIn = CountryOfOrigin.UnitedStates;
    
    [UnityEngine.Serialization.FormerlySerializedAs("barrelLength")]
    public string overallLength = "11 cm";
    
    public string yearManufactured = "2004";

    [Header("Gun State")]
    public GunState gunState = GunState.Legit;
    
    [Tooltip("If state is Fake, select the reason(s) why. You can select multiple!")]
    public FakeReason fakeReason = FakeReason.None;

    [Header("Model Variations")]
    [Tooltip("Drag the fake prefab variations of this gun model here.")]
    public GameObject[] fakeVariations;
    
    [Tooltip("Drag the stolen prefab variations of this gun model here (usually just the real prefab).")]
    public GameObject[] stolenVariations;

    [Header("Manufacturer & Logos")]
    [Tooltip("All possible manufacturer logos for this gun model.")]
    public Sprite[] manufacturerLogos;

    [Tooltip("Which indices in the above arrays are FAKE manufacturer logos (picks one at random).")]
    public int[] fakeLogoIndices = new int[] { 1 };

    [Header("Pricing")]
    public float minAskPrice = 500f;
    public float maxAskPrice = 900f;
}
