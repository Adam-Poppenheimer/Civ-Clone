using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IHexCell, IUnit>, IUnitPositionCanon {

        #region instance fields and properties

        private ICityFactory CityFactory;

        private UnitSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(ICityFactory cityFactory, UnitSignals signals){
            CityFactory             = cityFactory;
            Signals                 = signals;

            Signals.UnitBeingDestroyedSignal.Subscribe(OnUnitBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IHexCell location) {
            return location == null || CanPlaceUnitOfTypeAtLocation(unit.Type, location, false);            
        }

        protected override void DoOnPossessionBroken(IUnit possession, IHexCell oldOwner) {
            possession.gameObject.transform.SetParent(null, false);
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IHexCell newOwner) {
            possession.gameObject.transform.SetParent(newOwner != null ? newOwner.transform : null, false);

            if(newOwner != null && newOwner.IsUnderwater) {
                var unitLocation = possession.gameObject.transform.localPosition;
                unitLocation.y = newOwner.WaterSurfaceY;
                possession.gameObject.transform.localPosition = unitLocation;
            }

            Signals.UnitLocationChangedSignal.OnNext(new UniRx.Tuple<IUnit, IHexCell>(possession, newOwner));
        }

        #endregion

        public bool CanPlaceUnitOfTypeAtLocation(UnitType type, IHexCell location, bool ignoreOccupancy) {
            if(location == null) {
                return true;
            }
            
            if(!ignoreOccupancy && AlreadyHasUnitOfType(location, type)) {
                return false;
            }
            
            if(type == UnitType.LandCivilian || type == UnitType.LandMilitary || type == UnitType.City) {
                return !location.IsUnderwater;
            }else {
                return location.IsUnderwater;
            }
        }

        private bool IsValidForWaterUnit(IHexCell cell) {
            return (
                cell.IsUnderwater
                || CityFactory.AllCities.Where(city => city.Location == cell).LastOrDefault() != null
            );
        }

        private bool IsValidForLandUnit(IHexCell cell) {
            return !cell.IsUnderwater;
        }

        private bool AlreadyHasUnitOfType(IHexCell owner, UnitType type) {
            return GetPossessionsOfOwner(owner).Select(unit => unit.Type).Contains(type);
        }

        private void OnUnitBeingDestroyed(IUnit unit) {
            ChangeOwnerOfPossession(unit, null);
        }

        #endregion

    }

}
