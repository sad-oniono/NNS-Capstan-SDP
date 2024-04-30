using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using Drydock;
using System.Threading;
using System;

public class ArduinoInterface
{
    SerialPort arduino_port;
    Thread DataTransfer;
    string name = "";
    Dock drydock;

    static int n_vars = 10;
    static int n_capstans = 6;
    int c = 0;
    double[,] amps = new double[n_capstans, n_vars];
    public double[] avg_amps = new double[n_capstans];
    bool amps_initialized = false;


    public ArduinoInterface(string location, ref Dock _drydock)
    {
        name = location;
        drydock = _drydock;
        try
        {
            arduino_port = new SerialPort(location, 70000);
            //arduino_port.DtrEnable = true;
            //arduino_port.RtsEnable = true;
            arduino_port.Open();

            arduino_port.WriteLine(CreateString());

            DataTransfer = new Thread(Update);
            DataTransfer.Start();
        }
        catch
        {
            Debug.LogWarning("Finding arduino failed (is the serialport locaiton set correctly?)");
        }
    }

    public void Close()
    {
        arduino_port.Close();
    }

    private void Update()
    {
        while(true)
        {
            arduino_port.WriteLine(CreateString());
            Debug.Log(CreateString());
            Thread.Sleep(1000);
            //var amps_string = arduino_port.ReadTo("\n");
            //amps_string = amps_string.Substring(0, amps_string.Length - 1);
            //double[] amp = Array.ConvertAll(amps_string.Split(' '), Double.Parse);
            //Debug.Log(amps_string);
            //AddAmpMeasurement(amp);
            //ProcessAmps();
        }
    }

    private string CreateString()
    {

        string output = "";

        foreach (Capstan c in drydock.capstans)
        {
            output += c.set_speed + " ";
        }

        return output;
    }

    private void ProcessAmps()
    {
        double[] sums = new double[amps.GetLength(0)];
        if (amps_initialized)
        {

            for (int i = 0; i < amps.GetLength(0); i++)
            {

                for (int j = 0; j < amps.GetLength(1); j++)
                {

                    sums[i] += Math.Pow(amps[i, j], 2);

                }

                sums[i] *= 1 / amps.GetLength(1);
                sums[i] = Math.Sqrt(sums[i]);
            }

            avg_amps = sums;
        }

    }

    private void AddAmpMeasurement(double[] current_amps)
    {
        for (int i = 0; i < current_amps.Length; i++)
        {

            amps[i, c] = current_amps[i];

        }

        c++;
        if (c >= n_vars)
        {
            c = 0;
            amps_initialized = true;
        }

    }

}
