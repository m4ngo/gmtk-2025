using System.Collections.Generic;
using UnityEngine;

public class SnakeRenderer : MonoBehaviour
{
    [Header("Graphics")]
    [SerializeField] private Transform head;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject bodyTemplate;
    [SerializeField] private List<LineRenderer> body = new List<LineRenderer>();
    private ParticleSystem winParticles;

    private float shakeSpeed = 100f;
    private float currentShake = 0f;
    private Vector2 direction = Vector2.zero;
    private Vector2 curPos;

    private void Start()
    {
        winParticles = head.GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (currentShake > 0)
        {
            parent.position = (Mathf.Sin(Time.time * shakeSpeed) > 0 ? 0.02f : -0.02f) * direction;
            head.position = curPos + direction * (currentShake);
            currentShake -= Time.deltaTime;
            if (currentShake <= 0)
            {
                currentShake = 0;
                parent.position = Vector2.zero;
                head.position = body[body.Count - 1].GetPosition(body[body.Count - 1].positionCount - 1);
            }
        }
    }

    public void Shake(Vector2 dir)
    {
        direction = dir;
        currentShake = 0.1f;
        CouldntMoveSound();
    }

    private void CouldntMoveSound()
    {
        SoundManager.Instance.Play(2, 0.1f, true);
        SoundManager.Instance.Play(3, 0.1f, true);
    }

    public void SetPositions(List<Vector2Int> input, Vector2Int dir)
    {
        currentShake = 0f;
        foreach (LineRenderer l in body)
        {
            Destroy(l.gameObject);
        }
        body.Clear();

        List<Vector3> points = new List<Vector3>();
        Vector2Int prev = input[0];
        foreach (Vector2Int v in input)
        {
            points.Add((Vector2)v);
            if (Vector2.SqrMagnitude(v - prev) > 1.2f || body.Count <= 0)
            {
                if (body.Count > 0)
                {
                    LineRenderer last = body[body.Count - 1];
                    if (last.positionCount == 1)
                    {
                        last.positionCount = last.positionCount + 1;
                        last.SetPosition(1, last.GetPosition(0));
                    }
                }
                body.Add(Instantiate(bodyTemplate, parent).GetComponent<LineRenderer>());
            }

            LineRenderer curBody = body[body.Count - 1];
            curBody.positionCount = curBody.positionCount + 1;
            curBody.SetPosition(curBody.positionCount - 1, (Vector2)v);
            prev = v;
        }

        foreach (LineRenderer r in body)
        {
            r.GetComponent<CopyParent>().UpdateChild();
        }

        //body[body.Count - 1].positionCount = points.Count;
        //body[body.Count - 1].SetPositions(points.ToArray());

        Vector2 lastPoint = points[points.Count - 1];
        head.position = lastPoint;
        curPos = lastPoint;
        // Set head to face the direction of the last 2 points
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        head.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    public void Bite(Vector2Int pos)
    {
        head.GetChild(0).position = (Vector2)pos;
        winParticles.Play();
    }
}
