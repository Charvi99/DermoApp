//using System;
//using UnityEngine;
//using Intel.RealSense;
//using UnityEngine.Rendering;
//using UnityEngine.Assertions;
//using System.Runtime.InteropServices;
//using System.Threading;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using UnityEngine.XR;
//using System.Threading.Tasks;
//using static RsPoseStreamTransformer;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//public class RsLoadPointcloud : MonoBehaviour
//{
//    public Mesh mesh;
//    public Texture2D uvmap;



//    [NonSerialized]
//    private Vector3[] vertices;


//    void Start()
//    {
//        ResetMesh(1280, 720);
//        //await Task.Run(() => LoadPointcloud("test.txt", "uvmapSave.png"));
//        //LoadPointcloud("test.txt", "uvmapSave.png");


//    }
//    //==================================================

//    //==================================================



//    private void ResetMesh(int width, int height)
//    {
//        Assert.IsTrue(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
//        uvmap = new Texture2D(width, height, TextureFormat.RGFloat, false, true)
//        {
//            wrapMode = TextureWrapMode.Clamp,
//            filterMode = FilterMode.Point,
//        };
//        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_UVMap", uvmap);

//        if (mesh != null)
//            mesh.Clear();
//        else
//            mesh = new Mesh()
//            {
//                indexFormat = IndexFormat.UInt32,
//            };

//        vertices = new Vector3[width * height];

//        var indices = new int[vertices.Length];
//        for (int i = 0; i < vertices.Length; i++)
//            indices[i] = i;

//        mesh.MarkDynamic();
//        mesh.vertices = vertices;

//        var uvs = new Vector2[width * height];
//        Array.Clear(uvs, 0, uvs.Length);
//        for (int j = 0; j < height; j++)
//        {
//            for (int i = 0; i < width; i++)
//            {
//                uvs[i + j * width].x = i / (float)width;
//                uvs[i + j * width].y = j / (float)height;
//            }
//        }

//        mesh.uv = uvs;

//        mesh.SetIndices(indices, MeshTopology.Points, 0, false);
//        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

//        GetComponent<MeshFilter>().sharedMesh = mesh;
//    }

//    void OnDestroy()
//    {

//        if (mesh != null)
//            Destroy(null);
//    }

//    public void LoadPointcloud(string meshPath, string texturePath)
//    {
//        mesh.vertices = LoadMesh(meshPath);
//        mesh.UploadMeshData(false);

//        byte[] FileData;
//        if (File.Exists(texturePath))
//        {
//            FileData = File.ReadAllBytes(texturePath);
//            uvmap = new Texture2D(2, 2);           // Create new "empty" texture
//            if (uvmap.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
//                uvmap.Apply();                 // If data = readable -> return texture
//        }

//    }

//    private void SavePointcloud(string meshPath, string texturePath)
//    {
//        SaveMesh(mesh.vertices, "test.txt");

//        byte[] uvmapSave = uvmap.EncodeToPNG();
//        File.WriteAllBytes("uvmapSave.png", uvmapSave);

//    }

//    public void SaveMesh(Vector3[] tab, string path)
//    {
//        FileStream fs = File.Create(path);
//        BinaryFormatter bf = new BinaryFormatter();
//        Vector3S[] vector3s = new Vector3S[tab.Length];
//        for (int i = 0; i < tab.Length; i++)
//            vector3s[i] = new Vector3S(tab[i]);

//        bf.Serialize(fs, vector3s);
//        fs.Close();
//    }
//    public Vector3[] LoadMesh(string filepath)
//    {
//        var formatter = new BinaryFormatter();

//        Vector3S[] data;
//        using (FileStream filestream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
//        {
//            data = (Vector3S[])formatter.Deserialize(filestream);

//            filestream.Close();
//        }
//        Vector3[] output = new Vector3[data.Length];
//        for (int i = 0; i < data.Length; i++)
//        {
//            output[i] = data[i].AsVector3;
//        }

//        return output;
//    }
//}

////[Serializable]
////public struct Vector3S
////{
////    #region Constructor

////    public Vector3S(Vector3 vector3)
////    {
////        _x = vector3.x;
////        _y = vector3.y;
////        _z = vector3.z;
////    }

////    public Vector3S(float x, float y, float z)
////    {
////        _x = x;
////        _y = y;
////        _z = z;
////    }

////    #endregion

////    #region Inspector Fields
////    [SerializeField]
////    private float _x;

////    [SerializeField]
////    private float _y;

////    [SerializeField]
////    private float _z;

////    #endregion

////    #region Properties
////    public float X { get => _x; set => _x = value; }
////    public float Y { get => _y; set => _y = value; }
////    public float Z { get => _z; set => _z = value; }

////    /// <summary>
////    /// Returns the Vector3S as a Unity Vector3.
////    /// </summary>
////    public Vector3 AsVector3
////    {
////        get
////        {
////            return new Vector3(_x, _y, _z);
////        }
////    }

////    #endregion
////}

