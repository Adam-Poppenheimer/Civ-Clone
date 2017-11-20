using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

namespace Assets.Cities.UI {

    public class WorkerSlotDisplay : MonoBehaviour, IWorkerSlotDisplay {

        #region instance fields and properties

        #region from IWorkerSlotDisplay

        public bool IsOccupied { get; set; }

        #endregion

        public Image SlotImage {
            get { return _slotImage; }
            set { _slotImage = value; }
        }
        [SerializeField] private Image _slotImage;

        private ICityUIConfig Config;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityUIConfig config) {
            Config = config;
        }

        #region from IWorkerSlotDisplay

        public void Refresh() {
            SlotImage.material = IsOccupied ? Config.OccupiedSlotMaterial : Config.UnoccupiedSlotMaterial;
        }

        #endregion

        #endregion

    }

}
