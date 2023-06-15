using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;
//using UnityEditor.Rendering;
using Main.Assets.Scripts;
using Intel.RealSense;
using UnityEngine.XR;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;

public class main : MonoBehaviour
{
    
    public enum ScreenState
    {
        Pacient,
        Mole,
        Revision
    }
    public enum StateMode
    {
        Overview,
        New
    }
    
    //blueprinty
    public GameObject bp_ScrollObj;

    //gameobjets
    public GameObject go_ContentPanel;
    public GameObject go_ScrollView;
    public GameObject go_PacientTab;
    public GameObject go_MoleTab;
    public GameObject go_RevisionTab;
    public GameObject go_BtnBack;
    public GameObject go_BtnAdd;
    public GameObject go_PacientOverview;
    public GameObject go_MoleOverview;
    public GameObject go_RevOverview;
    public GameObject go_NewPacient;
    public GameObject go_NewMole;
    public GameObject go_NewRev;
    public GameObject go_PCRealtime;
    public GameObject go_PCShow;
    public GameObject go_PCTextureRealtime;
    public GameObject go_PCTextureShow;
    public GameObject sphere;
    public Material toleranceMaterial;


    //prop
    private List<Pacient> Pacients;
    private List<GameObject> CurrentScrollItemList = new List<GameObject>();
    private Saver Saver;

    private static int ActivePacient = 0;
    private static int ActiveMole = 0;
    private static int ActiveRev = 0;
    private static bool DeleteFlag = false;

    private static PacientOverview_class PacientOverview;
    private static MoleOverview_class MoleOverview;
    private static RevOverview_class RevOverview;

    private static NewPacient_class NewPacient;
    private static NewMole_class NewMole;
    private static NewRev_class NewRev;

    

    private ScreenState screen;

    public ScreenState Screen
    {
        get { return screen; }
        set
        {
            if(Mode == StateMode.New)
            {
                return; // pridat messagebox
            }
            screen = value;

            PacientOverview.View.SetActive(false);
            MoleOverview.View.SetActive(false);
            RevOverview.View.SetActive(false);

            go_PacientTab.SetActive(false);
            go_MoleTab.SetActive(false);
            go_RevisionTab.SetActive(false);

            NewPacient.View.SetActive(false);
            NewMole.View.SetActive(false);
            NewRev.View.SetActive(false);

            if (screen == ScreenState.Pacient)
            {
                go_ContentPanel.GetComponent<Image>().color = new Color32(84, 147, 139, 255);

                ScrollView_Fill(Pacients);
                PacientOverview.View.SetActive(true);
            }
            else if (screen == ScreenState.Mole)
            {
                go_PacientTab.SetActive(true);
                go_ContentPanel.GetComponent<Image>().color = new Color32(121, 182, 149, 255);

                if (ActivePacient > -1)
                {
                    ScrollView_Fill(Pacients[ActivePacient].MoleList);
                    MoleOverview.View.SetActive(true);
                }
            }
            else if (screen == ScreenState.Revision)
            {
                go_PacientTab.SetActive(true);
                go_MoleTab.SetActive(true);
                //go_RevisionTab.SetActive(true);
                go_ContentPanel.GetComponent<Image>().color = new Color32(211, 167, 122, 255);

                if (ActiveMole > -1)
                {
                    ScrollView_Fill(Pacients[ActivePacient].MoleList[ActiveMole].RevisionList);
                    RevOverview.View.SetActive(true);
                }
            }
        }
    }

    private StateMode mode;
    
