using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class RiverSectionCanon : IRiverSectionCanon {

        #region instance fields and properties

        #region from IRiverSectionCanon

        public ReadOnlyCollection<RiverSection> Sections {
            get { return sections.AsReadOnly(); }
        }
        private List<RiverSection> sections = new List<RiverSection>();

        #endregion

        private Dictionary<Tuple<IHexCell, IHexCell>, RiverSection> SectionBetweenCells =
            new Dictionary<Tuple<IHexCell, IHexCell>, RiverSection>();



        
        private IHexGrid         Grid;
        private IMapRenderConfig RenderConfig;
        private IRiverCanon      RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public RiverSectionCanon(IHexGrid grid, IMapRenderConfig renderConfig, IRiverCanon riverCanon) {
            Grid         = grid;
            RenderConfig = renderConfig;
            RiverCanon   = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IRiverSectionCanon

        //The fact that we're building sections only from the NE, E, and SE
        //cell edges is very important. A lot of the code here is structured
        //as it is to deal with this reality. 
        public void RefreshRiverSections() {
            sections.Clear();
            SectionBetweenCells.Clear();            

            foreach(var cell in Grid.Cells) {
                for(HexDirection direction = HexDirection.NE; direction <= HexDirection.SE; direction++) {
                    if(RiverCanon.HasRiverAlongEdge(cell, direction)) {
                        var neighbor = Grid.GetNeighbor(cell, direction);

                        Vector3 start = cell.AbsolutePosition + RenderConfig.GetFirstCorner (direction);
                        Vector3 end   = cell.AbsolutePosition + RenderConfig.GetSecondCorner(direction);

                        RiverFlow flow = RiverCanon.GetFlowOfRiverAtEdge(cell, direction);

                        //Our control points need to operate differently if we're at endpoints,
                        //and both ControlOne and ControlTwo might have alternate behavior.
                        //We need to check both ends of the section for endpoints
                        bool previousCellRiver     = RiverCanon.HasRiverAlongEdge(cell,     direction.Previous ());
                        bool previousNeighborRiver = RiverCanon.HasRiverAlongEdge(neighbor, direction.Previous2());

                        bool hasPreviousEndpoint = 
                            !previousCellRiver &&
                            !previousNeighborRiver;

                        bool hasNextEndpoint = 
                            !RiverCanon.HasRiverAlongEdge(cell,     direction.Next ()) &&
                            !RiverCanon.HasRiverAlongEdge(neighbor, direction.Next2());

                        var newRiverSection = new RiverSection() {
                            AdjacentCellOne         = cell,
                            AdjacentCellTwo         = neighbor,
                            DirectionFromOne        = direction,
                            Start                   = start,
                            End                     = end,
                            FlowFromOne             = flow,
                            HasPreviousEndpoint     = hasPreviousEndpoint,
                            HasNextEndpoint         = hasNextEndpoint
                        };

                        SectionBetweenCells[new Tuple<IHexCell, IHexCell>(cell, neighbor)] = newRiverSection;

                        sections.Add(newRiverSection);
                    }
                }
            }
        }

        public RiverSection GetSectionBetweenCells(IHexCell cellOne, IHexCell cellTwo) {
            if(cellOne == null || cellTwo == null) {
                return null;
            }

            RiverSection retval;

            if(!SectionBetweenCells.TryGetValue(new Tuple<IHexCell, IHexCell>(cellOne, cellTwo), out retval)) {
                SectionBetweenCells.TryGetValue(new Tuple<IHexCell, IHexCell>(cellTwo, cellOne), out retval);
            }

            return retval;
        }

        public bool IsNeighborOf(RiverSection thisSection, RiverSection otherSection) {
            if(otherSection == null) {
                return false;
            }

            IHexCell thisCenter    = thisSection.AdjacentCellOne;
            IHexCell thisLeft      = Grid.GetNeighbor(thisSection.AdjacentCellOne, thisSection.DirectionFromOne.Previous());
            IHexCell thisRight     = thisSection.AdjacentCellTwo;
            IHexCell thisNextRight = Grid.GetNeighbor(thisSection.AdjacentCellOne, thisSection.DirectionFromOne.Next());

            return (thisLeft      != null && GetSectionBetweenCells(thisCenter, thisLeft)      == otherSection)
                || (thisLeft      != null && GetSectionBetweenCells(thisLeft,   thisRight)     == otherSection)
                || (thisNextRight != null && GetSectionBetweenCells(thisCenter, thisNextRight) == otherSection)
                || (thisNextRight != null && GetSectionBetweenCells(thisRight,  thisNextRight) == otherSection);
        }

        public bool AreSectionFlowsCongruous(RiverSection thisSection, RiverSection otherSection) {
            if(thisSection.DirectionFromOne == HexDirection.NE) {
                switch(otherSection.DirectionFromOne) {
                    case HexDirection.NE: return thisSection.FlowFromOne != otherSection.FlowFromOne;
                    case HexDirection.E:  return thisSection.FlowFromOne == otherSection.FlowFromOne;
                    case HexDirection.SE: return thisSection.FlowFromOne != otherSection.FlowFromOne;
                }
            }else if(thisSection.DirectionFromOne == HexDirection.E) {
                switch(otherSection.DirectionFromOne) {
                    case HexDirection.NE: return thisSection.FlowFromOne == otherSection.FlowFromOne;
                    case HexDirection.E:  return false;
                    case HexDirection.SE: return thisSection.FlowFromOne == otherSection.FlowFromOne;
                }
            }else if(thisSection.DirectionFromOne == HexDirection.SE) {
                switch(otherSection.DirectionFromOne) {
                    case HexDirection.NE: return thisSection.FlowFromOne != otherSection.FlowFromOne;
                    case HexDirection.E:  return thisSection.FlowFromOne == otherSection.FlowFromOne;
                    case HexDirection.SE: return thisSection.FlowFromOne != otherSection.FlowFromOne;
                }
            }

            return false;
        }

        #endregion

        #endregion

    }

}
