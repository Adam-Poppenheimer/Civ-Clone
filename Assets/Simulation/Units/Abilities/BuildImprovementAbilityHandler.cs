using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Improvements;
using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units.Abilities {

    public class BuildImprovementAbilityHandler : IUnitAbilityHandler {


        #region instance fields and properties

        private IImprovementValidityLogic ValidityLogic;

        private IUnitPositionCanon UnitPositionCanon;

        private IEnumerable<IImprovementTemplate> AvailableTemplates;

        private IImprovementFactory ImprovementFactory;

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildImprovementAbilityHandler(IImprovementValidityLogic validityLogic, IUnitPositionCanon unitPositionCanon,
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

        #region from IUnitAbilityHandler

        public bool CanHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            var improvementName = GetRequestedImprovementName(ability);

            if(improvementName != null){
                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);
                var templateOfName = AvailableTemplates.Where(template => template.name.Equals(improvementName)).FirstOrDefault();
                
                return templateOfName != null
                    && ValidityLogic.IsTemplateValidForTile(templateOfName, unitLocation)
                    && ImprovementLocationCanon.CanPlaceImprovementOfTemplateAtLocation(templateOfName, unitLocation);
            }

            return false;
        }

        public bool TryHandleAbilityOnUnit(IUnitAbilityDefinition ability, IUnit unit) {
            if(CanHandleAbilityOnUnit(ability, unit)) {
                ImprovementFactory.Create(
                    AvailableTemplates.Where(template => template.name.Equals(GetRequestedImprovementName(ability))).First(),
                    UnitPositionCanon.GetOwnerOfPossession(unit)
                );
                return true;
            }else {
                return false;
            }
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