    public StateMode Mode
    {
        get { return mode; }
        set 
        { 
            mode = value;

            PacientOverview.View.SetActive(false);
            MoleOverview.View.SetActive(false);
            RevOverview.View.SetActive(false);

            //go_PacientTab.SetActive(false);
            //go_MoleTab.SetActive(false);
            //go_RevisionTab.SetActive(false);

            NewPacient.View.SetActive(false);
            NewMole.View.SetActive(false);
            NewRev.View.SetActive(false);

            if (mode == StateMode.New)
            {
                if (Screen == ScreenState.Pacient)
                {
                    NewPacient.View.SetActive(true);
                }
                else if (Screen == ScreenState.Mole)
                {
                    NewMole.View.SetActive(true);
                }
                else if (Screen == ScreenState.Revision)
                {
                    NewRev.View.SetActive(true);
                    go_PCRealtime.SetActive(true);
                    go_PCShow.SetActive(false);
                    NewRev.Img.GetComponent<cameraStream>().IsLive = true;
                    if(NewRev.Sphere != null)
                        Destroy(NewRev.Sphere, 0.5f);
                    NewRev.SetInfo("Take scan first");
                    NewRev.Name.text = "Rev " + (Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count + 1).ToString();
                    if (Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count > 0)
                        NewRev.setPrevRevCoordinates(Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count - 1]);

                }
            }
            else if (mode == StateMode.Overview)
                Screen = Screen;


        }
    }



    private socketTest SerialPort;
    // Start is called before the first frame update
    void Start()
    {
        Pacients = new List<Pacient>();
        Saver = new Saver();
        LoadDatabase();
        ScrollView_Fill(Pacients);

        PacientOverview = new PacientOverview_class(go_PacientOverview);
        MoleOverview = new MoleOverview_class(go_MoleOverview);
        RevOverview = new RevOverview_class(go_RevOverview);

        NewPacient = new NewPacient_class(go_NewPacient);
        NewMole = new NewMole_class(go_NewMole);
        NewRev = new NewRev_class(go_NewRev);

        FillBtnsActions();

        Screen = ScreenState.Pacient;
        PacientClick_scrollView(ActivePacient);

        SerialPort = GameObject.Find("dermatoskop").GetComponent<socketTest>();

        //loadTolerances();

        //go_PCRealtime.SetActive(false);
        //go_PCShow.SetActive(false);
        //go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
    }
    
    public void loadTolerances()
    {
       
        float x_init = -1285.116f;
        float y_init = -499.836f;
        float z_init = 0.63109f;
        float inkrement = 0.05f;
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                NewRev.tolSphere.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                NewRev.tolSphere[NewRev.tolSphere.Count-1].transform.position = new Vector3(x_init - (i * inkrement), y_init - (j * inkrement), z_init);
                NewRev.tolSphere[NewRev.tolSphere.Count - 1].GetComponent<Renderer>().material = toleranceMaterial;
                //Color color = new Color(0, 0, 0, 0.1f);
                //NewRev.tolSphere[NewRev.tolSphere.Count - 1].GetComponent<Renderer>().material.SetColor("_Color", color);
                NewRev.tolSphere[NewRev.tolSphere.Count - 1].transform.localScale = Vector3.one / 45.5f;
                NewRev.tolSphere[NewRev.tolSphere.Count - 1].transform.parent = NewRev.View.transform;
            }
        }
    }

    private void LoadDatabase()
    {
        
        Pacients = new List<Pacient>();
        using (var reader = new StreamReader(@"Assets\PacientDatabase\PacientList.csv", System.Text.Encoding.GetEncoding(1250)))
        {
            List<string> listA = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');

                Pacients.Add(new Pacient(values[0], values[1]));
            }

        }
    }

    private void ScrollView_Clear()
    {
        if (CurrentScrollItemList.Count > 0)
        {
            for (var i = CurrentScrollItemList.Count - 1; i >= 0; i--)
            {
                CurrentScrollItemList[i].transform.GetComponent<Button>().onClick.RemoveAllListeners();
                UnityEngine.Object.Destroy(CurrentScrollItemList[i]);
            }
            CurrentScrollItemList.Clear();
        }
    }

    private void ScrollView_Fill(List<Pacient> source)
    {
        ScrollView_Clear();
        for (int i = 0; i < source.Count; i++)
        {
            int index = i;
            CurrentScrollItemList.Add((GameObject)Instantiate(bp_ScrollObj, go_ScrollView.transform));
            CurrentScrollItemList[index].transform.GetChild(0).gameObject.GetComponent<Text>().text = source[i].Name;
            CurrentScrollItemList[index].transform.GetComponent<Button>().onClick.AddListener(delegate { PacientClick_scrollView(index); });
        }
    }
    private void PacientClick_scrollView(int i)
    {
        for (int j = 0; j < Pacients.Count; j++)
        {
            CurrentScrollItemList[j].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 20);
        }
        CurrentScrollItemList[i].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 90);

        if (Pacients.Count > i)
        {
            ActivePacient = i;
            PacientOverview.Name.text = go_PacientTab.transform.GetChild(0).gameObject.GetComponent<Text>().text = Pacients[ActivePacient].Name;
        }
    }
    private void ScrollView_Fill(List<Mole> source)
    {
        ScrollView_Clear();
        for (int i = 0; i < source.Count; i++)
        {
            int index = i;
            CurrentScrollItemList.Add((GameObject)Instantiate(bp_ScrollObj, go_ScrollView.transform));
            CurrentScrollItemList[index].transform.GetChild(0).gameObject.GetComponent<Text>().text = source[i].Name;
            CurrentScrollItemList[index].transform.GetComponent<Button>().onClick.AddListener(delegate { MoleClick_scrollView(index); });
        }
    }
    private async void MoleClick_scrollView(int i)
    {
        for (int j = 0; j < Pacients[ActivePacient].MoleList.Count; j++)
        {
            CurrentScrollItemList[j].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 20);
        }
        CurrentScrollItemList[i].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 90);

        if (Pacients[ActivePacient].MoleList.Count > i)
        {
            // FREE MEMORY
            if (ActiveMole > 0)
            {
                for (int j = 0; j < Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count; j++)
                {
                    Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Pointcloud = null;
                    Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Texture = null;
                    Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].TextureUV = null;

                }
            }
            // LOAD NEW SCANS
            ActiveMole = i;
            MoleOverview.Name.text = go_MoleTab.transform.GetChild(0).gameObject.GetComponent<Text>().text = Pacients[ActivePacient].MoleList[ActiveMole].Name;
            for (int j = Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count - 1; j >= 0 ; j--)
            {
                Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Pointcloud = await go_PCShow.GetComponent<RsPointCloudRenderer>().LoadMesh(@"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Dir + @"\pc.txt");
                Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Texture = go_PCShow.GetComponent<RsPointCloudRenderer>().LoadTexture(@"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Dir + @"\pc_texture.png");
                Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].TextureUV = go_PCShow.GetComponent<RsPointCloudRenderer>().LoadTexture(@"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Dir + @"\pc_uv.png");
                byte[] bytes = File.ReadAllBytes(@"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].Dir + @"\img.png");
                Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].RevImg = new Texture2D(100, 100); ;
                Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[j].RevImg.LoadImage(bytes);
            }
        }
    }
    private void ScrollView_Fill(List<Revision> source)
    {
        ScrollView_Clear();
        for (int i = 0; i < source.Count; i++)
        {
            int index = i;
            CurrentScrollItemList.Add((GameObject)Instantiate(bp_ScrollObj, go_ScrollView.transform));
            CurrentScrollItemList[index].transform.GetChild(0).gameObject.GetComponent<Text>().text = "Rev " + source[i].Index.ToString();
            CurrentScrollItemList[index].transform.GetComponent<Button>().onClick.AddListener(delegate { RevClick_scrollView(index); });
        }
    }
    private void RevClick_scrollView(int i)
    {
        for (int j = 0; j < Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count; j++)
        {
            CurrentScrollItemList[j].gameObject.GetComponent<Image>().color = new Color32(255,255, 255, 20);
        }
        CurrentScrollItemList[i].gameObject.GetComponent<Image>().color = new Color32(255, 255, 255, 90);

        //CurrentScrollItemList[i].gameObject
        if (Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count > i)
        {
            ActiveRev = i;
            go_PCShow.SetActive(true);
            go_PCRealtime.SetActive(false);

            go_PCShow.GetComponent<RsPointCloudRenderer>().SetFreeze(true);
            go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
            //go_PCShow.GetComponent<RsPointCloudRenderer>().ResetMesh(1280, 720);
            go_PCShow.GetComponent<RsPointCloudRenderer>().SetPointcloud(
                                                        Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].Pointcloud,
                                                        Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].Texture,
                                                        Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].TextureUV
                                                        );


            //Texture newTexture =  go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().LoadTexture(@"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].Dir + @"\pc.png");
            //go_PCShow.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", newTexture);
            //go_PCShow.GetComponent<MeshRenderer>().material.SetTexture("_UVMap", newTexture);


            RevOverview.Name.text = go_RevisionTab.transform.GetChild(0).gameObject.GetComponent<Text>().text = "Rev no." + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[ActiveRev].Index.ToString();

            RevOverview.Img.texture = Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].RevImg;
            
            RevOverview.Sphere.transform.position = Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[i].getXYZCoordinates();
            
        }
    }
    int sphereCount = 0;
    public void DermatoscopeClick()
    {
        ////Vector3 matchingPoint = go_PCShow.GetComponent<RsPointCloudRenderer>().SearchForPoint(sphere);
        //Vector3 matchingPoint = sphere.transform.position;
        //NewRev.Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //NewRev.Sphere.transform.parent = NewRev.View.transform;
        //NewRev.Sphere.transform.position = matchingPoint;
        //NewRev.Sphere.transform.localScale = Vector3.one / 150;
        ////NewRev.Sphere.GetComponent<Renderer>().material.color = new Color(36, 169, 232);
        //if (sphereCount == 0)
        //{
        //    sphereCount = 1;
        //    NewRev.Sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        //}
        //else if (sphereCount == 1)
        //{
        //    sphereCount = 2;
        //    NewRev.Sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        //}
        //else if (sphereCount == 2)
        //{
        //    sphereCount = 0;
        //    NewRev.Sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
        //}


        //if (NewRev.Img.GetComponent<cameraStream>().IsLive)
        //{
        //    NewRev.Img.GetComponent<cameraStream>().IsLive = false;
        //    Vector3 matchingPoint = go_PCShow.GetComponent<RsPointCloudRenderer>().SearchForPoint(sphere);

        //    NewRev.Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    NewRev.Sphere.transform.parent = NewRev.View.transform;
        //    NewRev.Sphere.transform.position = matchingPoint;
        //    NewRev.Sphere.transform.localScale = Vector3.one / 80;
        //    NewRev.Sphere.GetComponent<Renderer>().material.color = new Color(36, 169, 232);
        //    NewRev.SetInfo("Reset with dermatoscope by clicking again");
        //}
        //else
        //{
        //    NewRev.Img.GetComponent<cameraStream>().IsLive = true;
        //    if (NewRev.Sphere != null)
        //        Destroy(NewRev.Sphere, 0.5f);

        //    NewRev.SetInfo("Find mole and take picture with dermatoscope");
        //}
        if (NewRev.Img.GetComponent<cameraStream>().IsLive)
        {
            NewRev.Img.GetComponent<cameraStream>().IsLive = false;

            NewRev.Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            NewRev.Sphere.transform.parent = NewRev.View.transform;
            NewRev.Sphere.transform.position = sphere.transform.position;
            NewRev.Sphere.transform.localScale = Vector3.one / 100;
            NewRev.Sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            NewRev.SetInfo("Reset with dermatoscope by clicking again");
        }
        else
        {
            NewRev.Img.GetComponent<cameraStream>().IsLive = true;
            if (NewRev.Sphere != null)
                Destroy(NewRev.Sphere, 1f);

            NewRev.SetInfo("Find mole and take picture with dermatoscope");
        }

    }

    private void FillBtnsActions()
    {
        go_BtnBack.GetComponent<Button>().onClick.AddListener(delegate { BtnBack(); });
        go_BtnAdd.GetComponent<Button>().onClick.AddListener(delegate { BtnAdd(); });

        // ========== OVERVIEW OBJECT CLASS ==========
        PacientOverview.BtnDetail.onClick.AddListener(delegate { BtnDetail(); });
        PacientOverview.BtnSettings.onClick.AddListener(delegate { BtnSettings(); });
        PacientOverview.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });

        MoleOverview.BtnDetail.onClick.AddListener(delegate { BtnDetail(); });
        MoleOverview.BtnSettings.onClick.AddListener(delegate { BtnSettings(); });
        MoleOverview.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });

        RevOverview.BtnDetail.onClick.AddListener(delegate { BtnDetail(); });
        RevOverview.BtnSettings.onClick.AddListener(delegate { BtnSettings(); });
        RevOverview.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });

        // ========== NEW OBJECT CLASS ==========
        NewPacient.BtnSave.onClick.AddListener(delegate { BtnSave(); });
        NewPacient.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });

        NewMole.BtnSave.onClick.AddListener(delegate { BtnSave(); });
        NewMole.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });

        NewRev.BtnSave.onClick.AddListener(delegate { BtnSave(); });
        NewRev.BtnDelete.onClick.AddListener(delegate { BtnDelete(); });
        NewRev.BtnTakePC.onClick.AddListener(delegate { BtnTakePC(); });



    }
    private void BtnBack()
    {
        if (Screen > ScreenState.Pacient)
            Screen = (ScreenState)((int)Screen - 1);
    }
    private void BtnAdd()
    {
        Mode = StateMode.New;
    }
    private void BtnDetail()
    {
        if (Screen != ScreenState.Revision)
            Screen = (ScreenState)((int)Screen + 1);

        if (Screen == ScreenState.Pacient)
            PacientClick_scrollView(0); 
        else if (Screen == ScreenState.Mole)
            MoleClick_scrollView(0); 
        else if (Screen == ScreenState.Revision)
        {
            if (DeleteFlag && Mode == StateMode.Overview)
            {
                DeleteFlag = false;
                Saver.Delete(ActiveRev, Pacients[ActivePacient].MoleList[ActiveMole]);
                ScrollView_Fill(Pacients[ActivePacient].MoleList[ActiveMole].RevisionList);
                RevOverview.ChangeLeaveToSave();
                RevOverview.SetInfo("");
            }
            else
            {
                RevClick_scrollView(Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count-1);
            }
        }
    }
    private void BtnSettings()
    {
        Screen = ScreenState.Mole;
    }
    private void BtnSave()
    {

        if (Screen == ScreenState.Pacient)
        {
            Saver.Save(NewPacient, Pacients);
            LoadDatabase();
            ScrollView_Fill(Pacients);
            Mode = StateMode.Overview;
            Screen = ScreenState.Pacient;
        }
        else if (Screen == ScreenState.Mole)
        {
            Saver.Save(NewMole, Pacients[ActivePacient]);
            LoadDatabase();
            ScrollView_Fill(Pacients[ActivePacient].MoleList);
            Mode = StateMode.Overview;
            Screen = ScreenState.Mole;
        }
        else if (Screen == ScreenState.Revision)
        {
            //if delete is enable
            if (DeleteFlag && Mode == StateMode.New)
            {
                DeleteFlag = !DeleteFlag;
                NewRev.ChangeLeaveToSave();
                NewRev.SetInfo("");
                Mode = StateMode.Overview;
                Screen = ScreenState.Revision;
            }
            else
            {
                if (NewRev.Sphere == null)
                {
                    NewRev.SetInfo("Mole wasn't captured");
                    return;
                }
                //====================================
                using (StreamWriter writer = new StreamWriter(Pacients[ActivePacient].MoleList[ActiveMole].Name + "_" + DateTime.Now.ToString("HH_mm_ss") + ".csv"))
                {
                    string data = NewRev.Sphere.transform.position.ToString();
                    data += ";";
                    data += NewRev.navigator.getPrev().ToString();
                    writer.Write(data);
                }
                //====================================
                Saver.Save(NewRev, Pacients[ActivePacient], Pacients[ActivePacient].MoleList[ActiveMole], NewRev.Sphere);



                LoadDatabase();
                int revCount = Pacients[ActivePacient].MoleList[ActiveMole].RevisionList.Count;
                string path = @"Assets\PacientDatabase\" + Pacients[ActivePacient].MoleList[ActiveMole].RevisionList[revCount - 1].Dir;
                go_PCShow.GetComponent<RsPointCloudRenderer>().SavePointcloud(path);
                go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
                go_PCShow.GetComponent<RsPointCloudRenderer>().SetFreeze(true);

                NewRev.saveImg(path);

                ScrollView_Fill(Pacients[ActivePacient].MoleList[ActiveMole].RevisionList);
                Mode = StateMode.Overview;
                Screen = ScreenState.Revision;
            }


                
        }

    }
    private void BtnDelete()
    {
        DeleteFlag = !DeleteFlag;
        if (DeleteFlag)
        {
            if (Mode == StateMode.Overview)
            {
                if (Screen == ScreenState.Revision)
                {
                    RevOverview.SetInfo("Delete revision?");
                    RevOverview.ChangeSaveToLeave();
                }
            }
            else if (Mode == StateMode.New)
            {
                if (Screen == ScreenState.Revision)
                {
                    NewRev.SetInfo("Leave without saving?");
                    NewRev.ChangeSaveToLeave();
                }
            }
        }
        else
        {
            if (Mode == StateMode.Overview)
            {
                if (Screen == ScreenState.Revision)
                {
                    RevOverview.SetInfo("");
                    RevOverview.ChangeLeaveToSave();
                }
            }
            else if (Mode == StateMode.New)
            {
                if (Screen == ScreenState.Revision)
                {
                    NewRev.SetInfo("");
                    NewRev.ChangeLeaveToSave();
                }
            }
        }

    }

    private void BtnTakePC()
    {
        // RESET
        if (NewRev.PCTaken == true)
        {
            NewRev.PCTaken = false;

            go_PCShow.SetActive(false);
            go_PCRealtime.SetActive(true);
            NewRev.BtnTakePC.GetComponentInChildren<Text>().text = "TAKE SCAN";
            go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(false);
            go_PCTextureRealtime.GetComponent<RsStreamTextureRenderer>().FreezeTexture(false);
            go_PCShow.GetComponent<RsPointCloudRenderer>().SetFreeze(false);
            go_PCRealtime.GetComponent<RsPointCloudRenderer>().SetFreeze(false);


        }

        // TAKE
        else if (NewRev.PCTaken == false)
        {
            NewRev.PCTaken = true;

            go_PCShow.SetActive(true);
            go_PCRealtime.SetActive(false);

            go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
            go_PCTextureRealtime.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
            go_PCShow.GetComponent<RsPointCloudRenderer>().SetFreeze(true);
            go_PCRealtime.GetComponent<RsPointCloudRenderer>().SetFreeze(true);
            //===========
            Vector3[] vertices = null;
            Points points = go_PCRealtime.GetComponent<RsPointCloudRenderer>().getPoints();
            using (var p = points.GetProfile<VideoStreamProfile>())
            {
                go_PCShow.GetComponent<RsPointCloudRenderer>().ResetMesh(p.Width, p.Height);
                vertices = new Vector3[p.Width * p.Height];
            }

            go_PCShow.GetComponent<RsPointCloudRenderer>().uvmap.LoadRawTextureData(points.TextureData, points.Count * sizeof(float) * 2);
            go_PCShow.GetComponent<RsPointCloudRenderer>().uvmap.Apply();

            //vertices = new Vector3[921600];
            points.CopyVertices(vertices);
            go_PCShow.GetComponent<RsPointCloudRenderer>().mesh.vertices = vertices;
            go_PCShow.GetComponent<RsPointCloudRenderer>().mesh.UploadMeshData(false);

            Texture newTexture = go_PCRealtime.GetComponent<MeshRenderer>().material.GetTexture("_MainTex");
            Texture newUV = go_PCRealtime.GetComponent<MeshRenderer>().material.GetTexture("_UVMap");

            go_PCShow.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", newTexture);
            go_PCShow.GetComponent<MeshRenderer>().material.SetTexture("_UVMap", newUV);
            NewRev.BtnTakePC.GetComponentInChildren<Text>().text = "RESET";

            go_PCTextureShow.GetComponent<RsStreamTextureRenderer>().FreezeTexture(true);
            go_PCShow.GetComponent<RsPointCloudRenderer>().SetFreeze(true);

            NewRev.SetInfo("Find mole and take picture with dermatoscope");


        }
    }
}

