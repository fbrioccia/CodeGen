using Inc.Xrm.Plugins;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;

namespace CodeGenerator
{
	public static class CodeGeneratorHelper
	{
		private const string entityCodeFieldName = "ava_code";

		private const string projectLookupFieldName = "ava_contractid";
		private const string subProjectLookupFieldName = "ava_projectid";
		private const string workLookupFieldName = "ava_subprojectid";

		private static readonly char[] intCharSet = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		private static readonly char[] upperCaseCharSet = Enumerable.Range('A', 26).Select(x => (char)x).ToArray();
		private static readonly char[] lowerCaseCharSet = Enumerable.Range('a', 26).Select(x => (char)x).ToArray();
		private static readonly List<string> codeList = new List<string>();


		public static string GetCodePartOfTheSpecifiedLevel(int codeLevel, string avaCode)
		{
			string[] splitCode = avaCode.Split('-');

			var codePart = string.Empty;

			codePart = splitCode[codeLevel - 1];

			return codePart;
		}


		public static List<int> GetWorksCodesAlreadyAssigned(int codeLevel, List<Entity> entityList)
		{
			List<int> codesWorksAlreadyAssigned = new List<int>();

			var codeListInstance = new CodeList();

			List<string> codeList = codeListInstance.FillMap();

			foreach (var entity in entityList)
			{
				string avaCode = entity.GetAttributeValue<string>("ava_code");
				if (avaCode != null)
				{
					string code = GetCodePartOfTheSpecifiedLevel(codeLevel, avaCode);
					int codeConverted = ConvertCharsToIntCodeForWorks(code, codeList);
					codesWorksAlreadyAssigned.Add(codeConverted);
				}
			}

			return codesWorksAlreadyAssigned;
		}


		public static List<int> GetCodesAlreadyAssigned(int codeLevel, List<Entity> entityList)
		{
			List<int> codesAlreadyAssigned = new List<int>();

			foreach (var entity in entityList)
			{
				string avaCode = entity.GetAttributeValue<string>("ava_code");
				if (avaCode != null)
				{
					string code = GetCodePartOfTheSpecifiedLevel(codeLevel, avaCode);
					int codeConverted = ConvertCharsToIntCode(code);
					codesAlreadyAssigned.Add(codeConverted);
				}
			}
			return codesAlreadyAssigned;
		}

		public static int GenerateAssignableIntCodeValue(int codeLength, List<int> intCodesAlreadyAssigned)
		{
			List<int> intSet = GenerateIntSet((codeLength * GetCharSet().Length) - 1);

			List<int> assignableIntCodeValuesSet = intSet.Except(intCodesAlreadyAssigned).ToList();

			return assignableIntCodeValuesSet.First();
		}

		public static string GenerateAssignableCodeValueForWorks(int codeLength, List<int> intCodesAlreadyAssigned)
		{
			var codeListInstance = new CodeList();

			List<string> codeList = codeListInstance.FillMap();

			foreach (var intCode in intCodesAlreadyAssigned)
			{
				codeList[intCode - 1] = string.Empty;
				//codeList.RemoveAt(intCode-1);	
			}
			codeList.RemoveAll(codeWork => codeWork == "");
			return codeList.First();
		}


		public static List<int> GenerateIntSet(int dimension)
		{
			int[] intSet = Enumerable.Range(1, dimension).Select(x => (int)x).ToArray();

			return intSet.ToList();
		}


		public static char[] GetCharSet()
		{
			char[] completeSet = new char[intCharSet.Length + upperCaseCharSet.Length];

			intCharSet.CopyTo(completeSet, 0);
			upperCaseCharSet.CopyTo(completeSet, intCharSet.Length);
			

			return completeSet;
		}

		public static int CountSameLevelElements(IOrganizationService service, EntityReference entityReference)
		{
			QueryExpression query = new QueryExpression(entityReference.LogicalName);


			EntityCollection sameLevelEntities = service.RetrieveMultiple(query);

			return sameLevelEntities.Entities.Count;
		}

