using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    [Serializable, CreateAssetMenu(menuName = "Civ Clone/Map Size Category")]
    public class MapSizeCategory : ScriptableObject, IMapSizeCategory {

        #region instance fields and properties

        public Vector2 DimensionsInChunks {
            get { return _dimensionsInChunks; }
        }
        [SerializeField] private Vector2 _dimensionsInChunks;

        public ReadOnlyCollection<int> ValidCivCounts {
            get { return _validCivCounts.AsReadOnly(); }
        }
        [SerializeField] private List<int> _validCivCounts;

        public int IdealCivCount {
            get { return _idealCivCount; }
        }
        [SerializeField] private int _idealCivCount;

        #endregion

    }

}
