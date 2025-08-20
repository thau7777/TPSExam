using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<T>();

            return _instance;
        }
    }


    protected virtual void Awake()
    {
        // If another instance exists, destroy this one
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this as T;
    }
}
