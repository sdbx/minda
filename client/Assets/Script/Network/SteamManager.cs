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

public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    public static bool IsSteamVersion = true;
#endif
#if DISABLESTEAMWORKS
    public static bool isSteamVersion = false;
#endif
    public uint steamId;
    public static SteamManager Instance;
    private bool _initialized = false;

    private CSteamID _mLobby;
    private string _currentGameRoomId;
    private SteamAPIWarningMessageHook_t _mSteamApiWarningMessageHook;
    private static void SteamApiDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

    private CallResult<LobbyCreated_t> _onLobbyCreatedCallResult;
    private CallResult<LobbyEnter_t> _onLobbyEnterCallResult;
    private CallResult<LobbyInvite_t> _onLobbyInviteCallResult;

    private void OnLobbyCreated(LobbyCreated_t pCallback, bool bIoFailure)
    {
        _mLobby = (CSteamID)pCallback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(_mLobby, "roomid", _currentGameRoomId);
        Debug.Log(SteamMatchmaking.GetLobbyData(_mLobby, "roomid") + "과" + _mLobby + "를 얻엇다");
        Debug.Log("[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t pCallback, bool bIoFailure)
    {
        _mLobby = new CSteamID(pCallback.m_ulSteamIDLobby);
        Debug.Log("로비 아이디 " + _mLobby + "를 얻엇다");
        Debug.Log("로비엔터 code: " + SteamMatchmaking.GetLobbyData(_mLobby, "roomid"));
        LobbyServer.Instance.JoinInvitedRoom(SteamMatchmaking.GetLobbyData(_mLobby, "roomid"));

    }

    private void OnLobbyInvite(LobbyInvite_t pCallback, bool bIoFailure)
    {
        JoinLobby(new CSteamID(pCallback.m_ulSteamIDLobby));
    }

    public void ActivateInvite(string roomId)
    {
        if (!_initialized) { return; }
        _currentGameRoomId = roomId;
        var handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 53);
        _onLobbyCreatedCallResult.Set(handle);
    }

    public void UnActivateInvite()
    {
        if (!_initialized) { return; }
        SteamMatchmaking.LeaveLobby(_mLobby);
        _mLobby = CSteamID.Nil;
    }

    private void JoinLobby(CSteamID lobbyId)
    {
        if (!_initialized) { return; }
        Debug.Log("조인로비");
        var handle = SteamMatchmaking.JoinLobby(lobbyId);
        _onLobbyEnterCallResult.Set(handle);
    }

    private void OnEnable()
    {
        if (!_initialized) { return; }

        _onLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
        _onLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
        _onLobbyInviteCallResult = CallResult<LobbyInvite_t>.Create(OnLobbyInvite);

        var args = System.Environment.GetCommandLineArgs();
        var input = "";
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "+connect_lobby" && args.Length > i + 1)
            {
                input = args[i + 1];
                Debug.Log(input + " 후에에엥");
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


        if (_mSteamApiWarningMessageHook == null)
        {
            // Set up our callback to receive warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
            _mSteamApiWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamApiDebugTextHook);
            SteamClient.SetWarningMessageHook(_mSteamApiWarningMessageHook);
        }
    }

    private void Awake()
    {

        //singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        var args = System.Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "token" && args.Length > i + 1)
            {
                if (args[i + 1] != "")
                {
                    return;
                }
            }
        }



        if (IsSteamVersion)
        {
            _initialized = SteamAPI.Init();
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
        Debug.Log("Steamworks.NET " + steamId + " initialized:" + _initialized);
    }

    private void Update()
    {
        if (!_initialized)
        {
            return;
        }

        SteamAPI.RunCallbacks();
    }

    public (string, HAuthTicket) GetAuthTicket()
    {
        if (!_initialized)
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
        if (!_initialized)
        {
            return;
        }
        SteamAPI.Shutdown();
    }

}
