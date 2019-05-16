using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Barbarians {

    public class BarbarianAvailableUnitsLogic : IBarbarianAvailableUnitsLogic {

        #region instance fields and properties

        #region from IBarbarianAvailableUnitsLogic

        public Func<IHexCell, IEnumerable<IUnitTemplate>> LandTemplateSelector {
            get { return GetAvailableLandTemplates; }
        }

        public Func<IHexCell, IEnumerable<IUnitTemplate>> NavalTemplateSelector {
            get { return GetAvailableNavalTemplates; }
        }

        #endregion

        private List<IUnitTemplate> EmptyList = new List<IUnitTemplate>();



        private IBarbarianConfig     BarbarianConfig;
        private IUnitPositionCanon   UnitPositionCanon;
        private ICivilizationFactory CivFactory;

        #endregion

        #region constructors

        [Inject]
        public BarbarianAvailableUnitsLogic(
            IBarbarianConfig barbarianConfig, IUnitPositionCanon unitPositionCanon,
            ICivilizationFactory civFactory
        ) {
            BarbarianConfig   = barbarianConfig;
            UnitPositionCanon = unitPositionCanon;
            CivFactory        = civFactory;
        }

        #endregion

        #region instance methods

        #region from IBarbarianAvailableUnitsLogic

        public IEnumerable<IUnitTemplate> GetAvailableLandTemplates(IHexCell cell) {
            if(cell.Terrain.IsWater()) {
                return EmptyList;
            }else {
                return BarbarianConfig.UnitsToSpawn.Where(BuildLandValidityFilter(cell));
            }
        }

        public IEnumerable<IUnitTemplate> GetAvailableNavalTemplates(IHexCell cell) {
            if(cell.Terrain.IsWater()) {
                return BarbarianConfig.UnitsToSpawn.Where(BuildWaterValidityFilter(cell));
            }else {
                return EmptyList;
            }
        }

        #endregion

        private Func<IUnitTemplate, bool> BuildLandValidityFilter(IHexCell cell) {
            return delegate(IUnitTemplate template) {
                return template.Type.IsLandMilitary()
                    && UnitPositionCanon.CanPlaceUnitTemplateAtLocation(template, cell, CivFactory.BarbarianCiv);
            };
        }

        private Func<IUnitTemplate, bool> BuildWaterValidityFilter(IHexCell cell) {
            return delegate(IUnitTemplate template) {
                return template.Type.IsWaterMilitary()
                    && UnitPositionCanon.CanPlaceUnitTemplateAtLocation(template, cell, CivFactory.BarbarianCiv);
            };
        }

        #endregion
        
    }

}
