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

        #endregion

        #region instance fields and properties

        public int Total {
            get { return Food + Production + Gold + Culture; }
        }

        [SerializeField] private int Food;
        [SerializeField] private int Production;
        [SerializeField] private int Gold;
        [SerializeField] private int Culture;

        #endregion

        #region indexers

        public int this[ResourceType index] {
            get {
                switch(index) {
                    case ResourceType.Food:       return Food;
                    case ResourceType.Production: return Production;
                    case ResourceType.Gold:       return Gold;
                    case ResourceType.Culture:    return Culture;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch(index) {
                    case ResourceType.Food:       Food       = value; break;
                    case ResourceType.Production: Production = value; break;
                    case ResourceType.Gold:       Gold       = value; break;
                    case ResourceType.Culture:    Culture    = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        #endregion

        #region constructors

        public ResourceSummary(int food = 0, int production = 0, int gold = 0, int culture = 0) {
            Food = food;
            Production = production;
            Gold = gold;
            Culture = culture;
        }

        public ResourceSummary(ResourceSummary other) {
            Food = other.Food;
            Production = other.Production;
            Gold = other.Gold;
            Culture = other.Culture;
        }

        #endregion

        #region operators

        public static ResourceSummary operator +(ResourceSummary firstSummary, ResourceSummary secondSummary) {
            return new ResourceSummary(
                food:       firstSummary.Food       + secondSummary.Food,
                production: firstSummary.Production + secondSummary.Production,
                gold:       firstSummary.Gold       + secondSummary.Gold,
                culture:    firstSummary.Culture    + secondSummary.Culture
            );
        }

        public static ResourceSummary operator -(ResourceSummary firstSummary, ResourceSummary secondSummary) {
            return new ResourceSummary(
                food:       firstSummary.Food       - secondSummary.Food,
                production: firstSummary.Production - secondSummary.Production,
                gold:       firstSummary.Gold       - secondSummary.Gold,
                culture:    firstSummary.Culture    - secondSummary.Culture
            );
        }

        public static ResourceSummary operator -(ResourceSummary summary) {
            return new ResourceSummary(
                food:       -summary.Food,
                production: -summary.Production,
                gold:       -summary.Gold,
                culture:    -summary.Culture
            );
        }

        public static ResourceSummary operator *(ResourceSummary summary, int coefficient) {
            return new ResourceSummary(
                food:       summary.Food       * coefficient,
                production: summary.Production * coefficient,
                gold:       summary.Gold       * coefficient,
                culture:    summary.Culture    * coefficient
            );
        }

        public static ResourceSummary operator *(ResourceSummary summary, float coefficient) {
            return new ResourceSummary(
                food:       Mathf.RoundToInt(summary.Food       * coefficient),
                production: Mathf.RoundToInt(summary.Production * coefficient),
                gold:       Mathf.RoundToInt(summary.Gold       * coefficient),
                culture:    Mathf.RoundToInt(summary.Culture    * coefficient)
            );
        }

        public static ResourceSummary operator /(ResourceSummary summary, float divisor) {
            return new ResourceSummary(
                food:       Mathf.RoundToInt(summary.Food       / divisor),
                production: Mathf.RoundToInt(summary.Production / divisor),
                gold:       Mathf.RoundToInt(summary.Gold       / divisor),
                culture:    Mathf.RoundToInt(summary.Culture    / divisor)
            );
        }

        #endregion

    }

}
