#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.Collections;
using Steamworks;
using Network;
using UI.Toast;
using Utils;
using System;
using System.Collections.Generic;

class SteamManager : MonoBehaviour {
    #if !DISABLESTEAMWORKS
    public static bool isSteamVersion = true;
    #endif
    #if DISABLESTEAMWORKS
    public static bool isSteamVersion = false;
    #endif
    public uint steamId;
    public static SteamManager instance;
    private bool initialized = false;

    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

    private void OnEnable() {
		if (!initialized) { return; }

		if (m_SteamAPIWarningMessageHook == null) {
			// Set up our callback to receive warning messages from Steam.
			// You must launch with "-debug_steamapi" in the launch args to receive warnings.
			m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
		}
	}

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
            initialized = SteamAPI.Init();
            try
            {
                if (SteamAPI.RestartAppIfNecessary((AppId_t)steamId))
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
        }
        Debug.Log("Steamworks.NET " + steamId + " initialized:" + initialized);
    }

    private void Update() {
        if (!initialized) {
            return;
        }

        SteamAPI.RunCallbacks();
    }

    public (string, HAuthTicket) GetAuthTicket()
    {
        if (!initialized)
        {
            return ("", default(HAuthTicket));
        }

        var authTicketBuffer = new byte[1024];
        uint authTicketSize;
        var hticket = SteamUser.GetAuthSessionTicket(authTicketBuffer, 1024, out authTicketSize);
        var authTicket = new byte[authTicketSize];
        Array.Copy(authTicketBuffer, authTicket, authTicketSize);
        return (StringUtils.ByteArrayToHexString(authTicket), hticket);
    }

    private void OnDestroy()
    {
        if (!initialized)
        {
            return;
        }
        SteamAPI.Shutdown();
    }

}