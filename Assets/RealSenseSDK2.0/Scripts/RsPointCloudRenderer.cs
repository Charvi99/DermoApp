using System;
using UnityEngine;
using Intel.RealSense;
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//using static Codice.CM.Common.Serialization.PacketFileReader;
using static UnityEngine.Mesh;
using System.Threading.Tasks;
using System.IO.Pipes;
//using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RsPointCloudRenderer : MonoBehaviour
{
    public RsFrameProvider Source;
    public Mesh mesh;
    public Texture2D uvmap;

    [NonSerialized]
    private Vector3[] vertices;

    FrameQueue q;
    private bool freeze;

    void Start()
    {
        freeze = false;
        Source.OnStart += OnStartStreaming;
        Source.OnStop += Dispose;
    }

    private void OnStartStreaming(PipelineProfile obj)
    {
        q = new FrameQueue(1);

        using (var depth = obj.Streams.FirstOrDefault(s => s.Stream == Intel.RealSense.Stream.Depth && s.Format == Format.Z16).As<VideoStreamProfile>())
            ResetMesh(depth.Width, depth.Height);

        Source.OnNewSample += OnNewSample;
    }

    public void ResetMesh(int width, int height)
    {
        Assert.IsTrue(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        uvmap = new Texture2D(width, height, TextureFormat.RGFloat, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point,
        };
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvmap);

        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh()
            {
                indexFormat = IndexFormat.UInt32,
            };

        vertices = new Vector3[width * height];

        var indices = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            indices[i] = i;

        mesh.MarkDynamic();
        mesh.vertices = vertices;

        var uvs = new Vector2[width * height];
        Array.Clear(uvs, 0, uvs.Length);
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                uvs[i + j * width].x = i / (float)width;
                uvs[i + j * width].y = j / (float)height;
            }
        }

        mesh.uv = uvs;

        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        //mesh.triangles = new int[width * height];
        //for (int i = 0; i < mesh.triangles.Length; i++)
        //    mesh.triangles[i] = i;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void OnDestroy()
    {
        if (q != null)
        {
            q.Dispose();
            q = null;
        }

        if (mesh != null)
            Destroy(null);
    }

    private void Dispose()
    {
        Source.OnNewSample -= OnNewSample;

        if (q != null)
        {
            q.Dispose();
            q = null;
        }
    }

    private void OnNewSample(Frame frame)
    {
        if (q == null)
            return;
        try
        {
            if (frame.IsComposite)
            {
                using (var fs = frame.As<FrameSet>())
                using (var points = fs.FirstOrDefault<Points>(Intel.RealSense.Stream.Depth, Format.Xyz32f))
                {
                    if (points != null)
                    {
                        q.Enqueue(points);
                    }
                }
                return;
            }

            if (frame.Is(Extension.Points))
            {
                q.Enqueue(frame);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    public Points getPoints()
    {
        Points points;
        if (q.PollForFrame<Points>(out points))
            return points;
        else
            return null;
    }

    protected void LateUpdate()
    {
        if(freeze == false)
            if (q != null)
            {
                Points points;
                if (q.PollForFrame<Points>(out points))
                    using (points)
                    {
                        if (points.Count != mesh.vertexCount)
                        {
                            using (var p = points.GetProfile<VideoStreamProfile>())
                                ResetMesh(p.Width, p.Height);
                        }

                        if (points.TextureData != IntPtr.Zero)
                        {
                            uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
                            uvmap.Apply();
                        }

                        if (points.VertexData != IntPtr.Zero)
                        {
                            points.CopyVertices(vertices);

                            mesh.vertices = vertices;
                            mesh.UploadMeshData(false);
                        }
                    }
            }
    }
    public void SetPointcloud(Vector3[] vertices, Texture2D texture, Texture2D textureUV)
    {
        
        if(vertices != null)
        {
            Vector3[] vertices_temp = new Vector3[921600];
            vertices_temp = vertices;
            mesh.vertices = vertices_temp;
            mesh.UploadMeshData(false);
        }

        if (textureUV != null)
        {
            uvmap = new Texture2D(texture.width, texture.height, TextureFormat.RGFloat, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
            };
            uvmap = textureUV;           
            uvmap.Apply();
        }
        GetComponent<MeshRenderer>().material.SetTexture("_MainTex",texture);
        GetComponent<MeshRenderer>().material.SetTexture("_UVMap", textureUV);

    }
    public void SetFreeze(bool val)
    {
        freeze = val;
    }
    public void SavePointcloud(string path)
    {
        SaveMesh(mesh.vertices, path + @"\pc.txt");
        Texture2D texture = (Texture2D)GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
        Texture2D UV = (Texture2D)GetComponent<MeshRenderer>().material.GetTexture("_UVMap");
        SaveTexture(texture, UV, path);


    }
    public void SaveMesh(Vector3[] tab, string path)
    {
        using (FileStream fs = File.Create(path))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                for (int i = 0; i < tab.Length; i++)
                {
                    if(tab[i].x != 0 && tab[i].y != 0 && tab[i].z != 0)
                    {
                        string x = string.Format("{0:0.#######}", tab[i].x);
                        string y = string.Format("{0:0.#######}", tab[i].y);
                        string z = string.Format("{0:0.#######}", tab[i].z);
                        sw.Write(i.ToString() + ";" + x + ";" + y + ";" + z + '\n');
                    }
                }
            }
            fs.Close();
        }
       
    }
    public void SaveTexture(Texture2D texture, Texture2D UV, string path)
    {
        byte[] textureSave = texture.EncodeToPNG();
        File.WriteAllBytes(path + @"\pc_texture.png", textureSave);
        byte[] uvmapSave = UV.EncodeToPNG();
        File.WriteAllBytes(path + @"\pc_uv.png", uvmapSave);
    }
    public async void LoadPointcloud(string meshPath, string texturePath)
    {
        mesh.vertices = await LoadMesh(meshPath);
        mesh.UploadMeshData(false);

        uvmap = LoadTexture(texturePath);           // Create new "empty" texture
        if(uvmap != null)
            uvmap.Apply();

    }
    public Texture2D LoadTexture(string filepath)
    {
        byte[] FileData;
        if (File.Exists(filepath))
        {
            FileData = File.ReadAllBytes(filepath);
            Texture2D newTexture = new Texture2D(2, 2);           // Create new "empty" texture
            if (newTexture.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return newTexture;                 // If data = readable -> return texture
            return null;
        }
        return null;
    }
    public async Task<Vector3[]> LoadMesh(string filepath)
    {
        Vector3[] output = new Vector3[921600];
        await Task.Run(() =>
        {
            using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string b = sr.ReadLine();
                        var bsplit = b.Split(';');
                        int nextVectIndex = int.Parse(bsplit[0]);
                        //while (nextVectIndex < i)
                        //    output[i++] = new Vector3(0,0,0);
                        output[nextVectIndex] = new Vector3(float.Parse(bsplit[1]), float.Parse(bsplit[2]), float.Parse(bsplit[3]));
                    }
                }
                fs.Close();
            }
        });

        return output;
    }

    public Vector3 SearchForPoint(GameObject gameObject)
    {
        //Vector3 pos = gameObject.transform.position;
        //float distance = 10000;
        //Vector3 matchingPoint = new Vector3();
        ////int i = 0;
        ////var t = mesh.triangles;
        //foreach(Vector3 scanPoint in mesh.vertices)
        //{
        //    if(scanPoint != Vector3.zero)
        //    {
        //        float newDistance = Vector3.Distance(pos, scanPoint);
        //        if(newDistance < distance)
        //        {
        //            distance = newDistance;
        //            matchingPoint = new Vector3(scanPoint.x, scanPoint.y, scanPoint.z);
                   
        //            //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //            ////sphere.transform.parent = gameObject.transform;
        //            //sphere.transform.position = scanPoint;
        //            //sphere.transform.localScale = Vector3.one / 50;
        //        }


        //    }
        //    //i++;
        //}
        //return matchingPoint;

        Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        float distance = 10000;
        Vector3 matchingPoint = new Vector3();
        foreach (Vector3 scanPoint in mesh.vertices)
        {
            if (scanPoint != Vector3.zero)
            {

                float newDistance = Vector2.Distance(pos, new Vector2(scanPoint.x, scanPoint.y));
                if (newDistance < distance)
                {
                    distance = newDistance;
                    matchingPoint = new Vector3(scanPoint.x, scanPoint.y, scanPoint.z);

                }


            }
        }
        return matchingPoint;

    }
}

//[Serializable]
//public struct Vector3S
//{
//    #region Constructor

//    public Vector3S(Vector3 vector3)
//    {
//        _x = vector3.x;
//        _y = vector3.y;
//        _z = vector3.z;
//    }

//    public Vector3S(float x, float y, float z)
//    {
//        _x = x;
//        _y = y;
//        _z = z;
//    }

//    #endregion

//    #region Inspector Fields
//    [SerializeField]
//    private float _x;

//    [SerializeField]
//    private float _y;

//    [SerializeField]
//    private float _z;

//    #endregion

//    #region Properties
//    public float X { get => _x; set => _x = value; }
//    public float Y { get => _y; set => _y = value; }
//    public float Z { get => _z; set => _z = value; }

//    /// <summary>
//    /// Returns the Vector3S as a Unity Vector3.
//    /// </summary>
//    public Vector3 AsVector3
//    {
//        get
//        {
//            return new Vector3(_x, _y, _z);
//        }
//    }

//    #endregion
//}
