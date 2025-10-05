using UnityEngine;
using System.IO;
using System;

public class DataSaver : MonoBehaviour
{
    public CameraController cameraController;
    public string participant = "P01";
    public string trial = "T01";
    public string presentationOption;
    public string experiment;

    private string path;
    private bool headerWritten = false;
    private int frameCount = 0;

    void Start()
    {
        presentationOption = cameraController.presentationOption.ToString();
        experiment = cameraController.experiment.ToString();

        string folder = Path.Combine(Application.dataPath, "Data");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        string filename = $"{participant}_{presentationOption}_{experiment}_{trial}.csv";
        path = Path.Combine(folder, filename);

        // 检查文件是否已存在header
        headerWritten = File.Exists(path) && new FileInfo(path).Length > 0;
        if (!headerWritten)
        {
            string header = "local_time,frame,participant,trial,distance,speed,amplitude,ratio,pos_x,pos_y,pos_z";
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(header);
            }
            headerWritten = true;
        }
    }

    void Update()
    {
        frameCount++;
        SaveData();
    }

    public void SaveData()
    {
        float distance = cameraController.distance;
        float speed = cameraController.speed;
        float amplitude = cameraController.amplitude;
        float ratio = cameraController.ratio;
        Vector3 pos = cameraController.camMain.transform.position;

        string localTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        string row = $"{localTime},{frameCount},{participant},{trial},{distance},{speed},{amplitude},{ratio},{pos.x},{pos.y},{pos.z}";

        using (StreamWriter sw = new StreamWriter(path, true))
        {
            sw.WriteLine(row);
        }
    }
}
