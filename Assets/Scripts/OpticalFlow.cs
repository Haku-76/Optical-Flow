using UnityEngine;
using System.IO;

public class OpticalFlow : MonoBehaviour
{
    public enum MotionType
    {
        Linear,
        EaseInOut,
        EaseOutIn
    }

    public MotionType motionType = MotionType.Linear;

    [Header("Mode Toggle")]
    public bool captureMode = false;
    public string folder;

    [Space(15)]
    public float distance;
    public float speed;

    [Space(15)]
    public Vector3 startPos;
    public Vector3 leftLimit;
    public Vector3 rightLimit;

    [Header("Capture Mode")]
    public int index = 0;             // 当前总帧数
    public int cycleFrameCount = 120; // 一个完整循环的帧数
    public int endFrameIndex = 240;   // 总采集帧数

    [Header("Animation Mode")]
    public float elapsedTime = 0f;
    public int loopCount = 0;
    private float previousLoopPhase = 0f;

    void Start()
    {
        startPos = this.transform.position;
        leftLimit = startPos - this.transform.right * (distance / 2.0f);
        rightLimit = startPos + this.transform.right * (distance / 2.0f);

        this.transform.position = leftLimit;

        folder = $"Screenshots/{motionType}";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    void Update()
    {
        if (captureMode)
        {
            if (index >= endFrameIndex)
            {

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            float phase = (index % cycleFrameCount) / (float)cycleFrameCount * 2f; // 0~2
            float z = CalculateZ(phase);
            this.transform.position = new Vector3(transform.position.x, transform.position.y, z);

            string path = $"{folder}/{motionType}_{index:D3}.png";
            ScreenCapture.CaptureScreenshot(path);

            index++;
        }
        else
        {
            elapsedTime += Time.deltaTime * speed;

            float currentLoopPhase = Mathf.Floor(elapsedTime / 2f);  // Full cycle: 2 seconds
            if (currentLoopPhase > previousLoopPhase)
            {
                loopCount++;
                previousLoopPhase = currentLoopPhase;
            }

            float t = elapsedTime % 2f;
            float z = CalculateZ(t);
            this.transform.position = new Vector3(transform.position.x, transform.position.y, z);

            if (loopCount >= 10)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }

    float CalculateZ(float t)
    {
        float ease = 0f;

        if (t <= 1f)
        {
            switch (motionType)
            {
                case MotionType.Linear:
                    ease = t;
                    break;
                case MotionType.EaseInOut:
                    ease = (1f - Mathf.Cos(Mathf.PI * t)) / 2f;
                    break;
                case MotionType.EaseOutIn:
                    ease = Mathf.Acos(-2f * t + 1f) / Mathf.PI;
                    break;
            }
        }
        else
        {
            float t2 = t - 1f;
            switch (motionType)
            {
                case MotionType.Linear:
                    ease = 1f - t2;
                    break;
                case MotionType.EaseInOut:
                    ease = (1f - Mathf.Cos(Mathf.PI * (1f - t2))) / 2f;
                    break;
                case MotionType.EaseOutIn:
                    ease = Mathf.Acos(-2f * (1f - t2) + 1f) / Mathf.PI;
                    break;
            }
        }

        return leftLimit.z + distance * ease;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 center = startPos;
        Vector3 left = center - transform.right * (distance / 2f);
        Vector3 right = center + transform.right * (distance / 2f);
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.2f);
        Gizmos.DrawSphere(right, 0.2f);
    }
}
