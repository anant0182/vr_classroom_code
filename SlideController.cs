using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq; // Needed for sorting

[RequireComponent(typeof(PhotonView))]
public class SlideController : MonoBehaviourPunCallbacks
{
    [Header("Displays")]
    public RawImage laptopUI_Image;      // The RawImage on the Laptop Canvas (for UI)
    public Renderer laptopQuadRenderer;  // The MeshRenderer on the Laptop Screen Quad (Black part)
    public Renderer whiteboardRenderer;  // The MeshRenderer on the Big Whiteboard Quad

    [Header("Controls")]
    public Button uploadButton;
    public Button nextButton;
    public Button prevButton;

    private Texture2D[] slides;
    private int currentIndex = -1;
    private const string SLIDES_PATH = "Slides"; // Folder in Resources

    void Start()
    {
        // Assign listeners
        if (uploadButton) uploadButton.onClick.AddListener(UploadSlides);
        if (nextButton) nextButton.onClick.AddListener(NextSlide);
        if (prevButton) prevButton.onClick.AddListener(PrevSlide);
        
        // Initial State
        UpdateUI(false);
    }

    public void UploadSlides()
    {
        if (!PhotonNetwork.InRoom) LoadSlidesLocal(); // Offline
        else if (PhotonNetwork.IsMasterClient) photonView.RPC("RPC_LoadSlides", RpcTarget.AllBufferedViaServer); // Online
    }

    public void NextSlide()
    {
        if (!HasSlides()) return;
        int nextIndex = (currentIndex + 1) % slides.Length;

        if (!PhotonNetwork.InRoom) SetSlideLocal(nextIndex);
        else if (PhotonNetwork.IsMasterClient) photonView.RPC("RPC_SetSlide", RpcTarget.AllBufferedViaServer, nextIndex);
    }

    public void PrevSlide()
    {
        if (!HasSlides()) return;
        int prevIndex = (currentIndex - 1 + slides.Length) % slides.Length;

        if (!PhotonNetwork.InRoom) SetSlideLocal(prevIndex);
        else if (PhotonNetwork.IsMasterClient) photonView.RPC("RPC_SetSlide", RpcTarget.AllBufferedViaServer, prevIndex);
    }

    // --- Core Logic ---

    void LoadSlidesLocal()
    {
        // Load ONLY Texture2D assets, sorted by name
        slides = Resources.LoadAll<Texture2D>(SLIDES_PATH)
            .OrderBy(x => x.name)
            .ToArray();

        if (slides.Length > 0)
        {
            SetSlideLocal(0);
            UpdateUI(true);
        }
        else Debug.LogError($"No images found in Resources/{SLIDES_PATH}");
    }

    void SetSlideLocal(int index)
    {
        currentIndex = index;
        UpdateDisplay(slides[currentIndex]);
    }

    void UpdateDisplay(Texture2D slide)
    {
        // 1. Update Laptop UI (Canvas RawImage)
        if (laptopUI_Image != null) laptopUI_Image.texture = slide;

        // 2. Update Laptop Screen (Quad Mesh)
        if (laptopQuadRenderer != null) laptopQuadRenderer.material.mainTexture = slide;

        // 3. Update Big Whiteboard (Quad Mesh)
        if (whiteboardRenderer != null) whiteboardRenderer.material.mainTexture = slide;
    }

    // --- Networking RPCs ---

    [PunRPC]
    void RPC_LoadSlides()
    {
        LoadSlidesLocal(); 
    }

    [PunRPC]
    void RPC_SetSlide(int index)
    {
        SetSlideLocal(index);
    }

    // --- Helpers ---

    bool HasSlides()
    {
        return slides != null && slides.Length > 0;
    }

    void UpdateUI(bool slidesLoaded)
    {
        bool canControl = !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
        if (uploadButton) uploadButton.interactable = canControl; 
        if (nextButton) nextButton.interactable = canControl && slidesLoaded;
        if (prevButton) prevButton.interactable = canControl && slidesLoaded;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && currentIndex != -1)
            photonView.RPC("RPC_SetSlide", newPlayer, currentIndex);
    }
}