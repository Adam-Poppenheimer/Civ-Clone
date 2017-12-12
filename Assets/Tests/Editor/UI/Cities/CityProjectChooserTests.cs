using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Core;
using Assets.Simulation.Units;

using Assets.UI.Cities;
using Assets.UI.Cities.Production;

namespace Assets.Tests.UI.Cities {

    public class CityProjectChooserTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBuildingProductionValidityLogic> MockBuildingValidityLogic;

        private Mock<IUnitProductionValidityLogic> MockUnitValidityLogic;

        private List<IBuildingTemplate> ValidBuildingTemplates = new List<IBuildingTemplate>();

        private List<IUnitTemplate> ValidUnitTemplates = new List<IUnitTemplate>();

        private Dropdown ProjectDropdown;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            ValidBuildingTemplates.Clear();
            ValidUnitTemplates.Clear();

            MockBuildingValidityLogic = new Mock<IBuildingProductionValidityLogic>();
            MockUnitValidityLogic     = new Mock<IUnitProductionValidityLogic>();

            MockBuildingValidityLogic
                .Setup(logic => logic.GetTemplatesValidForCity(It.IsAny<ICity>()))
                .Returns(() => ValidBuildingTemplates);            

            MockUnitValidityLogic
                .Setup(logic => logic.GetTemplatesValidForCity(It.IsAny<ICity>()))
                .Returns(() => ValidUnitTemplates);

            Container.Bind<IBuildingProductionValidityLogic>().FromInstance(MockBuildingValidityLogic.Object);
            Container.Bind<IUnitProductionValidityLogic>().FromInstance(MockUnitValidityLogic.Object);

            ProjectDropdown = Container.InstantiateComponentOnNewGameObject<Dropdown>();

            Container.Bind<Dropdown>().WithId("Project Dropdown").FromInstance(ProjectDropdown);

