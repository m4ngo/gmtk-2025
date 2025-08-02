using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private bool sin = true;
    [SerializeField] private Vector2 dir;

    private void Update()
    {
        transform.localPosition = dir * (sin ? Mathf.Sin(Time.time * speed) : Mathf.Cos(Time.time * speed));
    }
}
