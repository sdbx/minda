using System;
using Game;
using Network;
using UnityEngine;

namespace Models
{
    public class Inventory
    {
        public int one_color_skin;
        public int two_color_skin;
        public int? current_skin;
    }

    public class Skin
    {
        public int id;
        public string name;
        public string black_picture;
        public string white_picture;
    }

    public struct CurrentSkin
    {
        public int? id;
        public CurrentSkin(int? id)
        {
            this.id = id;
        }
    }

    public class LoadedSkin
    {
        static public void Get(Skin skin, Action<LoadedSkin> callback)
        {
            LoadedSkin downloadedSkin = new LoadedSkin(skin, null, null);
            
            if (skin.black_picture != null)
            {
                LobbyServerAPI.DownloadImage(skin.black_picture, (Texture texture) =>
                {
                    downloadedSkin.blackTexture = texture;

                    if (downloadedSkin.whiteTexture != null)
                    {
                        callback(downloadedSkin);
                    }
                });
            }

            if (skin.white_picture != null)
            {
                LobbyServerAPI.DownloadImage(skin.white_picture, (Texture texture) =>
                {
                    downloadedSkin.whiteTexture = texture;
                    if (downloadedSkin.blackTexture != null)
                    {
                        callback(downloadedSkin);
                    }
                });
            }
        }

        public LoadedSkin(Skin skin, Texture black, Texture white)
        {
            id = skin.id;
            name = skin.name;
            blackTexture = black;
            whiteTexture = white;
        }

        public int id;
        public string name;
        public Texture blackTexture;
        public Texture whiteTexture;
    }
}