using System.ServiceModel;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

[ServiceContract]
public interface IRDReport
{
	[OperationContract]
	ReportMatrix GetPagewiseData(RepRecord objRec, int iCompId);

	[OperationContract]
	bool EndAllReports(string sSessionId, int iCompId);

	[OperationContract]
	IdNamePair SaveReportDesigner(_Reports objReports, int iCompanyId);

	[OperationContract]
	IdNamePair[] LoadReportLayouts(uint iReportId, int iSubReportId, int iUserId, int iCompanyId);

	[OperationContract]
	_Reports LoadReportDesigner(uint iReportId, int iLayoutId, int iCompId, uint iSubReportId = 0u);

	[OperationContract]
	IdNamePair[] ImportRDReports(_Reports[] arrReports, int iCompanyId);

	[OperationContract]
	_Reports[] ExportRDReports(uint[] arrReportId, int iCompId);

	[OperationContract]
	_Menu[] GetMenuDetails(bool bGroup, int iCompId);

	[OperationContract]
	string DeleteReportDesigner(uint iReportId, int iCompId);

	[OperationContract]
	int GetParameterFieldId(MasterTypeId iMasterTypeId, int iCompanyId);

	[OperationContract]
	ReportMatrix GetAnalyzeData(RepRecord objRec, int iCompanyId);

	[OperationContract]
	string[] LoadAllTextData(int iFieldId, int iCompanyId);

	[OperationContract]
	_RDVirtual LoadRDVirtual(int iFieldId, bool bSearchByField, int iCompanyId);

	[OperationContract]
	IdNamePair SaveRDVirtual(_RDVirtual oData, int iCompanyId);

	[OperationContract]
	IdNamePair[] LoadVirtualFields(int iCompanyId);

	[OperationContract]
	_Menu UpdateRDMenuGroup(_Menu oMenu, MenuOptions eMenuOpt, int iCompanyId);

	[OperationContract]
	ComboData[] LoadAllDatabase(string sConnectionString, int iCompanyId);

	[OperationContract]
	PosXReading LoadXReadingData(XReadingHeader objHeader, int iCompId);
}
