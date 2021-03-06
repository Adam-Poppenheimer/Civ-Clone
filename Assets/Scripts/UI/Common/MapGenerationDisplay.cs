﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.MapGeneration;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.Common {

    public class MapGenerationDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Dropdown MapTypeDropdown     = null;
        [SerializeField] private Dropdown MapSizeDropdown     = null;
        [SerializeField] private Dropdown SeaLevelDropdown    = null;
        [SerializeField] private Dropdown CivCountDropdown    = null;
        [SerializeField] private Dropdown StartingEraDropdown = null;

        [SerializeField] private CivTemplateRecord TemplateRecordPrefab    = null;
        [SerializeField] private RectTransform     TemplateRecordContainer = null;

        public string UIAnimatorTrigger { get; set; }

        private List<CivTemplateRecord> InstantiatedTemplateRecords = 
            new List<CivTemplateRecord>();

        private IMapTemplate  SelectedMapTemplate;
        private Vector2       DimensionsInCells;
        private int           ContinentalLandPercentage;
        private int           CivCount;
        private TechnologyEra StartingEra;

        private HashSet<ICivilizationTemplate> ChosenTemplates = new HashSet<ICivilizationTemplate>();
        



        private IMapGenerationConfig        MapGenerationConfig;
        private IEnumerable<IMapTemplate>   MapTemplates;
        private IMapGenerator               MapGenerator;
        private Animator                    UIAnimator;
        private List<ICivilizationTemplate> ValidCivTemplates;
        private ITechCanon                  TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IMapGenerationConfig config, IEnumerable<IMapTemplate> mapTemplates,
            IMapGenerator mapGenerator, [Inject(Id = "UI Animator")] Animator uiAnimator,
            ReadOnlyCollection<ICivilizationTemplate> civTemplates, ITechCanon techCanon
        ) {
            MapGenerationConfig = config;
            MapTemplates        = mapTemplates;
            MapGenerator        = mapGenerator;
            UIAnimator          = uiAnimator;
            ValidCivTemplates   = civTemplates.Where(template => !template.IsBarbaric).ToList();
            TechCanon           = techCanon;
        }

        #region Unity messages

        private void Start() {
            InitializeTypeDropdown       ();
            InitializeSizeDropdown       ();
            InitializeSeaLevelDropdown   ();
            InitializeStartingEraDropdown();
        }

        #endregion

        public void UpdateSelectedMapTemplate(int optionIndex) {
            SelectedMapTemplate = MapTemplates.Where(
                template => template.name.Equals(MapTypeDropdown.options[optionIndex].text)
            ).FirstOrDefault();
        }

        public void UpdateMapSize(int optionIndex) {
            var mapSize = MapGenerationConfig.MapSizes[optionIndex];

            DimensionsInCells = mapSize.DimensionsInCells;

            InitializeCivCountDropdown(mapSize.ValidCivCounts);

            CivCountDropdown.value = mapSize.ValidCivCounts.IndexOf(mapSize.IdealCivCount);
        }

        public void UpdateSeaLevel(int optionIndex) {
            ContinentalLandPercentage = MapGenerationConfig.GetLandPercentageForSeaLevel((SeaLevelCategory)optionIndex);
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

        public void UpdateStartingEra(int optionIndex) {
            StartingEra = (TechnologyEra)optionIndex;
        }

        public void UpdateChosenCivs() {
            ChosenTemplates.Clear();

            foreach(var templateRecord in InstantiatedTemplateRecords) {
                var templateChosen = ValidCivTemplates.First(template => template.Name.Equals(templateRecord.SelectedDropdownText));

                ChosenTemplates.Add(templateChosen);
            }

            foreach(var templateRecord in InstantiatedTemplateRecords) {
                var templateChosen = ValidCivTemplates.First(template => template.Name.Equals(templateRecord.SelectedDropdownText));

                templateRecord.PopulateValidTemplatesDropdown(ValidCivTemplates, ChosenTemplates, templateChosen);
            }
        }

        public void GenerateMap() {
            var startingTechs = TechCanon.GetTechsOfPreviousEras(StartingEra).Concat(TechCanon.GetEntryTechsOfEra(StartingEra));

            var variables = new MapGenerationVariables() {
                CellCountX = Mathf.RoundToInt(DimensionsInCells.x),
                CellCountZ = Mathf.RoundToInt(DimensionsInCells.y),
                ContinentalLandPercentage = ContinentalLandPercentage,
                Civilizations = ChosenTemplates.ToList(),
                StartingTechs = startingTechs
            };

            MapGenerator.GenerateMap(
                SelectedMapTemplate,
                variables
            );

            UIAnimator.SetTrigger(UIAnimatorTrigger);
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

            List<Dropdown.OptionData> sizeOptions = MapGenerationConfig.MapSizes.Select(
                size => new Dropdown.OptionData(size.name)
            ).ToList();

            MapSizeDropdown.AddOptions(sizeOptions);
            
            var standardOption = MapSizeDropdown.options.Where(option => option.text.Equals(MapGenerationConfig.DefaultMapSize.name)).FirstOrDefault();

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

        private void InitializeStartingEraDropdown() {
            StartingEraDropdown.ClearOptions();

            List<Dropdown.OptionData> eraOptions = EnumUtil.GetValues<TechnologyEra>().Select(
                era => new Dropdown.OptionData(era.ToString())
            ).ToList();

            StartingEraDropdown.AddOptions(eraOptions);
            
            var standardOption = StartingEraDropdown.options.Where(option => option.text.Equals(TechnologyEra.Ancient.ToString())).FirstOrDefault();

            StartingEraDropdown.value = StartingEraDropdown.options.IndexOf(standardOption);

            UpdateStartingEra(StartingEraDropdown.value);
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

            var templatesToChoose = ValidCivTemplates.Take(CivCount).ToArray();

            foreach(var template in templatesToChoose) {
                ChosenTemplates.Add(template);
            }

            for(int i = 0; i < templatesToChoose.Length; i++) {
                var thisTemplateRecord = InstantiatedTemplateRecords[i];
                var selectedTemplate = templatesToChoose[i];

                thisTemplateRecord.PopulateValidTemplatesDropdown(ValidCivTemplates, ChosenTemplates, selectedTemplate);
            }
        }

        #endregion

    }

}
