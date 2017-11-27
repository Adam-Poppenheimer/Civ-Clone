using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.Cities {

    public class CityUnemploymentDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text UnemployedPeopleField;

        private ITilePossessionCanon TilePossessionCanon;
        private IBuildingPossessionCanon BuildingPossessionCanon;        

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITilePossessionCanon tilePossessionCanon,
            IBuildingPossessionCanon buildingPossessionCanon) {

            TilePossessionCanon = tilePossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #region from CityDisplayBase

        protected override void DisplayCity(ICity city) {
            int occupiedTiles = TilePossessionCanon.GetTilesOfCity(city).Where(tile => tile.WorkerSlot.IsOccupied).Count();

            int occupiedBuildingSlots = 0;
            foreach(var building in BuildingPossessionCanon.GetBuildingsInCity(city)) {
                occupiedBuildingSlots += building.Slots.Where(slot => slot.IsOccupied).Count();
            }

            int unemployedPeople = city.Population - (occupiedTiles + occupiedBuildingSlots);

            UnemployedPeopleField.text = unemployedPeople.ToString();
        }

        #endregion

        #endregion

    }

}
