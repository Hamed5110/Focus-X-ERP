using System;
using System.Collections.Generic;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Focus.TranSettings.DataStructs;
using Focus.Transactions.DataStructs;

namespace Focus.RD.BL;

public class RDServiceRequest
{
	private CalendarType m_objCalType;

	public bool IsSession(int iMethodId)
	{
		if (iMethodId == 0 || iMethodId == 33 || iMethodId == 100)
		{
			return false;
		}
		return true;
	}

	public Param CallWebMethod(Input objInput, int iCompanyId, CalendarType objCalType, string strSessionId)
	{
		Output output = null;
		InvoiceLayout invoiceLayout = new InvoiceLayout();
		invoiceLayout.m_objCalType = m_objCalType;
		output = new Output();
		RDMethods rDMethods = (RDMethods)objInput.Params[0];
		m_objCalType = objCalType;
		switch (rDMethods)
		{
		case RDMethods.TestService:
			output.Status = true;
			break;
		case RDMethods.LoadLayout:
			output.ReturnData = invoiceLayout.Load((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveInvoieLayout:
			output.ReturnData = invoiceLayout.Save((LayoutInformation)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteInvoiceLayout:
			output.ReturnData = invoiceLayout.Delete((int)objInput.Params[1], (MasterType)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.DeleteVoucherInvoiceLayout:
			output.ReturnData = invoiceLayout.DeleteVoucherInvoiceLayout((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadMasters:
			output.ReturnData = invoiceLayout.LoadMasters((string)objInput.Params[1], (MasterType)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetReportData:
			output.ReturnData = invoiceLayout.GetReportData((string)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadReportInvoiceLayout:
			output.ReturnData = invoiceLayout.LoadReportInvoiceLayout((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetReportInvoiceLayoutId:
			output.ReturnData = invoiceLayout.GetReportInvoiceLayoutId((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.EvaluateInvoiceFormula:
			output.ReturnData = invoiceLayout.EvaluateInvoiceFormula((string)objInput.Params[1], (int)objInput.Params[2], (Transaction)objInput.Params[3], (StaticTextClass)objInput.Params[4], (string[])objInput.Params[5], iCompanyId);
			break;
		case RDMethods.LoadMastersWithTransactions:
			if (objInput.Params[1].GetType() == typeof(RepRecord))
			{
				output.ReturnData = LoadMastersWithTransactions((RepRecord)objInput.Params[1], iCompanyId);
			}
			else
			{
				output.ReturnData = LoadMastersWithTransactions((Module)objInput.Params[1], (int)objInput.Params[2], (objInput.Params[3] != null) ? ((_Filter[])objInput.Params[3]) : null, iCompanyId);
			}
			break;
		case RDMethods.LoadLedgerMastersWithTransactions:
			if (objInput.Params[1].GetType() == typeof(RepRecord))
			{
				output.ReturnData = LoadLedgerMastersWithTransactions((RepRecord)objInput.Params[1], iCompanyId);
			}
			else
			{
				output.ReturnData = LoadLedgerMastersWithTransactions((int)objInput.Params[1], (int)objInput.Params[2], (objInput.Params[2] != null) ? ((_Filter[])objInput.Params[3]) : null, iCompanyId);
			}
			break;
		case RDMethods.LoadSubLedgerMastersWithTransactions:
			output.ReturnData = invoiceLayout.LoadSubLedgerMastersWithTransactions((int)objInput.Params[1], (int)objInput.Params[2], (objInput.Params[3] != null) ? ((_Filter[])objInput.Params[3]) : null, iCompanyId);
			break;
		case RDMethods.GetAccountProperty:
			output.ReturnData = invoiceLayout.getAccountProperty((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetTotalTrsanctionData:
			output.ReturnData = invoiceLayout.GetTotalPages(iCompanyId);
			break;
		case RDMethods.LoadPrintInvoiceLayout:
			output.ReturnData = invoiceLayout.LoadPrintingInvoice((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SavePrintInvoiceLayout:
			output.ReturnData = invoiceLayout.SavePrintingInvoice((LayoutInformation)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeletePrintInvoiceLayout:
			output.ReturnData = invoiceLayout.DeletePrintInvoiceLayout((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetMasterValueFromField:
			if (objInput.Params[1].GetType() == typeof(FieldInfoInput))
			{
				output.ReturnData = GetMasterValueFromField((FieldInfoInput)objInput.Params[1], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.GetMasterValueFromField((int)objInput.Params[1], (int)objInput.Params[2], (string)objInput.Params[3], (int)objInput.Params[4], iCompanyId);
			}
			break;
		case RDMethods.GetMasterValuesFromFields:
			if (objInput.Params[1].GetType() == typeof(FieldInfoInput))
			{
				output.ReturnData = invoiceLayout.GetMastersValueFromFields((FieldInfoInput)objInput.Params[1], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.GetMastersValueFromFields((int)objInput.Params[1], (int)objInput.Params[2], (string)objInput.Params[3], (int)objInput.Params[4], iCompanyId);
			}
			break;
		case RDMethods.LoadReportLayouts:
			if (objInput.Params.Length > 3)
			{
				output.ReturnData = LoadReportLayouts((uint)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			}
			else
			{
				output.ReturnData = LoadReportLayouts((uint)objInput.Params[1], (int)objInput.Params[2], 1, iCompanyId);
			}
			break;
		case RDMethods.LoadLayouts:
			output.ReturnData = invoiceLayout.LoadLayouts((Module)objInput.Params[1], (int)objInput.Params[2], (LayoutType)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.LoadCrossReferenceMasters:
			output.ReturnData = invoiceLayout.LoadCrossReferenceMasters((Module)objInput.Params[1], (uint)objInput.Params[2], (int)objInput.Params[3], (BackTrackType)objInput.Params[4], iCompanyId);
			break;
		case RDMethods.DeleteLayout:
			output.ReturnData = invoiceLayout.DeleteLayout((int)objInput.Params[1], (Module)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadTranMasters:
			output.ReturnData = invoiceLayout.LoadTranMasters((int[])objInput.Params[1], objInput.Params.Length > 2 && (bool)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetReportNameFromId:
			output.ReturnData = invoiceLayout.GetReportNameFromId((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadColumnSchemas:
			output.ReturnData = invoiceLayout.LoadColumnSchemas((string)objInput.Params[1], (string)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.CopyFileToServer:
			output.ReturnData = CopyFileToServer((string)objInput.Params[1], (string)objInput.Params[2], (bool)objInput.Params[3], (string)objInput.Params[4], (byte[])objInput.Params[5]);
			break;
		case RDMethods.SaveProvisionalData:
			output.ReturnData = SaveProvisionalData((ProvisionalEntryHeader)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadProvisionalData:
			output.ReturnData = LoadProvisionalData((ProvisionalType)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.SaveCashFlowData:
			output.ReturnData = SaveCashFlowData((CashFlowTemplateData[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadCashFlowData:
			output.ReturnData = LoadCashFlowData(iCompanyId);
			break;
		case RDMethods.GetPagewiseData:
			output.ReturnData = GetPagewiseData((RepRecord)objInput.Params[1], strSessionId, iCompanyId);
			break;
		case RDMethods.GetPagewiseDataLite:
			output.ReturnData = GetPagewiseDataLite((RepRecordLite)objInput.Params[1], strSessionId, iCompanyId);
			break;
		case RDMethods.GetNextPageDataLite:
			output.ReturnData = GetNextPageDataLite((NextRepRecordLite)objInput.Params[1], strSessionId, iCompanyId);
			break;
		case RDMethods.GetAnalyzeData:
			output.ReturnData = GetAnalyzeData((RepRecord)objInput.Params[1], strSessionId, iCompanyId);
			break;
		case RDMethods.EndOfReport:
			output.ReturnData = EndOfReport((string)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.FlushReport:
			output.ReturnData = FlushReportPageData((string)objInput.Params[1], strSessionId, iCompanyId);
			break;
		case RDMethods.EndAllReports:
			output.ReturnData = EndAllReports(strSessionId, (int)objInput.Params[2]);
			break;
		case RDMethods.GetOpenReports:
			output.ReturnData = GetAllReports();
			break;
		case RDMethods.LoadXReadingData:
			output.ReturnData = LoadXReadingData((XReadingHeader)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.PrepareGraphData:
		case RDMethods.UploadGraphImage:
			output.ReturnData = PrepareGraphData((RepRecord)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetReportGroupHeading:
			output.ReturnData = invoiceLayout.GetReportGroupHeading((uint)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.ExportRD:
			output.ReturnData = invoiceLayout.ExportRD((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.ImportRD:
			output.ReturnData = invoiceLayout.ImportRD((LayoutInformation[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.ExportBillPrinting:
			output.ReturnData = invoiceLayout.ExportBillPrinting((Module)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.ImportRDReports:
			output.ReturnData = ImportRDReports((_Reports[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.ExportRDReports:
			output.ReturnData = ExportRDReports((objInput.Params[1] == null) ? null : ((uint[])objInput.Params[1]), iCompanyId);
			break;
		case RDMethods.SavePOSBillFormat:
			output.ReturnData = invoiceLayout.SavePOSBillFormat((POSPrintFormat)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadPOSBillFormat:
			output.ReturnData = invoiceLayout.LoadPOSBillFormat((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeletePOSBillFormat:
			output.ReturnData = invoiceLayout.DeletePOSBillFormat((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DonotPrintProductForZeroRate:
			output.ReturnData = DonotPrintProductForZeroRate((List<IdNamePair>)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadMapLinkFieldIdies:
			output.ReturnData = LoadMapLinkFieldIdies((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadReportParameter:
			if (objInput.Params.Length > 2)
			{
				output.ReturnData = invoiceLayout.LoadReportParameter(Convert.ToInt32(objInput.Params[1]), (ReportType)objInput.Params[2], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.LoadReportParameter(Convert.ToInt32(objInput.Params[1]), ReportType.Standard, iCompanyId);
			}
			break;
		case RDMethods.GetDataStatistics:
			output.ReturnData = invoiceLayout.GetDataStatistics((DataStatsInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetReportList:
			output.ReturnData = invoiceLayout.GetReportList(Convert.ToInt32(objInput.Params[1]), iCompanyId);
			break;
		case RDMethods.GetMasterList:
			if (objInput.Params.Length > 2 && Convert.ToInt32(objInput.Params[1]) == _focus.company(iCompanyId).faTagId && objInput.Params[2] != null && objInput.Params[2].GetType() == typeof(_Filter[]))
			{
				RDReport rDReport = new RDReport();
				rDReport.m_objCalType = m_objCalType;
				output.ReturnData = rDReport.GetDepartmentList((_Filter[])objInput.Params[2], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.GetMasterList(Convert.ToInt32(objInput.Params[1]), iCompanyId);
			}
			break;
		case RDMethods.SaveReportDesigner:
			output.ReturnData = SaveReportDesigner((_Reports)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadReportDesigner:
			if (objInput.Params.Length > 3)
			{
				output.ReturnData = LoadReportDesigner((uint)objInput.Params[1], (int)objInput.Params[2], (uint)objInput.Params[3], iCompanyId);
			}
			else
			{
				output.ReturnData = LoadReportDesigner((uint)objInput.Params[1], (int)objInput.Params[2], 0u, iCompanyId);
			}
			break;
		case RDMethods.LoadReportColumns:
			output.ReturnData = LoadReportColumns((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetDocFieldsOfTransactionSet:
			if (objInput.Params.Length > 4)
			{
				output.ReturnData = invoiceLayout.GetDocFieldsOfTransactionSet((Module)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], (int)objInput.Params[4], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.GetDocFieldsOfTransactionSet((Module)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], 0, iCompanyId);
			}
			break;
		case RDMethods.GetDocFieldsOfTransactionSets:
			output.ReturnData = invoiceLayout.GetDocFieldsOfTransactionSets((_TransactionSet[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetMenuDetails:
			output.ReturnData = GetMenuDetails((bool)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteReportDesigner:
			output.ReturnData = DeleteReportDesigner((uint)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetSalesInfo:
			output.ReturnData = invoiceLayout.GetSalesInfo((int)objInput.Params[1], (int[])objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetParameterFieldId:
			output.ReturnData = GetParameterFieldId((MasterTypeId)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetVatFormsData:
			output.ReturnData = invoiceLayout.GetVatFormsData((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.LoadAllTextData:
			output.ReturnData = LoadAllTextData((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadRDVirtual:
			output.ReturnData = LoadRDVirtual((int)objInput.Params[1], (bool)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.SaveRDVirtual:
			output.ReturnData = SaveRDVirtual((_RDVirtual)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadVirtualFields:
			output.ReturnData = LoadVirtualFields(iCompanyId);
			break;
		case RDMethods.DeleteVirtualGrouping:
			output.ReturnData = DeleteVirtualGrouping((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.UpdateRDMenuGroup:
			output.ReturnData = UpdateRDMenuGroup((_Menu)objInput.Params[1], (MenuOptions)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadAllDatabase:
			output.ReturnData = LoadAllDatabase((string)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadSelectedAppMenus:
			output.ReturnData = invoiceLayout.LoadSelectedAppMenus((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.LoadAppMenus:
			if (objInput.Params.Length == 3)
			{
				output.ReturnData = invoiceLayout.LoadAppMenus((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.LoadAppMenus((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId, (bool)objInput.Params[3]);
			}
			break;
		case RDMethods.SaveAppData:
			output.ReturnData = invoiceLayout.SaveAppData((AppMenuData[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadPictureDocumentField:
			output.ReturnData = invoiceLayout.LoadPictureDocumentField((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadAccountEmails:
			output.ReturnData = invoiceLayout.LoadAccountEmails((int[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveReportFilter:
			output.ReturnData = SaveReportFilter((uint)objInput.Params[1], (_Filter[])objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.LoadReportFilter:
			output.ReturnData = LoadReportFilter((uint)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadReportTemplate:
			output.ReturnData = invoiceLayout.LoadReportTemplate((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveReportTemplate:
			output.ReturnData = invoiceLayout.SaveReportTemplate((PrintTemplate)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteReportTemplate:
			output.ReturnData = invoiceLayout.DeleteReportTemplate((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadAllTemplateName:
			output.ReturnData = invoiceLayout.LoadAllTemplateName(iCompanyId);
			break;
		case RDMethods.SaveAppGroupData:
			output.ReturnData = invoiceLayout.SaveAppGroupData((IdNamePair)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteAppGroupData:
			output.ReturnData = invoiceLayout.DeleteAppGroupData((IdNamePair[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.UpdateModulePatch:
			output.ReturnData = invoiceLayout.UpdateModulePatch((Module[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveReportScheduler:
			output.ReturnData = invoiceLayout.SaveReportScheduler((ReportSchedule)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadReportScheduler:
			output.ReturnData = invoiceLayout.LoadReportSchedulers((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteScheduler:
			output.ReturnData = invoiceLayout.DeleteScheduler((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadPOSPrintDetails:
		case RDMethods.LoadPOSPrintDetailsFromSocket:
			if (objInput.Params.Length >= 2 && objInput.Params[1] != null && objInput.Params[1].GetType() == typeof(POSSocketInput))
			{
				output.ReturnData = LoadPOSPrintDetailsWithPrinter((POSSocketInput)objInput.Params[1], iCompanyId);
			}
			else if (objInput.Params.Length >= 7)
			{
				output.ReturnData = LoadPOSPrintDetails((int[])objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], (int)objInput.Params[4], (int)objInput.Params[5], (int)objInput.Params[6], iCompanyId, (int)objInput.Params[7]);
			}
			else
			{
				output.ReturnData = LoadPOSPrintDetails((int[])objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], (int)objInput.Params[4], (int)objInput.Params[5], (int)objInput.Params[6], iCompanyId);
			}
			break;
		case RDMethods.LogRestPrinter:
			output.ReturnData = LogRestPrinter((RestPOSPrintLog[])objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.LoadCurrencyData:
			output.ReturnData = invoiceLayout.LoadCurrencyData((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadAllCurrencyData:
			output.ReturnData = invoiceLayout.LoadAllCurrencyData(iCompanyId);
			break;
		case RDMethods.GetUsersContactDetails:
			output.ReturnData = invoiceLayout.GetUsersContactDetails((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetAgeingDetails:
			output.ReturnData = invoiceLayout.GetAgeingDetails((AccountAgeingInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetTableViewListData:
			if (objInput.Params.Length >= 3)
			{
				output.ReturnData = invoiceLayout.GetTableViewListData((ListControlData)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			}
			else
			{
				output.ReturnData = invoiceLayout.GetTableViewListData((ListControlData)objInput.Params[1], 0, iCompanyId);
			}
			break;
		case RDMethods.GetTransactionSetTypes:
			output.ReturnData = invoiceLayout.GetTransactionSetTypes((Module)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetMemberStatus:
			output.ReturnData = GetMemberStatus((MemberInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetOpenOrders:
			output.ReturnData = GetOpenOrders(iCompanyId);
			break;
		case RDMethods.GetOrderHistory:
			output.ReturnData = GetOrderHistory(iCompanyId, (long)objInput.Params[1], (long)objInput.Params[2]);
			break;
		case RDMethods.UpdateStatus:
			output.ReturnData = UpdateStatus(iCompanyId, (string)objInput.Params[1]);
			break;
		case RDMethods.SaveNonMemberValue:
			output.ReturnData = SaveNonMemberValue((NonMemberData)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveCashFlowTemplate:
			output.ReturnData = SaveCashFlowTemplate((CalRecord)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadCashFlowTemplate:
			output.ReturnData = LoadCashFlowTemplate((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.DeleteCashFlowTemplate:
			output.ReturnData = DeleteCashFlowTemplate((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetCashFlowCalendarData:
			output.ReturnData = GetCashFlowCalendarData((CalRecord)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetOrderByVoucher:
			output.ReturnData = invoiceLayout.GetOrderByVoucher((OrderByInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetPendingBills:
			output.ReturnData = GetPendingBill((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetMasterNameAndCode:
			output.ReturnData = GetMasterNameAndCode((int)objInput.Params[1], (string)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetVATSummaryReportData:
			output.ReturnData = GetVATSummaryData((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.GetVATAuditFileData:
			output.ReturnData = GetVATAuditFileData((VATAuditFileInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetVATReturnsReport:
			output.ReturnData = GetVATReturnsReportData((VATAuditFileInput)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetVoucherTypeRestriction:
			output.ReturnData = GetVoucherTypeRestriction((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.SaveScreenCustomization:
			output.ReturnData = SaveScreenCustomization((ScreenCustomization)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadScreenCustomization:
			output.ReturnData = LoadScreenCustomization((int)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadGSTAuditFileData:
			output.ReturnData = LoadGSTAuditFileData((VATAuditFileInput)objInput.Params[1], objInput.Params[2].ToString(), iCompanyId);
			break;
		case RDMethods.GetTagwiseStocks:
			output.ReturnData = GetTagwiseStocks((int)objInput.Params[1], (int)objInput.Params[2], iCompanyId);
			break;
		case RDMethods.GetStockData:
			if (objInput.Params.Length >= 3)
			{
				output.ReturnData = GetStockData((InputStockData)objInput.Params[1], iCompanyId, Convert.ToString(objInput.Params[2]));
			}
			else
			{
				output.ReturnData = GetStockData((InputStockData)objInput.Params[1], iCompanyId, null);
			}
			break;
		case RDMethods.InitializeSocket:
		{
			Output obj3 = new Output();
			output.ReturnData = InitializeSocket(iCompanyId);
			return Compression.Convert(obj3);
		}
		case RDMethods.DisableSocket:
		{
			Output obj2 = new Output();
			output.ReturnData = DisableSocket(iCompanyId);
			return Compression.Convert(obj2);
		}
		case RDMethods.EnableSocket:
		{
			Output obj = new Output();
			output.ReturnData = EnableSocket(iCompanyId);
			return Compression.Convert(obj);
		}
		case RDMethods.GetAttachmentData:
			output.ReturnData = invoiceLayout.GetAttachmentData((IdNamePair)objInput.Params[1], iCompanyId);
			break;
		case RDMethods.LoadMRPLayoutId:
			output.ReturnData = LoadMRPLayoutId((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], iCompanyId);
			break;
		case RDMethods.GetApprovalHistory:
			output.ReturnData = invoiceLayout.GetApprovalHistory((int)objInput.Params[1], null, iCompanyId);
			break;
		case RDMethods.GetMaxApprovalLevel:
			output.ReturnData = GetMaxApprovalLevel(iCompanyId);
			break;
		case RDMethods.GetGLAndInventoryVoucherwise:
			output.ReturnData = GetGLAndInventoryVoucherwise((IdValuePair[])objInput.Params[1], iCompanyId);
			break;
		case RDMethods.GetBatchNegitiveTransData:
		{
			string strHeader = "";
			output.ReturnData = new object[2]
			{
				GetBatchNegitiveTransData((IdValuePair[])objInput.Params[1], ref strHeader, iCompanyId),
				strHeader
			};
			break;
		}
		case RDMethods.getAuthorizationHistory:
			if (objInput.Params.Length > 5)
			{
				output.ReturnData = getAuthorizationHistory((int)objInput.Params[1], (Dictionary<string, string>)objInput.Params[2], (string)objInput.Params[3], (CalendarType)objInput.Params[4], (double)objInput.Params[5], iCompanyId);
			}
			if (objInput.Params.Length > 4)
			{
				output.ReturnData = getAuthorizationHistory((int)objInput.Params[1], (Dictionary<string, string>)objInput.Params[2], (string)objInput.Params[3], (CalendarType)objInput.Params[4], 0.0, iCompanyId);
			}
			else
			{
				output.ReturnData = getAuthorizationHistory((int)objInput.Params[1], null, null, CalendarType.Gregorean, 0.0, iCompanyId);
			}
			break;
		case RDMethods.GetQuotationVouchers:
		{
			List<IdNamePair> arrLinkVouchers = new List<IdNamePair>();
			GetQuotationVouchers((int)objInput.Params[1], (int)objInput.Params[2], (bool)objInput.Params[3], (int)objInput.Params[4], ref arrLinkVouchers, bDocumentClass: false);
			output.ReturnData = arrLinkVouchers;
			break;
		}
		case RDMethods.GetAuditLogDetails:
		{
			string sError2 = null;
			TranAuditDetailData[] auditLogDetails = GetAuditLogDetails((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], (objInput.Params[4] != null) ? ((VWVoucherFields[])objInput.Params[4]) : null, (int)objInput.Params[5], iCompanyId, objCalType, ref sError2);
			output.Message = sError2;
			output.ReturnData = auditLogDetails;
			break;
		}
		case RDMethods.GetLockUnlockAuditDetails:
		{
			string sError = null;
			TranAuditDetailData[] lockUnlockAuditDetails = GetLockUnlockAuditDetails((int)objInput.Params[1], (int)objInput.Params[2], (string)objInput.Params[3], iCompanyId, objCalType, ref sError);
			output.Message = sError;
			output.ReturnData = lockUnlockAuditDetails;
			break;
		}
		case RDMethods.GetInterestDetails:
			output.ReturnData = GetInterestDetails((int)objInput.Params[1], (int)objInput.Params[2], (int)objInput.Params[3], (decimal)objInput.Params[4], (bool)objInput.Params[5], (bool)objInput.Params[5], iCompanyId);
			break;
		}
		return Compression.Convert(output);
	}

	private ReturnStatus SaveScreenCustomization(ScreenCustomization oData, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.SaveScreenCustomization(oData, iCompanyId);
	}

	private Output LoadScreenCustomization(int oData, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.LoadScreenCustomization(oData, iCompanyId);
	}

	private IdNamePair[] LoadMastersWithTransactions(RepRecord objRec, int iCompanyId)
	{
		InvoiceLayout invoiceLayout = new InvoiceLayout();
		invoiceLayout.m_objCalType = m_objCalType;
		switch ((FocusReport)objRec.ReportId)
		{
		case FocusReport.CustomerBillwiseSummary:
		case FocusReport.VendorListingofOutstandingBills:
		case FocusReport.VendorStatements:
		case FocusReport.VendorDueDateAnalysis:
		case FocusReport.VendorAgeingSummaryBillwise:
		case FocusReport.VendorDetailAgeingByDueDate:
		case FocusReport.VendorSummaryAgeingByDueDate:
		case FocusReport.VendorOverdueAnalysis:
		case FocusReport.VendorOverdueSummary:
		case FocusReport.CustomerListingofOutstandingBills:
		case FocusReport.CustomerStatements:
		case FocusReport.CustomerDueDateAnalysis:
		case FocusReport.CustomerAgeingSummaryBillwise:
		case FocusReport.CustomerDetailAgeingByDueDate:
		case FocusReport.CustomerSummaryAgeingByDueDate:
		case FocusReport.CustomerOverdueAnalysis:
		case FocusReport.CustomerOverdueSummary:
		case FocusReport.VendorBillwiseSummary:
			return invoiceLayout.LoadMastersForBillwise(objRec.UserId, objRec.ReportId, objRec.Masters, objRec.FilterSource, iCompanyId);
		default:
			return invoiceLayout.LoadMastersWithTransactions(objRec.Module, objRec.UserId, objRec.FilterSource, objRec.LanguageId, iCompanyId);
		}
	}

	private IdNamePair[] LoadMastersWithTransactions(Module oModule, int iUserId, _Filter[] arrReportFilter, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.LoadMastersWithTransactions(oModule, iUserId, arrReportFilter, 0, iCompanyId);
	}

	private IdNamePair[] LoadLedgerMastersWithTransactions(RepRecord objRec, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.LoadLedgerMastersWithTransactions(objRec, iCompanyId);
	}

	private IdNamePair[] LoadLedgerMastersWithTransactions(int iUserId, int iStartDate, _Filter[] arrReportFilter, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.LoadLedgerMastersWithTransactions(iUserId, iStartDate, arrReportFilter, iCompanyId);
	}

	private string GetMasterValueFromField(FieldInfoInput oInput, int iCompId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.GetMasterValueFromField(oInput.FieldId, oInput.SubParentId, oInput.MasterIds, oInput.AltLanguageId, iCompId);
	}

	private IdNamePair[] LoadReportLayouts(uint iReportId, int iSubReportId, int iUserId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadReportLayouts(iReportId, iSubReportId, iUserId, iCompanyId);
	}

	private string CopyFileToServer(string sReportName, string sModule, bool IsExcel, string sExt, byte[] arrData)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.CopyFileToServer(sReportName, sModule, IsExcel, sExt, arrData);
	}

	private string SaveProvisionalData(ProvisionalEntryHeader objHeader, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveProvisionalData(objHeader, iCompId);
	}

	private ProvisionalEntryHeader LoadProvisionalData(ProvisionalType oType, int iDate, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadProvisionalData(oType, iDate, iCompId);
	}

	private string SaveCashFlowData(CashFlowTemplateData[] arrData, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveCashFlowData(arrData, iCompId);
	}

	private CashFlowTemplateData[] LoadCashFlowData(int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadCashFlowData(iCompId);
	}

	private ReportMatrix GetPagewiseData(RepRecord objRec, string strSessionId, int iCompanyId)
	{
		int rowsPerPage = objRec.RowsPerPage;
		int num = 0;
		ReportMatrix reportMatrix = null;
		RDReport rDReport = new RDReport();
		List<LineData> list = null;
		rDReport.m_objCalType = m_objCalType;
		if (objRec.Masters != null && objRec.Masters.Length != 0)
		{
			rDReport.SELECTED_MASTERS = objRec.Masters;
		}
		objRec.SessionId = strSessionId;
		if (objRec.ReportId == 586 && objRec.FetchAllData)
		{
			objRec.RowsPerPage = 100;
		}
		list = new List<LineData>();
		do
		{
			reportMatrix = rDReport.GetPagewiseData(objRec, iCompanyId);
			objRec.UniqueId = reportMatrix.UniqueId;
			objRec.IsStartOfReport = false;
			if (reportMatrix.ColumnData != null)
			{
				num += getExactLineCount(reportMatrix.ColumnData, rowsPerPage, objRec.ReportId);
				list.AddRange(reportMatrix.ColumnData);
			}
			if (rowsPerPage <= 0)
			{
				break;
			}
			objRec.RowsPerPage = ((rowsPerPage - num > 1) ? (rowsPerPage - num) : 2);
		}
		while (!reportMatrix.EndOfFile && reportMatrix.ColumnData != null && num < rowsPerPage);
		reportMatrix.ColumnData = list.ToArray();
		objRec.RowsPerPage = rowsPerPage;
		return reportMatrix;
	}

	private int getExactLineCount(LineData[] arrLine, int iRowPerPage, uint iReportId)
	{
		int num = arrLine.Length;
		if (arrLine.Length < iRowPerPage && arrLine.Length != 0 && (iReportId == 500 || iReportId == 551))
		{
			num += ((((ReportRowHeader)arrLine[0].CellData[0]).RowType != ReportRowType.OpenigClosingBalance || arrLine.Length != 1) ? 1 : 2);
		}
		return num;
	}

	private ReportMatrix GetAnalyzeData(RepRecord objRec, string strSessionId, int iCompanyId)
	{
		int rowsPerPage = objRec.RowsPerPage;
		ReportMatrix reportMatrix = null;
		RDReport rDReport = new RDReport();
		List<LineData> list = null;
		IdNamePair[] array = null;
		rDReport.m_objCalType = m_objCalType;
		objRec.SessionId = strSessionId;
		list = new List<LineData>();
		do
		{
			reportMatrix = rDReport.GetAnalyzeData(objRec, iCompanyId);
			objRec.UniqueId = reportMatrix.UniqueId;
			if (array == null)
			{
				array = reportMatrix.Columns;
			}
			objRec.CurrentPage++;
			objRec.Outputs = reportMatrix.BalanceValues;
			objRec.IsStartOfReport = false;
			if (reportMatrix.ColumnData != null)
			{
				list.AddRange(reportMatrix.ColumnData);
			}
			if (rowsPerPage <= 0)
			{
				break;
			}
			objRec.RowsPerPage = ((rowsPerPage - list.Count > 1) ? (rowsPerPage - list.Count) : 2);
		}
		while (!reportMatrix.EndOfFile && reportMatrix.ColumnData != null && list.Count < rowsPerPage);
		reportMatrix.ColumnData = list.ToArray();
		objRec.RowsPerPage = rowsPerPage;
		if (array != null && reportMatrix.Columns.Length != array.Length)
		{
			reportMatrix.Columns = array;
		}
		return reportMatrix;
	}

	private Dictionary<string, double>[] PrepareGraphData(RepRecord objRecord, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.PrepareGraphData(objRecord, iCompanyId);
	}

	private bool EndOfReport(string objKey, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.EndOfReport(objKey, iCompId);
	}

	private bool FlushReportPageData(string objKey, string strSession, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.FlushReportPageData(objKey, strSession, iCompId);
	}

	private bool EndAllReports(string sSessionId, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.EndAllReports(sSessionId, iCompId);
	}

	private IdNamePair[] GetAllReports()
	{
		return new RDReport().GetAllReports();
	}

	private ReportMatrix GetPagewiseDataLite(RepRecordLite objRec, string strSessionId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetPagewiseDataLite(objRec, strSessionId, iCompanyId);
	}

	private ReportMatrix GetNextPageDataLite(NextRepRecordLite oRecLite, string strSessionId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetNextPageDataLite(oRecLite, strSessionId, iCompanyId);
	}

	private PosXReading LoadXReadingData(XReadingHeader objHeader, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadXReadingData(objHeader, iCompanyId);
	}

	private IdNamePair[] ImportRDReports(_Reports[] arrReports, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.ImportRDReports(arrReports, iCompanyId);
	}

	private _Reports[] ExportRDReports(uint[] arrReportId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.ExportRDReports(arrReportId, iCompanyId);
	}

	private IdNamePair[] DonotPrintProductForZeroRate(List<IdNamePair> arrProducts, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.DonotPrintProductForZeroRate(arrProducts, iCompanyId);
	}

	private List<VWVoucherTriggersFldMap> LoadMapLinkFieldIdies(int iLinkPathId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadMapLinkFieldIdies(iLinkPathId, iCompanyId);
	}

	private IdNamePair SaveReportDesigner(_Reports objReports, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveReportDesigner(objReports, iCompanyId);
	}

	private _Menu[] GetMenuDetails(bool bGroup, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetMenuDetails(bGroup, iCompanyId);
	}

	private string DeleteReportDesigner(uint iReportId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.DeleteReportDesigner(iReportId, iCompanyId);
	}

	private _Reports LoadReportDesigner(uint iReportId, int iLayoutId, uint iSubReportId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadReportDesigner(iReportId, iLayoutId, iCompanyId, iSubReportId);
	}

	private _Column[] LoadReportColumns(int iLayoutId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadColumns(iLayoutId, IsAnalyzeTypeReport: false, DataSourceType.Cubes, iCompanyId);
	}

	private int GetParameterFieldId(MasterTypeId iMasterTypeId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetParameterFieldId(iMasterTypeId, iCompanyId);
	}

	private string[] LoadAllTextData(int iFieldId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadAllTextData(iFieldId, iCompanyId);
	}

	private _RDVirtual LoadRDVirtual(int iFieldId, bool bSearchByField, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadRDVirtual(iFieldId, bSearchByField, iCompanyId);
	}

	private IdNamePair[] LoadVirtualFields(int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadVirtualFields(iCompanyId);
	}

	private string DeleteVirtualGrouping(int iFieldId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.DeleteVirtualGrouping(iFieldId, iCompanyId);
	}

	private IdNamePair SaveRDVirtual(_RDVirtual oData, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveRDVirtual(oData, iCompanyId);
	}

	private _Menu UpdateRDMenuGroup(_Menu oMenu, MenuOptions eMenuOpt, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.UpdateRDMenuGroup(oMenu, eMenuOpt, iCompanyId);
	}

	private ComboData[] LoadAllDatabase(string sConnectionString, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadAllDatabase(sConnectionString, iCompanyId);
	}

	private string SaveReportFilter(uint iReportId, _Filter[] arrFilter, int iUserId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveReportFilter(iReportId, arrFilter, iUserId, iCompanyId);
	}

	private _Filter[] LoadReportFilter(uint iReportId, int iUserId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadReportFilter(iReportId, iUserId, iCompanyId);
	}

	private POSPrinterDetails LoadPOSPrintDetails(int[] arrBodyIds, int iType, int iKOTId, int iCounter, int iOrderType, int iLoginId, int iCompanyId, int iOutletId = -1)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadPOSPrintDetails(arrBodyIds, iType, iKOTId, iCounter, iOrderType, iLoginId, iCompanyId, iOutletId);
	}

	private POSPrinterDetails LoadPOSPrintDetailsWithPrinter(POSSocketInput oData, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadPOSPrintDetailsWithPrinter(oData, iCompanyId);
	}

	private bool LogRestPrinter(RestPOSPrintLog[] arrLogs, int iOrderType, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LogRestPrinter(arrLogs, iOrderType, iCompanyId);
	}

	private int GetMaxApprovalLevel(int iCompanyId)
	{
		return new QueryGenerator().GetMaxApprovalLevel(iCompanyId);
	}

	private MemberOutput[] GetMemberStatus(MemberInput objValue, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetMemberStatus(objValue, iCompanyId);
	}

	private string SaveNonMemberValue(NonMemberData objValue, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveNonMemberValue(objValue, iCompanyId);
	}

	private OpenOrders[] GetOpenOrders(int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetOpenOrders(iCompanyId);
	}

	private OpenOrders[] GetOrderHistory(int iCompanyId, long iFromDate, long iToDate)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetOrderHistory(iCompanyId, iFromDate, iToDate);
	}

	private bool UpdateStatus(int iCompanyId, string sVoucherNo)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.UpdateStatus(iCompanyId, sVoucherNo);
	}

	private string GetVoucherTypeRestriction(int iUserId, int iCompanyId)
	{
		return new QueryGenerator().GetVoucherTypeRestriction(iUserId, iCompanyId);
	}

	private ReturnStatus SaveCashFlowTemplate(CalRecord oData, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.SaveCashFlowTemplate(oData, iCompanyId);
	}

	private CalRecord LoadCashFlowTemplate(int iTemplateId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadCashFlowTemplate(iTemplateId, iCompanyId);
	}

	private ReturnStatus DeleteCashFlowTemplate(int iTemplateId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.DeleteCashFlowTemplate(iTemplateId, iCompanyId);
	}

	private CashFlowMatrix GetCashFlowCalendarData(CalRecord oData, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetCashFlowCalendarData(oData, iCompanyId);
	}

	private LineData[] GetPendingBill(int iDate, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetPendingBill(iDate, iCompanyId);
	}

	private IdNamePair[] GetMasterNameAndCode(int iMasterTypeId, string sFilter, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetMasterNameAndCode(iMasterTypeId, sFilter, iCompanyId);
	}

	private VATSummaryData GetVATSummaryData(int iStartDate, int iEndDate, int iFaTagId, int iCompanyId)
	{
		return new VATBL().GetData(iStartDate, iEndDate, iFaTagId, iCompanyId);
	}

	private VATAuditFileData GetVATAuditFileData(VATAuditFileInput oInput, int iCompanyId)
	{
		return new VATBL().GetAuditFile(oInput, iCompanyId);
	}

	private VATReturnsReportData GetVATReturnsReportData(VATAuditFileInput oInput, int iCompanyId)
	{
		return new VATBL().GetVATReturnsReport(oInput, iCompanyId);
	}

	private GSTAuditFile LoadGSTAuditFileData(VATAuditFileInput oInput, string sCompName, int iCompanyId)
	{
		return new VATBL().LoadGSTAuditFileData(oInput, sCompName, iCompanyId);
	}

	private bool InitializeSocket(int iCompId)
	{
		string sError = string.Empty;
		return new QueryGenerator().InitializeSocket(iCompId, ref sError);
	}

	private bool DisableSocket(int iCompId)
	{
		return new QueryGenerator().DisableSocket(iCompId);
	}

	private bool EnableSocket(int iCompId)
	{
		return new QueryGenerator().EnableSocket(iCompId);
	}

	private TagwiseBalance[] GetTagwiseStocks(int iMasterId, int iUserId, int iCompId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetTagwiseStocks(iMasterId, iUserId, iCompId);
	}

	private StockData GetStockData(InputStockData objInputDate, int iCompId, string sDateFormat)
	{
		RDReport rDReport = new RDReport();
		rDReport.m_objCalType = m_objCalType;
		if (!string.IsNullOrEmpty(sDateFormat))
		{
			return rDReport.GetStockData(objInputDate, iCompId, sDateFormat);
		}
		return rDReport.GetStockData(objInputDate, iCompId);
	}

	private int LoadMRPLayoutId(int iReportId, int iSubReportId, int iUserId, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.LoadMRPLayoutId(iReportId, iSubReportId, iUserId, iCompanyId);
	}

	private LineData[] GetGLAndInventoryVoucherwise(IdValuePair[] arrInputs, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetGLAndInventoryVoucherwise(arrInputs, iCompanyId);
	}

	private LineData[] GetBatchNegitiveTransData(IdValuePair[] arrInputs, ref string strHeader, int iCompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetBatchNegitiveTransData(arrInputs, ref strHeader, iCompanyId);
	}

	private ApprovalHistoryDetail[] getAuthorizationHistory(int iHeaderId, Dictionary<string, string> arrResources, string sDateFormat, CalendarType oCalType, double dClientOffset, int iCompanyId)
	{
		return new InvoiceLayout
		{
			m_objCalType = m_objCalType
		}.getAuthorizationHistory(iHeaderId, arrResources, sDateFormat, oCalType, dClientOffset, iCompanyId);
	}

	private List<IdNamePair> GetQuotationVouchers(int iVoucherType, int iFaTagId, bool bIncludeRaisedPRN, int iCompanyId, ref List<IdNamePair> arrLinkVouchers, bool bDocumentClass)
	{
		return new QueryGenerator().getQuotationVouchers(iVoucherType, iFaTagId, bIncludeRaisedPRN, iCompanyId, ref arrLinkVouchers, bDocumentClass);
	}

	private TranAuditDetailData[] GetAuditLogDetails(int iAuditId, int iHeaderId, int iVoucherType, VWVoucherFields[] arrDocFields, int iUserId, int iCompanyId, CalendarType oCalType, ref string sError)
	{
		return new RDReport().GetAuditLogDetails(iAuditId, iHeaderId, iVoucherType, arrDocFields, iUserId, iCompanyId, oCalType, ref sError);
	}

	private TranAuditDetailData[] GetLockUnlockAuditDetails(int iHeaderId, int iVoucherType, string sVoucherName, int iCompanyId, CalendarType oCalType, ref string sError)
	{
		return new RDReport().GetLockUnlockAuditDetails(iHeaderId, iVoucherType, sVoucherName, iCompanyId, oCalType, ref sError);
	}

	private LineData[] GetInterestDetails(int MasterId, int iStartDate, int iEndDate, decimal INTEREST_RATE, bool IsOnDueDate, bool IsUseAccountInterestRate, int CompanyId)
	{
		return new RDReport
		{
			m_objCalType = m_objCalType
		}.GetInterestDetails(MasterId, iStartDate, iEndDate, INTEREST_RATE, IsOnDueDate, IsUseAccountInterestRate, null, CompanyId);
	}
}
