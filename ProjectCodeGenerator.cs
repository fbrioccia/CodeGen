using Inc.Xrm.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CodeGenerator
{
	public class ProjectCodeGenerator : ICodeGenerator
	{
		private EntityReference entityReference;
		private IOrganizationService service;
		private const string parentEntityName = "ava_contract";
		private const string parentEntityLookupFieldName = "ava_contractid";
        private const int projectCodeLevel = 2;    //the second part of an ava_code identifies the project
        private const int projectCodePartLength = 1;

		public ProjectCodeGenerator(EntityReference entityReference, IOrganizationService service)
		{
			this.entityReference = entityReference;
			this.service = service;
		}

		public string GenerateCode()
		{
			if (CodeGeneratorHelper.IsContractUnpublished(service, entityReference))
			{
				return null;
			}

			string parentAvaCode = CodeGeneratorHelper.GetParentAvaCode(service, entityReference, parentEntityName, parentEntityLookupFieldName);

			ColumnSet columnSet = new ColumnSet(parentEntityLookupFieldName);
			var childEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, columnSet);
			var parentEntityReference = childEntity.GetAttributeValue<EntityReference>(parentEntityLookupFieldName);

            var parentProjects = CodeGeneratorHelper.GetChildsList(service, parentEntityReference, entityReference.LogicalName, parentEntityLookupFieldName, new ColumnSet("ava_code"));

			var projectsFromParentWithoutCurrent = parentProjects.Where(x => x.Id != entityReference.Id).ToList();

			var codesAlreadyAssigned = CodeGeneratorHelper.GetCodesAlreadyAssigned(projectCodeLevel, projectsFromParentWithoutCurrent);

            int partialProjectCodeToConvert = CodeGeneratorHelper.GenerateAssignableIntCodeValue(projectCodePartLength, codesAlreadyAssigned);
            string partialProjectCode = CodeGeneratorHelper.ConvertIntToCharCode(projectCodePartLength, partialProjectCodeToConvert);

			string projectCode = parentAvaCode + "-" + partialProjectCode;

			return projectCode;
		}
	}
}
