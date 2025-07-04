using UnityEngine;
using System.IO;
using static CameraController;

public class CameraController : MonoBehaviour
{
    public enum MotionOption
    {
        Linear,
        Cos,
        ArcCos
    }

    public enum PresentationOption
    {
        Con,
        LuminanceMixing,
        Stillness
    }

    public GameObject camMain;
    public GameObject camLeft;
    public GameObject camRight;

    [Space(15)]
    public MotionOption motionOption = MotionOption.Linear;

    [Space(15)]
    public float distance = 0.01f;
    public float speed = 1.0f;

    [Space(15)]
    public Vector3 centerPos;
    public Vector3 leftLimit;
    public Vector3 rightLimit;
    public Vector3 moveDir; // 新增，运动方向

    [Header("Capture Mode")]
    public bool captureModeOn = false;
    public string folder;
    public int index = 0;             // 当前总帧数
    public int cycleFrameCount = 120; // 一个完整循环的帧数
    public int endFrameIndex = 240;   // 总采集帧数

    [Header("Animation Mode")]
    public float elapsedTime = 0f;
    public int loopCount = 0;
    private int previousLoopPhase = 0;

    void Start()
    {
        centerPos = camMain.transform.position;
        moveDir = camMain.transform.right.normalized;
        leftLimit = centerPos - moveDir * (distance / 2.0f);
        rightLimit = centerPos + moveDir * (distance / 2.0f);

        camMain.transform.position = leftLimit;
        camLeft.transform.position = leftLimit;
        camRight.transform.position = rightLimit;

        folder = Path.Combine("Screenshots", motionOption.ToString());
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    void Update()
    {
        if (captureModeOn)
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
            float ease = CalculatePos(phase);
            Vector3 pos = leftLimit + moveDir * (distance * ease);
            camMain.transform.position = pos;

            string path = Path.Combine(folder, $"{motionOption}_{(index + 1):D3}.png");
            ScreenCapture.CaptureScreenshot(path);

            index++;
        }
        else
        {
            elapsedTime += Time.deltaTime * speed;
            int currentLoopPhase = Mathf.FloorToInt(elapsedTime / 2f);  // Full cycle: 2 seconds
            if (currentLoopPhase > previousLoopPhase)
            {
                loopCount++;
                previousLoopPhase = currentLoopPhase;
            }

            float t = elapsedTime % 2f;
            float ease = CalculatePos(t);
            Vector3 pos = leftLimit + moveDir * (distance * ease);
            camMain.transform.position = pos;

        }
    }

    float CalculatePos(float t)
    {
        float ease = 0f;
        if (t <= 1f)
        {
            switch (motionOption)
            {
                case MotionOption.Linear:
                    ease = t;
                    break;
                case MotionOption.Cos:
                    ease = (1f - Mathf.Cos(Mathf.PI * t)) / 2f;
                    break;
                case MotionOption.ArcCos:
                    ease = Mathf.Acos(-2f * t + 1f) / Mathf.PI;
                    break;
            }
        }
        else
        {
            float t2 = t - 1f;
            switch (motionOption)
            {
                case MotionOption.Linear:
                    ease = 1f - t2;
                    break;
                case MotionOption.Cos:
                    ease = (1f - Mathf.Cos(Mathf.PI * (1f - t2))) / 2f;
                    break;
                case MotionOption.ArcCos:
                    ease = Mathf.Acos(-2f * (1f - t2) + 1f) / Mathf.PI;
                    break;
            }
        }
        return ease;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawSphere(leftLimit, 0.02f);
        Gizmos.DrawSphere(rightLimit, 0.02f);
    }
}
