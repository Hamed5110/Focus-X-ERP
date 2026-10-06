using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Focus.Common.DataStructs;
using Focus.DatabaseFactory;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Focus.RD.BL;

public class RDBackend
{
	private const string START_DATE = "@iStartDate";

	private const string END_DATE = "@iEndDate";

	private const string USER_ID = "@iUserId";

	private const string BLANK_STRING = "''";

	private const char VAR_START_CHAR = '@';

	private const char SPLIT_CHAR = '.';

	private Database m_objDb;

	private int m_iCompId;

	private string m_strSuffix;

	private RepRecord m_objRec;

	public StandardQuery GetRDQuery(RepRecord objRec, Database objDb, int iCompId, ref DataSourceType oBaseDatasourceType)
	{
		StandardQuery standardQuery = null;
		RDReport rDReport = null;
		_Reports reports = null;
		string text = null;
		string strQuery = null;
		string[] arrTables = null;
		m_objDb = objDb;
		m_iCompId = iCompId;
		m_strSuffix = FConvert.GetSuffix(iCompId);
		m_objRec = objRec;
		rDReport = new RDReport();
		try
		{
			if (objRec.ReportId == RDCommon.RD_STARTID)
			{
				reports = objRec.ReportCustomize;
			}
			else
			{
				reports = rDReport.LoadReportDesigner(objRec.ReportId, objRec.LayoutId, m_iCompId);
				oBaseDatasourceType = reports.DataSourceType;
			}
			DataSourceType dataSourceType = reports.DataSourceType;
			if ((uint)(dataSourceType - 1) <= 1u)
			{
				standardQuery = new StandardQuery();
				strQuery = ((reports.DataSourceType != DataSourceType.Query) ? make_view_query(reports) : reports.RDQuery.SqlQuery);
				replace_inputvariables(reports.Parameters, objRec, ref strQuery, m_iCompId);
				reports.RDQuery.SqlQuery = strQuery;
				if (reports.Filters != null && reports.Filters.Length != 0)
				{
					text = filter_string(reports.Filters, objRec, m_iCompId);
					if (text != null && text.Length > 0)
					{
						strQuery = ((!strQuery.ToLower().Contains("where ")) ? (strQuery + $" where {text} ") : (strQuery + $" and {text} "));
					}
				}
				standardQuery.Query = strQuery;
			}
		}
		catch (Exception ex)
		{
			strQuery = ex.Message;
		}
		if (objRec.ExtraColumns == null)
		{
			objRec.ExtraColumns = GetAllColumns(objRec, strQuery, arrTables);
		}
		return standardQuery;
	}

