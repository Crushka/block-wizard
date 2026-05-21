//using UnityEngine;

//public class Bootstrap : MonoBehaviour
//{
//    public InventoryManager inventory;
//    public NodeEditorManager editor;

//    void Start()
//    {
//        if (inventory != null) inventory.InitInventory();
//        if (editor != null) editor.InitEditor();
//    }
//}

using UnityEngine;

public class SceneBootstrap : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private NodeEditorManager nodeEditorManager;
    [SerializeField] private SpellCaster spellCaster;

    [Header("Настройки")]
    [Tooltip("Восстанавливать граф заклинания из прошлой сцены?")]
    [SerializeField] private bool restoreGraph = true;

    [Tooltip("Восстанавливать заклинание в SpellCaster?")]
    [SerializeField] private bool restoreSpell = true;

    [System.Obsolete]
    void Start()
    {
        var gsm = GameStateManager.Instance;

        if (gsm == null || gsm.SavedGraph == null)
        {
            // Первый запуск — инициализируем как раньше
            inventoryManager?.InitInventory();
            nodeEditorManager?.InitEditor();
            return;
        }

        // Есть сохранённый граф — НЕ вызываем InitEditor()
        // чтобы не создавать лишний None-узел
        inventoryManager?.InitInventory();
        gsm.RestoreGraph();

        if (restoreSpell && spellCaster != null)
            gsm.RestoreSpell(spellCaster);

        // Спавн игрока у нужной точки
        if (gsm != null)
        {
            var spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawnPoints)
            {
                if (sp.spawnId == gsm.LastSpawnPointId)
                {
                    var player = FindAnyObjectByType<PlayerController>();
                    if (player != null)
                        player.transform.position = sp.transform.position;
                    break;
                }
            }
        }
    }
}