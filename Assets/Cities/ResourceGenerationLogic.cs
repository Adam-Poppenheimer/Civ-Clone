using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Cities.Buildings;

namespace Assets.Cities {

    public class ResourceGenerationLogic : IResourceGenerationLogic {

        #region instance fields and properties

        private IResourceGenerationConfig Config;

        private ITilePossessionCanon TileCanon;
        private IBuildingPossessionCanon BuildingCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceGenerationLogic(IResourceGenerationConfig config, ITilePossessionCanon tileCanon,
            IBuildingPossessionCanon buildingCanon) {
            Config = config;
            TileCanon = tileCanon;
            BuildingCanon = buildingCanon;
        }

        #endregion

        #region instance methods

        #region from IResourceGenerationLogic

        public ResourceSummary GetTotalYieldForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            var retval = ResourceSummary.Empty;

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
                return slot.BaseYield;
            }else {
                return ResourceSummary.Empty;
            }
        }

        public ResourceSummary GetYieldOfUnemployedForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return Config.UnemployedYield;
        }

        #endregion

        #endregion

    }

}
