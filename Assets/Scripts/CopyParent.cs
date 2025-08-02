using UnityEngine;

public class CopyParent : MonoBehaviour
{
    private LineRenderer line;
    private LineRenderer childLine;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        childLine = transform.GetChild(0).GetComponent<LineRenderer>();
    }

    public void UpdateChild()
    {
        Vector3[] pos = new Vector3[line.positionCount];
        line.GetPositions(pos);
        childLine.positionCount = line.positionCount;
        childLine.SetPositions(pos);
    }
}
