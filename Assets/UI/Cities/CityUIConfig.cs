﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.UI.Cities {

    [CreateAssetMenu(menuName = "Civ Clone/City UI Config")]
    public class CityUIConfig : ScriptableObject, ICityUIConfig {

        #region instance fields and properties

        #region from ICityUIConfig

        public Material OccupiedSlotMaterial {
            get { return _occupiedSlotMaterial; }
        }
        [SerializeField] private Material _occupiedSlotMaterial;

        public Material UnoccupiedSlotMaterial {
            get { return _unoccupiedSlotMaterial; }
        }
        [SerializeField] private Material _unoccupiedSlotMaterial;

        #endregion

        #endregion
        
    }

}