using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.Core {

    public interface ICoreConfig {

        #region properties
        
        Sprite FoodIcon        { get; }
        Sprite ProductionIcon  { get; }
        Sprite GoldIcon        { get; }
        Sprite CultureIcon     { get; }
        Sprite ScienceIcon     { get; }
        Sprite GreatPersonIcon { get; }

        Sprite YieldModificationIcon { get; }

        Color FoodColor        { get; }
        Color ProductionColor  { get; }
        Color GoldColor        { get; }
        Color CultureColor     { get; }
        Color ScienceColor     { get; }
        Color GreatPersonColor { get; }

        Color PositiveHappinessColor { get; }
        Color NegativeHappinessColor { get; }

        int HappinessIconIndex   { get; }
        int UnhappinessIconIndex { get; }

        IEnumerable<YieldType> NormalYields      { get; }
        IEnumerable<YieldType> GreatPersonYields { get; }

        #endregion

        #region methods

        Sprite GetIconForYieldType(YieldType type);

        Color GetColorForYieldType(YieldType type);

        #endregion

    }

}
