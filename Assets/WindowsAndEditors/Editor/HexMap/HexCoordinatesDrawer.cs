using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Assets.Simulation.HexMap;

namespace Assets.WindowsAndEditors.HexMap {

    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer {

        #region instance methods

        #region from PropertyDrawer

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            HexCoordinates coordinates = new HexCoordinates(
                property.FindPropertyRelative("_x").intValue,
                property.FindPropertyRelative("_z").intValue
            );

            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, coordinates.ToString());
        }

        #endregion

        #endregion

    }

}
