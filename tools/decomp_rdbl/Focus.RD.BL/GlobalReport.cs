using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Focus.Common.BL;
using Focus.Common.DataStructs;
using Focus.DatabaseFactory;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Focus.TranSettings.DataStructs;
using Focus.Transactions.BL;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Focus.RD.BL;

public class GlobalReport
{
	private string m_sSuffix;

	private int m_iCompanyId;

	private Database m_db;

	public StandardQuery vFirstLine(RepRecord oRec, bool bPreviousYear, ref List<string> arrDefaultTables, int iCompanyId)
	{
		m_sSuffix = FConvert.GetSuffix(iCompanyId);
		m_iCompanyId = iCompanyId;
		if (m_db == null)
		{
			m_db = DatabaseWrapper.GetDatabase2(iCompanyId);
		}
		return oRec.Module switch
		{
			Module.POS => callPOSQuery(oRec, bPreviousYear, ref arrDefaultTables), 
			Module.Production => callProductionQuery(oRec, bPreviousYear, ref arrDefaultTables, iCompanyId), 
			Module.CoreMasters => callMasterQuery(oRec, bPreviousYear, ref arrDefaultTables), 
			Module.WMS => callWMSQuery(oRec, bPreviousYear, ref arrDefaultTables), 
			_ => null, 
		};
	}

	public ReportMatrix vNextPage(ReportMatrix objData, RepRecord objRec, int iFieldCount, int iCompId)
	{
		switch ((FocusReport)objRec.ReportId)
		{
		case FocusReport.PaymentWiseSales:
			objData.ColumnData = ArrangePaymentTotal(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.AdvancedBudgetReport:
			if (FConvert.GetInputValue(objRec.Inputs, 5) == 0)
			{
				objData.ColumnData = CalculateBudgetBalance(objData.ColumnData, objRec, iCompId);
			}
			break;
		case FocusReport.WMSBillingReport:
			objData.ColumnData = CalculateWMSBilling(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSBillingSKUWiseReport:
			objData.ColumnData = CalculateWMSBillingSKUWise(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSBillingSKUBatchWise:
			objData.ColumnData = CalculateWMSBillingSKUBatchWise(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSCurrentStockReport:
			objData.ColumnData = CalculateWMSCurrentStock(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSSKUwiseInvTransReport:
			objData.ColumnData = CalculateWMSSKUwiseInvTrans(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSExpiryDateReport:
			objData.ColumnData = CalculateExpiryDate(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSBinOccupancyReport:
			objData.ColumnData = CalculateBinOccupancy(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSPendingAllocationsReport:
			objData.ColumnData = CalculatePendingAll(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSInventoryBalReport:
			objData.ColumnData = CalculateInvBal(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSEmptyLocReport:
			objData.ColumnData = CalculateEmptyLoc(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.BillingRepSchedule:
			objData.ColumnData = CalculateSchedule(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSBillingForecast:
			objData.ColumnData = CalculateBillForecast(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSStoringServiceCharge:
			objData.ColumnData = CalculateBillForecast(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSShortDateReport:
			objData.ColumnData = CalculateShortDate(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSPalletInPalletOutRpt:
			objData.ColumnData = CalculatePalletInPalletOut(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSBillingSummaryWeekWise:
			objData.ColumnData = CalculateBillWeekWise(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSEstimatedBillingReport:
			objData.ColumnData = CalculatePalletInPalletOut(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.StockOnHandByCartonWise:
			objData.ColumnData = CalculateAvailableQuantity(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSAdvanceServiceCharges:
			objData.ColumnData = CalculateWMSAdvServiceCharge(objData.ColumnData, objRec, iCompId);
			break;
		case FocusReport.WMSAdvanceBillingReport:
			objData.ColumnData = CalculateWMSAdvBilling(objData.ColumnData, objRec, iCompId);
			break;
		}
		return objData;
	}

	private StandardQuery callMasterQuery(RepRecord oRec, bool bPreviousYear, ref List<string> arrDefaultTables)
	{
		StandardQuery standardQuery = null;
		bool flag = Convert.ToBoolean(_focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 10));
		bool flag2 = Convert.ToBoolean(_focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 12));
		string empty = string.Empty;
		empty = ((!flag2) ? ("tCore_Header" + m_sSuffix + ".iDate") : ("tCore_Data" + m_sSuffix + ".iDueDate"));
		string empty2 = string.Empty;
		switch ((FocusReport)oRec.ReportId)
		{
		case FocusReport.AdvancedBudgetReport:
		{
			standardQuery = new StandardQuery();
			arrDefaultTables.AddRange("vrCore_Account,vrCore_Product".Split(','));
			if (oRec.StartingDate == 0)
			{
				oRec.StartingDate = _focus.company(m_iCompanyId).accountingDate;
			}
			string empty4 = string.Empty;
			string text6 = string.Empty;
			string empty5 = string.Empty;
			string text8 = string.Empty;
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				empty4 = ((FConvert.GetInputValue(oRec.Inputs, 5) != 0) ? (empty4 + $" and iTag1 = {FConvert.GetInputValue(oRec.Inputs, 1)}") : (empty4 + $" and mCore_ConfirmedBudget_Details.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 1)}"));
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				empty4 = ((FConvert.GetInputValue(oRec.Inputs, 5) != 0) ? (empty4 + $" and iTag2 = {FConvert.GetInputValue(oRec.Inputs, 2)}") : (empty4 + $" and mCore_ConfirmedBudget_Details.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 2)}"));
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				empty4 = ((FConvert.GetInputValue(oRec.Inputs, 5) != 0) ? (empty4 + $" and iTag3 = {FConvert.GetInputValue(oRec.Inputs, 3)}") : (empty4 + $" and mCore_ConfirmedBudget_Details.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 3)}"));
			}
			if (FConvert.GetInputValue(oRec.Inputs, 6) > 0)
			{
				empty5 += $" and mCore_Budget.iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				string text9 = $"select iValidFrom, iValidTo from mCore_Budget where iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				IDataReader dataReader2 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text9) : text9);
				while (dataReader2.Read())
				{
					oRec.StartingDate = Convert.ToInt32(dataReader2["iValidFrom"]);
					oRec.EndingDate = Convert.ToInt32(dataReader2["iValidTo"]);
				}
				dataReader2.Close();
			}
			else
			{
				empty5 += $" and  (({oRec.StartingDate} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo) or  ({oRec.EndingDate} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo))";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) == 1)
			{
				text8 = $" and vrCore_Account.iAccountTypeId In (3,4,10,11,13,14,16,19,20,22,25,27,29,34,38) ";
			}
			else if (FConvert.GetInputValue(oRec.Inputs, 4) == 2)
			{
				text8 = $" and  vrCore_Account.iAccountTypeId In (1,2,5,6,7,8,9,12,17,18,21,23,24,26,28,30,31,33,32,35,36,37) ";
			}
			string text10 = " >= 0";
			if (FConvert.GetInputValue(oRec.Inputs, 7) > 0)
			{
				text10 = " = " + FConvert.GetInputValue(oRec.Inputs, 7);
			}
			int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 4);
			int preferenceValue2 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 5);
			int preferenceValue3 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			string text5 = string.Format("");
			string text = string.Format("");
			if (preferenceValue > 0)
			{
				text5 = string.Format(" inner join {0} on Budget.iTag1 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue).viewName);
			}
			if (preferenceValue2 > 0)
			{
				text5 += string.Format("inner join {0} on Budget.iTag2 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue2).viewName);
			}
			if (preferenceValue3 > 0)
			{
				text5 += string.Format(" inner join {0} on Budget.iTag3 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue3).viewName);
			}
			string text11 = "";
			string text12 = "";
			string text13 = "";
			string text14 = "";
			string text15 = string.Empty;
			string text16 = string.Empty;
			string text17 = string.Empty;
			string text18 = string.Empty;
			empty2 = $"and tCore_Data{m_sSuffix}.iAuthStatus = 1";
			if (flag)
			{
				empty2 = $"and tCore_Data{m_sSuffix}.iAuthStatus < 2";
			}
			decimal num = 0m;
			decimal num2 = 0m;
			string text19 = string.Format("\r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount1 > 0 then mAmount1 else 0 end,0)) Value \r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 \r\n\t\t                    Join mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) \r\n\t\t                    left Join tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iCode\r\n                    Where   c.iDate between  {1} and {2}  and a.bUpdateFA = 1\r\n                    Union All \r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount2 > 0 then mAmount2 else 0 end,0)) Value\r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                            mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                            tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iBookNo\r\n                    Where   c.iDate between  {1} and {2}  and a.bUpdateFA = 1\r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters));
			IDataReader dataReader3 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text19) : text19);
			while (dataReader3.Read())
			{
				num = Convert.ToDecimal(dataReader3["Value"]);
				num2 = Convert.ToDecimal(dataReader3["Qty"]);
			}
			dataReader3.Close();
			decimal num3 = 0m;
			decimal num4 = 0m;
			string text20 = string.Format("\r\n                    Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                            abs(Case when mAmount1 < 0 then mAmount1 else 0 end) Value \r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                            mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) left Join\r\n                            tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                            inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iCode\r\n                    Where  c.iDate between {1} and {2} and a.bUpdateFA = 1\r\n                    Union All \r\n                    Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                            abs(Case when mAmount2 < 0 then mAmount2 else 0 end) Value\r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                            mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                            tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                            inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iBookNo\r\n                    Where   c.iDate between {1} and {2} and a.bUpdateFA = 1\r\n                    ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters));
			IDataReader dataReader4 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text20) : text20);
			while (dataReader4.Read())
			{
				num3 = Convert.ToDecimal(dataReader4["Value"]);
				num4 = Convert.ToDecimal(dataReader4["Qty"]);
			}
			dataReader4.Close();
			if (FConvert.GetInputValue(oRec.Inputs, 8) == 1)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 5) == 0)
				{
					text17 = string.Format("\r\n                            --Account Join with iCode\r\n                            Union All\r\n                            select mCore_Budget.sPlanName ,mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 BudgetAmt,0 PreCommittedAmt ,0 CommittedAmt, Sum(Case when mAmount1 > 0 then mAmount1 else 0 end) SpentAmt,  \r\n                            0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty   from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1 and tCore_Header{0}.iDate between  {1} and {2} {3} {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,mCore_Budget.sPlanName\r\n                            --Account Join With iBookNo\r\n                            Union All\r\n                            select mCore_Budget.sPlanName ,mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 BudgetAmt,0 PreCommittedAmt ,0 CommittedAmt, Sum(Case when mAmount2 > 0 then mAmount2 else 0 end) SpentAmt,  \r\n                            0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty   from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1 and tCore_Header{0}.iDate between  {1} and {2} {3} {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,mCore_Budget.sPlanName", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
					text18 = string.Format(" \r\n                             --Account Join with iCode\r\n                            Union All\r\n                            select mCore_Budget.sPlanName ,mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 BudgetAmt,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, 0 SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, \r\n                            abs(Sum(Case when mAmount1 < 0 then mAmount1 else 0 end)) ReturnAmt, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) ReturnQty   from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1 and tCore_Header{0}.iDate between  {1} and {2} {3} {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,mCore_Budget.sPlanName\r\n                            --Account Join With iBookNo\r\n                            Union All\r\n                            select mCore_Budget.sPlanName ,mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 BudgetAmt,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, 0 SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, \r\n                            abs(Sum(Case when mAmount2 < 0 then mAmount2 else 0 end)) ReturnAmt, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) ReturnQty   from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1 and tCore_Header{0}.iDate between  {1} and {2} {3} {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,mCore_Budget.sPlanName", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 5) == 1)
				{
					text17 = string.Format(" \r\n                            -- Account Join with iCode\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, Sum(Case when mAmount1 > 0 then mAmount1 else 0 end) Amount,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where  tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName \r\n                            -- Account Join with iBookNo\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, Sum(Case when mAmount2 > 0 then mAmount2 else 0 end) Amount,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where  tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
					text18 = string.Format(" \r\n                            -- Account Join with iCode\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 Amount,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, 0 BudgetQty, abs(Sum(Case when mAmount1 < 0 then mAmount1 else 0 end)) dAmt, \r\n                            Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) dQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where  tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName \r\n                            -- Account Join with iBookNo\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, 0 Amount,0 PreCommittedAmt ,0 CommittedAmt, 0 SpentAmt,  \r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, 0 BudgetQty, abs(Sum(Case when mAmount2 < 0 then mAmount2 else 0 end)) dAmt, \r\n                            Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) dQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where  tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 5) == 2)
				{
					text17 = string.Format(" \r\n                            -- Account Join with iCode\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3,  Sum(Case when mAmount1 > 0 then mAmount1 else 0 end) Amount,\r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName \r\n                            -- Account Join with iBookNo\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, Sum(Case when mAmount2 > 0 then mAmount2 else 0 end) Amount,\r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
					text18 = string.Format(" \r\n                            -- Account Join with iCode\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3,  abs(Sum(Case when mAmount1 < 0 then mAmount1 else 0 end)) Amount,\r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName \r\n                            -- Account Join with iBookNo\r\n                            Union All\r\n                            select mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId, 0 iTag1, 0 iTag2, 0 iTag3, abs(Sum(Case when mAmount2 < 0 then mAmount2 else 0 end)) Amount,\r\n                            tCore_Header{0}.iDate FromDate, mCore_Budget.sPlanName, Sum(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end ) BudgetQty from tCore_Data{0} \r\n\t                        inner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                            inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo \r\n\t                        inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                        inner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n                            inner join mCore_Account On mCore_Account.iMasterId = tCore_Data{0}.iCode and mCore_Account.iAccountType in (3,10)\r\n                            left Join tCore_Indta{0} On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                            where tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n                                AND tCore_Data{0}.iAuthStatus = 1  {9} {15}\r\n                            group by mCore_ConfirmedBudget_Details.iAccountId,mCore_ConfirmedBudget_Details.iProductId,tCore_Header{0}.iDate,mCore_Budget.sPlanName ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text10, oRec.LanguageId, empty2);
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			switch (FConvert.GetInputValue(oRec.Inputs, 5))
			{
			case 0:
				if (oRec.Masters.Count() > 0)
				{
					text11 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = iAccountId";
					text13 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = tCore_PreCommittedBudget{m_sSuffix}.iAccountId";
					text14 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = tCore_CommittedBudget{m_sSuffix}.iAccountId";
					text15 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = tCore_SpentBudget{m_sSuffix}.iAccountId";
					text16 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = tCore_ReturnBudget{m_sSuffix}.iAccountId";
				}
				standardQuery.Query = string.Format("\r\nSelect iAccountId [iHeaderId], sPlanName BudgetPlan, vrCore_Account.sCode [Account Code], vrCore_Account.sName [Account Name], AccGrp.sName [Account Group], \r\nsum(BudgetAmt) [Budgeted Value], case when sum(PreCommittedAmt) > 0 then 0 else abs(sum(PreCommittedAmt)) end [Pre Committed Value],case when sum(CommittedAmt) > 0 then 0 else abs(sum(CommittedAmt)) end [Committed Value],sum(abs(SpentAmt) + abs({22}))  [Consumed Value], sum(ReturnAmt) + ({24}) ReturnAmt, 0 [Available Balance(variances)], 0 [Utilization %],\r\nsum(BudgetQty) [Budgeted Quantity], sum(abs(PreCommittedQty)) [Pre Committed Quantity],sum(abs(CommittedQty)) [Committed Quantity],sum(abs(SpentQty) + ({23}))  [Consumed Quantity], sum(abs(ReturnQty)  + ({25})) ReturnQty, 0 [Available Quantity(variances)], 0 [Quantity Utilization %],\r\nsum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity,\r\n(case when sum(PreCommittedAmt) > 0 then 0 else abs(sum(PreCommittedAmt)) end) PreCommittedBalanceValue, sum(abs(PreCommittedQty)) PreCommittedBalanceQty, \r\n(case when sum(CommittedAmt) > 0 then 0 else abs(sum(CommittedAmt)) end) CommittedBalanceValue, (sum(abs(CommittedQty))) CommittedBalanceQty\r\n{4} \r\nfrom \r\n(\r\n    Select mCore_Budget.sPlanName, iAccountId, iProductId iProduct,iTag1, iTag2, iTag3, fValue BudgetAmt, 0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt,\r\n    fQuantity BudgetQty, 0 PreCommittedQty,0 CommittedQty,0 SpentQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n    from mCore_ConfirmedBudget_Details \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t\r\n    {8}\r\n    where bActive=1 and iProductId {14} {9} {3} \r\n\r\n    union all\r\n    --PreCommitted budget values\r\n    select mCore_Budget.sPlanName ,tCore_PreCommittedBudget{0}.iAccountId,tCore_PreCommittedBudget{0}.iProduct,tCore_PreCommittedBudget{0}.iTag1, \r\n    tCore_PreCommittedBudget{0}.iTag2, tCore_PreCommittedBudget{0}.iTag3, 0 BudgetAmt, sum(dAmt-dAmtUsed) PreCommittedAmt,0 CommittedAmt,0 SpentAmt,\r\n    0 BudgetQty, sum(dQty - dQtyUsed) PreCommittedQty,0 CommittedQty,0 SpentQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty  \r\n    from tCore_PreCommittedBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0}  on tCore_Data{0}.iTransactionId = tCore_PreCommittedBudget{0}.iTransId \r\n\tinner join tCore_Header{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_PreCommittedBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_PreCommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_PreCommittedBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_PreCommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_PreCommittedBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {21} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    {11}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND {21} between  {1} and {2} and tCore_PreCommittedBudget{0}.iProduct {14} {3} {9} {20} and IsNull(bRejected,0) = 0 --and IsNull(tCore_PreCommittedBudget{0}.bCancelled,0) = 0\r\n    group by tCore_PreCommittedBudget{0}.iAccountId,tCore_PreCommittedBudget{0}.iProduct,tCore_PreCommittedBudget{0}.iTag1,tCore_PreCommittedBudget{0}.iTag2,tCore_PreCommittedBudget{0}.iTag3, mCore_Budget.sPlanName\r\n\r\n    Union All\r\n    --Committed budget values\r\n    select mCore_Budget.sPlanName ,tCore_CommittedBudget{0}.iAccountId,tCore_CommittedBudget{0}.iProduct,tCore_CommittedBudget{0}.iTag1, \r\n    tCore_CommittedBudget{0}.iTag2, tCore_CommittedBudget{0}.iTag3, 0 BudgetAmt, 0 PreCommittedAmt,sum(dAmt-dAmtUsed) CommittedAmt,0 SpentAmt,\r\n    0 BudgetQty, 0 PreCommittedQty,sum(dQty - dQtyUsed) CommittedQty, 0 SpentQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty   \r\n    from tCore_CommittedBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0}  on tCore_Data{0}.iTransactionId = tCore_CommittedBudget{0}.iTransId \r\n\tinner join tCore_Header{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_CommittedBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_CommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_CommittedBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_CommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_CommittedBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    {12}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND tCore_Header{0}.iDate between  {1} and {2} and tCore_CommittedBudget{0}.iProduct {14} {3} {9} {20} and IsNull(bRejected,0) = 0\r\n    group by tCore_CommittedBudget{0}.iAccountId,tCore_CommittedBudget{0}.iProduct,tCore_CommittedBudget{0}.iTag1,tCore_CommittedBudget{0}.iTag2,tCore_CommittedBudget{0}.iTag3, mCore_Budget.sPlanName\r\n    \r\n    Union All\r\n    --Spent budget values\r\n    select mCore_Budget.sPlanName ,tCore_SpentBudget{0}.iAccountId,tCore_SpentBudget{0}.iProduct, tCore_SpentBudget{0}.iTag1, tCore_SpentBudget{0}.iTag2, \r\n    tCore_SpentBudget{0}.iTag3, 0 BudgetAmt,0 PreCommittedAmt ,0 CommittedAmt, sum(dAmt) SpentAmt ,  \r\n    0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, sum(dQty) SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty  \r\n    from tCore_SpentBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0} on tCore_Data{0}.iTransactionId = tCore_SpentBudget{0}.iTransId \r\n\tinner join tCore_Header{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = tCore_SpentBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_SpentBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_SpentBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_SpentBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_SpentBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    inner join mCore_Account On mCore_Account.iMasterId = tCore_SpentBudget{0}.iAccountId and mCore_Account.iAccountType not in (3,10)\r\n    {13}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND tCore_Header{0}.iDate between  {1} and {2} and tCore_SpentBudget{0}.iProduct {14} {3} {9} {20} and IsNull(bRejected,0) = 0\r\n    group by tCore_SpentBudget{0}.iAccountId,tCore_SpentBudget{0}.iProduct,tCore_SpentBudget{0}.iTag1,tCore_SpentBudget{0}.iTag2,tCore_SpentBudget{0}.iTag3,mCore_Budget.sPlanName \r\n    {16}\r\n\r\n    union all\r\n    --PreCommitedCancelled budget values\r\n    select mCore_Budget.sPlanName ,tCore_PreCommittedBudget{0}.iAccountId,tCore_PreCommittedBudget{0}.iProduct,tCore_PreCommittedBudget{0}.iTag1, \r\n    tCore_PreCommittedBudget{0}.iTag2, tCore_PreCommittedBudget{0}.iTag3, 0 BudgetAmt, 0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt,\r\n    0 BudgetQty, 0 PreCommittedQty,0 CommittedQty,0 SpentQty, sum(dAmt) PrecommittedCancelledAmount, sum(dQty) PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty \r\n    from tCore_PreCommittedBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0}  on tCore_Data{0}.iTransactionId = tCore_PreCommittedBudget{0}.iTransId \r\n\tinner join tCore_Header{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_PreCommittedBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_PreCommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_PreCommittedBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_PreCommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_PreCommittedBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {21} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    {11}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND {21} between  {1} and {2} and tCore_PreCommittedBudget{0}.iProduct {14} {3} {9} {20} and IsNull(bRejected,0) = 0 and IsNull(tCore_PreCommittedBudget{0}.bCancelled,0) = 1\r\n    group by tCore_PreCommittedBudget{0}.iAccountId,tCore_PreCommittedBudget{0}.iProduct,tCore_PreCommittedBudget{0}.iTag1,tCore_PreCommittedBudget{0}.iTag2,tCore_PreCommittedBudget{0}.iTag3, mCore_Budget.sPlanName\r\n\r\n    Union All\r\n    --CommitedCancelled budget values\r\n    select mCore_Budget.sPlanName ,tCore_CommittedBudget{0}.iAccountId,tCore_CommittedBudget{0}.iProduct,tCore_CommittedBudget{0}.iTag1, \r\n    tCore_CommittedBudget{0}.iTag2, tCore_CommittedBudget{0}.iTag3, 0 BudgetAmt, 0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt,\r\n    0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, 0 SpentQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, sum(dAmt) CommittedCancelledAmount, sum(dQty) CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty \r\n    from tCore_CommittedBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0}  on tCore_Data{0}.iTransactionId = tCore_CommittedBudget{0}.iTransId \r\n\tinner join tCore_Header{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_CommittedBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_CommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_CommittedBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_CommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_CommittedBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    {12}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND tCore_Header{0}.iDate between  {1} and {2} and tCore_CommittedBudget{0}.iProduct {14} {3} {9} {20} and IsNull(bRejected,0) = 0 and IsNull(tCore_CommittedBudget{0}.bCancelled,0) = 1\r\n    group by tCore_CommittedBudget{0}.iAccountId,tCore_CommittedBudget{0}.iProduct,tCore_CommittedBudget{0}.iTag1,tCore_CommittedBudget{0}.iTag2,tCore_CommittedBudget{0}.iTag3, mCore_Budget.sPlanName\r\n   \r\n    Union All\r\n    --Return budget values\r\n    select mCore_Budget.sPlanName ,tCore_ReturnBudget{0}.iAccountId,tCore_ReturnBudget{0}.iProduct,tCore_ReturnBudget{0}.iTag1, \r\n    tCore_ReturnBudget{0}.iTag2, tCore_ReturnBudget{0}.iTag3, 0 BudgetAmt, 0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt,\r\n    0 BudgetQty, 0 PreCommittedQty,0 CommittedQty, 0 SpentQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity,sum(dAmt) ReturnAmt,sum(dQty) ReturnQty    \r\n    from tCore_ReturnBudget{0} with (readuncommitted)\r\n    inner join tCore_Data{0}  on tCore_Data{0}.iTransactionId = tCore_ReturnBudget{0}.iTransId \r\n\tinner join tCore_Header{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n    inner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_ReturnBudget{0}.iAccountId and\r\n\tmCore_ConfirmedBudget_Details.iProductId = tCore_ReturnBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_ReturnBudget{0}.iTag1\r\n\tand mCore_ConfirmedBudget_Details.iTag2 = tCore_ReturnBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_ReturnBudget{0}.iTag3\r\n\tinner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\tinner join mCore_Budget with (readuncommitted)  On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo\r\n    inner join mCore_Account On mCore_Account.iMasterId = tCore_ReturnBudget{0}.iAccountId and mCore_Account.iAccountType not in (3,10)\r\n    {18}\r\n    where bSuspended=0 and tCore_Header{0}.bCancelled=0 AND tCore_Header{0}.iDate between  {1} and {2} and tCore_ReturnBudget{0}.iProduct  {14} {3} {9} {20}  and IsNull(bRejected,0) = 0\r\n    group by tCore_ReturnBudget{0}.iAccountId,tCore_ReturnBudget{0}.iProduct,tCore_ReturnBudget{0}.iTag1,tCore_ReturnBudget{0}.iTag2,tCore_ReturnBudget{0}.iTag3, mCore_Budget.sPlanName\r\n    {19}\r\n   \r\n) Budget\r\ninner join vrCore_Account on Budget.iAccountId= vrCore_Account.iMasterId and vrCore_Account.iTreeId =0\r\nleft join vrCore_Account AccGrp on AccGrp.iMasterId = vrCore_Account.iParentId and AccGrp.iTreeId = 0\r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {15}\r\njoin vrCore_Product on Budget.iProduct= vrCore_Product.iMasterId and vrCore_Product.iTreeId =0\r\n{7} \r\n{5}      \r\nwhere 1=1 {10}\r\ngroup by iAccountId,vrCore_Account.sCode, vrCore_Account.sName,AccGrp.sName,sPlanName  {17}\r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_FILTER", text5, text11, empty5, text8, text13, text14, text15, text10, oRec.LanguageId, text17, "--@EXTRA_GROUPBY", text16, text18, empty2, empty, num, num2, num3, num4);
				standardQuery.PrimaryColumn = "iHeaderId";
				break;
			case 1:
			{
				if (oRec.Masters.Count() > 0)
				{
					text11 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = iAccountId";
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				Date date = new Date(_focus.company(m_iCompanyId).accountingDate, CurrentData.get().CompanyCalendar);
				date = new Date(date.Year, date.Month, 1, CurrentData.get().CompanyCalendar);
				StringBuilder stringBuilder3 = new StringBuilder();
				empty2 = string.Format("and a.iAuthStatus = 1", m_sSuffix);
				if (flag)
				{
					empty2 = string.Format("and a.iAuthStatus < 2", m_sSuffix);
				}
				empty = ((!flag2) ? "c.iDate" : "a.iDueDate");
				int num10 = _focus.company(m_iCompanyId).accountingDate;
				Date date2 = date;
				decimal num5 = 0m;
				decimal num6 = 0m;
				decimal num7 = 0m;
				decimal num8 = 0m;
				for (int j = 1; j <= 4; j++)
				{
					num5 = 0m;
					num6 = 0m;
					num7 = 0m;
					num8 = 0m;
					text19 = string.Format("\r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount1 > 0 then mAmount1 else 0 end,0)) Value \r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 \r\n\t\t                    Join mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) \r\n\t\t                    left Join tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iCode\r\n                    Where   dbo.GetDatePart('q', c.iDate) = dbo.GetDatePart('q',{4})  and a.bUpdateFA = 1\r\n                    Union All \r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount2 > 0 then mAmount2 else 0 end,0)) Value\r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                            mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                            tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iBookNo\r\n                    Where   dbo.GetDatePart('q', c.iDate) = dbo.GetDatePart('q',{4})  and a.bUpdateFA = 1\r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters), num10);
					dataReader3 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text19) : text19);
					while (dataReader3.Read())
					{
						num5 = Convert.ToDecimal(dataReader3["Value"]);
						num6 = Convert.ToDecimal(dataReader3["Qty"]);
					}
					dataReader3.Close();
					text20 = string.Format("\r\n                                Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                                        abs(Case when mAmount1 < 0 then mAmount1 else 0 end) Value \r\n                                from    tCore_Data{0} a  Join \r\n                                        tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                                        mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) left Join\r\n                                        tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                                Where  dbo.GetDatePart('q', c.iDate) = dbo.GetDatePart('q',{4}) and a.iCode = {3} and a.bUpdateFA = 1\r\n                                Union All \r\n                                Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                                        abs(Case when mAmount2 < 0 then mAmount2 else 0 end) Value\r\n                                from    tCore_Data{0} a  Join \r\n                                        tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                                        mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                                        tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                                Where   dbo.GetDatePart('q', c.iDate) = dbo.GetDatePart('q',{4}) and a.iBookNo = {3} and a.bUpdateFA = 1\r\n                                ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters), num10);
					dataReader4 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text20) : text20);
					while (dataReader4.Read())
					{
						num7 = Convert.ToDecimal(dataReader4["Value"]);
						num8 = Convert.ToDecimal(dataReader4["Qty"]);
					}
					dataReader4.Close();
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(QBudgetAmt) else 0 end [M{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then (case when sum(PreCommittedAmt) >0 then 0 else abs(sum(PreCommittedAmt)) end) else 0 end [PreCommited{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then (case when SUM(CommittedAmt) > 0 then 0  else abs(SUM(CommittedAmt)) end) else 0 end  [Commited{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(SpentAmt) else 0 end [Spent{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(ReturnAmt) else 0 end [Return{j}]");
					stringBuilder2.Append($",0 [Balance{j}] ");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(QBudgetQty) else 0 end [Q{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(PreCommittedQty) else 0 end [PreCommitedQty{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(CommittedQty) else 0 end  [CommitedQty{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(SpentQty) else 0 end [SpentQty{j}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('q',BudgetInner.StartDate) = dbo.GetDatePart('q',{num10}) Then SUM(ReturnQty) else 0 end [ReturnQty{j}]");
					stringBuilder2.Append($",0 [BalanceQty{j}] ");
					stringBuilder.Append(string.Format(",sum(M{0}) [M{0}], Sum(PreCommited{0}) [PreCommited{0}],sum([Commited{0}]) [Commited{0}], sum(Spent{0}) + ({1}) [Spent{0}], sum(Return{0})+ ({3}) [Return{0}], \r\n                                Case When sum(M{0}) = 0 then 0 Else sum(M{0} - (PreCommited{0}+ Commited{0}+Spent{0}) + Return{0}) End [Balance{0}]", j, num5, num6, num7, num8));
					stringBuilder.Append(string.Format(",sum(Q{0}) [Q{0}], Sum(PreCommitedQty{0}) [PreCommitedQty{0}],sum([CommitedQty{0}]) [CommitedQty{0}], sum(SpentQty{0}) + ({2}) [SpentQty{0}], sum(ReturnQty{0})+ ({4}) [ReturnQty{0}],\r\n                                Case When  sum(Q{0}) = 0 then 0 Else sum(Q{0} - (PreCommitedQty{0}+ CommitedQty{0}+SpentQty{0}) + ReturnQty{0}) End [BalanceQty{0}]", j, num5, num6, num7, num8));
					stringBuilder3.Append(string.Format(",sum(M{0}) [M{0}], Sum(PreCommited{0}) [PreCommited{0}],sum([Commited{0}]) [Commited{0}], sum(Spent{0}) [Spent{0}], sum(Return{0}) [Return{0}], \r\n                                Case When sum(M{0}) = 0 then 0 Else sum(M{0} - (PreCommited{0}+ Commited{0}+Spent{0}) + Return{0}) End [Balance{0}]", j));
					stringBuilder3.Append(string.Format(",sum(Q{0}) [Q{0}], Sum(PreCommitedQty{0}) [PreCommitedQty{0}],sum([CommitedQty{0}]) [CommitedQty{0}], sum(SpentQty{0}) [SpentQty{0}], sum(ReturnQty{0}) [ReturnQty{0}],\r\n                                Case When  sum(Q{0}) = 0 then 0 Else sum(Q{0} - (PreCommitedQty{0}+ CommitedQty{0}+SpentQty{0}) + ReturnQty{0}) End [BalanceQty{0}]", j));
					date2 = date2.Add(3, DateAdd.Months);
					num10 = date2.Value;
				}
				standardQuery.Query = string.Format("\r\nselect iAccountId [iHeaderId], sPlanName BudgetPlan, vrCore_Account.sCode  [Account Code], vrCore_Account.sName [Account Name],  AccGrp.sName [Account Group], \r\nsum(BudgetAmt) [Budgeted Value], sum(PreCommittedAmt) [Pre Committed Value], sum(CommittedAmt) [Committed Value], sum(SpentAmt) + abs({23}) [Consumed Value], sum(ReturnAmt) + ({25}) [Return Value], sum(BudgetAmt -(PreCommittedAmt + CommittedAmt + SpentAmt ) + ReturnAmt) [Available Balance] ,\r\nsum(BudgetQty) [Budgeted Quantity], sum(PreCommittedQty) [Pre Committed Quantity], sum(CommittedQty) [Committed Quantity], sum(SpentQty) + ({24}) [Consumed Quantity] , sum(ReturnQty) + ({26}) [Return Quantity], sum(BudgetQty -(PreCommittedQty + CommittedQty + SpentQty ) + ReturnQty) [Available Quantity] ,\r\nsum(PrecommittedCancelledAmount) [Precommitted CancelledAmount], sum(PrecommittedCancelledQuantity) [Precommitted CancelledQuantity], sum(CommittedCancelledAmount) [Committed CancelledAmount], sum(CommittedCancelledQuantity) [Committed CancelledQuantity], \r\nsum(PreCommittedAmt) PreCommittedBalanceValue, sum(PreCommittedQty) PreCommittedBalanceQty,  sum(CommittedAmt) CommittedBalanceValue, sum(CommittedQty) CommittedBalanceQty {27} {8} from\r\n(\r\n        select iAccountId , iProduct, sum(BudgetAmt) BudgetAmt, sum(PreCommittedAmt) PreCommittedAmt, sum(CommittedAmt) CommittedAmt , sum(SpentAmt) SpentAmt , sum(ReturnAmt) ReturnAmt ,\r\n        0 [Available Balance] ,iTag1,iTag2,iTag3,sPlanName, sum(BudgetQty) BudgetQty, sum(PreCommittedQty) PreCommittedQty,sum(CommittedQty) CommittedQty , sum(SpentQty) SpentQty  , sum(ReturnQty) ReturnQty,\r\n        sum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity \r\n        --,(PreCommittedAmt - sum(PrecommittedCancelledAmount)) PreCommittedBalanceValue, (sum(PreCommittedQty) - sum(PrecommittedCancelledQuantity)) PreCommittedBalanceQty, \r\n        --(CommittedAmt - sum(CommittedCancelledAmount)) CommittedBalanceValue, (CommittedQty - sum(CommittedCancelledQuantity)) CommittedBalanceQty \r\n        {6}\r\n        from \r\n        (        \r\n                Select iAccountId,iProduct , sum(BudgetAmt) BudgetAmt, (case when sum(PreCommittedAmt) > 0 then 0 else abs(sum( PreCommittedAmt)) end) PreCommittedAmt, (case when sum(CommittedAmt) > 0 then 0 else abs(sum(CommittedAmt)) end) CommittedAmt,sum(abs( SpentAmt))  SpentAmt, sum(abs(ReturnAmt)) ReturnAmt,iTag1,iTag2,iTag3,sPlanName, \r\n                sum(BudgetQty) BudgetQty, abs(sum(PreCommittedQty)) PreCommittedQty, sum( abs(CommittedQty)) CommittedQty,sum(abs(SpentQty))  SpentQty,sum(abs(ReturnQty)) ReturnQty {5}\r\n                , sum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity\r\n                from\r\n                (\r\n                    ---retrieving Over all budget values\r\n                    Select iAccountId,iProductId iProduct,iTag1, iTag2,iTag3, mCore_ConfirmedBudget_Details.fValue BudgetAmt,\r\n\t                0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,0 StartDate,sPlanName,\r\n                    mCore_ConfirmedBudget_Details.fQuantity BudgetQty, 0 PreCommittedQty,0 CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty,\r\n                    0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty\r\n                    from   mCore_ConfirmedBudget_Details\t                \r\n                    inner join mCore_Budget_Revisions on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\r\n                    inner join mCore_Budget on  mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\r\n                    {3}\r\n                    --left join mCore_Budget_Quarterly on mCore_ConfirmedBudget_Details.iBudgetConfirmId=mCore_Budget_Quarterly.iBudgetConfirmId\r\n\t                where bActive=1 and iProductId {13} {4} {11}\r\n            \r\n                    union all \r\n                    -- retrieving quarterly budget values\r\n                    Select iAccountId,iProductId iProduct,iTag1, iTag2,iTag3, 0 BudgetAmt,\r\n\t                0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,iFromDate StartDate,sPlanName,\r\n                    0 BudgetQty, 0 PreCommittedQty,0 CommittedQty,0 SpentQty ,mCore_Budget_Quarterly.fValue QBudgetAmt, mCore_Budget_Quarterly.fQuantity QBudgetQty,\r\n                    0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty\r\n                    from   mCore_ConfirmedBudget_Details\t                \r\n                    inner join mCore_Budget_Revisions on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\r\n                    inner join mCore_Budget on  mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\r\n                    {3}\r\n                    left join mCore_Budget_Quarterly on mCore_ConfirmedBudget_Details.iBudgetConfirmId=mCore_Budget_Quarterly.iBudgetConfirmId\r\n\t                where bActive=1 and iProductId {13} {4} {11}\r\n\r\n\t                union all\r\n                    -- pre committed budget\r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,Sum(Amount) PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty, Sum(BudgetQty) PreCommittedQty, 0 CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty ,\r\n                    0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty\r\n                    From \r\n\t                ( \r\n\t\t                Select   b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,0 PreCommittedAmt,\r\n                        0 CommittedAmt,0 SpentAmt ,{22} FromDate, mCore_Budget.sPlanName,\r\n\t\t                IsNull(Sum(dQty-dQtyUsed),0) BudgetQty From tCore_PreCommittedBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId  Join \r\n\t\t                tCore_Header{0} c On c.iHeaderId = a.iHeaderId  \r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {22} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0  {11} {21}--and IsNull(b.bCancelled,0) = 0\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,{22}, mCore_Budget.sPlanName\r\n\t                ) A join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate \r\n                    {3}\r\n                    where 1=1  and iProduct {13} {4} \r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName\r\n\r\n\t                Union All\r\n                    --committed budget\r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,0 PreCommittedAmt,Sum(Amount) CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty,0 PreCommittedQty, Sum(BudgetQty) CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty,\r\n                    0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty\r\n                    From \r\n\t                ( \r\n\t\t                Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,0 PreCommittedAmt,0 CommittedAmt,\r\n                        0 SpentAmt ,c.iDate FromDate, mCore_Budget.sPlanName,\r\n                        IsNull(Sum(dQty-dQtyUsed),0) BudgetQty\r\n\t\t                From     tCore_CommittedBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId   \r\n\t\t                Join  tCore_Header{0} c On c.iHeaderId = a.iHeaderId                                                      \r\n\t\t                inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0 {11} {21}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,c.iDate, mCore_Budget.sPlanName\r\n\t                ) A join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {13} {4} \r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName\r\n\r\n\t                Union All\r\n                    --spent budget\r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,0 PreCommittedAmt,0 CommittedAmt,Sum(Amount) SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty, 0 PreCommittedQty, 0 CommittedQty,Sum(BudgetQty) SpentQty, 0 QBudgetAmt, 0 QBudgetQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty From \r\n\t                ( \r\n\t\t                Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,abs(IsNull(Sum(dAmt),0)) Amount,0 PreCommittedAmt,0 CommittedAmt,\r\n                        0 SpentAmt ,c.iDate FromDate, mCore_Budget.sPlanName,\r\n                        IsNull(Sum(dQty),0) BudgetQty\r\n\t\t                From     tCore_SpentBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId  \r\n\t\t                Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId\r\n\t\t                inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo   \r\n                        inner join mCore_Account On mCore_Account.iMasterId = b.iAccountId and mCore_Account.iAccountType Not in (3,10)\r\n                        where IsNull(b.bRejected,0) = 0 {11} {21}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,c.iDate, mCore_Budget.sPlanName\r\n                        {18}\r\n\t                ) A join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {13} {4}\r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName\r\n\r\n                    -- Query to get PreCommittedCancelledAmount and PreCommittedCancelledQuantity\r\n                    union all\r\n                    \r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty, 0 PreCommittedQty, 0 CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty , (dAmt) PrecommittedCancelledAmount, (dQty) PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty From \r\n\t                ( \r\n\t\t                Select   b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,0 PreCommittedAmt,\r\n                        0 CommittedAmt,0 SpentAmt ,{22} FromDate, mCore_Budget.sPlanName,\r\n\t\t                IsNull(Sum(dQty-dQtyUsed),0) BudgetQty,dAmt,dQty From tCore_PreCommittedBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId   \r\n\t\t                Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId  \r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {22} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0 and IsNull(b.bCancelled,0) = 1  {11} {21}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,{22}, mCore_Budget.sPlanName,dAmt,dQty\r\n\t                ) A join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1  and iProduct {13} {4} \r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName,dAmt,dQty\r\n\r\n                    -- Query to get CommittedCancelledAmount and CommittedCancelledQuantity\r\n                    Union All\r\n\r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty,0 PreCommittedQty, 0 CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, (dAmt) CommittedCancelledAmount, (dQty) CommittedCancelledQuantity, 0 ReturnAmt, 0 ReturnQty From \r\n\t                ( \r\n\t\t                Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,0 PreCommittedAmt,0 CommittedAmt,\r\n                        0 SpentAmt ,c.iDate FromDate, mCore_Budget.sPlanName,\r\n                        IsNull(Sum(dQty-dQtyUsed),0) BudgetQty,dAmt,dQty\r\n\t\t                From     tCore_CommittedBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId  \r\n\t\t                Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId                                                      \r\n\t\t                inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0 and IsNull(b.bCancelled,0) = 1 {11} {21}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,c.iDate, mCore_Budget.sPlanName,dAmt,dQty\r\n\t                ) A left join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {13} {4} \r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName,dAmt,dQty\r\n    \r\n                    -- Query to get Return Amount and Return Quantity\r\n                    Union All\r\n\r\n\t                Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,  \r\n                    0 BudgetQty,0 PreCommittedQty, 0 CommittedQty,0 SpentQty ,0 QBudgetAmt, 0 QBudgetQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity,sum(dAmt) ReturnAmt,sum(dQty) ReturnQty From \r\n\t                ( \r\n\t\t                Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,0 Amount,0 PreCommittedAmt,0 CommittedAmt,\r\n                        0 SpentAmt ,c.iDate FromDate, mCore_Budget.sPlanName,\r\n                        0 BudgetQty,dAmt,dQty\r\n\t\t                From     tCore_ReturnBudget{0} b Join \r\n\t\t                tCore_Data{0} a On a.iTransactionId = b.iTransId  \r\n\t\t                Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId                                                      \r\n\t\t                inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        inner join mCore_Account On mCore_Account.iMasterId = b.iAccountId and mCore_Account.iAccountType Not in (3,10)\r\n                        where IsNull(b.bRejected,0) = 0 {11} {21}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,c.iDate, mCore_Budget.sPlanName,dAmt,dQty\r\n                        {20}\r\n\t                ) A left join \r\n\t                dbo.fCore_GetStartEndDateForQuarter('{15}-{16}-{17}') Dates on A.FromDate >=Dates.StartDate and A.FromDate<= Dates.EndDate --on A.FromDate >={2} and a.FromDate<= {7}\r\n                    {3}\r\n                    where 1=1 and iProduct {13} {4} \r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName,dAmt,dQty                  \r\n                ) BudgetInner                    \r\n                group by iAccountId,iProduct,iTag1,iTag2,iTag3, StartDate,sPlanName\r\n        ) Temp\r\n        group by iAccountId,iProduct, iTag1,iTag2,iTag3,sPlanName\r\n) Budget\r\ninner join vrCore_Account on Budget.iAccountId= vrCore_Account.iMasterId and vrCore_Account.iTreeId =0\r\nleft join vrCore_Account AccGrp on AccGrp.iMasterId = vrCore_Account.iParentId and AccGrp.iTreeId =0\r\njoin vrCore_Product on Budget.iProduct= vrCore_Product.iMasterId \r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {14}\r\n{10}\r\n{9}  \r\nwhere 1=1 {12} \r\ngroup by iAccountId,vrCore_Account.sName,vrCore_Account.sCode, AccGrp.sName,sPlanName {19}", m_sSuffix, new Date(_focus.company(m_iCompanyId).accountingDate, CurrentData.get().CompanyCalendar), oRec.StartingDate, text11, empty4, stringBuilder2.ToString(), stringBuilder3, oRec.EndingDate, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty5, text8, text10, oRec.LanguageId, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Year, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Month, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Day, text17, "--@EXTRA_GROUPBY", text18, empty2, empty, num, num2, num3, num4, stringBuilder);
				standardQuery.PrimaryColumn = "iHeaderId";
				break;
			}
			case 2:
			{
				decimal num5 = 0m;
				decimal num6 = 0m;
				decimal num7 = 0m;
				decimal num8 = 0m;
				if (oRec.Masters.Count() > 0)
				{
					text11 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = iAccountId";
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				int num9 = new Date(_focus.company(m_iCompanyId).accountingDate, CurrentData.get().CompanyCalendar).Month;
				StringBuilder stringBuilder3 = new StringBuilder();
				empty2 = string.Format("and a.iAuthStatus = 1", m_sSuffix);
				if (flag)
				{
					empty2 = string.Format("and a.iAuthStatus < 2", m_sSuffix);
				}
				for (int i = 1; i <= 12; i++)
				{
					num5 = 0m;
					num6 = 0m;
					num7 = 0m;
					num8 = 0m;
					text19 = string.Format("\r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount1 > 0 then mAmount1 else 0 end,0)) Value \r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 \r\n\t\t                    Join mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) \r\n\t\t                    left Join tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iCode\r\n                    Where   dbo.GetDatePart('m',c.iDate) = {4}   and a.bUpdateFA = 1\r\n                    Union All \r\n                    Select  IsNull(Case When ISNull(i.fQuantity,0) < 0 then abs(i.fQuantity) else 0 end,0)  Qty, \r\n                            -(IsNull(Case when mAmount2 > 0 then mAmount2 else 0 end,0)) Value\r\n                    from    tCore_Data{0} a  Join \r\n                            tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                            mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                            tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n\t\t                    inner join fCore_fnDStringToTable('{3}',',') AccFilterTable on AccFilterTable.value = a.iBookNo\r\n                    Where    dbo.GetDatePart('m',c.iDate) = {4}  and a.bUpdateFA = 1\r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters), num9);
					dataReader3 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text19) : text19);
					while (dataReader3.Read())
					{
						num5 = Convert.ToDecimal(dataReader3["Value"]);
						num6 = Convert.ToDecimal(dataReader3["Qty"]);
					}
					dataReader3.Close();
					num3 = 0m;
					num4 = 0m;
					text20 = string.Format("\r\n                                Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                                        abs(Case when mAmount1 < 0 then mAmount1 else 0 end) Value \r\n                                from    tCore_Data{0} a  Join \r\n                                        tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                                        mCore_Account d On d.iMasterId = a.iCode and d.iAccountType in (3,10) left Join\r\n                                        tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                                Where  dbo.GetDatePart('m',c.iDate) = {4} and a.iCode = {3} and a.bUpdateFA = 1\r\n                                Union All \r\n                                Select  Case When ISNull(i.fQuantity,0) > 0 then abs(i.fQuantity) else 0 end  Qty, \r\n                                        abs(Case when mAmount2 < 0 then mAmount2 else 0 end) Value\r\n                                from    tCore_Data{0} a  Join \r\n                                        tCore_Header{0} c On c.iHeaderId = a.iHeaderId and c.bSuspended =0 Join\r\n                                        mCore_Account d On d.iMasterId = a.iBookNo and d.iAccountType in (3,10) left Join\r\n                                        tCore_Indta{0} i On i.iBodyId = a.iBodyId \r\n                                Where   dbo.GetDatePart('m',c.iDate) = {4} and a.iBookNo = {3} and a.bUpdateFA = 1\r\n                                ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, General.GetMasters(oRec.Masters), num9);
					dataReader4 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text20) : text20);
					while (dataReader4.Read())
					{
						num7 = Convert.ToDecimal(dataReader4["Value"]);
						num8 = Convert.ToDecimal(dataReader4["Qty"]);
					}
					dataReader4.Close();
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(MBudgetAmt) else 0 end [M{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then (case when SUM(PreCommittedAmt) >0 then 0 else abs(SUM(PreCommittedAmt)) end) else 0 end [PreCommited{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then (case when SUM(CommittedAmt) > 0 then 0 else abs(SUM(CommittedAmt)) end) else 0 end  [Commited{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(SpentAmt) else 0 end [Spent{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(ReturnAmt) else 0 end [Return{i}]");
					stringBuilder2.Append($",0 [Balance{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(MBudgetQty) else 0 end [Q{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(PreCommittedQty) else 0 end [PreCommitedQty{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(CommittedQty) else 0 end  [CommitedQty{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(SpentQty) else 0 end [SpentQty{i}]");
					stringBuilder2.Append($",Case When dbo.GetDatePart('m',BudgetInnerQ.StartDate) = {num9} Then SUM(ReturnQty) else 0 end [ReturnQty{i}]");
					stringBuilder2.Append($",0 [BalanceQty{i}]");
					stringBuilder3.Append(string.Format(",sum(M{0}) [M{0}], Sum(PreCommited{0}) [PreCommited{0}],sum([Commited{0}]) [Commited{0}], sum(Spent{0}) [Spent{0}], sum(Return{0}) [Return{0}], \r\ncase When sum(M{0}) = 0 Then 0 else sum(M{0} - (PreCommited{0}+ Commited{0}+Spent{0}) + Return{0}) End [Balance{0}]", i));
					stringBuilder3.Append(string.Format(",sum(Q{0}) [Q{0}], Sum(PreCommitedQty{0}) [PreCommitedQty{0}],sum([CommitedQty{0}]) [CommitedQty{0}], sum(SpentQty{0}) [SpentQty{0}], sum(ReturnQty{0}) [ReturnQty{0}], \r\ncase When sum(Q{0}) = 0 Then 0 else sum(Q{0} - (PreCommitedQty{0}+ CommitedQty{0}+SpentQty{0}) + ReturnQty{0}) End [BalanceQty{0}]", i));
					stringBuilder.Append(string.Format(",sum(M{0}) [M{0}], Sum(PreCommited{0}) [PreCommited{0}],sum([Commited{0}]) [Commited{0}], sum(Spent{0}) +({1}) [Spent{0}], sum(Return{0}) +({3}) [Return{0}], \r\ncase When sum(M{0}) = 0 Then 0 else sum(M{0} - (PreCommited{0}+ Commited{0}+Spent{0}) + Return{0}) End [Balance{0}]", i, num5, num6, num7, num8));
					stringBuilder.Append(string.Format(",sum(Q{0}) [Q{0}], Sum(PreCommitedQty{0}) [PreCommitedQty{0}],sum([CommitedQty{0}]) [CommitedQty{0}], sum(SpentQty{0})+({2}) [SpentQty{0}], sum(ReturnQty{0}) +({4}) [ReturnQty{0}], \r\ncase When sum(Q{0}) = 0 Then 0 else sum(Q{0} - (PreCommitedQty{0}+ CommitedQty{0}+SpentQty{0}) + ReturnQty{0}) End [BalanceQty{0}]", i, num5, num6, num7, num8));
					num9++;
					if (num9 == 13)
					{
						num9 = 1;
					}
				}
				empty = ((!flag2) ? "c.iDate" : "a.iDueDate");
				standardQuery.Query = string.Format("\r\nselect iAccountId [iHeaderId], sPlanName BudgetPlan, vrCore_Account.sCode [Account Code],vrCore_Account.sName [Account Name],AccGrp.sName [Account Group], sum(BudgetAmt) [Budgeted Value], sum(PreCommittedAmt) [Pre Committed Value], sum(CommittedAmt) [Committed Value], sum(SpentAmt) +abs({27}) [Consumed Value],sum(ReturnAmt) +abs({29}) [Return Value], \r\nsum(BudgetAmt -(PreCommittedAmt + CommittedAmt + SpentAmt ) + ReturnAmt) [Available Balance],  \r\nsum(BudgetQty) [Budgeted Quantity], sum(PreCommittedQty) [Pre Committed Quantity], sum(CommittedQty) [Committed Quantity], sum(SpentQty) +({28}) [Consumed Quantity], sum(ReturnQty) +({30}) [Return Quantity],\r\nsum(BudgetQty -(PreCommittedQty + CommittedQty + SpentQty ) + ReturnQty) [Available Quantity],sum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity, \r\n        sum(PreCommittedAmt) PreCommittedBalanceValue, sum(PreCommittedQty) PreCommittedBalanceQty, \r\n        sum(CommittedAmt) CommittedBalanceValue, sum(CommittedQty) CommittedBalanceQty\r\n{31} {9} from\r\n(\r\n        select iAccountId , iProduct, sum(BudgetAmt) BudgetAmt, sum(PreCommittedAmt) PreCommittedAmt, sum(CommittedAmt) CommittedAmt , sum(SpentAmt) SpentAmt , sum(ReturnAmt) ReturnAmt,\r\n        0 [Available Balance] ,iTag1,iTag2,iTag3,sPlanName, sum(BudgetQty) BudgetQty, sum(PreCommittedQty) PreCommittedQty, sum(CommittedQty) CommittedQty, sum(SpentQty) SpentQty , sum(ReturnQty) ReturnQty,\r\n        0 [Available Quantity] ,sum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity \r\n        --,(PreCommittedAmt - PrecommittedCancelledAmount) PreCommittedBalanceValue, (PreCommittedQty - PrecommittedCancelledQuantity) PreCommittedBalanceQty, \r\n        --(CommittedAmt - CommittedCancelledAmount) CommittedBalanceValue, (CommittedQty - CommittedCancelledQuantity) CommittedBalanceQty\r\n{6}\r\n        from \r\n        (        \r\n                Select iAccountId,iProduct , sum(BudgetAmt) BudgetAmt, (case when sum(PreCommittedAmt) > 0 then 0 else abs(sum(PreCommittedAmt))  end) PreCommittedAmt, (case when sum(CommittedAmt) > 0 then 0 else abs(sum(CommittedAmt)) end) CommittedAmt,sum(abs(SpentAmt))  SpentAmt,sum(abs(ReturnAmt))  ReturnAmt,iTag1,iTag2,iTag3,sPlanName,\r\n                sum(BudgetQty) BudgetQty, sum(abs(PreCommittedQty)) PreCommittedQty, sum(abs(CommittedQty)) CommittedQty, sum(abs(SpentQty)) SpentQty, sum(abs(ReturnQty)) ReturnQty, \r\n                sum(PrecommittedCancelledAmount) PrecommittedCancelledAmount, sum(PrecommittedCancelledQuantity) PrecommittedCancelledQuantity, sum(CommittedCancelledAmount) CommittedCancelledAmount, sum(CommittedCancelledQuantity) CommittedCancelledQuantity\r\n                {5} from\r\n                (\r\n                    -- Retrieving over all budget values\r\n                    Select iAccountId,iProductId iProduct,iTag1, iTag2,iTag3, mCore_ConfirmedBudget_Details.fValue BudgetAmt,\r\n                    0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,0 StartDate, sPlanName\r\n                    , mCore_ConfirmedBudget_Details.fQuantity BudgetQty,0 PreCommittedQty,0 CommittedQty,0 SpentQty, 0 MBudgetAmt, 0 MBudgetQty \r\n                    , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n                    from   mCore_ConfirmedBudget_Details \r\n                    inner join mCore_Budget_Revisions on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\r\n                    inner join mCore_Budget on  mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId  \r\n                    {3} \r\n                    --left join mCore_Budget_Monthly on mCore_ConfirmedBudget_Details.iBudgetConfirmId=mCore_Budget_Monthly.iBudgetConfirmId               \r\n                    where bActive=1 and iProductId {14} {4} {12}\r\n\r\n                    union all\r\n                        \r\n                    -- Retrieving Monthly budget values \r\n                    Select iAccountId,iProductId iProduct,iTag1, iTag2,iTag3, 0 BudgetAmt,\r\n                    0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,iFromDate StartDate, sPlanName\r\n                    , 0 BudgetQty,0 PreCommittedQty,0 CommittedQty,0 SpentQty, mCore_Budget_Monthly.fValue MBudgetAmt, mCore_Budget_Monthly.fQuantity MBudgetQty \r\n                    , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n                    from   mCore_ConfirmedBudget_Details \r\n                    inner join mCore_Budget_Revisions on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\r\n                    inner join mCore_Budget on  mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId  \r\n                    {3} \r\n                    left join mCore_Budget_Monthly on mCore_ConfirmedBudget_Details.iBudgetConfirmId=mCore_Budget_Monthly.iBudgetConfirmId               \r\n                    where bActive=1 and iProductId {14} {4} {12}\r\n\r\n                    union all\r\n                    -- Retrieving PreCommitted budget values\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    Sum(Amount) PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName,\r\n                    0 BudgetQty ,Sum(BudgetQty) PreCommittedQty,0 CommittedQty,0 SpentQty ,0 MBudgetAmt, 0 MBudgetQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n                    From \r\n\t                ( \r\n\t                    Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,\r\n                        {26} FromDate, mCore_Budget.sPlanName, IsNull(Sum(dQty-dQtyUsed),0) BudgetQty\r\n\t                    From     tCore_PreCommittedBudget{0} b Join \r\n\t\t\t            tCore_Data{0} a On a.iTransactionId = b.iTransId   \r\n\t\t\t            Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId \r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {26} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0  {12} {25} --and IsNull(b.bCancelled,0) = 0\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,{26}, mCore_Budget.sPlanName\r\n\t                ) A join \r\n\t                dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4}\r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate,sPlanName \r\n \r\n                    Union All\r\n                    -- Retrieving Committed budget values\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    0 PreCommittedAmt,Sum(Amount) CommittedAmt,0 SpentAmt ,FromDate StartDate ,sPlanName,\r\n                    0 BudgetQty ,0 PreCommittedQty,Sum(BudgetQty) CommittedQty,0 SpentQty,0 MBudgetAmt, 0 MBudgetQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty From \r\n                    ( \r\n                        Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,IsNull(Sum(dAmt-dAmtUsed),0) Amount,\r\n                        c.iDate FromDate, mCore_Budget.sPlanName, IsNull(Sum(dQty-dQtyUsed),0) BudgetQty\r\n                        From     tCore_CommittedBudget{0} b Join \r\n                        tCore_Data{0} a On a.iTransactionId = b.iTransId  \r\n                        Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId\r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n                        mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                        and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                        \r\n                        where IsNull(b.bRejected,0) = 0 {12} {25}\r\n                        Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2, b.iTag3,c.iDate,mCore_Budget.sPlanName\r\n                    ) A join \r\n                    dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4}\r\n                    Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName \r\n\r\n                    Union All\r\n                    -- Retrieving Spent budget values\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    0 PreCommittedAmt,0 CommittedAmt,Sum(Amount) SpentAmt ,FromDate StartDate,sPlanName, \r\n                    0 BudgetQty ,0 PreCommittedQty,0 CommittedQty,abs(Sum(BudgetQty)) SpentQty,0 MBudgetAmt, 0 MBudgetQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty From \r\n                    ( \r\n                        Select   b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,\r\n                        abs(IsNull(Sum(dAmt),0)) Amount,c.iDate FromDate, mCore_Budget.sPlanName,IsNull(Sum(dQty),0) BudgetQty\r\n                        From     tCore_SpentBudget{0} b Join \r\n                        tCore_Data{0} a On a.iTransactionId = b.iTransId \r\n                        Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId\r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n                        mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo           \r\n                        inner join mCore_Account On mCore_Account.iMasterId = b.iAccountId and mCore_Account.iAccountType Not in (3,10)\r\n                        where IsNull(b.bRejected,0) = 0 {12} {25}\r\n                        Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2, b.iTag3,c.iDate,mCore_Budget.sPlanName\r\n                        {22}\r\n                    ) A join \r\n                    dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4}\r\n                    Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName \r\n\r\n                    -- Query to get PrecommittedCancelledAmount and PrecommittedCancelledQuantity\r\n                    union all\r\n\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,0 StartDate,sPlanName,\r\n                    0 BudgetQty ,0 PreCommittedQty,0 CommittedQty,0 SpentQty ,0 MBudgetAmt, 0 MBudgetQty, (dAmt) PrecommittedCancelledAmount, (dQty) PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n                    From \r\n\t                ( \r\n\t                    Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,0 Amount,\r\n                        {26} FromDate, mCore_Budget.sPlanName, 0 BudgetQty,dAmt,dQty\r\n\t                    From     tCore_PreCommittedBudget{0} b Join \r\n\t\t\t            tCore_Data{0} a On a.iTransactionId = b.iTransId \r\n\t\t\t            Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId \r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n\t                    mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and {26} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                                                                            \r\n                        where IsNull(b.bRejected,0) = 0 and IsNull(b.bCancelled,0) = 1 {12} {25}\r\n\t                    Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,{26}, mCore_Budget.sPlanName,dAmt,dQty\r\n\t                ) A \r\n                    join  dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4} and A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n\t                Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,sPlanName,dAmt,dQty \r\n\r\n                     -- Query to get CommittedCancelledAmount and CommittedCancelledQuantity\r\n                    Union All\r\n\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,0 StartDate ,sPlanName,\r\n                    0 BudgetQty ,0 PreCommittedQty,0 CommittedQty,0 SpentQty,0 MBudgetAmt, 0 MBudgetQty , 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, (dAmt) CommittedCancelledAmount, (dQty) CommittedCancelledQuantity , 0 ReturnAmt, 0 ReturnQty\r\n                    From \r\n                    ( \r\n                        Select b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,0 Amount,\r\n                        c.iDate FromDate, mCore_Budget.sPlanName, 0 BudgetQty,dAmt,dQty\r\n                        From     tCore_CommittedBudget{0} b Join \r\n                        tCore_Data{0} a On a.iTransactionId = b.iTransId \r\n                        Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId\r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n                        mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                        and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo                                                        \r\n                        where IsNull(b.bRejected,0) = 0 {12} {25}\r\n                        Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2, b.iTag3,c.iDate,mCore_Budget.sPlanName,dAmt,dQty\r\n                    ) A left join \r\n                    dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4}\r\n                    Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,Dates.StartDate ,sPlanName ,dAmt,dQty\r\n    \r\n                    -- Query to get Return Amount and Return Quantity\r\n                    Union All\r\n                    Select iAccountId,iProduct,iTag1, iTag2,iTag3,0 Amount ,\r\n                    0 PreCommittedAmt,0 CommittedAmt,0 SpentAmt ,FromDate StartDate,sPlanName, \r\n                    0 BudgetQty ,0 PreCommittedQty,0 CommittedQty,0 SpentQty,0 MBudgetAmt, 0 MBudgetQty, 0 PrecommittedCancelledAmount, 0 PrecommittedCancelledQuantity, 0 CommittedCancelledAmount, 0 CommittedCancelledQuantity , Sum(Amount) ReturnAmt, abs(Sum(BudgetQty)) ReturnQty From \r\n                    ( \r\n                        Select   b.iAccountId,b.iProduct,b.iTag1, b.iTag2,b.iTag3,\r\n                        abs(IsNull(Sum(dAmt),0)) Amount,c.iDate FromDate, mCore_Budget.sPlanName,IsNull(Sum(dQty),0) BudgetQty\r\n                        From     tCore_ReturnBudget{0} b Join \r\n                        tCore_Data{0} a On a.iTransactionId = b.iTransId  \r\n                        Join tCore_Header{0} c On c.iHeaderId = a.iHeaderId\r\n                        inner join mCore_ConfirmedBudget_Details with (readuncommitted)  on mCore_ConfirmedBudget_Details.iAccountId = b.iAccountId and\r\n                        mCore_ConfirmedBudget_Details.iProductId = b.iProduct and mCore_ConfirmedBudget_Details.iTag1 = b.iTag1\r\n\t                    and mCore_ConfirmedBudget_Details.iTag2 = b.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = b.iTag3\r\n\t                    inner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\n\t                    inner join mCore_Budget  with (readuncommitted) On mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId and c.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo           \r\n                        inner join mCore_Account On mCore_Account.iMasterId = b.iAccountId and mCore_Account.iAccountType Not in (3,10)\r\n                        where IsNull(b.bRejected,0) = 0 {12} {25}\r\n                        Group By b.iAccountId,b.iProduct,b.iTag1, b.iTag2, b.iTag3,c.iDate,mCore_Budget.sPlanName\r\n                        {24}\r\n                    ) A join \r\n                    dbo.fCore_GetStartEndDateForMonth('{16}-{17}-{18}','{19}-{20}-{21}') Dates on A.FromDate >=StartDate and a.FromDate<= Dates.EndDate\r\n                    {3}\r\n                    where 1=1 and iProduct {14} {4}\r\n                    Group by  iAccountId,iProduct,iTag1, iTag2,iTag3,FromDate ,sPlanName \r\n\r\n                ) BudgetInnerQ                        \r\n                group by iAccountId,iProduct,iTag1,iTag2,iTag3, StartDate,sPlanName \r\n        ) Temp\r\n        group by iAccountId,iProduct,iTag1,iTag2,iTag3,sPlanName\r\n) Budget\r\njoin vrCore_Account on Budget.iAccountId= vrCore_Account.iMasterId and vrCore_Account.iTreeId = 0\r\nleft join vrCore_Account AccGrp on AccGrp.iMasterId = vrCore_Account.iParentId and AccGrp.iTreeId = 0\r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {15}\r\njoin vrCore_Product on Budget.iProduct= vrCore_Product.iMasterId  and vrCore_Product.iTreeId = 0 \r\n{11}\r\n{10}  \r\nwhere 1=1 {13}\r\ngroup by iAccountId,vrCore_Account.sCode,vrCore_Account.sName ,AccGrp.sName,sPlanName  {23}", m_sSuffix, new Date().IntToDate(oRec.StartingDate), new Date().IntToDate(oRec.EndingDate), text11, empty4, stringBuilder2.ToString(), stringBuilder3, oRec.StartingDate, oRec.EndingDate, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty5, text8, text10, oRec.LanguageId, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Year, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Month, new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Day, new Date(oRec.EndingDate, CurrentData.get().CompanyCalendar).Year, new Date(oRec.EndingDate, CurrentData.get().CompanyCalendar).Month, new Date(oRec.EndingDate, CurrentData.get().CompanyCalendar).Day, text17, "--@EXTRA_GROUPBY", text18, empty2, empty, num, num2, num3, num4, stringBuilder);
				standardQuery.PrimaryColumn = "iHeaderId";
				break;
			}
			}
			break;
		}
		case FocusReport.RevisedBudgetReport:
		{
			standardQuery = new StandardQuery();
			arrDefaultTables = new List<string>();
			arrDefaultTables.Add("vrCore_Account");
			arrDefaultTables.Add("vrCore_Product");
			if (oRec.StartingDate == 0)
			{
				oRec.StartingDate = _focus.company(m_iCompanyId).accountingDate;
			}
			string empty4 = string.Empty;
			string text21 = string.Empty;
			string empty5 = string.Empty;
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				empty4 += $" and b.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 1)}";
				text21 += $" and c.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 1)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				empty4 += $" and b.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 2)}";
				text21 += $" and c.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				empty4 += $" and b.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 3)}";
				text21 += $" and c.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 6) > 0)
			{
				empty5 += $" and mCore_Budget.iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				string text22 = $"select iValidFrom, iValidTo from mCore_Budget where iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				IDataReader dataReader5 = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text22) : text22);
				while (dataReader5.Read())
				{
					oRec.StartingDate = Convert.ToInt32(dataReader5["iValidFrom"]);
					oRec.EndingDate = Convert.ToInt32(dataReader5["iValidTo"]);
				}
				dataReader5.Close();
			}
			else
			{
				empty5 += $" and mCore_Budget.iValidFrom >= {oRec.StartingDate} and mCore_Budget.iValidTo <= {oRec.EndingDate}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 7) > 0)
			{
				empty4 += $" and b.iProductId = {FConvert.GetInputValue(oRec.Inputs, 7)}";
				text21 += $" and c.iProductId = {FConvert.GetInputValue(oRec.Inputs, 7)}";
			}
			int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 4);
			int preferenceValue2 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 5);
			int preferenceValue3 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			string text5 = string.Format("");
			if (preferenceValue > 0)
			{
				text5 = string.Format(" inner join {0} on Budget.iTag1 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue).viewName);
			}
			if (preferenceValue2 > 0)
			{
				text5 += string.Format("inner join {0} on Budget.iTag2 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue2).viewName);
			}
			if (preferenceValue3 > 0)
			{
				text5 += string.Format(" inner join {0} on Budget.iTag3 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue3).viewName);
			}
			string text23 = string.Empty;
			string text12 = "";
			if (oRec.Masters.Count() > 0)
			{
				text23 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = c.iAccountId";
				text12 = $"inner join fCore_fnDStringToTable('{General.GetMasters(oRec.Masters)}',',') AccFilterTable on AccFilterTable.value = b.iAccountId";
			}
			standardQuery.Query = string.Format("\r\nSelect iAccountId iHeaderId,sPlanName BudgetPlan, Type, sRevisionNo Version,iReviseNo RevisionNo, vrCore_Account.sCode AccountCode,vrCore_Account.sName AccountName, AccGrp.sName AccountGroup,  vrCore_Product.sCode ProductCode, vrCore_Product.sName ProductName,\r\nsum(OverAllBudget) OverAllBudget, sum(ApprovedBudget) ApprovedBudget,sum( [Add/ReduceBudget]) [Add/ReduceBudget],sum(abs( [TransferIn]))  [TransferIn],sum(abs( TransferOut))  TransferOut,\r\nsum(OverAllQuantity) OverAllQuantity, sum(ApprovedQuantity) ApprovedQuantity, sum( [Add/ReduceQuantity]) [Add/ReduceQuantity],sum(abs( TransferInQuantity))  TransferInQuantity,sum(abs(TransferOutQuantity))  TransferOutQuantity {4}\r\nfrom (\r\n\tselect 'Define' Type, c.iAccountId,c.iProductId,c.iTag1,c.iTag2,c.iTag3,c.fValue OverAllBudget,mCore_Budget_Details.fValue ApprovedBudget,\r\n\t0 [Add/ReduceBudget] ,0 [TransferIn], 0 TransferOut , mCore_Budget.sPlanName , c.fQuantity OverAllQuantity, mCore_Budget_Details.fQuantity ApprovedQuantity,\r\n\t0 [Add/ReduceQuantity] ,0 [TransferInQuantity], 0 TransferOutQuantity, sRevisionNo ,'' iReviseNo \r\n\tfrom  mCore_ConfirmedBudget_Details c \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = c.iRevisionId\r\n    inner join mCore_Budget_Details with (readuncommitted) On mCore_Budget_Details.iRevisionId = mCore_Budget_Revisions.iRevisionId\r\n    and mCore_Budget_Details.iAccountId = c.iAccountId and mCore_Budget_Details.iProductId = c.iProductId and mCore_Budget_Details.iTag1 = c.iTag1 and\r\n\tmCore_Budget_Details.iTag2 =c.iTag2 and mCore_Budget_Details.iTag3 = c.iTag3\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t \r\n    {0}\r\n\twhere bActive =1 and bNew=0 and mCore_Budget_Details.iStatus = 1 {8} {7}   --Define Budget\r\n\r\n    union all\r\n\tselect 'Append' Type, b.iAccountId,b.iProductId,b.iTag1,b.iTag2,b.iTag3,c.fValue OverAllBudget,b.fValue ApprovedBudget ,0 [Add/ReduceBudget],0 [TransferIn], 0 TransferOut , mCore_Budget.sPlanName,\r\n    c.fQuantity OverAllQuantity, b.fQuantity ApprovedQuantity,\t0 [Add/ReduceQuantity] ,0 [TransferInQuantity], 0 TransferOutQuantity, sRevisionNo, a.iReviseNo\r\n    from mCore_ReviseBudget a Join\r\n\tmCore_ReviseBudget_Details b On a.iReviseId =b.iReviseId Join mCore_ConfirmedBudget_Details c on c.iBudgetConfirmId =b.iBudgetConfirmId \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = c.iRevisionId\r\n\t--inner join mCore_Budget_Details with (readuncommitted) on mCore_Budget_Details.iRevisionId = c.iRevisionId and mCore_Budget_Details.iStatus =1\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t \r\n    {9}\r\n\twhere iType =0 and b.iStatus =1 {8} {3} --Append Budget\r\n\r\n\tUnion all\r\n\tselect Case when c.bNew = 0 then 'Define' else 'Add/Reduce' end Type, b.iAccountId,b.iProductId,b.iTag1,b.iTag2,b.iTag3,0 OverAllBudget,0 ApprovedBudget, b.fValue [Add/ReduceBudget],0 [TransferIn], 0 TransferOut , mCore_Budget.sPlanName,\r\n    0 OverAllQuantity, 0 ApprovedQuantity,\tb.fQuantity [Add/ReduceQuantity] ,0 [TransferInQuantity], 0 TransferOutQuantity, sRevisionNo, a.iReviseNo   \r\n    from mCore_ReviseBudget a Join\r\n\tmCore_ReviseBudget_Details b On a.iReviseId =b.iReviseId Join mCore_ConfirmedBudget_Details c on c.iBudgetConfirmId =b.iBudgetConfirmId \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = c.iRevisionId\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t \r\n    {9}\r\n\twhere iType =1 and b.iStatus=1 {8} {3} --add/reduce\r\n\r\n\tUnion all\r\n\tselect Case when c.bNew = 0 then 'Define' else 'Transfer' end Type, b.iAccountId,b.iProductId,b.iTag1,b.iTag2,b.iTag3,0 OverAllBudget,0 ApprovedBudget,0 [Add/ReduceBudget],0 [TransferIn] , b.fValue TransferOut , mCore_Budget.sPlanName,\r\n    0 OverAllQuantity, 0 ApprovedQuantity,\t0 [Add/ReduceQuantity] ,0 [TransferInQuantity], b.fQuantity TransferOutQuantity, sRevisionNo, a.iReviseNo \r\n    from mCore_ReviseBudget a Join \r\n\tmCore_ReviseBudget_Details b On a.iReviseId =b.iReviseId Join mCore_ConfirmedBudget_Details c on c.iBudgetConfirmId =b.iBudgetConfirmId \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = c.iRevisionId\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t \r\n\t{9}\r\n    where iType =2 and b.iStatus=1 and iMainBudgetConfirmId = 0 {8} {3}  --transfer\r\n\r\n\tUnion all\r\n\tselect Case when c.bNew = 0 then 'Define' else 'Transfer' end Type, b.iAccountId,b.iProductId,b.iTag1,b.iTag2,b.iTag3,0 OverAllBudget,0 ApprovedBudget,0 [Add/ReduceBudget] ,b.fValue [TransferIn] , 0 TransferOut , mCore_Budget.sPlanName,\r\n    0 OverAllQuantity, 0 ApprovedQuantity,\t0 [Add/ReduceQuantity] ,b.fQuantity [TransferInQuantity], 0 TransferOutQuantity, sRevisionNo, a.iReviseNo\r\n    from mCore_ReviseBudget a Join \r\n\tmCore_ReviseBudget_Details b On a.iReviseId =b.iReviseId Join mCore_ConfirmedBudget_Details c on c.iBudgetConfirmId =b.iBudgetConfirmId \r\n    inner join mCore_Budget_Revisions with (readuncommitted) on mCore_Budget_Revisions.iRevisionId = c.iRevisionId\r\n\tinner join mCore_Budget with (readuncommitted) on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\t \r\n    {9}\r\n\twhere iType =2 and b.iStatus=1 and iMainBudgetConfirmId > 0 {8} {3}  --transfer\r\n)\r\nBudget\r\njoin vrCore_Account on Budget.iAccountId= vrCore_Account.iMasterId and vrCore_Account.iTreeId =0\r\njoin vrCore_Account AccGrp on AccGrp.iMasterId= vrCore_Account.iParentId and AccGrp.iTreeId =0\r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {10}\r\njoin vrCore_Product on Budget.iProductId= vrCore_Product.iMasterId and vrCore_Product.iTreeId =0\r\n{6}\r\n{5}\r\ngroup by iAccountId,vrCore_Account.sCode,vrCore_Account.sName,AccGrp.sName, sPlanName,Type , vrCore_Product.sCode , vrCore_Product.sName, sRevisionNo, iReviseNo {11}", text23, oRec.StartingDate, oRec.EndingDate, empty4, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, text21, empty5, text12, oRec.LanguageId, "--@EXTRA_GROUPBY");
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		case FocusReport.BudgetAuthorizationReport:
		{
			standardQuery = new StandardQuery();
			arrDefaultTables = new List<string>();
			arrDefaultTables.Add("vrCore_Account");
			arrDefaultTables.Add("vrCore_Product");
			if (oRec.StartingDate == 0)
			{
				oRec.StartingDate = _focus.company(m_iCompanyId).accountingDate;
			}
			int inputValue2 = FConvert.GetInputValue(oRec.Inputs, 7);
			string arg = string.Empty;
			string empty4 = string.Empty;
			string text6 = string.Empty;
			string text7 = string.Empty;
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				empty4 += $" and mCore_Budget_Details.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 4)}";
				text7 += $" and mCore_ReviseBudget_Details.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				empty4 += $" and mCore_Budget_Details.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				text7 += $" and mCore_ReviseBudget_Details.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 5)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 6) > 0)
			{
				empty4 += $" and mCore_Budget_Details.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				text7 += $" and mCore_ReviseBudget_Details.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 6)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) == 1)
				{
					arg = "Pending";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 1) == 2)
				{
					arg = "Confirmed";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 1) == 3)
				{
					arg = "Rejected";
				}
				text6 += string.Format("where Auth.Level1AuthStatus = '{0}' OR Auth.Level2AuthStatus = '{0}' OR Auth.Level3AuthStatus = '{0}' ", arg);
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				empty4 += $" and mCore_Budget_Details.iAccountId = {FConvert.GetInputValue(oRec.Inputs, 2)}";
				text7 += $" and mCore_ReviseBudget_Details.iAccountId = {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				empty4 += $" and mCore_Budget_Details.iProductId = {FConvert.GetInputValue(oRec.Inputs, 3)}";
				text7 += $" and mCore_ReviseBudget_Details.iProductId = {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 4);
			int preferenceValue2 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 5);
			int preferenceValue3 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			string text5 = string.Format("");
			if (preferenceValue > 0)
			{
				text5 = string.Format(" inner join {0} on Auth.iTag1 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue).viewName);
			}
			if (preferenceValue2 > 0)
			{
				text5 += string.Format("inner join {0} on Auth.iTag2 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue2).viewName);
			}
			if (preferenceValue3 > 0)
			{
				text5 += string.Format(" inner join {0} on Auth.iTag3 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue3).viewName);
			}
			List<FieldData> arrMasterField = new List<FieldData>();
			List<MasterRights> list = new COptionbase().CheckRoleMasters(oRec.UserId, "1,3,5,7", ref arrMasterField, m_db, m_iCompanyId);
			_ = string.Empty;
			if (list != null && list.Count > 0)
			{
				foreach (MasterRights item in list)
				{
					string arg2 = (item.Exclude ? "NOT IN" : "IN");
					if (item.ID == 1)
					{
						empty4 += $" and mCore_Budget_Details.iAccountId {arg2} ({item.Tag})";
						text7 += $" and mCore_ReviseBudget_Details.iAccountId {arg2} ({item.Tag})";
					}
				}
			}
			standardQuery.PrimaryColumn = "iBudgetDetailId";
			standardQuery.Query = string.Format("\r\nselect iBudgetDetailId, Auth.BudgetType, Type, RevisionNumber, vrCore_Account.sName Account, vrCore_Product.sName Product, Quantity, BudgetValue \r\n, max(Level1User) Level1User, max(Level1AuthDate) Level1AuthDate, max(Level1AuthStatus) Level1AuthStatus,max(Level1AuthRemarks) Level1AuthRemarks\r\n, max(Level2User) Level2User, max(Level2AuthDate) Level2AuthDate, max(Level2AuthStatus) Level2AuthStatus,max(Level2AuthRemarks) Level2AuthRemarks\r\n, max(Level3User) Level3User, max(Level3AuthDate) Level3AuthDate, max(Level3AuthStatus) Level3AuthStatus,max(Level3AuthRemarks) Level3AuthRemarks\r\n, max(Level4User) Level4User, max(Level4AuthDate) Level4AuthDate, max(Level4AuthStatus) Level4AuthStatus,max(Level4AuthRemarks) Level4AuthRemarks {2}\r\nfrom \r\n(\r\n\t\tselect iBudgetDetailId, mCore_AuthorizationDetails{0}.iAuthorizationDetailId ,mCore_AuthorizationDetails{0}.iLevel,\r\n        Budget.BudgetType, Type, RevisionNumber,iAccountId, iProductId, Quantity, BudgetValue, iTag1, iTag2, iTag3, Budget.iStatus \r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 0 then max(case when bRole = 0 then  mSec_Users.sLoginName else mSec_RoleHeader.sRoleName end) else '' end Level1User\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 0 then max(iAuthDate) else 0 end Level1AuthDate\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 0 then max(Status) else '' end Level1AuthStatus\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 0 then max(Budget.sReason) else '' end Level1AuthRemarks\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 1 then max(case when bRole = 0 then  mSec_Users.sLoginName else mSec_RoleHeader.sRoleName end) else '' end Level2User\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 1 then max(iAuthDate) else 0 end Level2AuthDate\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 1 then max(Status) else '' end Level2AuthStatus\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 1 then max(Budget.sReason) else '' end Level2AuthRemarks\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 2 then max(case when bRole = 0 then  mSec_Users.sLoginName else mSec_RoleHeader.sRoleName end) else '' end Level3User\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 2 then max(iAuthDate) else 0 end Level3AuthDate\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 2 then max(Status) else '' end Level3AuthStatus\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 2 then max(Budget.sReason) else '' end Level3AuthRemarks\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 3 then max(case when bRole = 0 then  mSec_Users.sLoginName else mSec_RoleHeader.sRoleName end) else '' end Level4User\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 3 then max(iAuthDate) else 0 end Level4AuthDate\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 3 then max(Status) else '' end Level4AuthStatus\r\n\t\t, case when mCore_AuthorizationDetails{0}.iLevel = 3 then max(Budget.sReason) else '' end Level4AuthRemarks\r\n\t\tfrom\r\n\t\t(\r\n\t\t\tselect iDetailId iBudgetDetailId,'Define Budget' [BudgetType], 'Define' [Type], '' RevisionNumber,iAccountId,iProductId, fQuantity Quantity, fValue BudgetValue \r\n\t\t\t,tCore_BudgetAuthUser.iRoleOrUserId,  case tCore_BudgetAuthUser.iStatus when 0 then 'Pending' when 1 then 'Confirmed' when 2 then 'Rejected'  else '' end Status, iAuthNodeId\r\n\t\t\t,iAuthDate, iAuthTime, tCore_BudgetAuthUser.sReason\t, iTag1,iTag2, iTag3, bRole, tCore_BudgetAuthUser.iStatus \r\n\t\t\tfrom mCore_Budget \r\n\t\t\tinner join mCore_Budget_Revisions on mCore_Budget_Revisions.iBudgetId = mCore_Budget.iBudgetId\r\n\t\t\tinner join mCore_Budget_Details on mCore_Budget_Details.iRevisionId = mCore_Budget_Revisions.iRevisionId\t\r\n\t\t\tinner join tCore_BudgetAuth on tCore_BudgetAuth.iRevisonReviseId = mCore_Budget_Revisions.iRevisionId and tCore_BudgetAuth.bBudgetTypeId = 0 and tCore_BudgetAuth.iDetailId = mCore_Budget_Details.iBudgetDetailId\r\n\t\t\tleft join tCore_BudgetAuthUser on tCore_BudgetAuthUser.iAuthId = tCore_BudgetAuth.iAuthId\t\r\n\t\t\twhere mCore_Budget.iBudgetId = {1}  {5} -- and mCore_Budget_Details.iStatus <> 5\r\n\r\n\t\t\tunion all\r\n\r\n\t\t\tselect iDetailId iBudgetDetailId ,'Revise Budget' [Budget Type], case when mCore_ReviseBudget.iType = 0 then 'Append' when  mCore_ReviseBudget.iType = 1 then 'Add/Edit' else 'Transfer' End ,  iReviseNo RevisionNumber,\r\n\t\t\tmCore_ReviseBudget_Details.iAccountId ,mCore_ReviseBudget_Details.iProductId,  mCore_ReviseBudget_Details.fQuantity Quantity, mCore_ReviseBudget_Details.fValue BudgetValue \r\n\t\t\t,tCore_BudgetAuthUser.iRoleOrUserId,  case tCore_BudgetAuthUser.iStatus when 0 then 'Pending' when 1 then 'Confirmed' when 2 then 'Rejected'  else '' end Status, iAuthNodeId\r\n\t\t\t,iAuthDate, iAuthTime, tCore_BudgetAuthUser.sReason, mCore_ReviseBudget_Details.iTag1,mCore_ReviseBudget_Details.iTag2, mCore_ReviseBudget_Details.iTag3, bRole,tCore_BudgetAuthUser.iStatus\r\n\t\t\tfrom mCore_ReviseBudget\r\n\t\t\tinner join mCore_ReviseBudget_Details on mCore_ReviseBudget_Details.iReviseId = mCore_ReviseBudget.iReviseId\r\n\t\t\tinner join mCore_ConfirmedBudget_Details on mCore_ConfirmedBudget_Details.iBudgetConfirmId = mCore_ReviseBudget_Details.iBudgetConfirmId\r\n\t\t\tinner join mCore_Budget_Revisions on mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId\t\r\n\t\t\tleft join mCore_Budget on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId\r\n\t\t\tleft join tCore_BudgetAuth on tCore_BudgetAuth.iRevisonReviseId = mCore_ReviseBudget_Details.iReviseId and tCore_BudgetAuth.bBudgetTypeId = 1 and tCore_BudgetAuth.iDetailId = mCore_ReviseBudget_Details.iReviseDetailId\r\n\t\t\tleft join tCore_BudgetAuthUser on tCore_BudgetAuthUser.iAuthId = tCore_BudgetAuth.iAuthId \r\n\t\t\twhere mCore_Budget.iBudgetId = {1}  {6} ----and mCore_ReviseBudget_Details.iStatus <> 5\r\n\r\n\t\t) Budget\t\t\r\n\t\tleft join mSec_Users on  mSec_Users.iUserId = Budget.iRoleOrUserId and Budget.bRole =0\r\n        left join mSec_RoleHeader on  mSec_RoleHeader.iRoleId = Budget.iRoleOrUserId and Budget.bRole =1\r\n\t\tinner join mCore_AuthorizationDetails{0} on mCore_AuthorizationDetails{0}.iAuthorizationDetailId = Budget.iAuthNodeId\r\n\t\tgroup by iBudgetDetailId ,mCore_AuthorizationDetails{0}.iAuthorizationDetailId ,mCore_AuthorizationDetails{0}.iLevel, BudgetType, \r\n\t\tType, RevisionNumber,iAccountId , iProductId  , Quantity, BudgetValue , iTag1, iTag2, iTag3\t, Budget.iStatus\t\r\n) Auth\r\ninner join vrCore_Account on  vrCore_Account.iMasterId = Auth.iAccountId and vrCore_Account.iTreeId = 0\r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {7}\r\nLeft Join tCore_Data{0} on tCore_Data{0}.iCode = Auth.iAccountId -- to do added this 2 extra join to avoid exception from adv filter in report \r\nLeft join tCore_Data{0} Book on Book.iBookNo = Auth.iAccountId \r\n{3}\r\n{4}\r\nleft join vrCore_Product on  vrCore_Product.iMasterId = Auth.iProductId {9}\r\ngroup by  Auth.BudgetType, Type, RevisionNumber, vrCore_Account.sName , vrCore_Product.sName, Quantity, BudgetValue, Auth.iBudgetDetailId ,iAccountId, iProductId, iTag1, iTag2, iTag3 {8} \r\n ", m_sSuffix, inputValue2, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty4, text7, oRec.LanguageId, "--@EXTRA_GROUPBY", text6);
			break;
		}
		case FocusReport.PreCommittedBudgetReport:
		{
			standardQuery = new StandardQuery();
			string empty3 = string.Empty;
			string text = string.Empty;
			arrDefaultTables.AddRange($"vrCore_Account,vrCore_Product,tCore_Header{m_sSuffix}".Split(','));
			if (oRec.StartingDate == 0)
			{
				oRec.StartingDate = _focus.company(m_iCompanyId).accountingDate;
			}
			int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 4);
			int preferenceValue2 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 5);
			int preferenceValue3 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			int preferenceValue4 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Tag, 0);
			int preferenceValue5 = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Tag, 1);
			string empty4 = string.Empty;
			_ = string.Empty;
			_ = string.Empty;
			string text2 = string.Empty;
			string text3 = string.Empty;
			string empty5 = string.Empty;
			int inputValue = FConvert.GetInputValue(oRec.Inputs, 7);
			string empty6 = string.Empty;
			empty6 = inputValue switch
			{
				0 => "tCore_PreCommittedBudget" + m_sSuffix, 
				1 => "tCore_CommittedBudget" + m_sSuffix, 
				2 => "tCore_SpentBudget" + m_sSuffix, 
				_ => "tCore_ReturnBudget" + m_sSuffix, 
			};
			if (FConvert.GetInputValue(oRec.Inputs, 8) > 0)
			{
				empty4 += $" and mCore_Budget.iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 8)}";
				string text4 = $"select iValidFrom, iValidTo from mCore_Budget where iBudgetId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				IDataReader dataReader = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text4) : text4);
				while (dataReader.Read())
				{
					oRec.StartingDate = Convert.ToInt32(dataReader["iValidFrom"]);
					oRec.EndingDate = Convert.ToInt32(dataReader["iValidTo"]);
				}
				dataReader.Close();
			}
			else
			{
				empty4 += $" and mCore_Budget.iValidFrom >= {oRec.StartingDate} and mCore_Budget.iValidTo <= {oRec.EndingDate}";
			}
			empty3 += $" and tCore_Header{m_sSuffix}.iDate between {oRec.StartingDate} and {oRec.EndingDate} ";
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				empty4 += $" and {empty6}.iTag1 = {FConvert.GetInputValue(oRec.Inputs, 4)}";
				empty3 += $" and {_focus.company(m_iCompanyId).master(preferenceValue).viewName}.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				empty4 += $" and {empty6}.iTag2 = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				empty3 += $" and {_focus.company(m_iCompanyId).master(preferenceValue2).viewName}.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 5)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 6) > 0)
			{
				empty4 += $" and {empty6}.iTag3 = {FConvert.GetInputValue(oRec.Inputs, 6)}";
				empty3 += $" and {_focus.company(m_iCompanyId).master(preferenceValue3).viewName}.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 6)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				empty4 += $" and {empty6}.iAccountId = {FConvert.GetInputValue(oRec.Inputs, 2)}";
				string.Format(" and a.iCode = {1} ", empty6, FConvert.GetInputValue(oRec.Inputs, 2));
				string.Format(" and a.iBookNo = {1} ", empty6, FConvert.GetInputValue(oRec.Inputs, 2));
				text2 = $" and tCore_Data{m_sSuffix}.iCode = {FConvert.GetInputValue(oRec.Inputs, 2)} ";
				text3 = $" and tCore_Data{m_sSuffix}.iBookNo = {FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				empty4 += $" and {empty6}.iProduct = {FConvert.GetInputValue(oRec.Inputs, 3)}";
				empty3 += $" and tCore_Indta{m_sSuffix}.iProduct = {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			string text5 = string.Format("");
			if (preferenceValue > 0)
			{
				text5 = string.Format(" inner join {0} on {1}.iTag1 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName, empty6);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue).viewName);
				if (preferenceValue == preferenceValue4)
				{
					text = string.Format(" inner join {0} on tCore_Data{1}.iFaTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName, m_sSuffix);
				}
				else
				{
					text = ((preferenceValue != preferenceValue5) ? string.Format(" inner join {0} on tCore_Data_Tags{1}.iTag{2} = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName, m_sSuffix, preferenceValue) : string.Format(" inner join {0} on tCore_Data{1}.iInvTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue).viewName, m_sSuffix));
				}
			}
			if (preferenceValue2 > 0)
			{
				text5 += string.Format("inner join {0} on {1}.iTag2 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName, empty6);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue2).viewName);
				if (preferenceValue2 == preferenceValue4)
				{
					text += string.Format(" inner join {0} on tCore_Data{1}.iFaTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName, m_sSuffix);
				}
				else
				{
					text = ((preferenceValue2 != preferenceValue5) ? (text + string.Format(" inner join {0} on tCore_Data_Tags{1}.iTag{2} = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName, m_sSuffix, preferenceValue2)) : (text + string.Format(" inner join {0} on tCore_Data{1}.iInvTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue2).viewName, m_sSuffix)));
				}
			}
			if (preferenceValue3 > 0)
			{
				text5 += string.Format(" inner join {0} on {1}.iTag3 = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName, empty6);
				arrDefaultTables.Add(_focus.company(m_iCompanyId).master(preferenceValue3).viewName);
				if (preferenceValue3 == preferenceValue4)
				{
					text += string.Format(" inner join {0} on tCore_Data{1}.iFaTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName, m_sSuffix);
				}
				else
				{
					text = ((preferenceValue3 != preferenceValue5) ? (text + string.Format(" inner join {0} on tCore_Data_Tags{1}.iTag{2} = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName, m_sSuffix, preferenceValue3)) : (text + string.Format(" inner join {0} on tCore_Data{1}.iInvTag = {0}.iMasterId and {0}.iTreeId = 0", _focus.company(m_iCompanyId).master(preferenceValue3).viewName, m_sSuffix)));
				}
			}
			empty2 = $"and tCore_Data{m_sSuffix}.iAuthStatus = 1";
			if (flag)
			{
				empty2 = $"and tCore_Data{m_sSuffix}.iAuthStatus < 2";
			}
			empty = ((!flag2) ? ("tCore_Header" + m_sSuffix + ".iDate") : ("tCore_Data" + m_sSuffix + ".iDueDate"));
			switch (inputValue)
			{
			case 0:
				standardQuery.Query = string.Format("\r\nselect  tCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccountCode, vrCore_Account.sName AccountName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo,\r\ntCore_Header{0}.iDate, VendorAcc.sCode VendorCode,VendorAcc.sName VendorName, (dAmt-dAmtUsed) PreCommittedAmount,(dQty-dQtyUsed) PreCommittedQuantity {1}\r\nfrom tCore_PreCommittedBudget{0} with (readuncommitted)\r\ninner join tCore_Data{0} on tCore_Data{0}.iTransactionId =  tCore_PreCommittedBudget{0}.iTransId \r\ninner join tCore_Header{0} on tCore_Header{0}.iHeaderId =  tCore_Data{0}.iHeaderId And tCore_Header{0}.bSuspended = 0 \r\ninner join cCore_Vouchers{0} with (readuncommitted) on  cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\ninner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_PreCommittedBudget{0}.iAccountId and\r\n    mCore_ConfirmedBudget_Details.iProductId = tCore_PreCommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_PreCommittedBudget{0}.iTag1\r\n    and mCore_ConfirmedBudget_Details.iTag2 = tCore_PreCommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_PreCommittedBudget{0}.iTag3\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n    and {7} between mCore_Budget.iValidFrom and mCore_Budget.iValidTo \r\ninner join vrCore_Account on vrCore_Account.iMasterId = tCore_PreCommittedBudget{0}.iAccountId   and vrCore_Account.iTreeId = 0                 \r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {5} \r\nleft join vrCore_Product on  vrCore_Product.iMasterId = tCore_PreCommittedBudget{0}.iProduct  and vrCore_Product.iTreeId = 0\r\ninner join vrCore_Account VendorAcc on VendorAcc.iMasterId = tCore_Data{0}.iBookNo and VendorAcc.iTreeId = 0 \r\n{2}\r\n{3}\r\nwhere bSuspended=0 and tCore_Header{0}.bCancelled=0 and IsNull(tCore_PreCommittedBudget{0}.bRejected,0) = 0 {4} {6}\r\n", m_sSuffix, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty4, oRec.LanguageId, empty2, empty);
				break;
			case 1:
				standardQuery.Query = string.Format("\r\nselect  tCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, VendorAcc.sCode VendorCode,VendorAcc.sName VendorName,(dAmt-dAmtUsed) CommittedAmount,(dQty-dQtyUsed) CommittedQuantity  {1}\r\nfrom tCore_CommittedBudget{0} with (readuncommitted)\r\ninner join tCore_Data{0} on tCore_Data{0}.iTransactionId =  tCore_CommittedBudget{0}.iTransId  \r\ninner join tCore_Header{0} on tCore_Header{0}.iHeaderId =  tCore_Data{0}.iHeaderId And tCore_Header{0}.bSuspended = 0 \r\ninner join cCore_Vouchers{0} with (readuncommitted) on  cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\ninner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_CommittedBudget{0}.iAccountId and\r\n    mCore_ConfirmedBudget_Details.iProductId = tCore_CommittedBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_CommittedBudget{0}.iTag1\r\n    and mCore_ConfirmedBudget_Details.iTag2 = tCore_CommittedBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_CommittedBudget{0}.iTag3\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n    and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo \r\ninner join vrCore_Account on vrCore_Account.iMasterId = tCore_CommittedBudget{0}.iAccountId  and vrCore_Account.iTreeId = 0                 \r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {5}\r\nleft join vrCore_Product on  vrCore_Product.iMasterId = tCore_CommittedBudget{0}.iProduct and vrCore_Product.iTreeId = 0\r\ninner join vrCore_Account VendorAcc on VendorAcc.iMasterId = tCore_Data{0}.iBookNo and VendorAcc.iTreeId = 0 \r\n{2}\r\n{3}\r\nwhere bSuspended=0 and tCore_Header{0}.bCancelled=0 and IsNull(tCore_CommittedBudget{0}.bRejected,0) = 0 {4} {6}\r\n", m_sSuffix, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty4, oRec.LanguageId, empty2);
				break;
			case 2:
				standardQuery.Query = string.Format("\r\nselect  tCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, VendorAcc.sCode VendorCode,VendorAcc.sName VendorName,dAmt SpentAmount,dQty SpentQuantity {1}\r\nfrom tCore_SpentBudget{0} with (readuncommitted)\r\ninner join tCore_Data{0} on tCore_Data{0}.iTransactionId =  tCore_SpentBudget{0}.iTransId \r\ninner join tCore_Header{0} on tCore_Header{0}.iHeaderId =  tCore_Data{0}.iHeaderId And tCore_Header{0}.bSuspended = 0 \r\ninner join cCore_Vouchers{0} with (readuncommitted) on  cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\ninner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_SpentBudget{0}.iAccountId and\r\n    mCore_ConfirmedBudget_Details.iProductId = tCore_SpentBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_SpentBudget{0}.iTag1\r\n    and mCore_ConfirmedBudget_Details.iTag2 = tCore_SpentBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_SpentBudget{0}.iTag3\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n    and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo \r\ninner join vrCore_Account on vrCore_Account.iMasterId = tCore_SpentBudget{0}.iAccountId and vrCore_Account.iTreeId = 0                    \r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {5}\r\nleft join vrCore_Product on  vrCore_Product.iMasterId = tCore_SpentBudget{0}.iProduct and vrCore_Product.iTreeId = 0  \r\ninner join vrCore_Account VendorAcc on VendorAcc.iMasterId = tCore_Data{0}.iBookNo and VendorAcc.iTreeId = 0 \r\n{2}\r\n{3}\r\nwhere bSuspended=0 and tCore_Header{0}.bCancelled=0 and IsNull(tCore_SpentBudget{0}.bRejected,0) = 0 {4} {6}\r\n\r\n--Including Budget estimation as spent budget. Retrieving the estimated spend values\r\nunion all\r\nSelect \r\ntCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, '' VendorCode,'' VendorName,-(IsNull(Case when mAmount1 > 0 then mAmount1 else 0 end,0)) SpentAmount,IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) SpentQuantity {1}\r\nfrom    tCore_Data{0}   \r\n\t\tJoin tCore_Header{0}  On tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId and tCore_Header{0}.bSuspended =0 \r\n        inner join tCore_Data_Tags{0} on tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t\tinner join cCore_Vouchers{0} on cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n\t\tJoin vrCore_Account On vrCore_Account.iMasterId = tCore_Data{0}.iCode and vrCore_Account.iAccountTypeId in (3,10) \r\n\t\tleft Join tCore_Indta{0}  On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\tinner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n{10}\r\nWhere   tCore_Data{0}.bUpdateFA = 1  {7} {9}\r\nand (abs(IsNull(Case when mAmount1 > 0 then mAmount1 else 0 end,0)) <> 0) or (IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) <> 0)\r\nUnion All \r\nSelect \r\ntCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, '' VendorCode,'' VendorName,-(IsNull(Case when mAmount2 > 0 then mAmount2 else 0 end,0)) SpentAmount\r\n,IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) SpentQuantity {1}\r\nfrom    tCore_Data{0}   \r\n\t\tJoin tCore_Header{0}  On tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId and tCore_Header{0}.bSuspended =0 \r\n        inner join tCore_Data_Tags{0} on tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t\tinner join cCore_Vouchers{0} on cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n\t\tJoin vrCore_Account On vrCore_Account.iMasterId = tCore_Data{0}.iBookNo and vrCore_Account.iAccountTypeId in (3,10) \r\n\t\tleft Join tCore_Indta{0}  On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\tinner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n{10}\r\nWhere   tCore_Data{0}.bUpdateFA = 1 {8} {9}\r\nand (abs(IsNull(Case when mAmount2 > 0 then mAmount2 else 0 end,0)) <> 0) or (IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) < 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) <> 0)\r\n\r\n", m_sSuffix, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty4, oRec.LanguageId, empty2, text2, text3, empty3, text);
				break;
			case 3:
				standardQuery.Query = string.Format("\r\nselect  tCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, VendorAcc.sCode VendorCode,VendorAcc.sName VendorName,dAmt ReturnAmount, dQty ReturnQuantity {1}\r\n\r\nfrom tCore_ReturnBudget{0} with (readuncommitted)\r\ninner join tCore_Data{0} on tCore_Data{0}.iTransactionId =  tCore_ReturnBudget{0}.iTransId  \r\ninner join tCore_Header{0} on tCore_Header{0}.iHeaderId =  tCore_Data{0}.iHeaderId And tCore_Header{0}.bSuspended = 0 \r\ninner join cCore_Vouchers{0} with (readuncommitted) on  cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\ninner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_ReturnBudget{0}.iAccountId and\r\n    mCore_ConfirmedBudget_Details.iProductId = tCore_ReturnBudget{0}.iProduct and mCore_ConfirmedBudget_Details.iTag1 = tCore_ReturnBudget{0}.iTag1\r\n    and mCore_ConfirmedBudget_Details.iTag2 = tCore_ReturnBudget{0}.iTag2 and mCore_ConfirmedBudget_Details.iTag3 = tCore_ReturnBudget{0}.iTag3\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n    and tCore_Header{0}.iDate between mCore_Budget.iValidFrom and mCore_Budget.iValidTo \r\ninner join vrCore_Account on vrCore_Account.iMasterId = tCore_ReturnBudget{0}.iAccountId  and vrCore_Account.iTreeId = 0                          \r\nJOIN mCore_AccountLanguage ON vrCore_Account.iMasterId = mCore_AccountLanguage.iMasterId AND mCore_AccountLanguage.iLanguageId = {5}\r\nleft join vrCore_Product on  vrCore_Product.iMasterId = tCore_ReturnBudget{0}.iProduct and vrCore_Product.iTreeId = 0  \r\ninner join vrCore_Account VendorAcc on VendorAcc.iMasterId = tCore_Data{0}.iBookNo and VendorAcc.iTreeId = 0 \r\n{2}\r\n{3}\r\nwhere bSuspended=0 and tCore_Header{0}.bCancelled=0 and IsNull(tCore_ReturnBudget{0}.bRejected,0) = 0 {4} {6}\r\n\r\n--Including Budget estimation as spent budget. Retrieving the estimated return values\r\nunion all\r\nSelect \r\ntCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, '' VendorCode,'' VendorName,abs(Case when mAmount1 < 0 then mAmount1 else 0 end) ReturnAmount\r\n,IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) ReturnQuantity {1}\r\nfrom    tCore_Data{0}   \r\n\t\tJoin tCore_Header{0}  On tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId and tCore_Header{0}.bSuspended =0 \r\n        inner join tCore_Data_Tags{0} on tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t\tinner join cCore_Vouchers{0} on cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n\t\tJoin vrCore_Account On vrCore_Account.iMasterId = tCore_Data{0}.iCode and vrCore_Account.iAccountTypeId in (3,10) \r\n\t\tleft Join tCore_Indta{0}  On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\tinner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iCode\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n{10}\r\nWhere   tCore_Data{0}.bUpdateFA = 1  {7} {9} \r\nand (abs(Case when mAmount1 < 0 then mAmount1 else 0 end) <> 0) or (IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) <> 0)\r\nUnion All \r\nSelect \r\ntCore_Header{0}.iHeaderId,mCore_Budget.sPlanName BudgetPlan, vrCore_Account.sCode AccoumtCode, vrCore_Account.sName AccoumtName, cCore_Vouchers{0}.sName DocumentType, tCore_Header{0}.sVoucherNo DocumentNo, \r\ntCore_Header{0}.iDate, '' VendorCode,'' VendorName,abs(Case when mAmount2 < 0 then mAmount2 else 0 end) ReturnAmount\r\n,IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) ReturnQuantity {1}\r\nfrom    tCore_Data{0}   \r\n\t\tJoin tCore_Header{0}  On tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId and tCore_Header{0}.bSuspended =0 \r\n        inner join tCore_Data_Tags{0} on tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t\tinner join cCore_Vouchers{0} on cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n\t\tJoin vrCore_Account On vrCore_Account.iMasterId = tCore_Data{0}.iBookNo and vrCore_Account.iAccountTypeId in (3,10) \r\n\t\tleft Join tCore_Indta{0}  On tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\tinner join mCore_ConfirmedBudget_Details with (readuncommitted) on mCore_ConfirmedBudget_Details.iAccountId = tCore_Data{0}.iBookNo\r\ninner join mCore_Budget_Revisions with (readuncommitted)  On mCore_Budget_Revisions.iRevisionId = mCore_ConfirmedBudget_Details.iRevisionId \r\ninner join mCore_Budget with (readuncommitted)  on mCore_Budget.iBudgetId = mCore_Budget_Revisions.iBudgetId \r\n{10}\r\nWhere   tCore_Data{0}.bUpdateFA = 1 {8} {9} \r\nand (abs(Case when mAmount2 < 0 then mAmount2 else 0 end) <> 0) or (IsNull(Case When ISNull(tCore_Indta{0}.fQuantity,0) > 0 then abs(tCore_Indta{0}.fQuantity) else 0 end,0) <> 0)\r\n", m_sSuffix, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", text5, empty4, oRec.LanguageId, empty2, text2, text3, empty3, text);
				break;
			}
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		}
		return standardQuery;
	}

	private StandardQuery callWMSQuery(RepRecord oRec, bool bPreviousYear, ref List<string> arrDefaultTables)
	{
		StandardQuery standardQuery = null;
		string text = null;
		string text2 = null;
		string text3 = null;
		int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.Tag, 16);
		bool flag = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.WMS, 54) > 0;
		if (CurrentData.get().Masters != null)
		{
			_ = CurrentData.get().Masters;
		}
		string viewName = _focus.company(m_iCompanyId).master(preferenceValue).viewName;
		switch ((FocusReport)oRec.ReportId)
		{
		case FocusReport.WMSBillingReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("  select Bill.iServiceTypeId [iHeaderId], Ser.sName ServiceName, Bill.iChargeType \r\n                    --,sum(Bill.dTotalQty)dTotalQty\r\n                     ,case when Bill.iChargeType=0 then sum(Bill.dTotalCBM) else sum(Bill.dTotalQty) end dTotalQty\r\n                    ,case when Bill.iChargeType=0 then sum(Bill.dTotalCharge)/sum(Bill.dTotalCBM) else sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty) end dRate, \r\n                    sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge\r\n\t\t\t\t                    from \r\n\t\t\t\t                    tWms_BillingHeader BH join\r\n\t\t\t\t                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n\t\t\t\t                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n\t\t\t\t                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} and Bill.iChargeType not in (5,6) {3} \r\n                                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate,Bill.iChargeType\r\n\r\n                    union all\r\n\r\n                    select Bill.iServiceTypeId [iHeaderId], '' ServiceName, Bill.iChargeType \r\n                    --,sum(Bill.dTotalQty)dTotalQty\r\n                     ,0 dTotalQty\r\n                    ,0 dRate, \r\n                    sum(Bill.dTotalCharge+Bill.dRecCharges) dTotalCharge \r\n\t\t\t\t                    from \r\n\t\t\t\t                    tWms_BillingHeader BH join\r\n\t\t\t\t                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n\t\t\t\t                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n\t\t\t\t                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} and Bill.iChargeType in (5,6) {3} \r\n                                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate,Bill.iChargeType\r\n\r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSBillingSKUWiseReport:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text += $" and P.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("   select Bill.iServiceTypeId [iHeaderId], I.iProduct iSKUId, P.sName ProductName, Ser.sName ServiceName, Bill.iChargeType \r\n                    ,sum(Bill.dTotalQty)dTotalQty,sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty) dRate, sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge                    \r\n                    ,'' sBin from \r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join (select iSkidId,\r\n                    stuff((\r\n                    select distinct ',' + sBatchNo\r\n                    from tCore_Bins_0 bin\r\n                    inner join tCore_Batch_0 batch on bin.iBodyId= batch.iBodyId\r\n                    where bin.iSkidId= tCore_Bins_0.iSkidId\r\n                    for xml path('')\r\n\t\t\r\n                    ),1,1,'') as BatchNo\r\n                    from tCore_Bins_0\r\n                    --inner join tCore_Data_0 D on D.iBodyId=tCore_Bins_0.iBodyId\r\n                    --\t\tinner join tCore_Header_0 H on H.iHeaderId=D.iHeaderId\t\t\r\n                    --        where H.iVoucherType=9128 \r\n                    group by iSkidId)SkidnBatch on SkidnBatch.iSkidId=Bill.iLotNo\r\n                    inner join tCore_Bins_0 BB on BB.iSkidId=SkidnBatch.iSkidId\r\n                    inner join tCore_Indta_0 I on I.iBodyId=BB.iBodyId\r\n                    inner join tCore_Data_0 D on D.iBodyId=BB.iBodyId\r\n\t\t\t\t\tinner join tCore_Header_0 H on D.iHeaderId=H.iHeaderId\r\n                    join mCore_product P on P.iMasterId=I.iProduct\r\n                    where   Bill.iBillDate>={1} and  Bill.iBillDate<={2}   and Bill.iServiceTypeId=0 and H.iVoucherType in (9216,1176)\r\n                    and Bill.iChargeType=3 and Bill.iServiceTypeId=0 {3}\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, BH.iFromDate, BH.iToDate--, H.iVoucherType\r\n                    , Bill.iBillDate,SkidnBatch.BatchNo,I.iProduct , P.sName--,Bill.dTotalQty,Bill.dTotalCharge\r\n\r\n                    union all\r\n\r\n\t\t\t\t\t   --Storing for other than pallet\r\n                    select Bill.iServiceTypeId [iHeaderId], Bill.iSKUId, P.sName ProductName, Ser.sName ServiceName\r\n\t\t\t\t\t, Bill.iChargeType ,sum(Bill.dTotalQty)dTotalQty,sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty), sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge                    \r\n                    ,'' sBin from \r\n\r\n\t\t\t\t\t--select * from\r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\t\t\t\t\t\t\t\t\t\r\n                    --left join tCore_Header_0 H on H.iHeaderId=BH.iHeaderId\r\n                    left join tCore_Bins_0 Bin on Bin.iLotNo=Bill.iLotNo\r\n                    left join tCore_Batch_0 Bat on Bat.iBodyId=Bin.iBodyId\r\n\t\t\t\t\tleft join tCore_Data_0 D on D.iBodyId=Bin.iBodyId\r\n\t\t\t\t\tleft join tCore_Header_0 H on H.iHeaderId=D.iHeaderId\r\n                    where   Bill.iBillDate>={1} and  Bill.iBillDate<={2}  {3}  and Bill.iServiceTypeId=0 and H.iVoucherType in (9216,1176)\r\n                    and Bill.iChargeType not in (3,5,6)\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName,Bat.sBatchNo, H.sVoucherNo,BH.iFromDate, BH.iToDate--, H.iVoucherType\r\n\t\t\t\t\t, Bill.iBillDate\r\n\r\n\t\t\t\t\t  union all\r\n\r\n\t\t\t\t\t   --Storing for Fix type charge and Bin type charge\r\n                    select Bill.iServiceTypeId [iHeaderId],Bill.iSKUId, '' ProductName, '' ServiceName,\r\n                    Bill.iChargeType iChargeType, 0 dTotalQty,0 dRate, sum(Bill.dTotalCharge+Bill.dRecCharges) dTotalCharge, isnull(B.sCode,'')sBin   \r\n                    from \t\t\t\t\t\r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Bins B on B.iMasterId= isnull(Bill.iBinId,0)\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\t\r\n                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} and Bill.iChargeType in (5,6)  {3}\r\n                    group by Bill.iServiceTypeId,  Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName,BH.iFromDate, BH.iToDate--, H.iVoucherType\r\n\t\t\t\t\t, Bill.iBillDate,isnull(B.sCode,''),Bill.iBinId\r\n\r\n\t\t\t\t\tunion all\r\n\r\n                    --Service charges\r\n\t\t\t\t\tselect Bill.iServiceTypeId [iHeaderId], Bill.iSKUId, P.sName ProductName, Ser.sName ServiceName, Bill.iChargeType ,\r\n                    --,sum(Bill.dTotalQty)dTotalQty,\r\n\t\t\t\t\tcase when Bill.iChargeType=0 then sum(PU.fLength*PU.fWidth*PU.fHeight)*Bill.dTotalQty else sum(Bill.dTotalQty)end dTotalQty\r\n\t\t\t\t\t,case when Bill.iChargeType=0 then sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/(sum(PU.fLength*PU.fWidth*PU.fHeight)*Bill.dTotalQty) else sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty)end dRate\r\n                    , sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge,'' sBin\r\n                    \r\n                    from \r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\t\r\n\t\t\t\t\tjoin muCore_Product_Units PU on PU.iMasterId=P.iMasterId\r\n\t\t\t\t\tjoin tCore_Header_0 H on H.iHeaderId=Bill.iDocumentId\t\t\r\n                    join cCore_Vouchers_0 V on H.iVoucherType=V.iVoucherType\t\t\t\t\t\t\r\n                    --left join tCore_Header_0 H on H.iHeaderId=BH.iHeaderId                  \r\n                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2}  {3}\r\n\t\t\t\t\t--and P.iMasterId=43 \t\r\n\t\t\t\t\tand Bill.iServiceTypeId!=0\tand Bill.dTotalQty>0\t\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName\r\n\t\t\t\t\t,BH.iFromDate, BH.iToDate  ,H.sVoucherNo, Bill.iBillDate, V.sAbbr,Bill.dTotalQty  ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSBillingSKUBatchWise:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text += $" and P.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("   \r\n                    \t\t\t\t--Storing Pallet wise\r\n                    select Bill.iServiceTypeId [iHeaderId], Ser.sName ServiceName, Bill.iChargeType , I.iProduct iSKUId, P.sName ProductName\r\n                    ,sum(Bill.dTotalQty)dTotalQty,sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty) dRate, sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge,\r\n                    '' sVoucherNo, Bill.iBillDate, SkidnBatch.BatchNo sBatchNo, Bill.iBillDate iFromDate, Bill.iBillingEndDate iToDate--, H.iVoucherType\r\n                    ,'' sBin from \r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join (select iSkidId,\r\n                    stuff((\r\n                    select distinct ',' + sBatchNo\r\n                    from tCore_Bins_0 bin\r\n                    inner join tCore_Batch_0 batch on bin.iBodyId= batch.iBodyId\r\n                    where bin.iSkidId= tCore_Bins_0.iSkidId\r\n                    for xml path('')\r\n\t\t\r\n                    ),1,1,'') as BatchNo\r\n                    from tCore_Bins_0\r\n                    --inner join tCore_Data_0 D on D.iBodyId=tCore_Bins_0.iBodyId\r\n                    --\t\tinner join tCore_Header_0 H on H.iHeaderId=D.iHeaderId\t\t\r\n                    --        where H.iVoucherType=9128 \r\n                    group by iSkidId)SkidnBatch on SkidnBatch.iSkidId=Bill.iLotNo\r\n                    inner join tCore_Bins_0 BB on BB.iSkidId=SkidnBatch.iSkidId\r\n                    inner join tCore_Indta_0 I on I.iBodyId=BB.iBodyId\r\n                    inner join tCore_Data_0 D on D.iBodyId=BB.iBodyId\r\n\t\t\t\t\tinner join tCore_Header_0 H on D.iHeaderId=H.iHeaderId\r\n                    join mCore_product P on P.iMasterId=I.iProduct\r\n                    where   Bill.iBillDate>={1} and  Bill.iBillDate<={2}  {3} and Bill.iServiceTypeId=0 and H.iVoucherType in (9216,1176)\r\n                    and Bill.iChargeType=3\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, Bill.iBillDate, Bill.iBillingEndDate--, H.iVoucherType\r\n                    , Bill.iBillDate,SkidnBatch.BatchNo,I.iProduct , P.sName--,Bill.dTotalQty,Bill.dTotalCharge\r\n\r\n                    union all\r\n\r\n\r\n                    --Storing for other than pallet\r\n                    select Bill.iServiceTypeId [iHeaderId], Ser.sName ServiceName, Bill.iChargeType , Bill.iSKUId, P.sName ProductName\r\n                    ,sum(Bill.dTotalQty)dTotalQty,sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty)dRate, sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge,\r\n                    '' sVoucherNo, Bill.iBillDate, ISNULL(Bat.sBatchNo,'')sBatchNo, Bill.iBillDate iFromDate, Bill.iBillingEndDate iToDate--, H.iVoucherType\r\n                    ,'' sBin from \r\n\r\n\t\t\t\t\t--select * from\r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\t\t\t\t\t\t\t\t\t\r\n                    --left join tCore_Header_0 H on H.iHeaderId=BH.iHeaderId\r\n                    left join tCore_Bins_0 Bin on Bin.iLotNo=Bill.iLotNo\r\n                    left join tCore_Batch_0 Bat on Bat.iBodyId=Bin.iBodyId\r\n\t\t\t\t\tleft join tCore_Data_0 D on D.iBodyId=Bin.iBodyId\r\n\t\t\t\t\tleft join tCore_Header_0 H on H.iHeaderId=D.iHeaderId\r\n                    where   Bill.iBillDate>={1} and  Bill.iBillDate<={2} {3}  and Bill.iServiceTypeId=0 and H.iVoucherType in (9216,1176)\r\n                    and Bill.iChargeType not in (3,5,6)\r\n                    --and H.iVoucherType = case when Bill.iChargeType=3 then H.iVoucherType else (9216) end\r\n                    --and (H.iVoucherType = case when Bill.iChargeType=3 then H.iVoucherType else (9216) end or H.iVoucherType = case when Bill.iChargeType=3 then H.iVoucherType else (11776) end )\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName,Bat.sBatchNo, H.sVoucherNo, Bill.iBillDate, Bill.iBillingEndDate--, H.iVoucherType\r\n\t\t\t\t\t, Bill.iBillDate\r\n\r\n\t\t\t\t\t  union all\r\n\r\n\t\t\t\t\t   --Storing for Fix type charge and Bin type charge\r\n                    select Bill.iServiceTypeId [iHeaderId],'' ServiceName, Bill.iChargeType  iChargeType,Bill.iSKUId, P.sName ProductName,\r\n                    0 dTotalQty,0 dRate, sum(Bill.dTotalCharge+Bill.dRecCharges) dTotalCharge      ,     \r\n\t\t\t\t\t'' sVoucherNo, Bill.iBillDate, ''sBatchNo, Bill.iBillDate iFromDate, Bill.iBillingEndDate iToDate\r\n                    , isnull(B.sCode,'')sBin from \t\t\t\t\t\r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\r\n                    join mCore_Bins B on B.iMasterId= isnull(Bill.iBinId,0)\r\n                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} and Bill.iChargeType in (5,6) {3} \r\n                    group by Bill.iServiceTypeId, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName, Bill.iBillDate, Bill.iBillingEndDate--, H.iVoucherType\r\n\t\t\t\t\t, Bill.iBillDate,isnull(B.sCode,''),Bill.iBinId\r\n\r\n\t\t\t\t\tunion all\r\n\r\n                    --Service charges\r\n\t\t\t\t\tselect Bill.iServiceTypeId [iHeaderId], Ser.sName ServiceName, Bill.iChargeType , Bill.iSKUId, P.sName ProductName,\r\n                    --,sum(Bill.dTotalQty)dTotalQty,\r\n\t\t\t\t\tcase when Bill.iChargeType=0 then sum(PU.fLength*PU.fWidth*PU.fHeight)*Bill.dTotalQty else sum(Bill.dTotalQty)end dTotalQty\r\n\t\t\t\t\t,case when Bill.iChargeType=0 then sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/(sum(PU.fLength*PU.fWidth*PU.fHeight)*Bill.dTotalQty) else sum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty)end dRate\r\n                    , sum(Bill.dTotalCharge) dTotalCharge\r\n                    , V.sAbbr + H.sVoucherNo, Bill.iBillDate\r\n\t\t\t\t\t, '' sBatchNo, Bill.iBillDate iFromDate, Bill.iBillingEndDate,'' sBin\r\n                    from \r\n                    tWms_BillingHeader BH join\r\n                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                    join mCore_product P on P.iMasterId=Bill.iSKUId\t\r\n\t\t\t\t\tjoin muCore_Product_Units PU on PU.iMasterId=P.iMasterId\r\n\t\t\t\t\tjoin tCore_Header_0 H on H.iHeaderId=Bill.iDocumentId\t\t\r\n                    join cCore_Vouchers_0 V on H.iVoucherType=V.iVoucherType\t\t\t\t\t\t\r\n                    --left join tCore_Header_0 H on H.iHeaderId=BH.iHeaderId                  \r\n                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} {3}\r\n\t\t\t\t\t--and P.iMasterId=43 \t\r\n\t\t\t\t\tand Bill.iServiceTypeId!=0\tand Bill.dTotalQty>0\t\t\t\r\n                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate ,Bill.iSKUId, P.sName\r\n\t\t\t\t\t, Bill.iBillDate, Bill.iBillingEndDate  ,H.sVoucherNo, Bill.iBillDate, V.sAbbr,Bill.dTotalQty ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSCurrentStockReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select 0 iHeaderId, 'SKU1' SKUCode, 'SKUName' SKUName, 'SKUCategory' SKUCategory, 'PaL1'PalletCode , 5 Qty", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSSKUwiseInvTransReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and D.iInvTag ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and B.iAllocTag ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text += $" and P.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select H.iHeaderId, V.sName VoucherType ,\r\n                                                            sVoucherNo, H.iDate , P.iMasterId, P.sCode, B.fQuantity,Bat.iBatchId,  Bat.sBatchNo, \r\n                                                            B.iBin BinId, mB.sCode Bin,sk.iId SkidId, Sk.sSkidNo\r\n                                                            ,Bat.iMfDate,Bat.iExpiryDate\r\n                                                            from tCore_Header_0 H join tCore_Data_0 D on D.iHeaderId = H.iHeaderId\r\n\r\n                                                            join tCore_Indta_0 I on I.iBodyId = D.iBodyId                                                            \r\n\r\n                                                            join cCore_Vouchers_0 V on V.iVoucherType = H.iVoucherType\r\n\r\n                                                            join mCore_Product P on I.iProduct = P.iMasterId\r\n\r\n                                                            JOIN tCore_Bins_0 B on B.iBodyId =D.iBodyId\r\n\r\n                                                            join mCore_Bins mB on B.iBin=mB.iMasterId\r\n\r\n                                                            join tCore_Skid_0 Sk on Sk.iId =B.iSkidId\r\n\r\n                                                            left join tCore_Batch_0 Bat on Bat.iBodyId = D.iBodyId\r\n\r\n                                                            where P.iMasterid>0 AND H.iDate BETWEEN {1} AND {2} {3} --iBatchId = 965453 and\r\n                                                            --h.iSkidId =51312 and\r\n\r\n                                                            --iProduct = 30365\r\n\r\n                                                           -- ORDER BY H.iDate,sVoucherNo \r\n                                                            ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "H.iCreatedTime, H.iDate,sVoucherNo";
			break;
		case FocusReport.WMSExpiryDateReport:
			FConvert.GetInputValue(oRec.Inputs, 1);
			_ = 0;
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and B.iAllocTag ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("--use Focus80N0\r\n\r\n                                                 --Declare @ProductId int=85\r\n                                                select iProduct iHeaderId, P.sCode ProductCode, P.sName ProductName, sum(A.Qty)Qty, sum(A.ReservedQty)ReservedQty, iExpiryDate , iBatchId BatchId,sBatchNo Batch, iBin BinId,B.sCode Bin, SkidId, sSkidNo Skid \r\n\r\n                                                from (\r\n                                                --Declare @ProductId int=85\r\n                                                select SUM(B.fQuantity) Qty, 0 ReservedQty,  B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag, ISNULL( Bat.iBatchId,0) iBatchId, ISNULL(Bat.sBatchNo,'')sBatchNo\r\n                                                ,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n                                                from tCore_Header_0 H  inner Join       \r\n                                                tCore_Data_0 D on  H.iHeaderId = D.iHeaderId and D.iAuthStatus=1                 \r\n                                                inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId\r\n                                                inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0)\r\n                                                left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId\r\n                                                where (bUpdateStocks=1 ) and H.bCancelled=0 \r\n                                                AND D.bSuspendUpdateStocks <> 1 --and B.iFlag<8 --and B.iFlag!=1 \r\n                                                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) --AND H.iDate BETWEEN {1} AND {2} \r\n                                                {3}                \r\n                                                --and B.iBin = {1}\r\n                                                and Bat.iExpiryDate <= {2}--Given date\r\n                                                --AND T.iProduct=@ProductId\r\n                                                group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,Bat.iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n               \r\n                                                union  all\r\n               \r\n                                                select 0 Qty,  Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty,B.iBin,R.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag,\r\n                                                R.iFullfillTransId iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo from\r\n                                                tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                                                join tCore_Data_0 D on  R.iTransactionId = D.iTransactionId and D.iAuthStatus=1\r\n                                                inner Join  tCore_Header_0 H on  H.iHeaderId = D.iHeaderId \r\n                                                inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0) \r\n                                                left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId    \r\n                                                where R.iProduct>0 {3}  --and B.iFlag<8 --and B.iFlag!=1   \r\n                                                --                    and R.iBin={1}\r\n                                                and Bat.iExpiryDate <= {2}--Given date\r\n                                                --AND T.iProduct=@ProductId\r\n                                                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) --AND H.iDate BETWEEN {1} AND {2}\r\n                                                group by B.iBin,R.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,R.iFullfillTransId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n                                                )A\r\n                                                join mCore_Product P on P.iMasterId=A.iProduct\r\n                                                join mCore_Bins B on B.iMasterId=A.iBin\r\n                                                group by iProduct, P.sCode , P.sName , iExpiryDate , iBatchId ,sBatchNo , iBin , SkidId, sSkidNo,B.sCode \r\n                                                HAVING ( SUM(ISNULL(A.Qty, 0)) > 0  or SUM(ISNULL(A.ReservedQty, 0)) > 0 )\r\n                                                --order by A.qty Desc\r\n\r\n                                                --select * from cCore_vouchers_0", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSCycleCountReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and CCD.iBinId={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and CCD.iUser ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text += $" and CCD.iProductId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text = ((FConvert.GetInputValue(oRec.Inputs, 4) != 1) ? (text + string.Format(" and CCH.iConfirmationStatus = 3 ", FConvert.GetInputValue(oRec.Inputs, 4))) : (text + string.Format(" and CCH.iConfirmationStatus in (0,2) ", FConvert.GetInputValue(oRec.Inputs, 4))));
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select 0 iHeaderId, CCH.sDocumentNo, CCD.iProductId,P.sCode, CCD.iBinId ,B.sCode, sum(CCD.dActualQty) AllocatedQty, sum(CCD.dConfirmedQty) ConfirmedQty\r\n                                    ,CCD.iUser, E.sName, CCD.iBatchId, Bat.sBatchNo \r\n                                    --, CCD.iSkidId,Sk.sSkidNo\r\n                                    ,case CCH.iConfirmationStatus \r\n                                    when 3 then 'Completed' \r\n                                    when 2 then 'FullyConfirmed' \r\n                                    else 'Allocated' end CycleCountStatus\r\n                                    from tWms_CycleCountRequestDetail_0 CCD\r\n                                    join tWms_CycleCountRequest_0 CCH on CCD.iCCRHeaderId=CCH.iCCRHeaderId\r\n                                    join mCore_Product P on P.iMasterId=CCD.iProductId\r\n                                    join tCore_Skid_0 Sk on Sk.iId=CCD.iSkidId\r\n                                    join mCore_Bins B on B.iMasterId=CCD.iBinId\r\n                                    Left Join (Select iBatchId,sBatchNo, sum(fQuantity) fQuantity from tCore_Batch_0 Bat\r\n\t\t\t\t\t\t\t\t\tjoin tCore_Indta_0 I on Bat.iBodyId=I.iBodyId\r\n                                    group by iBatchId,sBatchNo) Bat on Bat.iBatchId=CCD.iBatchId\r\n                                    join mPay_Employee E on E.iMasterId=CCD.iUser\r\n                                    where CCD.iBinId>0 AND CCH.iDate BETWEEN {1} AND {2}  {3}\r\n                                    Group by CCH.sDocumentNo,CCD.iProductId,P.sCode, CCD.iBinId ,B.sCode, CCD.iUser, E.sName,CCH.iConfirmationStatus \r\n                                    ,CCD.iUser, E.sName, CCD.iBatchId, Bat.sBatchNo", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSBinOccupancyReport:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and B.iBin ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
				text2 += $" and B.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text2 += $" and Ar.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text2 += $" and Zo.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 4)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				text2 += $" and R.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 5)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("SELECT B.iMasterId iHeaderId, B.sCode, ISNULL(B.iCapacity,0) iCapacity,\r\n                                                    ISNULL(B.iCapacity,0)- abs(ISNULL(SUM(Qty*vmCore_Product.iBinCapacity),0)) FreeCapacity\r\n\t\t\t\t                                    ,case B.iType\t\t\t\t\r\n\t\t\t\t                                    when 0 then 'Bin'\r\n\t\t\t\t                                    when 1 then 'Floor'\r\n\t\t\t\t                                    when 2 then 'Stage'\r\n\t\t\t\t                                    when 3 then 'Yard'\r\n\t\t\t\t                                    when 4 then 'Mezzanine' end Type,R.sCode Rack, st.sName Storer, abs(ISNULL(SUM(Qty*vmCore_Product.iBinCapacity),0)) UtilizedCapacity, \r\n                                                    0 Utilization\r\n                                                      from (              \r\n                                                   select SUM(B.fQuantity) Qty, B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag, ISNULL( Bat.iBatchId,0) iBatchId, ISNULL(Bat.sBatchNo,'')sBatchNo\r\n                                                   ,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo,B.iAllocTag Storer\r\n                                                   from tCore_Header_0 H  inner Join       \r\n                                                    tCore_Data_0 D on  H.iHeaderId = D.iHeaderId and D.iAuthStatus=1                 \r\n                                                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId\r\n                                                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0)\r\n                                                    left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId\r\n                                                    where (bUpdateStocks=1 ) and H.bCancelled=0 \r\n                                                    AND D.bSuspendUpdateStocks <> 1 --and B.iFlag<8 --and B.iFlag!=1 \r\n                                                    AND H.bSuspended = 0  AND D.iAuthStatus IN(0,1)  AND H.iDate BETWEEN {1} AND {2}     {3}          \r\n                                                     -- and B.iBin = {7}\r\n                                                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,Bat.iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n               \r\n                                                   union  all\r\n               \r\n                                                    select Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) Qty,B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag,\r\n                                                    R.iFullfillTransId iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo,B.iAllocTag Storer from\r\n                                                    tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                                                    join tCore_Data_0 D on  R.iTransactionId = D.iTransactionId and D.iAuthStatus=1\r\n                                                    inner Join  tCore_Header_0 H on  H.iHeaderId = D.iHeaderId \r\n                                                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId  \r\n                                                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0) \r\n                                                    left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId    \r\n                                                    where T.iProduct>0  --and B.iFlag<8 --and B.iFlag!=1   \r\n                                                        --and R.iBin={7}\r\n                                                    AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) AND H.iDate BETWEEN {1} AND {2} {3}\r\n\r\n                                                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,R.iFullfillTransId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n                                                    )A\r\n                                                    right JOIN vmCore_Product on vmCore_Product.iMasterId = A.iProduct and vmCore_Product.iTreeId=0\r\n                                                    right join vmCore_Bins B on B.iMasterId=A.iBin\r\n\t\t\t\t                                    join mWms_Rack R on B.iRack=R.iMasterId \r\n\t\t\t\t\t\t\t\t\t\t\t\t\tjoin muWms_Rack RR on RR.iMasterId=R.iMasterId \r\n                                                    join muWms_Zone Zo on RR.iZone=Zo.iMasterId\r\n                                                    join muWms_Area Ar on Zo.iArea=Ar.iMasterId\r\n                                                    join mWms_Storer St on St.iMasterId = A.Storer\r\n                                                    where B.iMasterId>0 {6}\r\n\r\n                                                    group by B.iMasterId,B.iCapacity, B.sCode,B.iType,R.sCode,st.sName --,vmCore_Product.iBinCapacity\r\n                \r\n                                                      --group by A.iProduct , A.SkidId , A.sSkidNo ,A.iBin , A.invStatusId ,\r\n                                                    --A.iBatchId,A.sBatchNo, A.iMfDate,A.iExpiryDate,A.iFlag,vmCore_Product.iBinCapacity,B.iCapacity,B.iMasterId\r\n                \r\n                                                    --ORDER BY A.iBin", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text2, FConvert.GetInputValue(oRec.Inputs, 2));
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSPendingAllocationsReport:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and E.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select AUD.iAllocatedUserId iHeaderId,E.sName, H.iVoucherType,V.sName Voucher,\r\n                                             sum(dAllocatedQty) PendingQty, \r\n                                             case AUD.iStatus  \r\n                                             when 0 then 'Allocated'\r\n                                             when 2 then 'Suspended'\r\n                                             when 3 then  'Completed' end Status\r\n                                             from tCore_AllocateUserDetails_0 AUD\r\n\r\n                                            join tCore_Header_0 H on H.iHeaderId=AUD.iHeaderId\r\n                                            join cCore_Vouchers_0 V on V.iVoucherType=H.iVoucherType\r\n                                            join mPay_Employee E on AUD.iAllocatedUserId=E.iMasterId\r\n                                            where E.iMasterId>0 AND H.iDate BETWEEN {1} AND {2} and AUD.iStatus in (0,2) {3}\r\n                                            group by AUD.iAllocatedUserId,E.sCode,E.sName, H.iVoucherType,V.sName,AUD.iStatus  \r\n                                            --having sum(AUD.dAllocatedQty)>sum(AUD.dConfirmedQty)", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSInventoryBalReport:
		{
			arrDefaultTables.AddRange("vrCore_Bins,vrWms_Skid,vrCore_Product,vrWms_Storer".Split(','));
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and tCore_Data_0.iInvTag ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and tCore_Bins_0.iAllocTag ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text += $" and tCore_Bins_0.iBin = {FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text += $" AND tCore_Indta_0.iProduct= {FConvert.GetInputValue(oRec.Inputs, 4)} ";
			}
			string text12 = " where  vrCore_Product.iMasterId >0 ";
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				QueryGenerator queryGenerator = new QueryGenerator();
				queryGenerator.m_iCompId = m_iCompanyId;
				text12 += queryGenerator.filter_string(oRec.FilterSource, m_db, m_iCompanyId);
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                    select A.iHeaderId iHeaderId, A.ProductCode, A.ProductName, sum(A.Qty)Qty, sum(A.ReservedQty)ReservedQty, A.iExpiryDate , A.BatchId BatchId,A.Batch Batch, \r\n                    A.BinId BinId,A.Bin Bin, A.Skid, A.sSkidNo Skid \r\n                    ,A.invStatusId ,A.StorerName {5}\r\n                    \r\n                    from (\r\n                    --Declare @ProductId int=85\r\n                    select tCore_Indta_0.iProduct iHeaderId,mCore_Product.sCode ProductCode,mCore_Product.sName ProductName,SUM(tCore_Bins_0.fQuantity) Qty,0 ReservedQty,tCore_Batch_0.iExpiryDate,ISNULL( tCore_Batch_0.iBatchId,0) BatchId, ISNULL(tCore_Batch_0.sBatchNo,'')Batch\r\n\t\t\t\t\t,tCore_Bins_0.iBin BinId,mCore_Bins.sCode Bin,tCore_Bins_0.iSkidId Skid,tCore_Skid_0.sSkidNo,tCore_Bins_0.iAllocTag invStatusId,{8}.sName StorerName , tCore_Bins_0.iAllocTag\r\n                    from tCore_Header_0  inner Join       \r\n                    tCore_Data_0 on  tCore_Header_0.iHeaderId = tCore_Data_0.iHeaderId              \r\n                    inner join tCore_Bins_0  on tCore_Bins_0.iBodyId=tCore_Data_0.iBodyId\r\n                    inner join tCore_Indta_0  on  tCore_Indta_0.iBodyId =tCore_Data_0.iBodyId\r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(tCore_Bins_0.iSkidId,0)\r\n                    join mCore_Product on mCore_Product.iMasterId=tCore_Indta_0.iProduct\r\n\t\t\t\t\tjoin mCore_Bins  on mCore_Bins.iMasterId=tCore_Bins_0.iBin\t\r\n\t\t\t\t\tjoin {8} on {8}.iMasterId=tCore_Bins_0.iAllocTag\r\n                    left join tCore_Batch_0 on  tCore_Batch_0.iBodyId =tCore_Data_0.iBodyId\r\n                    {7}\r\n                     where (bUpdateStocks=1 ) and tCore_Header_0.bCancelled=0 \r\n                    AND tCore_Data_0.bSuspendUpdateStocks <> 1 --and tCore_Bins_0.iFlag<8 --and tCore_Bins_0.iFlag!=1 \r\n                    AND tCore_Header_0.bSuspended = 0 AND tCore_Data_0.iAuthStatus <2 AND tCore_Header_0.iDate BETWEEN {1} AND {2}  {3}               \r\n                    --and B.iBin = {1}\r\n                  --  AND T.iProduct=@ProductId\r\n                     group by tCore_Bins_0.iBin,tCore_Indta_0.iProduct,tCore_Bins_0.iSkidId , tCore_Bins_0.iAllocTag,tCore_Bins_0.iFlag,tCore_Batch_0.iBatchId,tCore_Batch_0.sBatchNo,tCore_Skid_0.sSkidNo,tCore_Batch_0.iExpiryDate,tCore_Batch_0.iMfDate,tCore_Bins_0.iLotNo, tCore_Bins_0.iAllocTag\r\n\t\t\t\t\t,mCore_Product.sCode,mCore_Product.sName,mCore_Bins.sCode,{8}.sName \r\n                    HAVING SUM(tCore_Bins_0.fQuantity) > 0\r\n                    --HAVING SUM(tCore_Indta_0.fQuantity) <> 0 \r\n                    union  all\r\n               \r\n                    select tCore_Indta_0.iProduct iHeaderId,mCore_Product.sCode ProductCode,mCore_Product.sName ProductName,0 Qty,  Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty,\r\n\t\t\t\t\ttCore_Batch_0.iExpiryDate,R.iFullfillTransId iBatchId, ISNULL(tCore_Batch_0.sBatchNo,'')sBatchNo\r\n\t\t\t\t\t,tCore_Bins_0.iBin,mCore_Bins.sCode BinCode,tCore_Bins_0.iSkidId SkidId,tCore_Skid_0.sSkidNo,tCore_Bins_0.iAllocTag invStatusId,{8}.sName StorerName , tCore_Bins_0.iAllocTag from\r\n                    tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                    join tCore_Data_0 on  R.iTransactionId = tCore_Data_0.iTransactionId \r\n                    inner Join  tCore_Header_0 on  tCore_Header_0.iHeaderId = tCore_Data_0.iHeaderId \r\n                    inner join tCore_Bins_0  on tCore_Bins_0.iBodyId=tCore_Data_0.iBodyId\r\n                    inner join tCore_Indta_0  on  tCore_Indta_0.iBodyId =tCore_Data_0.iBodyId  \r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(tCore_Bins_0.iSkidId,0) \r\n                    join mCore_Product on mCore_Product.iMasterId=tCore_Indta_0.iProduct\r\n\t\t\t\t\tjoin mCore_Bins  on mCore_Bins.iMasterId=tCore_Bins_0.iBin\t\r\n\t\t\t\t\tjoin {8} on {8}.iMasterId=tCore_Bins_0.iAllocTag\t\r\n                    join tCore_Batch_0 on  tCore_Batch_0.iBodyId =tCore_Data_0.iBodyId   \r\n                   {7}\r\n                    where tCore_Indta_0.iProduct>0  --and tCore_Bins_0.iFlag<8 --and tCore_Bins_0.iFlag!=1   \r\n                    --                    and R.iBin=132384513\r\n                    --AND tCore_Indta_0.iProduct=@ProductId\r\n                    AND tCore_Header_0.bSuspended = 0 AND tCore_Data_0.iAuthStatus <2 AND tCore_Header_0.iDate BETWEEN {1} AND {2} {3}\r\n                     group by tCore_Bins_0.iBin,tCore_Indta_0.iProduct,tCore_Bins_0.iSkidId , tCore_Bins_0.iAllocTag,tCore_Bins_0.iFlag,R.iFullfillTransId,tCore_Batch_0.sBatchNo,tCore_Skid_0.sSkidNo,tCore_Batch_0.iExpiryDate,tCore_Batch_0.iMfDate,tCore_Bins_0.iLotNo, tCore_Bins_0.iAllocTag\r\n\t\t\t\t\t,mCore_Product.sCode,mCore_Product.sName,mCore_Bins.sCode,{8}.sName \r\n                    having Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) > 0 \r\n                    )A\r\n                    join vrCore_Product on vrCore_Product.iMasterId=A.iHeaderId \r\n                    LEFT JOIN vrCore_Bins ON vrCore_Bins.iMasterId = A.BinId\r\n\t\t\t\t\tLEFT JOIN vrWms_Skid ON vrWms_Skid.iMasterId = A.Skid\r\n                    join vrWms_Storer on vrWms_Storer.iMasterId=A.iAllocTag\r\n\t\t\t\t\t--join tCore_Indta_0 on tCore_Indta_0.iProduct=vrCore_Product.iMasterId\r\n\t\t\t\t\t--join tCore_Data_0 on tCore_Data_0.iBodyId= tCore_Indta_0.iBodyId\r\n\t\t\t\t\t--join tCore_Bins_0 BB on BB.iBin=A.BinId\t\t\t\t\t\r\n\t\t\t\t\t----join mWms_Storer on mWms_Storer.iMasterId=A.invStatusId\t\t\t\t\t\r\n                    --join mCore_Bins  on mCore_Bins.iMasterId=A.BinId\t\r\n{7}                    \r\n{9}\r\n                    group by A.iHeaderId, A.ProductCode, A.ProductName , A.iExpiryDate , A.BatchId ,A.Batch , A.BinId , A.Skid, A.sSkidNo,A.Bin,A.invStatusId,A.StorerName {5}\r\n                    --group by A.iProduct, A.ProductCode, A.ProductName , A.iExpiryDate , A.iBatchId ,A.sBatchNo , A.iBin , A.SkidId, A.sSkidNo,A.BinCode,A.invStatusId,A.StorerName \r\n                    --HAVING ( SUM(ISNULL(A.Qty, 0)) > 0  or SUM(ISNULL(A.ReservedQty, 0)) > 0 )\r\n                    --order by A.qty Desc\r\n                    ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, "--@EXTRA_TABLES", viewName, text12);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		case FocusReport.WMSEmptyLocReport:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text2 += $" and Ar.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text2 += $" and Zo.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 3)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text2 += $" and R.iMasterId ={FConvert.GetInputValue(oRec.Inputs, 4)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n\r\n                Declare @TotLocations int,@EmptyLoc int,@NoOfOccupLoc int\r\n                Declare @PercOfOcc decimal(28,2)\r\n\r\n                --Total locations\r\n                set @TotLocations=(select count(*) from vmCore_bins B join vmWms_Rack R on R.iMasterId=B.iRack\r\n\t\t\t\t join vmWms_Zone Zo on Zo.iMasterId=R.iZone\r\n\t\t\t\t join vmWms_Aisle Al on Al.iMasterId=R.iAisle\r\n\t\t\t\t join vmWms_Area Ar on Ar.iMasterId=Al.iArea\r\n\t\t\t\twhere B.iMasterId>0  {7}\r\n\t\t\t\t )\r\n                --Empty locations\r\n                 select @EmptyLoc= (select count(*) EmptyLocations from (\r\n                select B.iMasterId iHeaderId, B.sCode sCode, B.sName BinName, mB.iCapacity,R.sCode Rack,\r\n                    case mB.iType\r\n                    when 0 then 'Bin'\r\n                    when 1 then 'Floor'\r\n                    when 2 then 'Stage'\r\n                    when 3 then 'Yard'\r\n                    when 4 then 'Mezzanine' end Type\r\n                    from \r\n                    mCore_Bins B join \r\n                    muCore_Bins mB on B.iMasterId=mB.iMasterId\r\n                    join mWms_Rack R on mB.iRack=R.iMasterId \r\n                    left join (\r\n                    --Declare @ProductId int=85\r\n                    select SUM(B.fQuantity) Qty, 0 ReservedQty,  B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag \r\n                    ,tCore_Skid_0.sSkidNo--, Max(Bat.iExpiryDate)iExpiryDate,Max(Bat.iMfDate)iMfDate,B.iLotNo\r\n                    from tCore_Header_0 H  inner Join       \r\n                    tCore_Data_0 D on  H.iHeaderId = D.iHeaderId and D.iAuthStatus <2                 \r\n                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId\r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0)\r\n                    --left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId\r\n                    where (bUpdateStocks=1 ) and H.bCancelled=0 \r\n                    AND D.bSuspendUpdateStocks <> 1 --and B.iFlag<8 --and B.iFlag!=1 \r\n                    AND H.bSuspended = 0  AND D.iAuthStatus IN(0,1)                 \r\n                    --and B.iBin = 132320001\r\n                    --AND T.iProduct=@ProductId\r\n                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,tCore_Skid_0.sSkidNo,B.iLotNo  having SUM(B.fQuantity)>0\r\n               \r\n                    union  all\r\n               \r\n                    select 0 Qty,  Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty,B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag,\r\n                    tCore_Skid_0.sSkidNo--, Max(Bat.iExpiryDate)iExpiryDate, Max(Bat.iMfDate)iMfDate,B.iLotNo \r\n                    from\r\n                    tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                    join tCore_Data_0 D on  R.iTransactionId = D.iTransactionId \r\n                    inner Join  tCore_Header_0 H on  H.iHeaderId = D.iHeaderId \r\n                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId  \r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0) \r\n                    --left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId    \r\n                    where T.iProduct>0  --and B.iFlag<8 --and B.iFlag!=1   \r\n                    --                    and R.iBin=132320001\r\n                    --AND T.iProduct=@ProductId\r\n                    AND H.bSuspended = 0 AND D.iAuthStatus <2\r\n                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,R.iFullfillTransId,tCore_Skid_0.sSkidNo,B.iLotNo having Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) >0\r\n                    )A on B.iMasterId=A.iBin \r\n                    --join mWms_Rack R on mB.iRack=R.iMasterId \r\n\t\t\t\t\tjoin muWms_Rack RR on RR.iMasterId=R.iMasterId \r\n                    join muWms_Zone Zo on RR.iZone=Zo.iMasterId\r\n                    join muWms_Area Ar on Zo.iArea=Ar.iMasterId\r\n                    where A.iBin is null and  B.iMasterId>0 {7}  and B.iStatus!=5 )A)\r\n\r\n                set @NoOfOccupLoc = @TotLocations-@EmptyLoc\r\n\r\n                if(@TotLocations>0)\r\n                    set @PercOfOcc=(100* ( cast(@NoOfOccupLoc as decimal(18,10))/ cast(@TotLocations as decimal(18,10))))\r\n                else\r\n    \t\t\t\tset @PercOfOcc=0\r\n                --select @TotLocations TotLoc,@EmptyLoc EmptyLoc,@NoOfOccupLoc NoOfOccupLoc, @PercOfOcc PercOfOccupancy\r\n\r\n\r\n\r\n            select B.iMasterId iHeaderId, B.sCode sCode, B.sName BinName, mB.iCapacity,R.sCode Rack,\r\n                    case mB.iType\r\n                    when 0 then 'Bin'\r\n                    when 1 then 'Floor'\r\n                    when 2 then 'Stage'\r\n                    when 3 then 'Yard'\r\n                    when 4 then 'Mezzanine' end Type\r\n                    from \r\n                    mCore_Bins B join \r\n                    muCore_Bins mB on B.iMasterId=mB.iMasterId\r\n                    join mWms_Rack R on mB.iRack=R.iMasterId \r\n                    left join (\r\n                    --Declare @ProductId int=85\r\n                    select SUM(B.fQuantity) Qty, 0 ReservedQty,  B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag \r\n                    ,tCore_Skid_0.sSkidNo--, Max(Bat.iExpiryDate)iExpiryDate,Max(Bat.iMfDate)iMfDate,B.iLotNo\r\n                    from tCore_Header_0 H  inner Join       \r\n                    tCore_Data_0 D on  H.iHeaderId = D.iHeaderId               \r\n                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId\r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0)\r\n                    --left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId\r\n                    where (bUpdateStocks=1 ) and H.bCancelled=0 \r\n                    AND D.bSuspendUpdateStocks <> 1 --and B.iFlag<8 --and B.iFlag!=1 \r\n                    AND H.bSuspended = 0  AND D.iAuthStatus <2                 \r\n                    --and B.iBin = {1}\r\n                    --AND T.iProduct=@ProductId\r\n                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,tCore_Skid_0.sSkidNo,B.iLotNo  having SUM(B.fQuantity)>0\r\n               \r\n                    union  all\r\n               \r\n                    select 0 Qty,  Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty,B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag,\r\n                    tCore_Skid_0.sSkidNo--, Max(Bat.iExpiryDate)iExpiryDate, Max(Bat.iMfDate)iMfDate,B.iLotNo \r\n                    from\r\n                    tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                    join tCore_Data_0 D on  R.iTransactionId = D.iTransactionId and D.iAuthStatus=1\r\n                    inner Join  tCore_Header_0 H on  H.iHeaderId = D.iHeaderId \r\n                    inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                    inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId  \r\n                    inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0) \r\n                    --left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId    \r\n                    where T.iProduct>0  --and B.iFlag<8 --and B.iFlag!=1   \r\n                    --                    and R.iBin={1}\r\n                    --AND T.iProduct=@ProductId\r\n                    AND H.bSuspended = 0 AND D.iAuthStatus <2\r\n                    group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,R.iFullfillTransId,tCore_Skid_0.sSkidNo,B.iLotNo having Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) >0\r\n                    )A on B.iMasterId=A.iBin \r\n                    --join mWms_Rack R on mB.iRack=R.iMasterId \r\n\t\t\t\t\tjoin muWms_Rack RR on RR.iMasterId=R.iMasterId \r\n                    join muWms_Zone Zo on RR.iZone=Zo.iMasterId\r\n                    join muWms_Area Ar on Zo.iArea=Ar.iMasterId\r\n                    where A.iBin is null and  B.iMasterId>0 {7} and B.iStatus!=5\r\n\r\n                    union all\r\n\r\n                    select max(iMasterId)+1, 'Total Location', cast(@TotLocations as nvarchar(100)) TotLoc,0, 'Empty Location',cast (@EmptyLoc as nvarchar(100)) EmptyLoc\r\n                    from mCore_Bins \r\n                    --join\r\n                    --muCore_Bins mB on tCore_Bins_0.iBin=mB.iMasterId\r\n                    --join mWms_Rack R on mB.iRack=R.iMasterId \r\n\t\t\t\t\t--join muWms_Rack RR on RR.iMasterId=R.iMasterId \r\n                    --join muWms_Zone Zo on RR.iZone=Zo.iMasterId\r\n                    --join muWms_Area Ar on Zo.iArea=Ar.iMasterId\r\n                    --WHERE tCore_Bins_0.iBin>0 {7}\r\n\r\n                    union all\r\n\r\n                    select max(iMasterId)+2, 'No Of Occupied Loc', cast(@NoOfOccupLoc as nvarchar(100)) NoOfOccupLoc,0, 'Perc Of Occupancy', cast(@PercOfOcc as nvarchar(100)) PercOfOccupancy\r\n                    from mCore_Bins \r\n                    --join\r\n                    --muCore_Bins mB on tCore_Bins_0.iBin=mB.iMasterId\r\n                    --join mWms_Rack R on mB.iRack=R.iMasterId \r\n\t\t\t\t\t--join muWms_Rack RR on RR.iMasterId=R.iMasterId \r\n                    --join muWms_Zone Zo on RR.iZone=Zo.iMasterId\r\n                    --join muWms_Area Ar on Zo.iArea=Ar.iMasterId\r\n                    --WHERE tCore_Bins_0.iBin>0 {7}\r\n                    \r\n", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, text2);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.BillingRepSchedule:
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                                              DECLARE @InputStorer INT = {7};\r\n\r\n                                              -- ensure no leftover temp table from previous failed runs\r\n                                              IF OBJECT_ID('tempdb..#BillingScheduleReportIdList') IS NOT NULL DROP TABLE #BillingScheduleReportIdList;\r\n                                              -- create the temp table explicitly and populate it (avoid SELECT ... INTO which fails if table already exists)\r\n                                                  CREATE TABLE #BillingScheduleReportIdList(iMasterId INT);\r\n\r\n                                                  IF (@InputStorer <= 0)\r\n                                                  BEGIN\r\n                                                     INSERT INTO #BillingScheduleReportIdList (iMasterId)\r\n                                                     \r\n                                                     SELECT DISTINCT iAllocTag FROM tCore_Bins{0} WHERE isnull(iAllocTag,0) > 0;\r\n                                                  END\r\n                                                  ELSE\r\n                                                  BEGIN\r\n                                                     INSERT INTO #BillingScheduleReportIdList (iMasterId) VALUES (@InputStorer);\r\n                                                  END\r\n                                                  IF OBJECT_ID('tempdb..#tblMainCharges') IS NOT NULL DROP TABLE #tblMainCharges;\r\n                                                    CREATE TABLE #tblMainCharges(\r\n                                                        BillHeaderId INT,\r\n                                                        ItemId INT,\r\n                                                        LotNo bigint,\r\n                                                        TotalCBM DECIMAL(18,8),\r\n                                                        TotalWeight DECIMAL(18,8),\r\n                                                        TotalQty DECIMAL(18,8),\r\n                                                        ServiceTypeId INT,\r\n                                                        TotalCharge DECIMAL(18,8),\r\n                                                        ChargeType INT,\r\n                                                        DocumentType INT,\r\n                                                        BillDate INT,\r\n                                                        BinId INT,\r\n                                                        WarehouseId INT,\r\n                                                        BillPattern BIT,\r\n                                                        ShipInQty DECIMAL(18,8),\r\n                                                        TariffId INT,\r\n                                                        Rate DECIMAL(18,8),\r\n                                                        MinCharge DECIMAL(18,8),\r\n                                                        DocumentID INT\r\n                                                    );\r\n                    -- ensure other temp tables are recreated cleanly\r\n                                                  IF OBJECT_ID('tempdb..#tblStoringCHarges1') IS NOT NULL DROP TABLE #tblStoringCHarges1;\r\n                                                  CREATE TABLE #tblStoringCHarges1 ( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot bigint,\r\n                                                    TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                                                    ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                                                    IRate decimal(18,6),RRate decimal(18,6),WhId int,BillEndDate int);\r\n\r\n                                                  IF OBJECT_ID('tempdb..#tblServiceCHarges2') IS NOT NULL DROP TABLE #tblServiceCHarges2;\r\n                                                  CREATE TABLE #tblServiceCHarges2 (\r\n                                                    SKUID Int ,ServiceTypeID Int,LotID bigint,TotalCBM Decimal(18,8),TotalWeight Decimal(18,8),Qty Int,\r\n                                                    Discount Int,ChargeOn Int,ApplyOn Int,TotalCharge Decimal(18,8), ChargeType Int,BillDate int,\r\n                                                    Rate Decimal(18,8),DocumentID Int,DocumentType Varchar(50),ShipInQty Decimal(18,8), WhId Int,bMinCharge bit,Remarks Varchar(200)\r\n                                                    )\r\n\r\n\r\n                                                  IF OBJECT_ID('tempdb..#tblStoringCHarges3') IS NOT NULL DROP TABLE #tblStoringCHarges3;\r\n                                                  CREATE TABLE #tblStoringCHarges3( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot bigint,\r\n                                                    TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                                                    ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                                                    IRate decimal(18,6),RRate decimal(18,6),PalletId int ,WhId int,BillEndDate int);\r\n\r\n                                                   DECLARE @StorerId INT;\r\n                                                  -- iterate using COUNT()\r\n                                                  WHILE (SELECT COUNT(*) FROM #BillingScheduleReportIdList) > 0\r\n                                                    BEGIN\r\n                                                    -- pick top one storer\r\n                                                    SELECT TOP (1) @StorerId = iMasterId\r\n                                                    FROM #BillingScheduleReportIdList \r\n                                                    ORDER BY iMasterId;\r\n\r\n                                                      -- pass the current @StorerId into the stored procedures so they compute for each storer\r\n                                                      insert into #tblStoringCHarges1\r\n                                                          exec dbo.pWms_GetStoringCharges_StorerWise_0 0,132777489,@StorerId,0,0,0;\r\n\r\n                                                      insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, Rate, TotalCBM, BillDate)--, WarehouseId)\r\n                                                          select SKU, Lot, ChargeType, TotalQuantity,\r\n                                                                 TotalCharge+RecuringCHarges,\r\n                                                                 Weight, Rate, CBM, BillDate--, WhId\r\n                                                          from #tblStoringCHarges1;\r\n\r\n                                                      insert into #tblServiceCHarges2\r\n                                                          exec pWms_GetServicesCharges_CBMQtyWeight_0 0,132777489,@StorerId;\r\n\r\n                                                      insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, ServiceTypeId, Rate, TotalCBM, BillDate, DocumentID, DocumentType, ShipInQty, WarehouseId, MinCharge)\r\n                                                          select SKUID, LotID, ChargeType, Qty, TotalCharge, TotalWeight, ServiceTypeID, Rate, TotalCBM, BillDate, DocumentID, DocumentType, ShipInQty, WhId, bMinCharge\r\n                                                          from #tblServiceCHarges2;\r\n\r\n                                                      insert into #tblStoringCHarges3\r\n                                                          exec pWms_GetStoringCharges_PalletWise_0 0,132777489,@StorerId,0,0;\r\n\r\n                                                      insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, Rate, TotalCBM, BillDate)--, WarehouseId)\r\n                                                          select SKU,Lot,ChargeType,TotalQuantity,TotalCharge+RecuringCHarges,Weight,Rate,CBM,BillDate\r\n                                                          from #tblStoringCHarges3;\r\n\r\n\r\n\r\n                                                      -- remove the processed storer (delete a single matching row)\r\n                                                           DELETE FROM #BillingScheduleReportIdList WHERE iMasterId = @StorerId;\r\n\r\n\r\n                                                  END\r\n\r\n                                                  -- cleanup\r\n                                                  IF OBJECT_ID('tempdb..#tblStoringCHarges1') IS NOT NULL DROP TABLE #tblStoringCHarges1;\r\n                                                  IF OBJECT_ID('tempdb..#tblServiceCHarges2') IS NOT NULL DROP TABLE #tblServiceCHarges2;\r\n                                                  IF OBJECT_ID('tempdb..#tblStoringCHarges3') IS NOT NULL DROP TABLE #tblStoringCHarges3;\r\n                                                  IF OBJECT_ID('tempdb..#BillingScheduleReportIdList') IS NOT NULL DROP TABLE #BillingScheduleReportIdList;\r\n\r\n                                                    select MC.BillHeaderId iHeaderId,MC.ItemId,P.sCode, isnull(MC.ServiceTypeId,0)ServiceTypeId,Ser.sCode ServiceCode,MC.ChargeType, sum(MC.TotalQty)TotalQty, \r\n                                                    MC.Rate Rate, sum(MC.TotalCharge)TotalCharge,0 LotNo, sum(MC.TotalWeight)TotalWeight,MC.DocumentType, V.sName VoucherName,\r\n                                                    sum(MC.TotalCBM)TotalCBM,MC.BillDate, 0 DocumentID from #tblMainCharges MC\r\n                                                    join mCore_Product P on P.iMasterId=MC.ItemId\r\n                                                    join vmCore_Product Ser on ISNULL(MC.ServiceTypeId,0)=Ser.iMasterId\r\n                                                    left join cCore_Vouchers_0 V on V.iVoucherType=MC.DocumentType\r\n                                                    group by MC.BillHeaderId ,MC.ItemId,P.sCode,MC.ServiceTypeId,Ser.sCode ,MC.ChargeType,MC.Rate,MC.DocumentType, V.sName,MC.BillDate\r\n\r\n                                                    --drop table #tblMainCharges ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 2), flag ? 1 : 0);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSBillingForecast:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and W.iMasterId={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text += $" and P.iMasterId={FConvert.GetInputValue(oRec.Inputs, 4)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format(" \r\n                    IF OBJECT_ID('tempdb..#tblMainCharges') IS NOT NULL DROP TABLE #tblMainCharges \r\n                    Create table #tblMainCharges(\r\n                    BillHeaderId int ,\r\n                            ItemId int, \r\n                            LotNo int ,\r\n                            TotalCBM Decimal(18,8) ,\r\n                            TotalWeight Decimal(18,8) ,\r\n                            TotalQty Decimal(18,8) ,\r\n                            ServiceTypeId int ,\r\n                            TotalCharge Decimal(18,8), \r\n                            ChargeType int ,\r\n                            DocumentType int, \r\n                            BillDate int ,\r\n                            BinId int ,\r\n                            WarehouseId int ,\r\n                            BillPattern bit,\r\n                            ShipInQty Decimal(18,8) ,\r\n                            TariffId int ,\r\n                            Rate Decimal(18,8) ,\r\n                            MinCharge Decimal(18,8) ,\r\n                            DocumentID int \r\n                )\r\n\r\n\r\n                Create Table #tblStoringCHarges1 ( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot Varchar(50),\r\n                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                IRate decimal(18,6),RRate decimal(18,6),WhId int)\r\n                insert into #tblStoringCHarges1\r\n                exec pWms_GetStoringCharges_StorerWiseForecast {1},{2},{7},0,{8} \r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType  , TotalQty  ,TotalCharge ,TotalWeight, Rate,TotalCBM,BillDate,WarehouseId)\r\n                select SKU,Lot,ChargeType,TotalQuantity,case when TotalCharge>0 then TotalCharge else RecuringCHarges end TotalCharge,Weight,Rate,CBM,BillDate,WhId from #tblStoringCHarges1\r\n\r\n                --select * from  #tblStoringCHarges1\r\n                --drop table #tblStoringCHarges1\r\n\r\n\r\n\r\n                --Create table #tblServiceCHarges2 (\r\n                --SKUID Int ,ServiceTypeID Int,LotID Int,TotalCBM Decimal(18,8),TotalWeight Decimal(18,8),Qty Int,\r\n                --Discount Int,ChargeOn Int,ApplyOn Int,TotalCharge Decimal(18,8), ChargeType Int,BillDate int,\r\n                --Rate Decimal(18,8),DocumentID Int,DocumentType Varchar(50),ShipInQty Decimal(18,8), WhId Int,bMinCharge bit,Remarks Varchar(200)\r\n                --)\r\n                --insert into #tblServiceCHarges2\r\n                --exec pWms_GetServicesCharges_CBMQtyWeight_0 132317441,132383004,6 \r\n\r\n                --insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType,TotalQty, TotalCharge,TotalWeight ,ServiceTypeId  , Rate  ,TotalCBM, BillDate,  DocumentID, \r\n                --DocumentType ,ShipInQty  ,WarehouseId  ,MinCharge)\r\n                --select SKUID,LotID,ChargeType,Qty,TotalCharge,TotalWeight,ServiceTypeID,Rate,TotalCBM,BillDate,DocumentID,DocumentType,ShipInQty,WhId,bMinCharge from #tblServiceCHarges2\r\n                ----select * from #tblServiceCHarges2\r\n                ----drop table #tblServiceCHarges2\r\n\r\n\r\n   \r\n                Create Table #tblStoringCHarges3( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot Varchar(50),\r\n                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                IRate decimal(18,6),RRate decimal(18,6),PalletId int ,WhId int)\r\n\r\n                insert into #tblStoringCHarges3\r\n                exec pWms_GetStoringCharges_PalletWiseForecast {1},{2},{7},{8} \r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType  , TotalQty  ,TotalCharge ,TotalWeight, Rate,TotalCBM,BillDate,WarehouseId)\r\n                select SKU,Lot,ChargeType,TotalQuantity,case when TotalCharge>0 then TotalCharge else RecuringCHarges end TotalCharge,Weight,Rate,CBM,BillDate,WhId from #tblStoringCHarges3\r\n\r\n                --select * from #tblStoringCHarges3\r\n                --drop table #tblStoringCHarges3\r\n\r\n\r\n                drop table #tblStoringCHarges1\r\n                --drop table #tblServiceCHarges2\r\n                drop table #tblStoringCHarges3\r\n\r\n                --select * from #tblMainCharges\r\n\r\n                select 0 iHeaderId, P.iMasterId ItemId,P.sCode,P.sName,0 ServiceTypeId,'' ServiceCode,MC.ChargeType, sum(MC.TotalQty)TotalQty, \r\n\t\t\t\tMC.Rate Rate, sum(MC.TotalCharge)TotalCharge,0 LotNo, sum(MC.TotalWeight)TotalWeight,-- V.sName VoucherName,\r\n                sum(MC.TotalCBM)TotalCBM,MC.BillDate,MC.WarehouseId,W.sCode \r\n\t\t\t\tfrom #tblMainCharges MC\r\n                join vrCore_Product P on P.iMasterId=MC.ItemId\r\n\t\t\t\tjoin mCore_Warehouse W on W.iMasterId=MC.WarehouseId\r\n\t\t\t\t--join vmCore_Product Ser on ISNULL(MC.ServiceTypeId,0)=Ser.iMasterId\r\n\t\t\t\t--join cCore_Vouchers_0 V on V.iVoucherType=MC.DocumentType\r\n                where P.iMasterId>0 {3}\r\n\t\t\t\tgroup by MC.BillHeaderId ,MC.ItemId,P.sCode,MC.ChargeType,MC.Rate,MC.DocumentType--, V.sName\r\n\t\t\t\t,MC.BillDate, P.iMasterId,MC.WarehouseId,W.sCode,P.sName\r\n\r\n                --drop table #tblMainCharges  ORDER BY iHeaderId", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 2), FConvert.GetInputValue(oRec.Inputs, 3));
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSStoringServiceCharge:
		{
			string text5 = string.Empty;
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and W.iMasterId={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				string masterUnderGroup = new QueryGenerator().GetMasterUnderGroup(FConvert.GetInputValue(oRec.Inputs, 3), 2, m_db, m_iCompanyId);
				masterUnderGroup = masterUnderGroup.Replace("(", string.Empty).Replace(")", string.Empty);
				text5 = $"inner join string_split('{masterUnderGroup}',',') TreeRestriction on isnull(P.iMasterId,0) = cast(TreeRestriction.value as int)";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                          DECLARE @InputStorer INT = {7};\r\n\r\n                          -- ensure no leftover temp table from previous failed runs\r\n                          IF OBJECT_ID('tempdb..#StorerIdList') IS NOT NULL DROP TABLE #StorerIdList;\r\n\r\n                              -- create the temp table explicitly and populate it (avoid SELECT ... INTO which fails if table already exists)\r\n                              CREATE TABLE #StorerIdList(iMasterId INT);\r\n\r\n                              IF (@InputStorer <= 0)\r\n                              BEGIN\r\n                                 INSERT INTO #StorerIdList (iMasterId)\r\n                                 --SELECT DISTINCT iMasterId FROM mWms_Storer WHERE iMasterId IS NOT NULL;\r\n                                 SELECT DISTINCT iAllocTag FROM tCore_Bins{0} WHERE isnull(iAllocTag,0) > 0;\r\n                              END\r\n                              ELSE\r\n                              BEGIN\r\n                                 INSERT INTO #StorerIdList (iMasterId) VALUES (@InputStorer);\r\n                              END\r\n                              IF OBJECT_ID('tempdb..#tblMainCharges') IS NOT NULL DROP TABLE #tblMainCharges;\r\n                              CREATE TABLE #tblMainCharges(\r\n                                  BillHeaderId int ,\r\n                                  ItemId int, \r\n                                  LotNo bigint ,\r\n                                  TotalCBM Decimal(18,8) ,\r\n                                  TotalWeight Decimal(18,8) ,\r\n                                  TotalQty Decimal(18,8) ,\r\n                                  ServiceTypeId int ,\r\n                                  TotalCharge Decimal(18,8), \r\n                                  ChargeType int ,\r\n                                  DocumentType int, \r\n                                  BillDate int ,\r\n                                  BinId int ,\r\n                                  WarehouseId int ,\r\n                                  BillPattern bit,\r\n                                  ShipInQty Decimal(18,8) ,\r\n                                  TariffId int ,\r\n                                  Rate Decimal(18,8) ,\r\n                                  MinCharge Decimal(18,8) ,\r\n                                  DocumentID int \r\n                              )\r\n\r\n                              -- ensure other temp tables are recreated cleanly\r\n                              IF OBJECT_ID('tempdb..#tblStoringCHarges1') IS NOT NULL DROP TABLE #tblStoringCHarges1;\r\n                              CREATE TABLE #tblStoringCHarges1 ( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot bigint,\r\n                                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                                IRate decimal(18,6),RRate decimal(18,6),WhId int,BillEndDate int);\r\n\r\n                              IF OBJECT_ID('tempdb..#tblServiceCHarges2') IS NOT NULL DROP TABLE #tblServiceCHarges2;\r\n                              CREATE TABLE #tblServiceCHarges2 (\r\n                                SKUID Int ,ServiceTypeID Int,LotID bigint,TotalCBM Decimal(18,8),TotalWeight Decimal(18,8),Qty Int,\r\n                                Discount Int,ChargeOn Int,ApplyOn Int,TotalCharge Decimal(18,8), ChargeType Int,BillDate int,\r\n                                Rate Decimal(18,8),DocumentID Int,DocumentType Varchar(50),ShipInQty Decimal(18,8), WhId Int,bMinCharge bit,Remarks Varchar(200)\r\n                                )\r\n\r\n                              IF OBJECT_ID('tempdb..#tblStoringCHarges3') IS NOT NULL DROP TABLE #tblStoringCHarges3;\r\n                              CREATE TABLE #tblStoringCHarges3( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot bigint,\r\n                                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                                IRate decimal(18,6),RRate decimal(18,6),PalletId int ,WhId int,BillEndDate int);\r\n\r\n                               DECLARE @StorerId INT;\r\n                              -- iterate using COUNT()\r\n                              WHILE (SELECT COUNT(*) FROM #StorerIdList) > 0\r\n                                BEGIN\r\n                                -- pick top one storer\r\n                                SELECT TOP (1) @StorerId = iMasterId\r\n                                FROM #StorerIdList \r\n                                ORDER BY iMasterId;\r\n\r\n                                  -- pass the current @StorerId into the stored procedures so they compute for each storer\r\n                                  insert into #tblStoringCHarges1\r\n                                      exec dbo.pWms_GetStoringCharges_StorerWise_0 {1},{2},@StorerId,0,0,{9};\r\n\r\n                                  insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, Rate, TotalCBM, BillDate, WarehouseId)\r\n                                      select SKU, Lot, ChargeType, TotalQuantity,\r\n                                             case when TotalCharge > 0 then TotalCharge else RecuringCHarges end TotalCharge,\r\n                                             Weight, Rate, CBM, BillDate, WhId\r\n                                      from #tblStoringCHarges1;\r\n\r\n                                  insert into #tblServiceCHarges2\r\n                                      exec pWms_GetServicesCharges_CBMQtyWeight_0 {1},{2},@StorerId;\r\n\r\n                                  insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, ServiceTypeId, Rate, TotalCBM, BillDate, DocumentID, DocumentType, ShipInQty, WarehouseId, MinCharge)\r\n                                      select SKUID, LotID, ChargeType, Qty, TotalCharge, TotalWeight, ServiceTypeID, Rate, TotalCBM, BillDate, DocumentID, DocumentType, ShipInQty, WhId, bMinCharge\r\n                                      from #tblServiceCHarges2;\r\n\r\n                                  insert into #tblStoringCHarges3\r\n                                      exec pWms_GetStoringCharges_PalletWise_0 {1},{2},@StorerId,0,{9};\r\n\r\n                                  insert into #tblMainCharges(ItemId, LotNo, ChargeType, TotalQty, TotalCharge, TotalWeight, Rate, TotalCBM, BillDate, WarehouseId)\r\n                                      select SKU, Lot, ChargeType, TotalQuantity,\r\n                                             case when TotalCharge > 0 then TotalCharge else RecuringCHarges end TotalCharge,\r\n                                             Weight, Rate, CBM, BillDate, WhId\r\n                                      from #tblStoringCHarges3;\r\n\r\n                                  -- remove the processed storer (delete a single matching row)\r\n                                       DELETE FROM #StorerIdList WHERE iMasterId = @StorerId;\r\n                              END\r\n\r\n                              -- cleanup\r\n                              IF OBJECT_ID('tempdb..#tblStoringCHarges1') IS NOT NULL DROP TABLE #tblStoringCHarges1;\r\n                              IF OBJECT_ID('tempdb..#tblServiceCHarges2') IS NOT NULL DROP TABLE #tblServiceCHarges2;\r\n                              IF OBJECT_ID('tempdb..#tblStoringCHarges3') IS NOT NULL DROP TABLE #tblStoringCHarges3;\r\n                              IF OBJECT_ID('tempdb..#StorerIdList') IS NOT NULL DROP TABLE #StorerIdList;\r\n\r\n                            select 0 iHeaderId, P.iMasterId ItemId,P.sCode,P.sName,Ser.iMasterId ServiceTypeId,Ser.sCode ServiceCode,MC.ChargeType, sum(MC.TotalQty)TotalQty, \r\n                            MC.Rate Rate, sum(MC.TotalCharge)TotalCharge,0 LotNo, sum(MC.TotalWeight)TotalWeight,-- V.sName VoucherName,\r\n                            sum(MC.TotalCBM)TotalCBM,MC.BillDate,MC.WarehouseId,W.sCode \r\n                            from #tblMainCharges MC\r\n                            join vrCore_Product P on P.iMasterId=MC.ItemId and P.iTreeId = 0\r\n                            join mCore_Warehouse W on W.iMasterId=MC.WarehouseId\r\n                            join vmCore_Product Ser on ISNULL(MC.ServiceTypeId,0)=Ser.iMasterId and Ser.iTreeId = 0\r\n                            {10}\r\n                            --join cCore_Vouchers_0 V on V.iVoucherType=MC.DocumentType\r\n                            where P.iMasterId>=0 {3}\r\n                            group by MC.BillHeaderId ,MC.ItemId,P.sCode,MC.ChargeType,MC.Rate,MC.DocumentType--, V.sName\r\n                            ,MC.BillDate, P.iMasterId,MC.WarehouseId,W.sCode,P.sName,Ser.iMasterId,Ser.sCode\r\n\r\n                                --drop table #tblMainCharges  ORDER BY iHeaderId", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 2), FConvert.GetInputValue(oRec.Inputs, 3), flag ? 1 : 0, text5);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		case FocusReport.WMSShortDateReport:
			FConvert.GetInputValue(oRec.Inputs, 1);
			_ = 0;
			if (FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text += $" and B.iAllocTag ={FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				oRec.StartingDate = Convert.ToInt32(new Date(oRec.EndingDate, CurrentData.get().CompanyCalendar).Add(FConvert.GetInputValue(oRec.Inputs, 3), DateAdd.Days).Value);
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				oRec.EndingDate = Convert.ToInt32(new Date(oRec.StartingDate, CurrentData.get().CompanyCalendar).Add(FConvert.GetInputValue(oRec.Inputs, 4), DateAdd.Days).Value);
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("--use Focus80N0\r\n                                                Declare @TodayDate int= (select dbo.datetoint((select GETDATE())))\r\n                                                 --Declare @ProductId int=85\r\n                                                select  StorerId ,ST.sCode StorerCode, ST.sName StorerName, iProduct iHeaderId, P.sCode ProductCode, P.sName ProductName,U.sName BaseUnit, sum(A.Qty) BaseQty,\r\n                                                0 QtyInCase,0 QtyInPcs, sum(A.ReservedQty)ReservedQty,\r\n                                                iExpiryDate , iBatchId BatchId,sBatchNo Batch, iBin BinId,B.sCode Bin, SkidId, sSkidNo Skid \r\n                                                ,(DateDiff(dd,dbo.IntToGregDate(@TodayDate), dbo.IntToGregDate(iExpiryDate)))  NoOfDays\r\n                                                {5}\r\n                                                from (\r\n                                                --Declare @ProductId int=85\r\n                                                select  SUM(B.fQuantity) Qty, 0 ReservedQty,  B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag StorerId,B.iFlag, ISNULL( Bat.iBatchId,0) iBatchId, ISNULL(Bat.sBatchNo,'')sBatchNo\r\n                                                ,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n                                                from tCore_Header_0 H  inner Join       \r\n                                                tCore_Data_0 D on  H.iHeaderId = D.iHeaderId and D.iAuthStatus=1                 \r\n                                                inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId\r\n                                                inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0)\r\n                                                left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId\r\n                                                where (bUpdateStocks=1) and H.bCancelled=0 \r\n                                                AND D.bSuspendUpdateStocks <> 1 --and B.iFlag<8 --and B.iFlag!=1 \r\n                                                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) --AND H.iDate BETWEEN {1} AND {2}\r\n                                                {3}                \r\n                                                --and B.iBin = {1}\r\n                                                and Bat.iExpiryDate<={2}--Given date\r\n                                                --AND T.iProduct=@ProductId\r\n                                                group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,Bat.iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n               \r\n                                                union  all\r\n               \r\n                                                select 0 Qty,  Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty,B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag StorerId,B.iFlag,\r\n                                                R.iFullfillTransId iBatchId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo from\r\n                                                tCore_ReservedStock_0 R  with (ReadUncommitted) \r\n                                                join tCore_Data_0 D on  R.iTransactionId = D.iTransactionId and D.iAuthStatus=1\r\n                                                inner Join  tCore_Header_0 H on  H.iHeaderId = D.iHeaderId \r\n                                                inner join tCore_Bins_0 B  on B.iBodyId=D.iBodyId\r\n                                                inner join tCore_Indta_0 T on  T.iBodyId =D.iBodyId  \r\n                                                inner join tCore_Skid_0 with (ReadUncommitted) on tCore_Skid_0.iId= isnull(B.iSkidId,0) \r\n                                                left join tCore_Batch_0 Bat on  Bat.iBodyId =D.iBodyId    \r\n                                                where T.iProduct>0 {3}  --and B.iFlag<8 --and B.iFlag!=1   \r\n                                                --                    and R.iBin={1}\r\n                                                and Bat.iExpiryDate<={2}--Given date\r\n                                                --AND T.iProduct=@ProductId\r\n                                                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) --AND H.iDate BETWEEN {1} AND {2}\r\n                                                group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,R.iFullfillTransId,Bat.sBatchNo,tCore_Skid_0.sSkidNo,Bat.iExpiryDate,Bat.iMfDate,B.iLotNo\r\n                                                )A\r\n                                                join mCore_Product P on P.iMasterId=A.iProduct\r\n                                                join muCore_Product_Units PU on PU.iMasterId=P.iMasterId\r\n                                                join mCore_Units U on U.iMasterId=PU.iDefaultBaseUnit\r\n                                                join mCore_Bins B on B.iMasterId=A.iBin\r\n                                                join {7} ST on ST.iMasterId=A.StorerId\r\n                                                group by iProduct, P.sCode , P.sName , iExpiryDate , iBatchId ,sBatchNo , iBin , SkidId, sSkidNo,B.sCode ,U.sName,StorerId ,ST.sCode ,ST.sName \r\n                                                HAVING ( SUM(ISNULL(A.Qty, 0)) > 0  or SUM(ISNULL(A.ReservedQty, 0)) > 0 )\r\n                                                --order by A.qty Desc\r\n\r\n                                                --select * from cCore_vouchers_0", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, viewName);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSPalletInPalletOutRpt:
		{
			FConvert.GetInputValue(oRec.Inputs, 1);
			_ = 0;
			FConvert.GetInputValue(oRec.Inputs, 2);
			_ = 0;
			bool flag3 = Convert.ToBoolean(FConvert.GetInputValue(oRec.Inputs, 2));
			int num3 = ((FConvert.GetInputValue(oRec.Inputs, 1) > 0) ? FConvert.GetInputValue(oRec.Inputs, 1) : 0);
			if (flag3)
			{
				standardQuery = new StandardQuery();
				standardQuery.Query = string.Format("--use Focus80N0\r\n                    Declare @Storer int={7}, @FDate int,@TDate int\r\n                    Set @FDate ={1} --jan 01st\r\n                    Set @TDate = {2}\r\n\r\n\t\t\t\t\t--drop table TempTable\r\n                    IF OBJECT_ID('tempdb..#TempTable') IS NOT NULL DROP TABLE #TempTable \r\n\t\t\t\t\tselect * into #TempTable\r\n\t\t\t\t\tfrom dbo.[fnWMS_GetBinCountTable] (@Storer,@FDate,@TDate)\r\n\t\t\t\t\tselect iHeaderId, Date,DocumentNo,Reference_No,DocumentType,PalletIn,PalletOut,NoOfPallet,\r\n\t\t\t\t\t--dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(Date)))) BinCount  ,  A.StorerName\r\n\t\t\t\t\t#TempTable.BinCount BinCount\r\n\t\t\t\t\tfrom (\r\n\r\n                    Select  0 iHeaderId,(@FDate) [Date],'' DocumentNo,'' Reference_No,'' DocumentType,0 PalletIn,0 PalletOut, dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,-1, dbo.inttodate(@FDate)))) NoOfPallet, '' StorerName\r\n                                        \r\n\r\n                    union                     \r\n                                        \r\n                    select distinct a.iHeaderId,  (iDate) [Date] ,sAbbr+':'+sVoucherNo DocumentNo,isnull(R.Reference_No,'')Reference_No, g.sName DocumentType ,Count(iSkidId) PalletIn,0 PalletOut ,0 NoOfPallet, S.sName StorerName--dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet  \r\n                    --,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate)))) BinCount \r\n                                        from tCore_Header_0 a join \r\n                                        tCore_Data_0 b on a.iHeaderId =b.iHeaderId \r\n\t\t\t\t\t--join tCore_Indta_0 c on c.iBodyId = b.iBodyId join tCore_Batch_0 d on d.iBodyId = b.iBodyId \r\n\t\t\t\t\tjoin tCore_Bins_0 e on e.iBodyId = b.iBodyId \r\n\t\t\t\t\t--join tCore_Skid_0 f on f.iId =e.iSkidId \r\n\t\t\t\t\tjoin cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType join \r\n                    tCore_HeaderData9216_0 R on R.iHeaderId=a.iHeaderId\r\n                    left join mWms_Storer S on S.iMasterId = e.iAllocTag\r\n                    where a.iVoucherType =9216 and iDate >=@FDate and  iDate <=@TDate\r\n                    and e.iAllocTag=(case when @Storer =0 then e.iAllocTag else @Storer end)\r\n                    group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,g.sName  ,iDate,a.iHeaderId,R.Reference_No, S.sName \r\n                                        --order by iDate\r\n                    union all\r\n                    select distinct a.iHeaderId,  (iDate) [Date],sAbbr+':'+sVoucherNo DocumentNo,'' Reference_No,g.sName DocumentType ,0 PalletIn,Count(iSkidId) PalletOut,0 NoOfPallet, S.sName StorerName  --,dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet \r\n                    --,iDate --,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate))))BinCount \r\n                    from tCore_Header_0 a join \r\n                    tCore_Data_0 b on a.iHeaderId =b.iHeaderId \r\n\t\t\t\t\t--join tCore_Indta_0 c on c.iBodyId = b.iBodyId join tCore_Batch_0 d on d.iBodyId = b.iBodyId \r\n\t\t\t\t\tjoin tCore_Bins_0 e on e.iBodyId = b.iBodyId \r\n\t\t\t\t\t--join tCore_Skid_0 f on f.iId =e.iSkidId \r\n\t\t\t\t\tjoin cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType\r\n                    left join mWms_Storer S on S.iMasterId = e.iAllocTag\r\n                    where a.iVoucherType in (6144,11776) and iDate >=@FDate and  iDate <=@TDate\r\n                    and e.iAllocTag=(case when @Storer =0 then e.iAllocTag else @Storer end)\r\n                    group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,g.sName ,iDate,a.iHeaderId , S.sName  --ORDER BY iDate\r\n                                        )A   \r\n\t\t\t\t\t\t\t\t\t\tjoin #TempTable on #TempTable.iDate=A.Date\r\n\t\t\t\t\t\t\t\t\t\t--ORDER BY A.Date\r\n\r\n\t\t\t\t\t\t\t\t\t\t--drop table #TempTable\r\n  ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, num3);
				standardQuery.PrimaryColumn = "Date";
			}
			else
			{
				standardQuery = new StandardQuery();
				standardQuery.Query = string.Format("--use Focus80N0\r\n                    Declare @Storer int={7}, @FDate int,@TDate int\r\n                    Set @FDate ={1} --jan 01st\r\n                    Set @TDate = {2}\r\n\r\n\t\t\t\t\t--drop table TempTable\r\n                    --IF OBJECT_ID('tempdb..#TempTable') IS NOT NULL DROP TABLE #TempTable \r\n\t\t\t\t\t--select * into #TempTable\r\n\t\t\t\t\t--from dbo.[fnWMS_GetBinCountTable] (@Storer,@FDate,@TDate)\r\n\r\n\t\t\t\t\tselect iHeaderId, Date,DocumentNo,Reference_No,DocumentType,PalletIn,PalletOut,NoOfPallet,\r\n\t\t\t\t\t--dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(Date)))) BinCount  \r\n\t\t\t\t\t--#TempTable.BinCount BinCount\r\n                    0 BinCount,  A.StorerName\r\n\t\t\t\t\tfrom (\r\n\r\n                    Select  0 iHeaderId,(@FDate) [Date],'' DocumentNo,'' Reference_No,'' DocumentType,0 PalletIn,0 PalletOut, dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,-1, dbo.inttodate(@FDate)))) NoOfPallet, '' StorerName\r\n                                        \r\n\r\n                    union                     \r\n                                        \r\n                    select distinct a.iHeaderId,  (iDate) [Date] ,sAbbr+':'+sVoucherNo DocumentNo,isnull(R.Reference_No,'')Reference_No, g.sName DocumentType ,Count(iSkidId) PalletIn,0 PalletOut ,0 NoOfPallet, S.sName StorerName--dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet  \r\n                    --,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate)))) BinCount \r\n                                        from tCore_Header_0 a join \r\n                                        tCore_Data_0 b on a.iHeaderId =b.iHeaderId \r\n\t\t\t\t\t--join tCore_Indta_0 c on c.iBodyId = b.iBodyId join tCore_Batch_0 d on d.iBodyId = b.iBodyId \r\n\t\t\t\t\tjoin tCore_Bins_0 e on e.iBodyId = b.iBodyId \r\n\t\t\t\t\t--join tCore_Skid_0 f on f.iId =e.iSkidId \r\n\t\t\t\t\tjoin cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType join \r\n                    tCore_HeaderData9216_0 R on R.iHeaderId=a.iHeaderId\r\n                    left join mWms_Storer S on S.iMasterId = e.iAllocTag                    \r\n                    where a.iVoucherType =9216 and iDate >=@FDate and  iDate <=@TDate\r\n                    and e.iAllocTag = (case when @Storer =0 then e.iAllocTag else @Storer end)\r\n                    group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,g.sName  ,iDate,a.iHeaderId,R.Reference_No, S.sName\r\n                                        --order by iDate\r\n                    union all\r\n                    select distinct a.iHeaderId,  (iDate) [Date],sAbbr+':'+sVoucherNo DocumentNo,isnull(R.Reference_No,'') Reference_No,g.sName DocumentType ,0 PalletIn,Count(iSkidId) PalletOut,0 NoOfPallet, S.sName StorerName--,dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet   \r\n                    --,iDate --,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate))))BinCount \r\n                    from tCore_Header_0 a join \r\n                    tCore_Data_0 b on a.iHeaderId =b.iHeaderId \r\n\t\t\t\t\t--join tCore_Indta_0 c on c.iBodyId = b.iBodyId join tCore_Batch_0 d on d.iBodyId = b.iBodyId \r\n\t\t\t\t\tjoin tCore_Bins_0 e on e.iBodyId = b.iBodyId \r\n\t\t\t\t\t--join tCore_Skid_0 f on f.iId =e.iSkidId \r\n\t\t\t\t\tjoin cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType\r\n                    left join tCore_HeaderData11776_0 R on R.iHeaderId = a.iHeaderId\r\n\t\t\t\t\tleft join tCore_HeaderData6144_0 R1 on R1.iHeaderId = a.iHeaderId\r\n                    left join mWms_Storer S on S.iMasterId = e.iAllocTag                    \r\n                    where a.iVoucherType in (6144,11776) and iDate >=@FDate and  iDate <=@TDate\r\n                    and e.iAllocTag = (case when @Storer = 0 then e.iAllocTag else @Storer end)\r\n                    group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,g.sName ,iDate,a.iHeaderId, S.sName,R.Reference_No  --ORDER BY iDate\r\n                                        )A   \r\n\t\t\t\t\t\t\t\t\t\t--join #TempTable on #TempTable.iDate=A.Date\r\n\t\t\t\t\t\t\t\t\t\t--ORDER BY A.Date\r\n\r\n\t\t\t\t\t\t\t\t\t\t--drop table #TempTable\r\n  ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, num3);
				standardQuery.PrimaryColumn = "Date";
			}
			break;
		}
		case FocusReport.WMSEstimatedBillingReport:
			FConvert.GetInputValue(oRec.Inputs, 1);
			_ = 0;
			FConvert.GetInputValue(oRec.Inputs, 2);
			_ = 0;
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("--use Focus80N0\r\n                    Declare @Storer int={7}, @FDate int,@TDate int\r\n                    Set @FDate ={1}\r\n                    Set @TDate = {2}\r\n\r\n    IF OBJECT_ID('tempdb..#tblMainCharges') IS NOT NULL DROP TABLE #tblMainCharges \r\n                    Create table #tblMainCharges(\r\n                    BillHeaderId int ,\r\n                            ItemId int, \r\n                            LotNo int ,\r\n                            TotalCBM Decimal(18,8) ,\r\n                            TotalWeight Decimal(18,8) ,\r\n                            TotalQty Decimal(18,8) ,\r\n                            ServiceTypeId int ,\r\n                            TotalCharge Decimal(18,8), \r\n                            ChargeType nvarchar(60) ,\r\n                            DocumentType int, \r\n                            BillDate int ,\r\n                            BinId int ,\r\n                            WarehouseId int ,\r\n                            BillPattern bit,\r\n                            ShipInQty Decimal(18,8) ,\r\n                            TariffId int ,\r\n                            Rate Decimal(18,8) ,\r\n                            MinCharge Decimal(18,8) ,\r\n                            DocumentID int \r\n                )\r\n\t\t\t\tIF OBJECT_ID('tempdb..#tblStoringCHarges1') IS NOT NULL DROP TABLE #tblStoringCHarges1 \r\nCreate Table #tblStoringCHarges1 ( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot Varchar(50),\r\n                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                IRate decimal(18,6),RRate decimal(18,6),WhId int, BillEndDate int)\r\n                insert into #tblStoringCHarges1\r\n                exec pWms_GetStoringCharges_StorerWise_0 @FDate,@TDate,@Storer,0,0,{8}\r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType  , TotalQty  ,TotalCharge ,TotalWeight, Rate,TotalCBM,BillDate,WarehouseId)\r\n                select SKU,Lot,ChargeType,TotalQuantity,case when TotalCharge>0 then TotalCharge else RecuringCHarges end TotalCharge,Weight,Rate,CBM,BillDate,WhId from #tblStoringCHarges1\r\n\r\n                --select * from  #tblStoringCHarges1\r\n                --drop table #tblStoringCHarges1\r\n\r\n\r\n\t\t\t\tIF OBJECT_ID('tempdb..#tblServiceCHarges2') IS NOT NULL DROP TABLE #tblServiceCHarges2 \r\n                Create table #tblServiceCHarges2 (\r\n                SKUID Int ,ServiceTypeID Int,LotID Int,TotalCBM Decimal(18,8),TotalWeight Decimal(18,8),Qty Int,\r\n                Discount Int,ChargeOn Int,ApplyOn Int,TotalCharge Decimal(18,8), ChargeType Int,BillDate int,\r\n                Rate Decimal(18,8),DocumentID Int,DocumentType Varchar(50),ShipInQty Decimal(18,8), WhId Int,bMinCharge bit,Remarks Varchar(200)\r\n                )\r\n                insert into #tblServiceCHarges2\r\n                exec pWms_GetServicesCharges_CBMQtyWeight_0 @FDate,@TDate,@Storer \r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType,TotalQty, TotalCharge,TotalWeight ,ServiceTypeId  , Rate  ,TotalCBM, BillDate,  DocumentID, \r\n                DocumentType ,ShipInQty  ,WarehouseId  ,MinCharge)\r\n                select SKUID,LotID,ChargeType,Qty,TotalCharge,TotalWeight,ServiceTypeID,Rate,TotalCBM,BillDate,DocumentID,DocumentType,ShipInQty,WhId,bMinCharge from #tblServiceCHarges2\r\n                ----select * from #tblServiceCHarges2\r\n                ----drop table #tblServiceCHarges2\r\n\r\n\r\n   IF OBJECT_ID('tempdb..#tblStoringCHarges3') IS NOT NULL DROP TABLE #tblStoringCHarges3 \r\n                Create Table #tblStoringCHarges3( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot Varchar(50),\r\n                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                IRate decimal(18,6),RRate decimal(18,6),PalletId int ,WhId int, BillEndDate int)\r\n\r\n                insert into #tblStoringCHarges3\r\n                exec pWms_GetStoringCharges_PalletWise_0 @FDate,@TDate,@Storer,0,{8}\r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType  , TotalQty  ,TotalCharge ,TotalWeight, Rate,TotalCBM,BillDate,WarehouseId)\r\n                select SKU,Lot,ChargeType,TotalQuantity,case when TotalCharge>0 then TotalCharge else RecuringCHarges end TotalCharge,Weight,Rate,CBM,BillDate,WhId from #tblStoringCHarges3\r\n\r\n                --select * from #tblStoringCHarges3\r\n                --drop table #tblStoringCHarges3\r\n\r\n\r\n\t\t\t\tIF OBJECT_ID('tempdb..#tblStoringCHarges4') IS NOT NULL DROP TABLE #tblStoringCHarges4 \r\n                Create Table #tblStoringCHarges4( BillDate int, Storer Varchar(50), SKU Varchar(50), Lot Varchar(50),\r\n                TotalQuantity decimal(18,6), TotalCharge decimal(18,6), RecuringCHarges decimal(18,6),  \r\n                ChargeType Varchar(20),  ChargeOn int,[Weight] decimal(18,6),Rate decimal(18,6), CBM decimal(18,6) default 1,SKUQty decimal(18,6),  \r\n                IRate decimal(18,6),RRate decimal(18,6),PalletId int ,WhId int, BillEndDate int, BinId int)\r\n\t\t\t\tinsert into #tblStoringCHarges4\r\n                exec pWms_GetStoringCharges_BinWiseEstimated @FDate,@TDate,@Storer\r\n\r\n\t\t\t\tinsert into #tblStoringCHarges4\r\n                exec pWms_GetStoringCharges_BinWise_DayWiseEstimated @FDate,@TDate,@Storer\r\n\r\n                insert into #tblMainCharges(ItemId ,LotNo  ,ChargeType  , TotalQty  ,TotalCharge ,TotalWeight, Rate,TotalCBM,BillDate,WarehouseId)\r\n                select SKU,Lot,ChargeType,TotalQuantity,case when TotalCharge>0 then TotalCharge else RecuringCHarges end TotalCharge,Weight,Rate,CBM,BillDate,WhId from #tblStoringCHarges4\r\n\r\n\r\n                drop table #tblStoringCHarges1\r\n                drop table #tblServiceCHarges2\r\n                drop table #tblStoringCHarges3\r\n\t\t\t\tdrop table #tblStoringCHarges4\r\n\r\n                    \r\n\t\t\t\tselect * from (\r\n\r\n                Select  0 iHeaderId,(@FDate) [Date],'' DocumentNo,'' Reference_No,'' DocumentType,0 PalletIn,0 PalletOut, dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,-1, dbo.inttodate(@FDate)))) NoOfPallet\r\n                ,dbo.fnWMS_GetBinCount(@Storer,@FDate) BinCount,  0 TotalCasesOut, 0 TotalCasesIn, 0 QtyCBM, 0 Rate, 0 TotalCharges\r\n\r\n                union \r\n                    \r\n\t\t\t\t\t\r\n\t\t\t\tselect distinct a.iHeaderId,  (iDate) [Date] ,sAbbr+':'+sVoucherNo DocumentNo,isnull(R.Reference_No,'')Reference_No, sName DocumentType ,Count(iSkidId) PalletIn,0 PalletOut ,dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet  \r\n                ,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate)))) BinCount ,  0 TotalCasesOut, 0 TotalCasesIn, 0 QtyCBM, 0 Rate, 0 TotalCharges\r\n                from tCore_Header_0 a join \r\n                tCore_Data_0 b on a.iHeaderId =b.iHeaderId join\r\n                tCore_Indta_0 c on c.iBodyId = b.iBodyId join\r\n                tCore_Batch_0 d on d.iBodyId = b.iBodyId join\r\n                tCore_Bins_0 e on e.iBodyId = b.iBodyId join\r\n                tCore_Skid_0 f on f.iId =e.iSkidId join\r\n                cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType join \r\n                tCore_HeaderData9216_0 R on R.iHeaderId=a.iHeaderId\r\n                where a.iVoucherType =9216 and iDate >=@FDate and  iDate <=@TDate\r\n                and e.iAllocTag=@Storer\r\n                group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,sName  ,iDate,a.iHeaderId,R.Reference_No\r\n\t\t\t\t--order by iDate\r\n                union all\r\n                select distinct a.iHeaderId,  (iDate) [Date],sAbbr+':'+sVoucherNo DocumentNo,'' Reference_No,sName DocumentType ,0 PalletIn,Count(iSkidId) PalletOut,dbo.fn_GetPalletOpeningStock(@Storer,dbo.DateToInt(DATEADD(dd,0, dbo.inttodate(iDate)))) NoOfPallet   \r\n                ,dbo.fnWMS_GetBinCount(@Storer,dbo.DateToInt(DATEADD(dd,1, dbo.inttodate(iDate))))BinCount ,  0 TotalCasesOut, 0 TotalCasesIn, 0 QtyCBM, 0 Rate, 0 TotalCharges\r\n                from tCore_Header_0 a join \r\n                tCore_Data_0 b on a.iHeaderId =b.iHeaderId join\r\n                tCore_Indta_0 c on c.iBodyId = b.iBodyId join\r\n                tCore_Batch_0 d on d.iBodyId = b.iBodyId join\r\n                tCore_Bins_0 e on e.iBodyId = b.iBodyId join\r\n                tCore_Skid_0 f on f.iId =e.iSkidId join\r\n                cCore_Vouchers_0 g on g.iVoucherType =a.iVoucherType\r\n                where a.iVoucherType in (6144,11776) and iDate >=@FDate and  iDate <=@TDate\r\n                and e.iAllocTag=@Storer\r\n                group by dbo.IntToDate(iDate),sAbbr,sVoucherNo,sName ,iDate,a.iHeaderId  --ORDER BY iDate\r\n\t\t\t\t)A   --ORDER BY Date\r\n--go\r\n\r\n\t\t\t\tunion all\r\n\r\n\t\t\t\tselect 0 iHeaderId, @TDate ,'','', '', 0 TotalQty, \r\n\t\t\t\t0 Rate, 0 TotalCharge, 0 TotalWeight,-- V.sName VoucherName,\r\n                0 ,0 , 0 QtyCBM, 0 Rate, 0 TotalCharges\r\n\r\n\t\t\t\tunion all \r\n\r\n\t\t\t\tselect 0 iHeaderId, @TDate ,'Billing Details','', '', 0 TotalQty, \r\n\t\t\t\t0 Rate, 0 TotalCharge, 0 TotalWeight,-- V.sName VoucherName,\r\n                0 ,0 , 0 QtyCBM, 0 Rate, 0 TotalCharges\r\n\t\t\t\t\r\n\t\t\t\tunion all \r\n\t\t\t\t\r\n                select 0 iHeaderId, @TDate ,'',case when Ser.sName='' then 'Storing Charges' else Ser.sName end, case MC.ChargeType collate SQL_Latin1_General_CP1_CI_AS\t\r\n                when 0 then 'CBM' \r\n                when 1 then 'Quantity'\r\n                when 2 then 'Weight'\r\n                when 3 then 'Pallet'\r\n                when 5 then 'Bin'\r\n                when 4 then 'Hours'\r\n                when 6 then 'Fix Type' end ChargeType\r\n\t\t\t\t, 0 , 0 , 0 \r\n\t\t\t\t,0,0,0 \r\n\t\t\t\t, sum(MC.TotalQty)TotalQty, \r\n\t\t\t\tsum(MC.TotalCharge)/sum(MC.TotalQty) Rate, sum(MC.TotalCharge)TotalCharge\r\n\t\t\t\t\r\n                --,sum(MC.TotalWeight)TotalWeight,-- V.sName VoucherName,\r\n                --sum(MC.TotalCBM)TotalCBM,0 \r\n\t\t\t\tfrom #tblMainCharges MC\r\n                left join vrCore_Product P on P.iMasterId=MC.ItemId\r\n\t\t\t\tleft join mCore_Warehouse W on W.iMasterId=MC.WarehouseId\r\n\t\t\t\tleft join vmCore_Product Ser on ISNULL(MC.ServiceTypeId,0)=Ser.iMasterId\r\n\t\t\t\t--join cCore_Vouchers_0 V on V.iVoucherType=MC.DocumentType\r\n                --where P.iMasterId>0  --and W.iMasterId=11 \r\n\t\t\t\tgroup by MC.ChargeType,Ser.sName  ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 1), flag ? 1 : 0);
			standardQuery.PrimaryColumn = "Date";
			break;
		case FocusReport.RTSRegister:
		{
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" AND  vrWms_Storer.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			string empty = string.Empty;
			string empty2 = string.Empty;
			string text4 = string.Empty;
			string commandText = string.Format("select Count(*) from sys.columns where object_id in \r\n                                                        (select object_id from sys.tables where name ='tCore_HeaderData{1}_{0}') and name ='Reference_No'", m_sSuffix, 9216);
			if (Convert.ToInt32(m_db.ExecuteScalar(CommandType.Text, commandText)) > 0)
			{
				empty = " vCore_TranHeaderData{0}.Reference_No ReferenceNo ";
				empty2 = " header.Reference_No ";
				text4 = "OR LEN(header.Reference_No) > 0";
			}
			else
			{
				empty = " '' ReferenceNo ";
				empty2 = " '' Reference_No ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format(" SELECT tCore_Header{0}.iHeaderId HeaderId ,tCore_Header{0}.sVoucherNo VoucherNo ,tCore_Header{0}.iDate Date ,vrWms_Storer.sCode StorerCode ,vrWms_Storer.sName StorerName ,\r\n                                                        {6} ,vrCore_Product.sCode ProductCode,vrCore_Product.sName ProductName ,vtCore_Bins{0}.sSkidNo SkidNo ,\r\n                                                        vrCore_Product.iDefaultBaseUnit DefaultBaseUnit ,tCore_Indta{0}.fQuantityInBase QuantityInBase ,\r\n                                                        vrCore_Bins.sName BinName ,tCore_Batch{0}.sBatchNo BatchNo ,tCore_Batch{0}.iExpiryDate BatchExpiryDate ,tCore_Batch{0}.iMfDate MfDate ,\r\n                                                        vCore_TranHeaderData{0}.sBOE TransHeaderDate ,vCore_TranHeaderData{0}.sContainerNo ContainerNo ,vCore_TranHeaderData{0}.sBOL TransHeaderBOL ,\r\n                                                        vtCore_LinksData{0}.sLinkVoucherNo LinkVoucherNo ,vCore_TranHeaderData{0}.iBOEDate TransBOEDate  \r\n                                                        FROM tCore_Header{0} JOIN tCore_Data{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId     \r\n                                                        JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType  \r\n                                                        JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                                                        JOIN (SELECT 0[iTreeId],mCore_Product.iMasterId,mCore_Product.sName,mCore_Product.sCode,mCore_Units.sName[iDefaultBaseUnit],mCore_Units.sCode[iDefaultBaseUnitsCode] \r\n                                                                        FROM mCore_Product  LEFT JOIN muCore_Product_Units  ON muCore_Product_Units.iMasterId = mCore_Product.iMasterId \r\n                                                                        LEFT JOIN mCore_Units  ON mCore_Units.iMasterId = muCore_Product_Units.iDefaultBaseUnit  WHERE mCore_Product.iStatus < 5) \r\n                                                                vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct  \r\n                                                        LEFT JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                                                        LEFT JOIN (SELECT 0[iTreeId],mWms_Storer.iMasterId,mWms_Storer.sName,mWms_Storer.sCode FROM mWms_Storer   WHERE mWms_Storer.iStatus < 5)\r\n                                                                vrWms_Storer ON vrWms_Storer.iMasterId = tCore_Data_Tags{0}.iTag1006 \r\n                                                        LEFT JOIN    (SELECT header.iHeaderId,header.iBOEDate,{7}   ,header.sBOE,header.sBOL,header.sContainerNo \r\n                                                                                FROM tCore_HeaderData{5}{0} header WITH (READUNCOMMITTED)  WHERE LEN(header.iBOEDate) > 0 {8} OR LEN(header.sBOE) > 0 OR LEN(header.sBOL) > 0 OR LEN(header.sContainerNo) > 0  )\r\n                                                                                vCore_TranHeaderData{0} ON vCore_TranHeaderData{0}.iHeaderId = tCore_Header{0}.iHeaderId \r\n                                                        LEFT JOIN vtCore_Bins{0} ON vtCore_Bins{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                                                        LEFT JOIN tCore_Bins{0} tags12 ON tags12.iBodyId = tCore_Data{0}.iBodyId   \r\n                                                        LEFT JOIN vrCore_Bins ON vrCore_Bins.iMasterId = tags12.iBin \r\n                                                        LEFT JOIN tCore_Batch{0} ON tCore_Batch{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                                                        LEFT JOIN vtCore_LinksData{0} ON vtCore_LinksData{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                                                        WHERE tCore_Header{0}.iDate BETWEEN {1} AND {2} AND ( tCore_Header{0}.bSuspended = 0) {3}  AND( tCore_Header{0}.iVoucherType = {5})\r\n                                                    ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), 9216, empty, empty2, text4);
			standardQuery.PrimaryColumn = "HeaderId";
			break;
		}
		case FocusReport.StockDetailsSummary:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and vrWms_Storer.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 1)}";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("SELECT vrCore_Product.iMasterId Masterid, vrWms_Storer.sCode StorerCode, vrWms_Storer.sName StorerName, \r\n                                SUM(QuantityInBase) QuantityInBase, vrCore_Product.sCode ProductCode, vrCore_Product.sName ProductName, vrCore_Product.iCategory ProductCategory,\r\n\t\t\t\t\t\t\t\tvrCore_Units.sName UnitName, \r\n\t\t\t\t\t\t\t\ttCore_Batch{0}.iExpiryDate BatchExpiryDate \r\n                                FROM(SELECT tCore_Data{0}.iBodyId[iBodyId], tCore_Indta{0}.iProduct, SUM(tCore_Indta{0}.fQuantityInBase) QuantityInBase  FROM tCore_Header{0}  \r\n\t\t\t\t\t\t\t    JOIN tCore_Data{0}  ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId   JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t\t\t\t\t\t\tWHERE tCore_Header{0}.iDate BETWEEN {1} AND {2} AND(tCore_Header{0}.bSuspended = 0) AND((tCore_Header{0}.bUpdateStocks = 1 AND tCore_Data{0}.bSuspendUpdateStocks<> 1))  \r\n\t\t\t\t\t\t\t\tGROUP BY tCore_Data{0}.iBodyId, tCore_Indta{0}.iProduct) InnerQry JOIN tCore_Data{0} ON InnerQry.iBodyId = tCore_Data{0}.iBodyId JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId                    \r\n                                JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId JOIN(SELECT 0[iTreeId], mCore_Product.iMasterId, mCore_Product.sName, mCore_Product.sCode, mPos_Category.sName[iCategory], mPos_Category.sCode[iCategorysCode] FROM mCore_Product  LEFT JOIN muCore_Product \r\n                                ON muCore_Product.iMasterId = mCore_Product.iMasterId LEFT JOIN mPos_Category  ON mPos_Category.iMasterId = muCore_Product.iCategory  WHERE mCore_Product.iStatus < 5) vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct  LEFT JOIN tCore_Bins{0} tags12 ON tags12.iBodyId = tCore_Data{0}.iBodyId \r\n                                LEFT JOIN vrCore_Bins ON vrCore_Bins.iMasterId = tags12.iBin LEFT JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n\t\t\t\t\t\t\t\tLEFT JOIN(SELECT 0[iTreeId],mWms_Storer.iMasterId,mWms_Storer.sName,mWms_Storer.sCode FROM mWms_Storer WHERE mWms_Storer.iStatus < 5)vrWms_Storer \r\n\t\t\t\t\t\t\t\tON vrWms_Storer.iMasterId = tCore_Data_Tags{0}.iTag1006 LEFT JOIN vrCore_Units WITH(READUNCOMMITTED) \r\n                                ON vrCore_Units.iMasterId = tCore_Indta{0}.iUnit  LEFT JOIN tCore_Batch{0} ON tCore_Batch{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n\t\t\t\t\t\t\t\tWHERE tCore_Header{0}.iDate BETWEEN {1} AND {2} AND(tCore_Header{0}.bSuspended = 0) \r\n                                AND ((tCore_Header{0}.bUpdateStocks = 1 AND tCore_Data{0}.bSuspendUpdateStocks<> 1)){3} GROUP BY vrCore_Product.iMasterId,vrCore_Product.sCode \r\n                                ,vrWms_Storer.sCode ,vrWms_Storer.sName ,vrCore_Product.sCode ,vrCore_Product.sName ,vrCore_Product.iCategory ,vrCore_Units.sName ,\r\n\t\t\t\t\t\t\t\ttCore_Batch{0}.iExpiryDate", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 1));
			standardQuery.PrimaryColumn = "Masterid";
			break;
		case FocusReport.BalanceStockWithStatus:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and muWms_Storer.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 1)}";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n\t\t\t\t\tselect iMasterId,BinName,StorerCode,StorerName,ProdCode ProductCode,ProdName ProductName,SkidNo,BaseUnit,BaseQty,\r\n                    BatchNo,MfDate,ExpDate,iCategoryName,ReservedQty,Flag,ExpiredStatus from(\r\n\r\n                   select  B.sName BinName, \r\n                    invStatus.sCode StorerCode,invStatus.sName StorerName,P.sCode ProdCode,P.sName ProdName,A.sSkidNo SkidNo, U.sCode BaseUnit,ISNULL(SUM(Qty),0)  BaseQty,\r\n                    A.sBatchNo BatchNo,ISNULL(A.iMfDate,0) MfDate,ISNULL(A.iExpiryDate,0) ExpDate, \r\n                    p. iCategoryName, \r\n                   sum(A.ReservedQty)ReservedQty,\r\n                    case \r\n                    when A.iFlag=0 then 'Available'  \r\n                    when A.iFlag=1 then 'Inspection' \r\n                    when A.iFlag=2 then 'Hold' \r\n                    when A.iFlag=4 then 'Damaged' else 'Cyclecount' end Flag\r\n                    ,case when A.iExpiryDate< dbo.DateToInt(GETDATE()) then 'Expired' else '' end [ExpiredStatus],P.iMasterId\r\n                from (\r\n                \r\n               select SUM(B.fQuantity) Qty, B.iBin,T.iProduct,B.iSkidId SkidId, B.iAllocTag invStatusId,B.iFlag, ISNULL( Bat.iBatchId,0) iBatchId, ISNULL(Bat.sBatchNo,'')sBatchNo\r\n               ,tCore_Skid{0}.sSkidNo,Max(Bat.iExpiryDate)iExpiryDate,Max(Bat.iMfDate)iMfDate, (IsNull(T.mRate,0)) Rate,B.iLotNo, 0 ReservedQty\r\n               from tCore_Header{0} H  inner Join       \r\n                tCore_Data{0} D  on  H.iHeaderId = D.iHeaderId and D.iAuthStatus=1                 \r\n                inner join tCore_Bins{0} B   on B.iBodyId=D.iBodyId\r\n                inner join tCore_Indta{0} T  on  T.iBodyId =D.iBodyId\r\n                inner join tCore_Skid{0} with (ReadUncommitted) on tCore_Skid{0}.iId= isnull(B.iSkidId,0)\r\n                left join tCore_Batch{0} Bat  on  Bat.iBodyId =D.iBodyId\r\n                where bUpdateStocks=1 and H.bCancelled=0 \r\n                AND D.bSuspendUpdateStocks <> 1 and B.iFlag<8\r\n                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1)  and H.iDate BETWEEN {1} AND {2}\r\n                group by B.iBin,T.iProduct,B.iSkidId , B.iAllocTag,B.iFlag,Bat.iBatchId,Bat.sBatchNo,tCore_Skid{0}.sSkidNo,T.mRate,B.iLotNo\r\n               \r\n               union  all\r\n               \r\n                select 0 Qty,R.iBin,T.iProduct,R.iSkidId SkidId, R.iAllocStatusId invStatusId,R.iFlag,\r\n                R.iFullfillTransId iBatchId,Bat.sBatchNo,tCore_Skid{0}.sSkidNo,Max(Bat.iExpiryDate)iExpiryDate, Max(Bat.iMfDate)iMfDate, 0 Rate,R.iLotNo,Sum((Case When bReserveOrRelease =0 then R.fQuantity else -R.fQuantity end )) ReservedQty from\r\n                tCore_ReservedStock{0} R  with (ReadUncommitted) \r\n                join tCore_Data{0} D  on  R.iTransactionId = D.iTransactionId and D.iAuthStatus=1\r\n                inner Join  tCore_Header{0} H  on  H.iHeaderId = D.iHeaderId \r\n               inner join tCore_Indta{0} T  on  T.iBodyId =D.iBodyId  \r\n                inner join tCore_Skid{0} with (ReadUncommitted) on tCore_Skid{0}.iId= isnull(R.iSkidId,0) \r\n                left join tCore_Batch{0} Bat  on  Bat.iBodyId =D.iBodyId    \r\n\t\t\t\tjoin tCore_Bins{0} b On b.iBodyId = D.iBodyId\r\n                where T.iProduct>0 \r\n                AND H.bSuspended = 0 AND D.iAuthStatus IN(0,1) and H.iDate BETWEEN {1} AND {2}\r\n                         \r\n                group by R.iBin,T.iProduct,R.iSkidId , R.iAllocStatusId,R.iFlag,R.iFullfillTransId,Bat.sBatchNo,tCore_Skid{0}.sSkidNo,R.iLotNo\r\n                )A\r\n                inner join vmCore_Product P  on P.iMasterId=A.iProduct\r\n                inner join mCore_Bins B on A.iBin=B.iMasterId\r\n                inner join mWMS_Storer invStatus with (ReadUncommitted) on invStatus.iMasterId=ISNULL(A.invStatusId,0)\r\n                inner join muWMS_Storer on muWMS_Storer.iMasterId=invStatus.iMasterId\r\n                --inner join mWms_Strategy ST with (ReadUncommitted)  on ST.iId=muWMS_Storer.iStrategy \r\n                join mCore_Units U on U.iMasterId=p.iDefaultBaseUnit\r\n                JOIN muCore_Bins ON muCore_Bins.iMasterId = B.iMasterId\r\n                where P.iMasterId>0 {3}\r\n                group by A.iProduct,P.iMasterId , P.sName , A.SkidId , A.sSkidNo ,A.iBin , B.sCode ,A.invStatusId , invStatus.sCode,\r\n                    A.iBatchId,A.sBatchNo, A.iMfDate,A.iExpiryDate,A.iFlag,A.iLotNo,B.sName,invStatus.sName ,P.sCode,U.sCode,p.iCategoryName                                \r\n                having ISNULL(SUM(Qty),0)>0 )sub", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text);
			standardQuery.PrimaryColumn = "iMasterId";
			break;
		case FocusReport.DispatchDetails:
		{
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and tags12.iAllocTag= {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			string text6 = string.Format(" Select top 1 LinkVoucherId from vmCore_Links_{0}  where BaseVoucherId&{2} = {1}", FConvert.GetYearId(m_iCompanyId), 11264, 65280);
			int num = Convert.ToInt32(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text6) : text6));
			text6 = string.Format("select Count(*) from sys.columns a Join\r\n                                          sys.tables b On b.object_id = a.object_id where b.name ='tCore_Data{1}{0}' and a.name = 'sBoxNo'", m_sSuffix, num);
			int num2 = Convert.ToInt32(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text6) : text6));
			standardQuery = new StandardQuery();
			if (num2 > 0)
			{
				standardQuery.Query = string.Format("  SELECT  tCore_Data{0}.iBodyId BodyId ,tCore_Header{0}.sVoucherNo VoucherNo, tCore_Header{0}.iDate Date,\r\n                    vrCore_Product.sCode ItemCode ,vrCore_Product.sName ItemName ,\r\n                    vtCore_Bins{0}.sSkidNo SkidNo, vrCore_Product.iDefaultBaseUnit DefaultBaseUnit, tCore_Indta{0}.fQuantityInBase Qunatityinbase,\r\n                    vrCore_Bins.sName BinName ,tCore_Batch{0}.sBatchNo BatchNo, tCore_Batch{0}.iExpiryDate ExpiryDate, tCore_Batch{0}.iMfDate MfDate,\r\n                     vCore_TranBodyData_0.sBoxNo BoxNo,vCore_TranBodyData_0.sCarton Carton ,vCore_TranBodyData_0.sPackingSkid PackingSkid, vCore_TranHeaderData_0.SalesOrderNo\r\n                    FROM tCore_Header{0} JOIN tCore_Data{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId     \r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType  \r\n                    JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    JOIN (SELECT 0[iTreeId],mCore_Product.iMasterId,mCore_Product.sName,mCore_Product.sCode,mCore_Units.sName[iDefaultBaseUnit],mCore_Units.sCode[iDefaultBaseUnitsCode] FROM mCore_Product  \r\n                    LEFT JOIN muCore_Product_Units  ON muCore_Product_Units.iMasterId = mCore_Product.iMasterId \r\n                    LEFT JOIN mCore_Units  ON mCore_Units.iMasterId = muCore_Product_Units.iDefaultBaseUnit  WHERE mCore_Product.iStatus < 5) vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct  \r\n                    LEFT JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    LEFT JOIN (SELECT BodyData.iBodyId,BodyData.sBoxNo,BodyData.sCarton,BodyData.sPackingSkid FROM tCore_Data{4}_0 BodyData WITH (READUNCOMMITTED))\r\n                    vCore_TranBodyData{0} ON vCore_TranBodyData{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t\t\t\tLEFT JOIN (SELECT HeaderData.SalesOrderNo,HeaderData.iHeaderId FROM tCore_HeaderData{4}_0 HeaderData WITH (READUNCOMMITTED))\r\n                    vCore_TranHeaderData{0} ON vCore_TranHeaderData{0}.iHeaderId = tCore_Header{0}.iHeaderId \r\n                    LEFT JOIN vtCore_Bins{0} ON vtCore_Bins{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                    LEFT JOIN tCore_Bins{0} tags12 ON tags12.iBodyId = tCore_Data{0}.iBodyId   \r\n                    LEFT JOIN vrCore_Bins ON vrCore_Bins.iMasterId = tags12.iBin \r\n                    LEFT JOIN tCore_Batch{0} ON tCore_Batch{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    WHERE ( tCore_Header{0}.bSuspended = 0) AND (tCore_Header{0}.iVoucherType = {4})  and tCore_Header{0}.iDate BETWEEN {1} AND {2} {3}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, num);
				standardQuery.PrimaryColumn = "BodyId";
			}
			else
			{
				standardQuery.Query = string.Format("SELECT  tCore_Data{0}.iBodyId BodyId ,tCore_Header{0}.sVoucherNo VoucherNo, tCore_Header{0}.iDate Date,\r\n                    vrCore_Product.sCode ItemCode ,vrCore_Product.sName ItemName ,\r\n                    vtCore_Bins{0}.sSkidNo SkidNo, vrCore_Product.iDefaultBaseUnit DefaultBaseUnit, tCore_Indta{0}.fQuantityInBase Qunatityinbase,\r\n                    vrCore_Bins.sName BinName ,tCore_Batch{0}.sBatchNo BatchNo, tCore_Batch{0}.iExpiryDate ExpiryDate, tCore_Batch{0}.iMfDate MfDate,\r\n                    '' BoxNo,'' Carton , '' PackingSkid, '' SalesOrderNo\r\n                    FROM tCore_Header{0} JOIN tCore_Data{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId     \r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType  \r\n                    JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    JOIN (SELECT 0[iTreeId],mCore_Product.iMasterId,mCore_Product.sName,mCore_Product.sCode,mCore_Units.sName[iDefaultBaseUnit],mCore_Units.sCode[iDefaultBaseUnitsCode] FROM mCore_Product  \r\n                    LEFT JOIN muCore_Product_Units  ON muCore_Product_Units.iMasterId = mCore_Product.iMasterId \r\n                    LEFT JOIN mCore_Units  ON mCore_Units.iMasterId = muCore_Product_Units.iDefaultBaseUnit  WHERE mCore_Product.iStatus < 5) vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct  \r\n                    LEFT JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    --LEFT JOIN (SELECT BodyData.iBodyId,BodyData.sCarton FROM tCore_Data{4}{0} BodyData WITH (READUNCOMMITTED))\r\n                    --vCore_TranBodyData{0} ON vCore_TranBodyData{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                    LEFT JOIN vtCore_Bins{0} ON vtCore_Bins{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                    LEFT JOIN tCore_Bins{0} tags12 ON tags12.iBodyId = tCore_Data{0}.iBodyId   \r\n                    LEFT JOIN vrCore_Bins ON vrCore_Bins.iMasterId = tags12.iBin \r\n                    LEFT JOIN tCore_Batch{0} ON tCore_Batch{0}.iBodyId = tCore_Data{0}.iBodyId  \r\n                    WHERE ( tCore_Header{0}.bSuspended = 0) AND (tCore_Header{0}.iVoucherType = {4})  and tCore_Header{0}.iDate BETWEEN {1} AND {2} {3}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, num);
				standardQuery.PrimaryColumn = "BodyId";
			}
			break;
		}
		case FocusReport.WMSBillingSummaryWeekWise:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			FConvert.GetInputValue(oRec.Inputs, 2);
			_ = 0;
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                   \r\n                                --Getting number of fridays\r\n\t\t\t\t\t\t\t\t\tDeclare @NoOfweeks int=0\r\n\t\t\t\t\t\t\t\t\tdeclare @dt1 datetime=dbo.inttodate({1})\r\n\t\t\t\t\t\t\t\t\tdeclare @dt2 datetime=dbo.inttodate({2})\r\n\t\t\t\t\t\t\t\t\tDeclare @cnt int,@dt as datetime\r\n\t\t\t\t\t\t\t\t\tDeclare @FirstFriday int\r\n\t\t\t\t\t\t\t\t\tDeclare @bFirst bit=0\r\n\t\t\t\t\t\t\t\t\tset @cnt=0\r\n\t\t\t\t\t\t\t\t\tif @dt1 < @dt2\r\n\t\t\t\t\t\t\t\t\tBegin\r\n\t\t\t\t\t\t\t\t\t set @dt=@dt1\r\n\t\t\t\t\t\t\t\t\t while @dt <= @dt2\r\n\t\t\t\t\t\t\t\t\t Begin\r\n\t\t\t\t\t\t\t\t\t  if Datepart(dw,@dt)=6\r\n\t\t\t\t\t\t\t\t\t  Begin\r\n\t\t\t\t\t\t\t\t\t  if(@bFirst=0)\r\n\t\t\t\t\t\t\t\t\t  set @FirstFriday= dbo.DateToInt(@dt)\r\n\t\t\t\t\t\t\t\t\t   set @cnt=@cnt+1\r\n\t\t\t\t\t\t\t\t\t   set @dt=dateadd(dd,1,@dt)\r\n\t\t\t\t\t\t\t\t\t  End\r\n\t\t\t\t\t\t\t\t\t  set @dt=dateadd(dd,1,@dt)\r\n\t\t\t\t\t\t\t\t\t End\r\n\t\t\t\t\t\t\t\t\tEnd\r\n\t\t\t\t\t\t\t\t\tset @NoOfweeks= @cnt\r\n\r\n\t\t\t\t\t\t\t\t\tDeclare @temp as table(iServiceType int ,ServiceName nvarchar(500),Week1 nvarchar(100),iChargeType int,\r\n                                        Tariff nvarchar(500),dTotalQty decimal(18,10),dRate decimal(18,10),dTotalCharge decimal(18,10) )\r\n\t\t\t\t\t\t\t\t\tDeclare @WeekNo int=0\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\tDeclare @StartDay int,@EndDay int\r\n\r\n\t\t\t\t\t\t\t\t\tset @StartDay=dbo.DateToInt(@dt1)\r\n\t\t\t\t\t\t\t\t\tset @EndDay=@FirstFriday\r\n\r\n\t\t\t\t\t\t\t\t\twhile(@NoOfweeks>0)\r\n\t\t\t\t\t\t\t\t\tbegin\r\n\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\tset @WeekNo=@WeekNo+1;\r\n\t\t\t\t\t\t\t\t\tinsert into @temp \r\n\t\t\t\t\t\t\t\t\tselect Bill.iServiceTypeId [iHeaderId], '' ServiceName, 'week '+cast(@WeekNo as nvarchar(200)) week1, Bill.iChargeType \r\n\t\t\t\t\t\t\t\t\t--,sum(Bill.dTotalQty)dTotalQty\r\n                                    , ISNULL(T.sCode,'') Tariff\r\n\t\t\t\t\t\t\t\t\t,sum(dTotalQty) dTotalQty\r\n\t\t\t\t\t\t\t\t\t,avg(Bill.dRate) dRate, \r\n\t\t\t\t\t\t\t\t\tsum(Bill.dTotalCharge+Bill.dRecCharges) dTotalCharge \r\n\t\t\t\t\t\t\t\t\tfrom \r\n\t\t\t\t\t\t\t\t\ttWms_BillingHeader BH join\r\n\t\t\t\t\t\t\t\t\ttWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n                                    join muCore_Product_Tariff_Details TD on TD.iMasterId=Bill.iSKUId\r\n\t\t\t\t\t\t\t\t\tleft join mWMS_Tariff T on T.iId= ISNULL(TD.iTariffCode,0)\r\n\t\t\t\t\t\t\t\t\t--join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n\t\t\t\t\t\t\t\t\twhere -- Bill.iBillDate>={1} and  Bill.iBillDate<={2} and \r\n\t\t\t\t\t\t\t\t\tBill.iBillDate>=@StartDay and  Bill.iBillDate<=@EndDay and \r\n\t\t\t\t\t\t\t\t\tBill.iChargeType in (5,6) {3} \r\n\t\t\t\t\t\t\t\t\tgroup by Bill.iServiceTypeId, Bill.iChargeType,Bill.iChargeType,T.sCode\t\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\t\r\n\t\t\t\t\t\t\t\t\tset @StartDay=  dbo.fCore_GrigToInt(DATEADD(dd,1, dbo.inttodate(@EndDay)))\r\n\t\t\t\t\t\t\t\t\tset @EndDay=dbo.fCore_GrigToInt(DATEADD(dd,7, dbo.inttodate(@StartDay)))\r\n\r\n\t\t\t\t\t\t\t\t\tset @NoOfweeks=@NoOfweeks-1\r\n\t\t\t\t\t\t\t\t\tend\r\n\r\n                                    Select 0 [iHeaderId], * from (\r\n\t\t\t\t\t\t\t\t\tselect iServiceType ,ServiceName ,Week1 ,iChargeType , Tariff,dTotalQty ,dRate ,dTotalCharge from @temp\r\n\t\t\t\t\t\t\t\t\tunion all\r\n\t\t\t\t\t\t\t\t\t select Bill.iServiceTypeId , Ser.sName ServiceName,'' week1, Bill.iChargeType, ISNULL(T.sCode,'') Tariff\r\n\t\t\t\t\t\t\t\t\t--,sum(Bill.dTotalQty)dTotalQty\r\n\t\t\t\t\t\t\t\t\t,case when Bill.iChargeType=0 then sum(Bill.dTotalCBM) else sum(Bill.dTotalQty) end dTotalQty\r\n\t\t\t\t\t\t\t\t\t,case when Bill.iChargeType=0 then sum(Bill.dTotalCharge)/sum(Bill.dTotalCBM) else \r\n\t\t\t\t\t\t\t\t\tsum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end)/sum(Bill.dTotalQty) end dRate, \r\n\t\t\t\t\t\t\t\t\tsum(case when Bill.dTotalCharge=0 then Bill.dRecCharges else Bill.dTotalCharge end) dTotalCharge\r\n\t\t\t\t                    from \r\n\t\t\t\t                    tWms_BillingHeader BH join\r\n\t\t\t\t                    tWms_BillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId\r\n\t\t\t\t                    join mCore_Product Ser on Ser.iMasterId=Bill.iServiceTypeId\r\n                                    join muCore_Product_Tariff_Details TD on TD.iMasterId=Bill.iSKUId\r\n\t\t\t\t\t\t\t\t\tleft join mWMS_Tariff T on T.iId= ISNULL(TD.iTariffCode,0)\r\n\t\t\t\t                    where  Bill.iBillDate>={1} and  Bill.iBillDate<={2} and \r\n\t\t\t\t\t\t\t\t\tBill.iChargeType not in (5,6) {3} \r\n                                    group by Bill.iServiceTypeId, Ser.sName, Bill.iChargeType,Bill.dRate,Bill.iChargeType,T.sCode)A\r\n\t\t\t\t\t\t\t\t\t ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3, FConvert.GetInputValue(oRec.Inputs, 1));
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSCartonWiseASNvsRTSQuantity:
		{
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and ASNData.iHeaderId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			string text11 = $" Select top 1 iLinkPathId from vmCore_Links{m_sSuffix} where BaseVoucherId = {8960}";
			long num4 = Convert.ToInt64(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text11) : text11));
			text11 = $" Select top 1 iLinkPathId from vmCore_Links{m_sSuffix} where BaseVoucherId = {9216}";
			Convert.ToInt64(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text11) : text11));
			standardQuery = new StandardQuery();
			arrDefaultTables.AddRange("vrCore_Product".Split(','));
			standardQuery.Query = string.Format("\r\n                    select ASNData.iHeaderId , ASNHeader.sVoucherNo,mCore_Product.sName ItemName, mCore_Product.sCode ItemCode,\r\n                    ASNExtBody.sCarton [ASN CartonNo],\r\n                    case when RTSData.sCarton is null then ASNindta.fQuantityInBase \r\n                    when ASNExtBody.sCarton = RTSData.sCarton  then ASNindta.fQuantityInBase else 0 end [ASN Qty] , \r\n                    sum(isnull(RTSData.fQuantityInBase,0)) [RTS Qty] , isnull(RTSData.sCarton,'') [RTS CartonNo] {4}\r\n                    from tCore_Data{0} ASNData \r\n                    inner join tCore_Data{8}{0} ASNExtBody on ASNExtBody.iBodyId = ASNData.iBodyId\r\n                    INNER JOIN tCore_Header{0} ASNHeader ON ASNHeader.iHeaderId= ASNData.iHeaderId\r\n                    inner join tCore_Indta{0} ASNINDTa on ASNindta.iBodyId= ASNData.iBodyId\r\n                    inner join mCore_Product on mCore_Product.iMasterId =ASNINDTa.iProduct                    \r\n                    left join (Select Link.iRefId, RTSData.iTransactionId ,fQuantityInBase, sCarton\r\n                    from tCore_Data{0} RTSData \r\n                    inner join tCore_Header{0} RTSHeader on RTSData.iHeaderId= RTSHeader.iHeaderId\r\n                    inner join tCore_Data{3}{0} RTSExtBody on RTSExtBody.iBodyId = RTSData.iBodyId\r\n                    inner join tCore_Indta{0} RTSIndta on RTSIndta.iBodyId= RTSData.iBodyId\r\n                    left join tCore_Links{0} Link ON RTSData.iTransactionId=Link.iTransactionId \r\n                     WHERE  iVoucherClass= {3}  and bBase=0\r\n                    ) RTSData ON ASNData.iTransactionId=iRefId\r\n                    {5}\r\n                    where 0 = 0 {2} {7}\r\n                    group by ASNData.iHeaderId,ASNHeader.sVoucherNo,mCore_Product.sName , mCore_Product.sCode , ASNExtBody.sCarton , ASNindta.fQuantityInBase, RTSData.sCarton {6}", m_sSuffix, num4, text, 9216, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_GROUPBY", "--@EXTRA_FILTER", 8960);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		case FocusReport.WMSUserWiseQuantities:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and A.UserId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\nselect UserId, UserName, EmployeeName, sum(ASNQuantity) ASNQuantity, sum(PutAwayQuantity) PutAwayQuantity, sum(PickQuantity) PickQuantity, sum(DeliveredQuantity) DeliveredQuantity, sum(DistributedQuantity) DistributedQuantity {6} \r\nfrom (\r\n--asn data\r\nselect  0 iHeaderId, mSec_Users.iUserId UserId, mSec_Users.sUserName [UserName],  mPay_Employee.sName EmployeeName, (tCore_AllocateUserDetails{0}.dAllocatedQty) [ASNQuantity] , 0 [PutAwayQuantity] , 0 [PickQuantity] , 0 [DeliveredQuantity] , 0 [DistributedQuantity] \r\nfrom tCore_Header{0}  \r\njoin tCore_Data{0} on tCore_Data{0}.iHeaderId= tCore_Header{0}.iHeaderId\r\ninner join tCore_AllocateUserDetails{0}  on tCore_AllocateUserDetails{0}.iTransactionId = tCore_Data{0}.iTransactionId\r\njoin mPay_Employee  on tCore_AllocateUserDetails{0}.iAllocatedUserId = mPay_Employee.iMasterId\r\nleft join mSec_Users on mSec_Users.iLinkId =  mPay_Employee.iMasterId\r\nwhere  tCore_Header{0}.iVoucherType = {1}\r\n\r\nunion all\r\n--picklist data\r\nselect   0 iHeaderId, mSec_Users.iUserId UserId, mSec_Users.sUserName [UserName], mPay_Employee.sName EmployeeName, 0 [ASNQuantity] , 0 [PutAwayQuantity] , (tCore_AllocateUserDetails{0}.dAllocatedQty) [PickQuantity] , 0 [DeliveredQuantity] , 0 [DistributedQuantity] \r\nfrom tCore_Header{0}  \r\njoin tCore_Data{0} on tCore_Data{0}.iHeaderId= tCore_Header{0}.iHeaderId\r\ninner join tCore_AllocateUserDetails{0}  on tCore_AllocateUserDetails{0}.iTransactionId = tCore_Data{0}.iTransactionId\r\njoin mPay_Employee  on tCore_AllocateUserDetails{0}.iAllocatedUserId = mPay_Employee.iMasterId\r\nleft join mSec_Users on mSec_Users.iLinkId =  mPay_Employee.iMasterId\r\nwhere  tCore_Header{0}.iVoucherType = {2}\r\n--group by u.iUserId, u.sUserName\r\nunion all\r\n--putaway data\r\nselect   0 iHeaderId, mSec_Users.iUserId UserId, mSec_Users.sUserName [UserName], mPay_Employee.sName EmployeeName,0 [ASNQuantity] , (tCore_AllocateUserDetails{0}.dAllocatedQty) [PutAwayQuantity] , 0 [PickQuantity] , 0 [DeliveredQuantity] , 0 [DistributedQuantity] \r\nfrom tCore_Header{0}  \r\njoin tCore_Data{0} on tCore_Data{0}.iHeaderId= tCore_Header{0}.iHeaderId\r\ninner join tCore_AllocateUserDetails{0}  on tCore_AllocateUserDetails{0}.iTransactionId = tCore_Data{0}.iTransactionId\r\njoin mPay_Employee  on tCore_AllocateUserDetails{0}.iAllocatedUserId = mPay_Employee.iMasterId\r\nleft join mSec_Users on mSec_Users.iLinkId =  mPay_Employee.iMasterId\r\nwhere  tCore_Header{0}.iVoucherType = {3}\r\nunion all\r\n--dispatch  data\r\nselect   0 iHeaderId, mSec_Users.iUserId UserId, mSec_Users.sUserName [UserName], mPay_Employee.sName EmployeeName,0 [ASN Quantity] , 0 [PutAwayQuantity] , 0 [PickQuantity] , (tCore_AllocateUserDetails{0}.dAllocatedQty) [DeliveredQuantity] , 0 [DistributedQuantity] \r\nfrom tCore_Header{0}  \r\njoin tCore_Data{0} on tCore_Data{0}.iHeaderId= tCore_Header{0}.iHeaderId\r\ninner join tCore_AllocateUserDetails{0}  on tCore_AllocateUserDetails{0}.iTransactionId = tCore_Data{0}.iTransactionId\r\njoin mPay_Employee  on tCore_AllocateUserDetails{0}.iAllocatedUserId = mPay_Employee.iMasterId\r\nleft join mSec_Users on mSec_Users.iLinkId =  mPay_Employee.iMasterId\r\nwhere  tCore_Header{0}.iVoucherType = {4}\r\n\r\nunion all\r\n\r\nselect  0 iHeaderId, mSec_Users.iUserId UserId, mSec_Users.sUserName [User Name],   mPay_Employee.sName EmployeeName, 0 [ASNQuantity] , 0 [PutAwayQuantity] , 0 [PickQuantity] , 0 [DeliveredQuantity] , dQty [DistributedQuantity] \r\nfrom  tWMS_DAllocateStorewiseQty\r\njoin mSec_Users on tWMS_DAllocateStorewiseQty.iAllocUserId = mSec_Users.iUserId\r\nleft join mPay_Employee  on mPay_Employee.iMasterId = mSec_Users.iLinkId\r\n) A\r\nleft join tCore_Header{0} on tCore_Header{0}.iHeaderId = A.iHeaderId\r\n{7}\r\nwhere 0 = 0 {5} {9}\r\ngroup by A.UserId, A.UserName , A.iHeaderId, A.EmployeeName {8}\r\n", m_sSuffix, 8960, 11008, 9472, 11520, text, "--@EXTRA_COLUMNS", "--@EXTRA_TABLES", "--@EXTRA_GROUPBY", "--@EXTRA_FILTER");
			standardQuery.PrimaryColumn = "A.iHeaderId";
			break;
		case FocusReport.WMSDateWiseShipInShipOutQuantity:
		{
			bool flag2 = _focus.company(m_iCompanyId).isModuleImplemented(ModulesImplemented.WMS3PL);
			string text7 = 9216.ToString();
			string text8 = 11776.ToString();
			string text9 = "";
			string text10 = ",'' StorerCode";
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				if (flag2)
				{
					text += $" and Storer.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
					text9 = $" inner join mWms_Storer Storer on  tCore_Bins{m_sSuffix}.iAllocTag = Storer.iMasterId";
				}
				else
				{
					text += $" and Storer.iMasterId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
					text9 = $" inner join mWms_InventoryAllocationStatus  Storer on  tCore_Bins{m_sSuffix}.iAllocTag = Storer.iMasterId";
				}
				text10 = ",Storer.sCode StorerCode";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 2) == 1)
			{
				text7 += $",{512},{2048}";
				text8 += $",{5376}";
			}
			standardQuery = new StandardQuery();
			arrDefaultTables.AddRange("vrCore_Product".Split(','));
			standardQuery.Query = string.Format("\r\nselect 0 iHeaderId, iDate, A.ProductName,A.ProductCode, A.iDefaultBaseUnitsCode BaseUnitCode, A.ReceivingUnitCode, sum(A.[Pallet Qty(Received)]) [Pallet Qty(Received)],\r\nabs(sum(A.[Base Qty(Received)])) [Base Qty(Received)] , A.DispatchUnitCode, abs(sum(A.[Pallet Qty(Dispatch)])) [Pallet Qty(Dispatch)] ,\r\nabs(sum(A.[Base Qty(Dispatch)])) [Base Qty(Dispatch)] , StorerCode from \r\n(\r\n\tselect iDate, dbo.IntToDate(iDate) Date, vrCore_Product.sCode ProductCode, vrCore_Product.sName ProductName,vrCore_Product.iDefaultBaseUnitsCode,  mCore_Units.sCode [ReceivingUnitCode], \r\n\ttCore_Indta{0}.fQuantity [Pallet Qty(Received)] , tCore_Indta{0}.fQuantityInBase [Base Qty(Received)] , '' DispatchUnitCode,\r\n\t0 [Pallet Qty(Dispatch)] , 0 [Base Qty(Dispatch)] {7}\r\n\tfrom tCore_Header{0}\r\n\tinner join tCore_Data{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n\tinner join tCore_Indta{0} on tCore_Data{0}.iBodyId  = tCore_Indta{0}.iBodyId\r\n\tinner join vrCore_Product on tCore_Indta_0.iProduct = vrCore_Product.iMasterId\r\n\tinner join mCore_Units on tCore_Indta_0.iUnit = mCore_Units.iMasterId\r\n    inner join tCore_Bins{0} on tCore_Data{0}.iBodyId = tCore_Bins{0}.iBodyId\r\n    {6}\r\n\twhere iVoucherType in({1}) and tCore_Data{0}.bSuspendUpdateFA = 0  AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 and tCore_Header{0}.iDate between {4} and {5} {3}--openingstock, excessinstock, rts, \r\n\tunion all\r\n\tselect iDate, dbo.IntToDate(iDate), vrCore_Product.sCode, vrCore_Product.sName,vrCore_Product.iDefaultBaseUnitsCode, '' [ReceivingUnitCode],\r\n\t0 [Pallet Qty(Received)] , 0 [Base Qty(Received)] ,mCore_Units.sCode DispatchUnitCode, tCore_Indta{0}.fQuantity [Pallet Qty(Dispatch)] , tCore_Indta{0}.fQuantityInBase [Base Qty(Dispatch)]\r\n\t{7}\r\n\tfrom tCore_Header{0}\r\n\tinner join tCore_Data{0} on tCore_Header_0.iHeaderId = tCore_Data_0.iHeaderId\r\n\tinner join tCore_Indta{0} on tCore_Data_0.iBodyId  = tCore_Indta_0.iBodyId\r\n\tinner join vrCore_Product on tCore_Indta_0.iProduct = vrCore_Product.iMasterId\r\n\tinner join mCore_Units on tCore_Indta_0.iUnit = mCore_Units.iMasterId\r\n    inner join tCore_Bins{0} on tCore_Data{0}.iBodyId = tCore_Bins{0}.iBodyId\r\n    {6}\r\n\twhere iVoucherType in({2}) and tCore_Data{0}.bSuspendUpdateFA = 0  AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 and tCore_Header{0}.iDate between {4} and {5} {3}-- shortagein stock, disp confirmation\r\n) A\r\ngroup by A.iDate,Date,A.ProductCode , A.ProductName,A.iDefaultBaseUnitsCode, A.ReceivingUnitCode, A.DispatchUnitCode,StorerCode\r\n                    ", m_sSuffix, text7, text8, text, oRec.StartingDate, oRec.EndingDate, text9, text10);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		}
		case FocusReport.CloseASNReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text2 = $" Where a.iASNId = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                    select P.iMasterId ProductId, ASNHeader.sVoucherNo ASNNO,ShipmentHeader.sVoucherNo TransferNo,P.sName ItemName,b.dQty Quantity\r\n                        ,I.sName AllocTag,b.sCartonNo CartonNo \r\n                    from tWMS_HAllocateStorewiseQty a\r\n                        join tWMS_DAllocateStorewiseQty b on b.iHeaderId =  a.iHeaderId\r\n                        join tCore_Header_0 ASNHeader  on a.iASNId = ASNHeader.iHeaderId \r\n                        join tCore_Header_0 ShipmentHeader on a.iNewShipTransferId=ShipmentHeader.iHeaderId \r\n                        join vrCore_Product P on b.iProductId = P.iMasterId join\r\n                         mWms_InventoryAllocationStatus I  on b.iAllocTagId=I.iMasterId {3} ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS");
			standardQuery.PrimaryColumn = "ProductId";
			break;
		case FocusReport.StockOnHandByCartonWise:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text = $" and T.iProduct = {FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			arrDefaultTables.AddRange("vrCore_Product".Split(','));
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                    Select vrCore_Product.iMasterId iProductId,  Stock.[Bin Location], stock.[Carton ID], vrCore_Product.sCode[Item No], sum(stock.[On-Hand Stock]) [On-Hand Stock],sum(stock.[Reserverd Qty]) [Reserverd Qty],0 AvailableQuantity {3}\r\n                    from\r\n                    (\r\n                    Select   mCore_Bins.sCode[Bin Location], isnull(B.sCarton, '')[Carton ID], T.iProduct, Sum(B.fQuantity) [On-Hand Stock], 0 [Reserverd Qty]\r\n                    From tCore_Header{0} H  Join\r\n                        tCore_Data{0} D  on  H.iHeaderId = D.iHeaderId join\r\n                        tCore_Bins{0} B   on B.iBodyId = D.iBodyId Join\r\n                        mCore_Bins ON mCore_Bins.iMasterId = B.iBin join\r\n                        muCore_Bins on mCore_Bins.iMasterId = muCore_Bins.iMasterId join\r\n                        mCore_BinsTreeDetails on mCore_Bins.iMasterId = mCore_BinsTreeDetails.iMasterId join\r\n                        tCore_Indta{0} T on T.iBodyId = D.iBodyId\r\n                    Where    bUpdateStocks = 1 and H.bCancelled = 0 and D.bSuspendUpdateStocks <> 1 and B.iFlag < 8 and B.iFlag != 1 and\r\n                        H.bSuspended = 0 AND D.iAuthStatus = 1\r\n                        and H.bUpdateStocks = 1 AND mCore_BinsTreeDetails.iTreeId = 0--AND muCore_Bins.iType <> 2\r\n                        AND muCore_Bins.bDamaged = 0 and muCore_Bins.bHold = 0 {4}\r\n                    Group By  mCore_Bins.sCode, B.sCarton, T.iProduct\r\n                    Having Sum(B.fQuantity) > 0\r\n\r\n                    Union All\r\n\r\n                    Select   mCore_Bins.sCode[Bin Location], isnull(R.sCarton, '')[Carton ID], T.iProduct, 0[On-Hand Stock], -Sum(Case When bReserveOrRelease = 0 then R.fQuantity else -R.fQuantity end) [Reserverd Qty]\r\n                    From tCore_ReservedStock{0} R with(ReadUncommitted)  join\r\n                        tCore_Data{0} D on  R.iReqTransactionId = D.iTransactionId and D.iAuthStatus = 1 Join\r\n                        tCore_Header{0} H on  H.iHeaderId = D.iHeaderId join\r\n                        tCore_Indta{0} T on  T.iBodyId = D.iBodyId  join\r\n                        mCore_Bins ON mCore_Bins.iMasterId = R.iBin join\r\n                        tCore_Skid{0}  with(ReadUncommitted)  on tCore_Skid_0.iId = isnull(R.iSkidId, 0) left join\r\n                        tCore_Batch{0} Bat  on Bat.iBodyId = D.iBodyId\r\n                    Where T.iProduct > 0 {4} --and B.iFlag < 8 and B.iFlag != 1 AND H.bSuspended = 0 AND D.iAuthStatus = 1\r\n                    Group By  mCore_Bins.sCode,R.sCarton,T.iProduct\r\n                    Having Sum(Case When bReserveOrRelease = 0 then R.fQuantity else -R.fQuantity end) > 0\r\n                    ) Stock inner join\r\n                    vrCore_Product on vrCore_Product.iMasterId = Stock.iProduct \r\n                    group by vrCore_Product.iMasterId,Stock.[Bin Location], Stock.[Carton ID],vrCore_Product.sCode, Stock.iProduct {3}\r\n                    ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, "--@EXTRA_COLUMNS", text);
			standardQuery.PrimaryColumn = "iProductId";
			break;
		case FocusReport.WMSAdvanceServiceCharges:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text = string.Format(" and StorerData.iTag{1}={0} ", FConvert.GetInputValue(oRec.Inputs, 1), 1006);
			}
			preferenceValue = _focus.company(m_iCompanyId).invTagId;
			viewName = _focus.company(m_iCompanyId).master(preferenceValue).viewName;
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("\r\n                    select tCore_Header{0}.iHeaderId,cCore_Vouchers_0.sName,tCore_Header{0}.sVoucherNo,mCore_Product.sName,mWMS_AdvTariffCharges.iChargedOn\r\n                    ,sum(tWms_ServiceChargeDetail.dQty) dQty,sum(tWms_ServiceChargeDetail.dUnitQty) dUnitQty,sum(isnull(mWMS_AdvTariffAddCharges.dCharges,0)) dCharges\r\n                    ,sum(mWMS_AdvTariffCharges.dMinCharges) dMinChrg,sum(mWMS_AdvTariffCharges.dMaxCharges) dMaxCharge\r\n                    ,sum(tWms_ServiceChargeDetail.dAdditionalItemQty) dAdditionalItemQty,sum(tWms_ServiceChargeDetail.dAdditionalUnitQty) dAdditionalUnitQty\r\n                    ,sum(tWms_ServiceChargeDetail.dTotalCharges) dTotalCharges,tCore_Header{0}.iDate iDate,{4}.iMasterId,{4}.sName\r\n                    from tWms_ServiceChargeDetail \r\n                    JOIN tWms_ServiceChargeHeader  ON tWms_ServiceChargeDetail.iSCHeaderId=tWms_ServiceChargeHeader.iSCHeaderId\r\n                    JOIN tCore_Header{0}  ON tCore_Header{0}.iHeaderId=tWms_ServiceChargeHeader.iDocumentId\r\n                    JOIN (select DISTINCT (iTag{5}),iHeaderId,iInvTag  from tCore_Data{0} \r\n                    JOIN tCore_Data_Tags{0}  ON tCore_Data_Tags{0}.iBodyId=tCore_Data{0}.iBodyId) StorerData ON StorerData.iHeaderId=tCore_Header{0}.iHeaderId\r\n                    JOIN cCore_Vouchers{0}  ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                    JOIN mCore_Product  ON mCore_Product.iMasterId=tWms_ServiceChargeDetail.iProduct\r\n                    JOIN {4} ON {4}.iMasterId=StorerData.iInvTag\r\n\t\t\t\t\tjoin mWMS_AdvTariffCharges on mWMS_AdvTariffCharges.iTariffChargeId=tWms_ServiceChargeDetail.iTariffChargeId\r\n                    left join mWMS_AdvTariffAddCharges on  mWMS_AdvTariffAddCharges.iAddChargeId = tWms_ServiceChargeDetail.iAddChargeId\r\n                    where  tWms_ServiceChargeHeader.iScreenType=0 and tWms_ServiceChargeDetail.iDate between {1} and {2} {3}\r\n                    group by tCore_Header{0}.iHeaderId,cCore_Vouchers{0}.sName,tCore_Header{0}.sVoucherNo, mCore_Product.sName,mWMS_AdvTariffCharges.iChargedOn ,{4}.iMasterId,{4}.sName,tCore_Header{0}.iDate\r\n                    \r\n                    UNION ALL\r\n\r\n                    select cv.iHeaderId,cCore_Vouchers_0.sName,tCore_Header{0}.sVoucherNo,mCore_Product.sName,tc.iChargedOn\r\n                    ,cvd.dQty dQty,0 dUnitQty,isnull(ta.dCharges,0) dCharges\r\n                    ,(tc.dMinCharges) dMinChrg,(tc.dMaxCharges) dMaxCharge\r\n                    ,0 dAdditionalItemQty,0 dAdditionalUnitQty\r\n                    ,(cvd.dTotalCharges) dTotalCharges,tCore_Header{0}.iDate iDate,{4}.iMasterId,{4}.sName\r\n                    from tWms_AdvChargeVoucher cv\r\n                    inner join tWms_AdvChargeVoucherDetails cvd on cv.iCVHeaderId = cvd.iCVHeaderId\r\n                    JOIN tCore_Header{0}  ON tCore_Header_0.iHeaderId=cv.iHeaderId\r\n                    inner join mWMS_AdvTariffCharges tc on tc.iTariffChargeId = cvd.iTariffChargeId\r\n                    inner join mWMS_AdvTariff t on tc.iTariffId = t.iTariffId\r\n                    left join mWMS_AdvTariffAddCharges ta on ta.iTariffChargeId = tc.iTariffChargeId\r\n                    JOIN cCore_Vouchers{0}  ON cCore_Vouchers{0}.iVoucherType =  tCore_Header{0}.iVoucherType\r\n                    JOIN mCore_Product  ON mCore_Product.iMasterId=cvd.iSerProductId\r\n                    JOIN (select DISTINCT (iTag{5}),iHeaderId,iInvTag  from tCore_Data{0} \r\n                    JOIN tCore_Data_Tags{0}  ON tCore_Data_Tags{0}.iBodyId=tCore_Data{0}.iBodyId) StorerData ON StorerData.iHeaderId=tCore_Header{0}.iHeaderId\r\n                    JOIN {4} ON {4}.iMasterId=StorerData.iInvTag\r\n                    where cv.iApplyDate between {1} and {2} {3}\r\n                    ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, viewName, 1006);
			standardQuery.PrimaryColumn = "iHeaderId";
			break;
		case FocusReport.WMSAdvanceBillingReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0)
			{
				text += $" and BH.iStorerId ={FConvert.GetInputValue(oRec.Inputs, 1)} ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format(" Select ATC.iProduct [iHeaderId], Storer.sName StorerName, ATC.iOperationType, ATC.iChargeCalcType, \r\n                                                      (IsNull(V.sAbbr,'') +' ' + IsNull(H.sVoucherNo,'')) sVoucherNo, Ser.sName ServiceName, ATC.iChargedOn ,\r\n                                                      BillSummary.iBillDate, BillSummary.dTotalQty,  BillSummary.iTotalUnitQty, BillSummary.dAdditionalQty dAdditionalQty,\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t  BillSummary.iAdditionalUnitQty, BillSummary.dRate , BillSummary.dTotalCharge\r\n                                                      from \r\n                                                      tWms_AdvBillingHeader BH Join\r\n                                                      --tWms_AdvBillingDetails Bill on BH.iBillHeaderId=Bill.iBillHeaderId Join\r\n                                                      tWms_AdvBillingSummary BillSummary on BH.iBillHeaderId=BillSummary.iBillHeaderId Join     \r\n\t\t\t\t\t\t\t\t\t                  --mWMS_AdvTariff AdvT on AdvT.iTariffId = BH.iTariffId Join\r\n\t\t\t\t\t\t\t\t\t                  mWMS_AdvTariffCharges ATC On ATC.iTariffChargeId = BillSummary.iTraiffChargeId Join\r\n                                                      mCore_Product Ser on Ser.iMasterId=ATC.iProduct Join\r\n\t\t\t\t\t\t\t\t\t                  mWms_Storer Storer On Storer.iMasterId = BH.iStorerId and Storer.iStatus <> 5 Left Join\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t  tCore_Header{0} H On H.iHeaderId = BillSummary.iVHeaderId Left Join\r\n\t\t\t\t\t\t\t\t\t\t\t\t\t  cCore_Vouchers{0} V On V.iVoucherType = H.iVoucherType\r\n                                                    where  BillSummary.iBillDate >= {1} and  BillSummary.iBillDate <= {2} {3}\r\n                                                    --group by ATC.iProduct,Storer.sName, ATC.iOperationType, ATC.iChargeCalcType, BH.sVoucherRefNo, Ser.sName, Bill.iChargeOn ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text);
			break;
		}
		return standardQuery;
	}

	private StandardQuery callPOSQuery(RepRecord oRec, bool bPreviousYear, ref List<string> arrDefaultTables)
	{
		StandardQuery standardQuery = null;
		string text = null;
		string text2 = null;
		string text3 = null;
		if (oRec.ReportId == 8541)
		{
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND iModifiedTime BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) == 1)
			{
				string text4 = string.Format(" OR tCore_HeaderData{1}{0}.sBillReferenceNo LIKE (@PosSales)", m_sSuffix, 3584);
				text3 = string.Format("\r\n                        UNION ALL \r\n                        SELECT tCore_Header{0}.iDate[iDate],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5}  THEN CASE WHEN iPaymentType= 9 OR iPaymentType= 16 THEN mAmount2 ELSE 0 END ELSE 0 END[Cash Amount],\r\n\t                    0 [P_Cash Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=2 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit Card Amount],\r\n\t                    0 [P_Credit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=3 THEN mAmount2 ELSE 0 END ELSE 0 END[Debit Card Amount],\r\n\t                    0 [P_Debit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=4 THEN mAmount2 ELSE 0 END ELSE 0 END[Cheque Value],\r\n\t                    0 [P_Cheque Value],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=5  OR iPaymentType= 17 THEN mAmount2 ELSE 0 END ELSE 0 END[Coupon Amount],\r\n                        0 [P_Coupon Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=7 THEN mAmount2 ELSE 0 END ELSE 0 END[Points Redemption],\r\n\t                    0 [P_Points Redemption],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=6  OR iPaymentType= 18 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit Notes],\r\n\t                    0 [P_Credit Notes],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=8 THEN mAmount2 ELSE 0 END ELSE 0 END[Discount Coupon],\r\n                        0 [P_Discount Coupon],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=10 THEN 0 ELSE 0 END ELSE 0 END[Credit sales],\r\n\t                    0 [P_Credit sales],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=11 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit charges],\r\n\t                    0 [P_Credit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=12 THEN mAmount2 ELSE 0 END ELSE 0 END[Debit charges],\r\n\t                    0 [P_Debit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSR) {5} THEN CASE WHEN iPaymentType=14 THEN mAmount2 ELSE 0 END ELSE 0 END[EPayment Type],\r\n\t                    0 [P_EPayment Type],0[EPayment charges],0[P_EPayment charges], \r\n\t                    tCore_Data{0}.iInvTag[OutletId] \r\n\t                    FROM tCore_Header{0}\r\n\t                    JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n\t                    JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON tCore_Header{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType\r\n\t                    JOIN tCore_HeaderData{3}{0} WITH (READUNCOMMITTED) ON tCore_Header{0}.iHeaderId = tCore_HeaderData{3}{0}.iHeaderId\r\n\t                    JOIN tCore_Data{3}{0} WITH (READUNCOMMITTED) ON tCore_Data{0}.iBodyId = tCore_Data{3}{0}.iBodyId\r\n\t                    WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 \r\n\t                    AND tCore_Header{0}.bSuspended = 0 AND tCore_Header{0}.iDate BETWEEN {1} AND {2} {4} ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, 3584, text, text4, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS");
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("Declare @PosSales varchar(10) = '', @PosSO varchar(10) = '', @PosSR varchar(10) = ''\r\n                    select @PosSales = sAbbr + '%' from cCore_Vouchers{0} where iVoucherType = 3331\r\n                    select @PosSO = sAbbr + '%' from cCore_Vouchers{0} where iVoucherType = 5632\r\n                    select @PosSR = sAbbr + '%' from cCore_Vouchers{0} where iVoucherType = 1792\r\n                    Select 0[iHeaderId],[iDate], 0[Total],\r\n                    SUM([Cash Amount])[Cash Amount], SUM([P_Cash Amount])[P_Cash Amount],\r\n                    SUM([Credit Card Amount])[Credit Card Amount], SUM([P_Credit Card Amount])[P_Credit Card Amount],\r\n                    SUM([Debit Card Amount])[Debit Card Amount], SUM([P_Debit Card Amount])[P_Debit Card Amount],\r\n                    SUM([Cheque Value])[Cheque Value],SUM([P_Cheque Value])[P_Cheque Value],\r\n                    SUM([Coupon Amount])[Coupon Amount],SUM([P_Coupon Amount])[P_Coupon Amount],\r\n                    SUM([Credit Notes])[Credit Notes],SUM([P_Credit Notes])[P_Credit Notes],\r\n                    SUM([Discount Coupon])[Discount Coupon],SUM([P_Discount Coupon])[P_Discount Coupon],\r\n                    SUM([Credit sales])[Credit sales],SUM([P_Credit sales])[P_Credit sales],\r\n                    SUM([Credit charges])[Credit charges],SUM([P_Credit charges])[P_Credit charges],\r\n                    SUM([Debit charges])[Debit charges],SUM([P_Debit charges])[P_Debit charges],\r\n                    SUM([Points Redemption])[Points Redemption],SUM([P_Points Redemption])[P_Points Redemption],\r\n                    SUM([EPayment Type]) [EPayment Type],\r\n                    SUM([P_EPayment Type]) [P_EPayment Type],\r\n                    SUM([EPayment charges]) [EPayment charges],\r\n                    SUM([P_EPayment charges]) [P_EPayment charges],\r\n                    [OutletId] {7} \r\n                    FROM \r\n                    (\r\n\t                    SELECT tCore_Header{0}.iDate[iDate],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5}  THEN CASE WHEN iPaymentType=1 THEN mAmount2 ELSE CASE WHEN iPaymentType = 9 THEN -mAmount2 ELSE 0 END END ELSE 0 END[Cash Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=1 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Cash Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=2 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=2 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Credit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=3 THEN mAmount2 ELSE 0 END ELSE 0 END[Debit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=3 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Debit Card Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=4 THEN mAmount2 ELSE 0 END ELSE 0 END[Cheque Value],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=4 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Cheque Value],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=5 THEN mAmount2 ELSE 0 END ELSE 0 END[Coupon Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=5 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Coupon Amount],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=7 THEN mAmount2 ELSE 0 END ELSE 0 END[Points Redemption],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=7 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Points Redemption],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=6 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit Notes],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=6 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Credit Notes],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=8 THEN mAmount2 ELSE 0 END ELSE 0 END[Discount Coupon],\r\n                        CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=8 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Discount Coupon],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=10 THEN fCreditSaleAmount ELSE 0 END ELSE 0 END[Credit sales],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=10 THEN fCreditSaleAmount ELSE 0 END ELSE 0 END[P_Credit sales],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=11 THEN mAmount2 ELSE 0 END ELSE 0 END[Credit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=11 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Credit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=12 THEN mAmount2 ELSE 0 END ELSE 0 END[Debit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=12 THEN mAmount2 ELSE 0 END ELSE 0 END[P_Debit charges],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=14 THEN mAmount2 ELSE 0 END ELSE 0 END[EPayment Type],\r\n\t                    CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=14 THEN mAmount2 ELSE 0 END ELSE 0 END[P_EPayment Type],\r\n                        CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSales) {5} THEN CASE WHEN iPaymentType=15 THEN mAmount2 ELSE 0 END ELSE 0 END[EPayment charges],\r\n                        CASE WHEN tCore_HeaderData{3}{0}.sBillReferenceNo LIKE (@PosSO) {5} THEN CASE WHEN iPaymentType=15 THEN mAmount2 ELSE 0 END ELSE 0 END[P_EPayment charges],\r\n\t                    tCore_Data{0}.iInvTag[OutletId] \r\n\t                    FROM tCore_Header{0}\r\n\t                    JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n\t                    JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON tCore_Header{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType\r\n\t                    JOIN tCore_HeaderData{3}{0} WITH (READUNCOMMITTED) ON tCore_Header{0}.iHeaderId = tCore_HeaderData{3}{0}.iHeaderId\r\n\t                    JOIN tCore_Data{3}{0} WITH (READUNCOMMITTED) ON tCore_Data{0}.iBodyId = tCore_Data{3}{0}.iBodyId\r\n\t                    WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 \r\n\t                    AND tCore_Header{0}.bSuspended = 0 AND tCore_Header{0}.iDate BETWEEN {1} AND {2} {4} \r\n                        {8}\r\n                    )TempTable  \r\n                    WHERE iDate BETWEEN {1} AND {2}\r\n                    AND [OutletId] IN ({6}) \r\n                    GROUP BY [iDate], [OutletId]  ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, 4096, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text3);
			arrDefaultTables.AddRange(string.Format("tCore_Header{0},tCore_Data{0},tCore_Data{1}{0},mPos_Outlet,cCore_Vouchers{0}", m_sSuffix, 4096).Split(','));
			standardQuery.PrimaryColumn = string.Format(" OutletId, [iDate]", m_sSuffix);
		}
		return standardQuery;
	}

	private LineData[] ArrangePaymentTotal(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		decimal num4 = 0m;
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		switch ((FocusReport)objRec.ReportId)
		{
		case FocusReport.WalkinPreorderSalesWise:
			num3 = 5;
			break;
		case FocusReport.PaymentWiseSales:
			num3 = 29;
			break;
		}
		for (num = 0; num < list.Count; num++)
		{
			num4 = 0m;
			for (num2 = 3; num2 < num3; num2++)
			{
				num4 += Convert.ToDecimal(list[num].CellData[num2]);
			}
			list[num].CellData[2] = num4;
		}
		return list.ToArray();
	}

	private StandardQuery callProductionQuery(RepRecord oRec, bool bPreviousYear, ref List<string> arrDefaultTables, int iCompanyId)
	{
		StandardQuery standardQuery = null;
		string text = null;
		string text2 = null;
		string text3 = string.Empty;
		string text4 = string.Empty;
		string text5 = "";
		string text6 = string.Empty;
		string text7 = string.Empty;
		string text8 = string.Empty;
		string text9 = string.Empty;
		string text10 = string.Empty;
		string sMRPExtraColumns = string.Empty;
		string text11 = string.Empty;
		bool flag = _focus.company(iCompanyId).isModuleImplemented(ModulesImplemented.MRP1);
		Focus.Transactions.BL.Transactions transactions = new Focus.Transactions.BL.Transactions();
		switch ((FocusReport)oRec.ReportId)
		{
		case FocusReport.QCTestDefinitionReport:
		{
			string text36 = "vrCore_Warehouse";
			string text37 = string.Empty;
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND iModifiedTime BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) == 1)
			{
				text2 = string.Format(" OR tCore_HeaderData{1}{0}.sBillReferenceNo LIKE (@PosSR)", m_sSuffix, 4096);
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text36 = transactions.GetDefaultMasterTableName(FConvert.GetInputValue(oRec.Inputs, 4), iCompanyId);
				text37 = "--";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				text6 = $" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
			{
				text9 = $" left join (select top 1 iTestDefHeaderId,iTagValue from mQC_TestDefHeaderTagwiseDetails group by iTestDefHeaderId, iTagValue) QCTagDetails on QCTagDetails.iTestDefHeaderId=mQC_TestDefinitionHeader.iTestDefHeaderId";
				text6 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) not in ({0}) or isnull(QCTagDetails.iTagValue,0) not in ({0})", FConvert.GetInputObject(oRec.Inputs, 6)) : string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) in ({0}) or isnull(QCTagDetails.iTagValue,0) in ({0})", FConvert.GetInputObject(oRec.Inputs, 6)));
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select mQC_TestDefinitionHeader.iTestDefHeaderId,sDocNo,isnull(mQC_TestDefinitionHeader.iDate,0)TestDate,\r\n                                                    case when isnull(iVersion,0)=0 then  sTestName else  sTestName + ':(Version ' +cast(iVersion as varchar(10))+ ')' end sTestName, case when  bTestType=0 then 'Sample Check' else 'Total Check' end TestType, \r\n                                                    dSampleSize,isnull(PassedWareHouse.sName,'') sPassedWarehouseName,isnull(PassedWareHouse.sCode,'') sPassedWarehouseCode,\r\n                                                    isnull(FailedWareHouse.sName,'') sFailedWarehouseName,  isnull(FailedWareHouse.sCode,'') sFailedWarehouseCode,\r\n                                                    isnull(mQC_TestDefinitionHeader.iCreatedDate,0)iCreatedDate,isnull(mQC_TestDefinitionHeader.iCreatedTime,0)iCreatedTime,\r\n                                                    isnull(mQC_TestDefinitionHeader.iModifiedDate,0) iModifiedDate,\r\n                                                    isnull(mQC_TestDefinitionHeader.iModifiedTime,0) iModifiedTime,\r\n                                                    isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,\r\n                                                    Case when  mQC_TestDefinitionHeader.iAuthStatus=1 then 'Authorised' when mQC_TestDefinitionHeader.iAuthStatus=2 then 'Rejected'  else 'Not Authorised' End sAuthorised,\r\n                                                    Case when  isnull(mQC_TestDefinitionHeader.iStatus,0) =0 then 'In Active' when  isnull(mQC_TestDefinitionHeader.iStatus,0) =1 then  'Active' when isnull(mQC_TestDefinitionHeader.iStatus,0) =5 then 'Deleted' else '' end sStatus\r\n                                                    from mQC_TestDefinitionHeader\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tleft join mSec_Users createdUser on createdUser.iUserId=mQC_TestDefinitionHeader.iCreatedBy\r\n\t\t\t\t\t\t\t\t\t\t\t\t\tleft join mSec_Users ModifiedUser on ModifiedUser.iUserId=mQC_TestDefinitionHeader.iModifiedBy\r\n                                                    left join cCore_MasterDef MasterDef on MasterDef.iMasterTypeId = mQC_TestDefinitionHeader.iSeriesTag\r\n                                                    left join {9} PassedWareHouse on PassedWareHouse.iMasterId=mQC_TestDefinitionHeader.iPassedWarehouse\r\n                                                    {10} and PassedWareHouse.iTreeId=0\r\n                                                    left join {9} FailedWareHouse on FailedWareHouse.iMasterId=mQC_TestDefinitionHeader.iFailedWarehouse\r\n                                                    {10} and FailedWareHouse.iTreeId=0\r\n                                                    {11}\r\n                                                    where 1=1 {8}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, 4096, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6, text36, text37, text9);
			arrDefaultTables.Add(text36);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" iTestDefHeaderId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.QCRequisitionReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND iDate BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) == -1)
			{
				text2 = " ";
			}
			else if (FConvert.GetInputValue(oRec.Inputs, 3) == 0 || FConvert.GetInputValue(oRec.Inputs, 3) == 1)
			{
				text2 = $" and  (tempdata.cunt>0  and iFinalResult ={FConvert.GetInputValue(oRec.Inputs, 3)}) ";
			}
			else if (FConvert.GetInputValue(oRec.Inputs, 3) == 2)
			{
				text2 = $" and  (tempdata.cunt>0  and iFinalResult is null)";
			}
			else if (FConvert.GetInputValue(oRec.Inputs, 3) == 3)
			{
				text2 = " and  (isnull(tempdata.cunt,0)=0 and iFinalResult is null)";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text = $" and ReqBodyUser.iAllotedTo= {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				text6 = $" and isnull(iSeriesTag,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
			{
				text9 = $" left join tQC_RequisitionBody_0 ReqBody on ReqBody.iReqHeaderId = rh.iReqHeaderId left join (select iTestDefHeaderId,iTagValue from mQC_TestDefHeaderTagwiseDetails group by iTestDefHeaderId, iTagValue) QCTagDetails on QCTagDetails.iTestDefHeaderId=ReqBody.iTestDefHeaderId";
				text6 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? string.Format(" and isnull(iSeriesTag,0) not in ({0}) or isnull(QCTagDetails.iTagValue,0) not in ({0})", FConvert.GetInputObject(oRec.Inputs, 6)) : string.Format(" and isnull(iSeriesTag,0) in ({0}) or isnull(QCTagDetails.iTagValue,0) in ({0})", FConvert.GetInputObject(oRec.Inputs, 6)));
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			if (oRec.ExtraColumns != null && oRec.ExtraColumns.Length != 0)
			{
				text11 = GetExtraColumnDetails(oRec, ref sMRPExtraColumns, iCompanyId);
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select distinct rh.iReqHeaderId,sRequisitionNo DocumentNo,  isnull(rh.iDate,0) DocumentDate,vt.sName DocumentType,\r\n                        vrCore_Product.sName ProductName,dQtyAvailable Quantity,iTotalSampleSize MaxSampleSize,\r\n                        case when tempdata.cunt>0 then (case when iFinalResult=0 then 'Hold' when iFinalResult=1 then 'OK' \r\n                        else 'Inspection' end) else 'None' end RequisionResult,\r\n                        case when bClose = 0 then 'Not Closed' else 'Closed' end RequisitionStatus,\r\n                        isnull(rh.iCreatedDate,0) iCreatedDate,isnull(rh.iCreatedTime,0) iCreatedTime,\r\n                        isnull(rh.iModifiedDate,0) iModifiedDate,isnull(rh.iModifiedTime,0) iModifiedTime,\r\n                        --isnull(rh.iCreatedBy,0) iCreatedBy,isnull(rh.iModifiedBy,-1) iModifiedBy,\r\n                        isnull(CreatedUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,\r\n                        Case when  rh.iAuthStatus=1 then 'Authorised' when rh.iAuthStatus=2 then 'Rejected'  else 'Not Authorised' End sAuthorised,TransData.sVoucherNo sRefDocNumber, vrCore_Product.sCode ProductCode\r\n                        , vt.iVoucherType iVoucherType,vrCore_Product.iMasterId iProductId\r\n                        {9}\r\n                        FROM tQC_RequisitionHeader{0} rh\r\n                        inner join cCore_Vouchers{0} vt on vt.iVoucherType=iDocType \r\n                        inner join vrCore_Product on rh.iProductId=vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                        inner join tCore_Data{0} tCoreData on tCoreData.iTransactionId =rh.iTransactionId\r\n                        left join tCore_Batch{0} CoreBatch on CoreBatch.iBatchid=rh.iTransactionId\r\n                        left join mCore_ProductLanguage on mCore_ProductLanguage.iMasterId=vrCore_Product.iMasterId AND iLanguageId = {6}\r\n                        left join mSec_Users CreatedUser on CreatedUser.iUserId=rh.iCreatedBy\r\n                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=rh.iModifiedBy\r\n                        Left join(select count(th.itestHeaderId) cunt,th.iReqHeaderId \r\n                        from tQC_Testingheader{0} th \r\n                        where th.ireqHeaderId in (select iReqHeaderId from tQC_RequisitionHeader{0})GROUP BY th.iReqHeaderId)tempdata\r\n                        on tempdata.iReqHeaderId=rh.iReqHeaderId\r\n                        left  join (select distinct isnull( iAllotedTo,0) iAllotedTo,iReqHeaderId  from tQC_RequisitionBody{0}\r\n                        group by iAllotedTo,iReqHeaderId) ReqBodyUser on ReqBodyUser.iReqHeaderId=rh.iReqHeaderId \r\n                        left join (select distinct sVoucherNo,iTransactionId from tCore_Header{0} tHeader\r\n\t\t\t\t\t\t\tinner join tCore_Data{0} tdata on tHeader.iHeaderId=tdata.iHeaderId ) TransData on TransData.iTransactionId=rh.iTransactionId\r\n                        {8}\r\n                        {10}\r\n                        WHERE 1=1 {3} {4} {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), oRec.LanguageId, text6, text9, sMRPExtraColumns, text11);
			arrDefaultTables.Add("vrCore_Product");
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" rh.iReqHeaderId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.QCSampleCheckReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND TestingHeader.iDate BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text = $" and ReqBodyUser.iAllotedTo= {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text6 = $" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) = {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 5) != null && FConvert.GetInputObject(oRec.Inputs, 5).ToString().Length > 0)
			{
				text9 = $" left join mQC_TestDefinitionHeader on mQC_TestDefinitionHeader.iTestDefHeaderId = TestingHeader.iTestDefHeaderId left join (select top 1 iTestDefHeaderId,iTagValue from mQC_TestDefHeaderTagwiseDetails group by iTestDefHeaderId, iTagValue) QCTagDetails on QCTagDetails.iTestDefHeaderId=mQC_TestDefinitionHeader.iTestDefHeaderId";
				text6 = ((FConvert.GetInputValue(oRec.Inputs, 6) != 0) ? string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) not in ({0}) or isnull(QCTagDetails.iTagValue,0) not in ({0})", FConvert.GetInputObject(oRec.Inputs, 5)) : string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) in ({0}) or isnull(QCTagDetails.iTagValue,0) in ({0})", FConvert.GetInputObject(oRec.Inputs, 5)));
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			if (oRec.ExtraColumns != null && oRec.ExtraColumns.Length != 0)
			{
				text11 = GetExtraColumnDetails(oRec, ref sMRPExtraColumns, iCompanyId);
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select  iTestHeaderId,TestingHeader.sDocNo,isnull(TestingHeader.iDate,0) iDate,vrCore_Product.sName ProductName,vrCore_Product.sCode ProductCode,\r\n                        RequisitionHeader.sRequisitionNo,cast(dQtyTested as decimal(18,6))dQtyTested,\r\n                        Case when isnull(TestingHeader.iFinalResult,0)=0 then 'Failed' else 'Passed' end sFinalResult,sNarration\r\n                        ,isnull(TestingHeader.iCreatedDate,0)iCreatedDate,isnull(TestingHeader.iCreatedTime,0)iCreatedTime,\r\n                        isnull(TestingHeader.iModifiedDate,0) iModifiedDate,\r\n                        isnull(TestingHeader.iModifiedTime,0) iModifiedTime,\r\n                        isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,Case when  TestingHeader.iAuthStatus=1 then 'Authorised' when TestingHeader.iAuthStatus=2 then 'Rejected'  else 'Not Authorised' End sAuthorised,vt.iVoucherType iVoucherType\r\n                        {10} \r\n                        ,RequisitionHeader.iTransactionId, vrCore_Product.iMasterId iProductId                        \r\n                        FROM tQC_TestingHeader{0} TestingHeader  With(Readuncommitted)\r\n                        inner join tQC_RequisitionHeader{0}  RequisitionHeader on RequisitionHeader.iReqHeaderId =TestingHeader.iReqHeaderId\r\n                        inner join cCore_Vouchers{0} vt on vt.iVoucherType=RequisitionHeader.iDocType                         \r\n                        inner join vrCore_Product on TestingHeader.iProductId=vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                        inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                        left  join (select isnull( iAllotedTo,0) iAllotedTo,iReqHeaderId,iTestDefHeaderId  from tQC_RequisitionBody{0} group by iAllotedTo,iReqHeaderId,iTestDefHeaderId) ReqBodyUser on ReqBodyUser.iReqHeaderId=TestingHeader.iReqHeaderId and ReqBodyUser.iTestDefHeaderId=TestingHeader.iTestDefHeaderId\r\n                        --left join tCore_Batch{0} batch on batch.iBatchid=RequisitionHeader.iTransactionId                      \r\n                        left join mSec_Users createdUser on createdUser.iUserId=TestingHeader.iCreatedBy\r\n                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=TestingHeader.iModifiedBy\r\n                        {9}\r\n                        {11}\r\n                        WHERE 1=1 {3} {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6, oRec.LanguageId, text9, sMRPExtraColumns, text11);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" iTestHeaderId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.QCTotalCheckReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND TotalTestHeader.iDate BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text = $" and ReqBodyUser.iAllotedTo= {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text6 = $" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) = {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 5) != null && FConvert.GetInputObject(oRec.Inputs, 4).ToString().Length > 0)
			{
				text9 = $" left join mQC_TestDefinitionHeader on mQC_TestDefinitionHeader.iTestDefHeaderId = TotalTestHeader.iTestDefHeaderId left join (select top 1 iTestDefHeaderId,iTagValue from mQC_TestDefHeaderTagwiseDetails group by iTestDefHeaderId, iTagValue) QCTagDetails on QCTagDetails.iTestDefHeaderId=mQC_TestDefinitionHeader.iTestDefHeaderId";
				text6 = ((FConvert.GetInputValue(oRec.Inputs, 6) != 0) ? string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) not in ({0}) or isnull(QCTagDetails.iTagValue,0) not in ({0})", FConvert.GetInputObject(oRec.Inputs, 5)) : string.Format(" and isnull(mQC_TestDefinitionHeader.iSeriesTag,0) in ({0}) or isnull(QCTagDetails.iTagValue,0) in ({0})", FConvert.GetInputObject(oRec.Inputs, 5)));
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select  iTotalTestHeaderId,TotalTestHeader.sDocNo,\r\n                                    TotalTestHeader.iDate iDate,PL.sName ProductName,RequisitionHeader.sRequisitionNo,TestdefHeader.sTestName,\r\n                                    cast(fQtyTested as decimal(18,6))fQtyTested,cast(fQtyPassed as decimal(18,6))fQtyPassed,cast(fQtyFailed as decimal(18,6))fQtyFailed,\r\n                                    Case when isnull(TotalTestHeader.iFinalResult,0)=0 then 'Failed' else 'Passed' end sFinalResult,Employee.sName as CheckedBy\r\n                                    ,isnull(TotalTestHeader.iCreatedDate,0)iCreatedDate,isnull(TotalTestHeader.iCreatedTime,0)iCreatedTime,\r\n                                    isnull(TotalTestHeader.iModifiedDate,0) iModifiedDate,isnull(TotalTestHeader.iModifiedTime,0) iModifiedTime,\r\n                                    isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,\r\n                                    Case when  TotalTestHeader.iAuthStatus=1 then 'Authorised' when TotalTestHeader.iAuthStatus=2 then 'Rejected'  else 'Not Authorised' End sAuthorised\r\n                                    , vt.iVoucherType iVoucherType, vmCore_Product.iMasterId iProductId\r\n                                    from tQC_TotalTestHeader{0} TotalTestHeader\r\n                                    inner join tQC_RequisitionHeader{0}  RequisitionHeader on RequisitionHeader.iReqHeaderId =TotalTestHeader.iReqHeaderId\r\n                                    inner join cCore_Vouchers{0} vt on vt.iVoucherType=RequisitionHeader.iDocType \r\n                                    inner join mQC_TestDefinitionHeader TestdefHeader on TestdefHeader.iTestDefHeaderId =TotalTestHeader.iTestDefHeaderId\r\n                                    inner join vmCore_Product on TotalTestHeader.iProductId=vmCore_Product.iMasterId And vmCore_Product.iTreeId = 0\r\n                                    inner JOIN mCore_ProductLanguage PL ON vmCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                                    inner join vmPay_Employee Employee on Employee.iMasterId = TotalTestHeader.iCheckedBy\r\n                                    --left join tCore_Batch{0} batch on batch.iBatchid=RequisitionHeader.iTransactionId \r\n                                    left join mSec_Users createdUser on createdUser.iUserId=TotalTestHeader.iCreatedBy\r\n                                    left join mSec_Users ModifiedUser on ModifiedUser.iUserId=TotalTestHeader.iModifiedBy\r\n                                    {9}\r\n                                    left  join (select distinct isnull( iAllotedTo,0) iAllotedTo,iReqHeaderId,iTestDefHeaderId  from tQC_RequisitionBody{0} group by iAllotedTo,iReqHeaderId,iTestDefHeaderId) ReqBodyUser on ReqBodyUser.iReqHeaderId=TotalTestHeader.iReqHeaderId and ReqBodyUser.iTestDefHeaderId=TotalTestHeader.iTestDefHeaderId\r\n                                    where 1=1 {3} {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6, oRec.LanguageId, text9);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" iTotalTestHeaderId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.QCQuantityBreakUp:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND A.iDate BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
			{
				text = $" and ReqBodyUser.iAllotedTo= {FConvert.GetInputValue(oRec.Inputs, 3)}";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select distinct A.iBreakUpId, sDocNo,A.iDate,vmCore_Product.sName,b.sRequisitionNo,a.fQuantity,a.fPassedQuantity,a.fFailedQuantity,\r\n                                        Employee.sName,isnull(b.bClose,0) bClose\r\n                                        ,isnull(A.iCreatedDate,0)iCreatedDate,isnull(A.iCreatedTime,0)iCreatedTime,\r\n                                        isnull(A.iModifiedDate,0) iModifiedDate,isnull(A.iModifiedTime,0) iModifiedTime,\r\n                                        isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,\r\n                                        Case when  A.iAuthStatus=1 then 'Authorised' when A.iAuthStatus=2 then 'Rejected'  else 'Not Authorised' End sAuthorised\r\n                                        from tQC_QuantityBreakupHeader{0} A\r\n                                        inner join tQC_RequisitionHeader{0} B on A.iReqHeaderId=b.iReqHeaderId \r\n                                        inner join vmCore_Product on vmCore_Product.iMasterId=A.iProductId\r\n                                        inner JOIN mCore_ProductLanguage PL ON vmCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                                        left join vmPay_Employee Employee on Employee.iMasterId = a.iApprovedBy\r\n                                        left join mSec_Users createdUser on createdUser.iUserId=A.iCreatedBy\r\n                                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=A.iModifiedBy\r\n                                        left  join (select isnull( iAllotedTo,0) iAllotedTo,iReqHeaderId  from tQC_RequisitionBody{0}\r\n                                        group by iAllotedTo,iReqHeaderId) ReqBodyUser on ReqBodyUser.iReqHeaderId=A.iReqHeaderId \r\n                                        where 1=1 {3} {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6, oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" iBreakUpId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.QCBreakDown:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" AND A.iStartDate BETWEEN {FConvert.GetInputValue(oRec.Inputs, 1)} AND {FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select id,b.sName WorkCenterName,B.sCode WorkCenterCode, iWorkCenterId,iStartDate,iStartTime,iEndDate,iEndTime,A.sRemarks,isnull(mCal_ShiftHeader.sShiftName,'') sShiftName,A.iCreatedDate,A.iCreatedTime,isnull(createdUser.sLoginName,'') CreatedBy,A.iModifiedDate,\r\n\t                    A.iModifiedTime,\tisnull(ModifiedUser.sLoginName,'') ModifiedBy\r\n\t                    from tQC_MachineBreakdown{0} A\r\n                        left join mCal_ShiftHeader on A.iShiftId= mCal_ShiftHeader.iShiftId \r\n                        inner join mMrp_WorkCenter B on B.iMasterId=A.iWorkCenterId\r\n\t                    left join mSec_Users createdUser on createdUser.iUserId=A.iCreatedBy\r\n                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=A.iModifiedBy where 1=1 {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = $" id desc";
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.MRPBillofMaterial:
		{
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
				{
					text = $" and BH.iFrom >= {FConvert.GetInputValue(oRec.Inputs, 1)} and BH.iTo <={FConvert.GetInputValue(oRec.Inputs, 2)}";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 3) == -1)
				{
					text2 = $" and isnull(BVH.iAuthStatus,0)= isnull(BVH.iAuthStatus,0)";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) < 3)
				{
					text2 = $" and isnull(BVH.iAuthStatus,0)= {FConvert.GetInputValue(oRec.Inputs, 3)}";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) != 5)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != 3) ? " and isnull(BVH.iStatus,0)= 0" : " and isnull(BVH.iStatus,0)= 1");
				}
				else
				{
					text10 = "Inner Join (Select Max(iVersion) Ver,sName BOMName,sCode BOMCode,Max(iBomId) iBomId From mMRP_BomHeader Group by sName,sCode ) as BOMV on BOMV.iBomId = BH.iBomId";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
				{
					text2 += $" and isnull(BH.iTagId,0) = {FConvert.GetInputValue(oRec.Inputs, 4)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 5) != null && FConvert.GetInputObject(oRec.Inputs, 5).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 6) != 0) ? (text2 + $" and isnull(BH.iTagValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 5)})") : (text2 + $" and isnull(BH.iTagValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 5)})"));
				}
				if (FConvert.GetInputValue(oRec.Inputs, 7) > 0 || oRec.UserId == 1)
				{
					text7 = "";
					text8 = "";
				}
				else
				{
					text7 = "left join (select distinct vmCore_Product.iMasterId, mMRP_BomHeader.bPhantom,vmCore_Product.iBOM from vmCore_Product inner join mMRP_BomVariantHeader on vmCore_Product.iBOM=mMRP_BomVariantHeader.iVariantId inner join mMRP_BomHeader on mMRP_BomHeader.iBomId=mMRP_BomVariantHeader.iBomId) Phantom on Phantom.iBOM=PR.iBOM and Phantom.iMasterId=BB.iProductId";
					text8 = "and isnull(Phantom.bPhantom,0) <> 1";
				}
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			int preferenceValue = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.MRP, 28);
			string empty = string.Empty;
			string text12 = string.Empty;
			string text13 = ",'' BomTagName,'' BomTagCode ";
			if (preferenceValue > 0)
			{
				string text14 = $"Select 'm' + sModule + '_' + sMasterName as BOMTagName From cCore_MasterDef where iMasterTypeId = {preferenceValue}";
				empty = Convert.ToString(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text14) : text14));
				text12 = " Left Join " + empty + " BTT On BTT.iMasterId = BH.iTagValue ";
				text13 = " ,BTT.sName BomTagName,BTT.sCode BomTagCode ";
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select  \r\n                        BH.iBomId, BVH.iVariantId,Case When BVH.sName = 'Default' then BH.sName Else BVH.sName End sBOMName,\r\n                        BH.sCode sBomCode,PL.sName ProductName,PR.sCode ProductCode,\r\n                        case when  BH.iType = 0 then 'Production'\r\n                        when  BH.iType = 1 then 'Assembly'  when  BH.iType = 2 then 'Engineering'\r\n                        when  BH.iType = 3 then 'Univarsal' when BH.iType = 4 then 'Plant Maintance'\r\n                        when BH.iType = 5 then 'Sales Destribution'\r\n                        when BH.iType = 6 then 'Template' \r\n                        when BH.iType = 7 then 'Non-Stocked Assembly'\r\n                        else '' end as sType,BH.sDescription,BH.iVersion Version,isnull(BH.sRemarks,'') sRemarks,\r\n                        Case when isnull(BVH.iAuthStatus,0)=0 then 'Authorise Pending' when BVH.iAuthStatus=1 then 'Authorised' when BVH.iAuthStatus=2 then 'Rejected' else '' end as sAuthStatus,\r\n                        --isnull(MasterDef.sMasterName,'') TagValue,\r\n                        dbo.fCore_IntToDateTime(isnull(BH.iCreatedDate,0)) iCreatedDate, --0 iCreatedTime,\r\n                        dbo.fCore_IntToDateTime(isnull(BH.iModifiedDate,0)) iModifiedDate,\r\n                        --0 iModifiedTime,\r\n                        isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy, Case when isnull(sUser.sUserName,'')='' then '' else sUser.sUserName end as AuthorizedUser\r\n                        {11}\r\n                        FROM mMRP_BomVariantHeader BVH  \r\n                        inner Join mMRP_BomHeader BH On BH.iBomId = BVH.iBomId  \r\n                        Inner Join mMRP_BOMBody BB On BB.iVariantId = BVH.iVariantId And BB.bInput = 0 And bMainOutPut = 1 and BH.iStatus<5 and BVH.iStatus<>5 \r\n                        inner join vmCore_Product PR on BB.iProductId=PR.iMasterId And PR.iTreeId = 0\r\n                        inner JOIN mCore_ProductLanguage PL ON PR.iMasterId = PL.iMasterId And  PL.iLanguageId={9}\r\n                        inner join vrCore_Product on vrCore_Product.iMasterId = BB.iProductId And vrCore_Product.iTreeId = 0\r\n                        {10}\r\n                        left join mSec_Users createdUser on createdUser.iUserId=BH.iCreatedBy\r\n                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=BH.iModifiedBy\r\n                        --left join cCore_MasterDef MasterDef on MasterDef.iMasterTypeId = BH.iTagId\r\n                        {12}\r\n                        LEFT Join (Select iDocumentId,STRING_AGG( 'L' + convert(varchar,iLevel+1) + ' : ' + sUserName ,', ') sUserName from mCore_AuthorizationDetails_0 AD\r\n\t                        JOIN  tCore_ProductionAuth PA on AD.iAuthorizationDetailId = PA.iAuthNodeId\r\n\t                        JOIN tCore_productionAuthUser AU on AU.iAuthId = PA.iAuthId\r\n\t                        JOIN mSec_Users sUser ON AU.iRoleOrUserId = sUser.iUserId\r\n\t                        where AU.iStatus = 1\r\n\t                        GROUP BY iDocumentId \r\n\t                    ) sUser On sUser.iDocumentId  = BVH.iVariantId\r\n                        {6}\r\n                        WHERE  1=1 --and BVH.sName = 'Default'\r\n                        {3} {4} {5}", oRec.StartingDate, oRec.EndingDate, text, text2, text6, text8, text7, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", oRec.LanguageId, text10, text13, text12);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" BVH.iVariantId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.MRPBOMProcess:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" and BH.iFrom >= {FConvert.GetInputValue(oRec.Inputs, 1)} and BH.iTo <={FConvert.GetInputValue(oRec.Inputs, 2)}";
			}
			text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != -1) ? $" and isnull(BVH.iAuthStatus,0)= {FConvert.GetInputValue(oRec.Inputs, 3)}" : $" and isnull(BVH.iAuthStatus,0)= isnull(BVH.iAuthStatus,0)");
			if (FConvert.GetInputValue(oRec.Inputs, 4) > 0)
			{
				text2 += $" and isnull(BH.iTagId,0) = {FConvert.GetInputValue(oRec.Inputs, 4)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 5) != null && FConvert.GetInputObject(oRec.Inputs, 5).ToString().Length > 0)
			{
				text2 = ((FConvert.GetInputValue(oRec.Inputs, 6) != 0) ? (text2 + $" and isnull(BH.iTagValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 5)})") : (text2 + $" and isnull(BH.iTagValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 5)})"));
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 7) > 0 || oRec.UserId == 1)
				{
					text7 = "";
					text8 = "";
				}
				else
				{
					text7 = "left join (select distinct vmCore_Product.iMasterId, mMRP_BomHeader.bPhantom,vmCore_Product.iBOM from vmCore_Product inner join mMRP_BomVariantHeader on vmCore_Product.iBOM=mMRP_BomVariantHeader.iVariantId inner join mMRP_BomHeader on mMRP_BomHeader.iBomId=mMRP_BomVariantHeader.iBomId) Phantom on Phantom.iBOM=PR.iBOM and Phantom.iMasterId=BB.iProductId";
					text8 = "and isnull(Phantom.bPhantom,0) <> 1";
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select  \r\n                        BH.iBomId, BVH.iVariantId,Case When BVH.sName = 'Default' then BH.sName Else BVH.sName End sBOMName,\r\n                        BH.sCode sBomCode,PL.sName ProductName,PR.sCode ProductCode,\r\n                        case when  BH.iType = 0 then 'Production'\r\n                        when  BH.iType = 1 then 'Assembly'  when  BH.iType = 2 then 'Engineering'\r\n                        when  BH.iType = 3 then 'Univarsal' when BH.iType = 4 then 'Plant Maintance'\r\n                        when BH.iType = 5 then 'Sales Destribution'\r\n                        when BH.iType = 7 then 'Non-Stocked Assembly'\r\n                        else '' end as sType,BH.sDescription,BH.iVersion Version,\r\n                        case when isnull(mMRP_RoutingHeader.iCreatedDate,0)>0 then\r\n                        dbo.fCore_IntToDateTime(dbo.fCore_DateTimeToInt( dbo.IntToDate(isnull(mMRP_RoutingHeader.iCreatedDate,0))+' '+ dbo.fCore_IntToTime(isnull(mMRP_RoutingHeader.iCreatedTime,0))))\r\n                        else '' end iCreatedDate,\r\n\r\n                        case when \r\n                        isnull(mMRP_RoutingHeader.iModifiedDate,0) >0 then\r\n                        dbo.fCore_IntToDateTime(dbo.fCore_DateTimeToInt( dbo.IntToDate(isnull(mMRP_RoutingHeader.iModifiedDate,0))+' '+ dbo.fCore_IntToTime(isnull(mMRP_RoutingHeader.iModifiedTime,0))))\r\n                        else \r\n                        case when isnull(mMRP_RoutingHeader.iCreatedDate,0)>0 then\r\n                        dbo.fCore_IntToDateTime(dbo.fCore_DateTimeToInt( dbo.IntToDate(isnull(mMRP_RoutingHeader.iCreatedDate,0))+' '+ dbo.fCore_IntToTime(isnull(mMRP_RoutingHeader.iCreatedTime,0))))\r\n                        else '' end\r\n                        end iModifiedDate,\r\n\r\n                        --case when isnull(mMRP_RoutingHeader.iModifiedDate,0) >0 then\r\n                        --dbo.fCore_IntToDateTime(dbo.fCore_DateTimeToInt( dbo.IntToDate(isnull(mMRP_RoutingHeader.iModifiedDate,0))+' '+ dbo.fCore_IntToTime(isnull(mMRP_RoutingHeader.iModifiedTime,0))))\r\n                        --else \r\n                        --dbo.fCore_IntToDateTime(dbo.fCore_DateTimeToInt( dbo.IntToDate(isnull(mMRP_RoutingHeader.iCreatedDate,0))+' '+ dbo.fCore_IntToTime(isnull(mMRP_RoutingHeader.iCreatedTime,0))))\r\n                        --end iModifiedDate,\r\n\r\n                        isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,\r\n                        case when  isnull(mMRP_RoutingHeader.bBOMProcess,0)=1 then 'BOM Process Exists' else 'BOM Process Not Exists' end BOMProcessStatus\r\n                        From mMRP_BomVariantHeader BVH  \r\n                        inner Join mMRP_BomHeader BH On BH.iBomId = BVH.iBomId  \r\n                        Inner Join mMRP_BOMBody BB On BB.iVariantId = BVH.iVariantId And BB.bInput = 0 And bMainOutPut = 1 and BVH.iStatus<5 and BH.iStatus<5 \r\n                        inner join vmCore_Product PR on BB.iProductId=PR.iMasterId And PR.iTreeId = 0\r\n                        inner JOIN mCore_ProductLanguage PL ON PR.iMasterId = PL.iMasterId And  PL.iLanguageId={7}\r\n                        left join mMRP_RoutingHeader on mMRP_RoutingHeader.iBomId=BVH.iVariantId and isnull( mMRP_RoutingHeader.bBOMProcess,0)!=0\r\n                        left join mSec_Users createdUser on createdUser.iUserId=BH.iCreatedBy\r\n                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=BH.iModifiedBy\r\n                        {6}\r\n                        where  1=1  {3} {4} {5}", oRec.StartingDate, oRec.EndingDate, text, text2, text6, text8, text7, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" BVH.iVariantId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.MRPProductionOrderBasic:
		{
			_ = string.Empty;
			bool flag4 = false;
			string text24 = string.Empty;
			_ = string.Empty;
			_ = string.Empty;
			_ = string.Empty;
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
				{
					text = $" and iDueDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)}  ";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 3) == -1)
				{
					text2 = " and  iOrderStatus in (0,1,2,3,4,6)";
				}
				else
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != -2) ? $"and iOrderStatus={FConvert.GetInputValue(oRec.Inputs, 3)}" : " and  iOrderStatus in (0,1,2,3)");
				}
				text2 += " and (bMRPDone is null or  bMRPDone = 1 ) and  isnull(bMRPOrder,1)=0";
				flag4 = true;
				if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
				{
					text2 += $" and isnull(header.iTagFilterId,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? (text2 + $" and isnull(header.iTagFilterValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 6)})") : (text2 + $" and isnull(header.iTagFilterValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 6)})"));
				}
				if (FConvert.GetInputObject(oRec.Inputs, 8) != null && FConvert.GetInputObject(oRec.Inputs, 8).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 9) != 0) ? (text2 + $" and isnull(vrCore_Product.iMasterId,0) not in ({FConvert.GetInputObject(oRec.Inputs, 8)})") : (text2 + $" and isnull(vrCore_Product.iMasterId,0) in ({FConvert.GetInputObject(oRec.Inputs, 8)})"));
				}
				if (FConvert.GetInputValue(oRec.Inputs, 12) <= 0 && oRec.UserId != 1)
				{
					text8 = " and isnull(BH.bPhantom,0) <> 1";
				}
				else
				{
					text8 = ((FConvert.GetInputValue(oRec.Inputs, 13) <= 0) ? "" : " and isnull(BH.bPhantom,0) = 1 ");
				}
				text24 = Convert.ToString((from m in oRec.Inputs
					where m.ID == 14
					select m.Value).FirstOrDefault());
			}
			int invTagId = _focus.company(m_iCompanyId).invTagId;
			if (flag4 && FConvert.GetInputValue(oRec.Inputs, 11) > 1)
			{
				string text25 = "";
				if (invTagId > 2)
				{
					bool bExclude = false;
					string text26 = new COptionbase().CheckRoleMasters(FConvert.GetInputValue(oRec.Inputs, 11), invTagId.ToString(), IsMasterName: false, 1, iCompanyId, ref bExclude);
					if (text26 != string.Empty)
					{
						text25 = ((!bExclude) ? $" AND iWareHouseId in ({text26})" : $" AND iWareHouseId not in ({text26})");
					}
				}
				text2 += text25;
			}
			string text27 = string.Empty;
			string text28 = string.Empty;
			if (invTagId > 0)
			{
				string text29 = $"Select 'vr' + sModule + '_' + sMasterName as InvTagName From cCore_MasterDef where iMasterTypeId = {invTagId}";
				text27 = Convert.ToString(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text29) : text29));
				text28 = " Inner Join " + text27 + " On " + text27 + ".iMasterId = case  when header.iWareHouseId IS NULL then 0 when header.iWareHouseId=-1 then 0 else header.iWareHouseId end and " + text27 + ".iTreeId=0";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId, text27);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			List<string> list2 = new List<string>();
			if (oRec.ExtraColumns != null && oRec.ExtraColumns.Length != 0)
			{
				list2.AddRange(new List<string> { "vrCore_Product", "vrCore_Units", "vrCore_Account" });
				if (!string.IsNullOrEmpty(text27))
				{
					list2.Add(text27);
				}
				bool bMRPExtraFields2 = true;
				int iFieldCount2 = new RDDefault().GetMRPScreenColumns(oRec.ReportId, (int)oRec.SubReportId).Length + 1;
				text11 = GetExtraColumnDetails(oRec, ref sMRPExtraColumns, iCompanyId, list2, bMRPExtraFields2, iFieldCount2);
			}
			standardQuery = new StandardQuery();
			string query = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                                iStartDate, iEndDate,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                                when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                                then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                                case when isnull( bCancelled,0)=0 then case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                                when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                                when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                                when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO   \r\n                                ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                                case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                                case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime,                                                                       \r\n                                isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                                ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo,isnull(header.iDate,0)iOrderDate,\r\n                                Case when isnull(header.iAuthStatus,0)=0 then 'Authorise Pending' when header.iAuthStatus=1 then 'Authorised' when header.iAuthStatus=2 then 'Rejected' else '' end as AuthStatus,\r\n                                Case when isnull(sUser.sUserName,'')='' then '' else sUser.sUserName end as AuthorizedUser\r\n                                {11}    \r\n                                FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                                inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                                Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0 when header.iWareHouseId=-1 then 0 else header.iWareHouseId end   \r\n                                inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId and vrCore_Product.iStatus <> 5 And vrCore_Product.iTreeId = 0                                                    \r\n                                inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={7}\r\n                                inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                                inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1                                   \r\n                                {10}\r\n                                left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                                left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy                                                    \r\n                                left join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0\r\n                                left join vrCore_Account on vrCore_Account.iMasterId = header.iCustomer and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0 and vrCore_Account.iTreeId=0                                                                                                                                                            \r\n                                left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId\r\n                                LEFT JOIN (Select iDocumentId,STRING_AGG( 'L' + convert(varchar,iLevel+1) + ' : ' + sUserName ,', ') sUserName from mCore_AuthorizationDetails{0} AD\r\n                                    JOIN  tCore_ProductionAuth PA on AD.iAuthorizationDetailId = PA.iAuthNodeId\r\n                                    JOIN tCore_productionAuthUser AU on AU.iAuthId = PA.iAuthId\r\n                                    JOIN mSec_Users sUser ON AU.iRoleOrUserId = sUser.iUserId\r\n                                    WHERE AU.iStatus = 1\r\n                                    GROUP BY iDocumentId \r\n                                ) sUser On sUser.iDocumentId  = header.iProdOrderId                                                                                                        \r\n                                {9}  \r\n                                {12}\r\n                                where 1=1   {4} {6} {8}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text6, oRec.LanguageId, text8, text24, text28, sMRPExtraColumns, text11);
			standardQuery.Query = query;
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" header.iProdOrderId Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.MRPProductionOrderMRP1:
		{
			string text38 = string.Empty;
			bool? flag6 = null;
			bool flag7 = false;
			string text39 = string.Empty;
			string text40 = string.Empty;
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
				{
					text = $" and iDueDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)}  ";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 3) == -1)
				{
					text2 = " and  iOrderStatus in (0,1,2,3,4,6)";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) == -2)
				{
					text2 = " and  iOrderStatus in (0,1,2,3)";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) == 7)
				{
					flag6 = true;
				}
				else
				{
					text2 = $" and iOrderStatus={FConvert.GetInputValue(oRec.Inputs, 3)}";
					flag6 = false;
				}
				if (FConvert.GetInputValue(oRec.Inputs, 4) == 0)
				{
					text3 = " and isnull(bMRPDone,0) = 1 ";
					text4 = " and isnull(bMRPDone,0) = 0";
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != 7) ? (text2 + " and isnull(bMRPOrder,1)=1") : (text2 + " and isnull(bMRPDone,0) = 0"));
					text38 = " left join (select distinct iProdOrderId from tMRP_PlannedDependentRequirement_0 where bMPS<>1 ) a\r\n                                    on a.iProdOrderId=header.iProdOrderId";
					text40 = " left join (\r\n                                    select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                    where bOutSourced=0\r\n                                    group by iProdOrderId) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n\r\n                                    left join (\r\n                                    select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                    where bOutSourced=1\r\n                                    group by iProdOrderId\r\n                                    ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId \r\n                                    ";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
				{
					text2 += $" and isnull(header.iTagFilterId,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? (text2 + $" and isnull(header.iTagFilterValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 6)})") : (text2 + $" and isnull(header.iTagFilterValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 6)})"));
				}
				if (FConvert.GetInputObject(oRec.Inputs, 8) != null && FConvert.GetInputObject(oRec.Inputs, 8).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 9) != 0) ? (text2 + $" and isnull(vrCore_Product.iMasterId,0) not in ({FConvert.GetInputObject(oRec.Inputs, 8)})") : (text2 + $" and isnull(vrCore_Product.iMasterId,0) in ({FConvert.GetInputObject(oRec.Inputs, 8)})"));
				}
				if (FConvert.GetInputValue(oRec.Inputs, 12) <= 0 && oRec.UserId != 1)
				{
					text8 = " and isnull(BH.bPhantom,0) <> 1";
				}
				else
				{
					text8 = ((FConvert.GetInputValue(oRec.Inputs, 13) <= 0) ? "" : " and isnull(BH.bPhantom,0) = 1");
				}
				text39 = Convert.ToString((from m in oRec.Inputs
					where m.ID == 14
					select m.Value).FirstOrDefault());
			}
			if (oRec.CodeName != null)
			{
				_ = oRec.CodeName != string.Empty;
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			string text41 = string.Empty;
			if (oRec.UserId > 1 && !flag7)
			{
				text41 = GetPOFilterOnUser(oRec.UserId, iCompanyId);
				if (!string.IsNullOrEmpty(text41))
				{
					text41 = $"Inner Join ({text41}) FillTags ON FillTags.iProdOrderId = Header.iProdOrderId";
				}
			}
			List<string> list5 = new List<string>();
			if (oRec.ExtraColumns != null && oRec.ExtraColumns.Length != 0)
			{
				list5.AddRange(new List<string> { "vrCore_Product", "vrCore_Units", "vrCore_Account" });
				bool bMRPExtraFields3 = true;
				int iFieldCount3 = new RDDefault().GetMRPScreenColumns(oRec.ReportId, (int)oRec.SubReportId).Length + 1;
				text11 = GetExtraColumnDetails(oRec, ref sMRPExtraColumns, iCompanyId, list5, bMRPExtraFields3, iFieldCount3);
			}
			standardQuery = new StandardQuery();
			text2 = ((flag6 != false) ? (text2 + text4) : (text2 + text3));
			string text42 = string.Empty;
			if (!flag7)
			{
				text42 = " ,isnull(InHouse.InHosueQty,0) InHosueQty ,isnull(OutSource.OutSourceQty,0) OutSourceQty,CASE WHEN ISNULL(header.iOrderStatus, 0) >= 2 THEN CASE WHEN InHouse.LatestReleasedDate IS NULL THEN OutSource.LatestReleasedDate WHEN OutSource.LatestReleasedDate IS NULL THEN InHouse.LatestReleasedDate WHEN InHouse.LatestReleasedDate > OutSource.LatestReleasedDate THEN InHouse.LatestReleasedDate ELSE OutSource.LatestReleasedDate END ELSE   '' END AS LatestReleasedDt";
				text40 = " left join (\r\n                                select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=0\r\n                                group by iProdOrderId) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n\r\n                                left join (\r\n                                select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=1\r\n                                group by iProdOrderId\r\n                                ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId \r\n                                ";
			}
			string text43 = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                                iStartDate, iEndDate,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                                when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                                then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                                case when isnull( bCancelled,0)=0 then case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                                when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                                when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                                when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO   \r\n                                ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                                case when isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                                case when isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime, \r\n                                --isnull(header.iModifiedDate,0) iModifiedDate,\r\n                                --isnull(header.iModifiedTime,0) iModifiedTime,                       \r\n                                isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                                ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo\r\n                                {13}\r\n                                ,isnull(header.iDate,0)iOrderDate\r\n                                {14}\r\n                                FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                                inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                                Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0  \r\n                                when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                                {6}\r\n                                inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                                inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                                inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                                inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1                                                     \r\n                                left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                                left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy        \r\n                                left join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0\r\n                                left join vrCore_Account on vrCore_Account.iMasterId = header.iVendorId and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0 and vrCore_Account.iTreeId=0\r\n                                left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId                                                    \r\n                                {12}                                                    \r\n                                {11}\r\n                                {9}\r\n                                {15}\r\n                                where 1=1    {4} {7} {10}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text5, text6, oRec.LanguageId, text41, text8, text39, text40, text42, sMRPExtraColumns, text11);
			text2 += text3;
			if (flag)
			{
				text2 = ((flag6 != true) ? "and iOrderStatus in (0,1,2,3,4,6) and isnull(bMRPOrder,1)=1 and isnull(bMRPDone,0) = 1" : "and iOrderStatus in (0,1,2,3,4,6) and isnull(bMRPOrder,1)=1 and isnull(bMRPDone,0) = 0");
				if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
				{
					text2 += $" and isnull(header.iTagFilterId,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? (text2 + $" and isnull(header.iTagFilterValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 6)})") : (text2 + $" and isnull(header.iTagFilterValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 6)})"));
				}
			}
			string text21 = (standardQuery.Query = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                                        iStartDate, iEndDate,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                                        when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                                        then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                                        case when isnull( bCancelled,0)=0 then case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                                        when isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                                        when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                                        when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO \r\n                                        ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                                        case when isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                                        case when isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime,    \r\n                                        isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                                        ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo,\r\n                                        isnull(InHouse.InHosueQty,0) InHosueQty ,isnull(OutSource.OutSourceQty,0) OutSourceQty,\r\n                                        --MAX(\r\n                                                CASE \r\n                                                WHEN ISNULL(header.iOrderStatus, 0) >= 2 THEN \r\n                                                CASE\r\n                                                WHEN InHouse.LatestReleasedDate IS NULL THEN OutSource.LatestReleasedDate\r\n                                                WHEN OutSource.LatestReleasedDate IS NULL THEN InHouse.LatestReleasedDate\r\n                                                WHEN InHouse.LatestReleasedDate > OutSource.LatestReleasedDate THEN InHouse.LatestReleasedDate\r\n                                                ELSE OutSource.LatestReleasedDate\r\n                                                END\r\n                                                ELSE \r\n                                                    ''\r\n                                                END\r\n                                        --) \r\n                                        ,isnull(header.iDate,0)iOrderDate\r\n                                        {13}\r\n                                        FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                                        inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                                        {8}\r\n                                        Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0  \r\n                                        when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                                        {6}\r\n                                        inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                                        inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={9}\r\n                                        inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                                        inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1                                                         \r\n                                        left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                                        left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy        \r\n                                        left join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0        \r\n                                        left join vrCore_Account on vrCore_Account.iMasterId = header.iVendorId and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0 and vrCore_Account.iTreeId=0\r\n                                        left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId\r\n                                        left join (\r\n                                            select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd{0}\r\n                                            where bOutSourced=0\r\n                                            group by iProdOrderId\r\n                                        ) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n                                        left join (\r\n                                            select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd{0}\r\n                                            where bOutSourced=1\r\n                                            group by iProdOrderId\r\n                                        ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId                                                         \r\n                                        {12}\r\n                                        {10}\r\n                                        {14}\r\n                                        where 1=1 {4} {7} {11}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text5, text6, text38, oRec.LanguageId, text41, text8, text39, sMRPExtraColumns, text11));
			string text45 = text21;
			if (!flag6.HasValue)
			{
				standardQuery.Query = text43 + " UNION ALL " + text45;
			}
			else if (flag6 == false)
			{
				standardQuery.Query = text43;
			}
			else
			{
				standardQuery.Query = text45;
			}
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" header.iProdOrderId Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.MRPProductionOrder:
		{
			string text15 = string.Empty;
			bool? flag2 = null;
			bool flag3 = false;
			string text16 = string.Empty;
			string text17 = string.Empty;
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
				{
					text = $" and iDueDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)}  ";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 3) == -1)
				{
					text2 = "and  iOrderStatus in (0,1,2,3,4,6)";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) == -2)
				{
					text2 = "and  iOrderStatus in (0,1,2,3)";
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 3) == 7)
				{
					flag2 = true;
				}
				else
				{
					text2 = $"and iOrderStatus={FConvert.GetInputValue(oRec.Inputs, 3)}";
					flag2 = false;
				}
				if (FConvert.GetInputValue(oRec.Inputs, 4) == 0)
				{
					text3 = " and isnull(bMRPDone,0) = 0 ";
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != 7) ? (text2 + " and  isnull(bMRPOrder,1)=1") : (text2 + " and isnull(bMRPDone,0) = 0"));
					text15 = "  left join (select distinct iProdOrderId from tMRP_PlannedDependentRequirement_0 where bMPS<>1 ) a\r\n                                on a.iProdOrderId=header.iProdOrderId";
					text17 = " left join (\r\n                                select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=0\r\n                                group by iProdOrderId) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n\r\n                                left join (\r\n                                select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=1\r\n                                group by iProdOrderId\r\n                                ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId \r\n                                ";
				}
				if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
				{
					text2 += $" and isnull(header.iTagFilterId,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? (text2 + $" and isnull(header.iTagFilterValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 6)})") : (text2 + $" and isnull(header.iTagFilterValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 6)})"));
				}
				if (FConvert.GetInputObject(oRec.Inputs, 8) != null && FConvert.GetInputObject(oRec.Inputs, 8).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 9) != 0) ? (text2 + $" and isnull(vrCore_Product.iMasterId,0) not in ({FConvert.GetInputObject(oRec.Inputs, 8)})") : (text2 + $" and isnull(vrCore_Product.iMasterId,0) in ({FConvert.GetInputObject(oRec.Inputs, 8)})"));
				}
				if (FConvert.GetInputValue(oRec.Inputs, 12) <= 0 && oRec.UserId != 1)
				{
					text8 = " and isnull(BH.bPhantom,0) <> 1";
				}
				else
				{
					text8 = ((FConvert.GetInputValue(oRec.Inputs, 13) <= 0) ? "" : " and isnull(BH.bPhantom,0) = 1");
				}
				text16 = Convert.ToString((from m in oRec.Inputs
					where m.ID == 14
					select m.Value).FirstOrDefault());
			}
			if (oRec.CodeName != null)
			{
				_ = oRec.CodeName != string.Empty;
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			string text18 = string.Empty;
			if (oRec.UserId > 1 && !flag3)
			{
				text18 = GetPOFilterOnUser(oRec.UserId, iCompanyId);
				if (!string.IsNullOrEmpty(text18))
				{
					text18 = $"Inner Join ({text18}) FillTags ON FillTags.iProdOrderId = Header.iProdOrderId";
				}
			}
			List<string> list = new List<string>();
			if (oRec.ExtraColumns != null && oRec.ExtraColumns.Length != 0)
			{
				list.AddRange(new List<string> { "vrCore_Product", "vrCore_Units", "vrCore_Account" });
				bool bMRPExtraFields = true;
				int iFieldCount = new RDDefault().GetMRPScreenColumns(oRec.ReportId, (int)oRec.SubReportId).Length + 1;
				text11 = GetExtraColumnDetails(oRec, ref sMRPExtraColumns, iCompanyId, list, bMRPExtraFields, iFieldCount);
			}
			standardQuery = new StandardQuery();
			if (FConvert.GetInputValue(oRec.Inputs, 10) == 1 && FConvert.GetInputValue(oRec.Inputs, 4) == 0)
			{
				standardQuery.Query = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                            case isnull(min(AllocDetails.iStartDate),0) when 0 then \r\n                                case isnull(min(AllocDetailsShift.iStartDate),0) when 0 then\r\n                                    isnull(min(header.iStartDate),0)\r\n                                else\r\n                                min(AllocDetailsShift.iStartDate) end\r\n                            else\r\n                            min(AllocDetails.iStartDate) end iStartDate,\r\n                            case isnull(max(AllocDetails.iEndDate),0) when 0 then \r\n                                case isnull(max(AllocDetailsShift.iEndDate),0) when 0 then\r\n                                    min(isnull(header.iEndDate,0))\r\n                                else\r\n                                max(AllocDetailsShift.iEndDate) end\r\n                            else\r\n                            max(AllocDetails.iEndDate) end iEndDate                        \r\n                            --iStartDate, iEndDate\r\n                            ,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                            when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                            then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                            case when isnull( bCancelled,0)=0 then \r\n                            case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                            --case when  isnull(iOrderStatus,0) in (0,1) then 'Not Released Order' \r\n                            when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                            when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                            when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO  \r\n                            ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                            case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                            case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime,\r\n                            --isnull(header.iModifiedDate,0) iModifiedDate,\r\n                            --isnull(header.iModifiedTime,0) iModifiedTime,\r\n                            isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                            ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo,\r\n                            isnull(InHouse.InHosueQty,0) InHosueQty ,isnull(OutSource.OutSourceQty,0) OutSourceQty,\r\n                            MAX(\r\n                                CASE \r\n                                WHEN ISNULL(header.iOrderStatus, 0) >= 2 THEN \r\n                                CASE\r\n                                WHEN InHouse.LatestReleasedDate IS NULL THEN OutSource.LatestReleasedDate\r\n                                WHEN OutSource.LatestReleasedDate IS NULL THEN InHouse.LatestReleasedDate\r\n                                WHEN InHouse.LatestReleasedDate > OutSource.LatestReleasedDate THEN InHouse.LatestReleasedDate\r\n                                ELSE OutSource.LatestReleasedDate\r\n                                END\r\n                                ELSE \r\n                                    ''\r\n                                END\r\n                                ) AS LatestReleasedDt,isnull(header.iDate,0)iOrderDate\r\n                            {13}\r\n                            FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                            inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                            Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0 when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                            {6}\r\n                            inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0                                                \r\n                            inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                            inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                            inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1                         \r\n                            left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                            left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy        \r\n                            left join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0\r\n                            left join vrCore_Account on vrCore_Account.iMasterId = header.iVendorId and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0                           \r\n                            {11}\r\n                            left join tMrp_AllocDetails{0} AllocDetails on AllocDetails.iScheduleId = header.iScheduleId\r\n                            left join tMRP_AllocDetailsShifts{0} AllocDetailsShift on AllocDetailsShift.iScheduleId = header.iScheduleId                        \r\n                            left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId                        \r\n                            left join (\r\n                                      select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate)  LatestReleasedDate\r\n                                from tMRP_PartialReleasedProdOrd_0\r\n                                                        where bOutSourced=0\r\n                                                        group by iProdOrderId\r\n                            ) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n                                                    left join (\r\n                                                        select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate)  LatestReleasedDate\r\n                                from tMRP_PartialReleasedProdOrd_0\r\n                                                        where bOutSourced=1\r\n                                                        group by iProdOrderId\r\n                                                    ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId                        \r\n                            {12}\r\n                            {9}\r\n                            {14}\r\n                            where 1=1 {4} {7} {10}\r\n                            GROUP BY header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0),PL.sName,vrCore_Product.sCode,\r\n                            iDueDate, case when isnull(header.iType,0)=0 then 'Production Order'  \r\n                            when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                            then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end  ,\r\n                            case when isnull( bCancelled,0)=0 then \r\n                            --case when  isnull(iOrderStatus,0) in (0,1) then 'Not Released Order' \r\n                            case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                            when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                            when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end ,case \r\n                            when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end  \r\n                            ,isnull(header.iCreatedDate,0),isnull(header.iCreatedTime,0),\r\n                            case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end  ,\r\n                            case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  ,\r\n                            isnull(createdUser.sLoginName,'') ,isnull(ModifiedUser.sLoginName,'') ,ISNULL(header.sSONO,''),isnull(ModifiedUser.iUserId,0),isnull(ModifiedUser.sLoginName,''),ModifiedUser.sLoginName, ISNULL(Body.fQuantity, 0)\r\n                            ,isnull(header.sRemarks,'') ,isnull(header.sSpecialInstruction,'') ,isnull(header.sBatchNo,''),isnull(InHouse.InHosueQty,0)  ,isnull(OutSource.OutSourceQty,0),ISNULL(header.iDate,0)\r\n                            {13} ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text5, text6, oRec.LanguageId, text18, text8, text7, text16, sMRPExtraColumns, text11);
			}
			else
			{
				text2 = ((flag2 != false) ? (text2 + text4) : (text2 + text3));
				string text19 = string.Empty;
				if (!flag3)
				{
					text19 = " ,isnull(InHouse.InHosueQty,0) InHosueQty ,isnull(OutSource.OutSourceQty,0) OutSourceQty,CASE WHEN ISNULL(header.iOrderStatus, 0) >= 2 THEN CASE WHEN InHouse.LatestReleasedDate IS NULL THEN OutSource.LatestReleasedDate WHEN OutSource.LatestReleasedDate IS NULL THEN InHouse.LatestReleasedDate WHEN InHouse.LatestReleasedDate > OutSource.LatestReleasedDate THEN InHouse.LatestReleasedDate ELSE OutSource.LatestReleasedDate END ELSE   '' END AS LatestReleasedDt";
					text17 = " left join (\r\n                                select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=0\r\n                                group by iProdOrderId) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n\r\n                                left join (\r\n                                select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                where bOutSourced=1\r\n                                group by iProdOrderId\r\n                                ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId \r\n                                ";
				}
				string text20 = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                                    iStartDate, iEndDate,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                                    when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                                    then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                                    case when isnull( bCancelled,0)=0 then case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                                    when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                                    when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                                    when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO   \r\n                                    ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                                    case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                                    case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime, \r\n                                    --isnull(header.iModifiedDate,0) iModifiedDate,\r\n                                    --isnull(header.iModifiedTime,0) iModifiedTime,                       \r\n                                    isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                                    ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo\r\n                                     {14}\r\n                                    ,isnull(header.iDate,0)iOrderDate\r\n                                    {15}\r\n                                    FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                                    inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                                    Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0  \r\n                                    when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                                    {6}\r\n                                    inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                                    inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                                    inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                                    inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1 \r\n                                    inner join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0\r\n                                    inner join vrCore_Account on vrCore_Account.iMasterId = header.iVendorId and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0 and vrCore_Account.iTreeId=0                                                        \r\n                                    left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                                    left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy                                                                                                                                                                        \r\n                                    left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId                                                        \r\n                                    {13}\r\n                                    {11}\r\n                                    {12}\r\n                                    {9}\r\n                                    {16}\r\n                                    where 1=1  {4} {7} {10}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text5, text6, oRec.LanguageId, text18, text8, text7, text16, text17, text19, sMRPExtraColumns, text11);
				text2 += text3;
				string text21 = (standardQuery.Query = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,vrCore_Product.sCode,\r\n                                    iStartDate, iEndDate,iDueDate, case when isnull(header.iType,0)=0 then 'Production Order' \r\n                                    when isnull(header.iType,0)=1 then 'Assembly' when isnull(header.iType,0)=2 then 'Engineering' when isnull(header.iType,0)=3 \r\n                                    then 'Universal' when isnull(header.iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                                    case when isnull( bCancelled,0)=0 then case when isnull(iOrderStatus,0) in (0) then 'Planned Order' when  isnull(iOrderStatus,0) in (1) then 'Not Released Order' \r\n                                    when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                                    when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                                    when isnull(bMRPDone,1)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO \r\n                                    ,isnull(header.iCreatedDate,0)iCreatedDate,isnull(header.iCreatedTime,0)iCreatedTime,\r\n                                    case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                                    case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime,    \r\n                                    isnull(createdUser.sLoginName,'') CreatedBy,case when isnull(ModifiedUser.iUserId,0)=0  then '' else  ModifiedUser.sLoginName end ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity\r\n                                    ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo,\r\n                                    isnull(InHouse.InHosueQty,0) InHosueQty ,isnull(OutSource.OutSourceQty,0) OutSourceQty,\r\n                                    --MAX(\r\n                                            CASE \r\n                                            WHEN ISNULL(header.iOrderStatus, 0) >= 2 THEN \r\n                                            CASE\r\n                                            WHEN InHouse.LatestReleasedDate IS NULL THEN OutSource.LatestReleasedDate\r\n                                            WHEN OutSource.LatestReleasedDate IS NULL THEN InHouse.LatestReleasedDate\r\n                                            WHEN InHouse.LatestReleasedDate > OutSource.LatestReleasedDate THEN InHouse.LatestReleasedDate\r\n                                            ELSE OutSource.LatestReleasedDate\r\n                                            END\r\n                                            ELSE \r\n                                                ''\r\n                                            END\r\n                                     --) \r\n                                    ,isnull(header.iDate,0)iOrderDate\r\n                                    {14}\r\n                                    FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                                    inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                                    {8}\r\n                                    Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0 when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                                    {6}\r\n                                    inner join vrCore_Product on Body.iItem = vrCore_Product.iMasterId And vrCore_Product.iTreeId = 0\r\n                                    inner JOIN mCore_ProductLanguage PL ON vrCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={9}\r\n                                    inner join mMRP_BomVariantHeader BVH on BVH.iVariantId = header.iVaraintId\r\n                                    inner join mMRP_BomHeader BH on BH.iBomId = BVH.iBomId and ISNULL(bMainProduct,1)=1 \r\n                                    inner join vrCore_Units on vrCore_Units.iMasterId=Body.iUnit and vrCore_Units.iTreeId=0                                                        \r\n                                    inner join vrCore_Account on vrCore_Account.iMasterId = header.iVendorId and vrCore_Account.iStatus <> 5 and vrCore_Account.bGroup = 0 and vrCore_Account.iTreeId=0                                                        \r\n                                    left join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                                    left join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy                                                                                                                                                                       \r\n                                    left join tMrp_RaiseProductionOrderExtraHeader{0} REF on REF.iProdOrderId = header.iProdOrderId                                                        \r\n                                    left join (\r\n                                    select sum( isnull(fQuantity,0)) InHosueQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                    where bOutSourced=0\r\n                                    group by iProdOrderId) InHouse on InHouse.iProdOrderId= header.iProdOrderId\r\n                                    left join (\r\n                                    select sum( isnull(fQuantity,0)) OutSourceQty,iProdOrderId,MAX(iReleasedDate) LatestReleasedDate from tMRP_PartialReleasedProdOrd_0\r\n                                    where bOutSourced=1\r\n                                    group by iProdOrderId\r\n                                    ) OutSource  on OutSource.iProdOrderId=header.iProdOrderId\r\n                                    {12}\r\n                                    {13}\r\n                                    {10}\r\n                                    {15}\r\n                                    where 1=1 {4} {7} {11}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), text5, text6, text15, oRec.LanguageId, text18, text8, text7, text16, sMRPExtraColumns, text11));
				string text23 = text21;
				if (!flag2.HasValue)
				{
					standardQuery.Query = text20 + " UNION ALL " + text23;
				}
				else if (flag2 == false)
				{
					standardQuery.Query = text20;
				}
				else
				{
					standardQuery.Query = text23;
				}
			}
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" header.iProdOrderId Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.MRPReleaseProductionOrder:
		{
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" and iDueDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			text2 = ((FConvert.GetInputValue(oRec.Inputs, 3) != -1) ? $" and iOrderStatus={FConvert.GetInputValue(oRec.Inputs, 3)}" : " and iOrderStatus in (0,1,2,3)");
			if (FConvert.GetInputValue(oRec.Inputs, 5) > 0)
			{
				text2 += $" and isnull(header.iTagFilterId,0) = {FConvert.GetInputValue(oRec.Inputs, 5)}";
			}
			if (FConvert.GetInputObject(oRec.Inputs, 6) != null && FConvert.GetInputObject(oRec.Inputs, 6).ToString().Length > 0)
			{
				text2 = ((FConvert.GetInputValue(oRec.Inputs, 7) != 0) ? (text2 + $" and isnull(header.iTagFilterValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 6)})") : (text2 + $" and isnull(header.iTagFilterValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 6)})"));
			}
			if ((oRec.Inputs != null && FConvert.GetInputValue(oRec.Inputs, 12) > 0) || oRec.UserId == 1)
			{
				text7 = "";
				text8 = "";
			}
			else
			{
				text7 = "left join (select distinct vmCore_Product.iMasterId, mMRP_BomHeader.bPhantom,vmCore_Product.iBOM from vmCore_Product inner join mMRP_BomVariantHeader on vmCore_Product.iBOM=mMRP_BomVariantHeader.iVariantId inner join mMRP_BomHeader on mMRP_BomHeader.iBomId=mMRP_BomVariantHeader.iBomId) Phantom on Phantom.iBOM=Product.iBOM and Phantom.iMasterId=Body.iItem";
				text8 = "and isnull(Phantom.bPhantom,0) <> 1";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			string text35 = string.Empty;
			if (oRec.UserId > 1)
			{
				text35 = GetPOFilterOnUser(oRec.UserId, iCompanyId);
				if (!string.IsNullOrEmpty(text35))
				{
					text35 = $"Inner Join ({text35}) FillTags ON FillTags.iProdOrderId = Header.iProdOrderId";
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select header.iProdOrderId,sProdOrderNo,ISNULL(sRefOrderNo,0) sRefOrderNo,PL.sName,Product.sCode,\r\n                        iStartDate, iEndDate,iDueDate, case when isnull(iType,0)=0 then 'Production Order' \r\n                        when isnull(iType,0)=1 then 'Assembly' when isnull(iType,0)=2 then 'Engineering' when isnull(iType,0)=3 \r\n                        then 'Universal' when isnull(iType,0)=4 then ' Plant Maintenance' else '' end [Type] ,\r\n                        case when isnull( bCancelled,0)=0 then case when  isnull(iOrderStatus,0) in (0,1) then 'Not Released Order' \r\n                        when  isnull(iOrderStatus,0)=2 then 'Released order' when  isnull(iOrderStatus,0)=3 then 'Work In Progress' \r\n                        when isnull(iOrderStatus,0)=4 then 'Finished' else 'Closed' end else 'Cancelled' end OrderStatus,case \r\n                        when isnull(bMRPDone,0)=1 then 'Done' else 'Not Done' end sPlannedStatus,ISNULL(header.sSONO,'') sSONO, \r\n                        isnull(header.iCreatedDate,0) iCreatedDate,isnull(header.iCreatedTime,0) iCreatedTime,\r\n                        case when  isnull(header.iModifiedDate,0) > 0 then isnull(header.iModifiedDate,0) else isnull(header.iCreatedDate,0) end iModifiedDate ,\r\n                        case when  isnull(header.iModifiedDate,0) > 0 then  isnull(header.iModifiedTime,0) else isnull(header.iCreatedTime,0) end  iModifiedTime,\r\n                        isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy, ISNULL(Body.fQuantity, 0) Quantity,\r\n                        ISNULL(ReleasedQuantity,0)ReleasedQuantity\r\n                        ,isnull(header.sRemarks,'') sRemarks,isnull(header.sSpecialInstruction,'') sSpecialInstruction,isnull(header.sBatchNo,'') sBatchNo,\r\n                        isnull(releasePO.iReleasedDate,0) LatestReleasedDate,isnull(header.iDate,0) iOrderDate\r\n                        FROM tMrp_ProdOrder{0} header with (ReadUncommitted)\r\n                        {7}\r\n                        {10}\r\n                        inner join tMrp_ProdOrderBody{0} Body with (ReadUncommitted) on header.iProdOrderId=Body.iProdOrderId\r\n                        Inner Join vmCore_InvTag InvTag on InvTag.iMasterId =case  when header.iWareHouseId IS NULL then 0  when header.iWareHouseId=-1 then 0 else header.iWareHouseId end\r\n                        inner join vmCore_Product  Product on  Body.iItem=Product.iMasterId And Product.iTreeId = 0\r\n                        inner JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={9}\r\n                        inner join vrCore_Product on vrCore_Product.iMasterId = Body.iItem And vrCore_Product.iTreeId = 0\r\n                        inner join mSec_Users createdUser on createdUser.iUserId=header.iCreatedBy\r\n                        inner join mSec_Users ModifiedUser on ModifiedUser.iUserId=header.iModifiedBy and ISNULL(bMainProduct,1)=1\r\n                        {12}\r\n                        left join (select sum(fQuantity) ReleasedQuantity,iProdOrderId,Max(iReleasedDate) iReleasedDate  from tMRP_PartialReleasedProdOrd{0} group by iProdOrderId)\r\n\t\t\t\t\t\treleasePO on releasePO.iProdOrderId=header.iProdOrderId                                                                       \r\n                        WHERE 1=1 and isnull(bMRPDone,0)=1 and header.bOutSource<>1  {4} {8} {11}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text5, text6, oRec.LanguageId, text35, text8, text7);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" header.iProdOrderId Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.BatchTracking:
		{
			string text30 = Convert.ToString(oRec.Inputs[0].Value);
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("Declare @iBodyId int = 0,@iProdOrderId bigint = 0,@iProductId int = 0\r\n                            Select @iBodyId = iBodyId From tCore_Rma{0} where sRmaNo = '{1}'\r\n                            if (@iBodyId = 0)\r\n                            Begin\r\n                            Select Top 1 @iBodyId = iBodyId From tCore_Batch{0} where sBatchNo = '{1}' And fQuantity > 0 Order by iBodyId\r\n                            End\t\r\n\r\n                            if(@iBodyId > 0)\r\n                            Begin\r\n                            Select @iProdOrderId = d.iProdOrderId ,@iProductId= iProduct\tFrom tCore_Data{0} a \r\n                            join tCore_Indta{0} b  on a.iBodyId = b.iBodyId\r\n                            join tCore_Header_2{0} d with(Readuncommitted) on d.iHeaderId = a.iHeaderId\r\n                            where a.iBodyId = @iBodyId\r\n\t\r\n                            Select sProdOrderNo,iDate,iDueDate,sRefOrderNo,fQuantity,iVaraintId,d.iBomId, Case when c.sName = 'Default' Then d.sName else c.sName End BOM From tMrp_ProdOrder{0}  a with(Readuncommitted) \r\n                            Join tMrp_ProdOrderBody{0} b with(Readuncommitted) on a.iProdOrderId = b.iProdOrderId\r\n                            Join mMRP_BomVariantHeader c with(Readuncommitted) on c.iVariantId= a.iVaraintId\r\n                            Join mMRP_BomHeader d with(Readuncommitted) on d.iBomId= c.iBomId\r\n                            where a.iProdOrderId = @iProdOrderId\r\n\r\n                            Select c.sVoucherNo,e.sName VoucherType,Product.iMasterId iProductId,PL.sName sProductName\r\n                            ,Product.sCode sProductCode,b.fQuantity,a.mOriginalAmount,mGross,Batch.sBatchNo,iBatchId,Batch.iMfDate,Batch.iExpiryDate\r\n                            ,isnull(Requisition.iReqHeaderId,0) iReqHeaderId,isnull(Requisition.sRequisitionNo,'') sRequisitionNo\r\n                            From tCore_Data{0} a \r\n                            join tCore_Indta{0} b  on a.iBodyId = b.iBodyId\r\n                            Join tCore_Header{0} c  on c.iHeaderId = a.iHeaderId\r\n                            join tCore_Header_2{0} d with(Readuncommitted) on d.iHeaderId = c.iHeaderId\r\n                            join cCore_Vouchers{0} e with(Readuncommitted) on e.iVoucherType= c.iVoucherType \r\n                            join mCore_Product Product  on Product.iMasterId=b.iProduct\r\n                            inner JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n                            Left join tCore_Batch{0} Batch  on Batch.iBodyId=b.iBodyId\r\n                            left join tQC_RequisitionHeader{0} Requisition on Requisition.iTransactionId=a.iTransactionId\r\n                            where a.iBodyId = @iBodyId \r\n\r\n                            Select c.sVoucherNo,e.sName VoucherType,Product.iMasterId iProductId,PL.sName sProductName\r\n                            ,Product.sCode sProductCode,-b.fQuantityInBase fQuantity,a.mOriginalAmount,mGross,Batch.sBatchNo,iBatchId,Batch.iMfDate,Batch.iExpiryDate\r\n                            From tCore_Data{0} a \r\n                            join tCore_Indta{0} b  on a.iBodyId = b.iBodyId\r\n                            Join tCore_Header{0} c  on c.iHeaderId = a.iHeaderId\r\n                            join tCore_Header_2{0} d with(Readuncommitted) on d.iHeaderId = c.iHeaderId\r\n                            join cCore_Vouchers{0} e with(Readuncommitted) on e.iVoucherType= c.iVoucherType \r\n                            Left join tCore_Batch{0} Batch  on Batch.iBodyId=b.iBodyId\r\n                            join mCore_Product Product  on Product.iMasterId=b.iProduct\r\n                            inner JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n                            where iProdOrderId = @iProdOrderId and  e.iVoucherClass in (6656,6912)\r\n\r\n                            if(Select Case when (iModulesImplemented & 0x10000 > 0) or (iModulesImplemented & 0x20000 > 0)  then 1 else 0 End ISWMS from tCore_Company_Details with (ReadUnCommitted)) = 1\r\n                            Begin\r\n\t                            Declare @iSalesOrderTranId Bigint = 0,@iProdOrderBodyId int = 0\r\n\t                            Select @iProdOrderBodyId = iMasterID From tMrp_ProdOrderBody{0} with (ReadUncommitted) where iProdOrderId = @iProdOrderId And iItem = @iProductId\r\n\t\t\r\n\t                            Select @iSalesOrderTranId =\tiReqTransactionId From tCore_ReservedStock{0}  where iResDocType = 0 And iType = 11 And iInwardResTransId = @iProdOrderBodyId\r\n\r\n\t                            Select sVoucherNo,e.sName VoucherType,Product.iMasterId iProductId,\r\n\t                            PL.sName sProductName,Product.sCode sProductCode,-(b.fQuantityInBase) fQuantity,a.mOriginalAmount,mGross,Acct.sName CustomerName\r\n\t                            From tCore_Data{0} a \r\n\t                            join tCore_Indta{0} b  on a.iBodyId = b.iBodyId\r\n\t                            Join tCore_Header{0} c  on c.iHeaderId = a.iHeaderId\r\n\t                            join cCore_Vouchers{0} e with(Readuncommitted) on e.iVoucherType= c.iVoucherType \r\n\t                            join mCore_Product Product  on Product.iMasterId=b.iProduct\r\n                                inner JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n\t                            Left join mCore_Account Acct  on Acct.iMasterId= iBookNo\r\n\t                            where a.iTransactionId = @iSalesOrderTranId\r\n\r\n                            End\r\n\r\n                            End ", m_sSuffix, text30, "--@EXTRA_COLUMNS", oRec.LanguageId);
			break;
		}
		case FocusReport.QCModifyTestResultReport:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" and iDueDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select  ModifyResult.id,ModifyResult.sDocNo ,ModifyResult.iDate ,rheader.sRequisitionNo,tHeader.sDocNo sTestNumber,\r\n                                                    --iResult,iModifiedResult,\r\n                                                    case when iResult = 0 then 'Failed' else 'Passed' end sResult,case when iModifiedResult = 0 then 'Failed' else 'Passed' end sModifiedResult,\r\n                                                    ModifyResult.sRemarks,isnull(ModifyResult.iCreatedDate,0) iCreatedDate,isnull(ModifyResult.iCreatedTime,0) iCreatedTime,isnull(createdUser.sLoginName,'') CreatedBy\r\n                                                    from tQC_ModifyResult{0}  ModifyResult\r\n                                                    inner join tQC_RequisitionHeader{0} rheader on rheader.iReqHeaderId=iRequisitionId\r\n                                                    inner join tQC_TestingHeader{0} tHeader on tHeader.iTestHeaderId=ModifyResult.iTestHeaderId\r\n                                                    left join mSec_Users createdUser on createdUser.iUserId=ModifyResult.iCreatedBy\r\n                                                    left join mSec_Users ModifiedUser on ModifiedUser.iUserId=ModifyResult.iModifiedBy where 1=1 {7}", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", text6);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" id Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.PreSchedulePlan:
			if (FConvert.GetInputValue(oRec.Inputs, 1) > 0 && FConvert.GetInputValue(oRec.Inputs, 2) > 0)
			{
				text = $" and iStartDate between {FConvert.GetInputValue(oRec.Inputs, 1)} and {FConvert.GetInputValue(oRec.Inputs, 2)} ";
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select PlanHeader.id,sDocumentNo,sOrderNumber,Case When  PlanHeader.iType=0 then 'Sales Order' else 'Plan Independent Order' end OrderType,\r\n                        dQuantity,iStartDate,case when bCompleted=1 then 'Completed' else 'Not Completed' end PlanStatus ,\r\n                        mMRP_BomHeader.sCode BOMCode,mMRP_BomHeader.sName BOMName,mMRP_RoutingHeader.sCode sRoutingCode,\r\n                        Product.sName ProductName,Product.sCode ProductCode\r\n                        from tMRP_PreSchedulePlan{0} PlanHeader\r\n                        inner join mMRP_BomVariantHeader on mMRP_BomVariantHeader.iVariantId=PlanHeader.iVarientId\r\n                        inner join mMRP_BomHeader on mMRP_BomHeader.iBomId=mMRP_BomVariantHeader.iBomId\r\n                        inner join mMRP_RoutingHeader on mMRP_RoutingHeader.iRoutingId=PlanHeader.iRoutingId\r\n                        inner join vmCore_Product Product on Product.iMasterId=PlanHeader.iProduct \r\n                        inner JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={8}\r\n                        where 1=1 {5}\r\n                        ", m_sSuffix, oRec.StartingDate, oRec.EndingDate, text, text2, text6, General.GetMasters(oRec.Masters), "--@EXTRA_COLUMNS", oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" id Desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.ProductionOrderInHand:
		{
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			if (FConvert.GetInputValue(oRec.Inputs, 1) != -1)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 1) == 0)
				{
					text6 = (string.IsNullOrEmpty(text6) ? " and ProductionOrders.iOrderStatus in (2,3) " : (" and ProductionOrders.iOrderStatus in (2,3) " + text6));
				}
				else if (FConvert.GetInputValue(oRec.Inputs, 1) == 4)
				{
					text6 = (string.IsNullOrEmpty(text6) ? " and ProductionOrders.iOrderStatus in (4) " : (" and ProductionOrders.iOrderStatus in (4) " + text6));
				}
			}
			string text46 = ((oRec.UserId == 1) ? "Left" : "Inner");
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("declare @iUserId int = {1} , @iEmployeeId int,@sEmpoyeeGroupAndIds nvarchar(max) ,@EmployeeGroup int\r\n                                           \r\n                                            select @iEmployeeId = isnull(iLinkId,-1) from mSec_Users where iUserType=1 and iUserId=@iUserId\r\n                                            select @EmployeeGroup=isnull(iParentId,0) from mPay_EmployeeTreeDetails where iMasterId=@iEmployeeId\r\n\t\t\t\t\t\t\t\t\t\t\tset @sEmpoyeeGroupAndIds= CONVERT( varchar(100),@iEmployeeId)+','+CONVERT( varchar(100),@EmployeeGroup)\r\n\r\n                                            select ProductionOrders.iProdOrderId,CONCAT(details.iSkillType,'_',mMRP_RoutingNodes.iProcessId) iSkillProcess,\r\n                                            --details.iSkillType,mMRP_RoutingNodes.iProcessId,\r\n                                            ProductionOrders.sProdOrderNo,mMRP_RoutingNodes.sProcessName,vmPay_SkillType.sName SkillTypeName,vmPay_Employee.sName EmpName,\r\n                                            vmCore_Product.sName ProductName,vmCore_Product.sCode ProductCode,\r\n                                            ISNULL(PRPO.fReleasedQty,0)Quantity,\r\n                                            dbo.DateToInt(dbo.fCore_IntToDateTime(ProductionOrders.iStartDate)) iStartDate,\r\n                                            dbo.DateToInt(dbo.fCore_IntToDateTime(ProductionOrders.iEndDate)) iEndDate,ProductionOrders.iDueDate,\r\n                                            ProductionOrders.sRefOrderNo\r\n                                            from vMrp_ProdOrder{0} ProductionOrders\r\n                                            --Inner Join tMrp_ProdOrderBody{0} with (ReadUnCommitted)  On ProductionOrders.iProdOrderId = tMrp_ProdOrderBody{0}.iProdOrderId and bMainProduct=1      \r\n                                            inner join tMRP_AllocationSkillHeader{0} header on ProductionOrders.iProdOrderId = header.iRecordId and iScreenId = 5                                        \r\n                                            inner join tMRP_AllocationSkillDetails{0} details on header.id =details.iSkillHeaderId \t\r\n                                            {4}  join dbo.fCore_fnDStringToTable ( @sEmpoyeeGroupAndIds,',') A on A.value=details.iEmployee\r\n                                          \r\n\r\n\t\t\t\t\t\t\t\t\t\t\tinner join mMRP_RoutingNodes on mMRP_RoutingNodes.iBodyId =details.iSubRecordId\r\n                                            inner join vmPay_Employee on vmPay_Employee.iMasterId = details.iEmployee\r\n                                            inner join vmPay_SkillType on vmPay_SkillType.iMasterId = details.iSkillType\r\n                                            --inner join mMRP_RoutingHeader on mMRP_RoutingHeader.iRoutingId=ProductionOrders.iRoutingId and isnull(bBOMProcess,0) = 0 --and mMRP_RoutingHeader.bSetAsDefault=1\r\n                                            --inner join mMRP_RoutingNodes on mMRP_RoutingNodes.iHeaderId=mMRP_RoutingHeader.iRoutingId\r\n                                           -- inner join mMRP_RoutingOutput on mMRP_RoutingOutput.iBodyId=mMRP_RoutingNodes.iBodyId\r\n                                            inner join vmCore_Product on vmCore_Product.iMasterId=ProductionOrders.iItem\r\n                                            inner JOIN mCore_ProductLanguage PL ON vmCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n                                            Left Join (Select iProdOrderId,SUM(fQuantity) fReleasedQty From tMRP_PartialReleasedProdOrd{0} Group by iProdOrderId) as PRPO on PRPO.iProdOrderId = ProductionOrders.iProdOrderId\r\n                                            where 1=1 and ProductionOrders.iOrderStatus>=2   {2} ", m_sSuffix, oRec.UserId, text6, oRec.LanguageId, text46);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" ProductionOrders.iProdOrderId ", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.ProductionOrderProcessWiseEmployeeActivity:
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("declare @iUserId int = {1} , @iEmployeeId int \r\n                                           \r\n                                            select @iEmployeeId = isnull(iLinkId,-1) from mSec_Users where iUserType=1 and iUserId=@iUserId\r\n\r\n                                            select ProductionOrders.iProdOrderId,CONCAT(details.iSkillType,'_',mMRP_RoutingNodes.iProcessId) iSkillProcess,\r\n                                            --details.iSkillType,mMRP_RoutingNodes.iProcessId,\r\n                                            ProductionOrders.sProdOrderNo,mMRP_RoutingNodes.sProcessName,vmPay_SkillType.sName SkillTypeName,vmPay_Employee.sName EmpName,\r\n                                            vmCore_Product.sName ProductName,vmCore_Product.sCode ProductCode,\r\n                                            ISNULL(PRPO.fReleasedQty,0)Quantity,\r\n                                            dbo.DateToInt(dbo.fCore_IntToDateTime(ProductionOrders.iStartDate)) iStartDate,\r\n                                            dbo.DateToInt(dbo.fCore_IntToDateTime(ProductionOrders.iEndDate)) iEndDate,ProductionOrders.iDueDate,\r\n                                            ProductionOrders.sRefOrderNo\r\n                                            from vMrp_ProdOrder{0} ProductionOrders\r\n                                            --Inner Join tMrp_ProdOrderBody{0} with (ReadUnCommitted)  On ProductionOrders.iProdOrderId = tMrp_ProdOrderBody{0}.iProdOrderId and bMainProduct=1      \r\n                                            inner join tMRP_AllocationSkillHeader{0} header on ProductionOrders.iProdOrderId = header.iRecordId and iScreenId = 5                                        \r\n                                            inner join tMRP_AllocationSkillDetails{0} details on header.id =details.iSkillHeaderId and \r\n                                            details.iEmployee = case when @iUserId = 1 then details.iEmployee\r\n                                            when @iEmployeeId = 0 then null\r\n                                            when @iEmployeeId > 0 then @iEmployeeId\r\n                                            end\r\n\r\n\t\t\t\t\t\t\t\t\t\t\tinner join mMRP_RoutingNodes on mMRP_RoutingNodes.iBodyId =details.iSubRecordId\r\n                                            inner join vmPay_Employee on vmPay_Employee.iMasterId = details.iEmployee\r\n                                            inner join vmPay_SkillType on vmPay_SkillType.iMasterId = details.iSkillType\r\n                                            --inner join mMRP_RoutingHeader on mMRP_RoutingHeader.iRoutingId=ProductionOrders.iRoutingId and isnull(bBOMProcess,0) = 0 --and mMRP_RoutingHeader.bSetAsDefault=1\r\n                                            --inner join mMRP_RoutingNodes on mMRP_RoutingNodes.iHeaderId=mMRP_RoutingHeader.iRoutingId\r\n                                           -- inner join mMRP_RoutingOutput on mMRP_RoutingOutput.iBodyId=mMRP_RoutingNodes.iBodyId\r\n                                            inner join vmCore_Product on vmCore_Product.iMasterId=ProductionOrders.iItem\r\n                                            inner JOIN mCore_ProductLanguage PL ON vmCore_Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n                                            Left Join (Select iProdOrderId,SUM(fQuantity) fReleasedQty From tMRP_PartialReleasedProdOrd{0} Group by iProdOrderId) as PRPO on PRPO.iProdOrderId = ProductionOrders.iProdOrderId\r\n                                            where 1=1   {2} ", m_sSuffix, oRec.UserId, text6, oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" ProductionOrders.iProdOrderId ", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.Routing:
			if (oRec.Inputs != null)
			{
				if (FConvert.GetInputValue(oRec.Inputs, 3) > 0)
				{
					text2 += $" and isnull(BH.iTagId,0) = {FConvert.GetInputValue(oRec.Inputs, 3)}";
				}
				if (FConvert.GetInputObject(oRec.Inputs, 4) != null && FConvert.GetInputObject(oRec.Inputs, 4).ToString().Length > 0)
				{
					text2 = ((FConvert.GetInputValue(oRec.Inputs, 5) != 0) ? (text2 + $" and isnull(BH.iTagValue,0) not in ({FConvert.GetInputObject(oRec.Inputs, 4)})") : (text2 + $" and isnull(BH.iTagValue,0) in ({FConvert.GetInputObject(oRec.Inputs, 4)})"));
				}
			}
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select RH.iRoutingId,RH.sCode sRoutingName,RH.sCode sRoutingCode,RH.iVersion RoutingVersion,RH.bSetAsDefault,\r\n                                    Case When VH.sName = 'Default' then BH.sName Else VH.sName End sBOMName,BH.sCode sBOMCode,vh.sName sVarientName,BH.iVersion BOMVersion,\r\n                                    Product.sName ProdName,Product.sCode ProdCode,RH.fFromLotSize,RH.fToLotSize,RH.sDesc,\r\n                                    Case when isnull(RH.iAuthStatus,0)=0 then 'Authorise Pending' when RH.iAuthStatus=1 then 'Authorised' when RH.iAuthStatus=2 then 'Rejected' else '' end as AuthStatus,\r\n                                    isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy, Case when isnull(sUser.sUserName,'')='' then '' else sUser.sUserName end as AuthorizedUser\r\n                                    FROM mMRP_RoutingHeader RH\r\n                                    INNER JOIN mMRP_BomVariantHeader VH on RH.iBomId=vh.iVariantId  \r\n                                    INNER JOIN mMRP_BomHeader BH on BH.iBomId=VH.iBomId\r\n                                    INNER JOIN mMRP_BOMBody BB  on BB.iVariantId=VH.iVariantId\r\n                                    INNER JOIN mCore_Product Product on Product.iMasterId=BB.iProductId and bInput=0 and bMainOutput=1 \r\n                                    INNER JOIN mCore_ProductLanguage PL ON Product.iMasterId = PL.iMasterId And  PL.iLanguageId={3}\r\n                                    INNER JOIN vrCore_Product on vrCore_Product.iMasterId = BB.iProductId And vrCore_Product.iTreeId = 0\r\n                                    LEFT JOIN mSec_Users createdUser on createdUser.iUserId=BH.iCreatedBy\r\n                                    LEFT JOIN mSec_Users ModifiedUser on ModifiedUser.iUserId=BH.iModifiedBy\r\n                                    LEFT JOIN (Select iDocumentId,STRING_AGG( 'L' + convert(varchar,iLevel+1) + ' : ' + sUserName ,', ') sUserName from mCore_AuthorizationDetails_0 AD\r\n                                        JOIN  tCore_ProductionAuth PA on AD.iAuthorizationDetailId = PA.iAuthNodeId\r\n                                        JOIN tCore_productionAuthUser AU on AU.iAuthId = PA.iAuthId\r\n                                        JOIN mSec_Users sUser ON AU.iRoleOrUserId = sUser.iUserId\r\n                                        where AU.iStatus = 1\r\n                                        GROUP BY iDocumentId \r\n                                    ) sUser On sUser.iDocumentId  = RH.iRoutingId\r\n                                    WHERE RH.bBOMProcess<>1 and RH.iBomId>0 {1} {2} ", oRec.UserId, text2, text6, oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" RH.iRoutingId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		case FocusReport.PlannedIndependentOrder:
		{
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " and " + text6;
				}
			}
			standardQuery = new StandardQuery();
			bool flag5 = false;
			List<string> list3 = oRec.ReportCustomize.Layout.Columns.Select((_Column m) => m.AliasName).ToList();
			if (list3.Contains("Item Name") || list3.Contains("Item Code") || list3.Contains("Quantity"))
			{
				flag5 = true;
			}
			if (flag5)
			{
				standardQuery.Query = string.Format("select PIO.iPIOId,PIO.sDocNo,PIO.iDate,isnull(PIODet.iDuedate,0) iDuedate,Product.sName sName,Product.sCode sCode,fQuantity,\r\n                                    --dbo.IntToDate(isnull(tMrp_PIODetails{2}.iCreatedDate,0)) iCreatedDate,dbo.IntToDate(isnull(tMrp_PIODetails{2}.iModifiedDate,0)) iModifiedDate,\r\n                                    CAST(CONVERT(date, dbo.IntToDate(isnull(tMrp_PIODetails{2}.iCreatedDate,0))) AS varchar(10)) + ' ' + dbo.fCore_IntToTime(isnull(tMrp_PIODetails{2}.iCreatedTime,0)) iCreatedDate,\r\n\t\t\t                        CAST(CONVERT(date, dbo.IntToDate(isnull(tMrp_PIODetails{2}.iModifiedDate,0))) AS varchar(10)) + ' ' + dbo.fCore_IntToTime(isnull(tMrp_PIODetails{2}.iModifiedTime,0)) iModifiedDate,\r\n                                    isnull(createdUser.sLoginName,'') CreatedBy,isnull(ModifiedUser.sLoginName,'') ModifiedBy,sRemarks,\r\n                                    Case when isnull(PIO.iAuthStatus,0)=0 then 'Authorise Pending' when PIO.iAuthStatus=1 then 'Authorised' when PIO.iAuthStatus=2 then 'Rejected' else '' end as AuthStatus,\r\n                                    Case when isnull(sUser.sUserName,'')='' then '' else sUser.sUserName end as AuthorizedUser\r\n                                    from tMrp_PIO{2} PIO \r\n                                    Join (Select MIN(iDuedate) iDuedate,iPIOId From tMrp_PIODetails{2} Group By iPIOId) PIODet on PIO.iPIOId = PIODet.iPIOId \r\n                                    inner join tMrp_PIODetails{2} on tMrp_PIODetails{2}.iPIOId=PIO.iPIOId\r\n                                    inner join mCore_Product Product on Product.iMasterId=tMrp_PIODetails{2}.iProductId                                                            \r\n                                    left join mSec_Users createdUser on createdUser.iUserId=tMrp_PIODetails{2}.iCreatedBy\r\n\t\t\t\t\t\t\t\t\t\t\t\tleft join mSec_Users ModifiedUser on ModifiedUser.iUserId=tMrp_PIODetails{2}.iModifiedBy\r\n                                    left join tMrp_PIOTags{2} PIOTags on PIOTags.iPIODetailId=tMrp_PIODetails{2}.iId                                                             \r\n                                    LEFT JOIN (Select iDocumentId,STRING_AGG( 'L' + convert(varchar,iLevel+1) + ' : ' + sUserName ,', ') sUserName from mCore_AuthorizationDetails{2} AD\r\n                                        JOIN  tCore_ProductionAuth PA on AD.iAuthorizationDetailId = PA.iAuthNodeId\r\n                                        JOIN tCore_productionAuthUser AU on AU.iAuthId = PA.iAuthId\r\n                                        JOIN mSec_Users sUser ON AU.iRoleOrUserId = sUser.iUserId\r\n                                        WHERE AU.iStatus = 1\r\n                                        GROUP BY iDocumentId \r\n                                    ) sUser On sUser.iDocumentId  = PIO.iPIOId\r\n                                    where 1=1 {1}", oRec.UserId, text6, m_sSuffix);
			}
			else
			{
				standardQuery.Query = string.Format("select PIO.iPIOId,PIO.sDocNo,PIO.iDate,isnull(PIODet.iDuedate,0) iDuedate from tMrp_PIO{2} PIO \r\n                                     Join (Select MIN(iDuedate) iDuedate,iPIOId From tMrp_PIODetails{2} Group By iPIOId) PIODet on PIO.iPIOId = PIODet.iPIOId  \r\n                                     where 1=1 {1}", oRec.UserId, text6, m_sSuffix);
			}
			_ = string.Empty;
			_ = string.Empty;
			string text31 = "";
			List<object> list4 = new List<object>();
			if (FConvert.GetInputValue(oRec.Inputs, 3) > 1)
			{
				int inputValue = FConvert.GetInputValue(oRec.Inputs, 4);
				int inputValue2 = FConvert.GetInputValue(oRec.Inputs, 5);
				list4 = (from p in oRec.Inputs
					where p.ID == 6
					select p.Value).ToList();
				bool bExclude2 = false;
				COptionbase cOptionbase = new COptionbase();
				if (inputValue > 2)
				{
					string text32 = cOptionbase.CheckRoleMasters(oRec.UserId, inputValue.ToString(), IsMasterName: false, 1, iCompanyId, ref bExclude2);
					if (text32 != string.Empty)
					{
						text31 = ((!bExclude2) ? $" AND isnull(iInvTag,0) in ({text32})" : $" AND isnull(iInvTag,0) not in ({text32})");
					}
					standardQuery.Query += text31;
				}
				if (inputValue2 > 0)
				{
					string text33 = cOptionbase.CheckRoleMasters(oRec.UserId, inputValue2.ToString(), IsMasterName: false, 1, iCompanyId, ref bExclude2);
					if (text33 != string.Empty)
					{
						text31 = ((!bExclude2) ? $" AND isnull(iFaTag,0) in ({text33})" : $" AND isnull(iFaTag,0) not in ({text33})");
					}
					standardQuery.Query += text31;
				}
				if (list4.Count > 0)
				{
					for (int num = 0; num < list4.Count; num++)
					{
						string arg = "iTag" + Convert.ToString(list4[num]);
						string text34 = cOptionbase.CheckRoleMasters(oRec.UserId, list4[num].ToString(), IsMasterName: false, 1, iCompanyId, ref bExclude2);
						if (text34 != string.Empty)
						{
							text31 = ((!bExclude2) ? $" AND PIOTags.{arg} in ({text34})" : $" AND PIOTags.{arg} not in ({text34})");
						}
						standardQuery.Query += text31;
					}
				}
			}
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" PIO.iPIOId desc", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		case FocusReport.MapTestToItems:
			if (oRec.FilterSource != null && oRec.FilterSource.Length != 0)
			{
				text6 = transactions.GetProductionFilter(oRec, iCompanyId);
				if (!string.IsNullOrEmpty(text6))
				{
					text6 = " where " + text6;
				}
			}
			standardQuery = new StandardQuery();
			standardQuery.Query = string.Format("select iMasterId,sName,sCode,sDocList,sTestList  from (\r\n                                                        SELECT t2.iMasterId,PL.sName,t2.sCode, STUFF((SELECT ',' + CAST(t3.sDocNo AS varchar) FROM mQC_ProductTestMapping t1 \r\n                                                        inner join mQC_TestDefinitionHeader t3 on t3.iTestDefHeaderId=t1.iTestDefHeaderId\r\n                                                        where t2.iMasterId = t1.iProductId FOR XML PATH('')), 1 ,1, '') AS sDocList,\r\n                                                        STUFF((SELECT ',' + CAST(t3.sTestName AS varchar) FROM mQC_ProductTestMapping t1 \r\n                                                        inner join mQC_TestDefinitionHeader t3 on t3.iTestDefHeaderId=t1.iTestDefHeaderId and t3.iStatus<>0\r\n                                                        where t2.iMasterId = t1.iProductId FOR XML PATH('')), 1 ,1, '') AS sTestList\r\n                                                        FROM vCore_Product t2\r\n                                                        inner JOIN mCore_ProductLanguage PL ON t2.iMasterId = PL.iMasterId And  PL.iLanguageId={2}\r\n                                                        {1}\r\n                                                        GROUP BY t2.iMasterId,PL.sName,t2.sCode) a where a.sDocList is not null and  a.sTestList is not null and 1=1 ", oRec.UserId, text6, oRec.LanguageId);
			if (oRec.ColumnSort == null)
			{
				standardQuery.PrimaryColumn = string.Format(" iMasterId", m_sSuffix);
			}
			else
			{
				standardQuery.PrimaryColumn = string.Format("{0} {1}", oRec.ColumnSort.OrderId, oRec.ColumnSort.IsAscending ? "ASC" : "DESC");
			}
			break;
		}
		return standardQuery;
	}

	private string GetPOFilterOnUser(int iUserId, int iCompanyId)
	{
		string text = null;
		text = "Select a.name From sys.columns a\r\n                            join sys.views b on a.object_id = b.object_id where b.name = 'vMRP_ProdOrderWithTags" + FConvert.GetSuffix(iCompanyId) + "'";
		IDataReader dataReader = m_db.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		List<string> list = new List<string>();
		while (dataReader.Read())
		{
			list.Add(dataReader[0].ToString());
		}
		dataReader.Close();
		text = "";
		text = $" Select distinct iProdOrderId From vMRP_ProdOrderWithTags{FConvert.GetSuffix(iCompanyId)} Where 1=1 ";
		List<FieldData> arrMasterField = new List<FieldData>();
		List<MasterRights> list2 = new COptionbase().CheckRoleMasters(iUserId, "1,3,5,7", ref arrMasterField, m_db, iCompanyId);
		for (int i = 0; i < list2.Count; i++)
		{
			if (list2[i].ID == _focus.company(iCompanyId).faTagId)
			{
				text = text + " And isnull(iFaTag,0) in (" + list2[i].Tag + ")";
			}
			else if (list2[i].ID == _focus.company(iCompanyId).invTagId)
			{
				text = text + " And isnull(iInvTag,0) in (" + list2[i].Tag + ")";
			}
			else if (list.Contains($"iTag{list2[i].ID}"))
			{
				text += $" And isnull(iTag{list2[i].ID},0) in ({list2[i].Tag})";
			}
		}
		if (list2.Count == 0)
		{
			text = "";
		}
		return text;
	}

	private LineData[] CalculateBudgetBalance(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		int num = 0;
		decimal num2 = 0m;
		decimal num3 = 0m;
		decimal num4 = 0m;
		decimal num5 = 0m;
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (num = 0; num < list.Count; num++)
		{
			num2 = 0m;
			num2 = Convert.ToDecimal(list[num].CellData[5]) - Convert.ToDecimal(list[num].CellData[6]) - Convert.ToDecimal(list[num].CellData[7]) - Convert.ToDecimal(list[num].CellData[8]) + Convert.ToDecimal(list[num].CellData[9]);
			list[num].CellData[10] = num2;
			num3 = Convert.ToDecimal(list[num].CellData[12]) - Convert.ToDecimal(list[num].CellData[13]) - Convert.ToDecimal(list[num].CellData[14]) - Convert.ToDecimal(list[num].CellData[15]) + Convert.ToDecimal(list[num].CellData[16]);
			list[num].CellData[17] = num3;
			num4 = 0m;
			if (Convert.ToDecimal(list[num].CellData[5]) != 0m)
			{
				num4 = (Convert.ToDecimal(list[num].CellData[6]) + Convert.ToDecimal(list[num].CellData[7]) + Convert.ToDecimal(list[num].CellData[8])) / (Convert.ToDecimal(list[num].CellData[5]) + Convert.ToDecimal(list[num].CellData[9])) * 100m;
				list[num].CellData[11] = num4;
			}
			else
			{
				list[num].CellData[11] = 0;
			}
			if (Convert.ToDecimal(list[num].CellData[12]) != 0m)
			{
				num5 = (Convert.ToDecimal(list[num].CellData[13]) + Convert.ToDecimal(list[num].CellData[14]) + Convert.ToDecimal(list[num].CellData[15])) / (Convert.ToDecimal(list[num].CellData[12]) + Convert.ToDecimal(list[num].CellData[16])) * 100m;
				list[num].CellData[18] = num5;
			}
			else
			{
				list[num].CellData[18] = 0;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSBilling(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(((ReportRowHeader)list[i].CellData[0]).Id) == 0)
			{
				list[i].CellData[1] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[2]))
			{
			case 0:
				list[i].CellData[2] = "CBM";
				break;
			case 1:
				list[i].CellData[2] = "Quantity";
				break;
			case 2:
				list[i].CellData[2] = "Weight";
				break;
			case 3:
				list[i].CellData[2] = "Pallet";
				break;
			case 4:
				list[i].CellData[2] = "Hours";
				break;
			case 5:
				list[i].CellData[2] = "Bin";
				break;
			case 6:
				list[i].CellData[2] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSAdvBilling(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		string text = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.WMS, 37) switch
		{
			1 => "Pallet", 
			0 => "Skid", 
			_ => "LP", 
		};
		for (int i = 0; i < list.Count; i++)
		{
			switch (Convert.ToInt16(list[i].CellData[2]))
			{
			case 1:
				list[i].CellData[2] = "Receiving";
				break;
			case 2:
				list[i].CellData[2] = "Shipment";
				break;
			case 3:
				list[i].CellData[2] = "Storage";
				break;
			case 4:
				list[i].CellData[2] = "Pick & Pack";
				break;
			case 5:
				list[i].CellData[2] = "VAS";
				break;
			case 6:
				list[i].CellData[2] = "Recurring";
				break;
			case 7:
				list[i].CellData[2] = "Return";
				break;
			case 8:
				list[i].CellData[2] = "Work Order";
				break;
			default:
				list[i].CellData[2] = "";
				break;
			}
			switch (Convert.ToInt16(list[i].CellData[3]))
			{
			case 1:
				list[i].CellData[3] = "Daily Accumulation";
				break;
			case 2:
				list[i].CellData[3] = "At Peak Inventory";
				break;
			case 3:
				list[i].CellData[3] = "At Period End";
				break;
			case 4:
				list[i].CellData[3] = "Distinct Period Count";
				break;
			case 5:
				list[i].CellData[3] = "Average Calculation";
				break;
			default:
				list[i].CellData[3] = "";
				break;
			}
			switch (Convert.ToInt16(list[i].CellData[6]))
			{
			case 1:
				list[i].CellData[6] = "Per Item";
				break;
			case 2:
				list[i].CellData[6] = "Per Unit";
				break;
			case 3:
				list[i].CellData[6] = "Per Item & Per Unit";
				break;
			case 4:
				list[i].CellData[6] = "Per Bin";
				break;
			case 5:
				list[i].CellData[6] = "Per " + text;
				break;
			case 6:
				list[i].CellData[6] = "Per CBM";
				break;
			case 7:
				list[i].CellData[6] = "Fix Charge";
				break;
			case 8:
				list[i].CellData[6] = "Recurring";
				break;
			case 9:
				list[i].CellData[6] = "Operation";
				break;
			case 10:
				list[i].CellData[6] = "Per " + text + " No";
				break;
			default:
				list[i].CellData[6] = "";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSAdvServiceCharge(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		string text = _focus.company(m_iCompanyId).getPreferenceValue(PreferenceCategories.WMS, 37) switch
		{
			1 => "Pallet", 
			0 => "Skid", 
			_ => "LP", 
		};
		for (int i = 0; i < list.Count; i++)
		{
			switch (Convert.ToInt16(list[i].CellData[4]))
			{
			case 1:
				list[i].CellData[4] = "Per Item";
				break;
			case 2:
				list[i].CellData[4] = "Per Unit";
				break;
			case 3:
				list[i].CellData[4] = "Per Item & Per Unit";
				break;
			case 4:
				list[i].CellData[4] = "Per Bin";
				break;
			case 5:
				list[i].CellData[4] = "Per " + text;
				break;
			case 6:
				list[i].CellData[4] = "Per CBM";
				break;
			case 7:
				list[i].CellData[4] = "Fix Charge";
				break;
			case 8:
				list[i].CellData[4] = "Recurring";
				break;
			case 9:
				list[i].CellData[4] = "Operation";
				break;
			case 10:
				list[i].CellData[4] = "Per " + text + " No";
				break;
			default:
				list[i].CellData[4] = "";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSBillingSKUWise(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(((ReportRowHeader)list[i].CellData[0]).Id) == 0)
			{
				list[i].CellData[3] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[4]))
			{
			case 0:
				list[i].CellData[4] = "CBM";
				break;
			case 1:
				list[i].CellData[4] = "Quantity";
				break;
			case 2:
				list[i].CellData[4] = "Weight";
				break;
			case 3:
				list[i].CellData[4] = "Pallet";
				break;
			case 4:
				list[i].CellData[4] = "Hours";
				break;
			case 5:
				list[i].CellData[4] = "Bin";
				break;
			case 6:
				list[i].CellData[4] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSBillingSKUBatchWise(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(((ReportRowHeader)list[i].CellData[0]).Id) == 0)
			{
				list[i].CellData[1] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[2]))
			{
			case 0:
				list[i].CellData[2] = "CBM";
				break;
			case 1:
				list[i].CellData[2] = "Quantity";
				break;
			case 2:
				list[i].CellData[2] = "Weight";
				break;
			case 3:
				list[i].CellData[2] = "Pallet";
				break;
			case 4:
				list[i].CellData[2] = "Hours";
				break;
			case 5:
				list[i].CellData[2] = "Bin";
				break;
			case 6:
				list[i].CellData[2] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateWMSCurrentStock(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateWMSSKUwiseInvTrans(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateExpiryDate(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		new List<LineData>();
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateBinOccupancy(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count(); i++)
		{
			if (list[i].CellData[3] == null || Convert.ToDecimal(list[i].CellData[3]) < 0m)
			{
				list[i].CellData[3] = 0;
			}
			if (list[i].CellData[7] != null)
			{
				list[i].CellData[8] = Convert.ToDecimal(list[i].CellData[7]) / Convert.ToDecimal(list[i].CellData[2]) * 100m;
			}
			else
			{
				list[i].CellData[8] = 0;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculatePendingAll(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateInvBal(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateEmptyLoc(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateSchedule(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(list[i].CellData[3]) == 0)
			{
				list[i].CellData[4] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[5]))
			{
			case 0:
				list[i].CellData[5] = "CBM";
				break;
			case 1:
				list[i].CellData[5] = "Quantity";
				break;
			case 2:
				list[i].CellData[5] = "Weight";
				break;
			case 3:
				list[i].CellData[5] = "Pallet";
				break;
			case 4:
				list[i].CellData[5] = "Hours";
				break;
			case 5:
				list[i].CellData[5] = "Bin";
				break;
			case 6:
				list[i].CellData[5] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateBillForecast(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(list[i].CellData[4]) == 0)
			{
				list[i].CellData[5] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[6]))
			{
			case 0:
				list[i].CellData[6] = "CBM";
				break;
			case 1:
				list[i].CellData[6] = "Quantity";
				break;
			case 2:
				list[i].CellData[6] = "Weight";
				break;
			case 3:
				list[i].CellData[6] = "Pallet";
				break;
			case 4:
				list[i].CellData[6] = "Hours";
				break;
			case 5:
				list[i].CellData[6] = "Bin";
				break;
			case 6:
				list[i].CellData[6] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateShortDate(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = new List<LineData>();
		list.AddRange(arrColumnData);
		return list.ToArray();
	}

	private LineData[] CalculateBillWeekWise(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (Convert.ToInt32(list[i].CellData[1]) == 0)
			{
				list[i].CellData[2] = "Storing Charges";
			}
			switch (Convert.ToInt16(list[i].CellData[4]))
			{
			case 0:
				list[i].CellData[4] = "CBM";
				break;
			case 1:
				list[i].CellData[4] = "Quantity";
				break;
			case 2:
				list[i].CellData[4] = "Weight";
				break;
			case 3:
				list[i].CellData[4] = "Pallet";
				break;
			case 4:
				list[i].CellData[4] = "Hours";
				break;
			case 5:
				list[i].CellData[4] = "Bin";
				break;
			case 6:
				list[i].CellData[4] = "Fix Type";
				break;
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculatePalletInPalletOut(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		List<LineData> list = null;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (int i = 0; i < list.Count; i++)
		{
			if (i != 0)
			{
				list[i].CellData[7] = Convert.ToDecimal(list[i - 1].CellData[7]) + Convert.ToDecimal(list[i].CellData[5]) - Convert.ToDecimal(list[i].CellData[6]);
			}
		}
		return list.ToArray();
	}

	private LineData[] CalculateAvailableQuantity(LineData[] arrColumnData, RepRecord objRec, int iCompId)
	{
		int num = 0;
		List<LineData> list = null;
		decimal num2 = 0m;
		list = new List<LineData>();
		list.AddRange(arrColumnData);
		for (num = 0; num < list.Count; num++)
		{
			num2 = Convert.ToDecimal(list[num].CellData[4]) + Convert.ToDecimal(list[num].CellData[5]);
			list[num].CellData[6] = num2;
		}
		return list.ToArray();
	}

	private string GetExtraColumnDetails(RepRecord oRec, ref string sMRPExtraColumns, int iCompId, List<string> lstExtraTableNames = null, bool bMRPExtraFields = false, int iFieldCount = 0)
	{
		FieldData[] arrFields = oRec.ExtraColumns;
		string text = string.Empty;
		List<IdNamePair> list = new List<IdNamePair>();
		if (bMRPExtraFields && iFieldCount > 0)
		{
			string[] array = arrFields.Select((FieldData f) => f.FieldName).ToArray();
			int[] array2 = (from p in oRec.ReportCustomize.Layout.Columns
				where p.FieldId > iFieldCount
				select p.FieldId).ToArray();
			if (array2.Length != 0)
			{
				list = GetMRPExtraFeildsColNames(array2);
			}
			if (list.Count > 0)
			{
				int k;
				for (k = 0; k < arrFields.Length; k++)
				{
					string text2 = (from m in list
						where m.ID == arrFields[k].FieldId
						select m.Name).FirstOrDefault();
					int num = 0;
					if (text2.Length > 0 && text2 != "")
					{
						for (int num2 = 0; num2 < array.Length; num2++)
						{
							if (arrFields[k].FieldName == array[num2])
							{
								array[num2] = text2;
								num++;
								break;
							}
						}
					}
					arrFields[k].FieldName = ((text2 != "") ? text2 : arrFields[k].FieldName);
				}
			}
		}
		FieldData[] array3 = arrFields;
		List<IdValuePair> list2 = new List<IdValuePair>();
		(list2 ?? (list2 = new List<IdValuePair>())).AddRange(array3?.Select((FieldData p, int idx) => new IdValuePair(idx, p)) ?? Enumerable.Empty<IdValuePair>());
		string[] array4 = null;
		string text3 = string.Empty;
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			FieldData objFieldData = (FieldData)list2[num3].Value;
			if (string.IsNullOrEmpty(objFieldData.FieldName))
			{
				continue;
			}
			array4 = ((!objFieldData.FieldName.StartsWith("dbo")) ? objFieldData.FieldName.Split(new char[1] { '.' }, 2) : objFieldData.FieldName.Split('.'));
			string sTableName = array4[0];
			bool flag = false;
			if (lstExtraTableNames != null && lstExtraTableNames.Count > 0)
			{
				flag = lstExtraTableNames.Any((string p) => p.Equals(sTableName, StringComparison.OrdinalIgnoreCase));
				if (flag && objFieldData.SubParentId == 0)
				{
					continue;
				}
			}
			if (!sTableName.StartsWith("vr") || !(sTableName != "vrCore_Product") || objFieldData.ParentId <= 0 || objFieldData.ParentId == 12)
			{
				continue;
			}
			if (objFieldData.SubParentId > 0 && objFieldData.SubParentId > 5000 && !flag)
			{
				string prodExtraFieldName = GetProdExtraFieldName(objFieldData.SubParentId);
				string[] array5 = null;
				array5 = prodExtraFieldName.Split(new char[1] { '.' }, 2);
				string text4 = prodExtraFieldName;
				array4[0] = array5[0];
				if (string.IsNullOrEmpty(text4))
				{
					continue;
				}
				if (sTableName == "vrCore_Units")
				{
					string text5 = (array4[0] = $"Units{objFieldData.ParentId}");
					string strColName = string.Join(".", array4);
					if (!text3.Contains(sTableName))
					{
						text = text + " LEFT JOIN " + sTableName + " " + text5 + " WITH (READUNCOMMITTED) ON " + text5 + ".sName = " + text4;
						sTableName = text5;
						text3 += "vrCore_Units,";
					}
					array3.Where((FieldData p) => p.FieldId == objFieldData.FieldId).ToList().ForEach((FieldData p) =>
					{
						p.FieldName = strColName;
					});
				}
				else if (!text3.Contains(sTableName))
				{
					text = text + " LEFT JOIN " + sTableName + " ON " + sTableName + ".sCode = " + text4 + "sCode AND " + sTableName + ".iTreeId = 0";
					text3 = text3 + sTableName + ",";
				}
			}
			else
			{
				if (!bMRPExtraFields)
				{
					continue;
				}
				int iFieldId = objFieldData.FieldId;
				if (objFieldData.SubParentId > 5000)
				{
					iFieldId = objFieldData.SubParentId;
				}
				string sColName = GetProdExtraFieldName(iFieldId);
				if (!string.IsNullOrEmpty(sColName))
				{
					sColName = "tbl_" + sColName;
					array3.Where((FieldData p) => p.FieldId == objFieldData.FieldId && p.FieldOrder == objFieldData.FieldOrder).ToList().ForEach((FieldData p) =>
					{
						p.FieldName = sColName;
					});
				}
			}
		}
		string text6 = string.Join(",", from p in array3
			where !string.IsNullOrEmpty(p?.FieldName)
			select p.FieldName);
		if (text6.Length > 0)
		{
			sMRPExtraColumns = "," + text6;
		}
		return text;
	}

	private string GetProdExtraFieldName(int iFieldId)
	{
		MasterDataType masterDataType = MasterDataType.Text;
		string result = string.Empty;
		try
		{
			string text = string.Format("SELECT 'vr'+sModule+'_'+sMasterName+'.'+sFieldName+'{1}'\r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {0}", iFieldId & 0xFFFFFF, (masterDataType == MasterDataType.DocumentViewer) ? "Name" : "");
			result = Convert.ToString(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	private List<IdNamePair> GetMRPExtraFeildsColNames(int[] arrIds)
	{
		List<IdNamePair> list = new List<IdNamePair>();
		try
		{
			string empty = string.Empty;
			if (arrIds != null && arrIds.Length != 0)
			{
				for (int i = 0; i < arrIds.Length; i++)
				{
					empty = $"select isnull(sName,'') from mMRP_ExtraFields where Id = {arrIds[i]}";
					string name = Convert.ToString(m_db.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty));
					IdNamePair idNamePair = new IdNamePair();
					idNamePair.ID = arrIds[i];
					idNamePair.Name = name;
					list.Add(idNamePair);
				}
			}
			return list;
		}
		catch (Exception)
		{
			return list = new List<IdNamePair>();
		}
	}
}
