using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Связи")]
    public CameraController cameraController;
    public PlayerController playerController;
    public BookInteraction bookInteraction;
    public Animator playerAnimator;
    [Header("Настройки прицеливания (Атаки)")]
    public float aimDistance = 2.0f; // Насколько близко подъедет камера
    public Vector3 aimCameraOffset = new Vector3(0.5f, 1.5f, 0f); // Смещение к руке/плечу (X - вправо, Y - вверх)
    public float transitionSpeed = 8.0f; // Скорость приближения/отдаления

    private InputAction aimAction;
    private bool isAiming = false;
    private float originalDistance;
    public float interactionDamage = 25f;
    public float interactionRange = 5f;

    void Awake()
    {
        aimAction = new InputAction("Aim", InputActionType.Button);
        aimAction.AddBinding("<Mouse>/rightButton");
        aimAction.AddBinding("<Gamepad>/leftTrigger");
    }

    void OnEnable()
    {
        aimAction.Enable();
        aimAction.started += OnAimStart;
        aimAction.canceled += OnAimCancel;
    }

    void OnDisable()
    {
        aimAction.started -= OnAimStart;
        aimAction.canceled -= OnAimCancel;
        aimAction.Disable();
    }

    void Start()
    {
        if (cameraController != null)
        {
            originalDistance = cameraController.distance;
        }
    }

    private void OnAimStart(InputAction.CallbackContext ctx)
    {
        // Не даем прицелиться, если игрок сейчас читает книгу
        if (bookInteraction != null && bookInteraction.IsReading) return;

        isAiming = true;

        // Блокируем возможность открыть книгу
        if (bookInteraction != null) bookInteraction.canRead = false;

        // Передаем состояние в контроллер игрока (чтобы он крутился за мышью)
        if (playerController != null) playerController.isAiming = true;

        // Передаем состояние в камеру (блокируем зум колесиком)
        if (cameraController != null)
        {
            cameraController.isAiming = true;
            originalDistance = cameraController.distance; // Запоминаем текущий зум перед прицеливанием
        }

        // Запускаем анимацию поднятия руки
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAiming", true);
        }
    }

    private void OnAimCancel(InputAction.CallbackContext ctx)
    {
        if (!isAiming) return;
        isAiming = false;

        // Возвращаем возможность читать
        if (bookInteraction != null) bookInteraction.canRead = true;

        // Возвращаем обычное поведение движения и камеры
        if (playerController != null) playerController.isAiming = false;
        if (cameraController != null) cameraController.isAiming = false;

        // Запускаем анимацию опускания руки (переход в Idle настраивается в Animator)
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAiming", false);
        }
    }

    void Update()
    {
        if (cameraController == null) return;

        // Плавное изменение дистанции и смещения камеры (эффект приближения к руке)
        float targetDistance = isAiming ? aimDistance : originalDistance;
        Vector3 targetOffset = isAiming ? aimCameraOffset : Vector3.zero;

        cameraController.distance = Mathf.Lerp(cameraController.distance, targetDistance, Time.deltaTime * transitionSpeed);
        cameraController.targetOffset = Vector3.Lerp(cameraController.targetOffset, targetOffset, Time.deltaTime * transitionSpeed);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            PerformSimpleAttack();
            Debug.Log("Попытка атаки");
        }
    }
    private void PerformSimpleAttack()
    {
        GolemAI closest = null;
        float minDist = interactionRange;

        // Просто перебираем наш готовый статический список
        foreach (GolemAI g in GolemAI.AllGolems)
        {
            if (g == null || g.isDead) continue;

            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;
            }
        }

        if (closest != null)
        {
            Debug.Log("-25 Hp");
            closest.TakeDamage(interactionDamage);
        }
    }
}