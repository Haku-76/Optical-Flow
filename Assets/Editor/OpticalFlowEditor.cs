using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

// 继承你原来的编辑器
[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    const int SAMPLE_COUNT = 400; // 采样点数量

    public override void OnInspectorGUI()
    {
        // 默认 Inspector
        DrawDefaultInspector();

        CameraController of = (CameraController)target;

        GUILayout.Space(15);
        GUILayout.Label("Motion Curve", EditorStyles.boldLabel);

        // ...（省略运动曲线绘制代码，和你原来一样）...

        DrawMotionCurve(of);

        GUILayout.Space(10);
        GUILayout.Label("Ratio Curve", EditorStyles.boldLabel);

        DrawRatioCurve(of);

        // 实时刷新
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    void DrawMotionCurve(CameraController of)
    {
        Rect graphRect = GUILayoutUtility.GetRect(200, 100);
        EditorGUI.DrawRect(graphRect, Color.black);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.MiddleLeft;

        Rect leftLabelRect = new Rect(graphRect.x + 5, graphRect.yMax - 10, 50, 20);
        Rect rightLabelRect = new Rect(graphRect.x + 5, graphRect.y, 50, 20);
        GUI.Label(leftLabelRect, "Left", labelStyle);
        GUI.Label(rightLabelRect, "Right", labelStyle);

        Handles.BeginGUI();
        Handles.color = Color.cyan;

        float currentTime = of.elapsedTime;
        float startTime = currentTime - 2f;

        Vector3[] points = new Vector3[SAMPLE_COUNT];
        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float t = Mathf.Lerp(startTime, currentTime, i / (float)(SAMPLE_COUNT - 1));
            float y = GetCurveY(t, of);

            float xPixel = Mathf.Lerp(graphRect.x, graphRect.xMax, i / (float)(SAMPLE_COUNT - 1));
            float yPixel = Mathf.Lerp(graphRect.yMax, graphRect.y, y);
            points[i] = new Vector3(xPixel, yPixel, 0);
        }
        Handles.DrawAAPolyLine(2f, points);
        Handles.EndGUI();
    }

    // 新增：Ratio 曲线的可视化
    void DrawRatioCurve(CameraController of)
    {
        Rect graphRect = GUILayoutUtility.GetRect(200, 80);
        EditorGUI.DrawRect(graphRect, Color.gray * 0.7f);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.MiddleLeft;

        Rect zeroRect = new Rect(graphRect.x + 5, graphRect.yMax - 10, 50, 20);
        Rect oneRect = new Rect(graphRect.x + 5, graphRect.y, 50, 20);
        GUI.Label(zeroRect, "0", labelStyle);
        GUI.Label(oneRect, "1", labelStyle);

        Handles.BeginGUI();
        Handles.color = Color.green;

        float currentTime = of.elapsedTime;
        float startTime = currentTime - 2f;

        Vector3[] points = new Vector3[SAMPLE_COUNT];
        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            float t = Mathf.Lerp(startTime, currentTime, i / (float)(SAMPLE_COUNT - 1));
            float ratio = GetRatioAtTime(t, of);

            float xPixel = Mathf.Lerp(graphRect.x, graphRect.xMax, i / (float)(SAMPLE_COUNT - 1));
            float yPixel = Mathf.Lerp(graphRect.yMax, graphRect.y, ratio);
            points[i] = new Vector3(xPixel, yPixel, 0);
        }
        Handles.DrawAAPolyLine(2f, points);
        Handles.EndGUI();
    }

    // 复制你的运动曲线采样逻辑
    float GetCurveY(float t, CameraController of)
    {
        if (!Application.isPlaying)
            return 0f;
        float modT = ((t % 2f) + 2f) % 2f;
        switch (of.motionOption)
        {
            case CameraController.MotionOption.Linear:
                return 1f - Mathf.Abs(modT - 1f);
            case CameraController.MotionOption.Cos:
                return (1f - Mathf.Cos(Mathf.PI * modT)) / 2f;
            case CameraController.MotionOption.ArcCos:
                if (modT <= 1f)
                    return Mathf.Acos(-2f * modT + 1f) / Mathf.PI;
                else
                    return Mathf.Acos(-2f * (2f - modT) + 1f) / Mathf.PI;
            default:
                return 0f;
        }
    }

    // 采样 ratio 的值（与 Update() 里的算法一致）
    float GetRatioAtTime(float t, CameraController of)
    {
        if (!Application.isPlaying)
            return 0f;

        float modT = ((t % 2f) + 2f) % 2f;
        float ease = of.motionOption switch
        {
            CameraController.MotionOption.Linear => (modT <= 1f) ? modT : 1f - (modT - 1f),
            CameraController.MotionOption.Cos => (modT <= 1f) ? (1f - Mathf.Cos(Mathf.PI * modT)) / 2f : (1f - Mathf.Cos(Mathf.PI * (1f - (modT - 1f)))) / 2f,
            CameraController.MotionOption.ArcCos => (modT <= 1f) ? Mathf.Acos(-2f * modT + 1f) / Mathf.PI : Mathf.Acos(-2f * (1f - (modT - 1f)) + 1f) / Mathf.PI,
            _ => 0f
        };
        // 按你的 ratio 计算公式
        float ratio = ease * of.amplitude;
        return Mathf.Clamp01(ratio);
    }
}
