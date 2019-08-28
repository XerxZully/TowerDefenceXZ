using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;

public class Level {
    public struct spawnData {
        public float xEditor;
        public float yEditor;
        public float xGame;
        public float yGame;
        public int type;
        public float time;
    }
    public class levelData {
        public int width;
        public int height;
        public int x;
        public int y;
        public int spawnsCount;
        public int towersCount;
        public int towerMaxLevel;
        public int startScore;
        public List<int> tiles;
        public List<int> path;
        public List<spawnData> spawns;
        public string levelName;

        public levelData() {
            tiles = new List<int>();
            path = new List<int>();
            spawns = new List<spawnData>();
        }
    }

    public levelData data = new levelData();

    public void ClearLevel( int width, int height )
    {
        data.tiles.Clear();
        data.path.Clear();
        data.width = width;
        data.height = height;
        data.x = width / 2;
        data.y = height / 2;
        data.levelName = "temp";
        for( int i = 0; i < data.width * data.height; i++ ) {
            data.tiles.Add( 0 );
        }
    }

    public void SaveLevelData( string fileName )
    {
        if( data.x < 0 ) data.x = 0;
        if( data.x >= data.width ) data.x = data.width - 1;
        if( data.y < 0 ) data.y = 0;
        if( data.y >= data.height ) data.y = data.height - 1;
        PathFind();
        MoveSpawnsToMapEdge();
        SortSpawnsByTime();

        string path = Application.dataPath + "/Resources/Levels/" + fileName;

        using( StreamWriter streamWriter = File.CreateText( path ) ) {
            string jsonString = JsonUtility.ToJson( data );
            streamWriter.WriteLine( jsonString );
            string spawnString;
            for (int i =0; i< data.spawnsCount; i++ ) {
                spawnString = JsonUtility.ToJson( data.spawns[i] );
                streamWriter.WriteLine( spawnString );
            }
        }
        Debug.Log( "Save level: " + path );
    }

    public void LoadLevelData( string fileName )
    {
        data.tiles.Clear();
        data.path.Clear();
        data.spawns.Clear();
        string path = Application.dataPath + "/Resources/Levels/" + fileName;

        using( StreamReader streamReader = File.OpenText( path ) ) {
            string jsonString = streamReader.ReadLine();
            data = JsonUtility.FromJson<levelData>( jsonString );
            string spawnString;
            spawnData newSpawn;
            for( int i = 0; i < data.spawnsCount; i++ ) {
                spawnString = streamReader.ReadLine();
                newSpawn = new spawnData();
                newSpawn = JsonUtility.FromJson<spawnData>( spawnString );
                data.spawns.Add( newSpawn );
            }
        }
        Debug.Log( "Load level: '" + data.levelName + "' [" + data.width + "," + data.height + "]" );

        //data.spawns.Clear(); GenerateSpawns( 30 );
        //SaveLevelData( fileName );
    }

    public void MoveSpawnsToMapEdge()
    {
        float w = data.width;
        float h = data.height;
        float x, y, xmod, ymod;
        float isOutOfMapBounds;
        float isXPositive;
        float isXLessY;
        spawnData spawn;

        for( int i = 0; i < data.spawnsCount; i++ ) {
            spawn = data.spawns[i];
            x = spawn.xEditor;
            y = spawn.yEditor;
            
            isOutOfMapBounds = ( ((x >= 0f) && (x <= w)) || ((y >= 0f) && (y <= h)) ) ? 0f : 1f;
            isXPositive = x >= 0f ? 1f : 0f;
            xmod = x - w * ( x >= 0f ? Mathf.Floor( x / w ) : Mathf.Ceil( x / w ) );
            ymod = y - h * ( y >= 0f ? Mathf.Floor( y / h ) : Mathf.Ceil( y / h ) );
            isXLessY = isXPositive * ( xmod < ymod ? 1f: 0f) + (1f - isXPositive) * (xmod >= ymod ? 1f : 0f);

            if ( isOutOfMapBounds == 1f) {
                if( isXLessY == 1f ) {
                    x = x < 0f ? 0.5f : w - 1.5f;
                } else {
                    y = y < 0f ? 0.5f : h - 1.5f;
                }
            }

            if( x < 0 ) x = -0.5f;
            if( y < 0 ) y = -0.5f;
            if( x > w ) x = w + 0.5f;
            if( y > h ) y = h + 0.5f;

            spawn.xGame = Mathf.Floor( x ) + 0.5f;
            spawn.yGame = Mathf.Floor( y ) + 0.5f;
            data.spawns[i] = spawn;
        }
    }

