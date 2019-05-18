using UnityEngine;

/*
 * This class is attached to the FiveSeven prefab and is responsible to apply
 * the received changes to it.
 * Only the trigger is implemented. The method for the trigger in this class
 * defers from its parent class by the axis over which the rotation must occur
 */

public class WeaponInteraction57 : WeaponInteraction
{
    // Start is called before the first frame update
    void Start()
    {
        triggerMinRot = 13;
        triggerMaxRot = -15;
        triggerTransform = transform.Find("object_001").transform;
    }

    // Set the position of the trigger by rotating over the X-axis (for other objects the rotation happens over the Z-axis)
    protected override void SetTrigger(int triggerValue)
    {
        triggerCurrentValue = Mathf.Clamp(triggerValue, 0, 255);
        float triggerXRot = triggerMinRot + (triggerMaxRot - triggerMinRot) * ((float)triggerValue / 255); // Maps the current value of the trigger given by the client (value between 0 and 255) between triggerMinRot and triggerMaxRot
        triggerTransform.localEulerAngles = new Vector3(triggerXRot, triggerTransform.localEulerAngles.y, triggerTransform.localEulerAngles.z);
    }
}
