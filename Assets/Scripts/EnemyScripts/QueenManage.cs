using UnityEngine;
using UnityEngine.Events;

public class QueenManage : MonoBehaviour
{
    public static QueenManage Instance;

    [Header("Настройки фазы")]
    public int totalCages = 6;
    public int cagesDestroyed = 0;
    public int cagesForPhase2 = 5;

    [Header("События")]
    public UnityEvent onAngerIncreased;
    public UnityEvent onPhase2Start;

    private void Awake()
    {
        Instance = this;
    }

    public void CageDestroyed()
    {
        cagesDestroyed++;
        Debug.Log($"Клеток разрушено: {cagesDestroyed}/{totalCages}");

        onAngerIncreased.Invoke();

        if (cagesDestroyed >= cagesForPhase2)
        {
            StartPhase2();
        }
    }

    private void StartPhase2()
    {
        Debug.Log("КОРОЛЕВА В ЯРОСТИ! ПЕРЕХОД ВО ВТОРУЮ ФАЗУ");
        onPhase2Start.Invoke();
    }
}