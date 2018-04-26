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

        private UnitSignals Signals;

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(UnitSignals signals,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ){
            Signals             = signals;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IHexCell location) {
             return CanPlaceUnitAtLocation(unit, location, false);
        }

        protected override void DoOnPossessionBroken(IUnit possession, IHexCell oldOwner) {
            possession.SetParent(null, true);

            Signals.LeftLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, oldOwner));
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IHexCell newOwner) {
            if(newOwner == null) {
                return;
            }

            possession.SetParent(newOwner.transform, true);            

            Signals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, newOwner));
        }

        #endregion

        #region from IUnitPositionCanon

        public bool CanPlaceUnitAtLocation(IUnit unit, IHexCell location, bool isMeleeAttacking) {
            if(location == null) {
                return true;
            }

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            foreach(var unitAtLocation in GetPossessionsOfOwner(location)) {
                var unitAtLocationOwner = UnitPossessionCanon.GetOwnerOfPossession(unitAtLocation);

                if(unitOwner != unitAtLocationOwner && !isMeleeAttacking) {
                    return false;
                }
            }

            return CanPlaceUnitTemplateAtLocation(unit.Template, location, isMeleeAttacking);
        }

        public bool CanPlaceUnitTemplateAtLocation(IUnitTemplate template, IHexCell location, bool isMeleeAttacking) {
            if(location == null) {
                return true;

            }else if(!isMeleeAttacking && LocationHasUnitBlockingType(location, template.Type)) {
                return false;

            }else if(CityLocationCanon.GetPossessionsOfOwner(location).Count() > 0) {
                return true;

            }else {
                return template.IsAquatic == location.IsUnderwater;
            }
        }

        #endregion

        private bool LocationHasUnitBlockingType(IHexCell location, UnitType type) {
            return GetPossessionsOfOwner(location).Where(unit => type.HasSameSupertypeAs(unit.Type)).Count() > 0;
        }

        #endregion

    }

}