// ========================= OVERVIEW OBJECT CLASSES =========================
public class PacientOverview_class
{
    public Text Name;
    public Button BtnDetail;
    public Button BtnSettings;
    public Button BtnDelete;
    public GameObject View;

    public PacientOverview_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>();
        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        BtnSettings = source.transform.GetChild(2).transform.GetChild(1).gameObject.GetComponent<Button>();
        BtnDetail = source.transform.GetChild(2).transform.GetChild(3).gameObject.GetComponent<Button>();
    }
}
public class MoleOverview_class
{
    public Text Name;
    public Button BtnDetail;
    public Button BtnSettings;
    public Button BtnDelete;
    public GameObject View;

    public MoleOverview_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>();
        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        BtnSettings = source.transform.GetChild(2).transform.GetChild(1).gameObject.GetComponent<Button>();
        BtnDetail = source.transform.GetChild(2).transform.GetChild(3).gameObject.GetComponent<Button>();
    }
}
public class RevOverview_class
{
    public Text Name;
    public Text Info;
    public RawImage Img;
    public Button BtnDetail;
    public Button BtnSettings;
    public Button BtnDelete;
    public GameObject View;
    public GameObject Sphere;

    public RevOverview_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<Text>();
        Img = source.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<RawImage>();
        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        BtnSettings = source.transform.GetChild(2).transform.GetChild(1).gameObject.GetComponent<Button>();
        Info = source.transform.GetChild(2).transform.GetChild(2).gameObject.GetComponent<Text>();
        BtnDetail = source.transform.GetChild(2).transform.GetChild(3).gameObject.GetComponent<Button>();
        BtnDetail.gameObject.SetActive(false);

        Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Sphere.transform.parent = View.transform;
        Sphere.transform.localScale = Vector3.one / 40;
    }
    public void SetInfo(string text)
    {
        Info.text = text;
    }
    public void ChangeSaveToLeave()
    {
        BtnDetail.gameObject.SetActive(true);
        BtnDetail.transform.GetChild(0).gameObject.SetActive(false);
        BtnDetail.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Delete";
        BtnDetail.GetComponent<Image>().color = new Color32(204, 124, 85, 255);
    }
    public void ChangeLeaveToSave()
    {
        BtnDetail.gameObject.SetActive(false);
    }
}

