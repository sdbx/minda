using System.IO;
using SFB;
using UnityEngine;

namespace Utils
{
    static public class FileUtils
    {
        public static string[] Browse(string title, ExtensionFilter[] filters, bool multiSelect = false, string directory = "")
        {
            var paths = StandaloneFileBrowser.OpenFilePanel(title, directory, filters, multiSelect);
            if (paths.Length > 0)
            {
                return paths;
            }
            else
            {
                return null;
            }
        }

        public static byte[] LoadImage(string title, string directory = "")
        {
            var filters = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
                new ExtensionFilter("All Files", "*" ),
            };

            var paths = Browse(title, filters);
            if (paths != null)
            {
                return File.ReadAllBytes(paths[0]);
            }
            return null;
        }

        public static string LoadText(string title, string directory = "")
        {
            var filters = new[] {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
                new ExtensionFilter("All Files", "*" ),
            };

            var paths = Browse(title, filters);
            if (paths != null)
            {
                return File.ReadAllText(paths[0]);
            }
            return null;
        }
    }
}
