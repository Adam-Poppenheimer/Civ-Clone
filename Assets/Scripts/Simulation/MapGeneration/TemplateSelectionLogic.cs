using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.MapGeneration {

    public class TemplateSelectionLogic : ITemplateSelectionLogic {

        #region instance fields and properties

        private ICellClimateLogic CellClimateLogic;

        #endregion

        #region constructors

        public TemplateSelectionLogic(ICellClimateLogic cellClimateLogic) {
            CellClimateLogic = cellClimateLogic;
        }

        #endregion

        #region instance methods

        #region from IRegionTemplateLogic

        public IRegionBiomeTemplate GetBiomeForLandRegion(MapRegion landRegion, IMapTemplate mapTemplate) {
            float regionTemperature   = GetRegionTemperature(landRegion);
            float regionPrecipitation = GetRegionPrecipitation(landRegion);


            var templatesByPriority = new List<IRegionBiomeTemplate>(mapTemplate.RegionBiomes);
            var templatePriority = new Dictionary<IRegionBiomeTemplate, float>();

            foreach(var regionTemplate in templatesByPriority) {
                templatePriority[regionTemplate] = GetBiomePriority(regionTemplate, regionTemperature, regionPrecipitation);
            }

            templatesByPriority.Sort((a, b) => templatePriority[a].CompareTo(templatePriority[b]));

            return templatesByPriority.First();
        }

        public IRegionTopologyTemplate GetTopologyForLandRegion(MapRegion region, IMapTemplate mapTemplate) {
            return mapTemplate.RegionTopologies.Random();
        }

        public IHomelandTemplate GetHomelandTemplateForCiv(ICivilization civ, IMapTemplate mapTemplate) {
            return mapTemplate.HomelandTemplates.Random();
        }

        #endregion

        private float GetRegionTemperature(MapRegion region) {
            return region.LandCells.Sum(cell => CellClimateLogic.GetTemperatureOfCell(cell)) / region.LandCells.Count;
        }

        private float GetRegionPrecipitation(MapRegion region) {
            return region.LandCells.Sum(cell => CellClimateLogic.GetPrecipitationOfCell(cell)) / region.LandCells.Count;
        }

        private float GetBiomePriority(IRegionBiomeTemplate biome, float temperature, float precipitation) {
            float distanceFromTemperatureAverage   = Math.Abs(temperature   - (biome.MinTemperature   + biome.MaxTemperature  ) / 2f);
            float distanceFromPrecipitationAverage = Math.Abs(precipitation - (biome.MinPrecipitation + biome.MaxPrecipitation) / 2f);

            if(temperature < biome.MinTemperature || temperature > biome.MaxTemperature) {
                distanceFromTemperatureAverage *= 2;
            }

            if(precipitation < biome.MinPrecipitation || precipitation > biome.MaxPrecipitation) {
                distanceFromPrecipitationAverage *= 2;
            }

            return distanceFromTemperatureAverage + distanceFromPrecipitationAverage;
        }

        #endregion

    }

}
