using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TO DO: Split update in mulitple methods
 * split class up in Parent and child classes
 * create interface for WeaponInteraction
 */
[System.Serializable]
public enum SwitchPosition
{
    SAFE,
    SEMI,
    AUTO
}

// types of weapons that can be recognized: max. 2 to the power of 6 different weapons
public enum WeaponType
{
    SCAR,
    FiveSeven,
    DEMO
}

public class WeaponInteraction : MonoBehaviour
{
    public int triggerCurrentValue;
    public int movablePiecesCurrentValue;
    public int magazineID;
    public bool magazinePresent;
    public SwitchPosition selectorSwitchPosition;
    public WeaponType weaponType;
    private static int weaponTypesBits; // number of bits needed to encode the number of different weapons to recognize

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
        int numberOfWeaponTypes = Enum.GetNames(typeof(WeaponType)).Length; // Number of enum elements in WeaponType (= number of different types of weapons)
        weaponTypesBits = (int) Math.Ceiling(Math.Log(numberOfWeaponTypes, 2)); //log2 of the different number of weapons. Log2 is always rounded up (=Ceiling)
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
        byte flagByte = flags[0];

        magazinePresent = !(flagByte % 2 == 0) ; // Last bit in the flag byte shows the mag status. So, if there is no mag present the value of the flag is even.

        //UNCOMMENT THESE LINES WHEN DEBUGGING: 
        //(Because weaponTypesBits becomes 0 due to the fact that the weapon gets destroyed every X seconds that it's inactive)
        //int numberOfWeaponTypes = Enum.GetNames(typeof(WeaponType)).Length; // Number of enum elements
        //weaponTypesBits = (int)Math.Ceiling(Math.Log(numberOfWeaponTypes, 2)); //log2 of the different number of weapons. Log2 is always rounded up (=Ceiling)

        byte weaponTypeByte = (byte)(flagByte/Math.Pow(2, 8-weaponTypesBits)); // bit shift right (the amount of bits that can be discarded = 3 + amount of bits that are not used)
        weaponType = (WeaponType) weaponTypeByte;

        byte switchPositionByte = (byte)(flagByte * Math.Pow(2, weaponTypesBits)); // bit shift left the enough of times to get rid of the used MSB's to encode the weapontype
        switchPositionByte /= (byte)Math.Pow(2, weaponTypesBits+1); // BSR enough times to set the bits needed to recognize the selector switches position as LSB's
        selectorSwitchPosition = (SwitchPosition) switchPositionByte;
    }

    public static WeaponType GetWeaponTypeOutOfData(byte[] packet)
    {
        byte[] flags = new byte[4];
        Array.Copy(packet, 3, flags, 0, 1);
        byte flagByte = flags[0];
        int numberOfWeaponTypes = Enum.GetNames(typeof(WeaponType)).Length; // Number of enum elements
        weaponTypesBits = (int)Math.Ceiling(Math.Log(numberOfWeaponTypes, 2)); //log2 of the different number of weapons. Log2 is always rounded up (=Ceiling)
        byte weaponTypeByte = (byte)(flagByte / Math.Pow(2, 8 - weaponTypesBits)); // bit shift right (the amount of bits that can be discarded = 3 + amount of bits that are not used)

        return (WeaponType)weaponTypeByte;
    }
}
