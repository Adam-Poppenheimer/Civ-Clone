using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Core {

    [CreateAssetMenu(menuName = "Civ Clone/Core/Config")]
    public class CoreConfig : ScriptableObject, ICoreConfig {

        #region instance fields and properties

        #region from IYieldConfig

        public Sprite FoodIcon {
            get { return _foodIcon; }
        }
        [SerializeField] private Sprite _foodIcon = null;

        public Sprite ProductionIcon {
            get { return _productionIcon; }
        }
        [SerializeField] private Sprite _productionIcon = null;

        public Sprite GoldIcon {
            get { return _goldIcon; }
        }
        [SerializeField] private Sprite _goldIcon = null;

        public Sprite CultureIcon {
            get { return _cultureIcon; }
        }
        [SerializeField] private Sprite _cultureIcon = null;

        public Sprite ScienceIcon {
            get { return _scienceIcon; }
        }
        [SerializeField] private Sprite _scienceIcon = null;

        public Sprite GreatPersonIcon {
            get { return _greatPersonYieldIcon; }
        }
        [SerializeField] private Sprite _greatPersonYieldIcon = null;

        public Sprite YieldModificationIcon {
            get { return _yieldModificationIcon; }
        }
        [SerializeField] private Sprite _yieldModificationIcon = null;


        public Color FoodColor {
            get { return _foodColor; }
        }
        [SerializeField] private Color _foodColor = Color.clear;

        public Color ProductionColor {
            get { return _productionColor; }
        }
        [SerializeField] private Color _productionColor = Color.clear;

        public Color GoldColor {
            get { return _goldColor; }
        }
        [SerializeField] private Color _goldColor = Color.clear;

        public Color CultureColor {
            get { return _cultureColor; }
        }
        [SerializeField] private Color _cultureColor = Color.clear;

        public Color ScienceColor {
            get { return _scienceColor; }
        }
        [SerializeField] private Color _scienceColor = Color.clear;

        public Color GreatPersonColor {
            get { return _greatPersonColor; }
        }
        [SerializeField] private Color _greatPersonColor = Color.clear;


        public Color PositiveHappinessColor {
            get { return _positiveHappinessColor; }
        }
        [SerializeField] private Color _positiveHappinessColor = Color.clear;

        public Color NegativeHappinessColor {
            get { return _negativeHappinessColor; }
        }
        [SerializeField] private Color _negativeHappinessColor = Color.clear;

        public int HappinessIconIndex {
            get { return _happinessIconIndex; }
        }
        [SerializeField] private int _happinessIconIndex = 0;

        public int UnhappinessIconIndex {
            get { return _unhappinessIconIndex; }
        }
        [SerializeField] private int _unhappinessIconIndex = 0;


        public IEnumerable<YieldType> NormalYields {
            get { return _normalYields; }
        }
        [SerializeField] private List<YieldType> _normalYields = null;

        public IEnumerable<YieldType> GreatPersonYields {
            get { return _greatPersonYields; }
        }
        [SerializeField] private List<YieldType> _greatPersonYields = null;

        #endregion

        #endregion

        #region instance methods

        #region from IYieldConfig

        public Sprite GetIconForYieldType(YieldType type) {
            switch(type) {
                case YieldType.Food:           return FoodIcon;
                case YieldType.Production:     return ProductionIcon;
                case YieldType.Gold:           return GoldIcon;
                case YieldType.Culture:        return CultureIcon;
                case YieldType.Science:        return ScienceIcon;
                case YieldType.GreatArtist:    return GreatPersonIcon;
                case YieldType.GreatEngineer:  return GreatPersonIcon;
                case YieldType.GreatMerchant:  return GreatPersonIcon;
                case YieldType.GreatScientist: return GreatPersonIcon;
                default: throw new NotImplementedException("No icon defined for yield type " + type.ToString());
            }
        }

        public Color GetColorForYieldType(YieldType type) {
            switch(type) {
                case YieldType.Food:           return FoodColor;
                case YieldType.Production:     return ProductionColor;
                case YieldType.Gold:           return GoldColor;
                case YieldType.Culture:        return CultureColor;
                case YieldType.Science:        return ScienceColor;
                case YieldType.GreatArtist:    return GreatPersonColor;
                case YieldType.GreatEngineer:  return GreatPersonColor;
                case YieldType.GreatMerchant:  return GreatPersonColor;
                case YieldType.GreatScientist: return GreatPersonColor;
                default: throw new NotImplementedException("No color defined for yield type " + type.ToString());
            }
        }

        #endregion

        #endregion

    }

}
