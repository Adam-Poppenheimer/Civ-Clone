using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;
using UnityEngine;

namespace Assets.Simulation.HexMap.Editor {

    public class TextureArrayWizard : ScriptableWizard {

        #region instance fields and properties

        [SerializeField] private Texture2D[] Textures;

        #endregion

        #region static methods

        [MenuItem("Assets/Create/Texture Array")]
        private static void CreateWizard() {
            ScriptableWizard.DisplayWizard<TextureArrayWizard>(
                "Create Texture Array", "Create"
            );
        }

        #endregion

        #region instance methods

        private void OnWizardCreate() {
            if(Textures.Length == 0) {
                return;
            }
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
            );

            if(path.Length == 0) {
                return;
            }

            Texture2D texture = Textures[0];

            Texture2DArray textureArray = new Texture2DArray(
                texture.width, texture.height, Textures.Length, texture.format, texture.mipmapCount > 1
            );
            textureArray.anisoLevel = texture.anisoLevel;
            textureArray.filterMode = texture.filterMode;
            textureArray.wrapMode   = texture.wrapMode;

            for(int i = 0; i < Textures.Length; i++) {
                for(int j = 0; j < texture.mipmapCount; j++) {
                    Graphics.CopyTexture(Textures[i], 0, j, textureArray, i, j);
                }
            }

            AssetDatabase.CreateAsset(textureArray, path);
        }

        #endregion

    }

}
