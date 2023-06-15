// Sets the device of the WebCamTexture to the first one available and starts playing it
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEditor.Hardware;

public class cameraStream : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private bool isLive;
    private Color[] lastPicture;
    public bool IsLive
    {
        get { return isLive; }
        set { 
            isLive = value;
            if(webcamTexture != null)
            {
                if (value)
                    webcamTexture.Play();
                else
                {
                    lastPicture = webcamTexture.GetPixels();
                    webcamTexture.Pause();
                }
            }
        }
    }

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        webcamTexture = new WebCamTexture();

        if (devices.Length > 0)
        {
            try
            {
                webcamTexture.deviceName = "USB Microscope";
                webcamTexture.Play();
                GetComponent<RawImage>().texture = webcamTexture;
                isLive = true;
                lastPicture = webcamTexture.GetPixels();
            }
            catch (System.Exception)
            {

                throw;
            }
            
        }
    }

    public Texture2D GetImg()
    {
        Texture2D tex = new Texture2D(webcamTexture.width, webcamTexture.height);
        tex.SetPixels(lastPicture);
        tex.Apply(); 
        return tex;
    }
}
