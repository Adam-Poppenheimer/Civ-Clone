using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.AI {

    public class InfluenceMapLogic : IInfluenceMapLogic {

        #region instance fields and properties

        private List<IInfluenceSource> InfluenceSources;
        private IHexGrid               Grid;

        #endregion

        #region constructors

        [Inject]
        public InfluenceMapLogic(List<IInfluenceSource> influenceSources, IHexGrid grid) {
            InfluenceSources = influenceSources;
            Grid             = grid;
        }

        #endregion

        #region instance methods

        #region from IInfluenceMapGenerator

        public void ClearMaps(InfluenceMaps maps) {
            maps.AllyPresence     = null;
            maps.EnemyPresence    = null;
            maps.PillagingValue = null;
        }

        public void AssignMaps(InfluenceMaps maps, ICivilization targetCiv) {
            if(maps.AllyPresence     == null) { maps.AllyPresence     = new float[Grid.Cells.Count]; }
            if(maps.EnemyPresence    == null) { maps.EnemyPresence    = new float[Grid.Cells.Count]; }
            if(maps.PillagingValue == null) { maps.PillagingValue = new float[Grid.Cells.Count]; }

            foreach(var cell in Grid.Cells) {
                maps.AllyPresence    [cell.Index] = 0f;
                maps.EnemyPresence   [cell.Index] = 0f;
                maps.PillagingValue[cell.Index] = 0f;
            }

            foreach(var influenceSource in InfluenceSources) {
                influenceSource.ApplyToMaps(maps, targetCiv);
            }
        }

        #endregion

        #endregion
        
    }

}
