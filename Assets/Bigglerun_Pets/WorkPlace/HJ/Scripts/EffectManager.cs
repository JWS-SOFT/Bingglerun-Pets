using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public List<GameObject> effectPrefabs;

    #region Singleton
    public static EffectManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public GameObject PlayEffect(GameObject effectPrefab, Vector3 position)
    {
        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

        return effect;
    }

    public GameObject PlayEffect(GameObject effectPrefab, Transform transform)
    {
        GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);

        return effect;
    }
}
