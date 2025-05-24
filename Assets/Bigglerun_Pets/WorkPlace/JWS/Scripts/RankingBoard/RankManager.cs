using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RankManager : MonoBehaviour
{
    [SerializeField] private GameObject rankListPosition;
    [SerializeField] private GameObject[] rankListPrefab;
    [SerializeField] private int maxRankList = 50;
    [SerializeField] private List<RankingList> rankingList = new List<RankingList>();
    private bool isRefreshing = false;
    private List<PlayerData> allLeaderboardData = new List<PlayerData>();

    private void OnEnable()
    {
        gameObject.transform.localScale = new Vector3(0.75f, 0.75f, 1);
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
                RankingList rankList = Instantiate(prefabObject, rankListPosition.transform).transform.GetComponent<RankingList>();
                string displayName = allLeaderboardData[i].competitiveBestCharacter == null ? "None" : allLeaderboardData[i].competitiveBestCharacter;
                rankList.SetRankList(allLeaderboardData[i].competitiveBestScore.ToString(), displayName, i);
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

    private void SetRankBoard()
    {
        for (int i = 0; i < maxRankList; i++)
        {
            GameObject prefabObject = rankListPrefab[3];
            if (i < 3) prefabObject = rankListPrefab[i];
            else prefabObject = rankListPrefab[3];
            RankingList rankList = Instantiate(prefabObject, rankListPosition.transform).transform.GetComponent<RankingList>();
            rankingList.Add(rankList);
        }
    }
}
