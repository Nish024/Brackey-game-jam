using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Data", menuName = "GunShop/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("Basic Info")]
    public string gunModelName = "Pistol 1";
    public string serialNumber = "SN-123456789";
    public string caliber = "9mm";
    public string barrelLength = "4.5 inches";
    public string yearManufactured = "2004";

    [Header("Manufacturer & Logo")]
    public string legitimateManufacturer = "Steyr Arms";
    public string claimedManufacturer = "Steyr Arms";
    [Tooltip("The logo texture to apply to the gun's grip/body.")]
    public Texture2D logoTexture;

    [Header("State Info")]
    public GunState actualState = GunState.Legit;
    public ItemRarity actualRarity = ItemRarity.Good;
    public ItemRarity claimedRarity = ItemRarity.Good;

    [Header("Pricing")]
    public float minAskPrice = 500f;
    public float maxAskPrice = 900f;
    
    [Tooltip("The true, honest base value of this item if legit.")]
    public float baseValue = 500f;
}
