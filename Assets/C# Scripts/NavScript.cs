using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class NavScript : MonoBehaviour
{
    private Slider slider_X;
    private Slider slider_Y;
    private Image slider_Z;

    private GameObject handle_X;
    private GameObject handle_Y;
    private Image handle_picture_X;
    private Image handle_picture_Y;
    private Sprite arrow;
    private Sprite knob;


    private float prev_x;
    private float prev_y;
    private float prev_z;

    void Awake()
    {
        slider_X = transform.GetChild(0).gameObject.GetComponent<Slider>();
        slider_Y = transform.GetChild(1).gameObject.GetComponent<Slider>();
        slider_Z = transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.GetComponent<Image>();


        slider_X.onValueChanged.AddListener(delegate { UpdateSliderX(); });
        handle_X = slider_X.transform.GetChild(2).transform.GetChild(0).gameObject;
        handle_picture_X = handle_X.GetComponent<Image>();



        slider_Y.onValueChanged.AddListener(delegate { UpdateSliderY(); });
        handle_Y = slider_Y.transform.GetChild(2).transform.GetChild(0).gameObject;
        handle_picture_Y = handle_Y.GetComponent<Image>();


        arrow = Resources.Load<Sprite>("play");
        knob = Resources.Load<Sprite>("button");

        prev_x = (float)0;
        prev_y = (float)0;
        prev_z = (float)0;

    }


    public void UpdateSliderX()
    {
        if (slider_X.value > 0.005f)
        {
            handle_X.transform.eulerAngles = new Vector3(0, 180, 0);
            handle_picture_X.sprite = arrow;
            //slider_X.fillRect.anchorMin = new Vector2(0.5f, 0);
            //slider_X.fillRect.anchorMax = new Vector2(slider_X.handleRect.anchorMin.x, 1);
        }
        else if (slider_X.value < -0.005f)
        {
            handle_X.transform.eulerAngles = new Vector3(0, 0, 0);
            handle_picture_X.sprite = arrow;
            //slider_X.fillRect.anchorMin = new Vector2(0.5f, 0);
            //slider_X.fillRect.anchorMax = new Vector2(slider_X.handleRect.anchorMin.x, 1);
        }
        else
        {
            handle_X.transform.eulerAngles = new Vector3(0, 0, 0);
            handle_picture_X.sprite = knob;
            //slider_X.fillRect.anchorMin = new Vector2(slider_X.handleRect.anchorMin.x, 0);
            //slider_X.fillRect.anchorMax = new Vector2(0.5f, 1);
        }
    }
    public void UpdateSliderY()
    {
        if (slider_Y.value > 0.005f)
        {
            handle_Y.transform.eulerAngles = new Vector3(0, 0, -90);
            handle_picture_Y.sprite = arrow;
            //slider_Y.fillRect.anchorMin = new Vector2(0, 0.5f);
            //slider_Y.fillRect.anchorMax = new Vector2(1, slider_Y.handleRect.anchorMin.x);
        }
        else if (slider_Y.value < -0.005f)
        {
            handle_Y.transform.eulerAngles = new Vector3(0, 0, 90);
            handle_picture_Y.sprite = arrow;
            //slider_Y.fillRect.anchorMin = new Vector2(0.5f, 0);
            //slider_Y.fillRect.anchorMax = new Vector2(slider_Y.handleRect.anchorMin.x, 1);
        }
        else
        {
            handle_Y.transform.eulerAngles = new Vector3(0, 0, 0);
            handle_picture_Y.sprite = knob;
            //slider_Y.fillRect.anchorMin = new Vector2(0, slider_Y.handleRect.anchorMin.x);
            //slider_Y.fillRect.anchorMax = new Vector2(1, 0.5f);
        }
    }

    public void setPrev(Vector3 input)
    {
        prev_x = (float)input.x;
        prev_y = (float)input.y;
        prev_z = (float)input.z;

        slider_X.maxValue = (float)(0.05f);
        slider_X.minValue = (float)(-0.05f);

        slider_Y.maxValue = (float)(0.05f);
        slider_Y.minValue = (float)(-0.05f);

    }

    public Vector3 getPrev()
    {
        return new Vector3(prev_x, prev_y, prev_z); 
    }

    public void SetValues(float actual_x, float actual_y, float actual_z)
    {
        
        Debug.Log("coords arived");
        float z = (prev_z - actual_z) * 2;
        Vector3 zScale = Vector3.one;
        if (z < 0)
            zScale += new Vector3(z,z,z);
        else if(z > 0)
            zScale -= new Vector3(z, z, z);



        float x = (float)(prev_x - actual_x);
        float y = (float)(prev_y - actual_y);
        slider_X.value = (float)(x);
        slider_Y.value = (float)(y);
        slider_Z.transform.localScale = zScale;

    }


}