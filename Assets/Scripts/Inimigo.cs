using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Inimigo : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 3.5f;
    public float changeDirectionTime = 2f;
    public float obstacleCheckDistance = 1f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private float timer;

    private Transform player;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        player = GameObject.FindGameObjectWithTag("Jogador").transform;

        audioSource = GetComponentInChildren<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
        }

        SetRandomDirection();
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            Vector3 origin = rb.position + Vector3.up * 1.0f;
            Vector3 directionToPlayer = (player.position - origin).normalized;

            if (Physics.Raycast(origin, directionToPlayer, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Jogador"))
                {
                    moveDirection = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;

                    if (audioSource != null && !audioSource.isPlaying)
                        audioSource.Play();
                }
                else
                {
                    RandomMovement();

                    if (audioSource != null && audioSource.isPlaying)
                        audioSource.Pause();
                }
            }
        }

        Vector3 targetPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    void RandomMovement()
    {
        if (Physics.Raycast(rb.position, moveDirection, obstacleCheckDistance))
        {
            SetRandomDirection();
        }

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            SetRandomDirection();
        }
    }

    void SetRandomDirection()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);
        moveDirection = new Vector3(randomX, 0f, randomZ).normalized;

        timer = changeDirectionTime;
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rb.position, rb.position + moveDirection * obstacleCheckDistance);
        }

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rb.position + Vector3.up * 1.0f, player.position);
        }
    }
}
