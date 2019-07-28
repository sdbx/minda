#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.Collections;
using Steamworks;
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

    protected Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationResponse;

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
            initialized = SteamAPI.Init();
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

    public void OnEnable()
    {
        m_MicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
    }

    void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
    {
        Debug.Log("[" + MicroTxnAuthorizationResponse_t.k_iCallback + " - MicroTxnAuthorizationResponse] - " + pCallback.m_unAppID + " -- " + pCallback.m_ulOrderID + " -- " + pCallback.m_bAuthorized);
    }

}