using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Cities;

namespace Assets.Cities.Buildings {

    public class TemplateValidityLogic : ITemplateValidityLogic {

        #region instance fields and properties

        private List<IBuildingTemplate> AvailableTemplates;

        #endregion

        #region constructors

        [Inject]
        public TemplateValidityLogic(List<IBuildingTemplate> availableTemplates) {
            AvailableTemplates = availableTemplates;
        }

        #endregion

        #region instance methods

        #region from IBuildingValidityLogic

        public IEnumerable<IBuildingTemplate> GetTemplatesValidForCity(ICity city) {
            if(city == null) {
                throw new ArgumentNullException("city");
            }

            return AvailableTemplates;
        }

        public bool IsTemplateValidForCity(IBuildingTemplate template, ICity city) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(city == null) {
                throw new ArgumentNullException("city");
            }

            return AvailableTemplates.Contains(template);
        }

        #endregion

        #endregion
        
    }

}
