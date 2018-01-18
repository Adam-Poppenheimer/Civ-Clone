using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.UI.StateMachine {

    public static class ClickedAnywhereTransitionLogic {

        #region static fields and properties

        private static List<Type> NonDefaultingTypes = new List<Type>() {
            typeof(ICity),
            typeof(HexGridChunk),
            typeof(Canvas),
            typeof(IUnit)
        };

        #endregion

        #region static methods

        public static bool ShouldClickCauseReturn(PointerEventData eventData) {
            var raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach(var raycastResult in raycastResults) {
                var clickedObject = raycastResult.gameObject;

                foreach(var type in NonDefaultingTypes) {
                    if(clickedObject.GetComponentInParent(type) != null) {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion
    }

}
