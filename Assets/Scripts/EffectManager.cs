using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }

    [SerializeField] public GameObject[] effects;

    public void SpawnEffect(int effect, Vector2 pos)
    {
        Destroy(Instantiate(effects[effect], pos, Quaternion.identity), 5f);
    }
}
