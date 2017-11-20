using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities;
using Assets.GameMap;

namespace Assets.Core {

    public class CityBuilder : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private int StartingPopulation;
        [SerializeField] private int StartingTerritoryRadius;

        private IRecordkeepingCityFactory CityFactory;

        private ITilePossessionCanon PossessionCanon;

        private IMapHexGrid HexGrid;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IRecordkeepingCityFactory cityFactory,
            ITilePossessionCanon possessionCanon, IMapHexGrid hexGrid) {
            CityFactory = cityFactory;
            PossessionCanon = possessionCanon;
            HexGrid = hexGrid;
        }

        public void BuildFullCityOnTile(IMapTile tile) {
            var newCity = CityFactory.Create(tile);

            newCity.Population = StartingPopulation;

            foreach(var nearbyTile in HexGrid.GetTilesInRadius(newCity.Location, StartingTerritoryRadius)) {
                if(PossessionCanon.CanChangeOwnerOfTile(nearbyTile, newCity)) {
                    PossessionCanon.ChangeOwnerOfTile(nearbyTile, newCity);
                }
            }
        }

        #endregion

    }

}
