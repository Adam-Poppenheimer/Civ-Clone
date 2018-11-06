using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

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



        private IBuildingProductionValidityLogic              BuildingValidityLogic;
        private IUnitProductionValidityLogic                  UnitValidityLogic;
        private ITechCanon                                    TechCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IProductionProjectFactory                     ProjectFactory;
        private DiContainer                                   Container;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IBuildingProductionValidityLogic buildingValidityLogic,
            IUnitProductionValidityLogic unitValidityLogic, ITechCanon techCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IProductionProjectFactory projectFactory, DiContainer container
        ){
            BuildingValidityLogic = buildingValidityLogic;
            UnitValidityLogic     = unitValidityLogic;
            TechCanon             = techCanon;
            CityPossessionCanon   = cityPossessionCanon;
            ProjectFactory        = projectFactory;
            Container             = container;
        }

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

            foreach(var buildingTemplate in TechCanon.GetResearchedBuildings(cityOwner)) {
                var newRecord = BuildRecord();

                newRecord.BuildingTemplate = buildingTemplate;
                newRecord.SelectionButton.interactable = BuildingValidityLogic.IsTemplateValidForCity(buildingTemplate, ObjectToDisplay);
                newRecord.SelectionButton.onClick.AddListener(
                    () => ObjectToDisplay.ActiveProject = ProjectFactory.ConstructProject(buildingTemplate)
                );

                newRecord.Refresh();

                InstantiatedProjectRecords.Add(newRecord);
            }
        }

        #endregion

        private CityProjectRecord BuildRecord() {
            var newRecord = Container.InstantiatePrefabForComponent<CityProjectRecord>(ProjectRecordPrefab);

            newRecord.transform.SetParent(ProjectRecordContainer, false);
            newRecord.gameObject.SetActive(true);

            return newRecord;
        }

        #endregion

    }

}
