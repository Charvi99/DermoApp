using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinatesClass
{
    public float x;
    public float y;
    public float z;
    public float qx;
    public float qy;
    public float qz;
    public float qw;
    public CoordinatesClass()
    {
        x = 0;
        y = 0;
        z = 0;
        qx = 0;
        qy = 0;
        qz = 0;
        qw = 0;
    }

    public CoordinatesClass(string[] vals)
    {
        x = float.Parse(vals[0]);
        y = float.Parse(vals[1]);
        z = float.Parse(vals[2]);
        qx = float.Parse(vals[3]);
        qy = float.Parse(vals[4]);
        qz = float.Parse(vals[5]);
        qw = float.Parse(vals[6]);
    }
}
