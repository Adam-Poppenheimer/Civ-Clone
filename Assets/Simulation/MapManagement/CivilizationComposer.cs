﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.Core;
using Assets.Simulation.Visibility;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.MapManagement {

    public class CivilizationComposer : ICivilizationComposer {

        #region instance fields and properties

        private ICivilizationFactory  CivilizationFactory;
        private ITechCanon            TechCanon;
        private IGameCore             GameCore;
        private ISocialPolicyComposer PolicyComposer;
        private IExplorationCanon     ExplorationCanon;
        private IHexGrid              Grid;
        private IFreeBuildingsCanon   FreeBuildingsCanon;

        private List<IBuildingTemplate>                   BuildingTemplates;
        private List<ITechDefinition>                     AvailableTechs;
        private ReadOnlyCollection<ICivilizationTemplate> AvailableCivTemplates;

        #endregion

        #region constructors

        [Inject]
        public CivilizationComposer(
            ICivilizationFactory civilizationFactory, ITechCanon techCanon, IGameCore gameCore,
            ISocialPolicyComposer policyComposer, IExplorationCanon explorationCanon, IHexGrid grid,
            IFreeBuildingsCanon freeBuildingsCanon, List<IBuildingTemplate> availableBuildings,
            [Inject(Id = "Available Techs")] List<ITechDefinition> availableTechs,
            ReadOnlyCollection<ICivilizationTemplate> availableCivTemplates
        ) {
            CivilizationFactory   = civilizationFactory;
            TechCanon             = techCanon;
            GameCore              = gameCore;
            PolicyComposer        = policyComposer;
            ExplorationCanon      = explorationCanon;
            Grid                  = grid;
            FreeBuildingsCanon    = freeBuildingsCanon;
            BuildingTemplates     = availableBuildings;
            AvailableTechs        = availableTechs;
            AvailableCivTemplates = availableCivTemplates;
        }

        #endregion

        #region instance methods

        #region from ICivilizationComposer

        public void ClearRuntime() {
            foreach(var civ in new List<ICivilization>(CivilizationFactory.AllCivilizations)) {
                civ.Destroy();                
            }
            PolicyComposer.ClearPolicyRuntime();
            FreeBuildingsCanon.Clear();
        }

        public void ComposeCivilizations(SerializableMapData mapData) {
            mapData.Civilizations = new List<SerializableCivilizationData>();

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                var civData = new SerializableCivilizationData() {
                    TemplateName     = civilization.Template.Name,
                    GoldStockpile    = civilization.GoldStockpile,
                    CultureStockpile = civilization.CultureStockpile,
                    DiscoveredTechs  = TechCanon.GetTechsDiscoveredByCiv(civilization).Select(tech => tech.Name).ToList(),
                    SocialPolicies   = PolicyComposer.ComposePoliciesFromCiv(civilization),
                };

                if(civilization.TechQueue != null && civilization.TechQueue.Count > 0) {
                    civData.TechQueue = civilization.TechQueue.Select(tech => tech.Name).ToList();
                }else {
                    civData.TechQueue = null;
                }

                var availableTechs = TechCanon.GetTechsAvailableToCiv(civilization);

                if(availableTechs.Count() > 0) {
                    foreach(var availableTech in availableTechs) {
                        int progress = TechCanon.GetProgressOnTechByCiv(availableTech, civilization);
                        if(progress != 0) {
                            if(civData.ProgressOnTechs == null) {
                                civData.ProgressOnTechs = new Dictionary<string, int>();
                            }

                            civData.ProgressOnTechs[availableTech.Name] = progress;
                        }
                    }
                }else {
                    civData.ProgressOnTechs = null;
                }   
                
                var exploredCells = Grid.Cells.Where(cell => ExplorationCanon.IsCellExploredByCiv(cell, civilization));

                civData.ExploredCells = exploredCells.Select(cell => cell.Coordinates).ToList();

                civData.FreeBuildings = FreeBuildingsCanon
                    .GetFreeBuildingsForCiv(civilization)
                    .Select(buildingList => buildingList.Select(buildingTemplate => buildingTemplate.name).ToList())
                    .ToList(); 

                mapData.Civilizations.Add(civData);
            }

            mapData.ActiveCivilization = GameCore.ActiveCivilization.Template.Name;
        }

        public void DecomposeCivilizations(SerializableMapData mapData) {
            foreach(var civData in mapData.Civilizations) {
                var civTemplate = AvailableCivTemplates.Where(template => template.Name.Equals(civData.TemplateName)).FirstOrDefault();

                var newCiv = CivilizationFactory.Create(civTemplate);

                newCiv.GoldStockpile    = civData.GoldStockpile;
                newCiv.CultureStockpile = civData.CultureStockpile;

                PolicyComposer.DecomposePoliciesIntoCiv(civData.SocialPolicies, newCiv);

                DecomposeTechs(civData, newCiv);

                if(civData.ExploredCells != null) {
                    foreach(var exploredCoords in civData.ExploredCells) {
                        var exploredCell = Grid.GetCellAtCoordinates(exploredCoords);

                        ExplorationCanon.SetCellAsExploredByCiv(exploredCell, newCiv);
                    }
                }

                DecomposeFreeBuildings(civData, newCiv);
            }

            GameCore.ActiveCivilization = CivilizationFactory.AllCivilizations.Where(
                civ => civ.Template.Name.Equals(mapData.ActiveCivilization)
            ).FirstOrDefault();
        }

        #endregion

        private void DecomposeTechs(SerializableCivilizationData civData, ICivilization newCiv) {
            if(civData.TechQueue != null) {
                for(int i = 0; i < civData.TechQueue.Count; i++) {
                    var techName = civData.TechQueue[i];

                    var techOfName = AvailableTechs.Where(tech => tech.Name.Equals(techName)).FirstOrDefault();
                    if(techOfName == null) {
                        throw new InvalidOperationException(string.Format("CivData.TechQueue had invalid tech name {0} in it", techName));
                    }
                    newCiv.TechQueue.Enqueue(techOfName);
                }
            }
                
            if(civData.DiscoveredTechs != null) {
                foreach(var discoveredTechName in civData.DiscoveredTechs) {
                    var techOfName = AvailableTechs.Where(tech => tech.Name.Equals(discoveredTechName)).FirstOrDefault();
                    if(techOfName == null) {
                        throw new InvalidOperationException(
                            string.Format("CivData.DiscoveredTechs had invalid tech name {0} in it", discoveredTechName)
                        );
                    }

                    TechCanon.SetTechAsDiscoveredForCiv(techOfName, newCiv);
                }
            }
                
            if(civData.ProgressOnTechs != null) {
                foreach(var techInProgressName in civData.ProgressOnTechs.Keys) {
                    var techOfName = AvailableTechs.Where(tech => tech.Name.Equals(techInProgressName)).FirstOrDefault();
                    if(techOfName == null) {
                        throw new InvalidOperationException(
                            string.Format("CivData.ProgressOnTechs had invalid tech name {0} in it", techInProgressName)
                        );
                    }

                    TechCanon.SetProgressOnTechByCiv(techOfName, newCiv, civData.ProgressOnTechs[techInProgressName]);
                }
            }
        }

        private void DecomposeFreeBuildings(SerializableCivilizationData civData, ICivilization newCiv) {
            if(civData.FreeBuildings != null) {
                foreach(List<string> freeBuildingSetByName in civData.FreeBuildings) {
                    var freeBuildingSet = new List<IBuildingTemplate>();

                    foreach(string templateName in freeBuildingSetByName) {
                        var templateOfName = BuildingTemplates.Where(template => template.name.Equals(templateName)).FirstOrDefault();

                        if(templateOfName == null) {
                            throw new InvalidOperationException(
                                string.Format("CivData.FreeBuildings had invalid building name {0} in it", templateName)
                            );
                        }

                        freeBuildingSet.Add(templateOfName);
                    }

                    FreeBuildingsCanon.SubscribeFreeBuildingToCiv(freeBuildingSet, newCiv);
                }
            }
        }

        #endregion

    }

}
