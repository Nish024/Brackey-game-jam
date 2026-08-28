using UnityEngine;
using TMPro;

/// <summary>
/// Attached to the Clipboard prefab. 
/// Holds references to the 3D TextMeshPro objects on the clipboard so they can be filled with data.
/// </summary>
public class ClipboardDataDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text gunNameText;
    [SerializeField] private TMP_Text serialNumberText;
    [SerializeField] private TMP_Text caliberText;
    [SerializeField] private TMP_Text barrelLengthText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text manufacturerText;

    public void Populate(GunData data)
    {
        if (data == null) return;

        if (gunNameText != null) gunNameText.text = $"Model: {data.gunModelName}";
        if (serialNumberText != null) serialNumberText.text = $"Serial: {data.serialNumber}";
        if (caliberText != null) caliberText.text = $"Caliber: {data.caliber}";
        if (barrelLengthText != null) barrelLengthText.text = $"Barrel: {data.barrelLength}";
        if (yearText != null) yearText.text = $"Year: {data.yearManufactured}";
        if (manufacturerText != null) manufacturerText.text = $"Mfr: {data.claimedManufacturer}";
    }
}
