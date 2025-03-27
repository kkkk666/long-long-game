using UnityEngine;

public class VerticalMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float amplitude = 1f;        // How far up/down to move
    public float frequency = 1f;        // How fast to move (cycles per second)
    public bool startAtTop = false;     // Whether to start at the top of the movement

    private Vector3 startPosition;
    private float offset;

    void Start()
    {
        startPosition = transform.position;
        
        // If starting at top, offset by amplitude
        if (startAtTop)
        {
            offset = amplitude;
        }
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin((Time.time * frequency * 2f * Mathf.PI) + offset) * amplitude;
        
        // Update the position
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
} 