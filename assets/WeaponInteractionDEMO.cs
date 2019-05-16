using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInteractionDEMO : WeaponInteraction
{
    // Start is called before the first frame update
    void Start()
    {
        triggerMinRot = 110;
        triggerMaxRot = 80;
        triggerTransform = transform.Find("Plane.003").transform;
        Vector3 currPos = transform.position;
        Vector3 offset = new Vector3(0.17f, -0.1f, 0); // Estimated by looking at the specific flare gun prefab
        transform.position = currPos + offset;
    }
}
