using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System.IO;
using Unity.VisualScripting;

public class Saver
{
    // Start is called before the first frame update
    private string Path = @"Assets\PacientDatabase";
    public int Save(NewPacient_class sourceNew, List<Pacient> database)
    {
        using (StreamWriter writer = new StreamWriter(Path + @"\PacientList.csv", false, System.Text.Encoding.GetEncoding(1250)))
        {
            string data = "";
            for (int i = 0; i < database.Count; i++)
            {
                data += database[i].Dir + ";" 
                      + database[i].Name + "\n";
            }
            string newDirOrigo = sourceNew.Name.text.ToLowerInvariant();
            string stringFormD = newDirOrigo.Normalize(System.Text.NormalizationForm.FormD);
            if (stringFormD.Contains(' '))
                stringFormD = stringFormD.Replace(" ", "_");
            System.Text.StringBuilder retVal = new System.Text.StringBuilder();
            for (int index = 0; index < stringFormD.Length; index++)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stringFormD[index]) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    retVal.Append(stringFormD[index]);
            }
            string newDir = retVal.ToString().Normalize(System.Text.NormalizationForm.FormC);

            data += newDir + ";" + sourceNew.Name.text;

            writer.Write(data);

            string newFolder = Path + @"\" + newDir;

            if (!System.IO.Directory.Exists(newFolder))
                System.IO.Directory.CreateDirectory(newFolder);
        }
        return 0;
    }
    public int Save(NewMole_class sourceNew, Pacient PacDatabase)
    {
        string path = Path + @"\" + PacDatabase.Dir;
        using (StreamWriter writer = new StreamWriter(path + @"\MoleList.csv", false, System.Text.Encoding.GetEncoding(1250)))
        {
            string data = "";
            for (int i = 0; i < PacDatabase.MoleList.Count; i++)
            {
                data += PacDatabase.MoleList[i].Dir + ";"
                      + PacDatabase.MoleList[i].Name + "\n";
            }
            string newDir = sourceNew.Name.text.ToLowerInvariant();
            if (newDir.Contains(' '))
                newDir = newDir.Replace(" ", "_");

            data += newDir + ";" + sourceNew.Name.text;

            writer.Write(data);

            string newFolder = path + @"\" + newDir;

            if (!System.IO.Directory.Exists(newFolder))
                System.IO.Directory.CreateDirectory(newFolder);
        }
        return 0;
    }

    public int Save(NewRev_class sourceNew, Pacient PacDatabase, Mole MoleDatabase, GameObject sphere)
    {
        string path = Path + @"\" + MoleDatabase.Dir;

        using (StreamWriter writer = new StreamWriter(path + @"\RevList.csv", false, System.Text.Encoding.GetEncoding(1250)))
        {
            string data = "";
            for (int i = 0; i < MoleDatabase.RevisionList.Count; i++)
            {
                data += "rev_" + MoleDatabase.RevisionList[i].Index.ToString() + ";"
                      + MoleDatabase.RevisionList[i].Index.ToString() + ";"
                      + MoleDatabase.RevisionList[i].getXYZCoordinatesString() + "\n";
            }

            int index = MoleDatabase.RevisionList.Count + 1;
            string newDir = MoleDatabase.Dir + @"\rev_" + index.ToString();
            MoleDatabase.RevisionList.Add(new Revision(newDir, index.ToString(), sphere.transform.position, sphere.transform.rotation));
            if (newDir.Contains(' '))
                newDir = newDir.Replace(" ", "_");

            data += "rev_" + MoleDatabase.RevisionList[index - 1].Index.ToString() + ";"
                    + MoleDatabase.RevisionList[index - 1].Index.ToString() + ";" 
                    + MoleDatabase.RevisionList[index - 1].getXYZCoordinatesString();

            writer.Write(data);

            string newFolder = Path + @"\" + newDir;

            if (!System.IO.Directory.Exists(newFolder))
                System.IO.Directory.CreateDirectory(newFolder);
        }
        return 0;
    }
    public int Delete(int toDelete, Mole MoleDatabase)
    {
        MoleDatabase.RevisionList.RemoveAt(toDelete);
        string path = Path + @"\" + MoleDatabase.Dir;

        using (StreamWriter writer = new StreamWriter(path + @"\RevList.csv", false, System.Text.Encoding.GetEncoding(1250)))
        {
            string data = "";
            for (int i = 0; i < MoleDatabase.RevisionList.Count; i++)
            {
                data += "rev_" + MoleDatabase.RevisionList[i].Index.ToString() + ";"
                      + MoleDatabase.RevisionList[i].Index.ToString() + "\n";
            }

            writer.Write(data);
           
            string newDir = MoleDatabase.Dir + @"\rev_" + (toDelete+1).ToString();
            string newFolder = Path + @"\" + newDir;

            if (System.IO.Directory.Exists(newFolder))
                System.IO.Directory.Delete(newFolder, true);
        }
        return 0;
    }


}
