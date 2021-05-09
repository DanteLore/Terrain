using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Linq;

public class GameController : MonoBehaviour
{
    public Canvas mainMenuCanvas;
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
        SetIsFullscreen(false);
        hudObject.SetActive(true);
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
            messageText.text = target.name;
        else if(playerController.CurrentChunk != null)
            messageText.text = "Chunk: " + playerController.CurrentChunk.coord;
        else
            messageText.text = "";
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            inMenu = !inMenu;
            SetMenuVisible(inMenu);
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
    }

    private void UpdateHud()
    {
        Vector2 pos = terrainGenerator.GameToMapPos(player.transform.position);
        float heading =  player.transform.rotation.eulerAngles.y;

        worldPosText.text = string.Format("X: {0:0} Z: {1:0}", pos.x, pos.y);
        headingText.text = string.Format("{0:0}°", heading);
        
        compassText.transform.localRotation = Quaternion.Euler(0, 0, -heading);
    }

    public void Quit()
    {
        Debug.Log("Bye bye!");
        Application.Quit();
    }

    public void Resume()
    {
        SetMenuVisible(false);
    }

    public void SetIsFullscreen(bool fullscreen)
    {
        Resolution res = fullscreen ? Screen.resolutions.Last() : Screen.resolutions.First(r => r.width == 1024);
    
        Screen.SetResolution(res.width, res.height, fullscreen);
    }
}
