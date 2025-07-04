using UnityEngine;
using UnityEditor;

// 自定义编辑器，用于在Inspector中可视化CameraController组件的运动曲线
[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    const int SAMPLE_COUNT = 400; // 曲线采样点数量

    public override void OnInspectorGUI()
    {
        // 绘制默认Inspector
        DrawDefaultInspector();

        CameraController of = (CameraController)target;

        GUILayout.Space(15);
        GUILayout.Label("Motion Curve", EditorStyles.boldLabel); // 显示曲线标题

        // 预留绘制曲线图的区域
        Rect graphRect = GUILayoutUtility.GetRect(200, 100);
        EditorGUI.DrawRect(graphRect, Color.black); // 背景填充为黑色

        // 添加纵轴标签：Left (y=0) 和 Right (y=1)
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.MiddleLeft;

        Rect leftLabelRect = new Rect(graphRect.x + 5, graphRect.yMax - 10, 50, 20);
        Rect rightLabelRect = new Rect(graphRect.x + 5, graphRect.y, 50, 20);
        GUI.Label(leftLabelRect, "Left", labelStyle);
        GUI.Label(rightLabelRect, "Right", labelStyle);

        Handles.BeginGUI();
        Handles.color = Color.cyan; // 曲线颜色为青色

        // 当前时间（播放时为elapsedTime）
        float currentTime = of.elapsedTime;
        float startTime = currentTime - 2f; // 显示过去2秒的曲线（一个周期）

        Vector3[] points = new Vector3[SAMPLE_COUNT]; // 用于存储曲线点
        for (int i = 0; i < SAMPLE_COUNT; i++)
        {
            // 计算每个采样点对应的时间 t
            float t = Mathf.Lerp(startTime, currentTime, i / (float)(SAMPLE_COUNT - 1));

            // 获取该时间点的曲线值（0~1之间）
            float y = GetCurveY(t, of);

            // 将曲线点转换为屏幕像素位置（注意y轴需要翻转）
            float xPixel = Mathf.Lerp(graphRect.x, graphRect.xMax, i / (float)(SAMPLE_COUNT - 1));
            float yPixel = Mathf.Lerp(graphRect.yMax, graphRect.y, y); // y轴反向
            points[i] = new Vector3(xPixel, yPixel, 0);
        }

        // 绘制曲线折线
        Handles.DrawAAPolyLine(2f, points);
        Handles.EndGUI();

        // 在播放模式下每帧刷新 Inspector
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    // 根据时间 t 和当前运动模式计算曲线的 y 值
    float GetCurveY(float t, CameraController of)
    {
        // 如果不在播放模式，返回0（不显示曲线）
        if (!Application.isPlaying)
            return 0f;

        //float modT = t % 2f; // 对周期 2 取模，确保周期性
        float modT = ((t % 2f) + 2f) % 2f;

        switch (of.motionOption)
        {
            case CameraController.MotionOption.Linear:
                // 线性运动：y = 1 - |t - 1|，构成一个对称三角波
                return 1f - Mathf.Abs(modT - 1f);

            case CameraController.MotionOption.Cos:
                // 缓入缓出：使用余弦函数构造平滑过渡
                return (1f - Mathf.Cos(Mathf.PI * t)) / 2f;

            case CameraController.MotionOption.ArcCos:
                // 缓出缓入：分段 acos 实现两段平滑过渡（对称波形）
                if (modT <= 1f)
                    return Mathf.Acos(-2f * modT + 1f) / Mathf.PI;
                else
                    return Mathf.Acos(-2f * (2f - modT) + 1f) / Mathf.PI;

            default:
                return 0f;
        }
    }
}
