using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode { idle, move, attack, transition, knockback }

    [Header("Definiowane w panelu inspekcyjnym")]
    public float speed = 5;
    public float attackDuration = 0.25f;
    public float attackDelay = 0.5f;
    public float transitionDelay = 0.5f; //OpóŸnienie przejœcia miêdzy poziomami
    public int maxHealth = 10;
    public float knockbackSpeed = 10;
    public float knockbackDuration = 0.25f;
    public float invincibleDuration = 0.5f;

    [Header("Definiowane dynamicznie")]
    public int dirHeld = -1; //Kierunek poruszania siê
    public int facing = 1; //Kierunek patrzenia
    public eMode mode = eMode.idle;
    public int numKeys = 0;
    public bool invincible = false;

    [SerializeField]
    private int _health;

    public int health
    {
        get { return _health; }
        set { _health = value; }
    }

    private float timeAtkDone = 0;
    private float timeAtkNext = 0;
    private float transitionDone = 0;
    private Vector2 transitionPos;
    private float knockbackDone = 0;
    private float invincibleDone = 0;
    private Vector3 knockbackVel;

    private SpriteRenderer sRend;
    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;

    private Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    private KeyCode[] buttons = new KeyCode[]
    {
        KeyCode.D, KeyCode.W, KeyCode.A, KeyCode.S
    };
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
        sRend = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    private void Update()
    {
        //Sprawdzenie odrzucenia i nieœmiertelnoœci
        if (invincible && Time.time > invincibleDone) invincible = false;
        sRend.color = invincible ? Color.red : Color.white;

        if (mode == eMode.knockback)
        {
            rigid.velocity = knockbackVel;
            if (Time.time < knockbackDone) return;
        }

        if (mode == eMode.transition)
        {
            rigid.velocity = Vector3.zero;
            anim.speed = 0;
            roomPos = transitionPos;

            if (Time.time < transitionDone) return;
            mode = eMode.idle;
        }

        //--Obs³uga przycisków--//
        dirHeld = -1;

        for (int i=0; i<4; i++)
        {
            if (Input.GetKey(buttons[i])) dirHeld = i;
        }

        if (Input.GetKeyDown(KeyCode.K) && Time.time >= timeAtkNext)
        {
            mode = eMode.attack;
            timeAtkDone = Time.time + attackDuration;
            timeAtkNext = Time.time + attackDelay;
        }

        if (Time.time >= timeAtkDone)
        {
            mode = eMode.idle;
        }

        if (mode != eMode.attack)
        {
            if (dirHeld == -1)
            {
                mode = eMode.idle;
            }
            else
            {
                facing = dirHeld;
                mode = eMode.move;
            }
        }

        //----------Dzia³anie z bie¿¹cym stanem Postaci Draya----------------//
        Vector3 vel = Vector3.zero;

        switch (mode)
        {
            case eMode.attack:
                anim.CrossFade("Dray_Attack_" + facing, 0);
                anim.speed = 0;
                break;

            case eMode.idle:
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 0;
                break;

            case eMode.move:
                vel = directions[dirHeld];
                anim.CrossFade("Dray_Walk_" + facing, 0);
                anim.speed = 1;
                break;
        }

        rigid.velocity = vel * speed;
    }

    private void LateUpdate()
    {
        Vector2 rPos = GetRoomPosOnGrid(0.5f); //Wymuszenie dok³adnoœci do po³owy kratki

        //Sprawdzenie czy gracz jest na kafelku
        int doorNum;
        for (doorNum=0; doorNum<4; doorNum++)
        {
            if (rPos == InRoom.DOORS[doorNum])
            {
                break;
            }
        }

        if (doorNum > 3 || doorNum != facing) return;

        //Przejœcie do nastêpnego pomieszczenia
        Vector2 rm = roomNum;
        switch (doorNum)
        {
            case 0: //Prawo
                rm.x += 1;
                break;
            case 1: //Góra
                rm.y += 1;
                break;
            case 2: //Lewo
                rm.x -= 1;
                break;
            case 3: //Dó³
                rm.y -= 1;
                break;
        }

        //Sprawdzenie czy pomieszczenie do którego przechodzimy jest poprawnie zdefiniowane
        if (rm.x >= 0 && rm.x <= InRoom.MAX_RM_X)
        {
            if (rm.y >= 0 && rm.y <= InRoom.MAX_RM_Y)
            {
                roomNum = rm;
                transitionPos = InRoom.DOORS[(doorNum + 2) % 4];

                roomPos = transitionPos;
                mode = eMode.transition;
                transitionDone = Time.time + transitionDelay;
            }
        }
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (invincible) return;
        DamageEffect dEf = coll.gameObject.GetComponent<DamageEffect>();
        if (dEf == null) return;

        health -= dEf.damage;
        invincible = true;
        invincibleDone = Time.time + invincibleDuration;

        if (dEf.knockback)
        {
            Vector3 delta = transform.position - coll.transform.position;
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                //Odrzucenie w kierunku poziomym
                delta.x = (delta.x > 0) ? 1 : -1;
                delta.y = 0;
            }
            else
            {
                //Odrzucenie w kierunku pionowym
                delta.x = 0;
                delta.y = (delta.y > 0) ? 1 : -1;
            }

            knockbackVel = delta * knockbackSpeed;
            rigid.velocity = knockbackVel;

            mode = eMode.knockback;
            knockbackDuration = Time.time + knockbackDuration;
        }
    }

    private void OnTriggerEnter(Collider colld)
    {
        PickUp pup = colld.GetComponent<PickUp>();
        if (pup == null) return;

        switch (pup.itemType)
        {
            case PickUp.eType.health:
                health = Mathf.Min(health + 2, maxHealth);
                break;

            case PickUp.eType.key:
                keyCount++;
                break;
        }

        Destroy(colld.gameObject);
    }

    public int GetFacing()
    {
        return facing;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public Vector2 GetRoomPosOnGrid(float mult = -1)
    {
        return inRm.GetRoomPosOnGrid(mult);
    }

    public bool moving
    {
        get { return (mode == eMode.move); }
    }

    public float gridMult
    {
        get { return inRm.gridMult; }
    }

    public Vector2 roomPos 
    {
        get { return inRm.roomPos; }  set { inRm.roomPos = value; } 
    }
    public Vector2 roomNum 
    {
        get { return inRm.roomNum; }
        set { inRm.roomNum = value; }
    }
    public int keyCount 
    {
        get { return numKeys; }
        set { numKeys = value; } 
    }
}
