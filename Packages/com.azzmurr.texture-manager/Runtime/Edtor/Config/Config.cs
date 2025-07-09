using UnityEngine;

namespace Azzmurr.Utils
{
    public abstract class Config
    {
        public const int MaterialNameWidth = 150;
        public const int TextureWidth = 450;
        public const int SizeWidth = 70;
        public const int PCWidth = 55;
        public const int AndroidWidth = 65;
        public const int FormatWidth = 150;
        public const int ActionsWidth = 175;

        public static GUIStyle Label => new(GUI.skin.GetStyle("Label"));

        public static GUIStyle ValidLabel =>
            new(GUI.skin.GetStyle("Label"))
            {
                normal =
                {
                    textColor = Color.green
                }
            };

        public static GUIStyle InvalidLabel =>
            new(GUI.skin.GetStyle("Label"))
            {
                normal =
                {
                    textColor = Color.red
                }
            };

        public static GUIStyle ValidCenteredLabel =>
            new(GUI.skin.GetStyle("Label"))
            {
                normal =
                {
                    textColor = Color.green
                },
                alignment = TextAnchor.UpperCenter
            };

        public static GUIStyle InvalidCenteredLabel =>
            new(GUI.skin.GetStyle("Label"))
            {
                normal =
                {
                    textColor = Color.red
                },
                alignment = TextAnchor.UpperCenter
            };
    }
}