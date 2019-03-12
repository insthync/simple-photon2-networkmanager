using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePhotonStartPoint : MonoBehaviour
{
    public Vector3 position { get { return transform.position; } }
    public Quaternion rotation { get { return transform.rotation; } }
}
