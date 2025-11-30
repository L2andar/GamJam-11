using UnityEngine;

public class ShowPanelOnTrigger : MonoBehaviour
{
    [Header("Assign the UI Panel")]
    public GameObject panel;
    [Header("General")]
    [SerializeField]
    SimpleFPSPlayer playerController = null;
    [SerializeField]
    DoorSound doorSound;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);   // Hide by default
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panel != null)
            {
                panel.SetActive(true);   // Show UI when player enters
                playerController.enabled = false; // Disable player controls
                doorSound.PlayDoorSound();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panel != null)
                panel.SetActive(false);  // Hide UI when player leaves
        }
    }
}
