using Inc.Xrm.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inc.Xrm.Plugins.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CodeGenerator
{
    public class WorkCodeGenerator : ICodeGenerator
    {
		private EntityReference entityReference;
		private IOrganizationService service;
        private const int workCodeLevel = 4;    //the fourth part of an ava_code identifies the work
        private const int workCodePartLength = 2;


		public WorkCodeGenerator(EntityReference entityReference, IOrganizationService service)
		{
			this.entityReference = entityReference;
			this.service = service;
		}

		public string GenerateCode()
        {
			string parentAvaCode = CodeGeneratorHelper.GetParentAvaCode(service, entityReference, SubProject.LogicalName, Work.ava_subprojectid);

			if (string.IsNullOrEmpty(parentAvaCode))
			{
				if (CodeGeneratorHelper.IsContractUnpublished(service, entityReference))
				{
					return null;
				}
				
				throw new Exception();
			}

			var columnSet = new ColumnSet(Work.ava_subprojectid);
			var childEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, columnSet);
			var parentEntityReference = childEntity.GetAttributeValue<EntityReference>(Work.ava_subprojectid);

			var parentWorks = CodeGeneratorHelper.GetChildsList(service, parentEntityReference, entityReference.LogicalName, Work.ava_subprojectid, new ColumnSet("ava_code"));

			var worksFromParentWithoutCurrent = parentWorks.Where(x => x.Id != entityReference.Id).ToList();

			var codesAlreadyAssigned = CodeGeneratorHelper.GetWorksCodesAlreadyAssigned(workCodeLevel, worksFromParentWithoutCurrent);

			string partialWorkCode = CodeGeneratorHelper.GenerateAssignableCodeValueForWorks(workCodePartLength, codesAlreadyAssigned);

			var workCode = string.Format("{0}-{1}", parentAvaCode, partialWorkCode);

			return workCode;
		}
    }
}
