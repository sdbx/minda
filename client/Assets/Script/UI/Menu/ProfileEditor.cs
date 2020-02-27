using System;
using System.Collections;
using Models;
using Network;
using SFB;
using UI;
using UI.Toast;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.Serialization;

public class ProfileEditor : MonoBehaviour
{
    [SerializeField]
    private RawImage profileImage;
    [SerializeField]
    private Text userNametext;
    [SerializeField]
    private InputField userNameInputField;
    [FormerlySerializedAs("EditStartBtn")] [SerializeField]
    private Button editStartBtn;
    //[SerializeField]
    //private Button profileImageRemoveBtn;
    [SerializeField]
    private Button profileImageChangeBtn;
    [SerializeField]
    private GameObject profileChangeIcon;
    [SerializeField]
    private Button saveChangesBtn;
    [SerializeField]
    private Button discardChagesBtn;

    private bool _isEditing;
    private byte[] _loadedImage;

    private bool _isProfileImageExist;

    private void Awake()
    {
        editStartBtn.onClick.AddListener(OnEditStartBtnClicked);
        //profileImageRemoveBtn.onClick.AddListener(OnProfileImageRemoveBtnClicked);
        profileImageChangeBtn.onClick.AddListener(OnProfileImageChangeBtnClicked);
        saveChangesBtn.onClick.AddListener(OnSaveChangesBtnClicked);
        discardChagesBtn.onClick.AddListener(OnDiscardChagesBtnClicked);
        SetDisplaymentState(false);
        Display();
    }

    private void OnEditStartBtnClicked()
    {
        if (!_isEditing)
        {
            _loadedImage = null;
            StartEdit();
        }
    }

    private void OnProfileImageChangeBtnClicked()
    {
        OpenFileDirectory();
    }

    private void OnProfileImageRemoveBtnClicked()
    {
        profileImage.texture = UiSettings.Instance.placeHolder;
        //profileImageRemoveBtn.gameObject.SetActive(false);
    }

    private void OnSaveChangesBtnClicked()
    {
        var newUser = new User();
        newUser.Username = userNameInputField.text;
        LobbyServer.Instance.Put("/users/me/", JsonConvert.SerializeObject(newUser), (EmptyResult nothing, int? err2) =>
        {
            if (err2 != null)
            {
                //에러처리
                ToastManager.Instance.Add(LanguageManager.GetText("profileuploadingerror"), "Error");
                return;
            }

            if (_loadedImage == null)
            {
                EndEdit();
                return;
            }

            var formData = new WWWForm();
            formData.AddBinaryData("file", _loadedImage);
            LobbyServer.Instance.Put("/users/me/picture/", formData, (data, err) =>
            {
                if (err != null)
                {
                    //에러처리
                    ToastManager.Instance.Add(LanguageManager.GetText("profileuploadingerror"), "Error");
                    return;
                }
                EndEdit();
            });
        });
    }


    private void EndEdit()
    {
        ToastManager.Instance.Add(LanguageManager.GetText("profilechanged"), "Success");
        LobbyServer.Instance.RefreshLoginUser((User user) =>
        {
            LobbyServer.Instance.RefreshLoginUserProfileImage((Texture texture) =>
            {
                userNametext.text = user.Username;
                profileImage.texture = texture;
            });
        });

        SetDisplaymentState(false);
        userNametext.text = userNameInputField.text;
    }

    private void OnDiscardChagesBtnClicked()
    {
        CancelEdit();
    }

    private void StartEdit()
    {
        SetDisplaymentState(true);
        userNameInputField.gameObject.SetActive(true);
        userNameInputField.text = userNametext.text;
    }

    private void CancelEdit()
    {
        SetDisplaymentState(false);
        Display();
    }

    private void Display()
    {
        LobbyServer.Instance.GetLoginUserProfileImage((Texture texture) =>
        {
            if (texture != null)
                profileImage.texture = texture;
        });
        userNametext.text = LobbyServer.Instance.loginUser.Username;
    }

    private void SetDisplaymentState(bool isEdit)
    {
        editStartBtn.gameObject.SetActive(!isEdit);
        profileChangeIcon.SetActive(isEdit);
        userNameInputField.gameObject.SetActive(isEdit);
        profileImageChangeBtn.enabled = isEdit;
        userNametext.gameObject.SetActive(!isEdit);
        // if(isEdit)
        //     profileImageRemoveBtn.gameObject.SetActive(isEdit);
        // else
        //     profileImageRemoveBtn.gameObject.SetActive(isEdit);
        // saveChangesBtn.gameObject.SetActive(isEdit);
        discardChagesBtn.gameObject.SetActive(isEdit);
        saveChangesBtn.gameObject.SetActive(isEdit);
    }

    public void OpenFileDirectory()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            new ExtensionFilter("All Files", "*" ),
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Map", "", extensions, false);

        if (paths.Length > 0)
        {
            try
            {
                profileImage.texture = Output(paths[0]);
                //profileImageRemoveBtn.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                ToastManager.Instance.Add(LanguageManager.GetText("imageloaderror"), "Error");
                Debug.Log("이미지 로드 오류 : " + e);
            }
        }
    }

    public Texture2D Output(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            _loadedImage = fileData;
        }
        return tex;
    }
}
