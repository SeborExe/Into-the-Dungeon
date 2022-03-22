using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSwap
{
    public int tileNum;
    public GameObject swapPrefab;
    public GameObject guaranteedItemDrop;
    public int overrideTileNum = -1;
}

public class TileCamera : MonoBehaviour
{
    static private int W, H;
    static private int[,] MAP;
    static public Sprite[] SPRITES;
    static public Transform TILE_ANCHOR;
    static public Tile[,] TILES;
    static public string COLLISIONS;

    [Header("Definiowane w panelu inspekcyjnym")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollision;
    public Tile tilePrefab;
    public int defaultTileNum;
    public List<TileSwap> tileSwaps;

    private Dictionary<int, TileSwap> tileSwapDict;
    private Transform enemyAnchor, itemAnchor;

    private void Awake()
    {
        COLLISIONS = Utils.RemoveLineEndings(mapCollision.text);
        PrepareTileSwapDict();
        enemyAnchor = (new GameObject("Enemy Anchor")).transform;
        itemAnchor = (new GameObject("Item Anchor")).transform;
        LoadMap();
    }

    public void LoadMap()
    {
        GameObject go = new GameObject("TILE_ANCHOR");
        TILE_ANCHOR = go.transform;
        SPRITES = Resources.LoadAll<Sprite>(mapTiles.name);

        //Wczytywanie danych mapy
        string[] lines = mapData.text.Split('\n');
        H = lines.Length;
        string[] tileNums = lines[0].Split(' ');
        W = tileNums.Length;
        System.Globalization.NumberStyles hexNum;
        hexNum = System.Globalization.NumberStyles.HexNumber;

        //Umieszczanie danych mapy w dwuwymiarowej tablicy - szybszy dostêp
        MAP = new int[W, H];
        for (int j=0; j<H; j++)
        {
            tileNums = lines[j].Split(' ');
            for (int i=0; i<W; i++)
            {
                if (tileNums[i] == "..")
                {
                    MAP[i, j] = 0;
                } 
                else
                {
                    MAP[i, j] = int.Parse(tileNums[i], hexNum);
                }

                CheckTileSwaps(i, j);
            }
        }

        print("Rozmiar mapy to: " + W + "x" + H);

        ShowMap();
    }

    static public int GET_MAP(int x, int y)
    {
        if (x<0 || x>=W || y<0 || y>=H)
        {
            return -1;
        }

        return MAP[x, y];
    }

    static public int GET_MAP(float x, float y)
    {
        int tX = Mathf.RoundToInt(x);
        int tY = Mathf.RoundToInt(y - 0.25f);
        return GET_MAP(tX, tY);
    }

    static public void SET_MAP(int x, int y, int tNum)
    {
        if (x<0 || x>=W || y<0 || y>=H)
        {
            return;
        }

        MAP[x, y] = tNum;
    }

    void ShowMap()
    {
        TILES = new Tile[W, H];

        for (int j=0; j<H; j++)
        {
            for (int i=0; i<W; i++)
            {
                if (MAP[i, j] != 0)
                {
                    Tile ti = Instantiate<Tile>(tilePrefab);
                    ti.transform.SetParent(TILE_ANCHOR);
                    ti.SetTile(i, j);
                    TILES[i, j] = ti;
                }
            }
        }
    }

    void PrepareTileSwapDict()
    {
        tileSwapDict = new Dictionary<int, TileSwap>();
        foreach (TileSwap ts in tileSwaps)
        {
            tileSwapDict.Add(ts.tileNum, ts);
        }
    }

    void CheckTileSwaps(int i, int j)
    {
        int tNum = GET_MAP(i, j);
        if (!tileSwapDict.ContainsKey(tNum)) return;

        //Podmieniamy kafelek
        TileSwap ts = tileSwapDict[tNum];
        if (ts.swapPrefab != null)
        {
            GameObject go = Instantiate(ts.swapPrefab);
            Enemy e = go.GetComponent<Enemy>();
            if (e != null)
            {
                go.transform.SetParent(enemyAnchor);
            }
            else
            {
                go.transform.SetParent(itemAnchor);
            }

            go.transform.position = new Vector3(i, j, 0);
            if (ts.guaranteedItemDrop != null)
            {
                if (e != null)
                {
                    e.guarantedItemDrop = ts.guaranteedItemDrop;
                }
            }
        }

        //Umieszczamy w jego miejsce kafelek ¿eby nie by³o dziury
        if (ts.overrideTileNum == -1)
        {
            SET_MAP(i, j, defaultTileNum);
        }
        else
        {
            SET_MAP(i, j, ts.overrideTileNum);
        }
    }
}
