using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapGeneration;
using Assets.Simulation.HexMap;

namespace Assets.UI.MapEditor {

    public class MapGenerationPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Slider MapChunkWidthSlider;
        [SerializeField] private Text   MapCellWidthField;

        [SerializeField] private Slider MapChunkHeightSlider;
        [SerializeField] private Text   MapCellHeightField;



        private IHexMapGenerator MapGenerator;

        #endregion

        #region instance methods

        [Inject]
        public void CommonInstall(IHexMapGenerator mapGenerator) {
            MapGenerator = mapGenerator;
        }

        #region Unity messages

        private void OnEnable() {
            UpdateMapChunkWidth (MapChunkWidthSlider .value);
            UpdateMapChunkHeight(MapChunkHeightSlider.value);
        }

        #endregion

        public void TryGeneratingMap() {
            MapGenerator.GenerateMap(
                Mathf.RoundToInt(MapChunkWidthSlider.value),
                Mathf.RoundToInt(MapChunkHeightSlider.value)
            );
        }

        public void UpdateMapChunkWidth(float newWidth) {
            MapCellWidthField.text = (Mathf.RoundToInt(newWidth) * HexMetrics.ChunkSizeX).ToString();
        }

        public void UpdateMapChunkHeight(float newHeight) {
            MapCellHeightField.text = (Mathf.RoundToInt(newHeight) * HexMetrics.ChunkSizeZ).ToString();
        }

        #endregion

    }

}
