# 리더보드 시스템 설정 가이드 (PlayerData 기반)

## 📋 개요
기존 PlayerData의 `competitiveBestScore` 필드를 활용하여 Firebase의 players 경로에서 데이터를 읽어와 리더보드를 구성합니다.
별도의 리더보드 데이터베이스나 점수 제출이 필요하지 않습니다.

LeaderboardManager는 GameManager의 컴포넌트로 자동 추가되며, **로비 상태에서만 작동**합니다.

## 1. 필요한 컴포넌트 설정

### LeaderboardUI Prefab 설정
1. `Assets/Bigglerun_Pets/WorkPlace/SH/Prefabs/UI/LeaderboardUI.prefab` 파일을 열기
2. 루트 오브젝트 `LeaderboardUI`에 `LeaderboardUIController` 스크립트 추가
3. LeaderboardUIController 컴포넌트 설정:
   - `Content Parent`: `Category/Scroll View/Viewport/Content` 오브젝트 할당
   - `Max Display Entries`: 50 (기본값)
   - `Auto Refresh`: true (기본값)
   - `Refresh Interval`: 30 (기본값)

### ⚠️ 중요: 별도 설정 불필요
- **LeaderboardManager는 GameManager에 자동으로 추가됩니다**
- 씬에 별도로 LeaderboardManager GameObject를 생성할 필요가 없습니다
- GameManager가 있는 씬에서 자동으로 작동합니다

## 2. 작동 조건

### 🎯 로비 상태에서만 작동
- **GameState.Lobby**일 때만 리더보드 기능이 활성화됩니다
- 다른 게임 상태(인게임, 타이틀 등)에서는 리더보드 기능이 비활성화됩니다
- 상태 변경 시 자동으로 캐시가 무효화됩니다

### 자동 상태 관리
- GameStateMachine이 상태를 변경할 때마다 LeaderboardManager에 알림
- 로비가 아닌 상태로 변경되면 자동으로 캐시 무효화
- 메모리 효율성과 성능 최적화

## 3. Firebase 설정 확인

### Firebase Database 규칙 설정
Firebase Console에서 Realtime Database 규칙을 다음과 같이 설정:

```json
{
  "rules": {
    "players": {
      ".read": true,
      ".write": true
    }
  }
}
```

### 조건부 컴파일 심볼 설정
1. Unity Editor에서 `Edit > Project Settings > Player` 열기
2. `Scripting Define Symbols`에 `FIREBASE_DATABASE` 추가
3. Apply 버튼 클릭

## 4. UI 연결

### 리더보드 버튼 연결
리더보드를 열고 싶은 버튼의 OnClick 이벤트에 다음 중 하나 설정:
- `UIController.OpenLeaderboard()` 메서드 호출
- `UIController.TogglePopup("LeaderboardUI")` 메서드 호출

### 새로고침 버튼 연결 (선택사항)
리더보드 UI 내에 새로고침 버튼이 있다면:
- `LeaderboardUIController.OnRefreshButtonClicked()` 메서드 호출

## 5. 리더보드 데이터 구조

### 사용되는 PlayerData 필드들
- `playerId`: 플레이어 고유 ID
- `nickname`: 플레이어 이름
- `competitiveBestScore`: 경쟁 모드 최고 점수 (리더보드 정렬 기준)
- `level`: 플레이어 레벨
- `totalStars`: 총 별 개수 (UI에서 별 표시용)

### 자동 데이터 업데이트
- 경쟁 모드 게임 종료 시 `PlayerDataManager.UpdateCompetitiveBestScore()` 호출됨
- 자동으로 Firebase의 players 경로에 저장됨
- **별도의 점수 제출 코드 불필요**

## 6. 리더보드 표시 내용

### UI 항목들
- **Rank**: 순위 (1위, 2위, 3위...)
- **User**: 플레이어 닉네임
- **Score**: 경쟁 모드 최고 점수
- **Level**: 플레이어 레벨 (기존 Server 필드 재활용)
- **배경색**: 순위별 색상 구분 + 본인 강조 (초록색)

### 정렬 기준
- `competitiveBestScore` 내림차순 정렬
- 점수가 0인 플레이어는 제외

## 7. 테스트

### Firebase 없이 테스트
1. `FIREBASE_DATABASE` 심볼을 제거하거나 주석 처리
2. **로비 씬에서** 게임 실행 시 더미 데이터로 리더보드가 표시됨

### Firebase와 함께 테스트
1. `FIREBASE_DATABASE` 심볼이 설정되어 있는지 확인
2. Firebase 프로젝트가 올바르게 설정되어 있는지 확인
3. **로비 상태에서** 리더보드 UI 열기
4. 경쟁 모드에서 게임 플레이 후 점수가 자동으로 Firebase에 저장됨
5. 다시 로비로 돌아와서 리더보드에서 실제 데이터 확인

## 8. 문제 해결

### 일반적인 문제들
1. **리더보드가 비어있음**: 
   - **로비 상태인지 확인** (가장 중요!)
   - Firebase 연결 상태 확인
   - competitiveBestScore > 0인 플레이어가 있는지 확인
   - 콘솔 로그 확인

2. **데이터가 표시되지 않음**: 
   - 현재 GameState가 Lobby인지 확인
   - LeaderboardUIController가 올바르게 설정되었는지 확인
   - Content Parent가 올바르게 할당되었는지 확인

3. **"로비 상태가 아니므로 리더보드 기능 비활성화" 로그**: 
   - 정상적인 동작입니다
   - 로비 상태에서만 리더보드를 사용하세요

4. **Firebase 오류**: 
   - 인터넷 연결 및 Firebase 프로젝트 설정 확인
   - Database 규칙 확인

### 디버그 로그 확인
콘솔에서 다음 태그로 시작하는 로그들을 확인:
- `[LeaderboardManager]`
- `[LeaderboardUIController]`
- `[PlayerDataManager]`
- `[GameStateMachine]`

## 9. 추가 기능

### 현재 플레이어 순위 확인 (로비에서만)
```csharp
int rank = await LeaderboardManager.Instance.GetCurrentPlayerRankAsync();
```

### 특정 플레이어 순위 확인 (로비에서만)
```csharp
int rank = await LeaderboardManager.Instance.GetPlayerRankAsync("playerId");
```

### 캐시 무효화 (강제 새로고침)
```csharp
LeaderboardManager.Instance.InvalidateCache();
```

## 10. 주요 변경사항

### ✅ 개선된 점
- **자동 관리**: GameManager에 컴포넌트로 자동 추가
- **상태 기반**: 로비에서만 작동하여 성능 최적화
- **메모리 효율**: 필요할 때만 데이터 로드
- **자동 정리**: 상태 변경 시 자동 캐시 무효화

### 🎯 핵심 특징
- **로비 전용**: GameState.Lobby에서만 작동
- **자동 추가**: 별도 GameObject 생성 불필요
- **상태 연동**: GameStateMachine과 완전 연동
- **성능 최적**: 필요시에만 리소스 사용

## 11. 사용 시나리오

1. **타이틀 → 로비**: 리더보드 기능 활성화
2. **로비에서 리더보드 열기**: 정상 작동
3. **로비 → 인게임**: 리더보드 캐시 자동 무효화
4. **인게임에서 리더보드 시도**: 빈 데이터 반환 (정상)
5. **인게임 → 로비**: 리더보드 기능 재활성화

이제 리더보드는 로비에서만 작동하며 GameManager의 컴포넌트로 자동 관리됩니다! 🎉 