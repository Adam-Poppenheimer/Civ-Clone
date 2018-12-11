using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;
using Assets.Simulation.Technology;

namespace Assets.UI.Cities.Production {

    public class CityProjectChooser : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private RectTransform ProjectRecordContainer;

        [SerializeField] private CityProjectRecord ProjectRecordPrefab;

        private List<CityProjectRecord> InstantiatedProjectRecords = new List<CityProjectRecord>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();



        private IBuildingProductionValidityLogic              BuildingValidityLogic;
        private IUnitProductionValidityLogic                  UnitValidityLogic;
        private ITechCanon                                    TechCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IProductionProjectFactory                     ProjectFactory;
        private DiContainer                                   Container;
        private List<IBuildingTemplate>                       AllBuildingTemplates;
        private IBuildingFactory                              BuildingFactory;
        private CitySignals                                   CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBuildingProductionValidityLogic buildingValidityLogic,
            IUnitProductionValidityLogic unitValidityLogic, ITechCanon techCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IProductionProjectFactory projectFactory, DiContainer container,
            List<IBuildingTemplate> allBuildingTemplates, IBuildingFactory buildingFactory,
            CitySignals citySignals
        ){
            BuildingValidityLogic = buildingValidityLogic;
            UnitValidityLogic     = unitValidityLogic;
            TechCanon             = techCanon;
            CityPossessionCanon   = cityPossessionCanon;
            ProjectFactory        = projectFactory;
            Container             = container;
            AllBuildingTemplates  = allBuildingTemplates;
            BuildingFactory       = buildingFactory;
            CitySignals           = citySignals;
        }

        #region Unity messages

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals.CityGainedBuildingSignal.Subscribe(data => Refresh()));
            SignalSubscriptions.Add(CitySignals.CityLostBuildingSignal  .Subscribe(data => Refresh()));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());
            SignalSubscriptions.Clear();
        }

        #endregion

        #region from CityDisplayBase

        public override void Refresh() {
            foreach(var projectRecord in new List<CityProjectRecord>(InstantiatedProjectRecords)) {
                Destroy(projectRecord.gameObject);
            }
            InstantiatedProjectRecords.Clear();

            if(ObjectToDisplay == null) {
                return;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(ObjectToDisplay);

            if(DisplayType == CityDisplayType.PlayMode) {
                AddUnitProjects(cityOwner);

                AddBuildingProjects(
                    cityOwner, TechCanon.GetResearchedBuildings(cityOwner),
                    template => ObjectToDisplay.ActiveProject = ProjectFactory.ConstructProject(template)
                );

            }else if(DisplayType == CityDisplayType.MapEditor) {
                AddBuildingProjects(
                    cityOwner, AllBuildingTemplates,
                    template => BuildingFactory.BuildBuilding(template, ObjectToDisplay)
                );
            }
        }

        #endregion

        private void AddUnitProjects(ICivilization cityOwner) {
            foreach(var unitTemplate in TechCanon.GetResearchedUnits(cityOwner)) {
                var newRecord = BuildRecord();

                newRecord.UnitTemplate = unitTemplate;
                newRecord.SelectionButton.interactable = UnitValidityLogic.IsTemplateValidForCity(unitTemplate, ObjectToDisplay);
                newRecord.SelectionButton.onClick.AddListener(
                    () => ObjectToDisplay.ActiveProject = ProjectFactory.ConstructProject(unitTemplate)
                );

                newRecord.Refresh();

                InstantiatedProjectRecords.Add(newRecord);
            }
        }

        private void AddBuildingProjects(
            ICivilization cityOwner, IEnumerable<IBuildingTemplate> validTemplates,
            Action<IBuildingTemplate> recordSelectionAction
        ) {
            foreach(var buildingTemplate in validTemplates) {
                var newRecord = BuildRecord();

                newRecord.BuildingTemplate = buildingTemplate;

                newRecord.SelectionButton.interactable = BuildingValidityLogic.IsTemplateValidForCity(buildingTemplate, ObjectToDisplay);
                newRecord.SelectionButton.onClick.AddListener(() => recordSelectionAction(buildingTemplate));

                newRecord.Refresh();

                InstantiatedProjectRecords.Add(newRecord);
            }
        }

        private CityProjectRecord BuildRecord() {
            var newRecord = Container.InstantiatePrefabForComponent<CityProjectRecord>(ProjectRecordPrefab);

            newRecord.transform.SetParent(ProjectRecordContainer, false);
            newRecord.gameObject.SetActive(true);

            return newRecord;
        }

        #endregion

    }

}
