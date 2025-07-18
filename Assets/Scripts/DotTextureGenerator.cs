﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 在RawImage上生成不重叠的白色圆点或正方形点纹理
/// </summary>
[RequireComponent(typeof(RawImage))]
public class DotTextureGenerator : MonoBehaviour
{
    public bool isCircle = true;  // true=圆形，false=正方形
    public int textureWidth = 2560;
    public int textureHeight = 1440;
    public int randomSeed = 1;
    public int dotRadius = 5;
    public float dotDensity = 0.0002f;

    private Texture2D dotTexture;
    private List<Vector2> dotPositions = new List<Vector2>();

    void Start()
    {
        GenerateDotTexture();
    }

    void GenerateDotTexture()
    {
        Random.InitState(randomSeed);
        int dotCount = Mathf.FloorToInt(textureWidth * textureHeight * dotDensity);
        int maxAttempts = dotCount * 20;
        int attempts = 0;

        dotPositions.Clear();
        while (dotPositions.Count < dotCount && attempts < maxAttempts)
        {
            float x = Random.Range(dotRadius, textureWidth - dotRadius);
            float y = Random.Range(dotRadius, textureHeight - dotRadius);

            bool overlap = false;
            foreach (var p in dotPositions)
            {
                if (Vector2.Distance(p, new Vector2(x, y)) < dotRadius * 5f)
                {
                    overlap = true;
                    break;
                }
            }
            if (!overlap)
                dotPositions.Add(new Vector2(x, y));
            attempts++;
        }

        dotTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGB24, false);
        Color32[] pixels = new Color32[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        foreach (var pos in dotPositions)
        {
            if (isCircle)
                DrawCircle(pixels, textureWidth, textureHeight, (int)pos.x, (int)pos.y, dotRadius, Color.white);
            else
                DrawSquare(pixels, textureWidth, textureHeight, (int)pos.x, (int)pos.y, dotRadius, Color.white);
        }

        dotTexture.SetPixels32(pixels);
        dotTexture.Apply();

        GetComponent<RawImage>().texture = dotTexture;
    }

    // 绘制圆形
    void DrawCircle(Color32[] pixels, int w, int h, int cx, int cy, int radius, Color32 color)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            int py = cy + y;
            if (py < 0 || py >= h) continue;
            for (int x = -radius; x <= radius; x++)
            {
                int px = cx + x;
                if (px < 0 || px >= w) continue;
                if (x * x + y * y <= r2)
                {
                    pixels[py * w + px] = color;
                }
            }
        }
    }

    // 绘制正方形
    void DrawSquare(Color32[] pixels, int w, int h, int cx, int cy, int radius, Color32 color)
    {
        for (int y = -radius; y <= radius; y++)
        {
            int py = cy + y;
            if (py < 0 || py >= h) continue;
            for (int x = -radius; x <= radius; x++)
            {
                int px = cx + x;
                if (px < 0 || px >= w) continue;
                pixels[py * w + px] = color;
            }
        }
    }
}
