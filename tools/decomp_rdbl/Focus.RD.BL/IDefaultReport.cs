using System.Collections.Generic;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public interface IDefaultReport
{
	StandardQuery vFirstLine(RepRecord objRec, bool bPreviousYear, ref List<string> arrDefaultTables, int iCompanyId);

	ReportMatrix vNextPage(ReportMatrix objData, RepRecord objRec, int iFieldCount, int iCompanyId);
}
