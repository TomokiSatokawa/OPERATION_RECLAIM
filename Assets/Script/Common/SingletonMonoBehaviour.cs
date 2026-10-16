using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }

        if (Instance != this)
        {
            Debug.LogError($"{typeof(T).Name} Ç™2Ç¬à»è„ë∂ç›ÇµÇ‹Ç∑");
            Destroy(this);
        }
    }
    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public static void Depose()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            Instance = null;
        }
    }
}
