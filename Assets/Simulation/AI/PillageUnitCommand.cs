using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.AI {

    public class PillageUnitCommand : IUnitCommand {

        #region instance fields and properties

        #region from IUnitCommand

        public CommandStatus Status { get; private set; }

        #endregion

        public IUnit Pillager;




        
        private IAbilityExecuter AbilityExecuter;

        #endregion

        #region constructors

        [Inject]
        public PillageUnitCommand(IAbilityExecuter abilityExecuter) {
            AbilityExecuter = abilityExecuter;
        }

        #endregion

        #region instance methods

        #region from IUnitCommand

        public void StartExecution() {
            if(Pillager == null) {
                throw new InvalidOperationException("Cannot execute while Pillager is null");
            }

            Status = CommandStatus.Running;

            var pillageAbilities = Pillager.Abilities.Where(
                ability => ability.CommandRequests.Any(request => request.Type == AbilityCommandType.Pillage)
            );

            foreach(var pillageAbility in pillageAbilities) {
                if( AbilityExecuter.CanExecuteAbilityOnUnit(pillageAbility, Pillager)) {
                    AbilityExecuter.ExecuteAbilityOnUnit   (pillageAbility, Pillager);

                    Status = CommandStatus.Succeeded;

                    return;
                }
            }

            Status = CommandStatus.Failed;
        }

        #endregion

        #endregion

    }

}
