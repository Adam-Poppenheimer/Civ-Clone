using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets {

    public struct ResourceSummary {

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


        #endregion

    }

}
