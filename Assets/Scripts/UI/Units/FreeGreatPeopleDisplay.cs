using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Units;

using Assets.UI.Civilizations;

using UnityCustomUtilities.Extensions;

namespace Assets.UI.Units {

    public class FreeGreatPeopleDisplay : CivilizationDisplayBase {

        #region instance fields and properties

        [SerializeField] private RectTransform GreatPeopleSelectorContainer = null;
        [SerializeField] private GameObject    GreatPeopleSelectorPrefab    = null;

        private List<Button> SelectorButtons = new List<Button>();





        private IGreatPersonFactory   GreatPersonFactory;
        private IFreeGreatPeopleCanon FreeGreatPeopleCanon;

        #endregion

        #region instance methods

        #region Unity messages

        private void Awake() {
            PopulateGreatPeopleButtons();
        }

        #endregion

        [Inject]
        private void InjectDependencies(
            IGreatPersonFactory greatPersonFactory, IFreeGreatPeopleCanon freeGreatPeopleCanon
        ) {
            GreatPersonFactory   = greatPersonFactory;
            FreeGreatPeopleCanon = freeGreatPeopleCanon;
        }

        #region from CivilizationDisplayBase

        public override void Refresh() {
            bool canSelectButtons = FreeGreatPeopleCanon.GetFreeGreatPeopleForCiv(ObjectToDisplay) > 0;

            foreach(var button in SelectorButtons) {
                button.interactable = canSelectButtons;
            }
        }

        #endregion

        private void PopulateGreatPeopleButtons() {
            foreach(var greatPersonType in EnumUtil.GetValues<GreatPersonType>()) {
                var newGPSelector = Instantiate(GreatPeopleSelectorPrefab);

                var cachedPersonType = greatPersonType;

                var selectorButton = newGPSelector.GetComponentInChildren<Button>();

                selectorButton.onClick.AddListener(
                    () => GreatPersonRequested(cachedPersonType)
                );

                var selectorLabel = newGPSelector.GetComponentInChildren<Text>();

                selectorLabel.text = greatPersonType.ToString();

                SelectorButtons.Add(selectorButton);

                newGPSelector.transform.SetParent(GreatPeopleSelectorContainer, false);
                newGPSelector.gameObject.SetActive(true);
            }
        }

        private void GreatPersonRequested(GreatPersonType type) {
            if(FreeGreatPeopleCanon.GetFreeGreatPeopleForCiv(ObjectToDisplay) > 0) {
                GreatPersonFactory.BuildGreatPerson(type, ObjectToDisplay);

                FreeGreatPeopleCanon.RemoveFreeGreatPersonFromCiv(ObjectToDisplay);

                Refresh();
            }
        }

        #endregion

    }

}
