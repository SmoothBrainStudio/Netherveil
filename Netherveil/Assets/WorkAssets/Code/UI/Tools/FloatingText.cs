using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    Vector3 newPos;
    float newColor;
    public bool toggleTextReduction = false;

    void Start()
    {
        newColor = 1f;
    }

    void Update()
    {
        //grind
        newPos = transform.position;
        newPos.y += Time.deltaTime;
        transform.position = newPos;

        //fade + gris
        newColor -= Time.deltaTime;
        Color newColor2 = text.color;
        newColor2.a = newColor;
        text.color = newColor2;

        if(toggleTextReduction)
        {
            text.fontSize -= Time.deltaTime * 20f;
        }

        if (text.alpha <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetSize(int size)
    {
        text.fontSize = size;
    }

    public void SetColor(Color color)
    { 
        text.color = color;
    }


}
