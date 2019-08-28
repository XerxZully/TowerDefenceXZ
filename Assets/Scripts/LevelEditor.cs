using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

[ExecuteInEditMode]
public class LevelEditor : MonoBehaviour
{
    const int TILESCOUNT = 64;
    TileBase[] tilesList = null;
    private GameObject tilemapObject = null;
    private Tilemap tilemap = null;
    [HideInInspector] public Level level = null;
    [HideInInspector] public List<GameObject> EnemiesList = new List<GameObject>();
    [HideInInspector] public GameObject Base = null;
    [HideInInspector] public GameObject prefabEnemy;

    [HideInInspector] public string stringFileName = "Default.txt";

    public void Init()
    {
        tilemapObject = GameObject.Find( "Tilemap" );
        tilemap = tilemapObject.GetComponent<Tilemap>();
        level = new Level();

        Tile[] tilePrefabs = new Tile[TILESCOUNT];
        tilesList = new TileBase[TILESCOUNT];
        for( int i = 0; i < TILESCOUNT; i++ ) {
            tilePrefabs[i] = ( Tile )Resources.Load( "Tiles/Tiles01_" + i );
            tilesList[i] = Instantiate( tilePrefabs[i] ) as TileBase;
            tilesList[i].name = "Tiles01_" + i;
        }
        tilePrefabs = null;
        prefabEnemy = ( GameObject )Resources.Load( "Objects/EnemyPrefab" );

        //default values:
        level.data.levelName = "Default level";
        level.data.width = 8;
        level.data.height = 10;
        level.data.x = 4;
        level.data.y = 5;  
        level.data.spawnsCount = 0;
        level.data.towersCount = 4;
        level.data.towerMaxLevel = 4;
        level.data.startScore = 4;

        Base = GameObject.Find( "Base" );
        if( Base == null ) {
            GameObject BasePrefab = ( GameObject )Resources.Load( "Objects/PlayerBasePrefab", typeof( GameObject ) );
            Base = Instantiate( BasePrefab );
            Base.name = "Base";
            Base.transform.parent = tilemapObject.transform;
        }
        Base.transform.position = new Vector3( level.data.x + 0.5f, level.data.y + 0.5f, -0.01f );
    }

    public void CloseLevel()
    {
        Transform t = tilemapObject.transform;
        for( int i = t.childCount - 1; i >= 0; i-- ) {
            if ( t.GetChild( i ).name != "Base" )
                GameObject.DestroyImmediate( t.GetChild(i).gameObject );
            //Debug.Log( t.GetChild( i ).name );
        }

        level.data.tiles.Clear();
        level.data.path.Clear();
        level.data.spawns.Clear();
        level.data.spawnsCount = 0;

        EnemiesList.Clear();
        tilemap.ClearAllTiles();
        Base.transform.position = new Vector3( level.data.x + 0.5f, level.data.y + 0.5f, -0.01f );
    }

    public void CreateLevel()
    {
        CloseLevel();

        int w = level.data.width;
        int h = level.data.height;
        tilemap.size = new Vector3Int( w, h, 1 );

        Vector3 pos = transform.position;

        for( int i = 0; i < w * h; i++ ) {
            level.data.tiles.Add( Random.Range( 0, 3 ) );
        }

        int k = 0;
        for( int j = 0; j < h; j++ ) {
            for( int i = 0; i < w; i++ ) {
                pos.Set( i, h - j - 1, pos.z );
                tilemap.SetTile( tilemap.WorldToCell( pos ), tilesList[level.data.tiles[k]] );
                k++;
            }
        }
        tilemap.RefreshAllTiles();

        Base.transform.position = new Vector3( level.data.x + 0.5f, level.data.y + 0.5f, -0.01f );
    }

    public void LoadLevel( string fileName )
    {
        CloseLevel();

        level.LoadLevelData( fileName );

        int w = level.data.width;
        int h = level.data.height;
        tilemap.size = new Vector3Int( w, h, 1 );

        Vector3 pos = transform.position;

        int k = 0;
        for( int j = 0; j < h; j++ ) {
            for( int i = 0; i < w; i++ ) {
                pos.Set( i, h - j - 1, pos.z );
                tilemap.SetTile( tilemap.WorldToCell( pos ), tilesList[level.data.tiles[k]] );
                k++;
            }
        }
        tilemap.RefreshAllTiles();

        Base.transform.position = new Vector3( level.data.x + 0.5f, level.data.y + 0.5f, -0.01f );

        for( int i = 0; i < level.data.spawnsCount; i++ ) {
            GameObject NewObject = Instantiate( prefabEnemy ) as GameObject;
            NewObject.GetComponent<Enemy>().CreateEnemy(
                level.data.spawns[i].type, level.data.spawns[i].time,
                new Vector3( level.data.spawns[i].xEditor, level.data.spawns[i].yEditor, level.data.spawns[i].yGame * 0.01f - 1f ) );
            NewObject.name = "Enemy" + i;
            NewObject.transform.parent = tilemapObject.transform;
            EnemiesList.Add( NewObject );
        }
    }

    public void SaveLevel( string fileName )
    {
        level.data.spawns.Clear();

        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Enemy");
        foreach( GameObject obj in allObjects ) {
            obj.transform.parent = tilemapObject.transform;
        }

        Transform t = tilemapObject.transform;
        Enemy e = null;
        Level.spawnData newSpawn;
        for( int i = t.childCount - 1; i >= 0; i-- ) {
            e = t.GetChild( i ).GetComponent<Enemy>();
            if( e != null ) {
                //Debug.Log( t.GetChild( i ).name );
                newSpawn = new Level.spawnData();
                newSpawn.xEditor = t.GetChild( i ).transform.position.x;
                newSpawn.yEditor = t.GetChild( i ).transform.position.y;
                newSpawn.type = t.GetChild( i ).GetComponent<Enemy>().type;
                newSpawn.time = Vector3.Distance( Base.transform.position, t.GetChild( i ).transform.position );
                level.data.spawns.Add( newSpawn );
            }
        }
        level.data.spawnsCount = level.data.spawns.Count;

        int w = level.data.width;
        int h = level.data.height;
        level.data.tiles.Clear();

        TileBase tile = null;
        Vector3Int pos;
        int index = 0;
        for( int j = 0; j < h; j++ ) {
            for( int i = 0; i < w; i++ ) {
                pos = new Vector3Int( i, h-j-1, 0 );
                tile = tilemap.GetTile( pos );
                index = 0;
                if( tile != null ) {
                    index = int.Parse( tile.name.Remove( 0, 8 ) );
                }
                level.data.tiles.Add( index );
            }
        }

        level.SaveLevelData( Path.GetFileName( fileName ) );
    }

    void Update()
    {
        if( (Base != null) && (level != null) ) {
            if( Base.transform.hasChanged ) {
                Base.transform.position = new Vector3(
                    Mathf.Floor( Base.transform.position.x - 0.5f ) + 0.5f,
                    Mathf.Floor( Base.transform.position.y - 0.5f ) + 0.5f,
                    Base.transform.position.z );
                level.data.x = ( int )Mathf.Floor( Base.transform.position.x );
                level.data.y = ( int )Mathf.Floor( Base.transform.position.y );
                Base.transform.hasChanged = false;
            }
        }
    }
}
