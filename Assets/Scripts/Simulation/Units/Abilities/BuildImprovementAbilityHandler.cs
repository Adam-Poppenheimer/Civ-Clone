using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Abilities {

    public class BuildImprovementAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IImprovementValidityLogic         ValidityLogic;
        private IUnitPositionCanon                UnitPositionCanon;
        private IEnumerable<IImprovementTemplate> AvailableTemplates;
        private IImprovementFactory               ImprovementFactory;
        private IImprovementLocationCanon         ImprovementLocationCanon;
        private IImprovementWorkLogic             ImprovementWorkLogic;

        #endregion

        #region constructors

        [Inject]
        public BuildImprovementAbilityHandler(
            IImprovementValidityLogic validityLogic, IUnitPositionCanon unitPositionCanon,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableTemplates,
            IImprovementFactory improvementFactory, IImprovementLocationCanon improvementLocationCanon,
            IImprovementWorkLogic improvementWorkLogic
        ){
            ValidityLogic            = validityLogic;
            UnitPositionCanon        = unitPositionCanon;
            AvailableTemplates       = availableTemplates;
            ImprovementFactory       = improvementFactory;
            ImprovementLocationCanon = improvementLocationCanon;
            ImprovementWorkLogic     = improvementWorkLogic;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            return command.Type == AbilityCommandType.BuildImprovement
                && (CanHandleWithExistingSite(command, unit) || CanHandleWithNewSite(command, unit));
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(!CanHandleCommandOnUnit(command, unit)) {
                throw new InvalidOperationException("Cannot handle command");
            }

            if(CanHandleWithExistingSite(command, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                unit.LockIntoConstruction();

                var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                improvementOnCell.WorkInvested += ImprovementWorkLogic.GetWorkOfUnitOnImprovement(unit, improvementOnCell);

                if(improvementOnCell.IsReadyToConstruct) {
                    improvementOnCell.Construct();
                }

            }else {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var templateOfName = GetTemplateOfName(command.ArgsToPass.FirstOrDefault());
                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                if(improvementAtLocation != null) {
                    ImprovementLocationCanon.ChangeOwnerOfPossession(improvementAtLocation, null);
                    improvementAtLocation.Destroy();
                }

                var newImprovement = ImprovementFactory.BuildImprovement(templateOfName, unitLocation);

                unit.LockIntoConstruction();

                newImprovement.WorkInvested += ImprovementWorkLogic.GetWorkOfUnitOnImprovement(unit, newImprovement);

                if(newImprovement.IsReadyToConstruct) {
                    newImprovement.Construct();
                }
            }
        }

        #endregion

        private bool CanHandleWithExistingSite(AbilityCommandRequest command, IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var templateOfName = GetTemplateOfName(command.ArgsToPass.FirstOrDefault());
            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            return templateOfName != null
                && improvementOnCell != null
                && templateOfName == improvementOnCell.Template
                && !improvementOnCell.IsConstructed
                && !improvementOnCell.IsPillaged;
        }

        private bool CanHandleWithNewSite(AbilityCommandRequest command, IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var templateOfName = GetTemplateOfName(command.ArgsToPass.FirstOrDefault());
            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            return templateOfName != null
                && ValidityLogic.IsTemplateValidForCell(templateOfName, unitLocation, false)
                && (improvementOnCell == null || improvementOnCell.Template != templateOfName);
        }

        private IImprovementTemplate GetTemplateOfName(string name) {
            return name == null ? null : AvailableTemplates.Where(template => template.name.Equals(name)).FirstOrDefault();
        }

        #endregion
        
    }

}
