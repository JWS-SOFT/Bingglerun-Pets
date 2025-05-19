using UnityEngine;

public class ItemEffectTest : MonoBehaviour
{
    [SerializeField] private string testItem = "item001";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerManager.Instance.InitializeLife();
        PlayerManager.Instance.InitializeSkillCount();

        ItemManager.Instance.AddUsableItems("item001", 2);
        ItemManager.Instance.AddUsableItems("item002", 2);
        ItemManager.Instance.AddUsableItems("item003", 5);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ItemManager.Instance.UseUsableItem(testItem);
    }
}
