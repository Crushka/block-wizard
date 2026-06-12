using Newtonsoft.Json.Bson;
using UnityEngine;

/// <summary>
/// Consumable potion. Implements IQuickSlotable so HUDQuickSlotSelector can
/// call Activate() when the player presses R.
///
/// Before calling Activate(), HUDQuickSlotSelector sets the PlayerHealth
/// reference via SetPlayerHealth().
///
/// Extend PotionType enum and the switch-block below to add new effects.
/// </summary>
public class Potion : RegularItem, IQuickSlotable
{
    [Header("Potion Settings")]
    public PotionType potionType = PotionType.Health;

    [Header("Effect Values")]
    [Tooltip("Сколько HP восстанавливает зелье здоровья")]
    public float healAmount = 30f;

    [Header("Speed Potion Settings")]
    public float speedBoostAmount = 1.5f;
    public float speedBoostDuration = 20f;

    // Injected at runtime by HUDQuickSlotSelector
    private PlayerHealth _playerHealth;
    private PlayerController _playerController;

    public void SetPlayerHealth(PlayerHealth ph) => _playerHealth = ph;
    public void SetPlayerController(PlayerController pc) => _playerController = pc;

    // ── IQuickSlotable ────────────────────────────────────────────────────────

    public void Activate()
    {
        switch (potionType)
        {
            case PotionType.Health:
                ApplyHeal();
                break;

            case PotionType.Speed:
                ApplySpeed();
                break;

            default:
                Debug.LogWarning($"[Potion] Неизвестный тип зелья: {potionType}");
                break;
        }
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    private void ApplyHeal()
    {
        if (_playerHealth == null)
        {
            Debug.LogWarning("[Potion] PlayerHealth не задан — эффект не применён.");
            return;
        }

        float before = _playerHealth.health;
        _playerHealth.health = Mathf.Min(_playerHealth.health + healAmount,
                                          _playerHealth.maxHealth);
        float restored = _playerHealth.health - before;
        Debug.Log($"[Potion] Зелье здоровья выпито. Восстановлено: {restored:F1} HP " +
                  $"({before:F1} → {_playerHealth.health:F1})");
    }

    private void ApplySpeed()
    {
        float originalSpeed = _playerController.getOriginalSpeed();
        _playerController.ApplySpeedModifier(speedBoostAmount, speedBoostDuration);

        Debug.Log($"[Potion] Зелье скорости выпито. Скорость умножена на {speedBoostAmount} " +
                  $"на {speedBoostDuration} секунд. (Оригинальная скорость: {originalSpeed})");
    }

    // ── Pickup ────────────────────────────────────────────────────────────────

    public override void AddToInventory()
    {
        InventorySlotManager.Instance?.AddItem(this);
    }
}