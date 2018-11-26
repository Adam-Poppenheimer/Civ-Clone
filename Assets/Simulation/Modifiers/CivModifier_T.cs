using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Simulation.Modifiers {

    public class CivModifier<T> : ICivModifier<T> {

        #region internal types

        public class ExtractionData {

            public Func<ISocialPolicyBonusesData, T> Extractor;

            public Func<T, T, T> Aggregator;

            public T UnitaryValue;

        }

        #endregion

        #region instance fields and properties

        private ISocialPolicyBonusLogic SocialPolicyBonusLogic;

        private ExtractionData DataForExtraction;

        #endregion

        #region constructors

        public CivModifier(ExtractionData dataForExtraction) {
            DataForExtraction = dataForExtraction;
        }

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ISocialPolicyBonusLogic socialPolicyBonusLogic) {
            SocialPolicyBonusLogic = socialPolicyBonusLogic;
        }

        #region from ICivModifier<T>

        public T GetValueForCiv(ICivilization civ) {
            return DataForExtraction.Aggregator(
                DataForExtraction.UnitaryValue,
                SocialPolicyBonusLogic.ExtractBonusFromCiv(
                    civ, DataForExtraction.Extractor, DataForExtraction.Aggregator
                )
            );
        }

        #endregion

        #endregion

    }

}
