using UnityEngine;

public class RadioController : MonoBehaviour
{
    public bool active;

    [Space(15)]
    public GameObject left;
    public GameObject right;

    [Space(15)]
    public float alpha;
    public float beta;

    [Header("Active")]
    public GameObject cam;
    public float distance = 0.5f;
    public Vector3 camPosition;
    public Vector3 initialPosition;
    public Vector3 leftPosition;
    public Vector3 rightPosition;

    [Header("Passive")]
    public float speed = 1.0f;

    private Material left_material;
    private Material right_material;
    private float time;

    void Start()
    {
        initialPosition = cam.transform.position;
        leftPosition = new Vector3(initialPosition.x - distance / 2.0f, initialPosition.y, initialPosition.z);
        rightPosition = new Vector3(initialPosition.x + distance / 2.0f, initialPosition.y, initialPosition.z);

        Renderer left_renderer = left.GetComponent<Renderer>();
        Renderer right_renderer = right.GetComponent<Renderer>();
        if (left_renderer != null && right_renderer != null)
        {
            left_material = left_renderer.material;
            right_material = right_renderer.material;
        }
    }

    void Update()
    {
        camPosition = cam.transform.position;

        if (active == true)
        {
            alpha = Mathf.Clamp01(Mathf.Abs(cam.transform.position.x - rightPosition.x) / distance);
            beta = 1.0f - alpha;
            UpdateRadio(alpha, beta);
        }
        else if (active == false && left_material != null && right_material != null)
        {
            time += Time.deltaTime * speed;
            alpha = Mathf.PingPong(time, 1f);
            beta = 1.0f - alpha;
            UpdateRadio(alpha, beta);
        }
    }

    void UpdateRadio(float alpha, float beta)
    {
        if (left_material.HasProperty("_Color"))
        {
            Color color = left_material.color;
            color.a = alpha;
            left_material.color = color;
        }
        else
        {
            Debug.LogWarning("Material does not have a _Color property to set alpha.");
        }

        if (right_material.HasProperty("_Color"))
        {
            Color color = right_material.color;
            color.a = beta;
            right_material.color = color;
        }
        else
        {
            Debug.LogWarning("Material does not have a _Color property to set beta.");
        }
    }
}
