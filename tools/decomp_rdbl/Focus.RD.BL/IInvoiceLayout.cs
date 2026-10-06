using System.Data.Common;
using System.ServiceModel;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;
using Focus.Transactions.DataStructs;

namespace Focus.RD.BL;

[ServiceContract]
public interface IInvoiceLayout
{
	[OperationContract]
	string Save(LayoutInformation info, int iCompId);

	[OperationContract]
	LayoutInformation Load(int iLayoutId, int iCompId);

	[OperationContract]
	LayoutInformation LoadReportInvoiceLayout(int iReportId, int iLayoutId, int iCompId);

	[OperationContract]
	int GetReportInvoiceLayoutId(int iReportId, int iLayoutId, int iCompId);

	[OperationContract]
	string DeleteVoucherInvoiceLayout(int iLayoutId, int iCompId);

	[OperationContract]
	string GetMasterValueFromField(int iFieldId, int iSubParentId, string sFilter, int iLanguageId, int iCompId);

	[OperationContract]
	object EvaluateInvoiceFormula(string sFormula, int iRowIndex, Transaction oTranData, StaticTextClass objTextClass, string[] arrCellValue, int iCompId);

	[OperationContract]
	string DeleteLayout(int iLayoutId, Module objModule, int iCompId);

	[OperationContract]
	IdNamePair[] LoadColumnSchemas(string strQuery, string sConnectionString, int iCompId);

	[OperationContract]
	ComboData[] LoadLayouts(Module ModuleType, int iReportId, LayoutType type, int iCompanyId);

	[OperationContract]
	ComboData[] LoadMasters(string strParameters, MasterType type, int iCompanyId);

	[OperationContract]
	TranMasterData[] LoadTranMasters(int[] arrMasterId, bool bMainLevelMasterOnly, int iCompanyId);

	[OperationContract]
	string Delete(int iLayoutId, MasterType type, int iCompId, DbConnection objCon, DbTransaction objTran, int iModuleType);

	[OperationContract]
	object[] GetReportData(string strSqlQuery, int iColumnCount, int iCompId);

	[OperationContract]
	ComboData GetReportNameFromId(int iReportId, int iCompId);

	[OperationContract]
	LayoutInformation[] ExportRD(int iVoucherType, int iCompId);

	[OperationContract]
	LayoutInformation[] ExportBillPrinting(Module objModule, int iCompId);

	[OperationContract]
	string SavePOSBillFormat(POSPrintFormat Data, int iCompId);

	[OperationContract]
	POSPrintFormat LoadPOSBillFormat(int iLocationId, int iCompId);

	[OperationContract]
	string DeletePOSBillFormat(int iTemplateId, int iCompId);

	[OperationContract]
	ReportInputParameters LoadReportParameter(int iReportId, ReportType oType, int iCompId);

	[OperationContract]
	IdNamePair[] GetDataStatistics(int iUserId, int iCompId);

	[OperationContract]
	IdNamePair[] GetMasterList(int iMasterTypeId, int iCompId, bool bSequence = false);

	[OperationContract]
	IdNamePair[] GetReportList(int iUserId, int iCompId);

	[OperationContract]
	DocumentField[] GetDocFieldsOfTransactionSet(Module oModule, int iTranSetType, int iVoucherType, int iLanguageId, int iCompId);

	[OperationContract]
	DocumentField[] GetDocFieldsOfTransactionSets(_TransactionSet[] arrTranSets, int iCompId);

	[OperationContract]
	InventorySalesInfo[] GetSalesInfo(int iDate, int[] arrPutletIds, int iCompId);

	[OperationContract]
	IdNamePair[] GetVatFormsData(int iStartDate, int iEndDate, int iInventoryTag, int iCompId);

	[OperationContract]
	IdNamePair[] UpdateModulePatch(Module[] arrModules, int iCompId);

	[OperationContract]
	string SaveReportScheduler(ReportSchedule objRS, int iCompId);

	[OperationContract]
	IdNamePair[] LoadReportSchedulers(int iReportId, int iCompId);

	[OperationContract]
	AppMenuData LoadSelectedAppMenus(int iMenuId, int iLanguageId, int iUserID, int iCompId);

	[OperationContract]
	AppMenuData[] LoadAppMenus(int iLanguageId, int iUserId, int iCompId, bool bForScreen);

	[OperationContract]
	string SaveAppData(AppMenuData[] arrMenu, int iCompId);

	[OperationContract]
	IdNamePair[] LoadAccountEmails(int[] arrFields, int iCompId);

	[OperationContract]
	string DeleteScheduler(int iReportId, int iCompId);

	[OperationContract]
	IdValuePair[] LoadReportTemplate(int iTemplateId, int iCompId);

	[OperationContract]
	string SaveReportTemplate(PrintTemplate objPrintTemplate, int iCompId);

	[OperationContract]
	string DeleteReportTemplate(int iTemplateId, int iCompId);

	[OperationContract]
	IdNamePair[] LoadAllTemplateName(int iCompId);

	[OperationContract]
	IdNamePair SaveAppGroupData(IdNamePair objIdName, int iCompId);

	[OperationContract]
	string DeleteAppGroupData(IdNamePair[] arrIdName, int iCompId);

	[OperationContract]
	CurrencyDetail LoadCurrencyData(int iCurrencyId, int iCompanyId);
}
