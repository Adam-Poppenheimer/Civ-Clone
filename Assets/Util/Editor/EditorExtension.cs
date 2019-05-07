using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace Assets.Util.Editor {

    public static class EditorExtension {

        public static int DrawBitMaskField(Rect position, int mask, Type enumType, GUIContent label) {
            var itemNames = Enum.GetNames(enumType);
            var itemValues = Enum.GetValues(enumType) as int[];

            int val = mask;
            int maskVal = 0;

            for(int i = 0; i < itemValues.Length; i++) {
                if(itemValues[i] != 0) {
                    if((val & itemValues[i]) == itemValues[i]) {
                        maskVal |= 1 << i;
                    }
                }else if(val == 0) {
                    maskVal |= 1 << i;
                }
            }

            int newMaskVal = EditorGUI.MaskField(position, label, maskVal, itemNames);
            int changes = maskVal ^ newMaskVal;

            for(int i = 0; i < itemValues.Length; i++) {
                if((changes & (1 << i)) != 0) {
                    if((newMaskVal & (1 << i)) != 0) {
                        if(itemValues[i] == 0) {
                            val = 0;
                            break;

                        }else {
                            val |= itemValues[i];
                        }
                    }else {
                        val &= ~itemValues[i];
                    }
                }
            }

            return val;
        }

    }

}
