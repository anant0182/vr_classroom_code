using UnityEngine;
using System.Linq;

public class WritingPen : MonoBehaviour
{
    [SerializeField] private Transform tip;
    [SerializeField] private int penSize = 5;
    private Renderer renderer;
    private Color[] colors;
    private float tipHeight;
    private RaycastHit hit;
    private Whiteboard whiteboard;
    private Vector2 touchPos;
    private bool touchedLastFrame;
    private Vector2 lastTouchPos;
    private Quaternion lastTouchRot;

    // Update is called once per frame
    void Start()
    {
        renderer = tip.GetComponent<Renderer>();
        colors = Enumerable.Repeat(renderer.material.color, penSize * penSize).ToArray();
        tipHeight = tip.localScale.y;
    }

    void Update()
    {
        Draw();
    }

    private void Draw()
    {
        if(Physics.Raycast(tip.position, transform.up, out hit, tipHeight))
        {
            if(hit.transform.CompareTag("Whiteboard"))
            {
                if(whiteboard == null)
                {
                    whiteboard = hit.transform.GetComponent<Whiteboard>();
                }
                touchPos = new Vector2(hit.textureCoord.x, hit.textureCoord.y);

                var x = (int)(touchPos.x * whiteboard.textureSize - (penSize / 2));
                var y = (int)(touchPos.y * whiteboard.textureSize - (penSize / 2));

                if (y < 0 || y >= whiteboard.textureSize || x < 0 || x >= whiteboard.textureSize) return;
                
                if(touchedLastFrame)
                {
                    whiteboard.texture.SetPixels(x, y, penSize, penSize, colors);
                    for(float f = 0.01f; f < 1.00f; f += 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(lastTouchPos.x, x, f);      
                        var lerpY = (int)Mathf.Lerp(lastTouchPos.y, y, f);
                        whiteboard.texture.SetPixels(lerpX, lerpY, penSize, penSize, colors);  
                    }

                    transform.rotation = lastTouchRot;
                    whiteboard.texture.Apply();
                }

                lastTouchPos = new Vector2(x, y);
                lastTouchRot = transform.rotation;
                touchedLastFrame = true;
                return;
            }
                

           
        }
        whiteboard = null;
        touchedLastFrame = false;
    }
}