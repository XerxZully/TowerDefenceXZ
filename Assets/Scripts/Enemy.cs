using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct dataEnemy {
    public RuntimeAnimatorController Controller;
    public Sprite sprite;
    public float Speed;
    public float Health;
    public float Damage;
    public int Score;
}

public class Enemy : MonoBehaviour
{
    [SerializeField] public dataEnemy[] Enemies = null;
    [SerializeField] public int type = 0;//0=ameba, 1=biter, 2=jelly, ...
    [HideInInspector] public float time = 0;
    [HideInInspector] public float Health = 1;

    static public LevelManager levelGameObject = null;
    static public Level level = null;
    static public PlayerBase Base = null;
    static public int w;
    static public int h;
    private float xTarget = 0;
    private float yTarget = 0;
    private float dx = 0;
    private float dy = 0;

    static private float globalSpeedRate = 0.2f;
    static private float targetBorder = 0.1f;
    static private float baseBorder = 0.5f;
    static private float updateCycle = 0.5f;
    static private float update = 0.0f;

    private void Awake()
    {
        if( levelGameObject == null ) {
            levelGameObject = GameObject.Find( "LevelMngr" ).GetComponent<LevelManager>();
            level = levelGameObject.level;
            Base = GameObject.Find( "Base" ).GetComponent<PlayerBase>();
            w = level.data.width;
            h = level.data.height;
        }
    }
    public void ChangeHealth( float value )
    {
        Health += value;
        if( Health <= 0 ) {
            AudioSource.PlayClipAtPoint( levelGameObject.SoundDeath, transform.position, 1f );
            levelGameObject.score += Enemies[type].Score;
            levelGameObject.money += Enemies[type].Score;
            levelGameObject.EnemiesList.Remove( gameObject );
            Destroy( gameObject );
        } else {
            AudioSource.PlayClipAtPoint( levelGameObject.SoundHit, transform.position, 0.7f );
        }
    }

    public void CreateEnemy( int setType, float setTime, Vector3 pos )
    {
        transform.position = pos;
        type = setType;
        time = setTime;
        Health = Enemies[type].Health;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = Enemies[type].Controller;
        gameObject.GetComponent<SpriteRenderer>().sprite = Enemies[type].sprite;

        xTarget = Mathf.Clamp( Mathf.Floor( transform.position.x ), 0f, w - 1 ) + 0.5f;
        yTarget = Mathf.Clamp( Mathf.Floor( transform.position.y ), 0f, h - 1 ) + 0.5f;

        if( pos.x <= 0 ) { dx = +1; dy =  0; }
        if( pos.x >= w ) { dx = -1; dy =  0; }
        if( pos.y <= 0 ) { dx =  0; dy = +1; }
        if( pos.y >= h ) { dx =  0; dy = -1; }
        dx *= Enemies[type].Speed * globalSpeedRate;
        dy *= Enemies[type].Speed * globalSpeedRate;
    }

    public bool ReachBase()
    {
        if( (Mathf.Abs( transform.position.x - level.data.x - 0.5f ) < baseBorder) &&
            (Mathf.Abs( transform.position.y - level.data.y - 0.5f ) < baseBorder) ) {

            return true;
        }
        return false;
    }

    public void getNextTarget()
    {
        if( (Mathf.Abs( xTarget - transform.position.x ) < targetBorder) &&
            (Mathf.Abs( yTarget - transform.position.y ) < targetBorder )) {
            dx = 0;
            dy = 0;

            int x = ( int )transform.position.x;
            int y = ( int )transform.position.y;
            y = h - y - 1;
            x = Mathf.Clamp( x, 0, w-1 );
            y = Mathf.Clamp( y, 0, h-1 );

            int index = x + y * level.data.width;
            int minX = x;
            int minY = y;
            int minCost = level.data.path[index];
            int newCost = int.MaxValue;

            if ( x > 0 ) {
                newCost = level.data.path[index - 1];
                if( newCost <= minCost ) { minCost = newCost; minX = x-1; minY = y; }
            }
            if( x < w-1 ) {
                newCost = level.data.path[index + 1];
                if( newCost <= minCost ) { minCost = newCost; minX = x+1; minY = y; }
            }
            if( y > 0 ) {
                newCost = level.data.path[index - w];
                if( newCost <= minCost ) { minCost = newCost; minX = x; minY = y-1; }
            }
            if( y < h - 1 ) {
                newCost = level.data.path[index + w];
                if( newCost <= minCost ) { minCost = newCost; minX = x; minY = y+1; }
            }

            minY = h - minY - 1;
            if( transform.position.x != minX ) xTarget = minX;
            if( transform.position.y != minY ) yTarget = minY;

            x = ( int )Mathf.Floor( transform.position.x );
            y = ( int )Mathf.Floor( transform.position.y );
            dx = 0;
            dy = 0;
            if( xTarget > x ) { dx = +1; dy =  0; }
            if( xTarget < x ) { dx = -1; dy =  0; }
            if( yTarget > y ) { dx =  0; dy = +1; }
            if( yTarget < y ) { dx =  0; dy = -1; }
            dx *= Enemies[type].Speed * globalSpeedRate;
            dy *= Enemies[type].Speed * globalSpeedRate;

            xTarget = Mathf.Clamp( Mathf.Floor( xTarget ), 0f, w - 1 ) + 0.5f;
            yTarget = Mathf.Clamp( Mathf.Floor( yTarget ), 0f, h - 1 ) + 0.5f;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if( ReachBase() ) {
            if( Base.Health > 0 ) {
                update += Time.deltaTime;
                if( update > updateCycle ) {
                    update = 0.0f;
                    Base.ChangeHealth( -Enemies[type].Damage ); 
                }
            }
        } else {
            //transform.position.z = transform.position.y * 0.01f - 2f
            transform.Translate( dx * Time.deltaTime, dy * Time.deltaTime, 0f );
            transform.position = new Vector3( transform.position.x, transform.position.y, transform.position.y * 0.01f - 1f );
            getNextTarget();
        }
    }
}
