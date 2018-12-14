using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapGeneration;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.UI.MapEditor {

    public class MapGenerationPanel : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Slider MapChunkWidthSlider;
        [SerializeField] private Text   MapCellWidthField;

        [SerializeField] private Slider MapChunkHeightSlider;
        [SerializeField] private Text   MapCellHeightField;



        private IMapGenerator        MapGenerator;
        private IMapGenerationConfig GenerationConfig;
        private IHexMapRenderConfig  RenderConfig;

        #endregion

        #region instance methods

        [Inject]
        public void CommonInstall(
            IMapGenerator mapGenerator, IMapGenerationConfig generationConfig,
            IHexMapRenderConfig renderConfig
        ) {
            MapGenerator     = mapGenerator;
            GenerationConfig = generationConfig;
            RenderConfig     = renderConfig;
        }

        #region Unity messages

        private void OnEnable() {
            UpdateMapChunkWidth (MapChunkWidthSlider .value);
            UpdateMapChunkHeight(MapChunkHeightSlider.value);
        }

        #endregion

        public void TryGeneratingMap() {
            var variables = new MapGenerationVariables() {
                ChunkCountX = Mathf.RoundToInt(MapChunkWidthSlider.value),
                ChunkCountZ = Mathf.RoundToInt(MapChunkHeightSlider.value),
                ContinentalLandPercentage = 25,
                Civilizations = new List<ICivilizationTemplate>(),
                StartingTechs = new List<ITechDefinition>()
            };

            MapGenerator.GenerateMap(
                GenerationConfig.TestTemplate, variables
            );
        }

        public void UpdateMapChunkWidth(float newWidth) {
            MapCellWidthField.text = (Mathf.RoundToInt(newWidth) * RenderConfig.ChunkSizeX).ToString();
        }

        public void UpdateMapChunkHeight(float newHeight) {
            MapCellHeightField.text = (Mathf.RoundToInt(newHeight) * RenderConfig.ChunkSizeZ).ToString();
        }

        #endregion

    }

}
