using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DoorInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float interactDistance = 3f;
    public LayerMask doorLayer;
    public LayerMask doorLoadLayer;
    public KeyCode interactKey = KeyCode.E;

    [Header("Door Animation Settings")]
    public float openAngle = 90f;
    public float speed = 4f;

    [Header("Scene Settings")]
    public string nextSceneName;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip areaEntrySound;  // Sound to play when entering trigger area

    private Camera cam;
    private bool hasPlayedAreaSound = false;
    private HashSet<Transform> openedDoors = new HashSet<Transform>();
    private Dictionary<Transform, bool> animatingDoors = new Dictionary<Transform, bool>();

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        HandleRaycast();
    }

    void HandleRaycast()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, doorLayer | doorLoadLayer))
        {
            if (hit.collider.CompareTag("Door") && Input.GetKeyDown(interactKey))
            {
                Transform doorTransform = hit.collider.transform;

                // Check if this specific door hasn't been opened yet
                if (!openedDoors.Contains(doorTransform) && !IsAnimating(doorTransform))
                {
                    bool isLoadDoor = IsInLayerMask(hit.collider.gameObject.layer, doorLoadLayer);
                    OpenDoor(doorTransform, isLoadDoor);
                }
            }
        }
    }

    void OpenDoor(Transform door, bool isLoadDoor)
    {
        openedDoors.Add(door);
        
        Quaternion closedRotation = door.rotation;
        Quaternion openRotation = Quaternion.Euler(door.eulerAngles + new Vector3(0, openAngle, 0));

        StartCoroutine(AnimateDoor(door, openRotation, isLoadDoor));
    }

    System.Collections.IEnumerator AnimateDoor(Transform door, Quaternion targetRot, bool isLoadDoor)
    {
        animatingDoors[door] = true;

        while (Quaternion.Angle(door.rotation, targetRot) > 0.1f)
        {
            door.rotation = Quaternion.Slerp(door.rotation, targetRot, Time.deltaTime * speed);
            yield return null;
        }

        door.rotation = targetRot;
        animatingDoors[door] = false;

        // If it's a DoorLoad layer → play sound then load scene
        if (isLoadDoor)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }

    bool IsAnimating(Transform door)
    {
        return animatingDoors.ContainsKey(door) && animatingDoors[door];
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return mask == (mask | (1 << layer));
    }

    // Trigger detection for area entry sound
    void OnTriggerEnter(Collider other)
    {
        // Play sound once when player enters the area
        if (!hasPlayedAreaSound && areaEntrySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(areaEntrySound);
            hasPlayedAreaSound = true;
        }
    }
}