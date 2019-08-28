using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct dataTower {
    public RuntimeAnimatorController Controller;
    public RuntimeAnimatorController BulletController;
    public float UpdateTime; // UpdateTime = updateCycle
    public int Cost;
    public float Damage;
    public float Radius;
    public AudioClip sound;
}

public class Tower : MonoBehaviour {
    [SerializeField] private GameObject prefabTowerBase = null;
    [SerializeField] private GameObject prefabBullet = null;
    [SerializeField] public dataTower[] Towers = null;
    [SerializeField] public AudioClip SoundBuild = null;
    [HideInInspector] public int type = 0;
    [HideInInspector] public float Health = 0;

    private GameObject towerBase = null;
    static public GameObject levelGameObject = null;
    static public LevelManager levelMngr = null;
    static public Level level = null;
    private GameObject target = null;

    private float updateCycle = 1.0f;
    private float update = 0.0f;
    private Quaternion rotationToTarget;

    // Start is called before the first frame update
    void Awake()
    {
        if( levelGameObject == null ) {
            levelGameObject = GameObject.Find( "LevelMngr" );
            levelMngr = levelGameObject.GetComponent<LevelManager>();
            level = levelMngr.level;
        }
    }

    public void CreateTower( int setType, Vector3 pos )
    {
        transform.position = pos;
        type = setType;
        updateCycle = Towers[type].UpdateTime;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = Towers[type].Controller;

        towerBase = Instantiate( prefabTowerBase );
        towerBase.transform.position = new Vector3( pos.x, pos.y, pos.y*0.01f - 1f );
        towerBase.transform.parent = levelGameObject.transform;
        rotationToTarget = transform.rotation;

        AudioSource.PlayClipAtPoint( SoundBuild, transform.position, 0.9f );
    }

    // Update is called once per frame
    void Update()
    {
        update += Time.deltaTime;
        if( update >= updateCycle ) {
            update = 0.0f;

            if( !HaveTarget() ) FindTarget();

            if( HaveTarget() ) {
                rotationToTarget = Quaternion.LookRotation( transform.position - target.transform.position, Vector3.forward );
                rotationToTarget.x = 0f;
                rotationToTarget.y = 0f;

                Shoot();
            }
        }
        transform.rotation = Quaternion.Slerp( transform.rotation, rotationToTarget, Time.deltaTime * 10f );

    }

    private bool HaveTarget()
    {
        if( target == null ) return false;
        if( target.activeSelf == false ) {
            target = null;
            return false;
        }

        return TargetInRect( target.transform.position.x, target.transform.position.y );
    }

    private bool FindTarget()
    {
        int index = -1;
        for( int i = 0; i < levelMngr.EnemiesList.Count; i++ ) {
            if( levelMngr.EnemiesList[i].activeSelf ) {
                if( TargetInRect( levelMngr.EnemiesList[i].transform.position.x, levelMngr.EnemiesList[i].transform.position.y ) ) {
                    index = i;
                    break;
                }
            }
        }

        if( index > -1 ) {
            target = levelMngr.EnemiesList[index];
            return true;
        }
        return false;
    }

    private bool TargetInRect( float x, float y )
    {
        if( (x > (transform.position.x - Towers[type].Radius)) && (x < (transform.position.x + Towers[type].Radius)) &&
            (y > (transform.position.y - Towers[type].Radius)) && (y < (transform.position.y + Towers[type].Radius)) ) {
            return true;
        }
        return false;
    }

    public void Shoot()
    {
        AudioSource.PlayClipAtPoint( Towers[type].sound, Camera.main.transform.position, 1.0f );

        GameObject newBullet = Instantiate( prefabBullet );
        newBullet.GetComponent<Animator>().runtimeAnimatorController = Towers[type].BulletController;
        newBullet.GetComponent<Bullet>().CreateBullet( transform.position, target.transform.position, target.GetComponent<Enemy>(), Towers[type].Damage, Towers[type].UpdateTime );
        newBullet.transform.rotation = rotationToTarget;
        newBullet.transform.parent = levelGameObject.transform;
    }

    void OnDestroy()
    {
        Destroy( towerBase );
    }
}
