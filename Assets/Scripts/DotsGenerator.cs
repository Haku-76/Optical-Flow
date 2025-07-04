using UnityEngine;
using System.Collections.Generic;

public class DotGenerator : MonoBehaviour
{
    public GameObject cam;

    [Space(15)]
    public float areaHeight;   // Y轴范围
    public float areaDepth;    // Z轴范围

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

        float totalArea = areaHeight * areaDepth;
        int dotCount = Mathf.FloorToInt(totalArea * dotDensity);

        int attempts = 0;
        int maxAttempts = dotCount * 10;
        dotPositions.Clear();  // 清除旧数据

        while (dotPositions.Count < dotCount && attempts < maxAttempts)
        {
            attempts++;
            float y = Random.Range(-areaHeight / 2f + dotRadius, areaHeight / 2f - dotRadius);
            float z = Random.Range(-areaDepth / 2f + dotRadius, areaDepth / 2f - dotRadius);
            Vector2 candidate = new Vector2(y, z);

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
                CreateDot(new Vector3(0, y, z));  // x=0，分布在 YZ 平面
            }
        }

        Debug.Log($"{dotPositions.Count} 个 dot 已生成。");
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
        Vector3 size = new Vector3(0.01f, areaHeight, areaDepth);  // YZ平面

        Gizmos.DrawWireCube(center, size);
    }
}
