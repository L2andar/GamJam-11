using UnityEngine;

public class DoorSound : MonoBehaviour
{
    [SerializeField]
    AudioClip doorVoice;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayDoorSound()
    {
        audioSource.PlayOneShot(doorVoice);
    }   
}
