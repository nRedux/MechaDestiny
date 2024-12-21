using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Unity.VisualScripting;
using UnityEngine.Localization.Settings;
using Newtonsoft.Json;
using System;

[Inspectable]
[System.Serializable]
public class LocalizedStringObject
{
    [Inspectable]
    public string Key = "";

    [SerializeField]
    public string Table = "";

    [SerializeField]
    public long Id = default;

    [JsonIgnore]
    [Inspectable]
    public string LocalizedValue
    {
        get
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString( Table , Id );
        }
    }

    [JsonConstructor]
    public LocalizedStringObject() { }

}
