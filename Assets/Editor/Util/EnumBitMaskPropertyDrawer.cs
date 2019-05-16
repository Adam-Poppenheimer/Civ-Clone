using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace Assets.Util.Editor {

    [CustomPropertyDrawer(typeof(BitMaskAttribute))]
    public class EnumBitMaskPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var typeAttr = attribute as BitMaskAttribute;

            property.intValue = EditorExtension.DrawBitMaskField(position, property.intValue, typeAttr.PropertyType, label);
        }

    }

}
