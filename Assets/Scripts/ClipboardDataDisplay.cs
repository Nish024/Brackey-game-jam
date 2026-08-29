using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
/// Attached to the Clipboard prefab. 
/// Holds references to the 3D TextMeshPro objects on the clipboard so they can be filled with data.
/// </summary>
public class ClipboardDataDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text gunNameText;
    [SerializeField] private TMP_Text serialNumberText;
    [FormerlySerializedAs("caliberText")]
    [SerializeField] private TMP_Text madeInText;
    [SerializeField] private TMP_Text barrelLengthText;
    [SerializeField] private TMP_Text yearText;
    public void Populate(GunData data)
    {
        if (data == null) return;

        if (gunNameText != null) gunNameText.text = $"Model: {data.gunModelName}";
        if (serialNumberText != null) serialNumberText.text = $"Serial: {data.serialNumber}";
        if (madeInText != null) madeInText.text = $"Made in: {FormatCountry(data.madeIn)}";
        if (barrelLengthText != null) barrelLengthText.text = $"Length: {data.overallLength}";
        if (yearText != null) yearText.text = $"Year: {data.yearManufactured}";
    }

    private string FormatCountry(CountryOfOrigin country)
    {
        switch (country)
        {
            case CountryOfOrigin.UnitedStates: return "United States";
            case CountryOfOrigin.UnitedKingdom: return "United Kingdom";
            case CountryOfOrigin.SouthKorea: return "South Korea";
            default: return country.ToString();
        }
    }
}
