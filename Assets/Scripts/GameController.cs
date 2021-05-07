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

    RigidbodyFirstPersonController playerController;
    MouseLook mouseLook;

    public GameObject player;

    public GameObject menuObject;
    public GameObject hudObject;

    private bool inMenu = false;

    void Awake()
    {
        SetIsFullscreen(false);
        playerController = player.GetComponent<RigidbodyFirstPersonController>();
        mouseLook = playerController.mouseLook;
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
        playerController.enabled = !showMenu;
        menuObject.SetActive(showMenu);
        hudObject.SetActive(!showMenu);
    }

    private void UpdateHud()
    {
        Vector2 pos = terrainGenerator.GameToMapPos(player.transform.position);
        float heading =  player.transform.rotation.eulerAngles.y;

        worldPosText.text = string.Format("X: {0:0.0} Z: {1:0.0}", pos.x, pos.y);
        headingText.text = string.Format("{0:0}°", heading);
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
