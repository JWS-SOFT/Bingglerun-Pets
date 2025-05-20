public enum GameState
{
    None,
    Init,
    Title,
    Lobby,
    // ModeSelect,
    StoryStageSelect,
    CompetitiveSetup,
    // InGame,  // 기존 인게임 상태 제거
    StoryInGame,      // 스토리 인게임 모드
    CompetitionInGame, // 경쟁 인게임 모드
    Result,
    Pause,
    Loading
}