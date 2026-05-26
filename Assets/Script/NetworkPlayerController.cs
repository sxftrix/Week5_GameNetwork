using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement & Physics")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float jumpHeight = 2f; 

    [Header("Player Health (Local UI)")]
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private Slider localHealthSlider;

    [Header("Floating Damage Text Prefab")]
    [SerializeField] private GameObject floatingTextPrefab;

    private CharacterController characterController;
    private float verticalVelocity;
    private bool jumpRequested = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && localHealthSlider != null)
        {
            localHealthSlider.gameObject.SetActive(true);
            localHealthSlider.maxValue = 100;
            localHealthSlider.value = currentHealth.Value;
            currentHealth.OnValueChanged += OnHealthChanged;
        }
        else if (localHealthSlider != null)
        {
            localHealthSlider.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && localHealthSlider != null)
        {
            currentHealth.OnValueChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (localHealthSlider != null)
        {
            localHealthSlider.value = newValue;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 movementInput = new Vector2(horizontalInput, verticalInput);

        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            jumpRequested = true;
        }

        if (IsServer)
        {
            MovePlayer(movementInput, jumpRequested);
            jumpRequested = false;
        }
        else
        {
            MovePlayerRpc(movementInput, jumpRequested);
            jumpRequested = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void MovePlayerRpc(Vector2 movementInput, bool clientJumped)
    {
        MovePlayer(movementInput, clientJumped);
    }

    private void MovePlayer(Vector2 movementInput, bool clientJumped)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (clientJumped && characterController.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector3 moveDirection = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
        Vector3 horizontalMovement = moveDirection * moveSpeed;
        Vector3 verticalMovement = Vector3.up * verticalVelocity;
        Vector3 finalMovement = horizontalMovement + verticalMovement;

        characterController.Move(finalMovement * Time.deltaTime);
    }

    [ContextMenu("Test Take 10 Damage")]
    public void TakeDamageServer(int damageAmount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Max(0, currentHealth.Value - damageAmount);
        SpawnDamageTextClientRpc(damageAmount);
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnDamageTextClientRpc(int damageAmount)
    {
        if (floatingTextPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 2f;
            GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);
            textObj.GetComponent<FloatingText>().Setup(damageAmount);
        }
    }
}