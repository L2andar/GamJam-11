using UnityEngine;
using System.Collections;
public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject guide;
    [SerializeField] SimpleFPSPlayer playerController;

    private void Start()
    {
        playerController.enabled = false;
    }
    IEnumerator ClosePlayerGuide()
    {
        yield return new WaitForSeconds(2f);
        if (guide != null)
        {
            guide.SetActive(false);
        }   
    }

    public void HandlePlay()
    {
        if (guide != null)
        {
            guide.SetActive(false);
        }
        playerController.enabled = true;
    }

}
