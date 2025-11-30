using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 4f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(AnimateDoor());
    }

    private System.Collections.IEnumerator AnimateDoor()
    {
        Quaternion targetRot = isOpen ? openRotation : closedRotation;

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * speed);
            yield return null;
        }
    }
}
