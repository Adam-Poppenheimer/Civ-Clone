﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Territory;

namespace Assets.Simulation.Core {

    /// <summary>
    /// A temporary utility used to build cities to empty maps. Should not remain in the build
    /// much longer.
    /// </summary>
    public class CityBuilder : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private int StartingPopulation;
        [SerializeField] private int StartingTerritoryRadius;

        private ICityFactory CityFactory;

        private IPossessionRelationship<ICity, IHexCell> PossessionCanon;

        private IHexGrid HexGrid;

        private IGameCore GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityFactory cityFactory,
            IPossessionRelationship<ICity, IHexCell> possessionCanon, IHexGrid hexGrid,
            IGameCore gameCore
        ){
            CityFactory = cityFactory;
            PossessionCanon = possessionCanon;
            HexGrid = hexGrid;
            GameCore = gameCore;
        }

        public void BuildFullCityOnTile(IHexCell tile) {
            var newCity = CityFactory.Create(tile, GameCore.ActiveCivilization);

            newCity.Population = StartingPopulation;

            foreach(var nearbyTile in HexGrid.GetCellsInRadius(newCity.Location, StartingTerritoryRadius)) {
                if(PossessionCanon.CanChangeOwnerOfPossession(nearbyTile, newCity)) {
                    PossessionCanon.ChangeOwnerOfPossession(nearbyTile, newCity);
                }
            }
        }

        #endregion

    }

}
