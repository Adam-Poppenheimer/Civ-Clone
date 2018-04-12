using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotion {

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
