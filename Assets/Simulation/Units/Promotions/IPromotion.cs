using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotion {

        #region properties

        string name { get; }

        string Description { get; }

        Sprite Icon { get; }

        #endregion

        #region methods

        bool HasArg(PromotionArgType arg);

        float          GetFloat();
        int            GetInt();
        TerrainFeature GetFeature();
        TerrainType    GetTerrain();
        TerrainShape   GetShape();
        UnitType       GetUnitType();

        #endregion

    }

}
