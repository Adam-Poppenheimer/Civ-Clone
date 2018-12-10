using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapGeneration;
using Assets.Simulation.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.TitleScreen {

    public class NewGameDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown MapTypeDropdown;
        [SerializeField] private Dropdown MapSizeDropdown;
        [SerializeField] private Dropdown SeaLevelDropdown;
        [SerializeField] private Dropdown CivCountDropdown;

        [SerializeField] private CivTemplateRecord TemplateRecordPrefab;
        [SerializeField] private RectTransform     TemplateRecordContainer;

        private List<CivTemplateRecord> InstantiatedTemplateRecords = 
            new List<CivTemplateRecord>();

        private IMapTemplate SelectedMapTemplate;
        private Vector2      DimensionsInChunks;
        private int          ContinentalLandPercentage;
        private int          CivCount;

        private HashSet<ICivilizationTemplate> ChosenTemplates = new HashSet<ICivilizationTemplate>();
        



        private IMapGenerationConfig                      Config;
        private IEnumerable<IMapTemplate>                 MapTemplates;
        private IMapGenerator                             MapGenerator;
        private Animator                                  UIAnimator;
        private ReadOnlyCollection<ICivilizationTemplate> AllCivTemplates;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapGenerationConfig config, IEnumerable<IMapTemplate> mapTemplates,
            IMapGenerator mapGenerator, [Inject(Id = "UI Animator")] Animator uiAnimator,
            ReadOnlyCollection<ICivilizationTemplate> civTemplates
        ) {
            Config       = config;
            MapTemplates = mapTemplates;
            MapGenerator = mapGenerator;
            UIAnimator   = uiAnimator;
            AllCivTemplates = civTemplates;
        }

        #region Unity messages

        private void Start() {
            InitializeTypeDropdown();
            InitializeSizeDropdown();
            InitializeSeaLevelDropdown();
        }

        #endregion

        public void UpdateSelectedMapTemplate(int optionIndex) {
            SelectedMapTemplate = MapTemplates.Where(
                template => template.name.Equals(MapTypeDropdown.options[optionIndex].text)
            ).FirstOrDefault();
        }

        public void UpdateMapSize(int optionIndex) {
            var mapSize = Config.MapSizes[optionIndex];

            DimensionsInChunks = mapSize.DimensionsInChunks;

            InitializeCivCountDropdown(mapSize.ValidCivCounts);

            CivCountDropdown.value = mapSize.ValidCivCounts.IndexOf(mapSize.IdealCivCount);
        }

        public void UpdateSeaLevel(int optionIndex) {
            ContinentalLandPercentage = Config.GetLandPercentageForSeaLevel((SeaLevelCategory)optionIndex);
        }

        public void UpdateCivCount(int optionIndex) {
            if(!int.TryParse(CivCountDropdown.options[optionIndex].text, out CivCount)) {
                Debug.LogErrorFormat("Detected invalid civ count string {0} when creating a new map", CivCountDropdown.options[optionIndex] );
                CivCount = 8;
            }

            foreach(var record in InstantiatedTemplateRecords) {
                Destroy(record.gameObject);
            }

            InstantiatedTemplateRecords.Clear();

            ChosenTemplates.Clear();

            for(int i = 0; i < CivCount; i++) {
                var newTemplateRecord = Instantiate(TemplateRecordPrefab);

                newTemplateRecord.gameObject.SetActive(true);
                newTemplateRecord.transform.SetParent(TemplateRecordContainer, false);

                InstantiatedTemplateRecords.Add(newTemplateRecord);
            }

            ResetSelectedCivs();
        }

        public void UpdateChosenCivs() {
            ChosenTemplates.Clear();

            foreach(var templateRecord in InstantiatedTemplateRecords) {
                var templateChosen = AllCivTemplates.First(template => template.Name.Equals(templateRecord.SelectedDropdownText));

                ChosenTemplates.Add(templateChosen);
            }

            foreach(var templateRecord in InstantiatedTemplateRecords) {
                var templateChosen = AllCivTemplates.First(template => template.Name.Equals(templateRecord.SelectedDropdownText));

                templateRecord.PopulateValidTemplatesDropdown(AllCivTemplates, ChosenTemplates, templateChosen);
            }
        }

        public void GenerateMap() {
            var variables = new MapGenerationVariables() {
                ChunkCountX = Mathf.RoundToInt(DimensionsInChunks.x),
                ChunkCountZ = Mathf.RoundToInt(DimensionsInChunks.y),
                CivCount = CivCount, ContinentalLandPercentage = ContinentalLandPercentage,
                Civilizations = ChosenTemplates.ToList()
            };

            MapGenerator.GenerateMap(
                SelectedMapTemplate,
                variables
            );

            UIAnimator.SetTrigger("Play Mode Requested");
        }

        private void InitializeTypeDropdown() {
            MapTypeDropdown.ClearOptions();

            List<Dropdown.OptionData> typeOptions = MapTemplates.Select(
                template => new Dropdown.OptionData(template.name)
            ).ToList();

            MapTypeDropdown.AddOptions(typeOptions);
            MapTypeDropdown.value = 0;

            UpdateSelectedMapTemplate(0);
        }

        private void InitializeSizeDropdown() {
            MapSizeDropdown.ClearOptions();

            List<Dropdown.OptionData> sizeOptions = Config.MapSizes.Select(
                size => new Dropdown.OptionData(size.name)
            ).ToList();

            MapSizeDropdown.AddOptions(sizeOptions);
            
            var standardOption = MapSizeDropdown.options.Where(option => option.text.Equals(Config.DefaultMapSize.name)).FirstOrDefault();

            MapSizeDropdown.value = MapSizeDropdown.options.IndexOf(standardOption);

            UpdateMapSize(MapSizeDropdown.value);
        }

        private void InitializeSeaLevelDropdown() {
            SeaLevelDropdown.ClearOptions();

            List<Dropdown.OptionData> seaLevelOptions = EnumUtil.GetValues<SeaLevelCategory>().Select(
                seaLevel => new Dropdown.OptionData(seaLevel.ToString())
            ).ToList();

            SeaLevelDropdown.AddOptions(seaLevelOptions);
            
            var standardOption = SeaLevelDropdown.options.Where(option => option.text.Equals(SeaLevelCategory.Normal.ToString())).FirstOrDefault();

            SeaLevelDropdown.value = SeaLevelDropdown.options.IndexOf(standardOption);

            UpdateSeaLevel(SeaLevelDropdown.value);
        }

        private void InitializeCivCountDropdown(IEnumerable<int> validCivCounts) {
            CivCountDropdown.ClearOptions();

            List<Dropdown.OptionData> civCountOptions = validCivCounts.Select(
                count => new Dropdown.OptionData(count.ToString())
            ).ToList();

            CivCountDropdown.AddOptions(civCountOptions);
        }

        private void ResetSelectedCivs() {
            ChosenTemplates.Clear();

            var templatesToChoose = AllCivTemplates.Take(CivCount).ToArray();

            foreach(var template in templatesToChoose) {
                ChosenTemplates.Add(template);
            }

            for(int i = 0; i < templatesToChoose.Length; i++) {
                var thisTemplateRecord = InstantiatedTemplateRecords[i];
                var selectedTemplate = templatesToChoose[i];

                thisTemplateRecord.PopulateValidTemplatesDropdown(AllCivTemplates, ChosenTemplates, selectedTemplate);
            }
        }

        #endregion

    }

}
