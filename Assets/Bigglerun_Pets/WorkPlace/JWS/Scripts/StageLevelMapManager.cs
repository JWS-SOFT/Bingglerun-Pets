using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageLevelMapManager : MonoBehaviour
{
    [SerializeField] private GameObject[] stageImage;
    [SerializeField] private GameObject unLockedPrefab;
    private List<Button> stageLevel = new List<Button>();

    public int testStageLevel = 1;

    private void Awake()
    {
        for (int i = 0; i < stageImage.Length; i++)
        {
            // 자식에 있는 모든 Button 컴포넌트를 한 번에 가져옴
            Button[] bt = stageImage[i].GetComponentsInChildren<Button>();

            // 존재하는 경우 리스트에 추가
            if (bt.Length > 0)
            {
                for (int j = 0; j < bt.Length; j++)
                {
                    stageLevel.Add(bt[j]);
                }
            }
        }

        Debug.Log("버튼수 : " + stageLevel.Count);
    }

    private void Start()
    {
        SetLevelButton(testStageLevel);
    }

    private void SetLevelButton(int currentLevel)
    {
        for (int i = 0; i < currentLevel; i++)
        {
            // 새로운 버튼 UI를 생성해서 원래 버튼의 부모 아래에 붙임
            LevelButtonUI lvbt = Instantiate(unLockedPrefab, stageLevel[i].transform.parent).GetComponent<LevelButtonUI>();
            stageLevel[i].gameObject.SetActive(false);
            lvbt.SetStageLevelSetting(i, i == currentLevel - 1);
        }
    }
}
