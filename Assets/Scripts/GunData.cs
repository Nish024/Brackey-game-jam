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

    [Header("Manufacturer & Logos")]
    [Tooltip("All possible manufacturer logos for this gun model.")]
    public Sprite[] manufacturerLogos;

    [Tooltip("The matching manufacturer name for each logo (must be same length as Manufacturer Logos).")]
    public string[] manufacturerNames;

    [Tooltip("Which index in the above arrays is the REAL/LEGITIMATE manufacturer.")]
    public int legitimateLogoIndex = 0;

    [Header("Rarity Info")]
    public ItemRarity actualRarity = ItemRarity.Good;
    public ItemRarity claimedRarity = ItemRarity.Good;

    [Header("Pricing")]
    public float minAskPrice = 500f;
    public float maxAskPrice = 900f;

    [Tooltip("The true, honest base value of this item if legit.")]
    public float baseValue = 500f;
}
