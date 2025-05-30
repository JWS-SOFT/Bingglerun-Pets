using UnityEditor;
using UnityEngine;

public class FirebaseDefineSymbols
{
    [MenuItem("Tools/Notice System/Enable Firebase (Realtime DB)")]
    public static void EnableFirebase()
    {
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        
        if (!currentDefines.Contains("FIREBASE_ENABLED"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                currentDefines + ";FIREBASE_ENABLED"
            );
            
            Debug.Log("FIREBASE_ENABLED 심볼이 추가되었습니다. (Realtime Database 모드)");
            Debug.Log("Firebase Realtime Database SDK가 설치되어 있는지 확인하세요!");
        }
        else
        {
            Debug.Log("FIREBASE_ENABLED 심볼이 이미 존재합니다.");
        }
    }
    
    [MenuItem("Tools/Notice System/Disable Firebase")]
    public static void DisableFirebase()
    {
        string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        
        if (currentDefines.Contains("FIREBASE_ENABLED"))
        {
            currentDefines = currentDefines.Replace("FIREBASE_ENABLED", "").Replace(";;", ";");
            if (currentDefines.EndsWith(";"))
                currentDefines = currentDefines.Substring(0, currentDefines.Length - 1);
            if (currentDefines.StartsWith(";"))
                currentDefines = currentDefines.Substring(1);
                
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                currentDefines
            );
            
            Debug.Log("FIREBASE_ENABLED 심볼이 제거되었습니다. 테스트 모드로 전환됩니다.");
        }
        else
        {
            Debug.Log("FIREBASE_ENABLED 심볼이 존재하지 않습니다.");
        }
    }
    
    [MenuItem("Tools/Notice System/Test Notice Data")]
    public static void CreateTestNoticeData()
    {
        Debug.Log("=== 테스트용 Realtime Database 구조 ===");
        Debug.Log(@"
{
  ""notices"": {
    ""notice_001"": {
      ""type"": ""notice"",
      ""title"": ""게임 업데이트 안내"",
      ""content"": ""새로운 캐릭터와 스테이지가 추가되었습니다."",
      ""timestamp"": ""2024-05-29T10:00:00Z"",
      ""isActive"": true,
      ""priority"": 1
    },
    ""update_001"": {
      ""type"": ""update"", 
      ""title"": ""버그 수정 업데이트"",
      ""content"": ""게임 안정성이 개선되었습니다."",
      ""timestamp"": ""2024-05-29T15:30:00Z"",
      ""isActive"": true,
      ""priority"": 2
    },
    ""event_001"": {
      ""type"": ""event"",
      ""title"": ""특별 이벤트 진행중"",
      ""content"": ""기간 한정 보상 이벤트가 진행 중입니다."",
      ""timestamp"": ""2024-05-29T18:00:00Z"",
      ""isActive"": true,
      ""priority"": 1  
    }
  }
}");
        Debug.Log("Firebase Console에서 위 구조로 데이터를 추가하세요!");
    }
}