using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.Units {

    public class UnitIconSlot : MonoBehaviour {

        #region internal types

        public class Pool : MonoMemoryPool<UnitIconSlot> {

            protected override void Reinitialize(UnitIconSlot item) {
                item.Clear();
            }

        }

        #endregion

        #region instance fields and properties

        public RectTransform RectTransform {
            get {
                if(_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }
        private RectTransform _rectTransform;

        #endregion

        #region instance methods

        public void AddIconToSlot(UnitMapIcon icon) {
            icon.transform.SetParent(transform, false);
        }

        public void Clear() {
            for(int i = transform.childCount - 1; i >= 0; i--) {
                transform.GetChild(i).SetParent(null, false);
            }
        }

        #endregion

    }

}
