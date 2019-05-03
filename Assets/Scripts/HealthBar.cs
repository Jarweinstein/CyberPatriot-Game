using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public TextMesh displayText;
    public string BarDisplay
    {
        get
        {
            return barDisplay;
        }
        set
        {
            barDisplay = "Health: " + value;
        }
    }
    string barDisplay;

    void Update()
    {
        displayText.text = barDisplay;
        transform.position = new Vector2(transform.parent.position.x, transform.parent.position.y + 0.5f);
        transform.rotation = Quaternion.identity;
    }
}
