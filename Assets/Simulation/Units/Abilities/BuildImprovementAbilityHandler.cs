using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.Units.Abilities {

    public class BuildImprovementAbilityHandler : IUnitAbilityHandler {


        #region instance fields and properties

        private IImprovementValidityLogic ValidityLogic;

        private IUnitPositionCanon UnitPositionCanon;

        private IEnumerable<IImprovementTemplate> AvailableTemplates;

        private IImprovementFactory ImprovementFactory;

        #endregion

        #region constructors

        [Inject]
        public BuildImprovementAbilityHandler(IImprovementValidityLogic validityLogic, IUnitPositionCanon unitPositionCanon,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableTemplates,
            IImprovementFactory improvementFactory            
        ){
            ValidityLogic      = validityLogic;
            UnitPositionCanon  = unitPositionCanon;
            AvailableTemplates = availableTemplates;
            ImprovementFactory = improvementFactory;
        }

        #endregion

        #region instance methods

        #region from IUnitAbilityHandler

        public bool CanHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            var improvementName = GetRequestedImprovementName(ability);

            if(improvementName != null){
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);
                var templateOfName = AvailableTemplates.Where(template => template.name.Equals(improvementName)).FirstOrDefault();
                
                return templateOfName != null && ValidityLogic.IsTemplateValidForTile(templateOfName, unitLocation);
            }

            return false;
        }

        public bool TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            var improvementName = GetRequestedImprovementName(ability);

            if(improvementName != null) {
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);
                var templateOfName = AvailableTemplates.Where(template => template.name.Equals(improvementName)).FirstOrDefault();
                
                if(templateOfName != null && ValidityLogic.IsTemplateValidForTile(templateOfName, unitLocation)) {
                    ImprovementFactory.Create(templateOfName, unitLocation);
                    return true;
                }
            }

            return false;
        }

        #endregion

        private string GetRequestedImprovementName(IUnitAbilityDefinition ability) {
            var improvementCommands = ability.CommandRequests.Where(request => request.CommandType == AbilityCommandType.BuildImprovement);

            if(improvementCommands.Count() == 0) {
                return null;
            }else if(improvementCommands.Count() > 1) {
                throw new InvalidOperationException("It's not valid to have two Build Improvement commands on the same ability");
            }else {
                return improvementCommands.First().ArgsToPass.FirstOrDefault();
            }
        }

        #endregion
        
    }

}
