using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;


namespace Assets.Simulation.Cities.Buildings {

    public class CellYieldFromBuildingsLogic : ICellYieldFromBuildingsLogic {

        #region instance fields and properties



        #endregion

        #region constructors

        [Inject]
        public CellYieldFromBuildingsLogic() { }

        #endregion

        #region instance methods

        #region from IBuildingCellBonusYieldLogic

        public YieldSummary GetBonusCellYieldFromBuildings(
            IHexCell cell, IEnumerable<IBuildingTemplate> buildings
        ) {
            var retval = YieldSummary.Empty;

            foreach(var cellMod in buildings.SelectMany(building => building.CellYieldModifications)) {
                if( cellMod.PropertyConsidered == CellPropertyType.Terrain &&
                    cell.Terrain == cellMod.TerrainRequired
                ) {
                    retval += cellMod.BonusYield;

                }else if( 
                    cellMod.PropertyConsidered == CellPropertyType.Shape &&
                    cell.Shape == cellMod.ShapeRequired
                ){
                    retval += cellMod.BonusYield;

                }else if(
                    cellMod.PropertyConsidered == CellPropertyType.Vegetation &&
                    cell.Vegetation == cellMod.VegetationRequired
                ) {
                    retval += cellMod.BonusYield;

                }else if(
                    cellMod.PropertyConsidered == CellPropertyType.CellIsUnderwater &&
                    cell.Terrain.IsWater() == cellMod.MustBeUnderwater
                ) {
                    retval += cellMod.BonusYield;
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
