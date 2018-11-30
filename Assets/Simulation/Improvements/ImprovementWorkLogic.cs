using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Improvements {

    public class ImprovementWorkLogic : IImprovementWorkLogic {

        #region instance fields and properties

        private ICivModifiers                                 CivModifiers;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementWorkLogic(
            ICivModifiers civModifiers, IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon
        ) {
            CivModifiers        = civModifiers;
            UnitPossessionCanon = unitPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementWorkLogic

        public float GetWorkOfUnitOnImprovement(IUnit unit, IImprovement improvement) {
            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            return 1 * CivModifiers.ImprovementBuildSpeed.GetValueForCiv(unitOwner);
        }

        #endregion

        #endregion
        
    }

}