	private void replace_inputvariables(_Parameter[] arrParams, RepRecord objRec, ref string strQuery, int iCompId)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = null;
		string empty = string.Empty;
		if (strQuery.Length <= 0)
		{
			return;
		}
		if (strQuery.Contains("@iStartDate"))
		{
			if (objRec != null)
			{
				strQuery = strQuery.Replace("@iStartDate", objRec.StartingDate.ToString());
			}
			else
			{
				strQuery = strQuery.Replace("@iStartDate", "''");
			}
		}
		if (strQuery.Contains("@iEndDate"))
		{
			if (objRec != null && objRec.EndingDate > 0)
			{
				strQuery = strQuery.Replace("@iEndDate", objRec.EndingDate.ToString());
			}
			else
			{
				strQuery = strQuery.Replace("@iEndDate", "''");
			}
		}
		if (strQuery.Contains("@iUserId"))
		{
			if (objRec != null && objRec.UserId > 0)
			{
				strQuery = strQuery.Replace("@iUserId", objRec.UserId.ToString());
			}
			else
			{
				strQuery = strQuery.Replace("@iUserId", "''");
			}
		}
		if (arrParams != null && arrParams.Length != 0)
		{
			new InvoiceLayout();
			for (num = 0; num < arrParams.Length; num++)
			{
				empty = string.Empty;
				if (!strQuery.Contains(arrParams[num].FieldVariable))
				{
					continue;
				}
				if (arrParams[num].SelectionMode == _SelectionMode.Multiple)
				{
					if (objRec.Masters != null && objRec.Masters.Length != 0)
					{
						empty = General.GetMasters(objRec.Masters);
						num3 = 1;
					}
				}
				else if (arrParams[num].SelectionMode == _SelectionMode.Single && arrParams[num].FieldType == _FieldType.Master && arrParams[num].IsGroupMaster)
				{
					if (objRec.Code > 0)
					{
						num3 = 1;
						empty = GetMasterUnderGroup(objRec.Code, arrParams[num].ControlId, m_objDb, m_iCompId);
					}
					else
					{
						empty = "1";
					}
				}
				else if (arrParams[num].FieldType == _FieldType.Normal && arrParams[num].ControlId * -1 == -2)
				{
					empty = GetInputText(objRec, arrParams[num].FieldId);
					if (!string.IsNullOrEmpty(empty))
					{
						empty = $"'{empty}'";
					}
					num3 = 1;
				}
				else
				{
					if (arrParams[num].FieldType == _FieldType.Normal && arrParams[num].ControlId * -1 == -5)
					{
						num3 = Convert.ToInt32(FConvert.GetInputValue(objRec.Inputs, arrParams[num].FieldId));
						strQuery = strQuery.Replace(arrParams[num].FieldVariable, num3.ToString());
						continue;
					}
					if (objRec.Inputs != null)
					{
						num3 = Convert.ToInt32(FConvert.GetInputValue(objRec.Inputs, arrParams[num].FieldId));
					}
					else if (objRec.Code > 0)
					{
						num3 = objRec.Code;
					}
					empty = num3.ToString();
				}
				if (empty.Length > 0 && num3 > 0)
				{
					strQuery = strQuery.Replace(arrParams[num].FieldVariable, empty);
				}
				else
				{
					strQuery = strQuery.Replace(arrParams[num].FieldVariable, string.Format("{0} OR 1=1", "''"));
				}
			}
			return;
		}
		int num4 = 0;
		while (strQuery.Contains('@') && num4 <= 255)
		{
			num2 = strQuery.IndexOf('@');
			if (num2 > -1)
			{
				for (num = num2; num < strQuery.Length && strQuery[num] != ' ' && strQuery[num] != '\r' && strQuery[num] != '\n' && strQuery[num] != ')' && num < strQuery.Length; num++)
				{
					text += strQuery[num];
				}
				if (text.Contains("'"))
				{
					text = text.Replace(",", string.Empty);
					num--;
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (objRec != null)
					{
						strQuery = strQuery.Replace(text, string.Format("{0} OR 1=1", "''"));
					}
					else
					{
						strQuery = strQuery.Replace(text, "''");
					}
					text = null;
				}
			}
			num4++;
		}
	}

	private string GetInputText(RepRecord objRec, int iFieldId)
	{
		byte b = 0;
		if (objRec != null && objRec.Inputs != null)
		{
			for (b = 0; b < objRec.Inputs.Length; b++)
			{
				if (objRec.Inputs[b].ID == iFieldId)
				{
					return Convert.ToString(objRec.Inputs[b].Value);
				}
			}
		}
		return null;
	}

	private string make_view_query(_Reports objReport)
	{
		int num = 0;
		StringBuilder stringBuilder = null;
		string text = null;
		stringBuilder = new StringBuilder();
		stringBuilder.Append(EnumUtils.DescriptionOf(SQLKey.SELECT));
		for (num = 0; num < objReport.Layout.Columns.Length; num++)
		{
			if (num > 0)
			{
				stringBuilder.Append(",");
			}
			stringBuilder.AppendFormat("{0}", objReport.Layout.Columns[num].ColumnName);
		}
		stringBuilder.Append(EnumUtils.DescriptionOf(SQLKey.FROM));
		stringBuilder.AppendFormat(" {0}", objReport.RDQuery.SqlQuery);
		if (objReport.Parameters != null && objReport.Parameters.Length != 0 && objReport.Filters != null && objReport.Filters.Length != 0)
		{
			text = string.Empty;
			for (int i = 0; i < objReport.Parameters.Length; i++)
			{
				for (num = 0; num < objReport.Filters.Length; num++)
				{
					string viewFieldName = GetViewFieldName(objReport.Filters[num].FieldId, objReport.Filters[num].DataType, objReport.RDQuery.SqlQuery, m_iCompId);
					if (objReport.Parameters[i].SelectionMode == _SelectionMode.Multiple)
					{
						string masters = General.GetMasters(m_objRec.Masters);
						if (num > 0)
						{
							text += " AND";
						}
						text = text + " " + viewFieldName + " IN (" + masters + ")";
					}
					else
					{
						int inputValue = FConvert.GetInputValue(m_objRec.Inputs, objReport.Parameters[i].FieldId);
						if (num > 0)
						{
							text += " AND";
						}
						text += $" {viewFieldName} = {inputValue}";
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(EnumUtils.DescriptionOf(SQLKey.WHERE));
				stringBuilder.Append(text);
			}
		}
		return stringBuilder.ToString();
	}

	private FieldData[] GetAllColumns(RepRecord objRec, string sQuery, string[] arrTables)
	{
		int num = 0;
		FieldData fieldData = null;
		List<FieldData> list = new List<FieldData>();
		if (objRec.ReportCustomize != null && objRec.ReportCustomize.Layout != null && objRec.ReportCustomize.Layout.Columns != null)
		{
			fieldData = new FieldData();
			FieldData fieldData2 = fieldData;
			int fieldId = (fieldData.SubParentId = 0);
			fieldData2.FieldId = fieldId;
			fieldData.FieldName = "Report Specific.iHeaderId";
			fieldData.DataType = MasterDataType.Number;
			list.Add(fieldData);
			for (num = 0; num < objRec.ReportCustomize.Layout.Columns.Length; num++)
			{
				fieldData = new FieldData();
				fieldData.FieldId = objRec.ReportCustomize.Layout.Columns[num].FieldId;
				fieldData.SubParentId = objRec.ReportCustomize.Layout.Columns[num].SubParentId;
				fieldData.ColumnId = objRec.ReportCustomize.Layout.Columns[num].ColumnId;
				if (arrTables != null && arrTables.Length != 0 && num < arrTables.Length)
				{
					fieldData.ColumnAlias = objRec.ReportCustomize.Layout.Columns[num].AliasName;
					fieldData.FieldName = arrTables[num];
				}
				else
				{
					FieldData fieldData3 = fieldData;
					string columnAlias = (fieldData.FieldName = objRec.ReportCustomize.Layout.Columns[num].AliasName);
					fieldData3.ColumnAlias = columnAlias;
				}
				fieldData.DataType = objRec.ReportCustomize.Layout.Columns[num].DataTypeId;
				fieldData.Miscellaneous = objRec.ReportCustomize.Layout.Columns[num].Miscelleneous;
				list.Add(fieldData);
			}
		}
		else
		{
			for (num = 0; num < objRec.Columns.Length; num++)
			{
				fieldData = new FieldData();
				fieldData.FieldId = objRec.Columns[num].ID;
				fieldData.SubParentId = 0;
				fieldData.FieldName = objRec.Columns[num].Name;
				if (objRec.Columns[num].Tag != null)
				{
					fieldData.DataType = (MasterDataType)objRec.Columns[num].Tag;
				}
				list.Add(fieldData);
			}
		}
		return list.ToArray();
	}

	private string filter_string(_Filter[] arrFilterSource, RepRecord objRec, int iCompanyId)
	{
		string[] arrTables = null;
		return get_filter_string(null, arrFilterSource, objRec, iCompanyId, ref arrTables);
	}

	public string get_filter_string(_Parameter[] arrParams, _Filter[] arrFilterSource, RepRecord objRec, int iCompanyId, ref string[] arrTables)
	{
		bool flag = false;
		byte b = 0;
		m_iCompId = iCompanyId;
		string text = string.Empty;
		InvoiceLayout invoiceLayout = null;
		AdvanceFilterQuery advanceFilterQuery = null;
		List<AdvanceFilterQuery> list = null;
		if (arrFilterSource != null && arrFilterSource.Length != 0)
		{
			list = new List<AdvanceFilterQuery>();
			invoiceLayout = new InvoiceLayout();
			for (b = 0; b < arrFilterSource.Length; b++)
			{
				flag = false;
				advanceFilterQuery = new AdvanceFilterQuery();
				advanceFilterQuery.Conjunction = (int)arrFilterSource[b].Conjuction;
				advanceFilterQuery.Operator = (int)arrFilterSource[b].Operator;
				advanceFilterQuery.CompareWith = (int)arrFilterSource[b].CompareWith;
				advanceFilterQuery.CompareValue = GetFilterInputValue(arrParams, arrFilterSource[b].CompareValue, objRec, ref flag);
				advanceFilterQuery.FieldId = arrFilterSource[b].FieldId;
				if ((arrFilterSource[b].DataType == MasterDataType.Master || arrFilterSource[b].DataType == MasterDataType.Number) && arrFilterSource[b].SubParentId > 0)
				{
					if (arrFilterSource[b].IsGroup)
					{
						string extraFieldName = invoiceLayout.GetExtraFieldName(arrFilterSource[b].FieldId, arrFilterSource[b].SubParentId, -1, bVoucherClass: false, iCompanyId);
						if (extraFieldName.Split('.').Length > 1)
						{
							advanceFilterQuery.CompareValue = GetMastersInGroup(extraFieldName.Split('.')[0], Convert.ToInt32(arrFilterSource[b].CompareValue), m_objDb, iCompanyId);
							if (Convert.ToString(advanceFilterQuery.CompareValue).Contains(','))
							{
								flag = true;
								arrFilterSource[b].CompareText = null;
							}
						}
						advanceFilterQuery.TreeData = new TreeHierarchyDetails();
						advanceFilterQuery.TreeData.IsGroup = flag;
						advanceFilterQuery.TreeData.FieldId = arrFilterSource[b].FieldId;
						advanceFilterQuery.TreeData.SubParentId = arrFilterSource[b].SubParentId;
					}
					else if (arrFilterSource[b].DataType != MasterDataType.Number && !string.IsNullOrEmpty(arrFilterSource[b].CompareText) && arrFilterSource[b].FieldId > 300000)
					{
						advanceFilterQuery.CompareValue = arrFilterSource[b].CompareText;
					}
				}
				if (objRec.SourceType == DataSourceType.View)
				{
					advanceFilterQuery.Name = GetViewFieldName(arrFilterSource[b].FieldId, arrFilterSource[b].DataType, objRec.ReportCustomize.RDQuery.SqlQuery, iCompanyId);
				}
				else
				{
					advanceFilterQuery.Name = invoiceLayout.GetExtraFieldName(arrFilterSource[b].FieldId, arrFilterSource[b].SubParentId, -1, bVoucherClass: false, iCompanyId, bCallingFromFilter: true);
					advanceFilterQuery.Name = GetSpecialName(advanceFilterQuery.Name, arrFilterSource[b].FieldId, arrFilterSource[b].SubParentId, flag);
					if (advanceFilterQuery.Name.EndsWith("YH"))
					{
						advanceFilterQuery.CompareValue = arrFilterSource[b].CompareText;
					}
				}
				if (!string.IsNullOrEmpty(advanceFilterQuery.CompareValue))
				{
					list.Add(advanceFilterQuery);
				}
				CheckExistingTable(advanceFilterQuery.Name, ref arrTables);
			}
			if (list.Count > 0)
			{
				text = FConvert.GetFilterString(list.ToArray());
				text = text.Substring(5);
				if (list.Count > 1)
				{
					text = "(" + text + ")";
				}
			}
		}
		return text;
	}

	private string GetViewFieldName(int iFieldId, MasterDataType oDataType, string strViewName, int m_iCompId)
	{
		try
		{
			string text = "select top 1 * from " + strViewName;
			DbCommand dbCommand = null;
			dbCommand = m_objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			IDataReader dataReader = m_objDb.ExecuteReader(dbCommand);
			string name = dataReader.GetName(iFieldId - 1);
			dataReader.Close();
			return name;
		}
		catch
		{
		}
		if (oDataType != MasterDataType.Fraction && oDataType != MasterDataType.Number)
		{
			return "''";
		}
		return "0";
	}

	private string GetMastersInGroup(string strMasterType, int iMasterGroupId, Database objDb, int iCompanyId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<string> list = new List<string>();
		string empty = string.Empty;
		try
		{
			if (!string.IsNullOrEmpty(strMasterType) && iMasterGroupId > 0)
			{
				if (objDb == null)
				{
					objDb = DatabaseWrapper.GetDatabase2(iCompanyId, -1, bReplicationServer: true);
				}
				if (strMasterType == "BookNo" || strMasterType == "Code")
				{
					strMasterType = "vrCore_Account";
				}
				empty = string.Format("WITH RecursionCTE (iMasterId, iParentId, bGroup)  AS  \r\n                                (   SELECT iMasterId, iParentId, bGroup\r\n                                    FROM {0}\r\n                                    WHERE iMasterId = {1} AND iTreeId = 0\r\n                                    UNION ALL  \r\n                                    SELECT R1.iMasterId, R1.iParentId, R1.bGroup\r\n                                    FROM {0} AS R1\r\n                                    JOIN RecursionCTE AS R2 ON R1.iParentId = R2.iMasterId\r\n                                )SELECT iMasterId  FROM RecursionCTE WHERE bGroup = 0 ", strMasterType, iMasterGroupId);
				dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = objDb.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list.Add(Convert.ToString(dataReader[0]));
				}
				dataReader.Close();
			}
		}
		catch
		{
		}
		if (list.Count == 0)
		{
			list.Add(iMasterGroupId.ToString());
		}
		return string.Format("({0})", string.Join(",", list.ToArray()));
	}

	private void CheckExistingTable(string sFieldName, ref string[] arrTables)
	{
		if (arrTables != null && sFieldName.Split('.').Length >= 1 && arrTables.Length != 0)
		{
			string text = sFieldName.Split('.')[0];
			int num = 0;
			for (num = 0; num < arrTables.Length && !(arrTables[num] == text); num++)
			{
			}
			if (num >= arrTables.Length)
			{
				Array.Resize(ref arrTables, arrTables.Length + 1);
				arrTables[arrTables.Length - 1] = sFieldName;
			}
		}
	}

	private string GetSpecialName(string sName, int iFieldId, int iSubParentId, bool bMasterId)
	{
		string[] array = null;
		switch ((VTFIELDID)iFieldId)
		{
		case VTFIELDID.FDF_MODIFIED_BY:
			sName = sName.Replace("mSec_Users", "MUser");
			break;
		case VTFIELDID.FDF_USER_ID:
			sName = sName.Replace("mSec_Users", "EUser");
			break;
		default:
			if (sName.Contains("sName") && !sName.Contains("Language"))
			{
				sName = sName.Replace("sName", "iMasterId");
				break;
			}
			if (sName.Contains("iMasterId"))
			{
				if (sName.Contains(_focus.company(m_iCompId).faTagName))
				{
					sName = $"tCore_Data{m_strSuffix}.iFaTag";
				}
				else if (sName.Contains(_focus.company(m_iCompId).invTagName))
				{
					sName = $"tCore_Data{m_strSuffix}.iInvTag";
				}
				break;
			}
			array = sName.Split('.');
			switch ((VTFIELDID)iSubParentId)
			{
			case VTFIELDID.FDF_CODE:
				sName = $"Code.{array[1]}";
				break;
			case VTFIELDID.FDF_BOOKNO:
				sName = $"BookNo.{array[1]}";
				break;
			}
			break;
		}
		return sName;
	}

	private string GetFilterInputValue(_Parameter[] arrParams, string strFieldValue, RepRecord objRec, ref bool IsMasterId)
	{
		DbCommand dbCommand = null;
		int num = 0;
		int num2 = 0;
		bool bNextRangeField = false;
		string text = null;
		if (strFieldValue != null && strFieldValue.Length > 0 && strFieldValue[0] == '@')
		{
			text = $"SELECT ISNULL(iFieldId,0)[iFieldId] FROM cCore_ReportParameter{m_strSuffix} \r\n                WHERE sFieldVariable='{GetSpecificInputField(strFieldValue, ref bNextRangeField)}' AND iReportId={objRec.ReportId}";
			dbCommand = m_objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			num = Convert.ToInt32(m_objDb.ExecuteScalar(dbCommand));
			if (bNextRangeField && num != 0)
			{
				num--;
			}
			if (arrParams != null && arrParams.Length != 0)
			{
				for (num2 = 0; num2 < arrParams.Length; num2++)
				{
					if (arrParams[num2].FieldId != num || arrParams[num2].FieldType != _FieldType.Master)
					{
						continue;
					}
					IsMasterId = true;
					if (arrParams[num2].SelectionMode == _SelectionMode.Multiple)
					{
						if (objRec.Masters.Length > 1)
						{
							return $"({General.GetMasters(objRec.Masters)})";
						}
						return General.GetMasters(objRec.Masters);
					}
					return GetMasterUnderGroup(objRec.Code, arrParams[num2].ControlId, m_objDb, m_iCompId);
				}
			}
			num = FConvert.GetInputValue(objRec.Inputs, num);
			if (num >= 0)
			{
				return num.ToString();
			}
			return string.Empty;
		}
		return strFieldValue;
	}

	private string GetSpecificInputField(string strField, ref bool bNextRangeField)
	{
		if (strField.Contains("@ Start "))
		{
			strField = strField.Replace(" Start ", "");
		}
		else if (strField.Contains("@ End "))
		{
			strField = strField.Replace(" End ", "");
			bNextRangeField = true;
		}
		return strField;
	}

	private string GetMasterUnderGroup(int iMasterId, int iMasterTypeId, Database objDb, int iCompId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<string> list = null;
		string text = null;
		string text2 = null;
		list = new List<string>();
		try
		{
			switch (iMasterTypeId)
			{
			case 1:
				text2 = "vmCore_Account";
				break;
			case 2:
				text2 = "vmCore_Product";
				break;
			default:
				text = $"select 'vm'+sModule+'_'+sMasterName from cCore_MasterDef where iMasterTypeId = {iMasterTypeId}";
				dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				text2 = objDb.ExecuteScalar(dbCommand).ToString();
				break;
			}
			text = string.Format("WITH RecursionCTE (iMasterId, iParentId, bGroup)  AS  \r\n                            (   SELECT iMasterId, iParentId, bGroup\r\n                                FROM {1}\r\n                                WHERE iMasterId = {2} AND iTreeId = 0\r\n                                UNION ALL  \r\n                                SELECT R1.iMasterId, R1.iParentId, R1.bGroup\r\n                                FROM {1} AS R1\r\n                                JOIN RecursionCTE AS R2 ON R1.iParentId = R2.iMasterId\r\n                            )SELECT iMasterId  FROM RecursionCTE WHERE bGroup = 0", FConvert.GetSuffix(iCompId), text2, iMasterId);
			dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDb.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(Convert.ToString(dataReader[0]));
			}
			dataReader.Close();
		}
		catch
		{
		}
		if (list.Count <= 0)
		{
			return $"({iMasterId})";
		}
		return string.Format("({0})", string.Join(",", list.ToArray()));
	}
}
