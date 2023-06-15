using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class Pacient
{
    public string Name;
    public string Dir;

    public List<Mole> MoleList;

    public Pacient()
    {
        Name = "";
        Dir = "";
        MoleList = new List<Mole>();
    }
    public Pacient(string dir, string name)
    {
        Name = name;
        Dir = dir;
        MoleList = new List<Mole>();
        LoadDatabase();

    }
    public void LoadDatabase()
    {
        string[] paths = { @"Assets\PacientDatabase", Dir, "MoleList.csv" };
        string path = Path.Combine(paths);

        if (!System.IO.File.Exists(path))
            return;

        using (StreamReader reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                string[] molePaths = { Dir, values[0] };
                string molePath = Path.Combine(molePaths);

                MoleList.Add(new Mole(molePath, values[1]));

            }
        }
    }
   


}