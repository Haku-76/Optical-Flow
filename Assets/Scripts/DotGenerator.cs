using UnityEngine;
using System.Collections.Generic;

public class DotGenerator : MonoBehaviour
{
    public GameObject cam;

    [Space(15)]
    public float areaWidth;   // X轴范围
    public float areaHeight;  // Y轴范围

    [Space(15)]
    public int seed = 0;
    public float dotRadius;
    public float dotDensity;

    private List<Vector2> dotPositions = new List<Vector2>();

    void Start()
    {
        //CreateBackground(); // 新增：先生成黑色背景
        GenerateDots();
    }

    void GenerateDots()
    {
        Random.InitState(seed); // 保证结果一致

        float totalArea = areaWidth * areaHeight;
        int dotCount = Mathf.FloorToInt(totalArea * dotDensity);

        int attempts = 0;
        int maxAttempts = dotCount * 10;
        dotPositions.Clear();  // 清除旧数据

        while (dotPositions.Count < dotCount && attempts < maxAttempts)
        {
            attempts++;
            float x = Random.Range(-areaWidth / 2f + dotRadius, areaWidth / 2f - dotRadius);
            float y = Random.Range(-areaHeight / 2f + dotRadius, areaHeight / 2f - dotRadius);
            Vector2 candidate = new Vector2(x, y);

            bool tooClose = false;
            foreach (var pos in dotPositions)
            {
                if (Vector2.Distance(pos, candidate) < dotRadius * 2f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                dotPositions.Add(candidate);
                CreateDot(new Vector3(x, y, 0));  // z=0，分布在XY平面
            }
        }

        Debug.Log($"{dotPositions.Count} 个 dot 已生成。");
    }

    void CreateBackground()
    {
        // 创建平面
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Plane);
        bg.transform.parent = this.transform;

        // 平面中心与DotGenerator一致，略往z负方向偏移，避免跟dot重叠
        bg.transform.position = transform.position + new Vector3(0, 0, -dotRadius);

        // Unity默认Plane是XZ平面，旋转到XY平面
        bg.transform.localRotation = Quaternion.Euler(270f, 0, 0);

        // 默认Plane大小为10x10，缩放到 areaWidth/areaHeight
        float scaleX = areaWidth / 10f;
        float scaleY = areaHeight / 10f;
        bg.transform.localScale = new Vector3(scaleX, scaleY, 1); // 缩放Y轴就是对应原来的Z

        // 设置黑色无光材质
        var renderer = bg.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.black;
            renderer.material = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    void CreateDot(Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = transform.position + position;
        sphere.transform.localScale = Vector3.one * dotRadius * 2f;
        sphere.transform.parent = this.transform;

        var renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = Color.white;
            renderer.material = mat;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(areaWidth, areaHeight, 0.01f);  // XY平面

        Gizmos.DrawWireCube(center, size);
    }
}
