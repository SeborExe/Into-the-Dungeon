using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeletos : Enemy, IFacingMover
{
    [Header("Definiowane w panelu inspekcyjnym: Skeletos")]
    public int speed = 2;
    public float timeThinkMin = 1f;
    public float timeThinkMax = 4f;

    [Header("Definiowane dynamicznie")]
    public int facing = 0;
    public float timeNextDecision = 0;

    private InRoom inRm;

    protected override void Awake()
    {
        base.Awake();
        inRm = GetComponent<InRoom>();
    }

    private void Update()
    {
        if (Time.time >= timeNextDecision)
        {
            DecideDirection();
        }

        rigid.velocity = directions[facing] * speed;
    }

    private void DecideDirection()
    {
        facing = Random.Range(0, 4);
        timeNextDecision = Time.time + Random.Range(timeThinkMin, timeThinkMax);
    }

    public int GetFacing() => facing;

    public float GetSpeed() => speed;

    public Vector2 GetRoomPosOnGrid(float mult = -1) => inRm.GetRoomPosOnGrid(mult);

    public bool moving => true;

    public float gridMult => inRm.gridMult;

    public Vector2 roomPos { get => inRm.roomPos; set => inRm.roomPos = value; }
    public Vector2 roomNum { get => inRm.roomNum; set => inRm.roomNum = value; }
}
