using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dray : MonoBehaviour, IFacingMover, IKeyMaster
{
    public enum eMode { idle, move, attack, transition }

    [Header("Definiowane w panelu inspekcyjnym")]
    public float speed = 5;
    public float attackDuration = 0.25f;
    public float attackDelay = 0.5f;
    public float transitionDelay = 0.5f; //OpóŸnienie przejœcia miêdzy poziomami

    [Header("Definiowane dynamicznie")]
    public int dirHeld = -1; //Kierunek poruszania siê
    public int facing = 1; //Kierunek patrzenia
    public eMode mode = eMode.idle;
    public int numKeys = 0;

    private float timeAtkDone = 0;
    private float timeAtkNext = 0;
    private float transitionDone = 0;
    private Vector2 transitionPos;

    private Rigidbody rigid;
    private Animator anim;
    private InRoom inRm;

    private Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    private KeyCode[] keys = new KeyCode[]
    {
        KeyCode.D, KeyCode.W, KeyCode.A, KeyCode.S
    };
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        inRm = GetComponent<InRoom>();
    }

    private void Update()
    {
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
            if (Input.GetKey(keys[i])) dirHeld = i;
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
            case 0:
                rm.x += 1;
                break;
            case 1:
                rm.y += 1;
                break;
            case 2:
                rm.x -= 1;
                break;
            case 3:
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

    public int GetFacing() => facing;

    public float GetSpeed() => speed;

    public Vector2 GetRoomPosOnGrid(float mult = -1) => inRm.GetRoomPosOnGrid(mult);

    public bool moving => (mode == eMode.move);

    public float gridMult => inRm.gridMult;

    public Vector2 roomPos { get => inRm.roomPos; set => inRm.roomPos = value; }
    public Vector2 roomNum { get => inRm.roomNum; set => inRm.roomNum = value; }
    public int KeyCount { get => numKeys; set => numKeys = value; }
}
