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

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            return ability.CommandRequests.Any(CommandFilter);
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                var goldenAgeCommand = ability.CommandRequests.First(
                    command => command.CommandType == AbilityCommandType.StartGoldenAge
                );

                int ageDuration = int.Parse(goldenAgeCommand.ArgsToPass[0]);

                if(GoldenAgeCanon.IsCivInGoldenAge(unitOwner)) {
                    GoldenAgeCanon.ChangeTurnsOfGoldenAgeForCiv(unitOwner, ageDuration);

                }else {
                    GoldenAgeCanon.StartGoldenAgeForCiv(unitOwner, ageDuration);
                }

                return new AbilityExecutionResults(true, null);

            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        private bool CommandFilter(AbilityCommandRequest command) {
            int argAsInt;

            return command.CommandType == AbilityCommandType.StartGoldenAge
                && command.ArgsToPass.Count == 1
                && int.TryParse(command.ArgsToPass[0], out argAsInt);
        }

        #endregion
        
    }

}
