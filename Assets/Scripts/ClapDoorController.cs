using System.Linq;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class ClapDoorController : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clapSound;  // Only one audio source
    [SerializeField]
    GameObject openedDoor;
    [SerializeField] SimpleFPSPlayer playerController;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject roomTrigger;
    AudioSource clapAudioSource;


    private void Awake()
    {
        clapAudioSource = GetComponent<AudioSource>();
    }
    //private void Update()
    //{
    //    // If one clap happened and time passed → resolve
    //    if (clapCount > 0 && Time.time - firstClapTime > maxDelayBetweenClaps)
    //    {
    //        ResolveClaps();
    //    }
    //}

    // 🔹 Call this method from LEFT button
    public void LeftButtonClap()
    {
        StartCoroutine(OnClap(1));
    }

    // 🔹 Call this method from RIGHT button
    public void RightButtonClap()
    {
        StartCoroutine(OnClap(2));
    }

    // 🔹 Handles the actual clap logic
     IEnumerator OnClap(int times)
    {
        // Play clap sound (one audio source)
        if (clapAudioSource != null)
        {
            clapAudioSource.PlayOneShot(clapSound, 0.8f);
            if (times == 2)
            {
                yield return new WaitForSeconds(0.3f);
                clapAudioSource.PlayOneShot(clapSound, 0.8f);
            }
            openedDoor.SetActive(false); // Open the door
            panel.SetActive(false);
            playerController.enabled = true;
            Destroy(roomTrigger);
        }
    }


}
