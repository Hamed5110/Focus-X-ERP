using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Focus.Common.DataStructs;
using Focus.DatabaseFactory;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public class BLVAT
{
	public string m_sError;

	public bool IsJVEnableVAT(int iCompanyId, ref string strError)
	{
		try
		{
			int yearId = FConvert.GetYearId(iCompanyId);
			string commandText = $"IF EXISTS (select iVoucherType JVCount from cCore_Vouchers_{yearId} WITH(READUNCOMMITTED) where iVoucherType = 8704 and bPostVAT = 1) SELECT 1 ELSE SELECT 0";
			return Convert.ToBoolean(DatabaseWrapper.GetDatabase2(iCompanyId).ExecuteScalar(CommandType.Text, commandText));
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return false;
		}
	}

	public List<VATVouchers> GetVATVouchers(int iCompanyId, ref string strError)
	{
		List<VATVouchers> list = new List<VATVouchers>();
		int yearId = FConvert.GetYearId(iCompanyId);
		try
		{
			VATVouchers vATVouchers = new VATVouchers();
			string sQuery = $"SELECT iVoucherType,sName,iVoucherType&0xff00 iBaseVType,\r\n                                CASE WHEN (iVoucherType&0xff00 = {Convert.ToInt32(VTTYPE.SALES)}) THEN 'Sales' ELSE 'Purchases' END as sSalPurType \r\n                                FROM cCore_Vouchers_{yearId} WITH(READUNCOMMITTED) \r\n                                WHERE (iVoucherType& {Convert.ToInt32(VTTYPE.VOUCHERMASK)}) IN ({Convert.ToInt32(VTTYPE.SALES)}, {Convert.ToInt32(VTTYPE.PURCH)}) and bPostVAT=1";
			DataSet data = GetData(iCompanyId, sQuery);
			if (data != null)
			{
				for (int i = 0; i < data.Tables[0].Rows.Count; i++)
				{
					vATVouchers = new VATVouchers();
					DataRow dataRow = data.Tables[0].Rows[i];
					vATVouchers.Name = Convert.ToString(dataRow["sName"]);
					vATVouchers.VoucherType = Convert.ToInt32(dataRow["iVoucherType"]);
					vATVouchers.VoucherTypeClass = Convert.ToInt32(dataRow["iBaseVType"]);
					vATVouchers.SalPurType = Convert.ToString(dataRow["sSalPurType"]);
					list.Add(vATVouchers);
				}
				return list;
			}
			return null;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return null;
		}
	}

	public DataTable GetVATSummaryData(int iCompanyId, int iVoucherType, int iVoucherTypeClass, string SalPurType, int sFromDate, int sToDate, string strMasters, int iPlaceOfSupplyTagId, ref string strError)
	{
		int num = 0;
		string empty = string.Empty;
		string text = string.Empty;
		DataTable dataTable = new DataTable();
		try
		{
			if (!string.IsNullOrEmpty(strMasters))
			{
				text = ((!strMasters.Contains(",")) ? (text + $" AND d.iFaTag = {strMasters}") : (text + $" AND d.iFaTag IN{strMasters}"));
			}
			empty = string.Format("SELECT DISTINCT '{0}' SalPurType,a.SalesRegion,a.Region,a.SalesValue,CAST(ISNULL(ABS(b.AdjAmount), 0) as decimal(18, 2)) Adjustments,a.VATAmount,\r\n                CAST(ISNULL(ABS(a.SalesValue), 0) - ISNULL(ABS(b.AdjAmount), 0) as decimal(18, 2)) SalesNetValue,\r\n                CASE WHEN(a.TaxCategory IS NULL OR a.TaxCategory IN(0)) THEN 'Taxable' WHEN(a.TaxCategory = 1) THEN 'Zero' WHEN(a.TaxCategory = 2) THEN 'Exempt' ELSE '' END as ProdCategory\r\n                FROM \r\n                (\r\n                    SELECT 0 iRef, case when vPOS.iParentId is null then vPOS.sName else  vPOS1.sName end SalesRegion, vPOS.sName Region, vprd.TaxCategory, \r\n                    ABS(d.mAmount2)-(ISNULL(fx.fExchangeRate, 1)*vib.VAT)[SalesValue], cast(ISNULL(fx.fExchangeRate, 1)*vib.VAT as decimal (18, 2)) VATAmount \r\n                    FROM tCore_Header_{1} h \r\n                    INNER JOIN tCore_Data_{1} d on h.iHeaderId = d.iHeaderId \r\n                    INNER JOIN tCore_Indta_{1} i on i.iBodyId = d.iBodyId \r\n                    INNER JOIN tCore_IndtaBodyScreenData_{1} ib with(ReadUncommitted) on ib.iBodyId = d.iBodyId \r\n                    INNER JOIN vCore_BodyScreenData_{1} vib on vib.iBodyId = d.iBodyId \r\n                    INNER JOIN tCore_Data_Tags_{1} htag on htag.iBodyId = d.iBodyId \r\n                    INNER JOIN vmCore_PlaceOfSupply vPOS with(ReadUncommitted) on vPOS.iMasterId = htag.iTag{2} \r\n                    LEFT JOIN vmCore_PlaceOfSupply vPOS1 with(ReadUncommitted) on vPOS1.iMasterId = vPOS.iParentId \r\n                    LEFT JOIN tCore_Data_FX_{1} fx on fx.iBodyId = d.iBodyId\r\n                    INNER JOIN vmCore_Product vprd on vprd.iMasterId = i.iProduct \r\n                    WHERE h.iDate between {5} and {6} AND d.bUpdateFA = 1 AND d.bSuspendUpdateFA = 0 AND h.bSuspended = 0 \r\n                    AND d.iAuthStatus < 2 AND h.iVoucherType = {3} {4}\r\n                )a \r\n                LEFT JOIN( SELECT 0[iRef],0[AdjAmount] )b on a.iRef = b.iRef ", SalPurType, num, iPlaceOfSupplyTagId, iVoucherType, text, sFromDate, sToDate);
			return GetData(iCompanyId, empty)?.Tables[0];
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return null;
		}
	}

	public List<VATSummary> GetVATSummaryGridData(List<ClsVAT> lstVATSummary, int iSalPurType, ref string strError)
	{
		List<VATSummary> list = new List<VATSummary>();
		try
		{
			for (int i = 0; i <= 12; i++)
			{
				VATSummary vATSummary = new VATSummary();
				switch (i)
				{
				case 0:
					if (iSalPurType == 0)
					{
						vATSummary.SalesRegion = "Sales UAE";
					}
					else
					{
						vATSummary.SalesRegion = "Purchase UAE";
					}
					vATSummary.Region = "ABU DHABI";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("ABU DHABI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("ABU DHABI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("ABU DHABI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 1:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "AJMAN";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("AJMAN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("AJMAN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("AJMAN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 2:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "DUBAI";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("DUBAI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("DUBAI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("DUBAI") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 3:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "FUJAIRAH";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("FUJAIRAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("FUJAIRAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("FUJAIRAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 4:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "RAS AL-KHAIMAH";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && (x.Region.ToUpper().Contains("KHAIMAH") || x.Region.ToUpper().Contains("RAK ")) && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && (x.Region.ToUpper().Contains("KHAIMAH") || x.Region.ToUpper().Contains("RAK ")) && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && (x.Region.ToUpper().Contains("KHAIMAH") || x.Region.ToUpper().Contains("RAK ")) && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 5:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "SHARJAH";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("SHARJAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("SHARJAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("SHARJAH") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 6:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "UMM AL-QUWAIN";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("QUWAIN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("QUWAIN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.Region.ToUpper().Contains("QUWAIN") && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 7:
					vATSummary.SalesRegion = string.Empty;
					vATSummary.Region = "TOTAL UAE";
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 8:
					if (iSalPurType == 0)
					{
						vATSummary.SalesRegion = "Sales GCC";
					}
					else
					{
						vATSummary.SalesRegion = "Purchase GCC";
					}
					vATSummary.Region = string.Empty;
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 9:
					if (iSalPurType == 0)
					{
						vATSummary.SalesRegion = "Zero Rate Sales";
					}
					else
					{
						vATSummary.SalesRegion = "Zero Rate Purchase";
					}
					vATSummary.Region = string.Empty;
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 10:
					if (iSalPurType == 0)
					{
						vATSummary.SalesRegion = "Exempt Sales";
					}
					else
					{
						vATSummary.SalesRegion = "Exempt Purchase";
					}
					vATSummary.Region = string.Empty;
					vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.SalesValue);
					vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.Adjustments);
					vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.VATAmount);
					break;
				case 11:
					if (iSalPurType == 0)
					{
						vATSummary.SalesRegion = "Sales Export";
						vATSummary.Region = string.Empty;
						vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.SalesValue);
						vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.Adjustments);
						vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.VATAmount);
					}
					else
					{
						vATSummary.SalesRegion = "RCM Import";
						vATSummary.Region = string.Empty;
						vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.SalesValue);
						vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.Adjustments);
						vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.VATAmount);
					}
					break;
				case 12:
					if (iSalPurType == 0)
					{
						vATSummary.Region = "Total Sales";
						vATSummary.SalesRegion = string.Empty;
						vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.SalesValue);
						vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.Adjustments);
						vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "EXPORT" || x.Region.ToUpper() == "EXPORT").Sum((ClsVAT x) => x.VATAmount);
					}
					else
					{
						vATSummary.Region = "Total Purchase";
						vATSummary.SalesRegion = string.Empty;
						vATSummary.SalesValue = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.SalesValue) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.SalesValue);
						vATSummary.Adjustments = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.Adjustments) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.Adjustments);
						vATSummary.VATAmount = lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Taxable").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "GCC" || x.Region.ToUpper() == "GCC").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Zero").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion == "UAE" && x.ProdCategory == "Exempt").Sum((ClsVAT x) => x.VATAmount) + lstVATSummary.Where((ClsVAT x) => x.SalesRegion.ToUpper() == "RCM" || x.Region.ToUpper() == "RCM").Sum((ClsVAT x) => x.VATAmount);
					}
					break;
				}
				if (iSalPurType == 0)
				{
					vATSummary.SalPurType = "Sales";
				}
				else
				{
					vATSummary.SalPurType = "Purchases";
				}
				list.Add(vATSummary);
			}
			return list;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return null;
		}
	}

	public DataSet GetData(int iCompanyId, string sQuery)
	{
		return DatabaseWrapper.GetDatabase2(iCompanyId).ExecuteDataSet(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(sQuery) : sQuery);
	}
}
