using UnityEngine;
using UnityEngine.Localization;

public enum WeaponType
{
    Melee = 0x0010,
    MachineGun = 0x0020,
    Shotgun = 0x0030,
    Rifle = 0x0040,
    GrenadeLauncher = 0x0050
}

[System.Serializable]
public class WeaponTypeInfo
{
    public WeaponType Type = WeaponType.Melee;
    public Sprite WeaponTypeSprite;
    public LocalizedString Name;
    public LocalizedString Description;
}
