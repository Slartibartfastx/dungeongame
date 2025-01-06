public enum Oriantation
{
    n,
    e,
    s,
    w,
    none


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

public enum GameState
{
    gameStarted,
    playingLvl,
    engagingEnemy,
    bossFight,
    engagingBoss,
    lvlComplete,
    Won,
    Lost,
    Paused,
    dungeonMap,
    restart

}