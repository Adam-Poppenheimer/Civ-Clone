using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Improvements {

    public class ImprovementYieldLogic : IImprovementYieldLogic {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private ITechCanon TechCanon;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        public ImprovementYieldLogic(
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            IImprovementLocationCanon improvementLocationCanon, ITechCanon techCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon
        ) {
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            TechCanon                 = techCanon;
            CellPossessionCanon       = cellPossessionCanon;
            CityPossessionCanon       = cityPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementYieldLogic

        public ResourceSummary GetExpectedYieldOfImprovementOnCell(IImprovementTemplate template, IHexCell cell) {
            ResourceSummary retval;

            var resourceNodeOnCell = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(resourceNodeOnCell != null && resourceNodeOnCell.Resource.Extractor == template) {
                retval = resourceNodeOnCell.Resource.BonusYieldWhenImproved;
            }else {
                retval = template.BonusYieldNormal;
            }

            var cityOwningCell = CellPossessionCanon.GetOwnerOfPossession(cell);

            if(cityOwningCell != null) {
                var civOwningTile = CityPossessionCanon.GetOwnerOfPossession(cityOwningCell);

                foreach(var techModifyingYield in TechCanon.GetTechsDiscoveredByCiv(civOwningTile)) {
                    var yieldChangesFromTech = techModifyingYield.ImprovementYieldModifications
                        .Where(tuple => tuple.Template == template)
                        .Select(tuple => tuple.BonusYield);

                    if(yieldChangesFromTech.Count() > 0) {                        
                        retval += yieldChangesFromTech.Aggregate((a, b) => a + b);
                    }
                }
            }

            return retval;
        }

        public ResourceSummary GetYieldOfImprovement(IImprovement improvement) {
            return GetExpectedYieldOfImprovementOnCell(
                improvement.Template, ImprovementLocationCanon.GetOwnerOfPossession(improvement)
            );
        }

        #endregion

        #endregion

    }

}
