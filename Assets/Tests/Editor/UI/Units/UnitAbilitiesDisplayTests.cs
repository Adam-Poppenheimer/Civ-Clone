using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.UI.Units;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Core;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class UnitAbilitiesDisplayTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<Mock<IAbilityDisplay>> InstantiatedDisplays = new List<Mock<IAbilityDisplay>>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            InstantiatedDisplays.Clear();

            Container.BindMemoryPool<IAbilityDisplay, AbilityDisplayMemoryPool>().FromMethod(delegate(DiContainer container) {
                var mockDisplay = new Mock<IAbilityDisplay>();
                mockDisplay.SetupAllProperties();

                mockDisplay.Setup(display => display.transform).Returns(new GameObject().transform);

                InstantiatedDisplays.Add(mockDisplay);

                return mockDisplay.Object;
            });

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();

            Container.Bind<UnitAbilitiesDisplay>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<UnitSignals>().FromMock();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and ObjectToDisplay is not null, " +
            "UnitAbilitiesDisplay should instantiate just enough IAbilityDisplays to " +
            "display all of ObjectToDisplay's abilities, and then place each ability " +
            "into exactly one display")]
        public void Refresh_DisplaysSpawnedAndInitalized() {
            var abilities = new List<IUnitAbilityDefinition>() {
                BuildAbility(), BuildAbility(), BuildAbility()
            };

            var unit = BuildUnit(abilities);

            var abilitiesDisplay = Container.Resolve<UnitAbilitiesDisplay>();

            abilitiesDisplay.ObjectToDisplay = unit;
            abilitiesDisplay.Refresh();

            Assert.AreEqual(abilities.Count, InstantiatedDisplays.Count,
                "UnitAbilitiesDisplay instantiated an unexpected number of IAbilityDisplays");

            foreach(var ability in abilities) {
                Assert.AreEqual(1, InstantiatedDisplays.Where(display => display.Object.AbilityToDisplay.Equals(ability)).Count(),
                    "An ability isn't represented in some display");                
            }

            foreach(var display in InstantiatedDisplays) {
                Assert.AreEqual(unit, display.Object.UnitToInvokeOn, "A display has an unexpected UnitToInvokeOn");
                display.Verify(displ => displ.Refresh(), Times.Once, "A display wasn't refreshed");
            }
        }

        [Test(Description = "When Refresh is called multiple times, UnitAbilitiesDisplay will despawn unused " +
            "IAbilityDisplays and only hold just enough active displays to handle its current needs")]
        public void RefreshMultipleTimes_DespawnsUnusedDisplays() {
            var unitOne   = BuildUnit(new List<IUnitAbilityDefinition>() { BuildAbility(), BuildAbility(), BuildAbility()                 });
            var unitTwo   = BuildUnit(new List<IUnitAbilityDefinition>() { BuildAbility(), BuildAbility(), BuildAbility(), BuildAbility() });
            var unitThree = BuildUnit(new List<IUnitAbilityDefinition>() { BuildAbility(), BuildAbility()                                 });

            var abilitiesDisplay = Container.Resolve<UnitAbilitiesDisplay>();

            abilitiesDisplay.ObjectToDisplay = unitOne;
            abilitiesDisplay.Refresh();

            abilitiesDisplay.ObjectToDisplay = unitTwo;
            abilitiesDisplay.Refresh();

            abilitiesDisplay.ObjectToDisplay = unitThree;
            abilitiesDisplay.Refresh();

            var memoryPool = Container.Resolve<AbilityDisplayMemoryPool>();

            Assert.AreEqual(2, memoryPool.NumActive, "MemoryPool has an unexpected number of active items");
            Assert.AreEqual(2, memoryPool.NumInactive, "MemoryPool has an unexpected number of inactive items");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(List<IUnitAbilityDefinition> abilities) {
            var mockUnit = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.Abilities).Returns(abilities);

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            return mockUnit.Object;
        }

        private IUnitAbilityDefinition BuildAbility() {
            return new Mock<IUnitAbilityDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
