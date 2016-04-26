using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
	public class CodeGeneratorFactory
	{
		internal class ProjectTypes
		{
			public const string Project = "ava_project";
			public const string SubProject = "ava_subproject";
			public const string Work = "ava_work";
		}


		private readonly IOrganizationService service;

		public CodeGeneratorFactory(IOrganizationService service)
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			this.service = service;
		}


		public ICodeGenerator CreateCodeGeneratorFor(EntityReference targetRef)
		{
			switch (targetRef.LogicalName)
			{
				case ProjectTypes.Project:
					return new ProjectCodeGenerator(targetRef, service);
					
				case ProjectTypes.SubProject:
					return new SubProjectCodeGenerator(targetRef, service);
					
				case ProjectTypes.Work:
					return new WorkCodeGenerator(targetRef, service);
										
				default:
					throw new InvalidPluginExecutionException(OperationStatus.Failed, "Cannot generate a code for a record of type " + targetRef.LogicalName);
			}
		}
	}
}
