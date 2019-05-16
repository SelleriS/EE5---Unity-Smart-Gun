using UnityEngine;

public class WeaponInteraction57 : WeaponInteraction
{
    // Start is called before the first frame update
    void Start()
    {
        triggerMinRot = -0.6111399f;
        triggerMaxRot = -0.55f;
        triggerTransform = transform.Find("object.001").transform;
        Vector3 currPos = transform.position;
        Vector3 offset = new Vector3(0.15f, -0.12f, 0); // Estimated by looking at the specific flare gun prefab
        transform.position = currPos + offset;
    }

    protected override void SetTrigger(int triggerValue)
    {
        triggerCurrentValue = Mathf.Clamp(triggerValue, 0, 255);
        float triggerXMov = triggerMinRot + (triggerMaxRot - triggerMinRot) * ((float)triggerValue / 255);
        Vector3 newPos = new Vector3(triggerXMov, triggerTransform.position.y, triggerTransform.position.z);
        triggerTransform.position = newPos;
    }
}
