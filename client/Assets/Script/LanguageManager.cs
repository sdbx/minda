
using System;
using System.Collections.Generic;
using System.IO;
using UI.Toast;
using UnityEngine;
using Utils;

public class LanguageManager : MonoBehaviour
{
    private static Dictionary<string,string> loadedLaunguagePack;
    private static bool isLoaded = false;
    private void Awake()
    {
        string language = "en-US";
        if(File.Exists("language.txt"))
        {
            language = File.ReadAllText("language.txt");
        }
        LoadPack(language);
    }

    public void LoadPack(string language)
    {
        if (!File.Exists(language))
        {
            ToastManager.instance.Add("Launage File doesn't exist", "Error");
            return;
        }

        try
        {
            loadedLaunguagePack = new Dictionary<string, string>();
            foreach (var text in File.ReadLines(language))
            {
                if(text=="")
                    continue;
                List<string> splitedText = new List<string>(text.Split(':'));
                var key = splitedText[0];
                splitedText.RemoveAt(0);
                var value = String.Join(":", splitedText).Replace("/n","\n");
                loadedLaunguagePack.Add(key.ToLower(), value);
            }
            isLoaded =  true;
        }
        catch (Exception e)
        {
            Debug.Log("Language Pack Loading Error " + e);
        }
    }

    public static string GetText(string key)
    {
        key = key.ToLower();
        if(!isLoaded||!loadedLaunguagePack.ContainsKey(key))
            return null;
        
        return loadedLaunguagePack[key];
    }
    
    public static string GetText(string key, params string[] args)
    {
        var text = GetText(key);
        for (int i = 0; i < args.Length; i++)
        {
            text = text.Replace("{" + i + "}", args[i]);
        }
        return text;
    }
}
