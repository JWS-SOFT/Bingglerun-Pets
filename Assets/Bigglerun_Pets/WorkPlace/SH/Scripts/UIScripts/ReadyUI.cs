using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReadyUI : MonoBehaviour
{
    [SerializeField] private List<Toggle> itemToggles; // 각 아이템에 연결된 Toggle
    [SerializeField] private List<string> itemNames;   // 토글과 순서 일치하는 아이템 이름들

    public string SelectedItemName { get; private set; } = null;

    private void Start()
    {
        for (int i = 0; i < itemToggles.Count; i++)
        {
            int index = i;
            itemToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
        }
    }
    
    //활성화 될 때 게임 스테이트 변경
    private void OnEnable()
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.CompetitiveSetup);
    }

    //비활성화 될 때 게임 스테이트 변경
    private void OnDisable()
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }

    private void OnToggleChanged(int changedIndex, bool isOn)
    {
        if (isOn)
        {
            // 다른 토글은 해제
            for (int i = 0; i < itemToggles.Count; i++)
            {
                if (i != changedIndex)
                    itemToggles[i].isOn = false;
            }

            SelectedItemName = itemNames[changedIndex];
        }
        else
        {
            // 선택 해제된 경우: 아무 것도 선택 안 됨
            if (IsAllToggleOff())
                SelectedItemName = null;
        }
        ItemManager.Instance.SelectPreGameItem(SelectedItemName);

        Debug.Log("선택된 아이템: " + (SelectedItemName ?? "없음"));
    }

    private bool IsAllToggleOff()
    {
        foreach (var toggle in itemToggles)
        {
            if (toggle.isOn)
                return false;
        }
        return true;
    }
}
