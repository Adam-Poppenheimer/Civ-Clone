using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.AI {

    public class UnitCommandExecuter : IUnitCommandExecuter {

        #region instance fields and properties

        private DictionaryOfLists<IUnit, IUnitCommand> CommandsForUnit =
            new DictionaryOfLists<IUnit, IUnitCommand>();




        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public UnitCommandExecuter(
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            CoroutineInvoker = coroutineInvoker;
        }

        #endregion

        #region instance methods

        #region from IUnitCommandExecuter

        public List<IUnitCommand> GetCommandsForUnit(IUnit unit) {
            return CommandsForUnit[unit];
        }

        public void SetCommandsForUnit(IUnit unit, List<IUnitCommand> commands) {
            CommandsForUnit[unit] = commands;
        }

        public void ClearCommandsForUnit(IUnit unit) {
            CommandsForUnit[unit].Clear();
        }

        public void IterateAllCommands(Action postExecutionAction) {
            CoroutineInvoker.StartCoroutine(IterateAllCommands_Coroutine(postExecutionAction));
        }

        #endregion

        private IEnumerator IterateAllCommands_Coroutine(Action postExecutionAction) {
            HashSet<IUnit> unitsStillRunning = new HashSet<IUnit>(CommandsForUnit.Keys);

            while(unitsStillRunning.Count > 0) {
                foreach(var unit in unitsStillRunning.ToArray()) {
                    var commands = CommandsForUnit[unit];

                    var currentCommand = commands.FirstOrDefault();

                    if(currentCommand == null || currentCommand.Status == CommandStatus.Failed) {
                        CommandsForUnit.RemoveList(unit);
                        unitsStillRunning.Remove(unit);

                    }else if(currentCommand.Status == CommandStatus.Succeeded) {
                        commands.Remove(currentCommand);

                    }else if(currentCommand.Status == CommandStatus.NotStarted) {
                        currentCommand.StartExecution();
                    }
                }

                yield return null;
            }

            postExecutionAction();
        }

        #endregion

    }

}
