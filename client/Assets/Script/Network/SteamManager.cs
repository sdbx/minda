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

public class SteamManager : MonoBehaviour {
    #if !DISABLESTEAMWORKS
    public static bool isSteamVersion = true;
    #endif
    #if DISABLESTEAMWORKS
    public static bool isSteamVersion = false;
    #endif
    public uint steamId;
    public static SteamManager instance;
    private bool initialized = false;

    private CSteamID m_Lobby;
    private string currentGameRoomID;
    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
	private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
		Debug.LogWarning(pchDebugText);
	}

    private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;
    private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
    private CallResult<LobbyInvite_t> OnLobbyInviteCallResult;

    void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
    {
        m_Lobby = (CSteamID)pCallback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(m_Lobby, "roomid", currentGameRoomID);
        Debug.Log(SteamMatchmaking.GetLobbyData(m_Lobby, "roomid")+"과"+m_Lobby+"를 얻엇다");
        Debug.Log("[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t pCallback, bool bIOFailure)
    {
        m_Lobby = new CSteamID(pCallback.m_ulSteamIDLobby);
        Debug.Log("로비 아이디 "+m_Lobby+"를 얻엇다");
        Debug.Log("로비엔터 code: "+SteamMatchmaking.GetLobbyData(m_Lobby, "roomid"));
        LobbyServer.instance.JoinInvitedRoom(SteamMatchmaking.GetLobbyData(m_Lobby, "roomid"));
 
    }

    private void OnLobbyInvite(LobbyInvite_t pCallback, bool bIOFailure)
    {
        JoinLobby(new CSteamID(pCallback.m_ulSteamIDLobby));
    }

    public void ActivateInvite(string roomID)
    {
        currentGameRoomID = roomID;
        SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 53);
        OnLobbyCreatedCallResult.Set(handle);
    }

    private void JoinLobby(CSteamID lobbyId)
    {
        Debug.Log("조인로비");
        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(lobbyId);
        OnLobbyEnterCallResult.Set(handle);
    }

    private void OnEnable()
    {
        if (!initialized) { return; }

        OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
        OnLobbyInviteCallResult = CallResult<LobbyInvite_t>.Create(OnLobbyInvite);

        string[] args = System.Environment.GetCommandLineArgs();
        string input = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "+connect_lobby" && args.Length > i + 1)
            {
                input = args[i + 1];
                Debug.Log(input+" 후에에엥");
            }
        }
 
        if (!string.IsNullOrEmpty(input))
        {
            // Invite accepted, launched game. Join friend's game
            ulong lobbyId = 0;
 
            if (ulong.TryParse(input, out lobbyId))
            {
                Debug.Log("조인로비 시작");
                JoinLobby(new CSteamID(lobbyId));
            }
 
        }


        if (m_SteamAPIWarningMessageHook == null)
        {
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