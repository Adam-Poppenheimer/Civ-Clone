using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation {

    [Serializable]
    public struct ResourceSummary {

        #region static fields and properties

        public static ResourceSummary Empty {
            get { return new ResourceSummary(); }
        }

        public static ResourceSummary Ones {
            get { return new ResourceSummary(food: 1, production: 1, gold: 1, culture: 1, science: 1); }
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

        public float this[ResourceType index] {
            get {
                switch(index) {
                    case ResourceType.Food:       return Food;
                    case ResourceType.Production: return Production;
                    case ResourceType.Gold:       return Gold;
                    case ResourceType.Culture:    return Culture;
                    case ResourceType.Science:    return Science;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch(index) {
                    case ResourceType.Food:       Food       = value; break;
                    case ResourceType.Production: Production = value; break;
                    case ResourceType.Gold:       Gold       = value; break;
                    case ResourceType.Culture:    Culture    = value; break;
                    case ResourceType.Science:    Science    = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region constructors

        public ResourceSummary(
            float food = 0, float production = 0, float gold = 0,
            float culture = 0, float science = 0
        ){
            Food       = food;
            Production = production;
            Gold       = gold;
            Culture    = culture;
            Science    = science;
        }

        public ResourceSummary(ResourceSummary other) {
            Food       = other.Food;
            Production = other.Production;
            Gold       = other.Gold;
            Culture    = other.Culture;
            Science    = other.Science;
        }

        #endregion

        #region operators

        public static ResourceSummary operator +(ResourceSummary firstSummary, ResourceSummary secondSummary) {
            return new ResourceSummary(
                food:       firstSummary.Food       + secondSummary.Food,
                production: firstSummary.Production + secondSummary.Production,
                gold:       firstSummary.Gold       + secondSummary.Gold,
                culture:    firstSummary.Culture    + secondSummary.Culture,
                science:    firstSummary.Science    + secondSummary.Science
            );
        }

        public static ResourceSummary operator -(ResourceSummary firstSummary, ResourceSummary secondSummary) {
            return new ResourceSummary(
                food:       firstSummary.Food       - secondSummary.Food,
                production: firstSummary.Production - secondSummary.Production,
                gold:       firstSummary.Gold       - secondSummary.Gold,
                culture:    firstSummary.Culture    - secondSummary.Culture,
                science:    firstSummary.Science    - secondSummary.Science
            );
        }

        public static ResourceSummary operator -(ResourceSummary summary) {
            return new ResourceSummary(
                food:       -summary.Food,
                production: -summary.Production,
                gold:       -summary.Gold,
                culture:    -summary.Culture,
                science:    -summary.Science
            );
        }

        public static ResourceSummary operator *(ResourceSummary summary, int coefficient) {
            return new ResourceSummary(
                food:       summary.Food       * coefficient,
                production: summary.Production * coefficient,
                gold:       summary.Gold       * coefficient,
                culture:    summary.Culture    * coefficient,
                science:    summary.Science    * coefficient
            );
        }

        public static ResourceSummary operator *(ResourceSummary summary, float coefficient) {
            return new ResourceSummary(
                food:       summary.Food       * coefficient,
                production: summary.Production * coefficient,
                gold:       summary.Gold       * coefficient,
                culture:    summary.Culture    * coefficient,
                science:    summary.Science    * coefficient
            );
        }

        public static ResourceSummary operator *(ResourceSummary firstSummary, ResourceSummary secondSummary) {
            return new ResourceSummary(
                food:       firstSummary.Food       * secondSummary.Food,
                production: firstSummary.Production * secondSummary.Production,
                gold:       firstSummary.Gold       * secondSummary.Gold,
                culture:    firstSummary.Culture    * secondSummary.Culture,
                science:    firstSummary.Science    * secondSummary.Science
            );
        }

        public static ResourceSummary operator /(ResourceSummary summary, float divisor) {
            return new ResourceSummary(
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
