using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Jogador : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 1f;
    public float sprintMultiplier = 1.1f;
    public float controllerRadius = 0.3f; // usado para SphereCast

    [Header("Stamina")]
    public float maxStamina = 60f;
    public float staminaDecreaseRate = 50f;
    public float staminaRecoveryRate = 10f;
    public Slider staminaBar;
    private float currentStamina;
    private bool isSprinting = false;

    [Header("Pulo")]
    public float jumpForce = 4f;
    private bool isGrounded;

    [Range(0.1f, 20f)]
    public float mouseSensitivity = 5f;

    [Header("Referências")]
    public Transform cameraHolder;
    public AudioSource finalAudio;
    public AudioSource stepsAudio;
    public AudioSource ambientAudio;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float fallThreshold = -10f;

    public bool gameEnded = false;
    private bool isRespawning = false;

    private Rigidbody rb;
    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        if (gameEnded || PauseMenu.isPaused) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        CheckGround();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        HandleSprint();
    }

    void FixedUpdate()
    {
        if (gameEnded || isRespawning || PauseMenu.isPaused) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDirection = forward * verticalInput + right * horizontalInput;
        moveDirection.y = 0;

        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 normalized = moveDirection.normalized;
            Vector3 targetPosition = rb.position + normalized * currentSpeed * Time.fixedDeltaTime;

            float checkDistance = (currentSpeed * Time.fixedDeltaTime) + 0.05f;
            RaycastHit hit;
            bool obstacleAhead = Physics.SphereCast(rb.position, controllerRadius, normalized, out hit, checkDistance, ~0, QueryTriggerInteraction.Ignore);

            if (!obstacleAhead)
            {
                rb.MovePosition(targetPosition);
            }
            else
            {
                Vector3 safePos = rb.position + normalized * Mathf.Max(0f, hit.distance - controllerRadius);
                rb.MovePosition(safePos);
            }

            if (stepsAudio != null && !stepsAudio.isPlaying)
                stepsAudio.Play();
        }
        else
        {
            if (stepsAudio != null && stepsAudio.isPlaying)
                stepsAudio.Stop();
        }

        if (rb.position.y < fallThreshold)
        {
            StartCoroutine(FallRespawn());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Inimigo"))
        {
            GameOver();
        }
        else if (collision.gameObject.CompareTag("Final"))
        {
            GameObject[] inimigos = GameObject.FindGameObjectsWithTag("Inimigo");
            foreach (GameObject inimigo in inimigos)
            {
                Destroy(inimigo);
            }

            Destroy(collision.gameObject);

            StartCoroutine(FinalSequence());
        }
    }

    void GameOver()
    {
        if (stepsAudio != null && stepsAudio.isPlaying)
            stepsAudio.Stop();

        if (ambientAudio != null && ambientAudio.isPlaying)
            ambientAudio.Stop();

        Restart();
    }

    IEnumerator FinalSequence()
    {
        gameEnded = true;

        if (stepsAudio != null && stepsAudio.isPlaying)
            stepsAudio.Stop();

        if (ambientAudio != null && ambientAudio.isPlaying)
            ambientAudio.Stop();

        Vector3 targetPosition = transform.position + transform.forward * 3f + Vector3.up * 1.2f;

        Quaternion targetRotation = Quaternion.LookRotation(transform.position - targetPosition, Vector3.up);

        float duration = 2f;
        float elapsed = 0f;

        Vector3 startPos = cameraHolder.position;
        Quaternion startRot = cameraHolder.rotation;

        Transform originalParent = cameraHolder.parent;
        cameraHolder.SetParent(null);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            cameraHolder.position = Vector3.Lerp(startPos, targetPosition, t);
            cameraHolder.rotation = Quaternion.Slerp(startRot, targetRotation, t);

            yield return null;
        }

        cameraHolder.position = targetPosition;
        cameraHolder.rotation = targetRotation;

        cameraHolder.SetParent(originalParent);

        if (finalAudio != null)
            finalAudio.Play();
    }

    IEnumerator FallRespawn()
    {
        isRespawning = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(0.5f);

        rb.position = respawnPoint.position;
        rb.rotation = respawnPoint.rotation;
        rb.linearVelocity = Vector3.zero;

        isRespawning = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        PauseMenu.isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void HandleSprint()
    {
        bool sprintKeyHeld = Input.GetKey(KeyCode.LeftShift);
        bool moving = Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;

        if (sprintKeyHeld && moving && currentStamina > 0)
        {
            isSprinting = true;
            currentStamina -= staminaDecreaseRate * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;
            }
        }

        if (staminaBar != null)
            staminaBar.value = currentStamina;
    }

    void CheckGround()
    {
        Vector3 spherePos = transform.position + Vector3.down * 0.5f;
        isGrounded = Physics.CheckSphere(spherePos, 0.3f, ~0, QueryTriggerInteraction.Ignore);
    }

    void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0f;
        rb.linearVelocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
