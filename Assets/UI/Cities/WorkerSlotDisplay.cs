using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace Assets.UI.Cities {

    public class WorkerSlotDisplay : MonoBehaviour, IWorkerSlotDisplay {

        #region instance fields and properties

        public Image SlotImage {
            get { return _slotImage; }
            set { _slotImage = value; }
        }
        [SerializeField] private Image _slotImage;

        #endregion

        #region instance methods

        public void DisplayOccupationStatus(bool isOccupied, ICityUIConfig config) {
            SlotImage.material = isOccupied ? config.OccupiedSlotMaterial : config.UnoccupiedSlotMaterial;
        }

        #endregion

    }

}
