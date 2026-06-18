using System;

namespace SaveSystem.Data
{
    /// <summary>
    /// Единая общая структура всех сохраняемых данных игры.
    /// Добавляйте новые поля сюда по мере необходимости.
    /// </summary>
    [Serializable]
    public sealed class GameData
    {
        public PlayerProgressData PlayerProgress;
        public PlayerStateData    PlayerState;

        public GameData()
        {
            PlayerProgress = new PlayerProgressData();
            PlayerState    = new PlayerStateData();
        }
    }

    // -------------------------------------------------------------------------
    // ПРОГРЕСС ИГРОКА
    // Всё, что отражает продвижение по игре и не сбрасывается при смерти.
    // Убитые боссы — часть прогресса игрока.
    // -------------------------------------------------------------------------
    [Serializable]
    public sealed class PlayerProgressData
    {
        // TODO: добавьте сюда поля прогресса, например:
        // public int   CurrentLevel;
        // public bool  BossA_Defeated;
        // public bool  BossB_Defeated;
        // public int   TotalDeaths;
        // public float TotalPlaytimeSeconds;
    }

    // -------------------------------------------------------------------------
    // СОСТОЯНИЕ ИГРОКА
    // Текущие характеристики персонажа в момент сохранения.
    // -------------------------------------------------------------------------
    [Serializable]
    public sealed class PlayerStateData
    {
        // TODO: добавьте сюда поля состояния, например:
        // public float  CurrentHp;
        // public float  MaxHp;
        // public int    CurrentMana;
        // public float  PositionX;
        // public float  PositionY;
        // public float  PositionZ;
        // public string CurrentSceneName;
    }
}
