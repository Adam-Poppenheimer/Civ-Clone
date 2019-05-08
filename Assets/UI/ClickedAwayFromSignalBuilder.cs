using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.UI {

    public static class SignalBuilderUtility {

        #region static methods

        public static UniRx.IObservable<Unit> BuildMouseDeselectedSignal(GameObject targetObject, UniRx.IObservable<Unit> clickedAnywhereSignal) {
            return clickedAnywhereSignal.Where(BuildMouseDeselectionPredicate(targetObject));
        }

        private static Func<Unit, bool> BuildMouseDeselectionPredicate(GameObject targetObject) {
            return delegate (Unit unit) {
                var selectedObject = EventSystem.current.currentSelectedGameObject;
                
                if(selectedObject != null) {
                    var notSelected = !selectedObject.transform.IsChildOf(targetObject.transform);
                    return notSelected;
                }else {
                    return true;
                }   
            };
        }

        #endregion

    }

}