    public void PathFind()
    {
        int x = data.x;
        int y = data.y;
        int w = data.width;
        int h = data.height;

        data.path.Clear();
        for( int i = 0; i < w * h; i++ ) {
            data.path.Add( data.tiles[i] < 3 ? 0 : int.MaxValue );
        }

        data.path[x + (h - y - 1) * w] = 1;

        int k;
        int addCell;
        bool done = false;
        while ( !done ) {
            done = true;
            k = 0;
            for( int j = 0; j < h; j++ ) {
                for( int i = 0; i < w; i++ ) {
                    if( data.path[k] == int.MaxValue ) { k++; continue; }
                    addCell = data.path[k] + 1;
                    if( data.path[k] > 0 ) {
                        if( i > 0 ) {
                            if( data.path[k - 1] == 0 )  { data.path[k - 1] = addCell; done = false; }
                            if( (j > 0  ) && (data.path[k - 1 - w] == 0) ) { data.path[k - 1 - w] = addCell; done = false; }
                            if( (j < h-1) && (data.path[k - 1 + w] == 0) ) { data.path[k - 1 + w] = addCell; done = false; }
                        }
                        if( (j > 0  ) && (data.path[k - w] == 0) ) { data.path[k - w] = addCell; done = false; }
                        if( (j < h-1) && (data.path[k + w] == 0) ) { data.path[k + w] = addCell; done = false; }
                        if( i < w-1 ) {
                            if( data.path[k + 1] == 0 ) { data.path[k + 1] = addCell; done = false; }
                            if( (j > 0  ) && (data.path[k + 1 - w] == 0) ) { data.path[k + 1 - w] = addCell; done = false; }
                            if( (j < h-1) && (data.path[k + 1 + w] == 0) ) { data.path[k + 1 + w] = addCell; done = false; }
                        }
                    }
                    k++;
                }
            }
            
        }

        /* // temp for testing
        Debug.Log( data.x + " " + data.y );
        string s;
        k = 0;
        Debug.Log( "_______________________________" );
        for( int j = 0; j < h; j++ ) {
            s = "";
            for( int i = 0; i < w; i++ ) {
                if( data.path[k] == -1 )
                    s += "# ";
                else
                    s += data.path[k] + " ";
                k++;
            }
            Debug.Log( s );
        }
        Debug.Log( "_______________________________" );
        */
    }

    public void SortSpawnsByTime()
    {
        if( data.spawns.Count > 0 ) {
            List<spawnData> SpawnList = new List<spawnData>();

            spawnData newSpawn = data.spawns[0];
            SpawnList.Add( newSpawn );
            int j;
            for( int i = 1; i < data.spawns.Count; i++ ) {
                newSpawn = data.spawns[i];
                for( j = 0; j < SpawnList.Count; j++ ) {
                    if( SpawnList[j].time > newSpawn.time ) break;
                }
                SpawnList.Insert( j, newSpawn );
            }
            for( int i = 0; i < data.spawns.Count; i++ ) {
                data.spawns[i] = SpawnList[data.spawns.Count-1-i];
            }
            SpawnList.Clear();
        }
    }

    // temp for testing
    public void GenerateSpawns( int count )
    {
        Debug.Log( "GenerateSpawns" );
        spawnData newSpawn; //
        float distance = 10f;
        int spawnTypes = 6;
        int minTime = 2;
        int maxTime = 15;
        data.spawnsCount = count;
        for (int i = 0; i < count; i++) {
            newSpawn = new spawnData();
            newSpawn.xEditor = Random.Range( -distance, distance + data.width );
            //if( newSpawn.xEditor >= 0 ) newSpawn.xEditor += data.width;
            newSpawn.yEditor = Random.Range( -distance, distance + data.height );
            //if( newSpawn.yEditor >= 0 ) newSpawn.yEditor += data.height;
            if ( (newSpawn.xEditor >= 0) && (newSpawn.xEditor < data.width) && (newSpawn.yEditor >= 0) && (newSpawn.yEditor < data.height) ) {
                switch ( Random.Range(0,4) ) {
                    case 0: newSpawn.xEditor += data.width; break;
                    case 1: newSpawn.xEditor -= data.width; break;
                    case 2: newSpawn.yEditor += data.height; break;
                    case 3: newSpawn.yEditor -= data.height; break;
                }
            }
                newSpawn.type = Random.Range( 0, spawnTypes );
            newSpawn.time = Random.Range( minTime, maxTime );

            data.spawns.Add( newSpawn );
        }
        Debug.Log( data.spawns.Count );
    }
}
