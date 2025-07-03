using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class AlphaController : MonoBehaviour
{
    public bool active;
    public float alpha;

    [Header("Active")]
    public GameObject cam;
    public float distance = 0.5f;
    public Vector3 camPosition;
    public Vector3 initialPosition;
    public Vector3 leftPosition;
    public Vector3 rightPosition;

    [Header("Passive")]
    public float speed = 1.0f;

    private Material _material;
    private float time;

    void Start()
    {
        initialPosition = cam.transform.position;
        leftPosition = new Vector3(initialPosition.x - distance / 2.0f, initialPosition.y, initialPosition.z);
        rightPosition = new Vector3(initialPosition.x + distance / 2.0f, initialPosition.y, initialPosition.z);

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            _material = renderer.material;
        }
    }

    void Update()
    {
        camPosition = cam.transform.position;

        if (active == true)
        {
            alpha = Mathf.Clamp01(Mathf.Abs(cam.transform.position.x - leftPosition.x) / distance);
            UpdateAlpha(alpha);
        }
        else if (active == false && _material != null)
        {
            time += Time.deltaTime * speed;
            alpha = Mathf.PingPong(time, 1f); // 在0到1之间来回摆动
            UpdateAlpha(alpha);
        }
    }

    void UpdateAlpha(float alpha)
    {
        if (_material.HasProperty("_Color"))
        {
            Color color = _material.color;
            color.a = alpha;
            _material.color = color;
        }
        else
        {
            Debug.LogWarning("Material does not have a _Color property to set alpha.");
        }
    }
}
