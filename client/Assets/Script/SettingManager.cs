
using System;
using System.Collections.Generic;
using System.IO;
using UI.Toast;
using UnityEngine;
using Utils;

public class SettingManager : MonoBehaviour
{
    private static Dictionary<string, string> _loadedSettings;
    [SerializeField]
    private static bool _isLoaded = false;
    private void Awake()
    {
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "setting.txt")))
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "setting.txt"), "ServerAddress:https://api.minda.games\nToken:null");
        }
        LoadSettings();
    }

    public void LoadSettings()
    {
        //버전 체크 필요
        var path = Path.Combine(Application.persistentDataPath, "setting.txt");

        try
        {
            _loadedSettings = new Dictionary<string, string>();
            foreach (var text in File.ReadLines(path))
            {
                if (text == "")
                    continue;
                var splitedText = new List<string>(text.Split(':'));
                var key = splitedText[0];
                splitedText.RemoveAt(0);
                var value = String.Join(":", splitedText).Replace("/n", "\n");
                _loadedSettings.Add(key.ToLower(), value);
            }
            _isLoaded = true;
        }
        catch (Exception)
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "setting.txt"), "ServerAddress:https://api.minda.games");
        }
    }

    public static string GetSetting(string key)
    {
        key = key.ToLower();
        if (!_isLoaded || !_loadedSettings.ContainsKey(key))
            return null;

        return _loadedSettings[key];
    }
}
