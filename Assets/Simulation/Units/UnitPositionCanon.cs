using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

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

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(ICityFactory cityFactory, UnitSignals signals,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IHexGrid grid
        ){
            CityFactory         = cityFactory;
            Signals             = signals;
            CityPossessionCanon = cityPossessionCanon;
            UnitPossessionCanon = unitPossessionCanon;
            Grid                = grid;
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
            if(newOwner == null) {
                return;
            }

            possession.gameObject.transform.position = Grid.PerformIntersectionWithTerrainSurface(newOwner.transform.position);
            possession.gameObject.transform.SetParent(newOwner.transform, true);            

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
