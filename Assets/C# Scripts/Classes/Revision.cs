using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Revision
{
    public DateTime Date;
    public string Note;
    public string ImgPath;
    public int Index;
    public string Dir;
    public Vector3[] Pointcloud { 
        get;
        set;
    }
    public Texture2D Texture;
    public Texture2D TextureUV;

    public GameObject MoleDetailPanel_gameobject;
    public GameObject MoleScrollViewElement_gameobject;
    public GameObject RevButton_gameobject;
    public Texture2D RevImg;

    public float x;
    public float y;
    public float z;
    public float qx;
    public float qy;
    public float qz;
    public float qw;

    public Revision()
    {
        Date = DateTime.Now;
        Dir = "";
        Note = "";
    }
    public Revision(string dir, string index, Vector3 position, Quaternion rotation)
    {
        Index = int.Parse(index);
        Dir = dir;

        x = position.x;
        y = position.y;
        z = position.z;

        qx = rotation.x;
        qy = rotation.y;
        qz = rotation.z;
        qw = rotation.w;
    }
    public Revision(string dir, string index, string posRot)
    {
        Index = int.Parse(index);
        Dir = dir;

        string[] vals = posRot.Split(':');
        x = float.Parse(vals[0]);
        y = float.Parse(vals[1]);
        z = float.Parse(vals[2]);
        qx = float.Parse(vals[3]);
        qy = float.Parse(vals[4]);
        qz = float.Parse(vals[5]);
        qw = float.Parse(vals[6]);

        //LoadData();
    }

    public void LoadData()
    {
        string[] paths = { @"Assets\PacientDatabase", Dir, "data.csv" };
        string path = Path.Combine(paths);

        if (!System.IO.File.Exists(path))
            return;

        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                Date = DateTime.Parse(values[0]);
                string[] imgPaths = { @"Assets\PacientDatabase", Dir, "img.jpg" };
                string imgPath = Path.Combine(imgPaths);
                ImgPath = imgPath;
                //RevImg = Resources.Load<Sprite>(ImgPath);
                Note = values[1];
                string[] vals = values[2].Split(':');
                x = float.Parse(vals[0]);
                y = float.Parse(vals[1]);
                z = float.Parse(vals[2]);
                qx = float.Parse(vals[3]);
                qy = float.Parse(vals[4]);
                qz = float.Parse(vals[5]);
                qw = float.Parse(vals[6]);
            }
        }

    }
    public string getXYZCoordinatesString()
    {
        string output = x.ToString() + ":" +
                        y.ToString() + ":" +
                        z.ToString() + ":" +
                        qx.ToString() + ":" +
                        qy.ToString() + ":" +
                        qz.ToString() + ":" +
                        qw.ToString();
        return output;
    }
    public Vector3 getXYZCoordinates()
    {
        string output = x.ToString() + ":" +
                        y.ToString() + ":" +
                        z.ToString() + ":" +
                        qx.ToString() + ":" +
                        qy.ToString() + ":" +
                        qz.ToString() + ":" +
                        qw.ToString();
        return new Vector3(x,y,z);
    }
}