using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerComboAttack : MonoBehaviour
{
    [Header("Связи")]
    public Animator playerAnimator;
    public BookInteraction bookInteraction;

    [Header("Настройки комбо-атак")]
    public float attackCooldown = 0.5f;
    public float comboWindow = 2.0f;
    public float holdThreshold = 0.35f;

    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3" };
    public string[] holdAttackBools = { "Attack1Hold", "Attack2Hold", "Attack3Hold" };

    private InputAction attackAction;
    private int currentComboIndex = 0;
    private float lastAttackTime = -Mathf.Infinity;

    private bool holdActivated = false;
    private float holdStartTime = 0f;
    private int holdComboIndex = -1;

    [HideInInspector] public bool canAttack = true;

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
        attackAction.canceled += OnAttackReleased;
    }

    void OnDisable()
    {
        attackAction.performed -= OnAttack;
        attackAction.canceled -= OnAttackReleased;
        attackAction.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (!canAttack || (bookInteraction != null && bookInteraction.IsReading))
            return;

        float currentTime = Time.time;

        if (currentTime - lastAttackTime < attackCooldown)
            return;

        if (currentTime - lastAttackTime > comboWindow)
            currentComboIndex = 0;

        if (playerAnimator != null && attackTriggers.Length > 0)
        {
            ResetAllAttackTriggers();
            playerAnimator.SetTrigger(attackTriggers[currentComboIndex]);
        }

        lastAttackTime = currentTime;

        holdComboIndex = currentComboIndex;
        holdStartTime = currentTime;
        holdActivated = false;

        currentComboIndex = (currentComboIndex + 1) % attackTriggers.Length;
    }

    private void OnAttackReleased(InputAction.CallbackContext ctx)
    {
        if (holdActivated && holdComboIndex >= 0 && holdComboIndex < holdAttackBools.Length)
        {
            playerAnimator.SetBool(holdAttackBools[holdComboIndex], false);
        }

        holdActivated = false;
        holdComboIndex = -1;
    }

    void Update()
    {
        if (holdComboIndex < 0 || holdActivated)
            return;

        if (!attackAction.IsPressed())
        {
            holdComboIndex = -1;
            return;
        }

        if (Time.time - holdStartTime >= holdThreshold)
        {
            holdActivated = true;

            if (playerAnimator != null && holdComboIndex < holdAttackBools.Length)
                playerAnimator.SetBool(holdAttackBools[holdComboIndex], true);
        }
    }

    private void ResetAllAttackTriggers()
    {
        foreach (string trigger in attackTriggers)
            playerAnimator.ResetTrigger(trigger);
    }
}