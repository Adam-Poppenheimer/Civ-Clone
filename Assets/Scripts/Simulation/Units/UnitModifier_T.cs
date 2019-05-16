using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Units {

    public class UnitModifier<T> : IUnitModifier<T> {

        #region internal types

        public class ExtractionData {

            public Func<ISocialPolicyBonusesData, T> PolicyBonusesExtractor;

            public Func<T, T, T> Aggregator;

            public T UnitaryValue;

        }

        #endregion

        #region instance fields and properties

        private ExtractionData DataForExtraction;




        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private ISocialPolicyCanon                            SocialPolicyCanon;

        #endregion

        #region constructors

        public UnitModifier(ExtractionData dataForExtraction) {
            DataForExtraction = dataForExtraction;
        }

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            ISocialPolicyCanon socialPolicyCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            SocialPolicyCanon   = socialPolicyCanon;
        }

        #region from IUnitModifier<T>

        public T GetValueForUnit(IUnit unit) {
            T retval = DataForExtraction.UnitaryValue;

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            foreach(var bonuses in SocialPolicyCanon.GetPolicyBonusesForCiv(unitOwner)) {
                retval = DataForExtraction.Aggregator(retval, DataForExtraction.PolicyBonusesExtractor(bonuses));
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
