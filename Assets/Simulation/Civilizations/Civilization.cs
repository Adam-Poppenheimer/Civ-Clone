using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The standard implementation of ICivilization.
    /// </summary>
    public class Civilization : MonoBehaviour, ICivilization {

        #region instance fields and properties

        #region from ICivilization

        public ICivilizationTemplate Template { get; set; }

        /// <inheritdoc/>
        public int GoldStockpile    { get; set; }

        /// <inheritdoc/>
        public int CultureStockpile { get; set; }

        public int LastScienceYield { get; set; }

        public Queue<ITechDefinition> TechQueue { get; set; }

        #endregion

        private ICivilizationYieldLogic YieldLogic;
        private ITechCanon              TechCanon;
        private CivilizationSignals     Signals;
        private IGreatPersonCanon       GreatPersonCanon;
        private IGreatPersonFactory     GreatPersonFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            ICivilizationYieldLogic yieldLogic, ITechCanon techCanon,
            CivilizationSignals signals, IGreatPersonCanon greatPersonCanon,
            IGreatPersonFactory greatPersonFactory
        ){
            YieldLogic         = yieldLogic;
            TechCanon          = techCanon;
            Signals            = signals;
            GreatPersonCanon   = greatPersonCanon;
            GreatPersonFactory = greatPersonFactory;

            TechQueue = new Queue<ITechDefinition>();
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.CivBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #region from ICivilization

        public void PerformIncome() {
            var yield = YieldLogic.GetYieldOfCivilization(this);

            GoldStockpile    += Mathf.FloorToInt(yield[YieldType.Gold]);
            CultureStockpile += Mathf.FloorToInt(yield[YieldType.Culture]);

            LastScienceYield = Mathf.FloorToInt(yield[YieldType.Science]);

            GreatPersonCanon.AddPointsTowardsTypeForCiv(
                GreatPersonType.GreatArtist, this, yield[YieldType.GreatArtist]
            );

            GreatPersonCanon.AddPointsTowardsTypeForCiv(
                GreatPersonType.GreatEngineer, this, yield[YieldType.GreatEngineer]
            );

            GreatPersonCanon.AddPointsTowardsTypeForCiv(
                GreatPersonType.GreatMerchant, this, yield[YieldType.GreatMerchant]
            );

            GreatPersonCanon.AddPointsTowardsTypeForCiv(
                GreatPersonType.GreatScientist, this, yield[YieldType.GreatScientist]
            );
        }

        public void PerformResearch() {
            if(TechQueue.Count > 0) {
                var activeTech = TechQueue.Peek();

                int techProgress = TechCanon.GetProgressOnTechByCiv(activeTech, this);
                techProgress += LastScienceYield;                

                if(techProgress >= activeTech.Cost && TechCanon.IsTechAvailableToCiv(activeTech, this)) {
                    TechCanon.SetTechAsDiscoveredForCiv(activeTech, this);
                    TechQueue.Dequeue();

                }else {
                    TechCanon.SetProgressOnTechByCiv(activeTech, this, techProgress);
                }
            }
        }

        public void PerformGreatPeopleGeneration() {
            foreach(var greatPersonType in EnumUtil.GetValues<GreatPersonType>()) {
                float progress = GreatPersonCanon.GetPointsTowardsTypeForCiv  (greatPersonType, this);
                float needed   = GreatPersonCanon.GetPointsNeededForTypeForCiv(greatPersonType, this);

                if(progress >= needed) {
                    float pointsLeft = progress - needed;
                    
                    GreatPersonCanon.SetPointsTowardsTypeForCiv(greatPersonType, this, pointsLeft);

                    GreatPersonFactory.BuildGreatPerson(greatPersonType, this);
                }
            }
        }

        public void Destroy() {
            if(Application.isPlaying) {
                Destroy(gameObject);
            }else {
                DestroyImmediate(gameObject);
            }
        }

        #endregion

        #endregion

    }

}
