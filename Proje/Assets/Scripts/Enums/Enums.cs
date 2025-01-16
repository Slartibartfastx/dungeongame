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

public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

public enum ChestState
{
    closed,
    healthItem,
    ammoItem,
    weaponItem,
    empty
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