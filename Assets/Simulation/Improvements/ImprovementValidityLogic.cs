﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Simulation.Improvements {

    public class ImprovementValidityLogic : IImprovementValidityLogic {

        #region instance fields and properties

        private ICityFactory CityFactory;

        private IHexGrid Grid;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementValidityLogic(ICityFactory cityFactory, IHexGrid grid,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon
        ){
            CityFactory       = cityFactory;
            Grid              = grid;
            NodePositionCanon = nodePositionCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementValidityLogic

        public bool IsTemplateValidForCell(IImprovementTemplate template, IHexCell cell) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            if(CityFactory.AllCities.Where(city => city.Location == cell).Count() != 0) {
                return false;
            }

            var nodeAtCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(nodeAtCell != null && nodeAtCell.Resource.Extractor == template) {
                return true;
            }

            return 
                (template.RestrictedToTerrains.Count() == 0 || template.RestrictedToTerrains.Contains(cell.Terrain)) &&
                (template.RestrictedToFeatures.Count() == 0 || template.RestrictedToFeatures.Contains(cell.Feature)) && 
                (template.RestrictedToShapes  .Count() == 0 || template.RestrictedToShapes  .Contains(cell.Shape  ));
        }

        #endregion

        #endregion

    }

}
