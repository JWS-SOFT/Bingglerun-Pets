using UnityEngine;

public class ItemEffectTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerManager.Instance.InitializeLife();
        PlayerManager.Instance.InitializeSkillCount();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerManager.Instance.StartBooster(3f);
        }
    }
}
