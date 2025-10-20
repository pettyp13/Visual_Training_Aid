using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class SphereController : MonoBehaviour
{
    public int id;
    public bool isTarget = false;
    public float speed = 1f;
    public Vector3 velocity;
    public Vector3 bounds = new Vector3(5f, 5f, 5f);

    Renderer rend;
    Color baseColor;
    Color targetColor = Color.yellow;
    Color selectedColor = Color.green;

    bool isMoving = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
    }

    // Initialize positions and set NOT moving
    public void Initialize(int _id, Vector3 _bounds, float _speed)
    {
        id = _id;
        bounds = _bounds;
        speed = _speed;

        // random starting position inside bounds
        transform.localPosition = new Vector3(
            Random.Range(-bounds.x, bounds.x),
            Random.Range(-bounds.y, bounds.y),
            Random.Range(-bounds.z, bounds.z)
        );

        // Set velocity to zero for now; will be assigned when StartMoving is called
        velocity = Vector3.zero;
        isMoving = false;
        isTarget = false;
        rend.material.color = baseColor;
    }

    void Update()
    {
        if (!isMoving) return; // <-- nothing moves until we explicitly start moving

        transform.localPosition += velocity * Time.deltaTime;

        // bounce on bounds
        Vector3 pos = transform.localPosition;
        if (Mathf.Abs(pos.x) > bounds.x) { velocity.x = -velocity.x; pos.x = Mathf.Sign(pos.x) * bounds.x; }
        if (Mathf.Abs(pos.y) > bounds.y) { velocity.y = -velocity.y; pos.y = Mathf.Sign(pos.y) * bounds.y; }
        if (Mathf.Abs(pos.z) > bounds.z) { velocity.z = -velocity.z; pos.z = Mathf.Sign(pos.z) * bounds.z; }
        transform.localPosition = pos;
    }

    public void SetTargetHighlight(bool highlight)
    {
        isTarget = highlight;
        rend.material.color = highlight ? targetColor : baseColor;
    }

    // Start movement (call after highlight phase)
    public void StartMoving(float overrideSpeed = -1f)
    {
        if (overrideSpeed > 0f) speed = overrideSpeed;
        velocity = Random.onUnitSphere * speed;
        isMoving = true;
    }

    // Stop moving (call at end of movement phase)
    public void StopMoving()
    {
        isMoving = false;
        velocity = Vector3.zero;
    }

    public void MarkSelected(bool correct)
    {
        rend.material.color = correct ? selectedColor : Color.red;
    }
}