// ========================= NEW OBJECT CLASSES =========================
public class NewPacient_class
{
    public InputField Name;
    public Button BtnSave;
    public Button BtnSettings;
    public Button BtnDelete;
    public GameObject View;

    public NewPacient_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<InputField>();
        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        BtnSave = source.transform.GetChild(2).transform.GetChild(2).gameObject.GetComponent<Button>();
    }
}
public class NewMole_class
{
    public InputField Name;
    public Button BtnSave;
    public Button BtnSettings;
    public Button BtnDelete;
    public GameObject View;

    public NewMole_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<InputField>();
        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        BtnSave = source.transform.GetChild(2).transform.GetChild(2).gameObject.GetComponent<Button>();
    }
}
public class NewRev_class
{
    public InputField Name;
    public Text Info;
    public Button BtnSave;
    public Button BtnDelete;
    public Button BtnTakePC;
    public Image PCViewer;
    public GameObject View;
    public bool PCTaken;
    public RawImage Img;
    public RawImage ImgPrevRev;

    public GameObject Sphere;
    public GameObject SpherePrevRev;
    public NavScript navigator;
    public List<GameObject> tolSphere;

    public NewRev_class(GameObject source)
    {
        View = source;
        Name = source.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<InputField>();

        ImgPrevRev = source.transform.GetChild(1).transform.GetChild(1).gameObject.GetComponent<RawImage>();
        Img = source.transform.GetChild(1).transform.GetChild(2).gameObject.GetComponent<RawImage>();
        PCViewer = source.transform.GetChild(1).transform.GetChild(0).gameObject.GetComponent<Image>();
        BtnTakePC = PCViewer.transform.GetChild(1).gameObject.GetComponent<Button> ();
        navigator = source.transform.GetChild(1).transform.GetChild(3).gameObject.GetComponent<NavScript>();

        BtnDelete = source.transform.GetChild(2).transform.GetChild(0).gameObject.GetComponent<Button>();
        Info = source.transform.GetChild(2).transform.GetChild(1).gameObject.GetComponent<Text>();
        BtnSave = source.transform.GetChild(2).transform.GetChild(2).gameObject.GetComponent<Button>();
        PCTaken = false;

        SpherePrevRev = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SpherePrevRev.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
        SpherePrevRev.transform.parent = View.transform;
        SpherePrevRev.transform.localScale = Vector3.one / 40;

        tolSphere = new List<GameObject>();

    }
    public void setPrevRevCoordinates(Revision prevRevision)
    {
        ImgPrevRev.texture = prevRevision.RevImg;
        SpherePrevRev.transform.position = prevRevision.getXYZCoordinates();
        navigator.setPrev(prevRevision.getXYZCoordinates());
    }
    public void saveImg(string path)
    {
        File.WriteAllBytes(path + @"\img.png", Img.GetComponent<cameraStream>().GetImg().EncodeToPNG());
    }
    public void SetInfo(string text)
    {
        Info.text = text;
    }
    public void ChangeSaveToLeave()
    {
        BtnSave.transform.GetChild(0).gameObject.SetActive(false);
        BtnSave.transform.GetChild(1).gameObject.GetComponent<Text>().text = "LEAVE";
        BtnSave.GetComponent<Image>().color = new Color32(204, 124, 85, 255);
    }
    public void ChangeLeaveToSave()
    {
        BtnSave.transform.GetChild(0).gameObject.SetActive(true);
        BtnSave.transform.GetChild(1).gameObject.GetComponent<Text>().text = "SAVE";
        BtnSave.GetComponent<Image>().color = new Color32(134, 182, 217, 255);
    }
}