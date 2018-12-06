using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units.Abilities {

    public class StartGoldenAgeAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IGoldenAgeCanon                               GoldenAgeCanon;

        #endregion

        #region constructors

        [Inject]
        public StartGoldenAgeAbilityHandler(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IGoldenAgeCanon goldenAgeCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            GoldenAgeCanon      = goldenAgeCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            int argAsInt;

            return command.Type == AbilityCommandType.StartGoldenAge
                && command.ArgsToPass.Count == 1
                && int.TryParse(command.ArgsToPass[0], out argAsInt);
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                int ageDuration = int.Parse(command.ArgsToPass[0]);

                if(GoldenAgeCanon.IsCivInGoldenAge(unitOwner)) {
                    GoldenAgeCanon.ChangeTurnsOfGoldenAgeForCiv(unitOwner, ageDuration);

                }else {
                    GoldenAgeCanon.StartGoldenAgeForCiv(unitOwner, ageDuration);
                }
            }else {
                throw new InvalidOperationException("Cannot handle command");
            }
        }

        #endregion

        #endregion
        
    }

}
