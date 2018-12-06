using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Units.Abilities {

    public class GainFreeTechAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ITechCanon                                    TechCanon;

        #endregion

        #region constructors

        [Inject]
        public GainFreeTechAbilityHandler(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon, ITechCanon techCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            TechCanon           = techCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            return command.Type == AbilityCommandType.GainFreeTech;
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                TechCanon.AddFreeTechToCiv(unitOwner);
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
