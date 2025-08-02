using UnityEngine;

public class GUIManager : MonoBehaviour
{
    public static GUIManager Instance { get; private set; }

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

    public Page[] pages;
    private Page activePage;

    private void Start()
    {
        activePage = pages[0];
        SetPage(activePage.name);
    }

    public Page GetPage(string name)
    {
        foreach (Page p in pages)
        {
            if (p.name == name)
            {
                return p;
            }
        }
        Debug.LogError($"Unable to find page of name: {name}");
        return null;
    }

    public void SetPage(string name)
    {
        activePage.pageObject.SetActive(false);
        activePage = GetPage(name);
        activePage.pageObject.SetActive(true);
    }
}

[System.Serializable]
public class Page
{
    public string name;
    public GameObject pageObject;
}