using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    bool ingame;

    public static GameManager Instance;

    public GameModes gamemode;

    public GameObject baseController;
    public GameObject baseUIManager;
    public List<GameObject> waypoints;

    public List<List<GameObject>> teams;

    int charaterDisplayed;

    public List<PjBase> pjList = new List<PjBase>();

    public LayerMask wallLayer;
    public LayerMask airLayer;
    public LayerMask unitLayer;
    public LayerMask playerWallLayer;

    public GameObject damageText;
    public GameObject iaTeam;

    public Color32 iceColor;
    public Color32 fireColor;
    public Color32 waterColor;
    public Color32 desertColor;
    public Color32 natureColor;
    public Color32 windColor;
    public Color32 lightningColor;
    public Color32 crystalColor;

    public float ingameSpeed;

    public GameObject coin;
    public GameObject rewardSpawner;
   

    public enum GameModes
    {
        singleplayer, multiplayer
    }
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Time.timeScale = ingameSpeed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Vector2 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        Vector3 angle = new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        return new Vector2(-angle.x,angle.z);
    }

}
