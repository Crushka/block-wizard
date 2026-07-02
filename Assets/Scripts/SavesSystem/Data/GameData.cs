using System;
using System.Collections.Generic;

namespace SaveSystem.Data
{
    [Serializable]
    public sealed class GameData
    {
        public PlayerProgressData PlayerProgress;
        public PlayerStateData PlayerState;
        public GameData()
        {
            PlayerProgress = new PlayerProgressData();
            PlayerState = new PlayerStateData();
        }
    }

    [Serializable]
    public sealed class PlayerProgressData
    {
        public List<string> DeadBossIds = new List<string>();
        public List<int> SavedInventory = new List<int>(); 
        public List<SavedItemSlot> SavedItemSlots = new List<SavedItemSlot>();
        public List<SpellSlot> SpellSlots = new List<SpellSlot>();
    }

    [Serializable]
    public sealed class SavedItemSlot
    {
        public string ItemId;
        public int Count;
        public int SlotIndex;
        public bool IsQuickSlot;
    }

    [Serializable]
    public sealed class PlayerStateData
    {
        public float PlayerHP;
        public float PlayerMaxHP;
        public string LastSpawnPointId;
        public int ActiveSpellSlotIndex;
        public string CurrentSceneName;
    }

   
}