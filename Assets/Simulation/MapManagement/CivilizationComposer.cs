using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.Core;

namespace Assets.Simulation.MapManagement {

    public class CivilizationComposer : ICivilizationComposer {

        #region instance fields and properties

        private ICivilizationFactory  CivilizationFactory;
        private ITechCanon            TechCanon;
        private IGameCore             GameCore;
        private ISocialPolicyComposer PolicyComposer;

        private List<ITechDefinition> AvailableTechs;

        #endregion

        #region constructors

        [Inject]
        public CivilizationComposer(
            ICivilizationFactory civilizationFactory, ITechCanon techCanon, IGameCore gameCore,
            ISocialPolicyComposer policyComposer,
            [Inject(Id = "Available Techs")] List<ITechDefinition> availableTechs
        ) {
            CivilizationFactory = civilizationFactory;
            TechCanon           = techCanon;
            GameCore            = gameCore;
            PolicyComposer      = policyComposer;
            AvailableTechs      = availableTechs;
        }

        #endregion

        #region instance methods

        public void ClearRuntime() {
            foreach(var civ in new List<ICivilization>(CivilizationFactory.AllCivilizations)) {
                civ.Destroy();                
            }
            PolicyComposer.ClearPolicyRuntime();
        }

        public void ComposeCivilizations(SerializableMapData mapData) {
            mapData.Civilizations = new List<SerializableCivilizationData>();

            foreach(var civilization in CivilizationFactory.AllCivilizations) {
                var civData = new SerializableCivilizationData() {
                    Name             = civilization.Name,
                    Color            = civilization.Color,
                    GoldStockpile    = civilization.GoldStockpile,
                    CultureStockpile = civilization.CultureStockpile,
                    DiscoveredTechs  = TechCanon.GetTechsDiscoveredByCiv(civilization).Select(tech => tech.Name).ToList(),
                    SocialPolicies   = PolicyComposer.ComposePoliciesFromCiv(civilization)
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

                mapData.Civilizations.Add(civData);
            }

            mapData.ActiveCivilization = GameCore.ActiveCivilization.Name;
        }

        public void DecomposeCivilizations(SerializableMapData mapData) {
            foreach(var civData in mapData.Civilizations) {
                var newCiv = CivilizationFactory.Create(civData.Name, civData.Color);

                newCiv.GoldStockpile = civData.GoldStockpile;
                newCiv.CultureStockpile = civData.CultureStockpile;

                PolicyComposer.DecomposePoliciesIntoCiv(civData.SocialPolicies, newCiv);

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

            GameCore.ActiveCivilization = CivilizationFactory.AllCivilizations.Where(
                civ => civ.Name.Equals(mapData.ActiveCivilization)
            ).FirstOrDefault();
        }

        #endregion

    }

}
