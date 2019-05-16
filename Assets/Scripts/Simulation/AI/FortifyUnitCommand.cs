using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.AI {

    public class FortifyUnitCommand : IUnitCommand {

        #region instance fields and properties

        #region from IUnitCommand

        public CommandStatus Status { get; private set; }

        #endregion

        public IUnit UnitToFortify { get; set; }




        private IUnitFortificationLogic FortificationLogic;
        private IAbilityExecuter        AbilityExecuter;

        #endregion

        #region constructors

        [Inject]
        public FortifyUnitCommand(
            IUnitFortificationLogic fortificationLogic, IAbilityExecuter abilityExecuter
        ) {
            FortificationLogic = fortificationLogic;
            AbilityExecuter    = abilityExecuter;
        }

        #endregion

        #region instance methods

        #region from IUnitCommand

        public void StartExecution() {
            if(UnitToFortify == null) {
                throw new InvalidOperationException("UnitToFortify cannot be null");
            }

            Status = CommandStatus.Running;

            if(FortificationLogic.GetFortificationStatusForUnit(UnitToFortify)) {
                Status = CommandStatus.Succeeded;

            }else {
                var fortifyAbility = UnitToFortify.Abilities.FirstOrDefault(
                    ability => ability.CommandRequests.Any(
                        request => request.Type == AbilityCommandType.Fortify
                    )
                );

                if(fortifyAbility == null || !AbilityExecuter.CanExecuteAbilityOnUnit(fortifyAbility, UnitToFortify)) {
                    Status = CommandStatus.Failed;

                }else {
                    AbilityExecuter.ExecuteAbilityOnUnit(fortifyAbility, UnitToFortify);

                    Status = FortificationLogic.GetFortificationStatusForUnit(UnitToFortify)
                           ? CommandStatus.Succeeded : CommandStatus.Failed;
                }
            }
        }

        #endregion

        #endregion
        
    }

}
