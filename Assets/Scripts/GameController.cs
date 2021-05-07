using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

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

    void Start()
    {
        playerController = player.GetComponent<RigidbodyFirstPersonController>();
        mouseLook = playerController.mouseLook;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            inMenu = !inMenu;
            mouseLook.SetCursorLock(!inMenu);
            mouseLook.UpdateCursorLock();
            playerController.enabled = !inMenu;
            menuObject.SetActive(inMenu);
            hudObject.SetActive(!inMenu);
        }

        // Emergency use only :)
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            Debug.Break();
        }
            
        UpdateHud();
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
}
