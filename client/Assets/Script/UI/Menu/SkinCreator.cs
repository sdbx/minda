using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using Steamworks;
using UnityEngine.Serialization;

namespace UI.Menu
{
    public class SkinCreator : MonoBehaviour
    {
        [SerializeField] private SkinPreview skinPreview;
        [SerializeField]
        private InputField skinName;
        [SerializeField]
        private Button blackLoadButton;
        [SerializeField]
        private Button whiteLoadButton;
        [SerializeField]
        private Button createButton;
        [FormerlySerializedAs("CancelButton")] [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private RawImage targetImage;
        [SerializeField]
        private Texture checkIcon;
        [SerializeField]
        private Texture refreshIcon;

        private bool _isUploading;
        private Tweener _tween;
        [SerializeField]
        private SkinSelector skinSelector;
        [FormerlySerializedAs("SkinSelectorToggler")] [SerializeField]
        private ObjectToggler skinSelectorToggler;
        [FormerlySerializedAs("SkinCreatorToggler")] [SerializeField]
        private ObjectToggler skinCreatorToggler;

        private byte[] _blackSkin;
        private byte[] _whiteSkin;

        protected Callback<MicroTxnAuthorizationResponse_t> MMicroTxnAuthorizationResponse;

        private void Awake()
        {
            blackLoadButton.onClick.AddListener(OnBlackLoadButtonClicked);
            whiteLoadButton.onClick.AddListener(OnWhiteLoadButtonClicked);
            createButton.onClick.AddListener(OnCreateButtonClicked);
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            MMicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
        }

        private void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
        {
            LobbyServer.Instance.Put<EmptyResult>("/skins/buy/" + pCallback.m_ulOrderID + "/", "", (empty, err) =>
                 {
                     if (err != null)
                     {
                         return;
                     }
                     ToastManager.Instance.Add(LanguageManager.GetText("purchasesuccess"), "Success");
                     Upload();
                 });
        }

        private void OnCancelButtonClicked()
        {
            MessageBox.Instance.Show(LanguageManager.GetText("cancelmessage"), (agree) =>
            {
                if (agree)
                {
                    skinSelector.Start();
                    skinSelectorToggler.Activate();
                    skinCreatorToggler.UnActivate();
                    skinPreview.SelectingMode();
                }
            }, "Yes", "No");
        }

        private void Upload()
        {
            if (_isUploading)
                return;
            var formData = new WWWForm();
            formData.AddField("name", skinName.text);
            if (_blackSkin == null || _whiteSkin == null)
            {
                ToastManager.Instance.Add(LanguageManager.GetText("imagenotuploaded"), "Warning");
                return;
            }
            if (skinName.text == "")
            {
                ToastManager.Instance.Add(LanguageManager.GetText("skinnameisempty"), "Warning");
                return;
            }
            cancelButton.gameObject.SetActive(false);
            formData.AddBinaryData("white", _whiteSkin);
            formData.AddBinaryData("black", _blackSkin);

            //startUpload

            //animation
            createButton.interactable = false;
            _tween = targetImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.InOutCubic);
            _isUploading = true;

            targetImage.texture = refreshIcon;

            LobbyServer.Instance.Post("/skins/me/two/", formData, (data, err) =>
            {
                _isUploading = false;
                createButton.interactable = true;
                _tween.Kill();
                skinSelectorToggler.Activate();
                skinCreatorToggler.UnActivate();
                skinPreview.SelectingMode();
                targetImage.texture = checkIcon;
                targetImage.transform.rotation = Quaternion.Euler(Vector3.zero);
                if (err != null)
                {
                    ToastManager.Instance.Add(LanguageManager.GetText("skinuploadingerror"), "Error");
                    return;
                }
                ToastManager.Instance.Add("Skin Created", "Success");
                cancelButton.gameObject.SetActive(true);
                skinSelector.LoadMySkinsAndEquipIndex(1);
            });
        }

        private void OnCreateButtonClicked()
        {
            if (_blackSkin == null || _whiteSkin == null)
            {
                ToastManager.Instance.Add(LanguageManager.GetText("imagenotuploaded"), "Warning");
                return;
            }
            if (skinName.text == "")
            {
                ToastManager.Instance.Add(LanguageManager.GetText("skinsnameisempty"), "Warning");
                return;
            }
            LobbyServer.Instance.RefreshLoginUser((me =>
            {
                if (me.Inventory.TwoColorSkin <= 0)
                {
                    LobbyServer.Instance.SendBuyRequest(() => { });
                    return;
                }
                Upload();
            }));
        }

        private void OnBlackLoadButtonClicked()
        {
            if (!_isUploading)
                LoadSkinImage(BallType.Black);
        }

        private void OnWhiteLoadButtonClicked()
        {
            if (!_isUploading)
                LoadSkinImage(BallType.White);
        }

        private void LoadSkinImage(BallType ballType)
        {
            var data = FileUtils.LoadImage($"{ballType} skin image");

            if (data == null)
            {
                return;
            }
            var formData = new WWWForm();
            formData.AddField("color", ballType.ToString().ToLower());
            formData.AddBinaryData("file", data);
            LobbyServer.Instance.Post("/skins/preview/", formData, (previewImageData, err) =>
            {
                if (!skinCreatorToggler.isActivated)
                    return;
                if (err != null)
                {
                    ToastManager.Instance.Add(LanguageManager.GetText("skinuploadingerror"), "Error");
                    return;
                }


                var texture = new Texture2D(1, 1);
                texture.LoadImage(previewImageData);

                if (ballType == BallType.Black)
                {
                    skinPreview.SetBlackTexture(texture);
                    _blackSkin = data;
                }
                else
                {
                    skinPreview.SetWhiteTexture(texture);
                    _whiteSkin = data;
                }

            });
        }
    }
}
