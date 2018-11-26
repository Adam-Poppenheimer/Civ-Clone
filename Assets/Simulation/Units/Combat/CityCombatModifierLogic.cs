using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Simulation.Units.Combat {

    public class CityCombatModifierLogic : ICityCombatModifierLogic {

        #region instance fields and properties

        private IUnitPositionCanon                       UnitPositionCanon;
        private ICityModifiers                           CityModifiers;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CityCombatModifierLogic(
            IUnitPositionCanon unitPositionCanon, ICityModifiers cityModifiers,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon
        ) {
            UnitPositionCanon = unitPositionCanon;
            CityModifiers     = cityModifiers;
            CityLocationCanon = cityLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICityCombatModifierLogic

        public void ApplyCityModifiersToCombat(IUnit attacker, IUnit defender, CombatInfo combatInfo) {
            if(attacker.Type == UnitType.City) {
                var attackerLocation = UnitPositionCanon.GetOwnerOfPossession(attacker);

                var cityAt = CityLocationCanon.GetPossessionsOfOwner(attackerLocation).FirstOrDefault();

                if(cityAt != null && combatInfo.CombatType == CombatType.Ranged) {
                    combatInfo.AttackerCombatModifier += CityModifiers.GarrisionedRangedCombatStrength.GetValueForCity(cityAt);
                }
            }
        }

        #endregion

        #endregion
        
    }

}
