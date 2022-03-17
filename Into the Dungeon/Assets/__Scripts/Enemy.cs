using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected static Vector3[] directions = new Vector3[]
    {
        Vector3.right, Vector3.up, Vector3.left, Vector3.down
    };

    [Header("Definiowane w panelu inspekcyjnym: Enemy")]
    public float maxHealth;

    [Header("Definiowane dynamicznie")]
    public float health;

    protected Animator anim;
    protected Rigidbody rigid;
    protected SpriteRenderer sRend;

    protected virtual void Awake()
    {
        health = maxHealth;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        sRend = GetComponent<SpriteRenderer>();
    }
}
