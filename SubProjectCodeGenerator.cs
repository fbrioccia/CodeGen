using Inc.Xrm.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inc.Xrm.Plugins.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CodeGenerator
{
    public class SubProjectCodeGenerator : ICodeGenerator
    {
	private EntityReference entityReference;
	private IOrganizationService service;
	private const string parentEntityName = "ava_project";
	private const string parentEntityLookupFieldName = "ava_projectid";
        private const int subProjectCodeLevel = 3;    //the third part of an ava_code identifies the subProject
        private const int subProjectCodePartLength = 1;

	public SubProjectCodeGenerator(EntityReference entityReference, IOrganizationService service)
	{
		this.entityReference = entityReference;
		this.service = service;
	}

	public string GenerateCode()
        {
		string parentAvaCode = CodeGeneratorHelper.GetParentAvaCode(service, entityReference, parentEntityName, parentEntityLookupFieldName);

		if(string.IsNullOrEmpty(parentAvaCode))
		{
			if(CodeGeneratorHelper.IsContractUnpublished(service, entityReference))
			{
				return null;
			}

			throw new Exception();
		}

		ColumnSet columnSet = new ColumnSet(parentEntityLookupFieldName);
		var childEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, columnSet);
		var parentEntityReference = childEntity.GetAttributeValue<EntityReference>(parentEntityLookupFieldName);

            	var parentSubProjects = CodeGeneratorHelper.GetChildsList(service, parentEntityReference, entityReference.LogicalName, parentEntityLookupFieldName, new ColumnSet("ava_code"));

		var subProjectsFromParentWithoutCurrent = parentSubProjects.Where(x => x.Id != entityReference.Id).ToList();

		var codesAlreadyAssigned = CodeGeneratorHelper.GetCodesAlreadyAssigned(subProjectCodeLevel, subProjectsFromParentWithoutCurrent);

        	int partialSubProjectCodeToConvert = CodeGeneratorHelper.GenerateAssignableIntCodeValue(subProjectCodePartLength, codesAlreadyAssigned);
        	string partialSubProjectCode = CodeGeneratorHelper.ConvertIntToCharCode(subProjectCodePartLength, partialSubProjectCodeToConvert);

		string subProjectCode = parentAvaCode + "-" + partialSubProjectCode;

		return subProjectCode;
	}
    }
}
