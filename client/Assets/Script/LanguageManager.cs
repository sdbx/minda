
using System;
using System.Collections.Generic;
using System.IO;
using UI.Toast;
using UnityEngine;
using Utils;

public class LanguageManager : MonoBehaviour
{
    private static Dictionary<string, string> _loadedLaunguagePack;
    [SerializeField]
    private static bool _isLoaded = false;
    private void Awake()
    {
        var language = "en-US";
        if (File.Exists(Path.Combine(Application.persistentDataPath, "language.txt")))
        {
            language = File.ReadAllText(Path.Combine(Application.persistentDataPath, "language.txt"));
        }
        else
        {
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "language.txt"), language);
        }
        if (language == "en-US")
        {
            var tempAsset = Resources.Load("en-US") as TextAsset;
            File.WriteAllText(Path.Combine(Application.persistentDataPath, language), tempAsset.text);
        }
        LoadPack(language);
    }

    public void LoadPack(string language)
    {
        var path = Path.Combine(Application.persistentDataPath, language);
        if (!File.Exists(path))
        {
            ToastManager.Instance.Add("Launage File doesn't exist", "Error");
            return;
        }

        try
        {
            _loadedLaunguagePack = new Dictionary<string, string>();
            foreach (var text in File.ReadLines(path))
            {
                if (text == "")
                    continue;
                var splitedText = new List<string>(text.Split(':'));
                var key = splitedText[0];
                splitedText.RemoveAt(0);
                var value = String.Join(":", splitedText).Replace("/n", "\n");
                _loadedLaunguagePack.Add(key.ToLower(), value);
            }
            _isLoaded = true;
        }
        catch (Exception e)
        {
            Debug.Log("Language Pack Loading Error " + e);
        }
    }

    public static string GetText(string key)
    {
        key = key.ToLower();
        if (!_isLoaded || !_loadedLaunguagePack.ContainsKey(key))
            return null;

        return _loadedLaunguagePack[key];
    }

    public static string GetText(string key, params string[] args)
    {
        var text = GetText(key);
        for (var i = 0; i < args.Length; i++)
        {
            text = text.Replace("{" + i + "}", args[i]);
        }
        return text;
    }
}
