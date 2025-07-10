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
    public Vector3 moveDir;

    [Header("Capture Mode")]
    public bool captureModeOn = false;
    public string folder;
    public int index = 0;             // current total frame
    public int cycleFrameCount = 120; // frame count per loop
    public int endFrameIndex = 240;   // total frame count

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

        switch (presentationOption)
        {
            case PresentationOption.Continuity:
                SetActiveCameras(true, false, false);
                SetActiveImages(true, false, false);
                break;

            case PresentationOption.LuminanceMixing:
                SetActiveCameras(false, true, true);
                SetActiveImages(false, true, true);
                break;

            case PresentationOption.Stillness:
                SetActiveCameras(true, true, true);
                SetActiveImages(true, true, true);
                break;
        }

        folder = Path.Combine("Screenshots", motionOption.ToString(), presentationOption.ToString());
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

    void Update()
    {
        amplitude = (serialReader != null && serialReader.enabled) ? serialReader.value : 1.0f;

        switch (presentationOption)
        {
            case PresentationOption.Continuity:
                HandleMode(continuity: true);
                break;

            case PresentationOption.LuminanceMixing:
                HandleMode(luminanceMixing: true);
                break;

            case PresentationOption.Stillness:
                HandleMode(stillness: true);
                break;
        }
    }

    void HandleMode(bool continuity = false, bool luminanceMixing = false, bool stillness = false)
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

            // Update ratio and image alpha as needed
            UpdatePresentationEffect(ease, luminanceMixing, stillness);

            string path = Path.Combine(folder, $"{motionOption}_{presentationOption}_{(index + 1):D3}.png");
            ScreenCapture.CaptureScreenshot(path);
            index++;
        }
        else
        {
            elapsedTime += Time.deltaTime * speed;
            int currentLoopPhase = Mathf.FloorToInt(elapsedTime / 2f); // Full cycle: 2 seconds
            if (currentLoopPhase > previousLoopPhase)
            {
                loopCount++;
                previousLoopPhase = currentLoopPhase;
            }

            float t = elapsedTime % 2f;
            float ease = CalculatePos(t);
            Vector3 pos = leftLimit + moveDir * (distance * ease);
            camMain.transform.position = pos;

            // Update ratio and image alpha as needed
            UpdatePresentationEffect(ease, luminanceMixing, stillness);
        }
    }

    void UpdatePresentationEffect(float ease, bool luminanceMixing, bool stillness)
    {
        if (luminanceMixing)
        {
            //ratio = Vector3.Distance(camLeft.transform.position, camMain.transform.position)
            //        / Vector3.Distance(camLeft.transform.position, camRight.transform.position);

            float baseRatio = Vector3.Distance(camLeft.transform.position, camMain.transform.position) / Vector3.Distance(camLeft.transform.position, camRight.transform.position);
            ratio = Mathf.Lerp(0.5f, baseRatio, amplitude);

            //float rightRatio = ratio * amplitude;
            //float leftRatio = (1.0f - ratio) * amplitude;

            float rightRatio = ratio;
            float leftRatio = 1.0f - ratio;

            //ratio = ratio * amplitude;
            //var imgRight = ImageRight.GetComponent<RawImage>();
            //Color cRight = imgRight.color;
            //cRight.a = Mathf.Clamp01(ratio);
            //imgRight.color = cRight;

            //var imgLeft = ImageLeft.GetComponent<RawImage>();
            //Color cLeft = imgLeft.color;
            //cLeft.a = amplitude;
            //imgLeft.color = cLeft;

            // Left image
            var imgLeft = ImageLeft.GetComponent<RawImage>();
            Color cLeft = imgLeft.color;
            cLeft.a = Mathf.Clamp01(leftRatio);
            imgLeft.color = cLeft;

            //Right image
            var imgRight = ImageRight.GetComponent<RawImage>();
            Color cRight = imgRight.color;
            cRight.a = Mathf.Clamp01(rightRatio);
            imgRight.color = cRight;
        }
        else if (stillness)
        {
            //float totalDistance = Vector3.Distance(camLeft.transform.position, camRight.transform.position);
            //ratio = Vector3.Distance(camLeft.transform.position, camMain.transform.position) / totalDistance;

            float baseRatio = Vector3.Distance(camLeft.transform.position, camMain.transform.position) / Vector3.Distance(camLeft.transform.position, camRight.transform.position);
            ratio = Mathf.Lerp(0.5f, baseRatio, amplitude);
            ratio = Mathf.Clamp(ratio, 0.05f, 0.95f);

            //float leftRatio = ratio * amplitude;
            //float rightRatio = (1.0f - ratio) * amplitude;

            float rightRatio = ratio;
            float leftRatio = 1.0f - ratio;

            // Left image
            var imgLeft = ImageLeft.GetComponent<RawImage>();
            Color cLeft = imgLeft.color;
            cLeft.a = Mathf.Clamp01(leftRatio);
            imgLeft.color = cLeft;

            // Right image
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

    void SetActiveCameras(bool main, bool left, bool right)
    {
        camMain.GetComponent<Camera>().enabled = main;
        camLeft.GetComponent<Camera>().enabled = left;
        camRight.GetComponent<Camera>().enabled = right;
    }

    void SetActiveImages(bool mid, bool left, bool right)
    {
        if (ImageMid) ImageMid.SetActive(mid);
        if (ImageLeft) ImageLeft.SetActive(left);
        if (ImageRight) ImageRight.SetActive(right);
    }

    void OnValidate()
    {
        amplitude = Mathf.Clamp(amplitude, 0f, 1f); // Keep amplitude in [0,1]
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftLimit, rightLimit);
        Gizmos.DrawSphere(leftLimit, 0.02f);
        Gizmos.DrawSphere(rightLimit, 0.02f);
    }
}
