using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public class General
{
	public const char COMMA = ',';

	public const char GROUP_SEPERATOR = '~';

	public const int GROUPLEVEL_STARTID = 255;

	public const int IMMEDIATEGROUP_NAME = 101;

	public const int IMMEDIATEGROUP_CODE = 102;

	public const string MASTERID = "@MasterId";

	public const string MASTERIDTABLE = "@MasterIdTable";

	public const string TRAN_DATA_COLUMNS = "@TRAN_DATA_COLUMNS";

	public const string TRAN_HEADERDATA_COLUMNS = "@TRAN_HEADERDATA_COLUMNS";

	public const string TRAN_HEADERDOCS_COLUMNS = "@TRAN_HEADERDOCS_COLUMNS";

	public const string TRAN_DATADOCS_COLUMNS = "@TRAN_DATADOCS_COLUMNS";

	public const string TRAN_ALLDATA_COLUMNS = "@TRAN_ALLDATA_COLUMNS";

	public const string SCREEN_DATA_COLUMNS = "@SCREEN_DATA_COLUMNS";

	public const string SCREENFIELD_INPUT = "_Input";

	public const string USER_RESTRICTION = "--ANDSEC";

	public const string EXTRA_COLUMNS = "--@EXTRA_COLUMNS";

	public const string EXTRA_TABLES = "--@EXTRA_TABLES";

	public const string EXTRA_FILTER = "--@EXTRA_FILTER";

	public const string ROLES_FILTER = "--@ROLES_FILTER";

	public const string EXTRA_GROUPING = "--@EXTRA_GROUPBY";

	public const string ORDERBY_START = "--@ORDERBY_START";

	public const string NO_PRIMARY_KEY = "-1[iHeaderId]";

	public const string DATEPART_YEAR = "@@Year";

	public const string DATEPART_MonthYearwise = "@@MonthYearwise";

	public const string DATEPART_Month = "@@Month";

	public const string DATEPART_Week = "@@Week";

	public const string DATEPART_WeekDay = "@@WeekDay";

	public const string DATEPART_Day = "@@Day";

	public const string DATEPART_DayOfYear = "@@DayOfYear";

	public const bool ISFORMULACHECK = true;

	public static FieldData[] GetAllColumns(IdNamePair[] arrDefault, FieldData[] arrExtra)
	{
		int num = 0;
		int num2 = 0;
		FieldData fieldData = null;
		List<FieldData> list = new List<FieldData>();
		num2 = ((arrExtra != null && arrExtra.Length != 0) ? arrExtra.Length : 0);
		for (num = 0; num < arrDefault.Length - num2; num++)
		{
			fieldData = new FieldData();
			fieldData.FieldId = arrDefault[num].ID;
			fieldData.SubParentId = -1;
			fieldData.FieldOrder = num;
			fieldData.FieldName = arrDefault[num].Name;
			if (arrDefault[num].Tag != null)
			{
				fieldData.DataType = (MasterDataType)arrDefault[num].Tag;
			}
			fieldData.ColumnWidth = ((fieldData.DataType == MasterDataType.Fraction) ? 60 : 80);
			list.Add(fieldData);
		}
		if (arrExtra != null)
		{
			for (num = 0; num < arrExtra.Length; num++)
			{
				arrExtra[num].FieldOrder = list.Count;
				list.Add(arrExtra[num]);
			}
		}
		return list.ToArray();
	}

	public static string GetServerFilePath(string strReportName, string strModule, bool bExcel, string sExt)
	{
		string text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) + "\\" + strModule + "_RepFolder\\";
		text = text.Substring(6, text.Length - 6);
		if (bExcel)
		{
			text += "EXCEL\\";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			if (!string.IsNullOrEmpty(sExt))
			{
				return text + strReportName + sExt;
			}
			return text + strReportName + ".xls";
		}
		text += "XML\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text + strReportName + ".xml";
	}

	public static string GetMasters(ComboData[] arrMasters)
	{
		if (arrMasters != null && arrMasters.Length != 0)
		{
			return string.Join(",", arrMasters.Select((ComboData p) => p.ID.ToString()).ToArray());
		}
		return "0";
	}

	public static int[] GetMasterIds(ComboData[] arrMasters)
	{
		List<int> list = new List<int>();
		if (arrMasters != null && arrMasters.Length != 0)
		{
			list.AddRange(arrMasters.Select((ComboData p) => p.ID).ToArray());
		}
		return list.ToArray();
	}

	public static string GetMasterIdsXML(ComboData[] arrMasters)
	{
		StringBuilder stringBuilder = new StringBuilder("");
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		if (arrMasters == null)
		{
			empty = " ID=\"0\"";
			empty2 = " P=\"0\"";
			empty3 = " iR=\"0\"";
			stringBuilder.Append("<R " + empty3 + empty + empty2 + "/>");
		}
		else
		{
			for (int i = 0; i < arrMasters.Length; i++)
			{
				empty = $" ID=\"{arrMasters[i].ID.ToString()}\"";
				empty2 = string.Format(" P=\"{0}\"", "0");
				empty3 = $" iR=\"{i.ToString()}\"";
				stringBuilder.Append("<R " + empty3 + empty + empty2 + "/>");
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetSpecialColumn(int iFieldId, int iSubFieldId, string strTable, MasterDataType oDataType, CalendarType oCalType, bool bColumnFilter = false)
	{
		if (oDataType == MasterDataType.Text)
		{
			string text = strTable;
			if (text.ToLower().Contains("time") && iFieldId >= 300000)
			{
				iFieldId = 80;
			}
			VTFIELDID vTFIELDID = (VTFIELDID)iFieldId;
			if (vTFIELDID <= VTFIELDID.FDF_MODIFIED_TIME)
			{
				switch (vTFIELDID)
				{
				case VTFIELDID.FDF_TIME:
				case VTFIELDID.FDF_MODIFIED_TIME:
					goto IL_0204;
				}
			}
			else if (vTFIELDID <= VTFIELDID.FDF_REFERENCE_DATE)
			{
				switch (vTFIELDID)
				{
				}
			}
			else if (vTFIELDID != VTFIELDID.FDF_REFERENCE_DUE_DATE)
			{
				_ = 144;
			}
			switch ((DatePart)iSubFieldId)
			{
			case DatePart.Year:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@Year", '~', strTable) : $"DATEPART(year, CONVERT(VARCHAR(30), dbo.IntToDate({strTable}),100))");
				break;
			case DatePart.MonthYearwise:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@MonthYearwise", '~', strTable) : string.Format("CAST(DATEPART(year, CONVERT(VARCHAR(30), dbo.IntToDate({0}),100)) AS VARCHAR)+' '+dbo.GetDateName('m',{0})", strTable));
				break;
			case DatePart.Month:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@Month", '~', strTable) : $"dbo.GetDateName('m',{strTable})");
				break;
			case DatePart.Week:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@Week", '~', strTable) : $"DATEPART(week, CONVERT(VARCHAR(30), dbo.IntToDate({strTable}),100))");
				break;
			case DatePart.WeekDay:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@WeekDay", '~', strTable) : $"DATENAME(weekday, CONVERT(VARCHAR(30), dbo.IntToDate({strTable}),100))");
				break;
			case DatePart.Day:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@Day", '~', strTable) : $"DATEPART(day, CONVERT(VARCHAR(30), dbo.IntToDate({strTable}),100))");
				break;
			case DatePart.DayOfYear:
				strTable = (bColumnFilter ? string.Format("{0}{1}{2}", "@@DayOfYear", '~', strTable) : $"DATEPART(dayofyear, CONVERT(VARCHAR(30), dbo.IntToDate({strTable}),100))");
				break;
			}
			if (oCalType == CalendarType.Nepali)
			{
				strTable = $"CASE WHEN {text} > 0 THEN {strTable} ELSE '' END";
			}
		}
		goto IL_0256;
		IL_0256:
		return strTable;
		IL_0204:
		strTable = (TimePart)iSubFieldId switch
		{
			TimePart.Hour => string.Format("CASE WHEN DATEPART(hour, CONVERT(VARCHAR(10), dbo.fCore_IntToTime({0}))) > 12 THEN\r\n                            RIGHT('00'+Convert(varchar(2), DATEPART(hour, CONVERT(VARCHAR(10), dbo.fCore_IntToTime({0}))) - 12) + ' PM', 5)\r\n                            ELSE RIGHT('00'+Convert(varchar(2), DATEPART(hour, CONVERT(VARCHAR(10), dbo.fCore_IntToTime({0})))) + ' AM', 5) END", strTable), 
			TimePart.Minute => $"DATEPART(minute, CONVERT(VARCHAR(10), dbo.fCore_IntToTime({strTable})))", 
			TimePart.Second => $"DATEPART(second, CONVERT(VARCHAR(10), dbo.fCore_IntToTime({strTable})))", 
			_ => $"CONVERT(VARCHAR(8), dbo.fCore_IntToTime({strTable}))", 
		};
		goto IL_0256;
	}

	public static string GetAccountFilter(uint iReportId)
	{
		string result = string.Empty;
		switch ((FocusReport)iReportId)
		{
		case FocusReport.ProfitAndLoss:
		case FocusReport.IncomeExpenseTrend:
			result = $"{(byte)3},{(byte)4},{(byte)10},{(byte)11},{(byte)13},{(byte)14},{(byte)15},{(byte)19},{(byte)20},{(byte)22},{(byte)27},{(byte)29},{(byte)30},{(byte)34},{(byte)38},{(byte)31},{(byte)25}";
			break;
		case FocusReport.BalanceSheet:
		case FocusReport.FundsFlow:
			result = $"{(byte)1},{(byte)2},{(byte)5},{(byte)6},{(byte)7},{(byte)8},{(byte)9},{(byte)12},{(byte)17},{(byte)21},{(byte)23},{(byte)24},{(byte)26},{(byte)28},{(byte)31},{(byte)32},{(byte)33},{(byte)35},{(byte)36},{(byte)37},{(byte)18},{(byte)16}";
			break;
		case FocusReport.TradingAndProfitAndLoss:
			result = $"{(byte)3},{(byte)4},{(byte)10},{(byte)11},{(byte)13},{(byte)14},{(byte)15},{(byte)19},{(byte)20},{(byte)22},{(byte)27},{(byte)29},{(byte)30},{(byte)34},{(byte)38},{(byte)25}";
			break;
		case FocusReport.TradingAccount:
			result = $"{(byte)3},{(byte)4},{(byte)34}";
			break;
		case FocusReport.CashFlow:
			result = $"{(byte)1},{(byte)11}";
			break;
		case FocusReport.CashFlowAnalysis:
			result = $"{(byte)1},{(byte)2}";
			break;
		case FocusReport.ReceivableAndPaybleBalance:
			result = $"{(byte)5},{(byte)6},{(byte)7}";
			break;
		}
		return result;
	}

	public static byte IsAccountingTransaction(_TransactionSet[] arrSets, Focus.Common.DataStructs.Module oModule, uint iFinancialVoucher)
	{
		if (arrSets != null && arrSets.Length != 0 && arrSets[0] != null)
		{
			if (arrSets[0].TransactionSetId == TransactionSetType.AccountingTransactionsOfanAccount || arrSets[0].TransactionSetId == TransactionSetType.AccountingTransactionsOfSelectedAccounts || arrSets[0].TransactionSetId == TransactionSetType.AllAccountingTransactions)
			{
				return 1;
			}
		}
		else if (oModule == Focus.Common.DataStructs.Module.None && iFinancialVoucher == 256)
		{
			return 2;
		}
		return 0;
	}

	public static string GetSortingOnItem(uint ReportId, IdValuePair[] arrInputs)
	{
		string result = string.Empty;
		switch ((FocusReport)ReportId)
		{
		case FocusReport.StockStatement:
		case FocusReport.StockMovement:
		case FocusReport.VirtualStockAnalysis:
		case FocusReport.ReorderReport:
		case FocusReport.PeakAndLowBalanceStock:
		case FocusReport.StockReservationReport:
			switch (FConvert.GetInputValue(arrInputs, 21))
			{
			case 1:
				result = "vrCore_Product.sName";
				break;
			case 2:
				result = "vrCore_Product.sCode";
				break;
			case 3:
				result = "ATree.iSeq";
				break;
			}
			break;
		}
		return result;
	}

	public static bool GroupingOptions(int iGroupingOptions, GroupingOption objGroupingOption)
	{
		return objGroupingOption switch
		{
			GroupingOption.DONT_START_NEXT_LEVEL_ON_NEW_LINE => (iGroupingOptions & (int)Math.Pow(2.0, 0.0)) == 1, 
			GroupingOption.DISPLAY_TOTAL_AT_END => (iGroupingOptions & (int)Math.Pow(2.0, 1.0)) >> 1 == 1, 
			GroupingOption.DISPLAY_VALUE_AT_START => (iGroupingOptions & (int)Math.Pow(2.0, 2.0)) >> 2 == 1, 
			GroupingOption.DISPLAY_VALUE_AT_END => (iGroupingOptions & (int)Math.Pow(2.0, 3.0)) >> 3 == 1, 
			GroupingOption.LEAVE_BLANK_LINE_AT_START => (iGroupingOptions & (int)Math.Pow(2.0, 4.0)) >> 4 == 1, 
			GroupingOption.DISPLAY_LINE_AT_END => (iGroupingOptions & (int)Math.Pow(2.0, 5.0)) >> 5 == 1, 
			GroupingOption.SKIP_PAGE_AT_END => (iGroupingOptions & (int)Math.Pow(2.0, 6.0)) >> 6 == 1, 
			GroupingOption.HIDE_GROUP_TOTAL => (iGroupingOptions & (int)Math.Pow(2.0, 7.0)) >> 7 == 1, 
			GroupingOption.HIDE_GROUP_ROW => (iGroupingOptions & (int)Math.Pow(2.0, 8.0)) >> 8 == 1, 
			GroupingOption.DISPLAY_BASED_ON_TREE_SEQUENCE => (iGroupingOptions & (int)Math.Pow(2.0, 13.0)) >> 13 == 1, 
			_ => false, 
		};
	}
}
