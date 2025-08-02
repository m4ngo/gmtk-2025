using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSway : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Vector2 swayStrength;
    [SerializeField] private float swaySpeed;

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            cam.localPosition = new Vector3(0, 0, cam.localPosition.z);
            return;
        }

        Vector2 mousePos = Input.mousePosition;
        Vector2 scaledPos = new Vector2(
            Mathf.Clamp((mousePos.x / Screen.width - 0.5f) * 2f, -1f, 1f), 
            Mathf.Clamp((mousePos.y / Screen.height - 0.5f) * 2f, -1f, 1f)
            );
        cam.localPosition = Vector3.Lerp(cam.localPosition, (Vector3)(swayStrength * scaledPos) + new Vector3(0, 0, cam.localPosition.z), Time.deltaTime * swaySpeed);
    }
}
