using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField] private float spinSpeed;

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}
