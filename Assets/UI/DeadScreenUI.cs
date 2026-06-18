using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadScreenUI : MonoBehaviour
{
    public static DeadScreenUI Instance { get; private set; }

    [Header("Настройки 'Начать сначала'")]
    [SerializeField] private string firstScene = "Level1";
    [SerializeField] private string firstSpawnPointId = "default";

    [Header("Ссылки на UI элементы")]
    [SerializeField] private GameObject visualPanel;
    [SerializeField] private Camera deadCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void StartFromBegin()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null)
        {
            // Удаляем файл сохранения через менеджер
            gsm.DeleteSaveFile();

            // Сбрасываем все параметры в памяти на дефолтные
            gsm.PlayerHP = gsm.PlayerMaxHP;
            gsm.LastSpawnPointId = firstSpawnPointId;
            gsm.SavedInventory.Clear();
            gsm.SpellSlots.Clear();
            gsm.DeadBossIds.Clear();
            gsm.ActiveSpellSlotIndex = 0;
            gsm.CurrentSceneName = firstScene; // Сбрасываем имя сцены
        }

        KillOldPlayer();
        LoadScene(firstScene);
    }

    public void StartFromCheckpoint()
    {
        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogError("[DeadScreenUI] GameStateManager не найден!");
            StartFromBegin();
            return;
        }

        // Проверяем наличие файла сохранения на диске через менеджер
        if (!gsm.HasSaveFile())
        {
            Debug.LogWarning("[DeadScreenUI] Нет сохранения на диске, загружаем сначала.");
            StartFromBegin();
            return;
        }

        // Загружаем данные с диска в GameStateManager
        gsm.LoadGameFromDisk();

        // Полностью восстанавливаем здоровье
        gsm.PlayerHP = gsm.PlayerMaxHP;

        // Считываем имя сцены, в которой было сделано сохранение
        string scene = gsm.CurrentSceneName;
        if (string.IsNullOrEmpty(scene)) scene = firstScene;

        KillOldPlayer();
        LoadScene(scene);
    }


    private void KillOldPlayer()
    {
        var player = PlayerPersistence.Instance;
        if (player != null)
        {
            Debug.Log("[DeadScreenUI] Уничтожаем старого игрока перед загрузкой сцены.");
            PlayerPersistence.ClearInstance();
            Destroy(player.gameObject);
        }
    }

    private void LoadScene(string sceneName)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        HideDeadScreen();
    }

    public void ShowDeadScreen()
    {
        if (visualPanel != null) visualPanel.SetActive(true);
        if (deadCamera != null) deadCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideDeadScreen()
    {
        if (visualPanel != null) visualPanel.SetActive(false);
        if (deadCamera != null) deadCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}