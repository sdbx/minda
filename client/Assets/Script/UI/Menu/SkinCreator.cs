using Models;
using Network;
using UI.Toast;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;

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
        private RawImage targetImage;
        [SerializeField]
        private Texture checkIcon;
        [SerializeField]
        private Texture refreshIcon;

        private bool isUploading;
        private Tweener tween;
        [SerializeField]
        private ObjectToggler SkinSelectorToggler;
        [SerializeField]
        private ObjectToggler SkinCreatorToggler;

        private byte[] blackSkin;
        private byte[] whiteSkin;

        private void Awake()
        {
            blackLoadButton.onClick.AddListener(OnBlackLoadButtonClicked);
            whiteLoadButton.onClick.AddListener(OnWhiteLoadButtonClicked);
            createButton.onClick.AddListener(OnCreateButtonClicked);
        }

        private void OnCreateButtonClicked()
        {
            if(isUploading)
                return;

            var formData = new WWWForm();
            formData.AddField("name", skinName.text);
            if(blackSkin==null||whiteSkin==null)
            {
                ToastManager.instance.Add("Please Upload Image", "Warning");
                return;
            }
            formData.AddBinaryData("white", whiteSkin);
            formData.AddBinaryData("black", blackSkin);
            //startUpload
            //animation
            createButton.interactable = false;
            tween = targetImage.transform.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.InOutCubic);
            isUploading = true;

            targetImage.texture = refreshIcon;
            targetImage.transform.rotation = Quaternion.Euler(Vector3.zero);

            LobbyServer.instance.Post("/skins/me/two/", formData, (data, err) =>
            {
                isUploading = false;
                createButton.interactable = true;
                tween.Kill();
                SkinSelectorToggler.Activate();
                SkinCreatorToggler.UnActivate();
                skinPreview.SelectingMode();
                if (err != null)
                {
                    ToastManager.instance.Add("Skin Uploading Error " + err, "Error");
                    return;
                }
                ToastManager.instance.Add("Skin Created", "Success");
            });
        }

        private void OnBlackLoadButtonClicked()
        {
            loadSkinImage(BallType.Black);
        }

        private void OnWhiteLoadButtonClicked()
        {
            loadSkinImage(BallType.White);
        }

        private void loadSkinImage(BallType ballType)
        {
            var data = FileUtils.LoadImage($"{ballType} skin image");

            if(data == null)
            {
                return;
            }
            var formData = new WWWForm();
            formData.AddField("color", ballType.ToString().ToLower());
            formData.AddBinaryData("file",data);
            LobbyServer.instance.Post("/skins/preview/",formData,(previewImageData,err)=>
            {
                if(err!=null)
                {
                    ToastManager.instance.Add("Skin Uploading Error "+err,"Error");
                    return;
                }
                

                var texture = new Texture2D(1,1);
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