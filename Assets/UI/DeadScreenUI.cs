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
        Cursor.lockState = CursorLockMode.Locked;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

      
    }


}
