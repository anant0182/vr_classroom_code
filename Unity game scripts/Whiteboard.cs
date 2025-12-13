using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Collider))]
public class Whiteboard : MonoBehaviour
{
    public int textureSize = 1024;
    public Color backgroundColor = Color.white;
    
    public Texture2D texture;
    private Renderer r;
    private PhotonView photonView;
    
    void Start()
    {
        r = GetComponent<Renderer>();
        photonView = GetComponent<PhotonView>();

        // Create a new texture in memory
        texture = new Texture2D(textureSize, textureSize);
        
        // Fill the texture with the background color
        Color[] colors = new Color[textureSize * textureSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = backgroundColor;
        }
        texture.SetPixels(colors);
        texture.Apply();

        // Assign this new texture to the object's material
        r.material.mainTexture = texture;
    }

    // Called by the Pen script
    public void DrawAt(Vector2 uv, Color color)
    {
        // If connected to a Room (Online or Offline Mode), use RPC to sync drawing
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("DrawPixelRPC", RpcTarget.AllBuffered, uv, new Vector3(color.r, color.g, color.b));
        }
        else
        {
            // If just testing in editor without connecting, draw locally
            DrawPixel(uv, color);
        }
    }

    [PunRPC]
    public void DrawPixelRPC(Vector2 uv, Vector3 colorVec)
    {
        Color color = new Color(colorVec.x, colorVec.y, colorVec.z);
        DrawPixel(uv, color);
    }

    private void DrawPixel(Vector2 uv, Color color)
    {
        int x = (int)(uv.x * textureSize);
        int y = (int)(uv.y * textureSize);

        // Draw a 5x5 block of pixels for visibility
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                int px = Mathf.Clamp(x + i, 0, textureSize - 1);
                int py = Mathf.Clamp(y + j, 0, textureSize - 1);
                texture.SetPixel(px, py, color);
            }
        }
        texture.Apply();
    }
}