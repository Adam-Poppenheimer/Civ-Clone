using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Promotions {

    [CreateAssetMenu(menuName = "Civ Clone/Promotion")]
    public class Promotion : ScriptableObject, IPromotion {

        #region instance fields and properties

        #region from IPromotion

        public string Name {
            get { return name; }
        }

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        #endregion

        [SerializeField] private List<PromotionArgType> Args;

        [SerializeField] private float Float;

        [SerializeField] private int Int;

        [SerializeField] private TerrainType Terrain;

        [SerializeField] private TerrainShape Shape;

        [SerializeField] private TerrainFeature Feature;

        [SerializeField] private UnitType UnitType;        

        #endregion

        #region instance methods

        #region from IPromotion

        public bool HasArg(PromotionArgType arg) {
            return Args.Contains(arg);
        }

        public float GetFloat(){
            return Float;
        }

        public int  GetInt(){
            return Int;
        }

        public TerrainFeature GetFeature(){
            return Feature;
        }

        public TerrainType GetTerrain(){
            return Terrain;
        }

        public TerrainShape GetShape(){
            return Shape;
        }

        public UnitType GetUnitType(){
            return UnitType;
        }

        #endregion

        #endregion

    }

}
