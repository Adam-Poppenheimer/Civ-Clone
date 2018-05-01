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

        #endregion

        #region constructors

        [Inject]
        public BuildImprovementAbilityHandler(
            IImprovementValidityLogic validityLogic, IUnitPositionCanon unitPositionCanon,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableTemplates,
            IImprovementFactory improvementFactory, IImprovementLocationCanon improvementLocationCanon
        ){
            ValidityLogic            = validityLogic;
            UnitPositionCanon        = unitPositionCanon;
            AvailableTemplates       = availableTemplates;
            ImprovementFactory       = improvementFactory;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            return CanHandleWithExistingSite(ability, unit) || CanHandleWithNewSite(ability, unit);
        }

        private bool CanHandleWithExistingSite(IAbilityDefinition ability, IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var templateOfName = GetTemplateOfName(GetRequestedImprovementName(ability));
            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            return templateOfName != null
                && improvementOnCell != null
                && templateOfName == improvementOnCell.Template
                && !improvementOnCell.IsConstructed
                && !improvementOnCell.IsPillaged;
        }

        private bool CanHandleWithNewSite(IAbilityDefinition ability, IUnit unit) {
            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            var templateOfName = GetTemplateOfName(GetRequestedImprovementName(ability));
            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

            return templateOfName != null
                && ValidityLogic.IsTemplateValidForCell(templateOfName, unitLocation)
                && (improvementOnCell == null || improvementOnCell.Template != templateOfName);
        }

        public AbilityExecutionResults TryHandleAbilityOnUnit(IAbilityDefinition ability, IUnit unit) {
            if(CanHandleWithExistingSite(ability, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                unit.LockIntoConstruction();

                var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                improvementOnCell.WorkInvested++;

                if(improvementOnCell.IsReadyToConstruct) {
                    improvementOnCell.Construct();
                }

                return new AbilityExecutionResults(true, null);

            }else if(CanHandleWithNewSite(ability, unit)) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var templateOfName = GetTemplateOfName(GetRequestedImprovementName(ability));
                var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(unitLocation).FirstOrDefault();

                if(improvementAtLocation != null) {
                    ImprovementLocationCanon.ChangeOwnerOfPossession(improvementAtLocation, null);
                    improvementAtLocation.Destroy();
                }

                var newImprovement = ImprovementFactory.BuildImprovement(templateOfName, unitLocation);

                unit.LockIntoConstruction();

                newImprovement.WorkInvested++;

                if(newImprovement.IsReadyToConstruct) {
                    newImprovement.Construct();
                }

                return new AbilityExecutionResults(true, null);
            }else {
                return new AbilityExecutionResults(false, null);
            }
        }

        #endregion

        public IImprovementTemplate GetTemplateOfName(string name) {
            return name == null ? null : AvailableTemplates.Where(template => template.name.Equals(name)).FirstOrDefault();
        }

        private string GetRequestedImprovementName(IAbilityDefinition ability) {
            var improvementCommands = ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.BuildImprovement);

            if(improvementCommands.Count() != 1) {
                return null;
            }else {
                return improvementCommands.First().ArgsToPass.FirstOrDefault();
            }
        }

        #endregion
        
    }

}
