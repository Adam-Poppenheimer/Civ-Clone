using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities.Buildings {

    public class ImprovementBuildingRestriction : IBuildingRestriction {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IHexCell>        CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IImprovement> ImprovementLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementBuildingRestriction(
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IImprovement> improvementLocationCanon
        ) {
            CellPossessionCanon      = cellPossessionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingRestriction

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city, ICivilization cityOwner) {
            var improvementTemplatesAroundCity = CellPossessionCanon.GetPossessionsOfOwner(city).SelectMany(
                cell => ImprovementLocationCanon.GetPossessionsOfOwner(cell)
            ).Select(
                improvement => improvement.Template
            ).Distinct();

            return template.PrerequisiteImprovementsNearCity.All(prereq => improvementTemplatesAroundCity.Contains(prereq));
        }

        #endregion

        #endregion
        
    }

}
