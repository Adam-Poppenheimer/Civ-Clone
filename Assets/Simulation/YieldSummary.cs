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
            get {
                return new YieldSummary(
                    food: 1f, production: 1f, gold: 1f, culture: 1f, science: 1f,
                    greatArtist: 1f, greatEngineer: 1f, greatMerchant: 1f,
                    greatScientist: 1f
                );
            }
        }

        #endregion

        #region instance fields and properties

        public float Total {
            get {
                return Food + Production + Gold + Culture + Science + GreatArtist +
                       GreatEngineer + GreatMerchant + GreatScientist;
            }
        }

        [SerializeField] private float Food;
        [SerializeField] private float Production;
        [SerializeField] private float Gold;
        [SerializeField] private float Culture;
        [SerializeField] private float Science;
        [SerializeField] private float GreatArtist;
        [SerializeField] private float GreatEngineer;
        [SerializeField] private float GreatMerchant;
        [SerializeField] private float GreatScientist;

        #endregion

        #region indexers

        public float this[YieldType index] {
            get {
                switch(index) {
                    case YieldType.Food:           return Food;
                    case YieldType.Production:     return Production;
                    case YieldType.Gold:           return Gold;
                    case YieldType.Culture:        return Culture;
                    case YieldType.Science:        return Science;
                    case YieldType.GreatArtist:    return GreatArtist;
                    case YieldType.GreatEngineer:  return GreatEngineer;
                    case YieldType.GreatMerchant:  return GreatMerchant;
                    case YieldType.GreatScientist: return GreatScientist;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch(index) {
                    case YieldType.Food:           Food           = value; break;
                    case YieldType.Production:     Production     = value; break;
                    case YieldType.Gold:           Gold           = value; break;
                    case YieldType.Culture:        Culture        = value; break;
                    case YieldType.Science:        Science        = value; break;
                    case YieldType.GreatArtist:    GreatArtist    = value; break;
                    case YieldType.GreatEngineer:  GreatEngineer  = value; break;
                    case YieldType.GreatMerchant:  GreatMerchant  = value; break;
                    case YieldType.GreatScientist: GreatScientist = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region constructors

        public YieldSummary(
            float food = 0f, float production = 0f, float gold = 0f,
            float culture = 0f, float science = 0f, float greatArtist = 0f,
            float greatEngineer = 0f, float greatMerchant = 0f,
            float greatScientist = 0f
        ){
            Food           = food;
            Production     = production;
            Gold           = gold;
            Culture        = culture;
            Science        = science;
            GreatArtist    = greatArtist;
            GreatEngineer  = greatEngineer;
            GreatMerchant  = greatMerchant;
            GreatScientist = greatScientist;
        }

        public YieldSummary(YieldSummary other) {
            Food           = other.Food;
            Production     = other.Production;
            Gold           = other.Gold;
            Culture        = other.Culture;
            Science        = other.Science;
            GreatArtist    = other.GreatArtist;
            GreatEngineer  = other.GreatEngineer;
            GreatMerchant  = other.GreatMerchant;
            GreatScientist = other.GreatScientist;
        }

        #endregion

        #region operators

        public static YieldSummary operator +(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:           firstSummary.Food           + secondSummary.Food,
                production:     firstSummary.Production     + secondSummary.Production,
                gold:           firstSummary.Gold           + secondSummary.Gold,
                culture:        firstSummary.Culture        + secondSummary.Culture,
                science:        firstSummary.Science        + secondSummary.Science,
                greatArtist:    firstSummary.GreatArtist    + secondSummary.GreatArtist,
                greatEngineer:  firstSummary.GreatEngineer  + secondSummary.GreatEngineer,
                greatMerchant:  firstSummary.GreatMerchant  + secondSummary.GreatMerchant,
                greatScientist: firstSummary.GreatScientist + secondSummary.GreatScientist
            );
        }

        public static YieldSummary operator -(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:           firstSummary.Food           - secondSummary.Food,
                production:     firstSummary.Production     - secondSummary.Production,
                gold:           firstSummary.Gold           - secondSummary.Gold,
                culture:        firstSummary.Culture        - secondSummary.Culture,
                science:        firstSummary.Science        - secondSummary.Science,
                greatArtist:    firstSummary.GreatArtist    - secondSummary.GreatArtist,
                greatEngineer:  firstSummary.GreatEngineer  - secondSummary.GreatEngineer,
                greatMerchant:  firstSummary.GreatMerchant  - secondSummary.GreatMerchant,
                greatScientist: firstSummary.GreatScientist - secondSummary.GreatScientist
            );
        }

        public static YieldSummary operator -(YieldSummary summary) {
            return new YieldSummary(
                food:           -summary.Food,
                production:     -summary.Production,
                gold:           -summary.Gold,
                culture:        -summary.Culture,
                science:        -summary.Science,
                greatArtist:    -summary.GreatArtist,
                greatEngineer:  -summary.GreatEngineer,
                greatMerchant:  -summary.GreatMerchant,
                greatScientist: -summary.GreatScientist
            );
        }

        public static YieldSummary operator *(YieldSummary summary, int coefficient) {
            return new YieldSummary(
                food:           summary.Food           * coefficient,
                production:     summary.Production     * coefficient,
                gold:           summary.Gold           * coefficient,
                culture:        summary.Culture        * coefficient,
                science:        summary.Science        * coefficient,
                greatArtist:    summary.GreatArtist    * coefficient,
                greatEngineer:  summary.GreatEngineer  * coefficient,
                greatMerchant:  summary.GreatMerchant  * coefficient,
                greatScientist: summary.GreatScientist * coefficient
            );
        }

        public static YieldSummary operator *(YieldSummary summary, float coefficient) {
            return new YieldSummary(
                food:           summary.Food           * coefficient,
                production:     summary.Production     * coefficient,
                gold:           summary.Gold           * coefficient,
                culture:        summary.Culture        * coefficient,
                science:        summary.Science        * coefficient,
                greatArtist:    summary.GreatArtist    * coefficient,
                greatEngineer:  summary.GreatEngineer  * coefficient,
                greatMerchant:  summary.GreatMerchant  * coefficient,
                greatScientist: summary.GreatScientist * coefficient
            );
        }

        public static YieldSummary operator *(YieldSummary firstSummary, YieldSummary secondSummary) {
            return new YieldSummary(
                food:       firstSummary.Food       * secondSummary.Food,
                production: firstSummary.Production * secondSummary.Production,
                gold:       firstSummary.Gold       * secondSummary.Gold,
                culture:    firstSummary.Culture    * secondSummary.Culture,
                science:    firstSummary.Science    * secondSummary.Science,
                greatArtist:    firstSummary.GreatArtist    * secondSummary.GreatArtist,
                greatEngineer:  firstSummary.GreatEngineer  * secondSummary.GreatEngineer,
                greatMerchant:  firstSummary.GreatMerchant  * secondSummary.GreatMerchant,
                greatScientist: firstSummary.GreatScientist * secondSummary.GreatScientist
            );
        }

        public static YieldSummary operator /(YieldSummary summary, float divisor) {
            return new YieldSummary(
                food:           summary.Food           / divisor,
                production:     summary.Production     / divisor,
                gold:           summary.Gold           / divisor,
                culture:        summary.Culture        / divisor,
                science:        summary.Science        / divisor,
                greatArtist:    summary.GreatArtist    / divisor,
                greatEngineer:  summary.GreatEngineer  / divisor,
                greatMerchant:  summary.GreatMerchant  / divisor,
                greatScientist: summary.GreatScientist / divisor
            );
        }

        #endregion

        #region instance methods

        #region from Object

        public override string ToString() {
            return string.Format(
                "Food: {0} | Production: {1} | Gold: {2} | Culture: {3} | Science {4} | " + 
                "GreatArtist {5} | greatEngineer {6} | greatMerchant {7} | greatScientist {8}",
                Food, Production, Gold, Culture, Science,
                GreatArtist, GreatEngineer, GreatMerchant, GreatScientist
            );
        }

        #endregion

        public bool Contains(YieldSummary other) {
            return Food           >= other.Food
                && Production     >= other.Production
                && Gold           >= other.Gold
                && Culture        >= other.Culture
                && Science        >= other.Science
                && GreatArtist    >= other.GreatArtist
                && GreatEngineer  >= other.GreatEngineer
                && GreatMerchant  >= other.GreatMerchant
                && GreatScientist >= other.GreatScientist;
        }

        #endregion

    }

}
