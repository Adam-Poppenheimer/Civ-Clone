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
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private ITechCanon                                       TechCanon;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private IFreshWaterCanon                                 FreshWaterCanon;

        #endregion

        #region constructors

        public ImprovementYieldLogic(
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            IImprovementLocationCanon improvementLocationCanon, ITechCanon techCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IFreshWaterCanon freshWaterCanon
        ) {
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            TechCanon                 = techCanon;
            CellPossessionCanon       = cellPossessionCanon;
            CityPossessionCanon       = cityPossessionCanon;
            FreshWaterCanon           = freshWaterCanon;
        }

        #endregion

        #region instance methods

        #region from IImprovementYieldLogic

        public ResourceSummary GetExpectedYieldOfImprovementOnCell(IImprovementTemplate template, IHexCell cell) {
            ResourceSummary retval;
            ICity cityOwningCell = null;
            ICivilization civOwningCell = null;

            cityOwningCell = CellPossessionCanon.GetOwnerOfPossession(cell);

            if(cityOwningCell != null) {
                civOwningCell = CityPossessionCanon.GetOwnerOfPossession(cityOwningCell);
            }

            var resourceNodeOnCell = ResourceNodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if( resourceNodeOnCell != null && resourceNodeOnCell.Resource.Extractor == template &&
                (civOwningCell == null || TechCanon.IsResourceVisibleToCiv(resourceNodeOnCell.Resource, civOwningCell))
            ){
                retval = resourceNodeOnCell.Resource.BonusYieldWhenImproved;               
            }else {
                retval = template.BonusYieldNormal;
            }

            if(civOwningCell != null) {
                var applicableTechMods = new List<IImprovementModificationData>();

                foreach(var techModifyingYield in TechCanon.GetTechsDiscoveredByCiv(civOwningCell)) {
                    applicableTechMods.AddRange(
                        techModifyingYield.ImprovementYieldModifications.Where(tuple => tuple.Template == template)
                    );
                }

                foreach(IImprovementModificationData mod in applicableTechMods) {
                    if(!mod.RequiresFreshWater || FreshWaterCanon.DoesCellHaveAccessToFreshWater(cell)) {
                        retval += mod.BonusYield;
                    }
                }
            }

            return retval;
        }

        public ResourceSummary GetYieldOfImprovement(IImprovement improvement) {
            if(improvement.IsConstructed && !improvement.IsPillaged) {
                return GetExpectedYieldOfImprovementOnCell(
                    improvement.Template, ImprovementLocationCanon.GetOwnerOfPossession(improvement)
                );
            }else {
                return ResourceSummary.Empty;
            }            
        }

        #endregion

        #endregion

    }

}
