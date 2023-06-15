using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.IO.Ports;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using Intel.RealSense;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Matrix4x4 = System.Numerics.Matrix4x4;

public class socketTest : MonoBehaviour
{
    static Socket listener;
    private CancellationTokenSource source;
    public ManualResetEvent allDone;
    public Renderer objectRenderer;
    private Color matColor;
    private Vector3 newPos;
    private Quaternion newQuat;
    private Quaternion newQuatArduino;
    private Vector3 newAngle;

    public static readonly int PORT = 5255;
    public static readonly int WAITTIME = 1;


    private Matrix transformMatrix;
    private Matrix transformTipMatrix;

    //private SerialPort port;
    [SerializeField] private static string receivedString;
    private Thread readThread;
    private main mainScript;
    private Action buttonClick;
    private static bool ButtonFlag;

    public GameObject navigator;
    private NavScript navScript;
    socketTest()
    {
        source = new CancellationTokenSource();
        allDone = new ManualResetEvent(false);
    }

    // Start is called before the first frame update
    async void Awake()
    {
        //port = new SerialPort("COM12", 115200);
        //port.WriteTimeout = 300;
        //port.DtrEnable = true;
        //port.RtsEnable = true;
        //port.Open();


        //if (port.IsOpen)
        //{
        //    readThread = new Thread(ReadThread);
        //    readThread.Start();
        //}
        ButtonFlag = false;
        mainScript = GameObject.Find("Main").GetComponent<main>();

        objectRenderer = GetComponent<Renderer>();
        transform.position = new Vector3(0, 0, 0);
        transformMatrix = CreateTransformMatrix();
        transformTipMatrix = CreateTipMatrix();
        arduinoPos = new Vector3(0, 0, 0);
        arduinoVel = new Vector3(0, 0, 0);

        navScript = navigator.GetComponent<NavScript>();

        await Task.Run(() => ListenEvents(source.Token));
        newQuatArduino = new Quaternion();


    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = new Vector3(newPos.x, newPos.y, newPos.z);
        transform.position = newPos;
        transform.rotation = newQuat;
        navScript.SetValues(newPos.x, newPos.y, newPos.z);
        //transform.position = arduinoPos;
        //transform.rotation = newQuatArduino;
        if (ButtonFlag)
        {
            ButtonFlag = false;
            mainScript.DermatoscopeClick();
            

        }
    }
    private DateTime prevTime;
    private Vector3 arduinoPos;
    private Vector3 arduinoVel;
    private void integratePosition(float ax, float ay, float az)
    {
        
        long dt = (DateTime.Now.Ticks - prevTime.Ticks)/100000;
        if(dt > 100000)
        {
            prevTime = DateTime.Now;
            return;
        }
        prevTime = DateTime.Now;


        Vector3 arduinoVelNew = new Vector3(ax * dt, ay * dt, az * dt);
        arduinoVel = arduinoVel + arduinoVelNew;

        Vector3 arduinoPosNew = new Vector3(arduinoVel.x * dt, arduinoVel.y * dt, arduinoVel.z * dt);
        arduinoPos = arduinoPos + arduinoPosNew;
    }
    private void SerialPortHandler(string inputData)
    {
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";
        string[] splitString;

        switch (inputData[0])
        {
            case 'A': //acc
               

                splitString = inputData.Split(':');
                float ax = float.Parse(splitString[1], NumberStyles.Any, ci);
                float ay = float.Parse(splitString[2], NumberStyles.Any, ci);
                float az = float.Parse(splitString[3], NumberStyles.Any, ci);
                integratePosition(ax, ay, az);
                Debug.Log(inputData);
                break;

            case 'B': //btn
                ButtonFlag = true;

                break;

            case 'R': //rot

                break;

            case 'M': //MPU9250 log
                Debug.Log(inputData);

                break;
            case 'Q': //quaternion
                splitString = inputData.Split(':');
                float qx = float.Parse(splitString[1], NumberStyles.Any, ci);
                float qy = float.Parse(splitString[2], NumberStyles.Any, ci);
                float qz = float.Parse(splitString[3], NumberStyles.Any, ci);
                float qw = float.Parse(splitString[4], NumberStyles.Any, ci);
                newQuatArduino = new Quaternion(qx, qy, qz, qw);
                Debug.Log(inputData);

                break;
            default:
                break;
        }
    }

    void ReadThread()
    {
        //while (port.IsOpen)
        //{
        //    receivedString = port.ReadLine();
        //    SerialPortHandler(receivedString);
        //    //SerialFlag = true;
        //}
    }



    private void ListenEvents(CancellationToken token)
    {


        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //IPAddress ipAddress = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        //IPAddress ipAddress = IPAddress.Parse("192.168.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, PORT);


        listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);


            while (!token.IsCancellationRequested)
            {
                allDone.Reset();

                print("Waiting for a connection... host :" + ipAddress.MapToIPv4().ToString() + " port : " + PORT);
                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                while (!token.IsCancellationRequested)
                {
                    if (allDone.WaitOne(WAITTIME))
                    {
                        break;
                    }
                }

            }

        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    void AcceptCallback(IAsyncResult ar)
    {
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);

        allDone.Set();

