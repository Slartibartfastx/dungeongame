public enum Orientation
{
    north,
    east,
    south,
    west,
    none
}

public enum GameState
{
    GameInitialization,       // gameStarted
    InLevelPlay,              // playingLevel
    EncounteringEnemies,      // engagingEnemies
    BossBattlePreparation,    // bossStage
    FacingBoss,               // engagingBoss
    LevelSuccess,             // levelCompleted
    VictoryAchieved,          // gameWon
    DefeatEncountered,        // gameLost
    GameplayPaused,           // gamePaused
    ViewingDungeonMap,        // dungeonOverviewMap
    ResettingGame             // restartGame
}

public enum AimDir
{
    Up,
    Down,
    Right,
    Left,
    UpRight,
    UpLeft
}