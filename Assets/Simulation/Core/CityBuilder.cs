using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;

namespace Assets.Simulation.Core {

    public class CityBuilder : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private int StartingPopulation;
        [SerializeField] private int StartingTerritoryRadius;

        private ICityFactory CityFactory;

        private ITilePossessionCanon PossessionCanon;

        private IMapHexGrid HexGrid;

        private GameCore GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityFactory cityFactory,
            ITilePossessionCanon possessionCanon, IMapHexGrid hexGrid,
            GameCore gameCore
        ){
            CityFactory = cityFactory;
            PossessionCanon = possessionCanon;
            HexGrid = hexGrid;
            GameCore = gameCore;
        }

        public void BuildFullCityOnTile(IMapTile tile) {
            var newCity = CityFactory.Create(tile, GameCore.PlayerCivilization);

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
