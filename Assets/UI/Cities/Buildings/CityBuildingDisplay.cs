using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities.Buildings {

    public class CityBuildingDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Transform BuildingDisplayContainer;

        private List<IBuildingDisplay> InstantiatedBuildingDisplays = new List<IBuildingDisplay>();

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();





        private IPossessionRelationship<ICity, IBuilding> PossessionCanon;
        private BuildingDisplayFactory                    DisplayFactory;
        private CitySignals                               CitySignals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPossessionRelationship<ICity, IBuilding> possessionCanon,
            BuildingDisplayFactory displayFactory, CitySignals citySignals
        ) {
            PossessionCanon = possessionCanon;
            DisplayFactory  = displayFactory;
            CitySignals     = citySignals;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            DisplayBuildings(PossessionCanon.GetPossessionsOfOwner(ObjectToDisplay));
        }

        protected override void DoOnEnable() {
            SignalSubscriptions.Add(CitySignals.CityGainedBuildingSignal.Subscribe(data => Refresh()));
            SignalSubscriptions.Add(CitySignals.CityLostBuildingSignal  .Subscribe(data => Refresh()));
        }

        protected override void DoOnDisable() {
            SignalSubscriptions.ForEach(subscription => subscription.Dispose());

            SignalSubscriptions.Clear();
        }

        #endregion

        private void DisplayBuildings(IEnumerable<IBuilding> buildings) {
            foreach(var buildingDisplay in InstantiatedBuildingDisplays) {
                buildingDisplay.gameObject.SetActive(false);
            }

            int buildingDisplayIndex = 0;
            foreach(var building in buildings) {   
                             
                var buildingDisplay = GetBuildingDisplay(buildingDisplayIndex++);

                buildingDisplay.Owner             = ObjectToDisplay;
                buildingDisplay.BuildingToDisplay = building;                

                buildingDisplay.gameObject.SetActive(true);
                buildingDisplay.Refresh(DisplayType == CityDisplayType.MapEditor);
            }
        }

        private IBuildingDisplay GetBuildingDisplay(int index) {
            if(InstantiatedBuildingDisplays.Count == index) {
                var newDisplay = DisplayFactory.Create();

                newDisplay.gameObject.transform.SetParent(BuildingDisplayContainer, false);

                InstantiatedBuildingDisplays.Add(newDisplay);
            }
            return InstantiatedBuildingDisplays[index];
        }

        #endregion
        
    }
}
