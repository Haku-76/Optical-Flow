using UnityEngine;
using System.IO;
using UnityEngine.UI;

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
        Continuity,
        LuminanceMixing,
        Stillness
    }

    public SerialReader serialReader;

    [Space(15)]
    public GameObject camMain;
    public GameObject camLeft;
    public GameObject camRight;
    public GameObject ImageMid;
    public GameObject ImageLeft;
    public GameObject ImageRight;

    [Space(15)]
    public MotionOption motionOption = MotionOption.Linear;
    public PresentationOption presentationOption = PresentationOption.Continuity;

    [Space(15)]
    public float distance = 0.01f;
    public float speed = 1.0f;
    [Range(0f, 1f)]
    public float amplitude = 1.0f;
    public float ratio = 0.0f;

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
        //amplitude = serialReader.value;

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

        else if (presentationOption == PresentationOption.Continuity)
        {
            camMain.GetComponent<Camera>().enabled = true;
            camLeft.GetComponent<Camera>().enabled = false;
            camRight.GetComponent<Camera>().enabled = false;
            ImageLeft.SetActive(true);
            ImageLeft.SetActive(false);
            ImageRight.SetActive(false);

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

        else if (presentationOption == PresentationOption.LuminanceMixing)
        {
            camMain.GetComponent<Camera>().enabled = false;
            camLeft.GetComponent<Camera>().enabled = true;
            camRight.GetComponent<Camera>().enabled = true;
            ImageMid.SetActive(false);
            ImageLeft.SetActive(true);
            ImageRight.SetActive(true);

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

            ratio = Vector3.Distance(camLeft.transform.position, camMain.transform.position) / Vector3.Distance(camLeft.transform.position, camRight.transform.position);
            ratio = ratio * amplitude;

            var img = ImageRight.GetComponent<RawImage>();
            Color c = img.color;
            c.a = Mathf.Clamp01(ratio);
            img.color = c;
        }

        else if (presentationOption == PresentationOption.Stillness)
        {
            camMain.GetComponent<Camera>().enabled = true;
            camLeft.GetComponent<Camera>().enabled = true;
            camRight.GetComponent<Camera>().enabled = true;
            ImageMid.SetActive(true);
            ImageLeft.SetActive(true);
            ImageRight.SetActive(true);

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

            float totalDistance = Vector3.Distance(camLeft.transform.position, camRight.transform.position);
            ratio = Vector3.Distance(camLeft.transform.position, camMain.transform.position) / totalDistance;

            float leftRatio = ratio;
            float rightRatio = 1.0f - ratio;

            leftRatio *= amplitude;
            rightRatio *= amplitude;

            // 左侧
            var imgLeft = ImageLeft.GetComponent<RawImage>();
            Color cLeft = imgLeft.color;
            cLeft.a = Mathf.Clamp01(leftRatio);
            imgLeft.color = cLeft;

            // 右侧
            var imgRight = ImageRight.GetComponent<RawImage>();
            Color cRight = imgRight.color;
            cRight.a = Mathf.Clamp01(rightRatio);
            imgRight.color = cRight;


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

    void OnValidate()
    {
        amplitude = Mathf.Clamp(amplitude, 0f, 1f); // 保证 amplitude 始终在 0~1 之间
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawSphere(leftLimit, 0.02f);
        Gizmos.DrawSphere(rightLimit, 0.02f);
    }
}
