#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.Collections;
using Steamworks;
using Utils;
using System;


class SteamManager : MonoBehaviour {
    #if !DISABLESTEAMWORKS
    public static bool isSteamVersion = true;
    #endif
    #if DISABLESTEAMWORKS
    public static bool isSteamVersion = false;
    #endif
    public static SteamManager instance;
    private bool initialized = false;

    private void Awake()
    {
        //singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        if (isSteamVersion)
        {
            try
            {
                if (SteamAPI.RestartAppIfNecessary((AppId_t)1025230))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. \n" + e, this);

                Application.Quit();
                return;
            }
            initialized = SteamAPI.Init();
        }
    }

    private void Update() {
        if (!initialized) {
            return;
        }

        SteamAPI.RunCallbacks();
    }

    public string GetAuthTicket()
    {
        if (!initialized)
        {
            return "";
        }

        var authTicketBuffer = new byte[1024];
        uint authTicketSize;
        SteamUser.GetAuthSessionTicket(authTicketBuffer, 1024, out authTicketSize);
        var authTicket = new byte[authTicketSize];
        Array.Copy(authTicketBuffer, authTicket, authTicketSize);
        return StringUtils.ByteArrayToHexString(authTicket);
    }
}