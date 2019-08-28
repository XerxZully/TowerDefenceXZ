using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Enemy enemy = null;
    private float Damage;
    public Vector3 target;
    private static float speedMult = 4.0f;
    [HideInInspector] public float speed = 1.0f;

    void Awake()
    {
        transform.gameObject.SetActive( false );
    }

    public void CreateBullet( Vector3 setPos, Vector3 setTarget, Enemy setEnemy, float setDamage, float setSpeed )
    {
        setPos.z += 0.01f;
        transform.position = setPos;
        target = setTarget;
        enemy = setEnemy;
        Damage = setDamage;
        speed = setSpeed * speedMult;
        transform.gameObject.SetActive( true );
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards( transform.position, target, speed * Time.deltaTime );

        if( Vector3.Distance( transform.position, target ) < 0.05f ) {
            if( enemy != null ) enemy.ChangeHealth( -Damage );
            Destroy( this.gameObject );
        }
    }
}
