using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instanse { get; private set; } = null;

    [SerializeField] private GameObject prefabPlayerBase = null;
    [SerializeField] private GameObject prefabEnemy = null;
    [SerializeField] private GameObject prefabTower = null;
    [HideInInspector] private Text textLevel = null;
    [HideInInspector] private Text textHealth = null;
    [HideInInspector] private Text textTowers = null;
    [HideInInspector] private Text textScore = null;
    [HideInInspector] private Text textMoney = null;
    [HideInInspector] private Text textEnemies = null;
    [SerializeField] private GameObject SpriteScreen = null;
    [SerializeField] private GameObject SpriteScreenMask = null;

    [SerializeField] public AudioClip SoundAwake = null;
    [SerializeField] public AudioClip SoundAttack = null;
    [SerializeField] public AudioClip SoundDeath = null;
    [SerializeField] public AudioClip SoundHit = null;
    [SerializeField] public AudioClip SoundWin = null;
    [SerializeField] public AudioClip SoundLose = null;
    static private AudioSource audioSource = null;

    [HideInInspector] public GameObject Base = null;
    [HideInInspector] public int score;
    [HideInInspector] public int money;
    [HideInInspector] public int kills;
    [HideInInspector] public int towers;
    [HideInInspector] public int towersCounter;
    [HideInInspector] public bool endGame = false;

    public Level level = null;
    static private float updateCycle = 0.5f;
    static private float update = 0.0f;
    static private int updateSpawnIndex = 0;
    static private float updateSpawn = 0.0f;
    static private float updateSpawnNext = 0.0f;
    static private float updateSpawnRate = 2.5f;
    private int buildTower = -1;

    public static Tilemap tilemap = null;
    const int TILESCOUNT = 64;
    TileBase[] tilesList = null;

    [HideInInspector] public List<GameObject> EnemiesList = new List<GameObject>();
    [HideInInspector] public List<GameObject> TowersList = new List<GameObject>();
    [HideInInspector] public List<Vector2Int> SpawnList = new List<Vector2Int>();
    [HideInInspector] public GameObject panelEnding = null;

    private void Awake()
    {
        Instanse = this;

        audioSource = gameObject.GetComponent<AudioSource>();

        tilemap = transform.Find( "Tilemap" ).GetComponent<Tilemap>();
        textLevel = GameObject.Find( "TextLevel" ).GetComponent<Text>();
        textHealth = GameObject.Find( "TextHealth" ).GetComponent<Text>();
        textTowers = GameObject.Find( "TextTowers" ).GetComponent<Text>();
        textScore = GameObject.Find( "TextScore" ).GetComponent<Text>();
        textMoney = GameObject.Find( "TextMoney" ).GetComponent<Text>();
        textEnemies = GameObject.Find( "TextEnemies" ).GetComponent<Text>();

        panelEnding = GameObject.Find( "PanelEnding" );
        GameObject endButton = GameObject.Find( "ButtonEnd" );
        endButton.GetComponent<Button>().onClick.AddListener( delegate { OnClickEnd(); } );
        panelEnding.SetActive( false );

        GameObject buttonSold = GameObject.Find( "ButtonSold" );
        buttonSold.GetComponent<Button>().onClick.AddListener( delegate { OnClickPlaceTower( -2 ); } );

        GameObject TextPrice = null;
        Tower towerScript = prefabTower.GetComponent<Tower>();
        for( int i = 0; i < towerScript.Towers.Length; i++ ) {
            TextPrice = GameObject.Find( "TextCost" + i );
            if( TextPrice != null ) {
                TextPrice.GetComponent<Text>().text = "$" + towerScript.Towers[i].Cost;
                int param = i;
                TextPrice.transform.parent.GetComponent<Button>().onClick.AddListener( delegate { OnClickPlaceTower( param ); } );
            }
        }

        LoadTilesList();
        level = new Level();
    }

    private void LoadTilesList() 
    {
        Tile[] tilePrefabs = new Tile[TILESCOUNT];
        tilesList = new TileBase[TILESCOUNT];
        for( int i = 0; i < TILESCOUNT; i++ ) {
            tilePrefabs[i] = ( Tile )Resources.Load( "Tiles/Tiles01_" + i );
            tilesList[i] = Instantiate( tilePrefabs[i] ) as TileBase;
        }
        tilePrefabs = null;
    }

    public void CloseLevel()
    {
        GameObject camera = GameObject.Find( "Main Camera" );
        camera.transform.localPosition = new Vector3( 0f, 0f, -10f );
        foreach( Transform child in transform ) {
            if( (child.gameObject.name != "Tilemap") && (child.gameObject.name != "SpriteScreen") )
                GameObject.Destroy( child.gameObject );
        }
        Base = null;
        EnemiesList.Clear();
        TowersList.Clear();
        tilemap.ClearAllTiles();
        score = 0;
        money = 0;
        kills = 0;
        towers = 0;
        towersCounter = 0;
        buildTower = -1;
        endGame = false;
    }

    public void LoadLevel( string fileName )
    {
        CloseLevel();
        
        level.LoadLevelData( fileName );

        int w = level.data.width;
        int h = level.data.height;
        tilemap.size = new Vector3Int( w, h, 1 );

        //TileBase[] allTiles = tilemap.GetTilesBlock( tilemap.cellBounds );
        Vector3 pos = transform.position;

        int k = 0;
        for( int j = 0; j < h; j++ ) {
            for( int i = 0; i < w; i++ ) {
                pos.Set( i, h-j-1, pos.z );
                tilemap.SetTile( tilemap.WorldToCell( pos ), tilesList[level.data.tiles[k]] );
                k++;
            }
        }
        tilemap.RefreshAllTiles();

        GameObject camera = GameObject.Find( "Main Camera" );
        camera.transform.localPosition = new Vector3( w / 2f, h / 2f, -10f );

        SpriteScreen.transform.position = new Vector3( w / 2f, h / 2f, -3f );
        SpriteScreenMask.GetComponent<RectTransform>().localScale = new Vector3( w, h, 1f );

        Base = Instantiate( prefabPlayerBase );
        Base.name = "Base";
        Base.transform.parent = gameObject.transform;
        Base.transform.localPosition = new Vector3( level.data.x + 0.5f, level.data.y + 0.5f, -0.01f );

        for ( int i = 0; i < level.data.spawnsCount; i++ ) {
            GameObject NewObject = Instantiate( prefabEnemy );
            NewObject.GetComponent<Enemy>().CreateEnemy(
                level.data.spawns[i].type, level.data.spawns[i].time,
                new Vector3( level.data.spawns[i].xGame, level.data.spawns[i].yGame, level.data.spawns[i].yGame*0.01f - 1f ) );
            NewObject.name = "Enemy" + i;
            NewObject.transform.parent = gameObject.transform;
            NewObject.SetActive( false );
            EnemiesList.Add( NewObject );
        }
        updateSpawnIndex = level.data.spawnsCount - 1;
        updateSpawn = 0.0f;
        updateSpawnNext = 0.0f;// level.data.spawns[updateSpawnIndex].time;// * updateSpawnRate;

        GameObject BtnTower = null;
        Tower towerScript = prefabTower.GetComponent<Tower>();
        for( int i = 0; i < towerScript.Towers.Length; i++ ) {
            BtnTower = GameObject.Find( "ButtonBuild" + i );
            if( BtnTower != null ) {
                BtnTower.GetComponent<Button>().interactable = i <= level.data.towerMaxLevel;
            }
        }

        money = level.data.startScore;
        textLevel.text = "Level: " + level.data.levelName;
        textHealth.text = "Health: " + Base.GetComponent<PlayerBase>().Health;
        textTowers.text = "Towers: 0 / " + level.data.towersCount;
        textMoney.text = "Money: $" + money;
        textScore.text = "Score: " + score;
        textEnemies.text = "Enemies: " + EnemiesList.Count + " / " + level.data.spawnsCount;
    }

    public void SaveLevel( string fileName )
    {
        level.SaveLevelData( fileName );
    }

    //for testing
    public void SpawnEnemy( int type, int time, float x, float y )
    {
        GameObject NewObject = Instantiate( prefabEnemy );
        NewObject.GetComponent<Enemy>().CreateEnemy( type, time, new Vector3( x, y, -2f ) );
        EnemiesList.Add( NewObject );
        
    }

    void Update()
    {
        if ( updateSpawnIndex >= 0 ) {
            updateSpawn += Time.deltaTime;
            Debug.Log( updateSpawn + " " + updateSpawnNext );
            if ( updateSpawn >= updateSpawnNext ) {
                updateSpawnNext = level.data.spawns[updateSpawnIndex].time * updateSpawnRate;
                for( int i = updateSpawnIndex; i >= 0; i-- ) {
                    if( (level.data.spawns[i].time * updateSpawnRate - 0.01) <= updateSpawnNext ) {
                        if ( EnemiesList[i] != null ) EnemiesList[i].SetActive( true );
                        audioSource.clip = SoundAwake;
                        audioSource.Play();
                        if( i <= 0 ) {
                            updateSpawnIndex = -1;
                            break;
                        }
                    } else {
                        updateSpawnIndex = i;
                        updateSpawnNext = level.data.spawns[i].time * updateSpawnRate;
                        break;
                    }
                }
            }
        }

        update += Time.deltaTime;
        if( update > updateCycle ) {
            update = 0.0f;
            textHealth.text = "Health: " + ( int )Base.GetComponent<PlayerBase>().Health;
            textTowers.text = "Towers: " + towers + " / " + level.data.towersCount;
            textMoney.text = "Money: $" + money;
            textScore.text = "Score: " + score;
            textEnemies.text = "Enemies: " + EnemiesList.Count + " / " + level.data.spawnsCount;

            if( !endGame ) {
                if( Base.GetComponent<PlayerBase>().Health <= 0 ) {
                    // LOSE !
                    endGame = true;
                    panelEnding.SetActive( true );
                    audioSource.PlayOneShot( SoundLose );
                    GameObject.Find( "TextEnding" ).GetComponent<Text>().text = "YOU LOSE!";
                    GameObject.Find( "TextEndingShadow" ).GetComponent<Text>().text = "YOU LOSE!";
                    GameObject.Find( "TextEnding" ).GetComponent<Text>().color = new Color( 255f / 255f, 54f / 255f, 0f, 1f );
                } else if( EnemiesList.Count <= 0 ) {
                    // WIN !
                    endGame = true;
                    panelEnding.SetActive( true );
                    audioSource.PlayOneShot( SoundWin );
                    GameObject.Find( "TextEnding" ).GetComponent<Text>().text = "YOU WIN!";
                    GameObject.Find( "TextEndingShadow" ).GetComponent<Text>().text = "YOU WIN!";
                    GameObject.Find( "TextEnding" ).GetComponent<Text>().color = new Color( 116f / 255f, 255f / 255f, 0f, 1f );
                }
            }
        }
        if( Input.GetKeyDown( KeyCode.Escape ) ) {
            if( buildTower >= 0 ) {
                buildTower = -1;
            } else {
                Base.GetComponent<PlayerBase>().ChangeHealth( -1000 );
            }
        }
        if( Input.GetMouseButtonDown( 0 ) ) {
            if( buildTower > -1 ) {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                PlaceTower( buildTower, mouseWorldPos );
            }
            if( buildTower == -2 ) {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
                RaycastHit2D hit = Physics2D.Raycast( mouseWorldPos, Vector2.zero );
                if( hit ) RemoveTower( hit.transform.gameObject );
                buildTower = -1;
            }
        } 
    }

    public void OnClickPlaceTower( int setType )
    {
        buildTower = setType;
    }

    public bool PlaceTower( int setType, Vector3 mouseWorldPos )
    {
        if( money < prefabTower.GetComponent<Tower>().Towers[setType].Cost )
            return false;

        Vector3Int tileCoords = tilemap.WorldToCell( mouseWorldPos );
        if( (tileCoords.x < 0) || (tileCoords.y < 0) || (tileCoords.x >= level.data.width) || (tileCoords.y >= level.data.height) )
            return false;

        int index = tileCoords.x + (level.data.height - tileCoords.y - 1) * level.data.width;
        if( level.data.tiles[index] < 4 ) //build only on rock
            return false;

        RaycastHit2D hit = Physics2D.Raycast( mouseWorldPos, Vector2.zero );
        if( hit ) {
            if( hit.transform.GetComponent<Tower>().type == setType )
                return false;

            RemoveTower( hit.transform.gameObject );
        }

        if( towers >= level.data.towersCount )
            return false;

        GameObject NewTower = Instantiate( prefabTower );
        towers++;
        towersCounter++;
        NewTower.name = "Tower" + towersCounter;
        NewTower.GetComponent<Tower>().CreateTower( setType, new Vector3( ( float )tileCoords.x + 0.5f, ( float )tileCoords.y + 0.5f, ( float )tileCoords.y * 0.01f - 2f ) );
        NewTower.transform.parent = gameObject.transform;
        TowersList.Add( NewTower );
        money -= prefabTower.GetComponent<Tower>().Towers[setType].Cost;
        buildTower = -1;

        return true;
    }

    public void RemoveTower( GameObject delTower )
    {
        int getType = delTower.GetComponent<Tower>().type;
        //money += prefabTower.GetComponent<Tower>().Towers[getType].Cost;
        TowersList.Remove( delTower );
        Destroy( delTower );
        towers--;
    }

    public void OnClickEnd()
    {
        PlayerPrefs.SetInt( "OpenMenu", 1 );
        SceneManager.LoadScene( "MenuScene" );
    }
}
