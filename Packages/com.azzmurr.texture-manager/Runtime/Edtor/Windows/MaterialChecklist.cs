using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Azzmurr.Utils
{
    internal class MaterialChecklist : EditorWindow
    {
        [MenuItem("Tools/Azzmurr/Material Checklist")]
        public static void Init()
        {
            var window = (MaterialChecklist) GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Show();
        }

        [MenuItem("Assets/Azzmurr/Material Checklist", true, 0)]
        public static bool CanShowFromSelection() => Selection.activeObject is Material;

        [MenuItem("Assets/Azzmurr/Material Checklist", false, 0)]
        public static void ShowFromSelection()
        {
            var window = (MaterialChecklist) GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Material = new MaterialMeta((Material)Selection.activeObject);
            window.Show();
        }

        public static void Init(Material activeGameObject)
        {
            var window = (MaterialChecklist) GetWindow(typeof(MaterialChecklist));
            window.titleContent = new GUIContent("Material Checklist");
            window.Material = new MaterialMeta(activeGameObject);
            window.Show();
        }

        public MaterialMeta Material;
        private Vector2 _mainScrollPosition;

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var gameObject = (Material)EditorGUILayout.ObjectField(GUIContent.none, Material?.Material, typeof(Material), false, GUILayout.Height(30));

                if (Material == null || Material.Material != gameObject)
                {
                    Material = gameObject ? new MaterialMeta(gameObject) : null;
                }
            }

            if (Material == null) return;
            GUILine();
            
            using (var scroll = new EditorGUILayout.ScrollViewScope(_mainScrollPosition))
            {
                _mainScrollPosition = scroll.scrollPosition;
                Material.RenderMaterialChecks();
            }
        }

        private static void GUILine(int iHeight = 1)
        {
            GUILayout.Space(10);
            var rect = EditorGUILayout.GetControlRect(false, iHeight);
            rect.width = EditorGUIUtility.currentViewWidth;
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            GUILayout.Space(10);
        }
    }
}