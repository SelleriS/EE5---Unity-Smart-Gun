using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum SwitchPosition
{
    SAFE,
    SEMI,
    AUTO
}

public class WeaponInteraction : MonoBehaviour
{
    public int triggerCurrentValue;
    public int movablePiecesCurrentValue;
    public int magazineID;
    public bool magazinePresent;
    public SwitchPosition selectorSwitchPosition;

    Transform triggerTransform;
    Transform movablePiecesTransform;
    Transform selectorSwitchTransform;
    GameObject magazineObject;

    float triggerPulledRot = 15;

    float movablePiecesMaxXpos = 0.117f;

    // Start is called before the first frame update
    void Start()
    {

        triggerTransform = transform.Find("Trigger").transform;
        movablePiecesTransform = transform.Find("Movable Pieces").transform;
        selectorSwitchTransform = transform.Find("Selector Switch").transform;
        magazineObject = transform.Find("Magazine").gameObject;
        selectorSwitchPosition = SwitchPosition.SAFE;
    }

    // Update is called once per frame
    void Update()
    {
        //Selector Switch
        switch (selectorSwitchPosition)
        {
            case SwitchPosition.SAFE:
                selectorSwitchTransform.eulerAngles = new Vector3(0, 0, 30);
                break;

            case SwitchPosition.SEMI:
                selectorSwitchTransform.eulerAngles = new Vector3(0, 0, 0);
                break;

            case SwitchPosition.AUTO:
                selectorSwitchTransform.localEulerAngles = new Vector3(0, 0, -30);
                break;
        }

        //Movable pieces
        movablePiecesCurrentValue = Mathf.Clamp(movablePiecesCurrentValue, 0, 255);
        float piecesXPos = movablePiecesMaxXpos * ((float)movablePiecesCurrentValue / 255);
        movablePiecesTransform.localPosition = new Vector3(piecesXPos, 0, 0);

        //Trigger
        triggerCurrentValue = Mathf.Clamp(triggerCurrentValue, 0, 255);
        float triggerZRot = triggerPulledRot * ((float)triggerCurrentValue / 255);
        triggerTransform.localEulerAngles = new Vector3(0, 0, triggerZRot);

        //Magazine
        //if no magazine is present, disable the magazine object (-> disable redering)
        magazineObject.SetActive(magazinePresent);
    }

    // Translate the data and edit the parameters of the virtual weapon
    public void PacketTranslater(byte[] packet) // if reading problems occur, this can be due to the endian (the sequence in which the bits are send, LSB first or last?)
    {
        // Create storage containers
        byte[] triggerValue = new byte[4];
        byte[] movablePiecesValue = new byte[4];
        byte[] magazineIDValue = new byte[4];
        byte[] flags = new byte[4];

        // Store data in containers
        Array.Copy(packet, 0, triggerValue, 0, 1); //Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        Array.Copy(packet, 1, movablePiecesValue, 0, 1);
        Array.Copy(packet, 2, magazineIDValue, 0, 1);
        Array.Copy(packet, 3, flags, 0, 1);

        // Set current values
        triggerCurrentValue = BitConverter.ToInt32(triggerValue, 0);
        movablePiecesCurrentValue = BitConverter.ToInt32(movablePiecesValue, 0);// BitConverter start reading at a certain index till the end. 
        magazineID = BitConverter.ToInt32(magazineIDValue, 0);
        int flagInt = BitConverter.ToInt32(flags, 0);

        //BitArray flagBits = new BitArray(flags[0]); <= This is the way to work, but I'm not able to extract bit values from the BitArray
        magazinePresent = !(flagInt % 2 == 0) ; // Last bit in the flag byte shows the mag status. So, if there is no mag present the value of the flag is even.

        selectorSwitchPosition = (SwitchPosition) (flagInt / 2); // bit shift right
    }
}
