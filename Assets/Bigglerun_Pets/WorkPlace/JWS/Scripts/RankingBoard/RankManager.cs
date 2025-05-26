using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RankManager : MonoBehaviour
{
    [SerializeField] private GameObject rankListPosition;
    [SerializeField] private GameObject[] rankListPrefab;
    [SerializeField] private int maxRankList = 50;
    private bool isRefreshing = false;
    private List<PlayerData> allLeaderboardData = new List<PlayerData>();

    private void OnEnable()
    {
        gameObject.transform.localScale = new Vector3(0.75f, 0.75f, 1);

        ResetRankBoard();
        RefreshLeaderboard();
    }

    public async void RefreshLeaderboard()
    {
        if (isRefreshing) return;
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("[LeaderboardUIController] LeaderboardManager가 없습니다.");
            return;
        }

        isRefreshing = true;

        // 로딩 UI 표시
        // ShowLoading(true);

        try
        {
            // 전체 리더보드 데이터 로드 (필터링 전)
            allLeaderboardData = await LeaderboardManager.Instance.LoadLeaderboardAsync(maxRankList * 3); // 여유있게 로드
            Debug.Log(allLeaderboardData.Count);
            for (int i = 0; i < allLeaderboardData.Count; i++)
            {
                GameObject prefabObject = rankListPrefab[3];
                if (i < 3) prefabObject = rankListPrefab[i];
                else prefabObject = rankListPrefab[3];
                
                string displayName = !string.IsNullOrEmpty(allLeaderboardData[i].nickname) ? allLeaderboardData[i].nickname : "Unknown Player";
                // 캐릭터별 엔트리인지 확인 (playerId에 "_캐릭터명" 형식이 포함된 경우)
                if (allLeaderboardData[i].playerId.Contains("_") && !string.IsNullOrEmpty(allLeaderboardData[i].competitiveBestCharacter))
                {
                    // 캐릭터 정보를 이름과 함께 표시
                    displayName = $"{displayName} ({allLeaderboardData[i].competitiveBestCharacter})";
                }

                bool iscurrentPlayer = PlayerDataManager.Instance.CurrentPlayerData.playerId == allLeaderboardData[i].playerId;

                RankingList rankList = Instantiate(prefabObject, rankListPosition.transform).transform.GetComponent<RankingList>();
                rankList.SetRankList(allLeaderboardData[i].competitiveBestScore.ToString(), displayName, i, iscurrentPlayer);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LeaderboardUIController] 리더보드 새로고침 실패: {ex.Message}");
        }
        finally
        {
            // 로딩 UI 숨기기
            // ShowLoading(false);
            isRefreshing = false;
        }
    }

    private void ResetRankBoard()
    {
        foreach (Transform child in rankListPosition.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