		public static int CountSameLevelElementsByParentReference(IOrganizationService service, EntityReference entityReference, EntityReference parentEntityReference, string parentEntityLookupFieldName)
		{
			QueryExpression query = new QueryExpression(entityReference.LogicalName);

			query.Criteria.AddCondition(new ConditionExpression(parentEntityLookupFieldName, ConditionOperator.Equal, parentEntityReference.Id));
			query.Criteria.AddCondition(new ConditionExpression(entityCodeFieldName, ConditionOperator.NotNull));

			EntityCollection sameLevelEntities = service.RetrieveMultiple(query);

			return sameLevelEntities.Entities.Count;
		}

		public static string ConvertIntToCharCode(int length, int numberToConvert)
		{

			if (length < 1 || length > 2)
			{
				throw new Exception("You can only convert a code with a maximum of two digits.");
			}

			char[] digit = new char[intCharSet.Length + upperCaseCharSet.Length + lowerCaseCharSet.Length];
			intCharSet.CopyTo(digit, 0);
			lowerCaseCharSet.CopyTo(digit, intCharSet.Length);
			upperCaseCharSet.CopyTo(digit, lowerCaseCharSet.Length + intCharSet.Length);

			int targetBase = digit.Length;
			string result = "";

			if (numberToConvert >= Math.Pow(targetBase, length))
				throw new Exception("Overflow: The provided number cannot be expressed in a string of " + length + " character/s");

			do
			{
				result = digit[numberToConvert % targetBase] + result;
				numberToConvert = numberToConvert / targetBase;
			} while (numberToConvert > 0);

			return (result.Length == length) ? result : result.PadLeft(length, '0');
		}


		public static string ConvertIntToCharCodeForWorks(string charsToConvert)
		{
			int foundAt = 0;

			var number = 0;
			int result = 0;
			var isInt = Int32.TryParse(charsToConvert, out number);

			if (isInt == true && number != 99)
			{
				result = number;
			}


			if (isInt == false)
			{
				var pos = 0;
				pos = codeList.IndexOf(charsToConvert);
				result = pos;
			}

			return result.ToString();
		}

		public static int ConvertCharsToIntCodeForWorks(string charsToConvert, List<string> codeList)
		{
			int foundAt = 0;

			var number = 0;
			int result = 0;
			var isInt = Int32.TryParse(charsToConvert, out number);

			if (isInt == true && number != 99)
			{
				result = number;
			}


			if (isInt == false)
			{
				var pos = 0;
				pos = codeList.IndexOf(charsToConvert);
				result = pos;
			}

			return result;
		}

		public static void fillCodeMap()
		{
			for (var i = 1; i < 100; i++)
			{
				codeList.Add(i.ToString());
			}
			
			foreach (char c in upperCaseCharSet)
			{
				foreach (char c1 in upperCaseCharSet)
				{
					string code = c.ToString() + c1.ToString();
					codeList.Add(code);
				}
			}
		}

		public static int ConvertCharsToIntCode(string charsToConvert)
		{
			int intCode = 0;
			char[] codingCharSet = GetCharSet();
			int targetBase = codingCharSet.Length;

			for (int i = 1; i <= charsToConvert.Length; i++)
			{
				char currentCharOfTheCode = charsToConvert[charsToConvert.Length - i];
				int indexOfCurrentCharOfTheCode = Array.IndexOf(codingCharSet, currentCharOfTheCode);

				intCode += (int)Math.Pow(targetBase, i - 1) * indexOfCurrentCharOfTheCode;
			}
			return intCode;
		}

		public static string GetParentAvaCode(IOrganizationService service, EntityReference childEntityReference, string parentEntityName, string parentEntityLookupFieldName)
		{
			var columnSet = new ColumnSet(parentEntityLookupFieldName);
			var childEntity = service.Retrieve(childEntityReference.LogicalName, childEntityReference.Id, columnSet);
			var parentEntityReference = childEntity.GetAttributeValue<EntityReference>(parentEntityLookupFieldName);
			if (parentEntityReference == null)
			{
				throw new InvalidPluginExecutionException(""); // TODO: mettere un errore parlante
			}


			columnSet = new ColumnSet(entityCodeFieldName);
			var parentEntity = service.Retrieve(parentEntityReference.LogicalName, parentEntityReference.Id, columnSet);
			string parentAvaCode = parentEntity.GetAttributeValue<string>(entityCodeFieldName);

			return parentAvaCode;
		}

