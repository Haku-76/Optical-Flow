using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class SerialReader : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 9600;
    public int threshold = 1200;   // 阈值，低于此为0，可在Inspector中修改
    private int maxValue = 4095;    // ADC最大值
    private SerialPort serialPort;

    [Space(15)]
    public int rawValue;
    public float value;

    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = 100;
        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened: " + portName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cannot open serial port: " + e.Message);
        }
    }

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string line = serialPort.ReadLine();
                int temp;
                if (int.TryParse(line.Trim(), out temp))
                {
                    rawValue = temp;
                    if (rawValue < threshold)
                    {
                        value = 0f;
                    }
                    else
                    {
                        value = Mathf.Clamp01((rawValue - threshold) / (float)(maxValue - threshold));
                    }
                }
            }
            catch (System.TimeoutException)
            {
                // 没收到数据，忽略
            }
        }
    }

    void OnApplicationQuit()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
