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

public class ProfileEditor : MonoBehaviour
{
    [SerializeField]
    private RawImage profileImage;
    [SerializeField]
    private Text userNametext;
    [SerializeField]
    private InputField userNameInputField;
    [SerializeField]
    private Button EditStartBtn;
    [SerializeField]
    private Button profileImageRemoveBtn;
    [SerializeField]
    private Button profileImageChangeBtn;
    [SerializeField]
    private GameObject profileChangeIcon;
    [SerializeField]
    private Button saveChangesBtn;
    [SerializeField]
    private Button discardChagesBtn;

    private bool isEditing;
    private byte[] loadedImage;


    private void Awake()
    {
        EditStartBtn.onClick.AddListener(OnEditStartBtnClicked);
        profileImageRemoveBtn.onClick.AddListener(OnProfileImageRemoveBtnClicked);
        profileImageChangeBtn.onClick.AddListener(OnProfileImageChangeBtnClicked);
        saveChangesBtn.onClick.AddListener(OnSaveChangesBtnClicked);
        discardChagesBtn.onClick.AddListener(OnDiscardChagesBtnClicked);
        SetDisplaymentState(false);
        display();
    }

    private void OnEditStartBtnClicked()
    {
        if (!isEditing)
        {
            loadedImage = null;
            StartEdit();
        }
    }

    private void OnProfileImageChangeBtnClicked()
    {
        OpenFileDirectory();
    }

    private void OnProfileImageRemoveBtnClicked()
    {
        profileImage.texture = UISettings.instance.placeHolder;
    }

    private void OnSaveChangesBtnClicked()
    {
        var newUser = new User();
        if (loadedImage != null)
        {
            LobbyServer.instance.UploadImage(loadedImage, (Pic pic, int? err) =>
             {
                 if (err != null)
                 {
                     //에러처리
                     ToastManager.instance.Add("Image uploading Error","Error");
                     return;
                 }
                 //newUser.picture = pic;
                 newUser.username = userNameInputField.text;

                 LobbyServer.instance.Put("/users/me/", JsonConvert.SerializeObject(newUser), (EmptyResult nothing, int? err2) =>
                 {
                     if (err2 != null)
                     {
                         //에러처리
                         ToastManager.instance.Add("Profile uploading Error", "Error");
                         return;
                     }
                     EndEdit();
                 });
             });
        }
        else
        {
            newUser.username = userNameInputField.text;

            LobbyServer.instance.Put("/users/me/", JsonConvert.SerializeObject(newUser), (EmptyResult nothing, int? err2) =>
            {
                if (err2 != null)
                {
                    //에러처리
                    ToastManager.instance.Add("Profile uploading Error", "Error");
                }
                EndEdit();
            });
        }
    }

    private void EndEdit()
    {
        LobbyServer.instance.RefreshLoginUser((User user) =>
        {
            LobbyServer.instance.RefreshLoginUserProfileImage((Texture texture) =>
            {
                userNametext.text = user.username;
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
        display();
    }

    private void display()
    {
        LobbyServer.instance.GetLoginUserProfileImage((Texture texture) =>
        {
            profileImage.texture = texture;
        });
        userNametext.text = LobbyServer.instance.loginUser.username;
    }

    private void SetDisplaymentState(bool isEdit)
    {
        EditStartBtn.gameObject.SetActive(!isEdit);
        profileChangeIcon.SetActive(isEdit);
        userNameInputField.gameObject.SetActive(isEdit);
        profileImageChangeBtn.gameObject.SetActive(isEdit);
        profileImageRemoveBtn.gameObject.SetActive(isEdit);
        saveChangesBtn.gameObject.SetActive(isEdit);
        discardChagesBtn.gameObject.SetActive(isEdit);
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
                profileImage.texture =  Output(paths[0]);
            }
            catch (Exception e)
            {
                ToastManager.instance.Add("Image load error", "Error");
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
            loadedImage = fileData;
        }
        return tex;
    }
}