		public static List<Entity> GetChildsList(IOrganizationService service, EntityReference parentEntityReference, string childEntityLogicalName, string childEntityLookupName, ColumnSet columnSet = null)
		{
			if (columnSet == null)
			{
				columnSet = new ColumnSet(true);
			}

			QueryExpression query = new QueryExpression(childEntityLogicalName);

			query.Criteria.AddCondition(new ConditionExpression(childEntityLookupName, ConditionOperator.Equal, parentEntityReference.Id));

			query.ColumnSet = columnSet;

			EntityCollection childEntities = service.RetrieveMultiple(query);

			return childEntities.Entities.ToList();
		}

		public static void SetAvaCode(IOrganizationService service, EntityReference entityReference, string avaCodeValue)
		{
			var entity = new Entity(entityReference.LogicalName);
			entity.Id = entityReference.Id;
			entity.Attributes["ava_code"] = avaCodeValue;

			service.Update(entity);
		}

		public static void CleanEntitiesAvaCode(IOrganizationService service, List<Entity> entityList)
		{
			foreach (var entity in entityList)
			{
				CleanAvaCode(service, entity.ToEntityReference());
			}
		}

		public static void CleanAvaCode(IOrganizationService service, EntityReference entityReference)
		{
			var entity = new Entity(entityReference.LogicalName);
			entity.Id = entityReference.Id;
			entity.Attributes["ava_code"] = null;

			service.Update(entity);
		}

		public static Entity GetEntityToUpdate(EntityReference entityReference, string avaCodeValue)
		{
			var entity = new Entity(entityReference.LogicalName);
			entity.Id = entityReference.Id;
			entity.Attributes["ava_code"] = avaCodeValue;

			return entity;
		}

		public static bool IsContractUnpublished(IOrganizationService service, EntityReference entityReference)
		{
			Entity currentEntity;
			EntityReference parentEntityReference;

			switch (entityReference.LogicalName)
			{
				case "ava_contract":
					currentEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet("ava_isunpublished"));
					return currentEntity.GetAttributeValue<bool>("ava_isunpublished");
				case "ava_project":
					currentEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(projectLookupFieldName));
					parentEntityReference = currentEntity.GetAttributeValue<EntityReference>(projectLookupFieldName);
					return IsContractUnpublished(service, parentEntityReference);
				case "ava_subproject":
					currentEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(subProjectLookupFieldName));
					parentEntityReference = currentEntity.GetAttributeValue<EntityReference>(subProjectLookupFieldName);
					return IsContractUnpublished(service, parentEntityReference);
				case "ava_work":
					currentEntity = service.Retrieve(entityReference.LogicalName, entityReference.Id, new ColumnSet(workLookupFieldName));
					parentEntityReference = currentEntity.GetAttributeValue<EntityReference>(workLookupFieldName);
					return IsContractUnpublished(service, parentEntityReference);
				default:
					throw new InvalidPluginExecutionException(OperationStatus.Failed, "Cannot determine the contract state for record of type " + entityReference.LogicalName);
			}
		}

	}


	public class CodeList
	{
		private List<string> codeList = new List<string>();
		private static readonly char[] upperCaseCharSet = Enumerable.Range('A', 26).Select(x => (char)x).ToArray();


		public List<string> FillMap()
		{
			for (var i = 1; i < 100; i++)
			{
				if (i < 10)
				{
					codeList.Add("0" + i.ToString());
				}
				else
				{
					codeList.Add(i.ToString());
				}
			}

			foreach (char c in upperCaseCharSet)
			{
				foreach (char c1 in upperCaseCharSet)
				{
					string code = c.ToString() + c1.ToString();
					codeList.Add(code);
				}
			}
			return codeList;
		}
	}
}
