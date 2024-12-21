using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionEffect : MonoBehaviour
{
    public abstract void Run( ActionResult source, Transform firePoint );

}
