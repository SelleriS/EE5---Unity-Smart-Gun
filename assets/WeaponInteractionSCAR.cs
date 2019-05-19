using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is attached to the SCAR prefab and is responsible to apply
 * the received changes to it.
 * Implemented features:
 * The trigger (SetTrigger from the parent class will be executed)
 * The selector switch 
 * The movable pieces/ cocking handle
 * The magazine (Check if a magazine is present)
 */

public class WeaponInteractionSCAR :WeaponInteraction
{ 
    private Transform movablePiecesTransform;
    private Transform selectorSwitchTransform;
    private GameObject magazineObject;
    private readonly float movablePiecesMaxXpos = 0.117f;

    // Start is called before the first frame update
    void Start()
    {
        triggerMinRot = 0;
        triggerMaxRot = 15;
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
        SetSelectorSwitch(selectorSwitchPosition);

        //Movable pieces
        SetMovablePieces(movablePiecesCurrentValue);

        //Trigger
        SetTrigger(triggerCurrentValue);

        //Magazine
        SetMagazine(magazinePresent);
    }

    // Set the position of the selector switch with the value received from the UDP packet
    private void SetSelectorSwitch(SwitchPosition switchPosition)
    {
        switch (switchPosition)
        {
            case SwitchPosition.SAFE:
                selectorSwitchTransform.localEulerAngles = new Vector3(0, 0, 30);
                break;

            case SwitchPosition.SEMI:
                selectorSwitchTransform.localEulerAngles = new Vector3(0, 0, 0);
                break;

            case SwitchPosition.AUTO:
                selectorSwitchTransform.localEulerAngles = new Vector3(0, 0, -30);
                break;
        }
    }

    // Set the position of the movable pieces with the value received from the UDP packet
    private void SetMovablePieces(int movablePiecesValue)
    {
        movablePiecesCurrentValue = Mathf.Clamp(movablePiecesValue, 0, 255);
        float piecesXPos = movablePiecesMaxXpos * ((float)movablePiecesCurrentValue / 255);
        movablePiecesTransform.localPosition = new Vector3(piecesXPos, 0, 0);
    }

    //if no magazine is present, disable the magazine object (-> disable redering)
    private void SetMagazine(bool present)
    {
        magazineObject.SetActive(present);
    }
}
