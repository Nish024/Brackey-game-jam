using UnityEngine;

/// <summary>
/// Attach this script to the Customer prefab along with a Collider.
/// Fires GameEvents when the mouse hovers over the customer.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CustomerHover : MonoBehaviour
{
    private void OnMouseEnter()
    {
        GameEvents.OnCustomerHoverEnter?.Invoke();
    }

    private void OnMouseExit()
    {
        GameEvents.OnCustomerHoverExit?.Invoke();
    }
}
