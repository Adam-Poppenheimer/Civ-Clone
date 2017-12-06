﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Territory;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public class ResourceGenerationLogic : IResourceGenerationLogic {

        #region instance fields and properties

        private ICityConfig Config;

        private ITilePossessionCanon TileCanon;
        private IBuildingPossessionCanon BuildingCanon;

        private IIncomeModifierLogic IncomeModifierLogic;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceGenerationLogic(ICityConfig config, ITilePossessionCanon tileCanon,
            IBuildingPossessionCanon buildingCanon, IIncomeModifierLogic incomeModifierLogic,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon) {
            Config = config;
            TileCanon = tileCanon;
            BuildingCanon = buildingCanon;
            IncomeModifierLogic = incomeModifierLogic;
            CityPossessionCanon = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IResourceGenerationLogic

        public ResourceSummary GetTotalYieldForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var retval = city.Location.WorkerSlot.BaseYield;

            int employedPops = 0;

            foreach(var tile in TileCanon.GetTilesOfCity(city)) {
                if(tile.SuppressSlot) {
                    continue;
                }

                retval += GetYieldOfSlotForCity(tile.WorkerSlot, city);
                if(tile.WorkerSlot.IsOccupied) {
                    employedPops++;
                }
            }

            foreach(var building in BuildingCanon.GetBuildingsInCity(city)) {
                foreach(var slot in building.Slots) {
                    retval += GetYieldOfSlotForCity(slot, city);
                    if(slot.IsOccupied) {
                        employedPops++;
                    }
                }

                retval += building.Template.StaticYield;
            }

            retval += GetYieldOfUnemployedForCity(city) * Math.Max(0, city.Population - employedPops);

            return retval;
        }

        public ResourceSummary GetYieldOfSlotForCity(IWorkerSlot slot, ICity city) {
            if(slot == null) {
                throw new ArgumentNullException("slot");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            if(slot.IsOccupied) {
                var multiplier = ResourceSummary.Ones +
                    IncomeModifierLogic.GetYieldMultipliersForSlot(slot) +
                    IncomeModifierLogic.GetYieldMultipliersForCity(city);   
                
                var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);
                if(owningCivilization != null) {
                    multiplier += IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
                }              

                return slot.BaseYield * multiplier;
            }else {
                return ResourceSummary.Empty;
            }
        }

        public ResourceSummary GetYieldOfUnemployedForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var multiplier = ResourceSummary.Ones + IncomeModifierLogic.GetYieldMultipliersForCity(city);

            var owningCivilization = CityPossessionCanon.GetOwnerOfPossession(city);
            if(owningCivilization != null) {
                multiplier += IncomeModifierLogic.GetYieldMultipliersForCivilization(owningCivilization);
            }

            return Config.UnemployedYield * multiplier;
        }

        #endregion

        #endregion

    }

}
