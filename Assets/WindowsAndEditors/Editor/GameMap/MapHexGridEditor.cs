﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Assets.Simulation.GameMap;

namespace Assets.WindowsAndEditors.GameMap {

    [CustomEditor(typeof(MapHexGrid))]
    class MapHexGridEditor : UnityEditor.Editor {

        #region instance methods

        #region from UnityEditor.Editor

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if(GUILayout.Button("Regenerate Map")) {
                RegenerateMap();
            }
        }

        #endregion

        private void RegenerateMap() {
            (target as MapHexGrid).RegenerateMap();

            MapTileInstaller.PerformEditorTimeInject();
        }

        #endregion

    }

}