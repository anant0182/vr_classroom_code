using UnityEngine;


public class Eraser : MonoBehaviour
{
    public Transform eraserSurface; // Assign the "EraserSurface" object here
    public int eraserSize = 10; // How big the eraser area is (pixels)

    private bool isErasing = false;
    private Whiteboard currentBoard;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Update()
    {
        // Only erase if the eraser is being held
        if (grabInteractable.isSelected)
        {
            // Check for contact with the whiteboard using a small overlap box
            // This is better than a single raycast for a large surface area
            Collider[] hitColliders = Physics.OverlapBox(eraserSurface.position, new Vector3(0.03f, 0.03f, 0.03f), eraserSurface.rotation);
            
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Whiteboard"))
                {
                    currentBoard = hitCollider.GetComponent<Whiteboard>();
                    if (currentBoard != null)
                    {
                        // We need the UV coordinate. A simple raycast from the eraser surface down can find this.
                        RaycastHit hit;
                        if (Physics.Raycast(eraserSurface.position, eraserSurface.forward, out hit, 0.1f))
                        {
                            // Send the erase command (drawing white)
                            // We pass Color.white to "erase"
                            currentBoard.DrawAt(hit.textureCoord, Color.white);
                        }
                    }
                }
            }
        }
    }
}