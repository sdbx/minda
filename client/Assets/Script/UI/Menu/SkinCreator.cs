using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using Steamworks;

namespace UI.Menu
{
    public class SkinCreator : MonoBehaviour
    {
        [SerializeField]
        SkinPreview skinPreview;
        [SerializeField]
        private InputField skinName;
        [SerializeField]
        private Button blackLoadButton;
        [SerializeField]
        private Button whiteLoadButton;
        [SerializeField]
        private Button createButton;
        [SerializeField]
        private Button CancelButton;

        [SerializeField]
        private RawImage targetImage;
        [SerializeField]
        private Texture checkIcon;
        [SerializeField]
        private Texture refreshIcon;

        private bool isUploading;
        private Tweener tween;
        [SerializeField]
        private SkinSelector skinSelector;
        [SerializeField]
        private ObjectToggler SkinSelectorToggler;
        [SerializeField]
        private ObjectToggler SkinCreatorToggler;

        private byte[] blackSkin;
        private byte[] whiteSkin;

        protected Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationResponse;

        private void Awake()
        {
            blackLoadButton.onClick.AddListener(OnBlackLoadButtonClicked);
            whiteLoadButton.onClick.AddListener(OnWhiteLoadButtonClicked);
            createButton.onClick.AddListener(OnCreateButtonClicked);
            CancelButton.onClick.AddListener(OnCancelButtonClicked);
            m_MicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
        }

        void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
        {
            LobbyServer.instance.Put<EmptyResult>("/skins/buy/" + pCallback.m_ulOrderID + "/","", (empty, err) =>
                {
                    if (err != null)
                    {
                        return;
                    }
                    ToastManager.instance.Add(LanguageManager.GetText("purchasesuccess"), "Success");
                    Upload();
                });
        }

        private void OnCancelButtonClicked()
        {
            MessageBox.instance.Show(LanguageManager.GetText("cancelmessage"),(agree)=>{
                if (agree)
                {
                    skinSelector.Start();
                    SkinSelectorToggler.Activate();
                    SkinCreatorToggler.UnActivate();
                    skinPreview.SelectingMode();
                }
            },"Yes","No");
        }

        private void Upload()
        {
            if (isUploading)
                return;
            var formData = new WWWForm();
            formData.AddField("name", skinName.text);
            if (blackSkin == null || whiteSkin == null)
            {
                ToastManager.instance.Add(LanguageManager.GetText("imagenotuploaded"), "Warning");
                return;
            }
            if (skinName.text == "")
            {
                ToastManager.instance.Add(LanguageManager.GetText("skinnameisempty"), "Warning");
                return;
            }
            CancelButton.gameObject.SetActive(false);
            formData.AddBinaryData("white", whiteSkin);
            formData.AddBinaryData("black", blackSkin);

            //startUpload

            //animation
            createButton.interactable = false;
            tween = targetImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.InOutCubic);
            isUploading = true;

            targetImage.texture = refreshIcon;

            LobbyServer.instance.Post("/skins/me/two/", formData, (data, err) =>
            {
                isUploading = false;
                createButton.interactable = true;
                tween.Kill();
                SkinSelectorToggler.Activate();
                SkinCreatorToggler.UnActivate();
                skinPreview.SelectingMode();
                targetImage.texture = checkIcon;
                targetImage.transform.rotation = Quaternion.Euler(Vector3.zero);
                if (err != null)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("skinuploadingerror"), "Error");
                    return;
                }
                ToastManager.instance.Add("Skin Created", "Success");
                CancelButton.gameObject.SetActive(true);
                skinSelector.LoadMySkinsAndEquipIndex(1);
            });
        }

        private void OnCreateButtonClicked()
        {
            if (blackSkin == null || whiteSkin == null)
            {
                ToastManager.instance.Add(LanguageManager.GetText("imagenotuploaded"), "Warning");
                return;
            }
            if (skinName.text == "")
            {
                ToastManager.instance.Add(LanguageManager.GetText("skinsnameisempty"), "Warning");
                return;
            }
            LobbyServer.instance.RefreshLoginUser((me=>{
                if(me.inventory.two_color_skin <= 0)
                {
                    LobbyServer.instance.SendBuyRequest(()=>{});
                    return;
                }
                Upload();
            }));
        }

        private void OnBlackLoadButtonClicked()
        {
            if(!isUploading)
                loadSkinImage(BallType.Black);
        }

        private void OnWhiteLoadButtonClicked()
        {
            if(!isUploading)
                loadSkinImage(BallType.White);
        }

        private void loadSkinImage(BallType ballType)
        {
            var data = FileUtils.LoadImage($"{ballType} skin image");

            if (data == null)
            {
                return;
            }
            var formData = new WWWForm();
            formData.AddField("color", ballType.ToString().ToLower());
            formData.AddBinaryData("file", data);
            LobbyServer.instance.Post("/skins/preview/", formData, (previewImageData, err) =>
            {
                if(!SkinCreatorToggler.isActivated)
                    return;
                if (err != null)
                {
                    ToastManager.instance.Add(LanguageManager.GetText("skinuploadingerror"), "Error");
                    return;
                }


                var texture = new Texture2D(1, 1);
                texture.LoadImage(previewImageData);

                if (ballType == BallType.Black)
                {
                    skinPreview.SetBlackTexture(texture);
                    blackSkin = data;
                }
                else
                {
                    skinPreview.SetWhiteTexture(texture);
                    whiteSkin = data;
                }

            });
        }
    }
}