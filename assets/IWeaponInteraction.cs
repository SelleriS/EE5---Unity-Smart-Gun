using UnityEngine;
using System;
using System.Net;

public interface IWeaponInteraction
{
    void PacketTranslater(byte[] packet);
}