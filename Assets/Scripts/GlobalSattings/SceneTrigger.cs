using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTrigger : MonoBehaviour
{
    [Tooltip("Имя сцены для загрузки (должна быть добавлена в Build Settings)")]
    [SerializeField] private string targetScene;

    [Tooltip("Метка игрока для фильтрации")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Сохранять граф заклинания при переходе?")]
    [SerializeField] private bool saveSpellGraph = true;

    [Tooltip("Имя спавна")]
    [SerializeField] private string spawnPointId = "default";

    private bool _triggered = false;

    void Awake()
    {
        // Триггер должен быть IsTrigger = true
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        Transition(other);
    }

    private void Transition(Collider playerCollider)
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogError("[SceneTrigger] GameStateManager не найден на сцене!");
            // Создаём на лету как запасной вариант:
            var go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            gsm = GameStateManager.Instance;
        }


        // 1. Сохраняем позицию игрока
        gsm.PlayerPosition = playerCollider.transform.position;

        gsm.LastSpawnPointId = spawnPointId;

        // 2. Сохраняем инвентарь
        var inv = FindAnyObjectByType<InventoryManager>();
        if (inv != null) gsm.SaveInventory(inv);

        // 3. Сохраняем граф (опционально)
        if (saveSpellGraph) gsm.SaveGraph();

        // 4. Сохраняем скомпилированное заклинание
        var caster = FindAnyObjectByType<SpellCaster>();
        // SpellCaster не хранит NodeBase публично — добавь свойство или
        // получи его через AttackFactory если нужно
        if (caster != null) gsm.SaveSpell(caster.CurrentSpellNode);

        Debug.Log($"[SceneTrigger] → {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}