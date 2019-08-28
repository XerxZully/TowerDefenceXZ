using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject prefabLevel = null;

    public static GameManager Instanse { get; private set; } = null;

    public static GameObject LevelMngr = null;
    [HideInInspector] public string levelfile;

    private void Awake()
    {
        if ( prefabLevel == null ) Application.Quit();

        levelfile = PlayerPrefs.GetString( "LevelFileName", "Error" );
        //Debug.Log( levelfile );
        if ( levelfile == "Error" ) Application.Quit();

        Instanse = this;
        LevelMngr = Instantiate( prefabLevel );
        LevelMngr.name = "LevelMngr";
        LevelMngr.GetComponent<LevelManager>().LoadLevel( levelfile );   
    }
}
