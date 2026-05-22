using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneTrigger : MonoBehaviour
{
    [Header("Переход")]
    [Tooltip("Имя сцены (должна быть в Build Settings)")]
    [SerializeField] private string targetScene;

    [Tooltip("ID точки спавна в целевой сцене")]
    [SerializeField] private string spawnPointId = "default";

    [Header("Фильтр")]
    [SerializeField] private string playerTag = "Player";

    [Header("Что сохранять")]
    [SerializeField] private bool saveInventory = true;
    [SerializeField] private bool saveGraph = true;
    [SerializeField] private bool saveHP = true;

    private bool _triggered = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        DoTransition(other.gameObject);
    }

    private void DoTransition(GameObject playerObject)
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[SceneTrigger] GameStateManager не найден, создаём");
            var go = new GameObject("GameStateManager");
            go.AddComponent<GameStateManager>();
            gsm = GameStateManager.Instance;
        }

        Debug.Log("[ScenceTrigger] spawn id: " + spawnPointId);
        gsm.LastSpawnPointId = spawnPointId;

        Debug.Log("[ScenceTrigger] last id: " + gsm.LastSpawnPointId);

        if (saveHP)
        {
            Debug.Log($"[SceneTrigger] HP сохранён: {gsm.PlayerHP}");
        }

        if (saveInventory)
        {
            var inv = Object.FindAnyObjectByType<InventoryManager>();
            if (inv != null)
                gsm.SaveInventory(inv);
            else
                Debug.LogWarning("[SceneTrigger] InventoryManager не найден");
        }

        if (saveGraph)
        {
            gsm.SaveAllSpellSlotsState();
        }

        var caster = Object.FindAnyObjectByType<SpellCaster>();
        if (caster != null && caster.CurrentSpellNode != null)
            gsm.SaveSpell(caster.CurrentSpellNode);

        Debug.Log($"[SceneTrigger] Переход в '{targetScene}', спавн: '{spawnPointId}'");
        SceneManager.LoadScene(targetScene);
       
        
    }
}