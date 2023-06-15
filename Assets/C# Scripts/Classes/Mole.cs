using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Mole
{
    

    public string Name;
    public string Dir;
    private bool _isActive;

    public List<Revision> RevisionList;
    public GameObject MoleScrollViewOverview_gameobject;
    public GameObject MoleDetailPanel_gameobject;
    public GameObject MoleScrollViewElement_gameobject;
    public GameObject RevisionPanel_gameobject;

    public Mole()
    {
        Name = "";
        RevisionList = new List<Revision>();
    }
    public Mole(string dir, string name)
    {
        Name = name;
        Dir = dir;
        RevisionList = new List<Revision>();
        LoadDatabase();
    }
    public void LoadDatabase()
    {
        string[] paths = { @"Assets\PacientDatabase", Dir, "RevList.csv" };
        string path = Path.Combine(paths);

        if (!System.IO.File.Exists(path))
            return;

        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                string[] revPaths = { Dir, values[0] };
                string revPath = Path.Combine(revPaths);

                RevisionList.Add(new Revision(revPath, values[1], values[2]));

            }
        }
    }
    public void ActivateMole()
    {
        _isActive = true;
    }
    public void DeactivateMole()
    {
        _isActive = false;
    }
    public bool isActive()
    {
        return _isActive;
    }
}
