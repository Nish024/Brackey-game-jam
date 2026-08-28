using UnityEngine;


public class phoneManager : MonoBehaviour
{

    [SerializeField] private GameObject expertPhoneScreen;
    [SerializeField] private GameObject chatScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchToChat()
    {
        chatScreen.SetActive(true);
        expertPhoneScreen.SetActive(false);
    }

    public void SwitchToExpert()
    {
        chatScreen.SetActive(false);
        expertPhoneScreen.SetActive(true);
    }
}
