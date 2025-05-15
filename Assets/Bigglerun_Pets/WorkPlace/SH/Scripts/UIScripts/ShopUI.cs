using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    [SerializeField] private Transform[] mainTab;

    public GameObject itemPrefab;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    [SerializeField] private List<Transform> accessaryItemList = new List<Transform>();
    [SerializeField] private Button itemButton;
    [SerializeField] private int selectedItemIndex;

    [SerializeField] private ConfirmUI confirmUI;

    private void Awake()
    {
        playerData = PlayerDataManager.Instance;
    }

    private void OnEnable()
    {
        SetData();
    }

    public void SetData()
    {
        playerGold.text = playerData.CurrentPlayerData.gold.ToString();
        playerCash.text = playerData.CurrentPlayerData.diamond.ToString();
    }

    private void UpdateData()
    {
        playerGold.text = playerData.CurrentPlayerData.gold.ToString();
        playerCash.text = playerData.CurrentPlayerData.diamond.ToString();
    }

    public void SubTabSwitch(int index)
    {
        Debug.Log($"SubTab {index}로 변경");
    }

    public void MainTabSwitch(int index)
    {
        switch (index)
        {
            case 0:
                mainTab[0].gameObject.SetActive(true);
                mainTab[1].gameObject.SetActive(false);
                break;
            case 1:
                mainTab[0].gameObject.SetActive(false);
                mainTab[1].gameObject.SetActive(true);
                break;
        }
    }

    public void ItemSelect()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;

        // 부모인 Content Transform
        Transform parent = clickedObj.transform.parent;

        // 클릭된 버튼이 Content에서 몇 번째 자식인지 찾기
        selectedItemIndex = clickedObj.transform.GetSiblingIndex();

        Debug.Log("클릭된 버튼의 인덱스: " + selectedItemIndex);
        confirmUI.decoItemData = parent.GetChild(selectedItemIndex).GetComponent<ShItem>().decoItemData;
    }

}
