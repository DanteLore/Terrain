using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text worldPosText;

    public Text headingText;

    public TerrainGenerator terrainGenerator; 

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = terrainGenerator.GameToMapPos(player.transform.position);
        float heading =  player.transform.rotation.eulerAngles.y;

        worldPosText.text = string.Format("X: {0:0.0} Y: {1:0.0}", pos.x, pos.y);
        headingText.text = string.Format("{0:0}°", heading);
    }
}
