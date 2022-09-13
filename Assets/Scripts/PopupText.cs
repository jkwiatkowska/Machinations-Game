using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupText : MonoBehaviour
{
    [SerializeField] Canvas Canvas;
    [SerializeField] Vector2 PositionOffsetPerSecond;
    [SerializeField] float FadePerSecond;
    [SerializeField] float ScaleChangePerSecond;
    [SerializeField] Vector2 RandomOffset;
    [SerializeField] float Duration;
    [SerializeField] PopupTextPool Pool;
    [SerializeField] TextMeshProUGUI Text;
    [SerializeField] Vector3 Scale;

    Vector3 WorldPosition;
    float Timer;
    Vector2 Offset;

    public void Setup(Vector3 position, string text)
    {
        WorldPosition = position;
        Offset = new Vector2(Random.Range(-RandomOffset.x, RandomOffset.x), Random.Range(-RandomOffset.y, RandomOffset.y));

        Text.text = text;
        transform.localScale = Scale;

        Timer = 0.0f;
        LateUpdate();
    }

    public void Setup(Vector3 position, string text, Color colour)
    {
        Text.color = colour;
        Setup(position, text);
    }

    //public void Setup(Vector3 worldPosition, Canvas canvas, string text)
    //{
    //    WorldPosition = worldPosition;
    //    Canvas = canvas;
    //    Offset = new Vector2(Random.Range(-RandomOffset.x, RandomOffset.x), Random.Range(-RandomOffset.y, RandomOffset.y));
    //
    //    Setup(text);
    //}
    //
    //public void Setup(string text)
    //{
    //    Text.text = text;
    //    transform.localScale = Scale;
    //
    //    Timer = 0.0f;
    //    Update();
    //}

    void LateUpdate()
    {
        Timer += Time.deltaTime;
        if (Timer > Duration)
        {
            Pool.ReturnToPool(this);
            return;
        }

        if (WorldPosition != null)
        {
            var offset = Offset + PositionOffsetPerSecond * Timer;

            if (Canvas != null && WorldPosition != null)
            {
                SetPosition(transform, Canvas, WorldPosition, offset.x, offset.y);
            }
            else
            {
                var position = transform.position;
                position.x += offset.x;
                position.y += offset.y;
                transform.position = position;
            }

            var fade = 1.0f - Mathf.Min(1.0f, FadePerSecond * Timer);

            var newColour = Text.color;
            newColour.a = fade;
            Text.color = newColour;

            transform.localScale *= 1.0f - Time.deltaTime * -ScaleChangePerSecond;
        }
    }

    public static void SetPosition(Transform transform, Canvas canvas, Vector3 worldPosition, float offsetX = 0.0f, float offsetY = 0.0f)
    {
        var screenSizeX = canvas.GetComponent<RectTransform>().rect.width;
        var screenSizeY = canvas.GetComponent<RectTransform>().rect.height;

        var screenPosition = Camera.main.WorldToViewportPoint(worldPosition);

        transform.localPosition = new Vector2(screenSizeX * (screenPosition.x - 0.5f + offsetX), screenSizeY * (screenPosition.y - 0.5f + offsetY));
    }

    public void SetColor(Color colour)
    {
        Text.color = colour;
    }
}
