using System;
using UnityEditor;
using UnityEngine;
 
namespace Azzmurr.Utils
{
    public class VariableGridScope : IDisposable
    {
        private readonly float[] columnWidths;
        private readonly float spacing;
        private int currentColumn = 0;
        private bool rowOpen = false;

        public VariableGridScope(float[] columnWidths, float spacing = 4f)
        {
            this.columnWidths = columnWidths;
            this.spacing = spacing;
        }

        public void Cell(Action<int> draw)
        {
            if (!rowOpen)
            {
                EditorGUILayout.BeginHorizontal();
                rowOpen = true;
            }

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(columnWidths[currentColumn]))) draw.Invoke(currentColumn);

            currentColumn++;

            if (currentColumn >= columnWidths.Length)
            {
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(spacing);
                currentColumn = 0;
                rowOpen = false;
            }
        }

        public void Dispose()
        {
            if (rowOpen)
            {
                EditorGUILayout.EndHorizontal();
                rowOpen = false;
            }
        }
    }
}