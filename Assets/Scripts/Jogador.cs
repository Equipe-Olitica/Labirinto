using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Jogador : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 6f;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (gameEnded) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        if (gameEnded || isRespawning) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 moveDirection = forward * verticalInput + right * horizontalInput;
        moveDirection.y = 0;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 targetPosition = rb.position + moveDirection.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            if (!stepsAudio.isPlaying)
                stepsAudio.Play();
        }
        else
        {
            if (stepsAudio.isPlaying)
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
        if (stepsAudio.isPlaying)
            stepsAudio.Stop();

        if (ambientAudio.isPlaying)
            ambientAudio.Stop();

        Destroy(gameObject);
    }

    IEnumerator FinalSequence()
    {
        gameEnded = true;

        if (stepsAudio.isPlaying)
            stepsAudio.Stop();

        if (ambientAudio.isPlaying)
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
        rb.rotation = Quaternion.identity;

        isRespawning = false;
    }
}
