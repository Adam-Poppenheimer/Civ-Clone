using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IHexCell, IUnit>, IUnitPositionCanon {

        #region instance fields and properties

        private ICityFactory CityFactory;

        private UnitSignals Signals;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(ICityFactory cityFactory, UnitSignals signals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ){
            CityFactory         = cityFactory;
            Signals             = signals;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IHexCell location) {
            if(location == null) {
                return true;
            }

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            foreach(var unitAtLocation in GetPossessionsOfOwner(location)) {
                var unitAtLocationOwner = UnitPossessionCanon.GetOwnerOfPossession(unitAtLocation);

                if(unitOwner != unitAtLocationOwner) {
                    return false;
                }
            }

            var cityAtLocation = CityFactory.AllCities.Where(city => city.Location == location).FirstOrDefault();

            if(cityAtLocation != null && unitOwner != CityPossessionCanon.GetOwnerOfPossession(cityAtLocation)) {
                return false;
            }

            return CanPlaceUnitOfTypeAtLocation(unit.Type, location, false); 
        }

        protected override void DoOnPossessionBroken(IUnit possession, IHexCell oldOwner) {
            possession.gameObject.transform.SetParent(null, false);

            Signals.LeftLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, oldOwner));
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IHexCell newOwner) {
            possession.gameObject.transform.SetParent(newOwner != null ? newOwner.transform : null, false);

            if(newOwner != null && newOwner.IsUnderwater) {

                if(newOwner.IsUnderwater) {
                    var unitLocation = possession.gameObject.transform.localPosition;
                    unitLocation.y = newOwner.WaterSurfaceY;
                    possession.gameObject.transform.localPosition = unitLocation;
                }
            }

            Signals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, newOwner));
        }

        #endregion

        #region from IUnitPositionCanon

        public bool CanPlaceUnitOfTypeAtLocation(UnitType type, IHexCell location, bool ignoreOccupancy) {
            if(location == null) {
                return true;
            }

            if(!ignoreOccupancy && AlreadyHasUnitOfType(location, type)) {
                return false;
            }else if(CityFactory.AllCities.Any(city => city.Location == location)) {
                return true;
            }else if(type == UnitType.LandCivilian || type == UnitType.LandMilitary || type == UnitType.City) {
                return !location.IsUnderwater;
            }else {
                return location.IsUnderwater;
            }
        }

        #endregion

        private bool AlreadyHasUnitOfType(IHexCell owner, UnitType type) {
            return GetPossessionsOfOwner(owner).Select(unit => unit.Type).Contains(type);
        }

        #endregion

    }

}
