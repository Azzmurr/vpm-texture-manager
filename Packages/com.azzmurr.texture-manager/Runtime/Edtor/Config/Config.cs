using UnityEngine;

namespace Azzmurr.Utils
{
    public class Config
    {
        static public string DEFAULT_MAT_NAME = "Used in mat swap";
        static public int ROW_HEIGHT = 20;
        static public int MATERIAL_BUTTON_WTDTH = 25;
        static public int MATERIAL_NAME_WIDTH = 150;
        static public int TEXTURE_WIDTH = 450;
        static public int SIZE_WIDTH = 70;
        static public int PC_WIDTH = 55;
        static public int ANDROID_WIDTH = 65;
        static public int FORMAT_WIDTH = 150;
        static public int ACTIONS_WIDTH = 175;

        static public GUIStyle Label
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                return style;
            }
        }

        static public GUIStyle ValidLabel
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                style.normal.textColor = Color.green;
                return style;
            }
        }

        static public GUIStyle InvalidLabel
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                style.normal.textColor = Color.red;
                return style;
            }
        }

        static public GUIStyle ValidCenteredLabel
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                style.normal.textColor = Color.green;
                style.alignment = TextAnchor.UpperCenter;
                return style;
            }
        }

        static public GUIStyle InvalidCenteredLabel
        {
            get
            {
                var style = new GUIStyle(GUI.skin.GetStyle("Label"));
                style.normal.textColor = Color.red;
                style.alignment = TextAnchor.UpperCenter;
                return style;
            }
        }
    }
}