            Container.Bind<CityProjectChooser>().FromNewComponentOnNewGameObject().AsSingle();

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<SlotDisplayClickedSignal>();
        }

        #endregion

        #region tests

        [Test(Description = "When Refresh is called and ObjectToDisplay is null, " +
            "nothing significant should happen and no exception should be thrown")]
        public void RefreshCalled_DoesNothingOnNullCity() {
            var projectChooser = Container.Resolve<CityProjectChooser>();

            Assert.DoesNotThrow(() => projectChooser.Refresh(),
                "CityProjectChooser.Refresh threw an unexpected exception on a null ObjectToDisplay");

            Assert.AreEqual(0, ProjectDropdown.options.Count,
                "ProjectDropdown's options list has an unexpected number of elements");
        }

        [Test(Description = "When Refresh is called, ProjectDropdown should have its options refreshed. " +
            "Those options should consists of a single option called 'None', followed by options for "+ 
            "every valid building template, followed by options for every valid unit template")]
        public void RefreshCalled_DropdownGivenAppropriateOptions() {
            BuildBuildingTemplate("Building Template One", true);
            BuildBuildingTemplate("Building Template Two", true);
            BuildBuildingTemplate("Building Template Three", true);

            BuildUnitTemplate("Unit Template One", true);
            BuildUnitTemplate("Unit Template Two", true);
            BuildUnitTemplate("Unit Template Three", true);

            var cityMock = BuildCity(null);

            var projectChooser = Container.Resolve<CityProjectChooser>();

            projectChooser.ObjectToDisplay = cityMock.Object;
            projectChooser.Refresh();

            Assert.AreEqual("None", ProjectDropdown.options[0].text, "Options[0].text has an unexpected value");
            
            var options = ProjectDropdown.options;

            var optionsByText = options.Where(option => options.First() != option).Select(option => option.text);

            var buildingTemplatesByName = ValidBuildingTemplates.Select(template => template.name);
            var unitTemplatesByName     = ValidUnitTemplates    .Select(template => template.Name);

            var templatesByName = buildingTemplatesByName.Concat(unitTemplatesByName);

            CollectionAssert.AreEqual(templatesByName, optionsByText,
                "ProjectDropdown.options does not have the expected options");
        }

        [Test(Description = "When Refresh is called, ProjectDropdown should be set to the option that " +
            "corresponds to ObjectToDisplay's active project")]
        public void RefreshCalled_AppropriateDropdownSelected() {
            var cityMock = BuildCity(BuildBuildingTemplate("Active Project Template", true));

            BuildBuildingTemplate("Inactive Template 1", true);
            BuildBuildingTemplate("Inactive Template 2", true);

            var projectChooser = Container.Resolve<CityProjectChooser>();

            projectChooser.ObjectToDisplay = cityMock.Object;
            projectChooser.Refresh();

            Assert.AreEqual(cityMock.Object.ActiveProject.Name, ProjectDropdown.options[ProjectDropdown.value].text,
                "ProjectDropdown's selected option doesn't have the same name as the city's active project");
        }

        [Test(Description = "When Refresh is called and ObjectToDisplay has a null active project, " +
            "ProjectDropdown's value should be set to zero, which corresponds to the 'None' option")]
        public void RefreshCalled_NoneSelectedIfNullProject() {
            BuildBuildingTemplate("Template One", true);
            BuildBuildingTemplate("Template Two", true);
            BuildBuildingTemplate("Template Three", true);

            var cityMock = BuildCity(null);

            var projectChooser = Container.Resolve<CityProjectChooser>();

            projectChooser.ObjectToDisplay = cityMock.Object;
            projectChooser.Refresh();

            Assert.AreEqual(0, ProjectDropdown.value, "ProjectDropdown.value did not reset to zero " +
                "when ObjectToDisplay's ActiveProject was null");
        }

        [Test(Description = "When ProjectDropdown fires its onValueChanged event and ObjectToDisplay " +
            "is not null, ObjectToDisplay's active project should be set to a template with the same " +
            "name as the text in the option selected")]
        public void ProjectDropdownChanged_ProjectIsUpdated() {
            var templateOne = BuildBuildingTemplate("Template One", true);
            var templateTwo = BuildBuildingTemplate("Template Two", true);

            var templateThree = BuildUnitTemplate("Template Three", true);

            var cityMock = BuildCity(null);

            var projectChooser = Container.Resolve<CityProjectChooser>();

            projectChooser.ObjectToDisplay = cityMock.Object;
            projectChooser.Refresh();

            ProjectDropdown.onValueChanged.Invoke(1);
            cityMock.Verify(city => city.SetActiveProductionProject(templateOne), Times.Once,
                "Invoking on TemplateOne did not call ObjectToDisplay.SetActiveProductionProject properly");

            ProjectDropdown.onValueChanged.Invoke(2);
            cityMock.Verify(city => city.SetActiveProductionProject(templateTwo), Times.Once,
                "Invoking on TemplateTwo did not call ObjectToDisplay.SetActiveProductionProject properly");
            
            ProjectDropdown.onValueChanged.Invoke(3);
            cityMock.Verify(city => city.SetActiveProductionProject(templateThree), Times.Once,
                "Invoking on TemplateThree did not call ObjectToDisplay.SetActiveProductionProject properly");
        }

        [Test(Description = "When ProjectDropdown fires its onValueChanged event and ObjectToDisplay " +
            "is null, no exception should be thrown")]
        public void ProjectDropdownChanged_DoesNotThrowOnNullCity() {
            Container.Resolve<CityProjectChooser>();

            Assert.DoesNotThrow(() => ProjectDropdown.onValueChanged.Invoke(1),
                "ProjectDropdown.onValueChanged invocation falsely caused an error when ObjectToDisplay was null");
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildBuildingTemplate(string name, bool isValid) {
            var mockTemplate = new Mock<IBuildingTemplate>();
            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            if(isValid) {
                ValidBuildingTemplates.Add(newTemplate);
            }

            MockBuildingValidityLogic.Setup(logic => logic.IsTemplateValidForCity(newTemplate, It.IsAny<ICity>())).Returns(isValid);

            return newTemplate;
        }

        private IUnitTemplate BuildUnitTemplate(string name, bool isValid) {
            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.Name).Returns(name);

            var newTemplate = mockTemplate.Object;

            if(isValid) {
                ValidUnitTemplates.Add(newTemplate);
            }

            MockUnitValidityLogic.Setup(logic => logic.IsTemplateValidForCity(newTemplate, It.IsAny<ICity>())).Returns(isValid);

            return newTemplate;
        }

        private Mock<ICity> BuildCity(IBuildingTemplate activeTemplate) {
            var mockCity = new Mock<ICity>();

            if(activeTemplate != null) {
                var mockProject = new Mock<IProductionProject>();
                mockProject.Setup(project => project.Name).Returns(activeTemplate.name);

                mockCity.Setup(city => city.ActiveProject).Returns(mockProject.Object);
            }

            return mockCity;
        }

        #endregion

        #endregion

    }

}
