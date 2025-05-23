using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class LeaderboardUIController : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;              // 스크롤 뷰의 Content 오브젝트
    public GameObject leaderboardItemPrefab;     // 리더보드 항목 프리팹 (없으면 기존 항목 복제)
    
    [Header("Settings")]
    public int maxDisplayEntries = 50;           // 표시할 최대 항목 수
    public bool autoRefresh = true;              // 자동 새로고침 여부
    public float refreshInterval = 30f;          // 새로고침 간격 (초)

    [Header("Loading UI")]
    public GameObject loadingPanel;              // 로딩 패널
    public TextMeshProUGUI loadingText;          // 로딩 텍스트

    // 리더보드 항목 UI 요소들
    private List<LeaderboardItemUI> leaderboardItems = new List<LeaderboardItemUI>();
    private bool isRefreshing = false;

    // 리더보드 항목 UI 구조를 위한 클래스
    [System.Serializable]
    public class LeaderboardItemUI
    {
        public GameObject itemObject;
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI levelText;        // 레벨 표시용 (Server 대신 사용)
        public TextMeshProUGUI characterText;    // 캐릭터 정보 표시용 (추가)
        public Image backgroundImage;
        
        // 별 표시용 (있으면) - 총 별 개수 표시
        public GameObject[] stars;
    }

    private void Start()
    {
        InitializeUI();
        
        if (autoRefresh)
        {
            StartCoroutine(AutoRefreshCoroutine());
        }
    }

    private void OnEnable()
    {
        // UI가 활성화될 때마다 리더보드 새로고침
        RefreshLeaderboard();
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // Content Parent가 설정되지 않았다면 자동으로 찾기
        if (contentParent == null)
        {
            Transform scrollView = transform.Find("Category/Scroll View");
            if (scrollView != null)
            {
                contentParent = scrollView.Find("Viewport/Content");
            }
        }

        // 기존 리더보드 항목들 찾기 및 설정
        SetupExistingItems();
    }

    /// <summary>
    /// 기존 UI 항목들을 리더보드 항목으로 설정
    /// </summary>
    private void SetupExistingItems()
    {
        if (contentParent == null) return;

        // 기존 List 항목들 찾기
        for (int i = 0; i < contentParent.childCount; i++)
        {
            Transform child = contentParent.GetChild(i);
            if (child.name.StartsWith("List"))
            {
                LeaderboardItemUI item = CreateLeaderboardItemFromTransform(child);
                if (item != null)
                {
                    leaderboardItems.Add(item);
                }
            }
        }

        Debug.Log($"[LeaderboardUIController] 기존 UI 항목 {leaderboardItems.Count}개 설정 완료");
    }

    /// <summary>
    /// Transform에서 LeaderboardItemUI 생성
    /// </summary>
    private LeaderboardItemUI CreateLeaderboardItemFromTransform(Transform itemTransform)
    {
        LeaderboardItemUI item = new LeaderboardItemUI();
        item.itemObject = itemTransform.gameObject;

        // 자식 요소들에서 UI 컴포넌트 찾기
        item.rankText = FindTextComponent(itemTransform, "Rank");
        
        // User 오브젝트를 찾아서 그 자식에서 닉네임 텍스트 찾기
        Transform userTransform = FindChildSafe(itemTransform, "User");
        if (userTransform != null)
        {
            // User 오브젝트의 자식에서 닉네임 텍스트 컴포넌트 찾기
            item.nameText = userTransform.GetComponentInChildren<TextMeshProUGUI>();
            
            // 명시적으로 Name이라는 자식이 있는지 확인
            Transform nameChild = userTransform.Find("Name");
            if (nameChild != null)
            {
                TextMeshProUGUI nameComponent = nameChild.GetComponent<TextMeshProUGUI>();
                if (nameComponent != null)
                {
                    item.nameText = nameComponent;
                }
            }
            
            // Level 정보도 User 자식에서 찾기
            Transform levelChild = userTransform.Find("Level");
            if (levelChild != null)
            {
                TextMeshProUGUI levelComponent = levelChild.GetComponent<TextMeshProUGUI>();
                if (levelComponent != null)
                {
                    item.levelText = levelComponent;
                }
            }
        }
        else
        {
            // 기존 방식으로 폴백
            item.nameText = FindTextComponent(itemTransform, "User");
        }
        
        item.scoreText = FindTextComponent(itemTransform, "Score");
        
        // levelText가 아직 설정되지 않았다면 기존 방식으로 찾기
        if (item.levelText == null)
        {
            item.levelText = FindTextComponent(itemTransform, "Server"); // Server 필드를 Level로 재활용
        }
        
        item.characterText = FindTextComponent(itemTransform, "Character"); // 캐릭터 정보 표시용
        
        // 배경 이미지 (옵션)
        item.backgroundImage = itemTransform.GetComponent<Image>();

        Debug.Log($"[LeaderboardUIController] 항목 설정 완료 - Rank: {item.rankText != null}, Name: {item.nameText != null}, Score: {item.scoreText != null}, Level: {item.levelText != null}");

        return item;
    }

    /// <summary>
    /// 특정 이름의 TextMeshProUGUI 컴포넌트 찾기
    /// </summary>
    private TextMeshProUGUI FindTextComponent(Transform parent, string childName)
    {
        Transform child = FindChildSafe(parent, childName);
        
        // 찾은 오브젝트에서 TextMeshProUGUI 컴포넌트 확인
        if (child != null)
        {
            TextMeshProUGUI textComponent = child.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                return textComponent;
            }
            
            // 자식에서 TextMeshProUGUI 컴포넌트를 찾기 (User 오브젝트의 자식인 경우)
            textComponent = child.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                return textComponent;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 안전한 자식 오브젝트 찾기 (순환 참조 방지)
    /// </summary>
    private Transform FindChildSafe(Transform parent, string childName)
    {
        HashSet<Transform> visited = new HashSet<Transform>();
        return FindChildRecursiveWithTracking(parent, childName, visited, 0, 5);
    }

    /// <summary>
    /// 재귀적으로 자식 오브젝트 찾기 (방문 추적 및 깊이 제한)
    /// </summary>
    private Transform FindChildRecursiveWithTracking(Transform parent, string childName, HashSet<Transform> visited, int currentDepth, int maxDepth)
    {
        // 깊이 제한 및 null 체크
        if (currentDepth >= maxDepth || parent == null || visited.Contains(parent))
        {
            return null;
        }

        // 방문 표시
        visited.Add(parent);

        // 직접 자식에서 찾기
        Transform directChild = parent.Find(childName);
        if (directChild != null) 
        {
            return directChild;
        }

        // 자식의 자식에서 찾기
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == null || visited.Contains(child)) continue;
            
            if (child.name == childName)
            {
                return child;
            }
                
            // 재귀 호출
            Transform found = FindChildRecursiveWithTracking(child, childName, visited, currentDepth + 1, maxDepth);
            if (found != null) 
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// 리더보드 새로고침
    /// </summary>
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
        ShowLoading(true);

        try
        {
            // 리더보드 데이터 로드
            var leaderboardData = await LeaderboardManager.Instance.LoadLeaderboardAsync(maxDisplayEntries);
            
            // UI 업데이트
            UpdateLeaderboardUI(leaderboardData);
            
            Debug.Log($"[LeaderboardUIController] 리더보드 업데이트 완료: {leaderboardData.Count}개 항목");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LeaderboardUIController] 리더보드 새로고침 실패: {ex.Message}");
        }
        finally
        {
            // 로딩 UI 숨기기
            ShowLoading(false);
            isRefreshing = false;
        }
    }

    /// <summary>
    /// 리더보드 UI 업데이트
    /// </summary>
    private void UpdateLeaderboardUI(List<PlayerData> playerDataList)
    {
        // 필요한 만큼 항목이 없다면 추가 생성
        while (leaderboardItems.Count < playerDataList.Count && leaderboardItems.Count < maxDisplayEntries)
        {
            CreateNewLeaderboardItem();
        }

        // 모든 항목 비활성화
        foreach (var item in leaderboardItems)
        {
            item.itemObject.SetActive(false);
        }

        // 데이터로 UI 업데이트
        for (int i = 0; i < playerDataList.Count && i < leaderboardItems.Count; i++)
        {
            UpdateLeaderboardItem(leaderboardItems[i], playerDataList[i], i + 1);
            leaderboardItems[i].itemObject.SetActive(true);
        }
    }

    /// <summary>
    /// 개별 리더보드 항목 업데이트
    /// </summary>
    private void UpdateLeaderboardItem(LeaderboardItemUI item, PlayerData playerData, int rank)
    {
        Debug.Log($"[LeaderboardUIController] 리더보드 항목 업데이트 - 순위: {rank}, 플레이어: {playerData.playerId}, 닉네임: {playerData.nickname}, 점수: {playerData.competitiveBestScore}");
        
        // 순위 설정
        if (item.rankText != null)
        {
            item.rankText.text = rank.ToString();
        }

        // 플레이어 이름 설정
        if (item.nameText != null)
        {
            string displayName = !string.IsNullOrEmpty(playerData.nickname) ? playerData.nickname : "Unknown Player";
            item.nameText.text = displayName;
            Debug.Log($"[LeaderboardUIController] 닉네임 설정 완료: {displayName} (컴포넌트: {item.nameText.name})");
        }
        else
        {
            Debug.LogWarning("[LeaderboardUIController] nameText 컴포넌트가 null입니다. User 오브젝트 구조를 확인해주세요.");
        }

        // 점수 설정 (competitiveBestScore)
        if (item.scoreText != null)
        {
            item.scoreText.text = playerData.competitiveBestScore.ToString("N0");
        }

        // 캐릭터 정보 준비
        string characterName = !string.IsNullOrEmpty(playerData.competitiveBestCharacter) 
            ? playerData.competitiveBestCharacter 
            : playerData.currentCharacter ?? "Unknown";

        // 레벨 정보 설정
        if (item.levelText != null)
        {
            // 캐릭터 정보가 별도 UI가 없다면 레벨과 함께 표시
            if (item.characterText == null)
            {
                item.levelText.text = $"Lv.{playerData.level} ({characterName})";
            }
            else
            {
                item.levelText.text = $"Lv.{playerData.level}";
            }
            Debug.Log($"[LeaderboardUIController] 레벨 설정 완료: {item.levelText.text} (컴포넌트: {item.levelText.name})");
        }
        else
        {
            Debug.LogWarning("[LeaderboardUIController] levelText 컴포넌트가 null입니다. User 오브젝트 구조를 확인해주세요.");
        }

        // 캐릭터 정보 설정 (별도 UI가 있는 경우)
        if (item.characterText != null)
        {
            item.characterText.text = characterName;
            Debug.Log($"[LeaderboardUIController] 캐릭터 정보 설정: {characterName}");
        }

        // 별 표시 설정 (총 별 개수를 5개 단위로 표시)
        if (item.stars != null && item.stars.Length > 0)
        {
            int starsToShow = Mathf.Min(item.stars.Length, playerData.totalStars / 10); // 10개당 별 1개
            for (int i = 0; i < item.stars.Length; i++)
            {
                if (item.stars[i] != null)
                {
                    item.stars[i].SetActive(i < starsToShow);
                }
            }
        }

        // 배경 색상 설정 (상위 순위 강조)
        if (item.backgroundImage != null)
        {
            // 현재 플레이어인지 확인
            bool isCurrentPlayer = PlayerDataManager.Instance?.CurrentPlayerData?.playerId == playerData.playerId;
            
            if (isCurrentPlayer)
            {
                item.backgroundImage.color = new Color(0.2f, 0.8f, 0.2f, 0.4f); // 초록색 (본인)
            }
            else
            {
                switch (rank)
                {
                    case 1:
                        item.backgroundImage.color = new Color(1f, 0.8f, 0.2f, 0.3f); // 금색
                        break;
                    case 2:
                        item.backgroundImage.color = new Color(0.7f, 0.7f, 0.7f, 0.3f); // 은색
                        break;
                    case 3:
                        item.backgroundImage.color = new Color(0.8f, 0.5f, 0.2f, 0.3f); // 동색
                        break;
                    default:
                        item.backgroundImage.color = new Color(1f, 1f, 1f, 0.1f); // 기본
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 새로운 리더보드 항목 생성
    /// </summary>
    private void CreateNewLeaderboardItem()
    {
        if (leaderboardItems.Count == 0) return;

        // 첫 번째 항목을 복제
        GameObject newItem = Instantiate(leaderboardItems[0].itemObject, contentParent);
        LeaderboardItemUI newItemUI = CreateLeaderboardItemFromTransform(newItem.transform);
        
        if (newItemUI != null)
        {
            leaderboardItems.Add(newItemUI);
        }
    }

    /// <summary>
    /// 로딩 UI 표시/숨김
    /// </summary>
    private void ShowLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }

        if (loadingText != null && show)
        {
            StartCoroutine(LoadingTextAnimation());
        }
    }

    /// <summary>
    /// 로딩 텍스트 애니메이션
    /// </summary>
    private IEnumerator LoadingTextAnimation()
    {
        string baseText = "리더보드 로딩";
        int dotCount = 0;

        while (loadingPanel != null && loadingPanel.activeInHierarchy)
        {
            if (loadingText != null)
            {
                loadingText.text = baseText + new string('.', dotCount);
            }

            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// 자동 새로고침 코루틴
    /// </summary>
    private IEnumerator AutoRefreshCoroutine()
    {
        while (autoRefresh)
        {
            yield return new WaitForSeconds(refreshInterval);
            
            if (gameObject.activeInHierarchy)
            {
                RefreshLeaderboard();
            }
        }
    }

    /// <summary>
    /// 수동 새로고침 버튼용
    /// </summary>
    public void OnRefreshButtonClicked()
    {
        RefreshLeaderboard();
    }

    /// <summary>
    /// 현재 플레이어 순위 표시용
    /// </summary>
    public async void ShowCurrentPlayerRank()
    {
        if (LeaderboardManager.Instance == null) return;
        
        int rank = await LeaderboardManager.Instance.GetCurrentPlayerRankAsync();
        if (rank > 0)
        {
            Debug.Log($"[LeaderboardUIController] 현재 플레이어 순위: {rank}위");
            // UI에 순위 표시 (필요시 구현)
        }
        else
        {
            Debug.Log("[LeaderboardUIController] 현재 플레이어가 리더보드에 없습니다.");
        }
    }
} 