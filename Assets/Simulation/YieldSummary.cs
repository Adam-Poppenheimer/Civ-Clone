using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation {

    [Serializable]
    public struct YieldSummary {

        #region static fields and properties

        public static YieldSummary Empty {
            get { return new YieldSummary(); }
        }

        public static YieldSummary Ones {
            get { return new YieldSummary(food: 1, production: 1, gold: 1, culture: 1, science: 1); }
        }

        #endregion

        #region instance fields and properties

        public float Total {
            get { return Food + Production + Gold + Culture + Science; }
        }

        [SerializeField] private float Food;
        [SerializeField] private float Production;
        [SerializeField] private float Gold;
        [SerializeField] private float Culture;
        [SerializeField] private float Science;

        #endregion

        #region indexers

        public float this[YieldType index] {
            get {
                switch(index) {
                    case YieldType.Food:       return Food;
                    case YieldType.Production: return Production;
                    case YieldType.Gold:       return Gold;
                    case YieldType.Culture:    return Culture;
                    case YieldType.Science:    return Science;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch(index) {
                    case YieldType.Food:       Food       = value; break;
                    case YieldType.Production: Production = value; break;
                    case YieldType.Gold:       Gold       = value; break;
                    case YieldType.Culture:    Culture    = value; break;
                    case YieldType.Science:    Science    = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region constructors

        public YieldSummary(
            float food = 0, float production = 0, float gold = 0,
            float culture = 0, float science = 0
        ){
            Food       = food;
            Production = production;
            Gold       = gold;
            Culture    = culture;
            Science    = science;
        }

        public YieldSummary(YieldSummary other) {
            Food       = other.Food;
            Production = other.Production;
            Gold       = other.Gold;
            Culture    = other.Culture;
            Science    = other.Science;
        }

        #endregion

        #region operators

        public static YieldSummary operator +(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:       firstSummary.Food       + secondSummary.Food,
                production: firstSummary.Production + secondSummary.Production,
                gold:       firstSummary.Gold       + secondSummary.Gold,
                culture:    firstSummary.Culture    + secondSummary.Culture,
                science:    firstSummary.Science    + secondSummary.Science
            );
        }

        public static YieldSummary operator -(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:       firstSummary.Food       - secondSummary.Food,
                production: firstSummary.Production - secondSummary.Production,
                gold:       firstSummary.Gold       - secondSummary.Gold,
                culture:    firstSummary.Culture    - secondSummary.Culture,
                science:    firstSummary.Science    - secondSummary.Science
            );
        }

        public static YieldSummary operator -(YieldSummary summary) {
            return new YieldSummary(
                food:       -summary.Food,
                production: -summary.Production,
                gold:       -summary.Gold,
                culture:    -summary.Culture,
                science:    -summary.Science
            );
        }

        public static YieldSummary operator *(YieldSummary summary, int coefficient) {
            return new YieldSummary(
                food:       summary.Food       * coefficient,
                production: summary.Production * coefficient,
                gold:       summary.Gold       * coefficient,
                culture:    summary.Culture    * coefficient,
                science:    summary.Science    * coefficient
            );
        }

        public static YieldSummary operator *(YieldSummary summary, float coefficient) {
            return new YieldSummary(
                food:       summary.Food       * coefficient,
                production: summary.Production * coefficient,
                gold:       summary.Gold       * coefficient,
                culture:    summary.Culture    * coefficient,
                science:    summary.Science    * coefficient
            );
        }

        public static YieldSummary operator *(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:       firstSummary.Food       * secondSummary.Food,
                production: firstSummary.Production * secondSummary.Production,
                gold:       firstSummary.Gold       * secondSummary.Gold,
                culture:    firstSummary.Culture    * secondSummary.Culture,
                science:    firstSummary.Science    * secondSummary.Science
            );
        }

        public static YieldSummary operator /(YieldSummary summary, float divisor) {
            return new YieldSummary(
                food:       summary.Food       / divisor,
                production: summary.Production / divisor,
                gold:       summary.Gold       / divisor,
                culture:    summary.Culture    / divisor,
                science:    summary.Science    / divisor
            );
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format("Food: {0} | Production: {1} | Gold: {2} | Culture: {3} | Science {4}",
                Food, Production, Gold, Culture, Science
            );
        }

        #endregion

        #endregion

    }

}
