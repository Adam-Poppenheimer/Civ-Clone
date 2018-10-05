using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.MapGeneration {

    public class CellScorer : ICellScorer {

        #region instance fields and properties

        private ITechDefinition[] AncientTechs {
            get {
                if(_ancientTechs == null) {
                    _ancientTechs = TechCanon.GetTechsOfEra(TechnologyEra.Ancient).ToArray();
                }
                return _ancientTechs;
            }
        }
        private ITechDefinition[] _ancientTechs;

        private IEnumerable<ITechDefinition> ClassicalTechs {
            get {
                if(_classicalTechs == null) {
                    _classicalTechs = AncientTechs.Concat(TechCanon.GetTechsOfEra(TechnologyEra.Classical)).ToArray();
                }
                return _classicalTechs;
            }
        }
        private ITechDefinition[] _classicalTechs;

        private IEnumerable<ITechDefinition> MedievalTechs {
            get {
                if(_medievalTechs == null) {
                    _medievalTechs = ClassicalTechs.Concat(TechCanon.GetTechsOfEra(TechnologyEra.Medieval)).ToArray();
                }
                return _medievalTechs;
            }
        }
        private ITechDefinition[] _medievalTechs;

        private CachedTechData AncientTechData {
            get {
                if(_ancientTechData == null) {
                    _ancientTechData = new CachedTechData() {
                        ImprovementModifications = new HashSet<IImprovementModificationData>(AncientTechs.SelectMany(tech => tech.ImprovementYieldModifications)),
                        VisibleResources         = new HashSet<IResourceDefinition>         (TechCanon.GetDiscoveredResourcesFromTechs  (AncientTechs)),
                        AvailableImprovements    = new HashSet<IImprovementTemplate>        (TechCanon.GetAvailableImprovementsFromTechs(AncientTechs)),
                        AvailableBuildings       = new HashSet<IBuildingTemplate>           (TechCanon.GetAvailableBuildingsFromTechs   (AncientTechs))
                    };
                }
                return _ancientTechData;
            }
        }
        private CachedTechData _ancientTechData;

        private CachedTechData ClassicalTechData {
            get {
                if(_classicalTechData == null) {
                    _classicalTechData = new CachedTechData() {
                        ImprovementModifications = new HashSet<IImprovementModificationData>(ClassicalTechs.SelectMany(tech => tech.ImprovementYieldModifications)),
                        VisibleResources         = new HashSet<IResourceDefinition>         (TechCanon.GetDiscoveredResourcesFromTechs  (ClassicalTechs)),
                        AvailableImprovements    = new HashSet<IImprovementTemplate>        (TechCanon.GetAvailableImprovementsFromTechs(ClassicalTechs)),
                        AvailableBuildings       = new HashSet<IBuildingTemplate>           (TechCanon.GetAvailableBuildingsFromTechs   (ClassicalTechs))
                    };
                }
                return _classicalTechData;
            }
        }
        private CachedTechData _classicalTechData;

        private CachedTechData MedievalTechData {
            get {
                if(_medievalTechData == null) {
                    _medievalTechData = new CachedTechData() {
                        ImprovementModifications = new HashSet<IImprovementModificationData>(MedievalTechs.SelectMany(tech => tech.ImprovementYieldModifications)),
                        VisibleResources         = new HashSet<IResourceDefinition>         (TechCanon.GetDiscoveredResourcesFromTechs  (MedievalTechs)),
                        AvailableImprovements    = new HashSet<IImprovementTemplate>        (TechCanon.GetAvailableImprovementsFromTechs(MedievalTechs)),
                        AvailableBuildings       = new HashSet<IBuildingTemplate>           (TechCanon.GetAvailableBuildingsFromTechs   (MedievalTechs))
                    };
                }
                return _medievalTechData;
            }
        }
        private CachedTechData _medievalTechData;



        private IYieldEstimator                                  YieldEstimator;
        private IMapScorer                                       MapScorer;
        private ITechCanon                                       TechCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CellScorer(
            IYieldEstimator yieldEstimator, IMapScorer mapScorer, ITechCanon techCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            YieldEstimator    = yieldEstimator;
            MapScorer         = mapScorer;
            TechCanon         = techCanon;
            NodeLocationCanon = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from ICellScorer

        public float GetScoreOfCell(IHexCell cell) {

            var ancientYield   = YieldEstimator.GetYieldEstimateForCell(cell, AncientTechData);
            var classicalYield = YieldEstimator.GetYieldEstimateForCell(cell, ClassicalTechData);
            var medievalYield  = YieldEstimator.GetYieldEstimateForCell(cell, MedievalTechData);

            var resourceNode = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            float ancientScore = MapScorer.GetScoreOfYield(ancientYield) +
                                 MapScorer.GetScoreOfResourceNode(resourceNode, AncientTechs);

            float classicalScore = MapScorer.GetScoreOfYield(classicalYield) +
                                   MapScorer.GetScoreOfResourceNode(resourceNode, ClassicalTechs);

            float medievalScore = MapScorer.GetScoreOfYield(medievalYield) +
                                  MapScorer.GetScoreOfResourceNode(resourceNode, MedievalTechs);

            return (ancientScore + classicalScore + medievalScore) / 3f;
        }

        #endregion

        #endregion
        
    }

}
