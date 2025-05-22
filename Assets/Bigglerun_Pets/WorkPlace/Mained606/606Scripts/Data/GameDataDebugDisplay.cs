using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameDataDebugDisplay : MonoBehaviour
{
    private Vector2 scrollPosition;
    private Rect windowRect = new Rect(10, 10, 450, 600); // 기본 크기
    private bool showItems = true;
    private bool showStages = true;
    private bool showDecorations = true;
    private bool showCharacters = true;
    
    // 창 상태 관련 변수
    private bool isCollapsed = false;
    private Rect originalWindowRect;
    private bool isResizing = false;
    private Vector2 resizeStartPos;
    private Rect resizeStartRect;
    
    // 최소/최대 창 크기
    private readonly float minWindowWidth = 200f;
    private readonly float minWindowHeight = 100f;
    private readonly float maxWindowWidth = 800f;
    private readonly float maxWindowHeight = 1000f;
    
    // 리사이징 영역 크기
    private readonly float resizeHandleSize = 20f;
    
    // 최소화 버튼 상수
    private readonly float collapsedHeight = 40f;
    private readonly float buttonWidth = 40f;
    private readonly float buttonHeight = 30f;

    void Start()
    {
        // 초기 설정
        originalWindowRect = windowRect;
    }

    void OnGUI()
    {
        if (!Application.isEditor) return;

        // 어두운 배경과 검은색 텍스트로 디버그 윈도우 생성
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.textColor = Color.black;
        windowStyle.fontSize = 24; // 창 제목 글씨 크기 키움
        
        // 창 표시
        windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, isCollapsed ? "게임 데이터 디버그 ▼" : "게임 데이터 디버그 ▲", windowStyle);
        
        // 리사이징 처리
        HandleResizing();
    }
    
    void DrawDebugWindow(int windowID)
    {
        // 모든 텍스트를 검은색으로 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.black;
        labelStyle.fontSize = 30; // 글씨 크기 키움 (두 배 이상)
        
        GUIStyle headerStyle = new GUIStyle(labelStyle);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 36; // 헤더 글씨 크기 키움 (두 배 이상)
        
        GUIStyle subHeaderStyle = new GUIStyle(labelStyle);
        subHeaderStyle.fontStyle = FontStyle.Bold;
        subHeaderStyle.fontSize = 32; // 서브헤더 글씨 크기 키움 (두 배 이상)
        
        // 접기/펼치기 버튼
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.textColor = Color.black;
        buttonStyle.fontSize = 24; // 버튼 글씨 크기 키움
        
        // 접기/펼치기 버튼 배치
        Rect toggleButtonRect;
        
        if (isCollapsed)
        {
            // 접힌 상태에서는 버튼을 창 가운데 위치에 배치
            toggleButtonRect = new Rect(windowRect.width / 2 - buttonWidth / 2, 5, buttonWidth, buttonHeight);
            
            if (GUI.Button(toggleButtonRect, "▼", buttonStyle))
            {
                ToggleCollapse();
                // 강제로 상태 변경
                isCollapsed = false;
                // 원래 크기로 복원
                windowRect = new Rect(
                    windowRect.x,
                    windowRect.y,
                    originalWindowRect.width,
                    originalWindowRect.height
                );
            }
            
            // 드래그 가능하도록
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 40));
            return;
        }
        else
        {
            // 펼쳐진 상태에서는 우측 상단에 배치
            toggleButtonRect = new Rect(windowRect.width - 50, 5, 40, 30);
            if (GUI.Button(toggleButtonRect, "▲", buttonStyle))
            {
                ToggleCollapse();
            }
        }
        
        // 스크롤 시작
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(windowRect.width - 20), GUILayout.Height(windowRect.height - 50));
        
        // 로그인 상태
        GUILayout.Label("로그인 정보", headerStyle);
        GUILayout.Label($"로그인: {(FirebaseManager.Instance?.IsAuthenticated == true ? "완료" : "안됨")}", labelStyle);
        
        if (FirebaseManager.Instance?.IsAuthenticated == true)
        {
            GUILayout.Label($"사용자 ID: {FirebaseManager.Instance.UserId}", labelStyle);
            GUILayout.Label($"로그인 유형: {FirebaseManager.Instance.CurrentLoginType}", labelStyle);
            GUILayout.Label($"이메일: {FirebaseManager.Instance.UserEmail ?? "없음"}", labelStyle);
        }
        
        GUILayout.Space(10);
        
        // 플레이어 데이터
        if (PlayerDataManager.Instance?.CurrentPlayerData != null)
        {
            var data = PlayerDataManager.Instance.CurrentPlayerData;
            
            // 기본 정보 섹션
            GUILayout.Label("기본 정보", headerStyle);
            GUILayout.Label($"플레이어 ID: {data.playerId}", labelStyle);
            GUILayout.Label($"닉네임: {data.nickname}", labelStyle);
            GUILayout.Label($"레벨: {data.level} (경험치: {data.experience})", labelStyle);
            GUILayout.Label($"마지막 업데이트: {UnixTimeToDateTime(data.lastUpdateTimestamp)}", labelStyle);
            
            GUILayout.Space(5);
            
            // 재화 정보
            GUILayout.Label("재화 정보", headerStyle);
            GUILayout.Label($"골드: {data.gold}", labelStyle);
            GUILayout.Label($"다이아몬드: {data.diamond}", labelStyle);
            GUILayout.Label($"하트: {data.heart}", labelStyle);
            
            GUILayout.Space(5);
            
            // 통계 데이터
            GUILayout.Label("통계", headerStyle);
            GUILayout.Label($"총 플레이 횟수: {data.totalPlayCount}", labelStyle);
            GUILayout.Label($"경쟁 모드 최고 스코어: {data.competitiveBestScore}", labelStyle);
            GUILayout.Label($"최고 스테이지: {data.highestStage}", labelStyle);
            GUILayout.Label($"총 별 개수: {data.totalStars}", labelStyle);
            GUILayout.Label($"총 수집 코인: {data.totalCoinsCollected}", labelStyle);
            
            GUILayout.Space(5);
            
            // 아이템 정보 (펼침/접기 가능)
            showItems = GUILayout.Toggle(showItems, "아이템 정보 (펼치기/접기)", subHeaderStyle);
            if (showItems && data.items != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"보유 아이템 수: {data.items.Count}개", labelStyle);
                
                if (data.items.Count > 0)
                {
                    foreach (var item in data.items)
                    {
                        GUILayout.Label($"{item.Key}: {item.Value}개", labelStyle);
                    }
                }
                else
                {
                    GUILayout.Label("보유 아이템 없음", labelStyle);
                }
                
                GUILayout.Label($"선택된 시작 아이템: {(string.IsNullOrEmpty(data.selectedPreGameItem) ? "없음" : data.selectedPreGameItem)}", labelStyle);
                GUILayout.EndVertical();
            }
            
            GUILayout.Space(5);
            
            // 장착 아이템
            GUILayout.Label("장착 아이템", headerStyle);
            GUILayout.Label($"모자: {(string.IsNullOrEmpty(data.equippedHat) ? "없음" : data.equippedHat)}", labelStyle);
            GUILayout.Label($"옷: {(string.IsNullOrEmpty(data.equippedBody) ? "없음" : data.equippedBody)}", labelStyle);
            GUILayout.Label($"신발: {(string.IsNullOrEmpty(data.equippedShoes) ? "없음" : data.equippedShoes)}", labelStyle);
            
            GUILayout.Space(5);
            
            // 스테이지 정보 (펼침/접기 가능)
            showStages = GUILayout.Toggle(showStages, "스테이지 정보 (펼치기/접기)", subHeaderStyle);
            if (showStages && data.storyStages != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"진행된 스테이지 수: {data.storyStages.Count}개", labelStyle);
                
                if (data.storyStages.Count > 0)
                {
                    foreach (var stage in data.storyStages)
                    {
                        string lastPlayed = stage.Value.completedTimestamp > 0 
                            ? UnixTimeToDateTime(stage.Value.completedTimestamp)
                            : "미완료";
                            
                        GUILayout.Label($"스테이지 {stage.Key}: ★{stage.Value.stars}/3 | 최고점수: {stage.Value.highScore} | 마지막 플레이: {lastPlayed}", labelStyle);
                    }
                }
                else
                {
                    GUILayout.Label("진행된 스테이지 없음", labelStyle);
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.Space(5);
            
            // 해금된 장식 (펼침/접기 가능)
            showDecorations = GUILayout.Toggle(showDecorations, "해금된 장식 (펼치기/접기)", subHeaderStyle);
            if (showDecorations && data.unlockedDecorations != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"해금된 장식 수: {data.unlockedDecorations.Count}개", labelStyle);
                
                if (data.unlockedDecorations.Count > 0)
                {
                    foreach (var decoration in data.unlockedDecorations)
                    {
                        GUILayout.Label(decoration, labelStyle);
                    }
                }
                else
                {
                    GUILayout.Label("해금된 장식 없음", labelStyle);
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.Space(5);
            
            // 캐릭터 정보 (펼침/접기 가능)
            showCharacters = GUILayout.Toggle(showCharacters, "캐릭터 정보 (펼치기/접기)", subHeaderStyle);
            if (showCharacters && data.unlockedCharacters != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"해금된 캐릭터 수: {data.unlockedCharacters.Count}개", labelStyle);
                GUILayout.Label($"현재 선택된 캐릭터: {data.currentCharacter}", labelStyle);
                
                if (data.unlockedCharacters.Count > 0)
                {
                    foreach (var character in data.unlockedCharacters)
                    {
                        GUILayout.Label(character, labelStyle);
                    }
                }
                else
                {
                    GUILayout.Label("해금된 캐릭터 없음", labelStyle);
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.Space(5);
            
            // 게임 설정
            GUILayout.Label("게임 설정", headerStyle);
            GUILayout.Label($"사운드: {(data.soundEnabled ? "켜짐" : "꺼짐")}", labelStyle);
            GUILayout.Label($"진동: {(data.vibrationEnabled ? "켜짐" : "꺼짐")}", labelStyle);
        }
        else
        {
            GUILayout.Label("플레이어 데이터 없음", labelStyle);
            GUIStyle bigButtonStyle = new GUIStyle(GUI.skin.button);
            bigButtonStyle.fontSize = 28; // 버튼 글씨 크기 키움
            if (GUILayout.Button("테스트 데이터 생성", bigButtonStyle))
            {
                CreateTestData();
            }
        }
        
        GUILayout.EndScrollView();
        
        // 우측 하단에 리사이징 핸들 표시
        GUIStyle resizeStyle = new GUIStyle();
        resizeStyle.normal.textColor = Color.gray;
        resizeStyle.fontSize = 30; // 리사이징 핸들 크기 키움
        GUI.Label(new Rect(windowRect.width - 26, windowRect.height - 26, 26, 26), "◢", resizeStyle);
        
        // 창 드래그 가능하도록
        GUI.DragWindow(new Rect(0, 0, windowRect.width - 50, 40));
    }
    
    // 창 접기/펼치기 토글
    private void ToggleCollapse()
    {
        // Debug.Log("토글 호출됨: " + (isCollapsed ? "펼치기" : "접기"));
        
        if (!isCollapsed)
        {
            // 접힐 때 기존 크기 저장
            originalWindowRect = new Rect(windowRect);
            // 너비는 유지하고 높이만 변경
            windowRect.height = collapsedHeight;
            isCollapsed = true;
        }
        else
        {
            // 펼쳐질 때 원래 크기로 완전히 복원 (너비와 높이 모두)
            windowRect = new Rect(
                windowRect.x,
                windowRect.y,
                originalWindowRect.width,
                originalWindowRect.height
            );
            isCollapsed = false;
        }
    }
    
    // 리사이징 처리
    private void HandleResizing()
    {
        // 마우스 위치 가져오기
        Vector2 mousePos = Event.current.mousePosition;
        
        // 리사이징 영역 (우측 하단)
        Rect resizeHandle = new Rect(
            windowRect.x + windowRect.width - resizeHandleSize,
            windowRect.y + windowRect.height - resizeHandleSize,
            resizeHandleSize,
            resizeHandleSize
        );
        
        // 접힌 상태에서는 리사이징 불가능
        if (isCollapsed) return;
        
        // 이벤트 처리
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                // 마우스가 리사이징 핸들 영역 안에 있으면 리사이징 시작
                if (resizeHandle.Contains(new Vector2(mousePos.x, mousePos.y)))
                {
                    isResizing = true;
                    resizeStartPos = mousePos;
                    resizeStartRect = windowRect;
                    Event.current.Use();
                }
                break;
                
            case EventType.MouseUp:
                // 리사이징 종료
                if (isResizing)
                {
                    isResizing = false;
                    Event.current.Use();
                }
                break;
                
            case EventType.MouseDrag:
                // 리사이징 중이면 창 크기 조정
                if (isResizing)
                {
                    float newWidth = Mathf.Clamp(resizeStartRect.width + (mousePos.x - resizeStartPos.x), minWindowWidth, maxWindowWidth);
                    float newHeight = Mathf.Clamp(resizeStartRect.height + (mousePos.y - resizeStartPos.y), minWindowHeight, maxWindowHeight);
                    
                    windowRect.width = newWidth;
                    windowRect.height = newHeight;
                    Event.current.Use();
                }
                break;
        }
    }
    
    // Unix 타임스탬프를 가독성 있는 날짜로 변환
    private string UnixTimeToDateTime(long unixTimeMs)
    {
        if (unixTimeMs <= 0) return "없음";
        
        System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dateTime = dateTime.AddMilliseconds(unixTimeMs).ToLocalTime();
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    // 테스트용 데이터 생성 버튼 클릭 시
    private async void CreateTestData()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("로그인이 필요합니다.");
            return;
        }
        
        if (PlayerDataManager.Instance != null)
        {
            await PlayerDataManager.Instance.LoadPlayerDataAsync(FirebaseManager.Instance.UserId);
        }
    }
} 