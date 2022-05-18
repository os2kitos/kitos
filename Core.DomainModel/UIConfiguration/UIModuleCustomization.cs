using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel.Constants;

namespace Core.DomainModel.UIConfiguration
{
    public class UIModuleCustomization : Entity, IOwnedByOrganization
    {
        public UIModuleCustomization()
        {
            Nodes = new List<CustomizedUINode>();
        }
        public int OrganizationId { get; set; }
        
        /// <summary>
        /// Application module, e.g. It-System
        /// </summary>
        public string Module { get; set; }
        
        public virtual Organization.Organization Organization{ get; set; }
        public virtual ICollection<CustomizedUINode> Nodes { get; set; }

        public Maybe<OperationError> UpdateConfigurationNodes(IEnumerable<CustomizedUINode> nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException("Nodes parameter cannot be null");

            var customizedUiNodes = nodes.ToList();

            var keysValidity = CheckKeysValidity(customizedUiNodes);
            if (keysValidity.HasValue)
                return new OperationError(keysValidity.Value, OperationFailure.BadInput);

            customizedUiNodes.MirrorTo(Nodes, x => $"K:{x.Key}_S:{x.Enabled}");
            return Maybe<OperationError>.None;
        }

        private static Maybe<string> CheckKeysValidity(IEnumerable<CustomizedUINode> configurations)
        {
            var customizedUiNodes = configurations.ToList();
            var searchExpression = new Regex(UIModuleConfigurationConstants.ConfigurationKeyRegex);

            //check if every key matches the Regex expression
            var incorrectKeys = customizedUiNodes.Where(x => searchExpression.Matches(x.Key).Count < 1).ToList();
            if (incorrectKeys.Count > 0)
                return $" One or more keys are incorrect: {string.Join(", ", incorrectKeys)}";

            //check if every key is unique
            var groupedKeys = customizedUiNodes.GroupBy(x => x.Key);
            var duplicateKeys = (from keyGroup in groupedKeys where keyGroup.Count() > 1 select keyGroup.Key).ToList();

            return duplicateKeys.Count > 0
                ? $"One or more keys are duplicate: {string.Join(",", incorrectKeys)}"
                : Maybe<string>.None;
        }
    }
}