        StateObject state = new StateObject();
        state.workSocket = handler;
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
    }

    void ReadCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        int read = handler.EndReceive(ar);

        if (read > 0)
        {
            state.colorCode.Append(Encoding.ASCII.GetString(state.buffer, 0, read));
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        else
        {
            if (state.colorCode.Length > 1)
            {
                string content = state.colorCode.ToString();
                print($"Read {content.Length} bytes from socket.\n Data : {content}");
                if (content == "BBBBB")
                    ButtonFlag = true;
                else
                    SetColors(content);
            }
            handler.Close();
        }
    }

    //Set color to the Material
    private void SetColors(string data)
    {
        Debug.Log(data);
        if(data.Length < 5)
        {
            mainScript.DermatoscopeClick();
            return;
        }

        string[] raw_data = data.Split(';');
        string[] xyz = raw_data[0].Split(',');
        string[] quat = raw_data[1].Split(',');
        CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        ci.NumberFormat.CurrencyDecimalSeparator = ".";

        float x = float.Parse(xyz[0], NumberStyles.Any, ci);
        float y = float.Parse(xyz[1], NumberStyles.Any, ci);
        float z = float.Parse(xyz[2], NumberStyles.Any, ci);


        float qx = float.Parse(quat[0], NumberStyles.Any, ci) + (float)(0.0);
        float qy = float.Parse(quat[1], NumberStyles.Any, ci) + (float)(0.0);
        float qz = float.Parse(quat[2], NumberStyles.Any, ci) + (float)(0.0);
        float qw = float.Parse(quat[3], NumberStyles.Any, ci) + (float)(0.0);

        //float theta = (float)(Math.Sqrt(qx * qx + qy * qy + qz * qz) * 180 / Math.PI);

        ////Vector3 axis = new Vector3(qy, qz, qx);
        //Vector3 axis = new Vector3(qy, qx, qz);

        //Quaternion pom = Quaternion.AngleAxis(theta, axis);
        //var a = pom.eulerAngles;
        //pom.eulerAngles = new Vector3(-a.x, a.y + 90, a.z);
        //newQuat = pom;

        newQuat = new Quaternion(qx, qy, qz, qw);

        newQuat.eulerAngles = new Vector3(newQuat.eulerAngles.z - 90.0f,
                                          newQuat.eulerAngles.y + 90.0f,
                                          newQuat.eulerAngles.x + 90.0f 
                                          );

        int scale = 1;
        newPos = new Vector3((float)((x / scale) + 0.047),
                             (float)((-y / scale) + 0.048),
                             (float)((z / scale) - 0.02));


    }
    private Matrix CreateTransformMatrix()
    {
        Matrix matrix = new Matrix();
        matrix.CreateEye();
        matrix.data[1][3] = (float)0.03;
        return matrix;
    }
    private Matrix CreateTipMatrix()
    {
        Matrix matrix = new Matrix();
        matrix.CreateEye();
        matrix.data[2][3] = (float)-0.2245;
        return matrix;
    }
    private Matrix ToMatrix(Vector3 trans)
    {
        Matrix matrix = new Matrix();
        matrix.CreateEye();
        matrix.SetOffset(trans);
        return matrix;
    }

    private Matrix MultiplyMatrix(Matrix matrixA, Matrix matrixB)
    {
        Matrix output = new Matrix();
        for (int matrix1_row = 0; matrix1_row < 4; matrix1_row++)
        {
            // for each matrix 1 row, loop through matrix 2 columns  
            for (int matrix2_col = 0; matrix2_col < 4; matrix2_col++)
            {
                // loop through matrix 1 columns to calculate the dot product  
                for (int matrix1_col = 0; matrix1_col < 4; matrix1_col++)
                {
                    output.data[matrix1_row][matrix2_col] +=
                      matrixA.data[matrix1_row][matrix1_col] *
                      matrixB.data[matrix1_col][matrix2_col];
                }
            }
        }
        return output;
    }
    


    private void OnDestroy()
    {
        source.Cancel();
        Debug.Log("Thread stopped...");
        //readThread.Abort();
        //port.Close();
    }

    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder colorCode = new StringBuilder();
    }

    private class Matrix
    {
        public List<List<float>> data;
        public Matrix()
        {
            data = new List<List<float>>();
            for (int i = 0; i < 4; i++)
            {
                data.Add(new List<float>());
                for(int j = 0; j < 4; j++)
                    data[i].Add(new float());

            }

        }
        public void CreateEye()
        {
            data[0][0] = 1;
            data[1][1] = 1;
            data[2][2] = 1;
            data[3][3] = 1;
        }
        public void SetScale(float scale)
        {
            data[3][3] = scale;
        }
        public void SetOffset(Vector3 trans)
        {
            data[0][3] = trans.x;
            data[1][3] = trans.y;
            data[2][3] = trans.z;
        }
        public Vector3 GetOffset()
        {
            return new Vector3(data[0][3], data[1][3], data[2][3]);
        }
        public void ToMatrix(string trans)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            string[] rows = trans.Split(";");
            for (int i = 0; i < rows.Length; i++)
            {
                string[] items = rows[i].Split(":");
                for (int j = 0; j < items.Length; j++)
                    data[i][j] = float.Parse(items[j], NumberStyles.Any, ci);
                

            }
        }
    }
}