using System.Data;
using System.Data.OleDb;
using System.IO;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public class RDFileSource
{
	public IDataReader GetPagewiseDataFromExcel(RepRecord objRec, _Reports objReport, out int iTotalRow)
	{
		string sExt = null;
		if (objReport.RDFolder != null && !string.IsNullOrEmpty(objReport.RDFolder.FolderName))
		{
			sExt = new FileInfo(objReport.RDFolder.FolderName).Extension;
		}
		return ReadExcelToTable(objReport.ReportName, FConvert.GetModuleAbbreviation(objReport.Module), objReport.DataSourceType == DataSourceType.Excel, sExt, out iTotalRow);
	}

	private IDataReader ReadExcelToTable(string strReportName, string strModule, bool IsExcel, string sExt, out int iTotalRow)
	{
		string serverFilePath = General.GetServerFilePath(strReportName, strModule, IsExcel, sExt);
		string text = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + serverFilePath + ";Extended Properties='Excel 12.0;HDR=NO;IMEX=1';";
		using OleDbConnection oleDbConnection = new OleDbConnection(text);
		oleDbConnection.Open();
		string text2 = oleDbConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[4] { null, null, null, "Table" }).Rows[0][2].ToString();
		OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", text2 + "A2:Z"), text);
		DataSet dataSet = new DataSet();
		oleDbDataAdapter.Fill(dataSet);
		iTotalRow = dataSet.Tables[0].Rows.Count;
		return dataSet.CreateDataReader(dataSet.Tables[0]);
	}
}
