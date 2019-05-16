using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TO DO: Split update in mulitple methods
 * split class up in Parent and child classes
 * create interface for WeaponInteraction
 */

public class WeaponInteraction : MonoBehaviour, IWeaponInteraction
{
    public int triggerCurrentValue;
    public WeaponType weaponType;

    protected Transform triggerTransform;

    protected float triggerMinRot;
    protected float triggerMaxRot;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        SetTrigger(triggerCurrentValue);
    }

    // Set the position of the trigger with the value received from the UDP packet
    protected virtual void SetTrigger(int triggerValue)
    {
        triggerCurrentValue = Mathf.Clamp(triggerValue, 0, 255);
        float triggerZRot = triggerMinRot + (triggerMaxRot - triggerMinRot) * ((float)triggerValue / 255); // Maps the current value of the trigger given by the client (value between 0 and 255) between triggerMinRot and triggerMaxRot
        triggerTransform.localEulerAngles = new Vector3(triggerTransform.localEulerAngles.x, triggerTransform.localEulerAngles.y, triggerZRot);
    }

    // Translate the data and edit the parameters of the virtual weapon
    public virtual void PacketTranslater(byte[] packet) // if reading problems occur, this can be due to the endian (the sequence in which the bits are send, LSB first or last?)
    {
        // Create storage containers
        byte[] triggerValue = new byte[4];

        // Store data in containers
        Array.Copy(packet, 0, triggerValue, 0, 1); //Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)

        // Set current values
        triggerCurrentValue = BitConverter.ToInt32(triggerValue, 0);

        weaponType = GetWeaponTypeOutOfData(packet);
    }

    public static WeaponType GetWeaponTypeOutOfData(byte[] packet)
    {
        byte[] flags = new byte[4];
        Array.Copy(packet, 3, flags, 0, 1);
        byte flagByte = flags[0];
        int numberOfWeaponTypes = Enum.GetNames(typeof(WeaponType)).Length; // Number of enum elements
        int weaponTypesBits = (int)Math.Ceiling(Math.Log(numberOfWeaponTypes, 2)); //log2 of the different number of weapons. Log2 is always rounded up (=Ceiling)
        byte weaponTypeByte = (byte)(flagByte / Math.Pow(2, 8 - weaponTypesBits)); // bit shift right (the amount of bits that can be discarded = 3 + amount of bits that are not used)

        return (WeaponType)weaponTypeByte;
    }

}
