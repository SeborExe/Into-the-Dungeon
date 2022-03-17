using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFacingMover
{
    int Facing { get; }
    bool moving { get; }
    float Speed { get; }
    float gridMult { get; }
    Vector2 roomPos { get; set; }
    Vector2 roomNum { get; set; }
    Vector2 GetRoomPosOnGrid(float mult = -1);
}
