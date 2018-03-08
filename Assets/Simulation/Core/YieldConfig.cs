using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Core {

    [CreateAssetMenu(menuName = "Civ Clone/Yield Config")]
    public class YieldConfig : ScriptableObject, IYieldConfig {

        #region instance fields and properties

        #region from IYieldConfig

        public Sprite FoodIcon {
            get { return _foodIcon; }
        }
        [SerializeField] private Sprite _foodIcon;

        public Sprite ProductionIcon {
            get { return _productionIcon; }
        }
        [SerializeField] private Sprite _productionIcon;

        public Sprite GoldIcon {
            get { return _goldIcon; }
        }
        [SerializeField] private Sprite _goldIcon;

        public Sprite CultureIcon {
            get { return _cultureIcon; }
        }
        [SerializeField] private Sprite _cultureIcon;

        public Sprite ScienceIcon {
            get { return _scienceIcon; }
        }
        [SerializeField] private Sprite _scienceIcon;

        public Color FoodColor {
            get { return _foodColor; }
        }
        [SerializeField] private Color _foodColor;

        public Color ProductionColor {
            get { return _productionColor; }
        }
        [SerializeField] private Color _productionColor;

        public Color GoldColor {
            get { return _goldColor; }
        }
        [SerializeField] private Color _goldColor;

        public Color CultureColor {
            get { return _cultureColor; }
        }
        [SerializeField] private Color _cultureColor;

        public Color ScienceColor {
            get { return _scienceColor; }
        }
        [SerializeField] private Color _scienceColor;

        #endregion

        #endregion

        #region instance methods

        #region from IYieldConfig

        public Sprite GetIconForResourceType(ResourceType type) {
            switch(type) {
                case ResourceType.Food:       return FoodIcon;
                case ResourceType.Production: return ProductionIcon;
                case ResourceType.Gold:       return GoldIcon;
                case ResourceType.Culture:    return CultureIcon;
                case ResourceType.Science:    return ScienceIcon;
                default: return null;
            }
        }

        public Color GetColorForResourceType(ResourceType type) {
            switch(type) {
                case ResourceType.Food:       return FoodColor;
                case ResourceType.Production: return ProductionColor;
                case ResourceType.Gold:       return GoldColor;
                case ResourceType.Culture:    return CultureColor;
                case ResourceType.Science:    return ScienceColor;
                default: return Color.magenta;
            }
        }

        #endregion

        #endregion

    }

}
