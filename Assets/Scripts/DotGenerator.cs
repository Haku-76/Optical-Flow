using UnityEngine;
using System.Collections.Generic;

public class DotGenerator : MonoBehaviour
{
    public GameObject cam;
    public bool isSphere = true; // true: Sphere, false: Cube

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
                if (Vector2.Distance(pos, candidate) < dotRadius * 5f)
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

    void CreateDot(Vector3 position)
    {
        // 根据isSphere变量决定使用球还是立方体
        PrimitiveType type = isSphere ? PrimitiveType.Sphere : PrimitiveType.Cube;
        GameObject dotObj = GameObject.CreatePrimitive(type);
        dotObj.transform.position = transform.position + position;
        dotObj.transform.localScale = Vector3.one * dotRadius * 2f;
        dotObj.transform.parent = this.transform;

        var renderer = dotObj.GetComponent<Renderer>();
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
