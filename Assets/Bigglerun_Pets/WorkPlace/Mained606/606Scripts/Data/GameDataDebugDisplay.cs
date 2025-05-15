using UnityEngine;
using System.Collections.Generic;

public class GameDataDebugDisplay : MonoBehaviour
{
    private Vector2 scrollPosition;
    private Rect windowRect = new Rect(10, 10, 450, 600); // 크기를 더 크게 조정
    private bool showDetails = true;
    private bool showItems = true;
    private bool showStages = true;
    private bool showDecorations = true;
    private bool showCharacters = true;

    void OnGUI()
    {
        if (!Application.isEditor) return;

        // 어두운 배경과 검은색 텍스트로 디버그 윈도우 생성
        GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.textColor = Color.black;
        
        windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, "게임 데이터 디버그", windowStyle);
    }
    
    void DrawDebugWindow(int windowID)
    {
        // 모든 텍스트를 검은색으로 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.black;
        labelStyle.fontSize = 14; // 글씨 크기 키움
        
        GUIStyle headerStyle = new GUIStyle(labelStyle);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 16;
        
        GUIStyle subHeaderStyle = new GUIStyle(labelStyle);
        subHeaderStyle.fontStyle = FontStyle.Bold;
        
        // 스크롤 시작
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(430), GUILayout.Height(550));
        
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
            GUILayout.Label($"최고 점수: {data.bestScore}", labelStyle);
            GUILayout.Label($"총 플레이 횟수: {data.totalPlayCount}", labelStyle);
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
            if (GUILayout.Button("테스트 데이터 생성"))
            {
                CreateTestData();
            }
        }
        
        GUILayout.EndScrollView();
        
        // 창 드래그 가능하도록
        GUI.DragWindow();
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
    private void CreateTestData()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("로그인이 필요합니다.");
            return;
        }
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.LoadPlayerDataAsync(FirebaseManager.Instance.UserId);
        }
    }
} 