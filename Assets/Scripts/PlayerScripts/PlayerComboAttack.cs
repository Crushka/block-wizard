using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerComboAttack : MonoBehaviour
{
    [Header("Связи")]
    public Animator playerAnimator;
    public BookInteraction bookInteraction;

    [Header("Настройки комбо-атак")]
    [Tooltip("Минимальное время между кликами (кулдаун)")]
    public float attackCooldown = 0.5f;

    [Tooltip("Максимальное время между кликами для продолжения серии")]
    public float comboWindow = 2.0f; [Tooltip("Названия триггеров в Animator для каждой из атак")]
    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3" };

    private InputAction attackAction;
    private int currentComboIndex = 0;
    private float lastAttackTime = -Mathf.Infinity;

    void Awake()
    {
        attackAction = new InputAction("MeleeAttack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Gamepad>/buttonWest");
    }

    void OnEnable()
    {
        attackAction.Enable();
        attackAction.performed += OnAttack;
    }

    void OnDisable()
    {
        attackAction.performed -= OnAttack;
        attackAction.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (bookInteraction != null && bookInteraction.IsReading)
            return;

        float currentTime = Time.time;

        if (currentTime - lastAttackTime < attackCooldown)
            return;

        if (currentTime - lastAttackTime > comboWindow)
        {
            currentComboIndex = 0;
        }

        if (playerAnimator != null && attackTriggers.Length > 0)
        {
            ResetAllAttackTriggers();
            playerAnimator.SetTrigger(attackTriggers[currentComboIndex]);
        }

        lastAttackTime = currentTime;

        currentComboIndex = (currentComboIndex + 1) % attackTriggers.Length;
    }

    private void ResetAllAttackTriggers()
    {
        foreach (string trigger in attackTriggers)
        {
            playerAnimator.ResetTrigger(trigger);
        }
    }
}