using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSway : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private Vector2 swayStrength;
    [SerializeField] private float swaySpeed;

    [Header("SerializeField")]
    [SerializeField] private float nudge = 0f;
    [SerializeField] private Vector2 nudgeDir;

    public void Nudge(Vector2 dir, float time)
    {
        nudgeDir = dir;
        nudge = time;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            if (nudge > 0)
            {
                nudge -= Time.deltaTime;
                if (nudge < 0)
                {
                    nudge = 0;
                }
            }
            cam.localPosition = Vector3.Lerp(cam.localPosition, (Vector3)nudgeDir * nudge + new Vector3(0, 0, cam.localPosition.z), Time.deltaTime);
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
