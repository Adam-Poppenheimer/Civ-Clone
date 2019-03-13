using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class RiverContourRationalizer : IRiverContourRationalizer {

        #region instance fields and properties

        private IHexGrid             Grid;
        private IRiverCanon          RiverCanon;
        private IRiverSectionCanon   RiverSectionCanon;
        private IRiverAssemblyCanon  RiverAssemblyCanon;
        private IContourRationalizer ContourRationalizer;

        #endregion

        #region constructors

        [Inject]
        public RiverContourRationalizer(
            IHexGrid grid, IRiverCanon riverCanon, IRiverSectionCanon riverSectionCanon,
            IRiverAssemblyCanon riverAssemblyCanon, IContourRationalizer contourRationalizer
        ) {
            Grid                = grid;
            RiverCanon          = riverCanon;
            RiverSectionCanon   = riverSectionCanon;
            RiverAssemblyCanon  = riverAssemblyCanon;
            ContourRationalizer = contourRationalizer;
        }

        #endregion

        #region instance methods

        #region from IRiverContourRationalizer

        public void RationalizeRiverContoursInCorner(IHexCell center, HexDirection direction) {
            IHexCell left = Grid.GetNeighbor(center, direction.Previous());

            //Contours will only fall short or overlap when we're at a river confluence,
            //so we can save ourselves time by checking for that
            if( left != null &&
                RiverCanon.HasRiverAlongEdge(center, direction.Previous()) &&
                RiverCanon.HasRiverAlongEdge(center, direction           ) &&
                RiverCanon.HasRiverAlongEdge(left,   direction.Next()    )
            ) {
                IHexCell right = Grid.GetNeighbor(center, direction);

                RiverSection centerLeftSection  = RiverSectionCanon.GetSectionBetweenCells(center, left);
                RiverSection centerRightSection = RiverSectionCanon.GetSectionBetweenCells(center, right);

                bool shouldCullContours = true;

                //Rationalizing contours also doesn't need to occur between segments
                //that are adjacent segments of the same river, which would put us on
                //the inside of a river curve. So we make some checks to exclude that
                //possibility, as well
                foreach(var river in RiverAssemblyCanon.Rivers) {
                    int leftIndex  = river.IndexOf(centerLeftSection);
                    int rightIndex = river.IndexOf(centerRightSection);

                    bool areAdjacentInRiver = ((leftIndex + 1) == rightIndex) || ((rightIndex + 1) == leftIndex);

                    if(leftIndex != -1 && rightIndex != -1 && areAdjacentInRiver) {
                        shouldCullContours = false;
                        break;
                    }
                }

                if(shouldCullContours) {
                    ContourRationalizer.RationalizeCellContours(center, direction);
                }
            }
        }

        #endregion

        #endregion

    }

}
