using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum eType { key, health, grappler }

    public static float COLLIDER_DELAY = 0.5f;

    [Header("Definiowane w panelu inspekcyjnym")]
    public eType itemType;

    //Metody Avake i Active blokuj¹ dzia³anie zderzacza PickUp na pó³ sekundy
    private void Awake()
    {
        GetComponent<Collider>().enabled = false;
        Invoke("Active", COLLIDER_DELAY);
    }

    void Active()
    {
        GetComponent<Collider>().enabled = true;
    }
}
