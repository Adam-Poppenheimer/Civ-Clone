using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.AI {

    public interface IAIConfig {

        #region properties

        int UnitMaxInfluenceRadius    { get; }
        int EncampmentInfluenceRadius { get; }

        float RoadPillageValue                  { get; }
        float NormalImprovementPillageValue     { get; }
        float ExtractingImprovementPillageValue { get; }

        #endregion

    }

}
