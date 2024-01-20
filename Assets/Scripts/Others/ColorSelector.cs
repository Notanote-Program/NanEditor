using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    private InputField T_RGBA;
    private Image image;
    private RawImage mainImage;
    private RawImage hueImage;
    private RawImage alphaImage;
    private Texture2D texture;
    private Texture2D hueTexture;
    private Texture2D alphaTexture;
    private int TexPixelLength = 256;
    private int TexPixelHeight = 256;
    private Color _mainColor;
    private float alpha = 1;
    Color[,] arrayColor;
    public RectTransform circleRect;
    public RectTransform hueCursor;
    public RectTransform alphaCursor;
    private enum ActiveCursor
    {
        mainCursor,
        hueCursor,
        alphaCursor,
    }
    private ActiveCursor activeCursor;
    public Color color
    {
        get { return _mainColor; }
        private set 
        { 
            _mainColor = new Color(value.r, value.g, value.b, alpha); 
            image.color = _mainColor;
            T_RGBA.SetTextWithoutNotify("#" + _mainColor.ToHexString());
        }
    }
    public void Start()
    {
    }
    public void init()
    {
        arrayColor = new Color[TexPixelLength, TexPixelHeight];
        texture = new Texture2D(TexPixelLength, TexPixelHeight);
        hueTexture = new Texture2D(TexPixelLength, TexPixelHeight);
        alphaTexture = new Texture2D(TexPixelLength, TexPixelHeight);
        initHue();
        initAlpha();

        image = transform.Find("Image").gameObject.GetComponent<Image>();

        mainImage = transform.Find("RawImage").gameObject.GetComponent<RawImage>();
        mainImage.texture = texture;
        mainImage.texture.wrapMode = TextureWrapMode.Clamp;

        hueImage = transform.Find("hueImage").gameObject.GetComponent<RawImage>();
        hueImage.texture = hueTexture;
        hueImage.texture.wrapMode = TextureWrapMode.Clamp;

        alphaImage = transform.Find("alphaImage").gameObject.GetComponent<RawImage>();
        alphaImage.texture = alphaTexture;
        alphaImage.texture.wrapMode = TextureWrapMode.Clamp;

        T_RGBA = transform.Find("T_RGBA").gameObject.GetComponent<InputField>();

        setTexture(Color.red);
        color = getColor(getUniformPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>()), texture);
    }
    
    private void initAlpha()
    {
        for (int i = 0; i < TexPixelLength; i++)
        {
            for (int j = 0; j < TexPixelHeight; j++)
            {
                arrayColor[i, j] = new Color(1,1,1,(float)j / TexPixelHeight);
            }
        }
        List<UnityEngine.Color> listColor = new List<UnityEngine.Color>();
        for (int i = 0; i < TexPixelLength; i++)
        {
            for (int j = 0; j < TexPixelHeight; j++)
            {
                listColor.Add(arrayColor[j, i]);
            }
        }

        alphaTexture.SetPixels(listColor.ToArray());
        alphaTexture.Apply();
    }
    private void initHue()
    {
        for (int i = 0; i < TexPixelLength; i++)
        {
            for(int j = 0; j < TexPixelHeight; j++)
            {
                arrayColor[i, j] = Color.HSVToRGB((float)j / TexPixelHeight,1,1);
            }
        }
        List<UnityEngine.Color> listColor = new List<UnityEngine.Color>();
        for (int i = 0; i < TexPixelLength; i++)
        {
            for (int j = 0; j < TexPixelHeight; j++)
            {
                listColor.Add(arrayColor[j, i]);
            }
        }

        hueTexture.SetPixels(listColor.ToArray());
        hueTexture.Apply();
    }

    public void setColor(Color _color)
    {
        
        alpha = _color.a;
        float H, S, V;
        Color.RGBToHSV(_color, out H, out S, out V);
        Color _endcolor = Color.HSVToRGB(H, 1, 1);
        setTexture(_endcolor);
        setMainCursorPos(_color);
        setHueCursorPos(H);
        setAlphaCursorPos(alpha);
        color = getColor(getUniformPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>()), texture);
        
    }
    public void setColor(string T_color)
    {
        Color _color;
        UnityEngine.ColorUtility.TryParseHtmlString(T_color, out _color);
        Debug.Log(_color);
        alpha = _color.a;
        float H, S, V;
        Color.RGBToHSV(_color, out H, out S, out V);
        Color _endcolor = Color.HSVToRGB(H, 1, 1);
        setTexture(_endcolor);
        setMainCursorPos(_color);
        setHueCursorPos(H);
        setAlphaCursorPos(alpha);
        color = getColor(getUniformPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>()), texture);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(mainImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            activeCursor = ActiveCursor.mainCursor;
            moveMainCursor(eventData);
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(hueImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            activeCursor = ActiveCursor.hueCursor;
            moveHueCursor(eventData);
        }
        if (RectTransformUtility.RectangleContainsScreenPoint(alphaImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera))
        {
            activeCursor = ActiveCursor.alphaCursor;
            moveAlphaCursor(eventData);
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        switch(activeCursor)
        {
            case ActiveCursor.mainCursor:
                moveMainCursor(eventData);
                break;
            case ActiveCursor.hueCursor:
                moveHueCursor(eventData); 
                break;
            case ActiveCursor.alphaCursor:
                moveAlphaCursor(eventData); 
                break;
        }
    }
    private void moveMainCursor(PointerEventData eventData)
    {
        Vector3 wordPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(mainImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out wordPos))
            circleRect.position = wordPos;
        circleRect.anchoredPosition = GetClampPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>());
        color = getColor(getUniformPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>()), texture);
    }
    private void moveHueCursor(PointerEventData eventData)
    {
        Vector3 wordPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(hueImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out wordPos))
            hueCursor.position = new Vector3(hueCursor.position.x, wordPos.y, hueCursor.position.z);
        hueCursor.anchoredPosition = GetClampPositionY(hueCursor.anchoredPosition, hueImage.GetComponent<RectTransform>());
        float hue = getUniformPosition(hueCursor.anchoredPosition, hueImage.GetComponent<RectTransform>()).y;
        setTexture(Color.HSVToRGB(hue,1,1));
        color = getColor(getUniformPosition(circleRect.anchoredPosition, mainImage.GetComponent<RectTransform>()), texture);
    }
    private void moveAlphaCursor(PointerEventData eventData)
    {
        Vector3 wordPos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(alphaImage.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out wordPos))
            alphaCursor.position = new Vector3(alphaCursor.position.x, wordPos.y, alphaCursor.position.z);
        alphaCursor.anchoredPosition = GetClampPositionY(alphaCursor.anchoredPosition, alphaImage.GetComponent<RectTransform>());
        alpha = getUniformPosition(alphaCursor.anchoredPosition, alphaImage.GetComponent<RectTransform>()).y;
        color = new Color(color.r, color.g, color.b, alpha);
    }
    private Vector2 GetClampPosition(Vector2 touchPos, RectTransform rt)
    {
        Vector2 vector2 = new Vector2(touchPos.x, touchPos.y);
        vector2.x = Mathf.Clamp(vector2.x, 0, rt.sizeDelta.x);
        vector2.y = Mathf.Clamp(vector2.y, 0, rt.sizeDelta.y);
        return vector2;
    }
    private Vector2 GetClampPositionX(Vector2 touchPos, RectTransform rt)
    {
        Vector2 vector2 = new Vector2(touchPos.x, touchPos.y);
        vector2.x = Mathf.Clamp(vector2.x, 0, rt.sizeDelta.x);
        return vector2;
    }
    private Vector2 GetClampPositionY(Vector2 touchPos, RectTransform rt)
    {
        Vector2 vector2 = new Vector2(touchPos.x, touchPos.y);
        vector2.y = Mathf.Clamp(vector2.y, 0, rt.sizeDelta.y);
        return vector2;
    }
    private Vector2 getUniformPosition(Vector2 anchoredPos, RectTransform rt)
    {
        return new Vector2(anchoredPos.x / rt.sizeDelta.x, anchoredPos.y / rt.sizeDelta.y);
    }
    private Vector2 getAnchoredPosition(Vector2 uniformPos, RectTransform rt)
    {
        return new Vector2(uniformPos.x * rt.sizeDelta.x, uniformPos.y * rt.sizeDelta.y);
    }
    private Color getColor(Vector2 pos, Texture2D tex)
    {
        return tex.GetPixel((int)(pos.x * tex.width), (int)(pos.y * tex.height));
    }
    private void setMainCursorPos(Color _color)
    {
        Color[] colorArray = texture.GetPixels();
        Debug.Log(colorArray.Length);
        float x= 0, y = 1;
        for(int i = 0; i < colorArray.Length; i++) 
        {
            Vector3 distance = new Vector3(_color.r, _color.g, _color.b) - new Vector3(colorArray[i].r, colorArray[i].g, colorArray[i].b);
            if(distance.magnitude < 1e-3)
            {
                x = (float)(i % texture.width) / texture.width;
                y = (float)(i / texture.width) / texture.height;
                break;
            }
        }
        // float x = (_color.g - _color.r) / (_endcolor.g * _color.r - _color.r + _color.g - _color.g * _endcolor.r);
        // float y = _color.r / (1 - x + _endcolor.r * x);
        Debug.Log(new Vector2(x, y));
        Vector2 pos = getAnchoredPosition(new Vector2(x, y), mainImage.GetComponent<RectTransform>());
        circleRect.anchoredPosition = GetClampPosition(pos, mainImage.GetComponent<RectTransform>());
    }
    private void setHueCursorPos(float hue)
    {
        Vector2 pos = getAnchoredPosition(new Vector2(0f, hue), hueImage.GetComponent<RectTransform>());
        hueCursor.anchoredPosition = GetClampPositionY(pos, hueImage.GetComponent<RectTransform>());
    }
    private void setAlphaCursorPos(float alpha)
    {
        Vector2 pos = getAnchoredPosition(new Vector2(0f, alpha), alphaImage.GetComponent<RectTransform>());
        alphaCursor.anchoredPosition = GetClampPositionY(pos, alphaImage.GetComponent<RectTransform>());
    }
    private void setTexture(Color endcolor) 
    {
        Color value = (endcolor - Color.white) / (TexPixelLength - 1);
        for (int i = 0; i < TexPixelLength; i++)
        {
            arrayColor[i, TexPixelHeight - 1] = Color.white + value * i;
        }
        // 同理，垂直方向
        for (int i = 0; i < TexPixelLength; i++)
        {
            value = (arrayColor[i, TexPixelHeight - 1] - Color.black) / (TexPixelHeight - 1);
            for (int j = 0; j < TexPixelHeight; j++)
            {
                arrayColor[i, j] = Color.black + value * j;
            }
        }
        //返回一个数组，保存了所有颜色色值
        List<Color> listColor = new List<Color>();
        for (int i = 0; i < TexPixelHeight; i++)
        {
            for (int j = 0; j < TexPixelLength; j++)
            {
                listColor.Add(arrayColor[j, i]);
            }
        }
        texture.SetPixels(listColor.ToArray());
        texture.Apply();
    }
}
