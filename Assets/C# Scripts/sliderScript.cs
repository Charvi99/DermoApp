using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class sliderScript : MonoBehaviour
{
    private Slider _slider;
    private GameObject handle;
    private Image handle_picture;
    private Slider.Direction direction;
    private Sprite arrow;
    private Sprite knob;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        direction = _slider.direction;
        _slider.maxValue = 100;
        _slider.minValue = -100;
        _slider.onValueChanged.AddListener(delegate { UpdateSliderSense(); });
        UpdateSliderSense();
        _slider.value = 0;

        handle = transform.GetChild(2).transform.GetChild(0).gameObject;
        handle_picture = handle.GetComponent<Image>();
        arrow = Resources.Load<Sprite>("play");
        knob = Resources.Load<Sprite>("button");
    }

    void Update()
    {
        //UpdateSliderSense();
    }

    public void UpdateSliderSense()
    {
        if(direction == Slider.Direction.LeftToRight)
        {
            if (_slider.value > 10)
            {
                handle.transform.eulerAngles = new Vector3(0, 180, 0);
                handle_picture.sprite = arrow;
                _slider.fillRect.anchorMin = new Vector2(0.5f, 0);
                _slider.fillRect.anchorMax = new Vector2(_slider.handleRect.anchorMin.x, 1);
            }
            else if (_slider.value < -10)
            {
                handle.transform.eulerAngles = new Vector3(0, 0, 0);
                handle_picture.sprite = arrow;
                _slider.fillRect.anchorMin = new Vector2(0.5f, 0);
                _slider.fillRect.anchorMax = new Vector2(_slider.handleRect.anchorMin.x, 1);
            }
            else
            {
                handle.transform.eulerAngles = new Vector3(0, 0, 0);
                handle_picture.sprite = knob;
                _slider.fillRect.anchorMin = new Vector2(_slider.handleRect.anchorMin.x, 0);
                _slider.fillRect.anchorMax = new Vector2(0.5f, 1);
            }
        }
        else if(direction == Slider.Direction.BottomToTop)
        {
           
            if (_slider.value > 10)
            {
                handle.transform.eulerAngles = new Vector3(0, 0, -90);
                handle_picture.sprite = arrow;
                _slider.fillRect.anchorMin = new Vector2(0, 0.5f);
                _slider.fillRect.anchorMax = new Vector2(1, _slider.handleRect.anchorMin.x);
            }
            else if (_slider.value < -10)
            {
                handle.transform.eulerAngles = new Vector3(0, 0, 90);
                handle_picture.sprite = arrow;
                _slider.fillRect.anchorMin = new Vector2(0.5f, 0);
                _slider.fillRect.anchorMax = new Vector2(_slider.handleRect.anchorMin.x, 1);
            }
            else
            {
                 handle.transform.eulerAngles = new Vector3(0, 0, 0);
                handle_picture.sprite = knob;
                _slider.fillRect.anchorMin = new Vector2(0, _slider.handleRect.anchorMin.x);
                _slider.fillRect.anchorMax = new Vector2(1, 0.5f);
            }
        }
    }
}