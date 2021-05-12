using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Linq;

public class GameController : MonoBehaviour
{
    public Canvas mainMenuCanvas;
    public Toggle fullScreenToggle;
    public Text worldPosText;
    public Text headingText;
    public TerrainGenerator terrainGenerator; 

    private RigidbodyFirstPersonController firstPersonController;
    private PlayerController playerController;
    MouseLook mouseLook;

    public GameObject player;

    public GameObject menuObject;
    public GameObject hudObject;

    public Text compassText;

    public Text crosshairText;
    public Text messageText;

    private bool inMenu = false;

    private const string activeCrosshairChar = "҉";
    private const string normalCrosshairChar = "҈";

    private const string compassArrowChar = "Λ"; // "†"; // "Ѧ";

    void Start()
    {
        firstPersonController = player.GetComponent<RigidbodyFirstPersonController>();
        playerController = player.GetComponent<PlayerController>();
        playerController.TargetChanged += OnPlayerTargetChanged;
        mouseLook = firstPersonController.mouseLook;

        if(compassText)  
           compassText.text = compassArrowChar;

        if(crosshairText)
            crosshairText.text = normalCrosshairChar;

        if(messageText)
            messageText.text = "";

        SetMenuVisible(false);
    }

    private void OnPlayerTargetChanged(PlayerController controller, Collider target)
    {
        if(target == null)
        {
            crosshairText.color = Color.white;
            crosshairText.text = normalCrosshairChar;
        }
        else
        {
            crosshairText.color = Color.red;
            crosshairText.text = activeCrosshairChar;
        }
        if(target != null)
        {
            messageText.text = target.name;
        }
        else if(playerController.CurrentChunk != null)
        {
            Biome biome = playerController.CurrentChunk.NearestBiome(player.transform.position);
            messageText.text = (biome != null) ? "Biome: " + biome : "";
        }
        else
        {
            messageText.text = "";
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SetMenuVisible(!inMenu);
        }

        // Emergency use only :)
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            Debug.Break();
        }

        UpdateHud();
    }

    private void SetMenuVisible(bool showMenu)
    {
        inMenu = showMenu;
        mouseLook.SetCursorLock(!showMenu);
        mouseLook.UpdateCursorLock();
        firstPersonController.enabled = !showMenu;
        menuObject.SetActive(showMenu);
        hudObject.SetActive(!showMenu);

        if(showMenu)
        {
            fullScreenToggle.isOn = Screen.fullScreen;
        }
    }

    private void UpdateHud()
    {
        Vector2 pos = terrainGenerator.GameToMapPos(player.transform.position);
        float heading =  player.transform.rotation.eulerAngles.y;

        worldPosText.text = string.Format("X: {0:0} Z: {1:0}", pos.x, pos.y);
        headingText.text = string.Format("{0:0}°", heading);
        
        compassText.transform.localRotation = Quaternion.Euler(0, 0, -heading);
    }

    public void QuitButtonClick()
    {
        Debug.Log("Bye bye!");
        Application.Quit();
    }

    public void ResumeButtonClick()
    {
        SetMenuVisible(false);
    }

    public void SetIsFullscreen(bool fullscreen)
    {
        Resolution res = fullscreen ? Screen.resolutions.Last() : Screen.resolutions.First(r => r.width == 1024);
    
        Screen.SetResolution(res.width, res.height, fullscreen);
    }
}
