using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class SerialReader : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 9600;
    public int threshold = 1200;
    private int maxValue = 4095;
    private SerialPort serialPort;

    [Space(15)]
    public int rawValue;
    public float value;

    private Thread readThread;
    private bool isRunning = false;
    private int latestRawValue = 0; // 线程安全队列更稳健，这里用变量足够

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 100;
        try
        {
            serialPort.Open();
            isRunning = true;
            readThread = new Thread(ReadSerialData);
            readThread.Start();
            Debug.Log("Serial port opened: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cannot open serial port: " + e.Message);
        }
    }

    void ReadSerialData()
    {
        while (isRunning && serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string line = serialPort.ReadLine();
                int temp;
                if (int.TryParse(line.Trim(), out temp))
                {
                    latestRawValue = temp;
                }
            }
            catch (System.Exception)
            {
                // 超时或其他串口异常，忽略
            }
        }
    }

    void Update()
    {
        rawValue = latestRawValue;
        if (rawValue < threshold)
        {
            value = 0f;
        }
        else
        {
            value = Mathf.Clamp01((rawValue - threshold) / (float)(maxValue - threshold));
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (readThread != null && readThread.IsAlive)
        {
            readThread.Join();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
