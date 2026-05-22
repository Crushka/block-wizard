using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadScreenUI : MonoBehaviour
{
    [Header("Настройки 'Начать сначала'")]
    [SerializeField] private string firstScene = "Level1";
    [SerializeField] private string firstSpawnPointId = "default";

    [Header("Префаб игрока (весь FullPlayer1)")]
    [SerializeField] private GameObject playerPrefab;

    public void StartFromBegin()
    {
        SaveSystem.DeleteSave();

        var gsm = GameStateManager.Instance;
        if (gsm != null)
        {
            gsm.PlayerHP = gsm.PlayerMaxHP;
            gsm.LastSpawnPointId = firstSpawnPointId;
            gsm.SavedInventory.Clear();
            gsm.SpellSlots.Clear();
            gsm.DeadBossIds.Clear();
            gsm.ActiveSpellSlotIndex = 0;
        }

        LoadScene(firstScene);
    }

    public void StartFromCheckpoint()
    {
        var gsm = GameStateManager.Instance;

        if (!SaveSystem.HasSave())
        {
            Debug.LogWarning("[DeadScreenUI] Нет сохранения, загружаем с начала.");
            StartFromBegin();
            return;
        }

        bool ok = SaveSystem.Load(gsm);
        if (!ok)
        {
            Debug.LogError("[DeadScreenUI] Ошибка загрузки.");
            StartFromBegin();
            return;
        }

        if (gsm != null) gsm.PlayerHP = gsm.PlayerMaxHP;

        string scene = SaveSystem.PeekSavedScene();
        if (string.IsNullOrEmpty(scene)) scene = firstScene;

        LoadScene(scene);
    }

    private void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == sceneName)
        {
            gameObject.SetActive(false);
            SpawnPlayer();
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[DeadScreenUI] playerPrefab не назначен!");
            return;
        }

        var gsm = GameStateManager.Instance;
        string targetId = gsm != null ? gsm.LastSpawnPointId : firstSpawnPointId;

        SpawnPoint spawnPoint = FindSpawnPoint(targetId);
        Vector3 pos = spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.transform.rotation : Quaternion.identity;

        GameObject player = Instantiate(playerPrefab, pos, rot);
        Debug.Log($"[DeadScreenUI] Игрок заспавнен на '{targetId}' → {pos}");

        var health = player.GetComponentInChildren<PlayerHealth>();
        if (health != null && gsm != null)
            health.health = gsm.PlayerMaxHP;

        if (health != null)
            health.SetDeadScreen(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        gameObject.SetActive(false);

        var deadCam = GameObject.Find("DeadCamera");
        if (deadCam != null) deadCam.SetActive(false);
    }

    private SpawnPoint FindSpawnPoint(string id)
    {
        var all = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude);
        foreach (var sp in all)
            if (sp.spawnId == id) return sp;

        foreach (var sp in all)
            if (sp.spawnId == "default") return sp;

        return null;
    }
}