using System;
using Game;
using Network;
using UnityEngine;

namespace Models
{
    public class Inventory
    {
        public int OneColorSkin;
        public int TwoColorSkin;
        public int? CurrentSkin;
    }

    public class Skin
    {
        public int Id;
        public string Name;
        public string BlackPicture;
        public string WhitePicture;
    }

    public struct CurrentSkin
    {
        public int? Id;
        public CurrentSkin(int? id)
        {
            this.Id = id;
        }
    }

    public class LoadedSkin
    {
        static public void Get(Skin skin, Action<LoadedSkin> callback)
        {
            var downloadedSkin = new LoadedSkin(skin, null, null);

            if (skin.BlackPicture != null)
            {
                LobbyServerApi.DownloadImage(skin.BlackPicture, (Texture texture) =>
                {
                    downloadedSkin.BlackTexture = texture;

                    if (downloadedSkin.WhiteTexture != null)
                    {
                        callback(downloadedSkin);
                    }
                });
            }

            if (skin.WhitePicture != null)
            {
                LobbyServerApi.DownloadImage(skin.WhitePicture, (Texture texture) =>
                {
                    downloadedSkin.WhiteTexture = texture;
                    if (downloadedSkin.BlackTexture != null)
                    {
                        callback(downloadedSkin);
                    }
                });
            }
        }

        public LoadedSkin(Skin skin, Texture black, Texture white)
        {
            Id = skin.Id;
            Name = skin.Name;
            BlackTexture = black;
            WhiteTexture = white;
        }

        public int Id;
        public string Name;
        public Texture BlackTexture;
        public Texture WhiteTexture;
    }
}
