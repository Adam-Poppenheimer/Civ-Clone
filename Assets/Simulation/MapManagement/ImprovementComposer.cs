using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Zenject;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapManagement {

    public class ImprovementComposer {

        #region instance fields and properties

        private IImprovementFactory               ImprovementFactory;
        private IImprovementLocationCanon         ImprovementLocationCanon;
        private IHexGrid                          Grid;
        private IEnumerable<IImprovementTemplate> AvailableImprovementTemplates;

        #endregion

        #region constructors

        public ImprovementComposer(
            IImprovementFactory improvementFactory, IImprovementLocationCanon improvementLocationCanon, IHexGrid grid,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableImprovementTemplates
        ){
            ImprovementFactory            = improvementFactory;
            ImprovementLocationCanon      = improvementLocationCanon;
            Grid                          = grid;
            AvailableImprovementTemplates = availableImprovementTemplates;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var improvement in new List<IImprovement>(ImprovementFactory.AllImprovements)) {
                ImprovementLocationCanon.ChangeOwnerOfPossession(improvement, null);
                improvement.Destroy();
            }
        }

        public void ComposeImprovements(SerializableMapData mapData) {
            mapData.Improvements = new List<SerializableImprovementData>();

            foreach(var improvement in ImprovementFactory.AllImprovements) {
                var improvementLocation = ImprovementLocationCanon.GetOwnerOfPossession(improvement);

                var newImprovementData = new SerializableImprovementData() {
                    Location      = improvementLocation.Coordinates,
                    Template      = improvement.Template.name,
                    WorkInvested  = improvement.WorkInvested,
                    IsConstructed = improvement.IsConstructed,
                    IsPillaged    = improvement.IsPillaged
                };

                mapData.Improvements.Add(newImprovementData);
            }
        }

        public void DecomposeImprovements(SerializableMapData mapData) {
            foreach(var improvementData in mapData.Improvements) {

                var templateToBuild = AvailableImprovementTemplates.Where(
                    template => template.name.Equals(improvementData.Template)
                ).First();
                var locationToBuild = Grid.GetCellAtCoordinates(improvementData.Location);                

                ImprovementFactory.BuildImprovement(
                    templateToBuild, locationToBuild, improvementData.WorkInvested,
                    improvementData.IsConstructed, improvementData.IsPillaged
                );
            }
        }

        #endregion

    }

}
