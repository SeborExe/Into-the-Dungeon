using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    public enum eMode { none, gOut, gInMiss, gInHit }

    [Header("Definiowane w panelu inspekcyjnym")]
    public float grappleSpd = 10;
    public float grappleLength = 7;
    public float grappleInLength = 0.5f;
    public int unsafeTileHealthPenalty = 2;
    public TextAsset mapGrappleable;

    [Header("Definiowane dynamicznie")]
    public eMode mode = eMode.none;

    //Numery kafli, do których mo¿na wystrzeliwaæ hak
    public List<int> grappleTiles;
    public List<int> unsafeTiles;

    private Dray dray;
    private Rigidbody rigid;
    private Animator anim;
    private Collider drayColld;

    private GameObject grapHead;
    private LineRenderer grapLine;
    private Vector3 p0, p1;
    private int facing;

    private Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    private void Awake()
    {
        string gTiles = mapGrappleable.text;
        gTiles = Utils.RemoveLineEndings(gTiles);
        grappleTiles = new List<int>();
        unsafeTiles = new List<int>();

        for (int i = 0; i < gTiles.Length; i++)
        {
            switch(gTiles[i])
            {
                case 'S':
                    grappleTiles.Add(i);
                    break;

                case 'X':
                    unsafeTiles.Add(i);
                    break;
            }
        }

        dray = GetComponent<Dray>();
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        drayColld = GetComponent<Collider>();

        Transform trans = transform.Find("Grappler");
        grapHead = trans.gameObject;
        grapLine = grapHead.GetComponent<LineRenderer>();
        grapHead.SetActive(false);
    }

    private void Update()
    {
        if (!dray.hasGrappler) return;

        switch(mode)
        {
            case eMode.none:
                if (Input.GetKeyDown(KeyCode.L))
                {
                    StartGrapple();
                }

                break;
        }
    }

    void StartGrapple()
    {
        facing = dray.GetFacing();
        dray.enabled = false;
        anim.CrossFade("Dray_Attack_" + facing, 0);
        drayColld.enabled = false;
        rigid.velocity = Vector3.zero;

        grapHead.SetActive(true);

        p0 = transform.position + (directions[facing] * 0.5f);
        p1 = p0;
        grapHead.transform.position = p1;
        grapHead.transform.rotation = Quaternion.Euler(0, 0, 90 * facing);

        grapLine.positionCount = 2;
        grapLine.SetPosition(0, p0);
        grapLine.SetPosition(1, p1);
        mode = eMode.gOut;
    }

    private void FixedUpdate()
    {
        switch (mode)
        {
            case eMode.gOut: //Wystrzelenie haku
                p1 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                grapHead.transform.position = p1;
                grapLine.SetPosition(1, p1);

                //Sprawdzenie czy hak w coœ trafi³
                int tileNum = TileCamera.GET_MAP(p1.x, p1.y);
                if (grappleTiles.IndexOf(tileNum) != -1)
                {
                    mode = eMode.gInHit;
                    break;
                }

                if ((p1 - p0).magnitude >= grappleLength)
                {
                    mode = eMode.gInMiss;
                }

                break;

            case eMode.gInMiss: //Je¿eli nietrafimy to hak wraca z prêdkoœci¹ 2x
                p1 -= directions[facing] * 2 * grappleSpd * Time.fixedDeltaTime;
                if (Vector3.Dot((p1-p0), directions[facing]) > 0)
                {
                    grapHead.transform.position = p1;
                    grapLine.SetPosition(1, p1);
                }
                else
                {
                    StopGrapple();
                }

                break;

            case eMode.gInHit: //Trafiony wiêc przyci¹gamy siê do œciany
                float dist = grappleInLength + grappleSpd * Time.fixedDeltaTime;
                if (dist > (p1-p0).magnitude)
                {
                    p0 = p1 - (directions[facing] * grappleInLength);
                    transform.position = p0;
                    StopGrapple();
                    break;
                }

                p0 += directions[facing] * grappleSpd * Time.fixedDeltaTime;
                transform.position = p0;
                grapLine.SetPosition(0, p0);
                grapHead.transform.position = p1;
                break;
        }
    }

    void StopGrapple()
    {
        dray.enabled = true;
        drayColld.enabled = true;

        //Sprawdzenie czy kafelek jest niebezpieczny
        int tileNum = TileCamera.GET_MAP(p0.x, p0.y);
        if (mode == eMode.gInHit && unsafeTiles.IndexOf(tileNum) != -1)
        {
            //Jesteœmy na niebezpiecznym kafelku
            dray.ResetInRoom(unsafeTileHealthPenalty);
        }

        grapHead.SetActive(false);
        mode = eMode.none;
    }

    private void OnTriggerEnter(Collider colld)
    {
        Enemy e = colld.GetComponent<Enemy>();
        if (e == null) return;

        mode = eMode.gInMiss;
    }
}
