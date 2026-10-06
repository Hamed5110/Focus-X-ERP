using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Focus.Common.DataStructs;
using Focus.DatabaseFactory;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Focus.TranSettings.DataStructs;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Focus.RD.BL;

public class VATBL
{
	private int m_iPlaceOfSupply;

	public string m_strCatchError;

	public VATAuditFileData GetAuditFile(VATAuditFileInput oInput, int iCompanyId)
	{
		int num = 0;
		int num2 = 0;
		string text = null;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		string text2 = null;
		string text3 = null;
		Date date = new Date(CalendarType.Gregorean);
		Database database = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		VATAuditFileData vATAuditFileData = new VATAuditFileData();
		List<VATCompanyInfo> list = new List<VATCompanyInfo>();
		List<VATPurchaseListing> list2 = new List<VATPurchaseListing>();
		List<VATSalesListing> list3 = new List<VATSalesListing>();
		List<VATGeneralLedger> list4 = new List<VATGeneralLedger>();
		VATCompanyInfo vATCompanyInfo = new VATCompanyInfo();
		vATAuditFileData.Footer = new VATFooter();
		try
		{
			QueryGenerator queryGenerator = new QueryGenerator();
			queryGenerator.m_iCompId = iCompanyId;
			queryGenerator.Suffix = _focus.company(iCompanyId).suffix;
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			if (oInput.FaTagId > 0)
			{
				string masterUnderGroup = queryGenerator.GetMasterUnderGroup(oInput.FaTagId, _focus.company(iCompanyId).faTagId, database, iCompanyId);
				text3 = ((!masterUnderGroup.Contains(",")) ? $" AND tCore_Data{_focus.company(iCompanyId).suffix}.iFaTag = {oInput.FaTagId}" : $" AND tCore_Data{_focus.company(iCompanyId).suffix}.iFaTag IN{masterUnderGroup}");
			}
			if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0 && oInput.FaTagId > 0)
			{
				vATCompanyInfo = getVATCompanyInfo(oInput.FaTagId, iCompanyId);
			}
			else
			{
				vATCompanyInfo.FormType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 13);
				vATCompanyInfo.TaxFormFilingType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 15);
				vATCompanyInfo.TaxablePersonAddress = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 19);
				vATCompanyInfo.TRN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 16);
				vATCompanyInfo.TAAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 24);
				vATCompanyInfo.TAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 21);
				vATCompanyInfo.TaxablePersonNameAr = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 18);
				vATCompanyInfo.TaxablePersonNameEn = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 17);
				vATCompanyInfo.TaxAgencyName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 20);
				vATCompanyInfo.TaxAgentName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 23);
			}
			date.Value = oInput.StartDate;
			vATCompanyInfo.PeriodStart = $"{date.Day}-{date.Month}-{date.Year}";
			date.Value = oInput.EndDate;
			vATCompanyInfo.PeriodEnd = $"{date.Day}-{date.Month}-{date.Year}";
			date.Value = new Date(CalendarType.Gregorean).GetToday(CalendarType.Gregorean).Value;
			vATCompanyInfo.FAFCreationDate = $"{date.Day}-{date.Month}-{date.Year}";
			vATCompanyInfo.ProductVersion = "Focus9";
			vATCompanyInfo.FAFVersion = "FAFv1.0.0";
			list.Add(vATCompanyInfo);
			vATAuditFileData.CompanyInfos = list.ToArray();
			VATPurchaseListing vATPurchaseListing = null;
			text2 = string.Format("SELECT vrCore_Account.sName[sParty],vrCore_Account.TRN[TRN], tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[sVoucher],    \r\n                tCore_Data{0}.iSerialNo,tCore_Data{0}.mAmount2,vrCore_Product.sName[sProduct],vCore_TranData{0}.PermitNo,vrCore_TaxCode.sCode[TaxCode], vCore_BodyScreenData{0}.VAT, mCore_Currency.sCode[CurCode],\r\n                ISNULL(fExchangeRate,1)*vCore_BodyScreenData{0}.VAT[VATBase],ISNULL(mFxAmount2,mAmount2)-vCore_BodyScreenData{0}.VAT[PurExVat]\r\n                FROM tCore_Header{0} \r\n                JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON tCore_Data{0}.iCurrencyId = mCore_Currency.iCurrencyId\r\n                JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId \r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                LEFT JOIN vrCore_Account ON vrCore_Account.iMasterId = tCore_Data{0}.iBookNo AND vrCore_Account.iTreeId = 0\r\n                LEFT JOIN vrCore_Account Code ON Code.iMasterId = tCore_Data{0}.iCode AND Code.iTreeId = 0\r\n                JOIN vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0 \r\n                JOIN vCore_TranData{0} ON vCore_TranData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN vrCore_TaxCode on vrCore_TaxCode.iMasterId = vCore_TranData{0}.TaxCode AND vrCore_TaxCode.iTreeId = 0 \r\n                JOIN vCore_BodyScreenData{0} on vCore_BodyScreenData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                WHERE tCore_Data{0}.bUpdateFA = 1 AND cCore_Vouchers{0}.bPostVAT = 1\r\n                AND tCore_Data{0}.bSuspendUpdateFA <> 1 \r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} AND (tCore_Header{0}.iVoucherClass = {3} OR tCore_Header{0}.iVoucherClass = {4} OR tCore_Header{0}.iVoucherClass = {5}) {6}\r\n                ORDER BY tCore_Header{0}.iDate,tCore_Header{0}.sVoucherNo", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, 768, 1280, 6400, text3);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			num4 = (num5 = (num = (num2 = 0)));
			text = null;
			while (dataReader.Read())
			{
				vATPurchaseListing = new VATPurchaseListing();
				vATPurchaseListing.FCYCode = Convert.ToString(dataReader["CurCode"]);
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				vATPurchaseListing.InvoiceDate = $"{date.Day}-{date.Month}-{date.Year}";
				vATPurchaseListing.InvoiceNo = Convert.ToString(dataReader["sVoucher"]);
				if (text != vATPurchaseListing.InvoiceNo)
				{
					num = 1;
					num2++;
				}
				text = vATPurchaseListing.InvoiceNo;
				vATPurchaseListing.LineNo = num++.ToString();
				vATPurchaseListing.PermitNo = Convert.ToString(dataReader["PermitNo"]);
				vATPurchaseListing.ProductDescription = Convert.ToString(dataReader["sProduct"]);
				if (Convert.ToDouble(dataReader["PurExVat"]) < 0.0)
				{
					vATPurchaseListing.PurchaseFCY = (Convert.ToDouble(dataReader["PurExVat"]) + 2.0 * Convert.ToDouble(dataReader["VAT"])).AddDecimalInColumn(2);
				}
				else
				{
					vATPurchaseListing.PurchaseFCY = Convert.ToDouble(dataReader["PurExVat"]).AddDecimalInColumn(2);
				}
				vATPurchaseListing.SupplierName = Convert.ToString(dataReader["sParty"]);
				vATPurchaseListing.SupplierTINTRN = Convert.ToString(dataReader["TRN"]);
				vATPurchaseListing.TaxCode = Convert.ToString(dataReader["TaxCode"]);
				if (Convert.ToDouble(dataReader["mAmount2"]) < 0.0)
				{
					vATPurchaseListing.AEDFCY = (0.0 - Convert.ToDouble(dataReader["VAT"])).AddDecimalInColumn(2);
					vATPurchaseListing.VATValueAED = (0.0 - Convert.ToDouble(dataReader["VATBase"])).AddDecimalInColumn(2);
					num4 -= Convert.ToDouble(dataReader["VATBase"]);
					vATPurchaseListing.PurchaseValueAED = (Convert.ToDouble(dataReader["mAmount2"]) + Convert.ToDouble(dataReader["VATBase"])).AddDecimalInColumn(2);
				}
				else
				{
					vATPurchaseListing.AEDFCY = Convert.ToDouble(dataReader["VAT"]).AddDecimalInColumn(2);
					vATPurchaseListing.VATValueAED = Convert.ToDouble(dataReader["VATBase"]).AddDecimalInColumn(2);
					num4 += Convert.ToDouble(dataReader["VATBase"]);
					vATPurchaseListing.PurchaseValueAED = (Convert.ToDouble(dataReader["mAmount2"]) - Convert.ToDouble(dataReader["VATBase"])).AddDecimalInColumn(2);
				}
				num5 += Convert.ToDouble(vATPurchaseListing.PurchaseValueAED);
				list2.Add(vATPurchaseListing);
			}
			dataReader.Close();
			vATAuditFileData.Purchases = list2.ToArray();
			vATAuditFileData.Footer.PurchaseTotalAED = num5.AddDecimalInColumn(2);
			vATAuditFileData.Footer.PurchaseVATTotalAED = num4.AddDecimalInColumn(2);
			vATAuditFileData.Footer.PurchaseTransactionCountTotal = num2.ToString();
			VATSalesListing vATSalesListing = null;
			text2 = string.Format("SELECT vrCore_Account.sName[sParty],vrCore_Account.TRN[TRN],tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[sVoucher],    \r\n                tCore_Data{0}.iSerialNo,-tCore_Data{0}.mAmount2[mAmount2],vrCore_Product.sName[sProduct],vCore_TranData{0}.PermitNo,vrCore_TaxCode.sCode[TaxCode], vCore_BodyScreenData{0}.VAT, mCore_Currency.sCode[CurCode],\r\n                ISNULL(fExchangeRate,1)*vCore_BodyScreenData{0}.VAT[VATBase],-ISNULL(mFxAmount2,mAmount2)-vCore_BodyScreenData{0}.VAT[PurExVat]\r\n                FROM tCore_Header{0}\r\n                JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON tCore_Data{0}.iCurrencyId = mCore_Currency.iCurrencyId\r\n                JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId \r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                LEFT JOIN vrCore_Account  ON vrCore_Account.iMasterId = tCore_Data{0}.iBookNo AND vrCore_Account.iTreeId = 0\r\n                LEFT JOIN vrCore_Account Code  ON Code.iMasterId = tCore_Data{0}.iCode AND Code.iTreeId = 0\r\n                LEFT JOIN vrCore_Product  ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0 \r\n                JOIN vCore_TranData{0} ON vCore_TranData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN vrCore_TaxCode on vrCore_TaxCode.iMasterId = vCore_TranData{0}.TaxCode AND vrCore_TaxCode.iTreeId = 0 \r\n                JOIN vCore_BodyScreenData{0} on vCore_BodyScreenData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                WHERE tCore_Data{0}.bUpdateFA = 1 AND cCore_Vouchers{0}.bPostVAT = 1\r\n                AND tCore_Data{0}.bSuspendUpdateFA <> 1\r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} \r\n                AND (tCore_Header{0}.iVoucherClass = {3} OR tCore_Header{0}.iVoucherClass = {4} OR tCore_Header{0}.iVoucherClass = {5}) {6}\r\n                ORDER BY tCore_Header{0}.iDate,tCore_Header{0}.sVoucherNo", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, 3328, 6144, 1792, text3);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			num4 = (num5 = (num = (num2 = 0)));
			text = null;
			while (dataReader.Read())
			{
				vATSalesListing = new VATSalesListing();
				vATSalesListing.Country = "";
				vATSalesListing.CustomerName = Convert.ToString(dataReader["sParty"]);
				vATSalesListing.CustomerTINTRN = Convert.ToString(dataReader["TRN"]);
				vATSalesListing.FCYCode = Convert.ToString(dataReader["CurCode"]);
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				vATSalesListing.InvoiceDate = $"{date.Day}-{date.Month}-{date.Year}";
				vATSalesListing.InvoiceNo = Convert.ToString(dataReader["sVoucher"]);
				if (text != vATSalesListing.InvoiceNo)
				{
					num = 1;
					num2++;
				}
				text = vATSalesListing.InvoiceNo;
				vATSalesListing.LineNo = num++.ToString();
				vATSalesListing.ProductDescription = Convert.ToString(dataReader["sProduct"]);
				vATSalesListing.SupplyFCY = Convert.ToDouble(dataReader["PurExVat"]).AddDecimalInColumn(2);
				vATSalesListing.SupplyValueAED = (Convert.ToDouble(dataReader["mAmount2"]) - Convert.ToDouble(dataReader["VATBase"])).AddDecimalInColumn(2);
				num5 += Convert.ToDouble(dataReader["mAmount2"]) - Convert.ToDouble(dataReader["VATBase"]);
				vATSalesListing.TaxCode = Convert.ToString(dataReader["TaxCode"]);
				vATSalesListing.VATFCY = Convert.ToDouble(dataReader["VAT"]).AddDecimalInColumn(2);
				vATSalesListing.VATValueAED = Convert.ToDouble(dataReader["VATBase"]).AddDecimalInColumn(2);
				num4 += Convert.ToDouble(dataReader["VATBase"]);
				list3.Add(vATSalesListing);
			}
			dataReader.Close();
			vATAuditFileData.Sales = list3.ToArray();
			vATAuditFileData.Footer.SupplyTotalAED = num5.AddDecimalInColumn(2);
			vATAuditFileData.Footer.SupplyTransactionCountTotal = num2.ToString();
			vATAuditFileData.Footer.SupplyVATTotalAED = num4.AddDecimalInColumn(2);
			VATGeneralLedger vATGeneralLedger = null;
			text2 = string.Format("SELECT tCore_Header{0}.iHeaderId[iHeaderId], tCore_Header{0}.iDate[iDate],\r\n                cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[Voucher], Code.sName[Account], \r\n                SUM(TranDr)[TranDr],SUM(TranCr)[TranCr],SUM(Debit)[Debit], SUM(Credit)[Credit],\r\n                mCore_Currency.sName[Currency],0[bHeader], cCore_Vouchers{0}.sName[Source],Code.sName[Account2],Code.sCode[Code2]\r\n                FROM (\r\n                    SELECT tCore_Data{0}.iBodyId, mCore_Account.sName[Account], iBookNo[iMasterId],mCore_Account.iMasterId[iBookNo],\r\n                    CASE WHEN mAmount2 < 0 THEN mAmount2 ELSE 0 END Debit, \r\n                    CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END Credit,\r\n\t                CASE WHEN ISNULL(mFxAmount2,mAmount2) < 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranDr, \r\n\t                CASE WHEN ISNULL(mFxAmount2,mAmount2) > 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranCr,\r\n                    CASE WHEN ISNULL(fLocalAmount2,mAmount2) < 0 THEN ISNULL(fLocalAmount2,mAmount2) ELSE 0 END LocalDr, \r\n\t                CASE WHEN ISNULL(fLocalAmount2,mAmount2) > 0 THEN ISNULL(fLocalAmount2,mAmount2) ELSE 0 END LocalCr\r\n                    FROM tCore_Data{0}\r\n                    JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                    JOIN mCore_Account ON tCore_Data{0}.iCode = mCore_Account.iMasterId\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                    AND tCore_Header{0}.iVoucherClass <> 256\r\n                    UNION ALL\r\n                    SELECT tCore_Data{0}.iBodyId, mCore_Account.sName[Account], iCode[iMasterId],mCore_Account.iMasterId[iBookNo],\r\n                    CASE WHEN mAmount1 < 0 THEN mAmount1 ELSE 0 END Debit, \r\n                    CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END Credit,\r\n\t                CASE WHEN ISNULL(mFXAmount1,mAmount1) < 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranDr, \r\n\t                CASE WHEN ISNULL(mFXAmount1,mAmount1) > 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranCr,\r\n                    CASE WHEN ISNULL(fLocalAmount1,mAmount1) < 0 THEN ISNULL(fLocalAmount1,mAmount1) ELSE 0 END LocalDr, \r\n\t                CASE WHEN ISNULL(fLocalAmount1,mAmount1) > 0 THEN ISNULL(fLocalAmount1,mAmount1) ELSE 0 END LocalCr\r\n                    FROM tCore_Data{0} \r\n                    JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                    JOIN mCore_Account ON tCore_Data{0}.iBookNo = mCore_Account.iMasterId\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                    AND tCore_Header{0}.iVoucherClass <> 256\r\n                )InnerTable\r\n                JOIN vrCore_Account  ON InnerTable.iMasterId = vrCore_Account.iMasterId AND vrCore_Account.iTreeId = 0\r\n                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = InnerTable.iBodyId  \r\n                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                LEFT JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                LEFT JOIN vrCore_Product  ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0\r\n                LEFT JOIN tCore_Indta{0} Cogs ON Cogs.iBodyId = tCore_Data{0}.iMainBodyId AND iType=5\r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = InnerTable.iBodyId\r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                JOIN vrCore_Account BookNo  ON InnerTable.iBookNo = BookNo.iMasterId AND BookNo.iTreeId = 0\r\n                JOIN vrCore_Account Code  ON InnerTable.iMasterId = Code.iMasterId AND Code.iTreeId = 0\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON mCore_Currency.iCurrencyId = tCore_Data{0}.iCurrencyId  \r\n                WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA = 0  \r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 AND cCore_Vouchers{0}.bPostVAT = 1\r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} {3}\r\n                AND (Debit < 0 OR Credit > 0 OR TranDr < 0 OR TranCr > 0 OR LocalDr < 0 OR LocalCr > 0)  \r\n                GROUP BY tCore_Header{0}.iHeaderId, tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr,cCore_Vouchers{0}.sName,tCore_Header{0}.sVoucherNo, Code.sName, Code.sCode,mCore_Currency.sName,Code.iMasterId\r\n                ORDER BY 3,4", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, text3);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			num4 = (num5 = (num3 = (num2 = 0)));
			while (dataReader.Read())
			{
				vATGeneralLedger = new VATGeneralLedger();
				vATGeneralLedger.Account2Name = Convert.ToString(dataReader["Account2"]);
				vATGeneralLedger.AccountCode = Convert.ToString(dataReader["Code2"]);
				vATGeneralLedger.AccountName = Convert.ToString(dataReader["Account"]);
				vATGeneralLedger.Credit = Convert.ToDouble(dataReader["Credit"]).AddDecimalInColumn(2);
				num5 += Convert.ToDouble(dataReader["Credit"]);
				vATGeneralLedger.Debit = Convert.ToDouble(dataReader["Debit"]).AddDecimalInColumn(2);
				num4 += Convert.ToDouble(dataReader["Debit"]);
				num3 = Convert.ToDouble(dataReader["Debit"]) + Convert.ToDouble(dataReader["Credit"]);
				vATGeneralLedger.Balance = num3.AddDecimalInColumn(2);
				vATGeneralLedger.SourceDocumentID = Convert.ToString(dataReader["Voucher"]);
				vATGeneralLedger.SourceType = Convert.ToString(dataReader["Source"]);
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				vATGeneralLedger.TransactionDate = $"{date.Day}-{date.Month}-{date.Year}";
				vATGeneralLedger.TransactionDescription = Convert.ToString(dataReader["Source"]);
				vATGeneralLedger.TransactionID = Convert.ToString(dataReader["iHeaderId"]);
				if (text != vATGeneralLedger.SourceDocumentID)
				{
					num2++;
				}
				text = vATGeneralLedger.SourceDocumentID;
				list4.Add(vATGeneralLedger);
			}
			dataReader.Close();
			vATAuditFileData.Ledger = list4.ToArray();
			vATAuditFileData.Footer.GLTCurrency = ((_focus.company(iCompanyId).currency != null) ? _focus.company(iCompanyId).currency.Code : "AED");
			vATAuditFileData.Footer.GLTotalCredit = num5.AddDecimalInColumn(2);
			vATAuditFileData.Footer.GLTotalDebit = num4.AddDecimalInColumn(2);
			vATAuditFileData.Footer.GLTransactionCountTotal = num2.ToString();
		}
		catch (Exception ex)
		{
			string strCatchError = (vATAuditFileData.Error = ex.Message);
			m_strCatchError = strCatchError;
		}
		return vATAuditFileData;
	}

	public GSTAuditFile LoadGSTAuditFileData(VATAuditFileInput oInput, string sCompName, int iCompanyId)
	{
		string text = null;
		Date date = new Date(CalendarType.Gregorean);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		string text2 = null;
		Database database = null;
		GSTAuditFile gSTAuditFile = new GSTAuditFile();
		Companies companies = new Companies();
		GSTCompany gSTCompany = null;
		Purchases purchases = null;
		Supplies supplies = null;
		Ledger ledger = new Ledger();
		List<Ledger> list = new List<Ledger>();
		Footer footer = null;
		List<GSTCompany> list2 = new List<GSTCompany>();
		List<Purchases> list3 = new List<Purchases>();
		List<Supplies> list4 = new List<Supplies>();
		LedgerEntry ledgerEntry = null;
		List<LedgerEntry> list5 = new List<LedgerEntry>();
		List<Footer> list6 = new List<Footer>();
		string text3 = null;
		FConvert.GetSuffix(iCompanyId);
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		VATReturnsReportData vATReturnsReportData = new VATReturnsReportData();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text2 = $" AND tCore_Data{_focus.company(iCompanyId).suffix}.iFaTag = {oInput.FaTagId}";
			gSTCompany = new GSTCompany();
			gSTCompany.BusinessName = sCompName;
			gSTCompany.BusinessRN = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.GST, 0).ToString();
			gSTCompany.GSTNumber = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.GST, 0).ToString();
			date.Value = oInput.StartDate;
			gSTCompany.PeriodStart = $"{date.Day}-{date.Month}-{date.Year}";
			date.Value = oInput.EndDate;
			gSTCompany.PeriodEnd = $"{date.Day}-{date.Month}-{date.Year}";
			date.Value = new Date(CalendarType.Gregorean).GetToday(CalendarType.Gregorean).Value;
			gSTCompany.GAFCreationDate = $"{date.Day}-{date.Month}-{date.Year}";
			gSTCompany.ProductVer = "Focus9";
			gSTCompany.GAFVersion = "GAFv1.0.0";
			list2.Add(gSTCompany);
			if (list2 != null && list2.Count > 0)
			{
				companies.Company = list2;
			}
			text3 = string.Format("SELECT vrCore_Account.sName[sParty], tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[sVoucher],    \r\n                tCore_Data{0}.iSerialNo,tCore_Data{0}.mAmount2,vrCore_Product.sName[sProduct], vCore_BodyScreenData{0}.GST, mCore_Currency.sCode[CurCode],\r\n                ISNULL(fExchangeRate,1)*vCore_BodyScreenData{0}.GST[GSTBase],ISNULL(mFxAmount2,mAmount2)-vCore_BodyScreenData{0}.GST[PurExGST]\r\n                FROM tCore_Header{0}\r\n                JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON tCore_Data{0}.iCurrencyId = mCore_Currency.iCurrencyId\r\n                JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId \r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                LEFT JOIN vrCore_Account  ON vrCore_Account.iMasterId = tCore_Data{0}.iBookNo AND vrCore_Account.iTreeId = 0\r\n                LEFT JOIN vrCore_Account Code  ON Code.iMasterId = tCore_Data{0}.iCode AND Code.iTreeId = 0\r\n                JOIN vrCore_Product  ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0 \r\n                JOIN vCore_TranData{0} ON vCore_TranData{0}.iBodyId = tCore_Data{0}.iBodyId                \r\n                JOIN vCore_BodyScreenData{0} ON vCore_BodyScreenData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                WHERE tCore_Data{0}.bUpdateFA = 1\r\n                AND tCore_Data{0}.bSuspendUpdateFA <> 1 \r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} AND (tCore_Header{0}.iVoucherClass = {3} OR tCore_Header{0}.iVoucherClass = {4} OR tCore_Header{0}.iVoucherClass = {5}) {6}\r\n                ORDER BY tCore_Header{0}.iDate,tCore_Header{0}.sVoucherNo", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, 768, 1280, 6400, text2);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text3) : text3);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			num4 = (num = (num2 = 0));
			while (dataReader.Read())
			{
				purchases = new Purchases();
				purchases.SupplierName = Convert.ToString(dataReader["sParty"]);
				purchases.SupplierBRN = "";
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				purchases.InvoiceDate = $"{date.Day}-{date.Month}-{date.Year}";
				purchases.InvoiceNumber = Convert.ToString(dataReader["sVoucher"]);
				if (text != purchases.InvoiceNumber)
				{
					num = 1;
					num2++;
				}
				purchases.ImportDeclarationNo = "";
				purchases.LineNumber = num++.ToString();
				purchases.ProductionDescription = Convert.ToString(dataReader["sProduct"]);
				purchases.PurchaseValueRM = "";
				purchases.GSTValueRM = "";
				purchases.TaxCode = "";
				purchases.FCYCode = Convert.ToString(dataReader["CurCode"]);
				purchases.PurchaseFCY = Convert.ToDouble(dataReader["PurExGST"]).AddDecimalInColumn(2);
				purchases.GSTFCY = "";
				num4 += Convert.ToDouble(dataReader["mAmount2"]);
				list3.Add(purchases);
			}
			dataReader.Close();
			if (list3 != null && list3.Count > 0)
			{
				companies.Purchases = list3;
			}
			else
			{
				companies.Purchases = new List<Purchases>();
			}
			text3 = string.Format("SELECT vrCore_Account.sName[sParty], tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[sVoucher],    \r\n                tCore_Data{0}.iSerialNo,-tCore_Data{0}.mAmount2[mAmount2],vrCore_Product.sName[sProduct], vCore_BodyScreenData{0}.GST, mCore_Currency.sCode[CurCode],\r\n                ISNULL(fExchangeRate,1)*vCore_BodyScreenData{0}.GST[GSTBase],-ISNULL(mFxAmount2,mAmount2)-vCore_BodyScreenData{0}.GST[PurExGST]\r\n                FROM tCore_Header{0}\r\n                JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON tCore_Data{0}.iCurrencyId = mCore_Currency.iCurrencyId\r\n                JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId \r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                LEFT JOIN vrCore_Account  ON vrCore_Account.iMasterId = tCore_Data{0}.iBookNo AND vrCore_Account.iTreeId = 0\r\n                LEFT JOIN vrCore_Account Code  ON Code.iMasterId = tCore_Data{0}.iCode AND Code.iTreeId = 0\r\n                LEFT JOIN vrCore_Product  ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0 \r\n                JOIN vCore_TranData{0} ON vCore_TranData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                JOIN vCore_BodyScreenData{0} ON vCore_BodyScreenData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                WHERE tCore_Data{0}.bUpdateFA = 1 \r\n                AND tCore_Data{0}.bSuspendUpdateFA <> 1\r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} \r\n                AND (tCore_Header{0}.iVoucherClass = {3} OR tCore_Header{0}.iVoucherClass = {4} OR tCore_Header{0}.iVoucherClass = {5})\r\n                ORDER BY tCore_Header{0}.iDate,tCore_Header{0}.sVoucherNo", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, 3328, 6144, 1792);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text3) : text3);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			text = null;
			num = (num3 = 0);
			while (dataReader.Read())
			{
				supplies = new Supplies();
				supplies.CustomerName = Convert.ToString(dataReader["sParty"]);
				supplies.CustomerBRN = "";
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				supplies.InvoiceDate = $"{date.Day}-{date.Month}-{date.Year}";
				supplies.InvoiceNumber = Convert.ToString(dataReader["sVoucher"]);
				if (text != supplies.InvoiceNumber)
				{
					num = 1;
					num3++;
				}
				supplies.LineNumber = num++.ToString();
				supplies.ProductionDescription = Convert.ToString(dataReader["sProduct"]);
				supplies.SupplyValueRM = "";
				supplies.GSTValueRM = "";
				supplies.TaxCode = "";
				supplies.Country = "";
				supplies.FCYCode = Convert.ToString(dataReader["CurCode"]);
				supplies.SupplyFCY = Convert.ToDouble(dataReader["PurExGST"]).AddDecimalInColumn(2);
				supplies.GSTFCY = "";
				num5 += Convert.ToDouble(dataReader["mAmount2"]);
				list4.Add(supplies);
			}
			dataReader.Close();
			if (list4 != null && list4.Count > 0)
			{
				companies.Supplies = list4;
			}
			else
			{
				companies.Supplies = new List<Supplies>();
			}
			text3 = string.Format("SELECT tCore_Header{0}.iHeaderId[iHeaderId], tCore_Header{0}.iDate[iDate],\r\n                cCore_Vouchers{0}.sAbbr+' : '+tCore_Header{0}.sVoucherNo[Voucher], Code.sName[Account], \r\n                SUM(TranDr)[TranDr],SUM(TranCr)[TranCr],SUM(Debit)[Debit], SUM(Credit)[Credit],\r\n                mCore_Currency.sName[Currency],0[bHeader], cCore_Vouchers{0}.sName[Source],Code.sName[Account2],Code.sCode[Code2]\r\n                FROM (\r\n                    SELECT tCore_Data{0}.iBodyId, mCore_Account.sName[Account], iBookNo[iMasterId],mCore_Account.iMasterId[iBookNo],\r\n                    CASE WHEN mAmount2 < 0 THEN mAmount2 ELSE 0 END Debit, \r\n                    CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END Credit,\r\n\t                CASE WHEN ISNULL(mFxAmount2,mAmount2) < 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranDr, \r\n\t                CASE WHEN ISNULL(mFxAmount2,mAmount2) > 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranCr,\r\n                    CASE WHEN ISNULL(fLocalAmount2,mAmount2) < 0 THEN ISNULL(fLocalAmount2,mAmount2) ELSE 0 END LocalDr, \r\n\t                CASE WHEN ISNULL(fLocalAmount2,mAmount2) > 0 THEN ISNULL(fLocalAmount2,mAmount2) ELSE 0 END LocalCr\r\n                    FROM tCore_Data{0}\r\n                    JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                    JOIN mCore_Account ON tCore_Data{0}.iCode = mCore_Account.iMasterId\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                    AND tCore_Header{0}.iVoucherClass <> 256\r\n                    UNION ALL\r\n                    SELECT tCore_Data{0}.iBodyId, mCore_Account.sName[Account], iCode[iMasterId],mCore_Account.iMasterId[iBookNo],\r\n                    CASE WHEN mAmount1 < 0 THEN mAmount1 ELSE 0 END Debit, \r\n                    CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END Credit,\r\n\t                CASE WHEN ISNULL(mFXAmount1,mAmount1) < 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranDr, \r\n\t                CASE WHEN ISNULL(mFXAmount1,mAmount1) > 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranCr,\r\n                    CASE WHEN ISNULL(fLocalAmount1,mAmount1) < 0 THEN ISNULL(fLocalAmount1,mAmount1) ELSE 0 END LocalDr, \r\n\t                CASE WHEN ISNULL(fLocalAmount1,mAmount1) > 0 THEN ISNULL(fLocalAmount1,mAmount1) ELSE 0 END LocalCr\r\n                    FROM tCore_Data{0} \r\n                    JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                    JOIN mCore_Account ON tCore_Data{0}.iBookNo = mCore_Account.iMasterId\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                    AND tCore_Header{0}.iVoucherClass <> 256\r\n                )InnerTable\r\n                JOIN vrCore_Account ON InnerTable.iMasterId = vrCore_Account.iMasterId AND vrCore_Account.iTreeId = 0\r\n                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = InnerTable.iBodyId  \r\n                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                LEFT JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId \r\n                LEFT JOIN vrCore_Product  ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct AND vrCore_Product.iTreeId = 0\r\n                LEFT JOIN tCore_Indta{0} Cogs ON Cogs.iBodyId = tCore_Data{0}.iMainBodyId AND iType=5\r\n                LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = InnerTable.iBodyId\r\n                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                JOIN vrCore_Account BookNo  ON InnerTable.iBookNo = BookNo.iMasterId AND BookNo.iTreeId = 0\r\n                JOIN vrCore_Account Code  ON InnerTable.iMasterId = Code.iMasterId AND Code.iTreeId = 0\r\n                JOIN mCore_Currency WITH (READUNCOMMITTED) ON mCore_Currency.iCurrencyId = tCore_Data{0}.iCurrencyId  \r\n                WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA = 0  \r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n                AND tCore_Header{0}.iDate BETWEEN {1} AND {2} {3}\r\n                AND (Debit < 0 OR Credit > 0 OR TranDr < 0 OR TranCr > 0 OR LocalDr < 0 OR LocalCr > 0)  \r\n                GROUP BY tCore_Header{0}.iHeaderId, tCore_Header{0}.iDate,cCore_Vouchers{0}.sAbbr,cCore_Vouchers{0}.sName,tCore_Header{0}.sVoucherNo, Code.sName, Code.sCode,mCore_Currency.sName,Code.iMasterId\r\n                ORDER BY 3,4", _focus.company(iCompanyId).suffix, oInput.StartDate, oInput.EndDate, text2);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text3) : text3);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			int num10;
			num9 = (num10 = 0);
			while (dataReader.Read())
			{
				ledgerEntry = new LedgerEntry();
				date.Value = Convert.ToInt32(dataReader["iDate"]);
				ledgerEntry.TransactionDate = $"{date.Day}-{date.Month}-{date.Year}";
				ledgerEntry.AccountID = dataReader["Code2"].ToString();
				ledgerEntry.AccountName = Convert.ToString(dataReader["Account"]);
				ledgerEntry.TransactionDescription = Convert.ToString(dataReader["Source"]);
				ledgerEntry.SourceDocumentID = Convert.ToString(dataReader["Voucher"]);
				ledgerEntry.SourceType = Convert.ToString(dataReader["Source"]);
				ledgerEntry.Debit = Convert.ToDouble(dataReader["Debit"]).AddDecimalInColumn(2);
				ledgerEntry.Credit = Convert.ToDouble(dataReader["Credit"]).AddDecimalInColumn(2);
				num9 = Convert.ToDouble(dataReader["Debit"]) + Convert.ToDouble(dataReader["Credit"]);
				ledgerEntry.Balance = num9.AddDecimalInColumn(2);
				if (text != ledgerEntry.SourceDocumentID)
				{
					num10++;
				}
				num6 += Convert.ToDouble(dataReader["Debit"]);
				num7 += Convert.ToDouble(dataReader["Credit"]);
				num8 += Convert.ToDouble(dataReader["Debit"]) + Convert.ToDouble(dataReader["Credit"]);
				list5.Add(ledgerEntry);
			}
			dataReader.Close();
			if (list5 != null && list5.Count > 0)
			{
				ledger.LedgerEntry = list5;
				list.Add(ledger);
				companies.Ledger = list;
			}
			else
			{
				companies.Ledger = new List<Ledger>();
			}
			footer = new Footer();
			footer.TotalPurchasesCount = Convert.ToString(num2);
			footer.TotalPurchasesAmount = num4.AddDecimalInColumn(2);
			footer.TotalPurchasesAmountGST = "TotalPurchasesAmountGST";
			footer.TotalSupplyCount = Convert.ToString(num3);
			footer.TotalSupplyAmount = num5.AddDecimalInColumn(2);
			footer.TotalSupplyAmountGST = "TotalSupplyAmountGST";
			footer.TotalLedgerCount = Convert.ToString(num10);
			footer.TotalLedgerDebit = num6.AddDecimalInColumn(2);
			footer.TotalLedgerCredit = num7.AddDecimalInColumn(2);
			footer.TotalLedgerBalance = num8.AddDecimalInColumn(2);
			list6.Add(footer);
			if (list6 != null && list6.Count > 0)
			{
				companies.Footer = list6;
			}
			else
			{
				companies.Footer = new List<Footer>();
			}
			if (companies != null)
			{
				gSTAuditFile.Companies = companies;
			}
		}
		catch (Exception ex)
		{
			string strCatchError = (vATReturnsReportData.Message = ex.Message);
			m_strCatchError = strCatchError;
		}
		return gSTAuditFile;
	}

	public string GetJurisdictionCode(string strJuris)
	{
		strJuris = strJuris.ToLower();
		if (strJuris.Contains("dhabi"))
		{
			return "AUH";
		}
		if (strJuris.Contains("dubai"))
		{
			return "DXB";
		}
		if (strJuris.Contains("quwain"))
		{
			return "UAQ";
		}
		if (strJuris.Contains("sharjah"))
		{
			return "SHJ";
		}
		if (strJuris.Contains("ajman"))
		{
			return "AJM";
		}
		if (strJuris.Contains("rak") || strJuris.Contains("khaima"))
		{
			return "RAK";
		}
		if (strJuris.Contains("fujair"))
		{
			return "FUJ";
		}
		return "";
	}

	public VATReturnsReportData GetVATReturnsReport(VATAuditFileInput oInput, int iCompanyId)
	{
		if (false)
		{
			string[] array = null;
			IdValuePair[] array2 = null;
			VATCompanyInfo oComp = null;
			string sError = null;
			if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
			{
				oComp = getVATCompanyInfo(oInput.FaTagId, iCompanyId);
			}
			VATReturnData[] vATReturnData = GetVATReturnData(oInput, iCompanyId, ref sError);
			VatCommon vatCommon = new VatCommon();
			vatCommon.CalendarType = _focus.company(iCompanyId).calType;
			vatCommon.VatHeader = getVatReturnHeader(oInput.CountryId, oInput, vatCommon.CalendarType, iCompanyId, oComp);
			if (oInput.CountryId == CountryVAT.KSA)
			{
				return vatCommon.ArrangeDataForKSA(vATReturnData, oInput);
			}
			if (oInput.CountryId == CountryVAT.BH)
			{
				return vatCommon.ArrangeDataForBAHRAIN(vATReturnData, oInput);
			}
			if (oInput.CountryId == CountryVAT.OM)
			{
				array = getGCCPalceOfSupply(iCompanyId);
				return vatCommon.ArrangeDataForOMAN(vATReturnData, oInput, array);
			}
			if (oInput.CountryId == CountryVAT.NPL)
			{
				array2 = GetVATVoucherCount(oInput, iCompanyId, ref sError);
				return vatCommon.ArrangeDataForNepal(vATReturnData, array2, oInput);
			}
			return vatCommon.ArrangeDataForUAE(vATReturnData, oInput);
		}
		int num = 0;
		int num2 = 0;
		string text = null;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		Date date = new Date(CalendarType.Gregorean);
		Database database = null;
		DbCommand dbCommand = null;
		VATCompanyInfo oComp2 = null;
		VATReturnAmount vATReturnAmount = null;
		VATReturnsReportData vATReturnsReportData = new VATReturnsReportData();
		IDataReader dataReader = null;
		QueryGenerator queryGenerator = null;
		double num10 = 5.0;
		try
		{
			queryGenerator = new QueryGenerator();
			queryGenerator.Suffix = suffix;
			queryGenerator.m_iCompId = iCompanyId;
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			num = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 15);
			num2 = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 22);
			if (oInput.FaTagId > 0)
			{
				text = queryGenerator.GetMasterUnderGroup(oInput.FaTagId, _focus.company(iCompanyId).faTagId, database, iCompanyId);
			}
			else if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
			{
				text = queryGenerator.getMultiEntityTagId(oInput.CountryId, iCompanyId);
			}
			if (num == 0 && num2 == 0)
			{
				vATReturnsReportData.Message = "VAT Input account not set in preference";
			}
			else
			{
				num10 = Convert.ToDouble(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"select ISNULL(MAX(fPerc), 5) FROM mCore_TaxRate where iAffectDate = (select MAX(iAffectDate) from mCore_TaxRate where iAffectDate <= {oInput.StartDate})") : $"select ISNULL(MAX(fPerc), 5) FROM mCore_TaxRate where iAffectDate = (select MAX(iAffectDate) from mCore_TaxRate where iAffectDate <= {oInput.StartDate})"));
				string sError2 = null;
				if (oInput.CountryId == CountryVAT.KSA)
				{
					VATReturnData[] vATReturnData2 = GetVATReturnData(oInput, iCompanyId, ref sError2);
					VatCommon vatCommon2 = new VatCommon();
					vatCommon2.CalendarType = _focus.company(iCompanyId).calType;
					vatCommon2.VatHeader = getVatReturnHeader(oInput.CountryId, oInput, vatCommon2.CalendarType, iCompanyId, oComp2);
					return vatCommon2.ArrangeDataForKSA(vATReturnData2, oInput);
				}
				if (oInput.CountryId == CountryVAT.OM)
				{
					VATReturnData[] vATReturnData3 = GetVATReturnData(oInput, iCompanyId, ref sError2);
					VatCommon vatCommon3 = new VatCommon();
					vatCommon3.CalendarType = _focus.company(iCompanyId).calType;
					vatCommon3.VatHeader = getVatReturnHeader(oInput.CountryId, oInput, vatCommon3.CalendarType, iCompanyId, oComp2);
					string[] gCCPalceOfSupply = getGCCPalceOfSupply(iCompanyId);
					return vatCommon3.ArrangeDataForOMAN(vATReturnData3, oInput, gCCPalceOfSupply);
				}
				if (oInput.CountryId == CountryVAT.UAE)
				{
					if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
					{
						oComp2 = getVATCompanyInfo(oInput.FaTagId, iCompanyId);
					}
					VATReturnData[] vATReturnData4 = GetVATReturnData(oInput, iCompanyId, ref sError2);
					VatCommon vatCommon4 = new VatCommon();
					vatCommon4.CalendarType = _focus.company(iCompanyId).calType;
					vatCommon4.VatHeader = getVatReturnHeader(oInput.CountryId, oInput, vatCommon4.CalendarType, iCompanyId, oComp2);
					return vatCommon4.ArrangeDataForUAE(vATReturnData4, oInput);
				}
				vATReturnsReportData.HeaderData = new VATReturntHeader();
				if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
				{
					int iFaTagId = oInput.FaTagId;
					if (!string.IsNullOrEmpty(text))
					{
						if (text.Contains(","))
						{
							string[] array3 = text.Split(',');
							if (array3.Length != 0)
							{
								iFaTagId = Convert.ToInt32(array3[0]);
							}
						}
						else if (FConvert.IsNumeric(text))
						{
							iFaTagId = Convert.ToInt32(text);
						}
					}
					oComp2 = getVATCompanyInfo(iFaTagId, iCompanyId);
					if (oComp2 != null)
					{
						vATReturnsReportData.HeaderData.FormType = oComp2.FormType;
						vATReturnsReportData.HeaderData.DocumentLocator = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 14);
						vATReturnsReportData.HeaderData.TaxFromFillType = oComp2.TaxFormFilingType;
						date = date.GetToday(CalendarType.Gregorean);
						vATReturnsReportData.HeaderData.SubmissionDate = $"{date.Day:00}-{date.Month:00}-{date.Year}";
						vATReturnsReportData.HeaderData.TRN = oComp2.TRN;
						vATReturnsReportData.HeaderData.TAAN = oComp2.TAAN;
						vATReturnsReportData.HeaderData.TAN = oComp2.TAN;
						vATReturnsReportData.HeaderData.TaxablePersonNameAr = oComp2.TaxablePersonNameAr;
						vATReturnsReportData.HeaderData.TaxablePersonNameEn = oComp2.TaxablePersonNameEn;
						vATReturnsReportData.HeaderData.TaxablePersonAddress = oComp2.TaxablePersonAddress;
						vATReturnsReportData.HeaderData.TaxAgencyName = oComp2.TaxAgencyName;
						vATReturnsReportData.HeaderData.TaxAgentName = oComp2.TaxAgentName;
					}
				}
				else
				{
					vATReturnsReportData.HeaderData.FormType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 13);
					vATReturnsReportData.HeaderData.DocumentLocator = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 14);
					vATReturnsReportData.HeaderData.TaxFromFillType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 15);
					date = date.GetToday(CalendarType.Gregorean);
					vATReturnsReportData.HeaderData.SubmissionDate = $"{date.Day:00}-{date.Month:00}-{date.Year}";
					vATReturnsReportData.HeaderData.TRN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 16);
					vATReturnsReportData.HeaderData.TAAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 24);
					vATReturnsReportData.HeaderData.TAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 21);
					vATReturnsReportData.HeaderData.TaxablePersonNameAr = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 18);
					vATReturnsReportData.HeaderData.TaxablePersonNameEn = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 17);
					vATReturnsReportData.HeaderData.TaxablePersonAddress = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 19);
					vATReturnsReportData.HeaderData.TaxAgencyName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 20);
					vATReturnsReportData.HeaderData.TaxAgentName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 23);
				}
				date.Value = oInput.StartDate;
				vATReturnsReportData.HeaderData.VATReturnPeriod = $"{date.Day:00}/{date.Month:00}/{date.Year}";
				vATReturnsReportData.HeaderData.PeriiodReferenceNo = $"{date.Month:00}-{date.Year}";
				date.Value = oInput.EndDate;
				vATReturnsReportData.HeaderData.VATReturnPeriod += $" - {date.Day:00}/{date.Month:00}/{date.Year}";
				vATReturnsReportData.HeaderData.TaxYear = $"{date.Day:00} {date.GetMonthName()} {date.Year}";
				Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre("select iMasterTypeId from cCore_MasterDef where sMasterName = 'PlaceOfSupply'") : "select iMasterTypeId from cCore_MasterDef where sMasterName = 'PlaceOfSupply'"));
				int iJuris = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre("select iMasterTypeId from cCore_MasterDef where sMasterName = 'Jurisdiction'") : "select iMasterTypeId from cCore_MasterDef where sMasterName = 'Jurisdiction'"));
				Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre("select iMasterTypeId from cCore_MasterDef where sMasterName = 'TaxCode'") : "select iMasterTypeId from cCore_MasterDef where sMasterName = 'TaxCode'"));
				string strScreenData = queryGenerator.CreateScreenDataView(Focus.Common.DataStructs.Module.Inventory, null, null, new List<string> { "Taxable" }, database, null, iCompanyId);
				queryGenerator.CreateTranDataView(new RepRecord
				{
					Module = Focus.Common.DataStructs.Module.Inventory
				}, null, bClassType: false, new List<string> { "VAT" }, database, iCompanyId);
				string strTranData = queryGenerator.CreateTranDataView(new RepRecord
				{
					Module = Focus.Common.DataStructs.Module.CoreTransactions
				}, null, bClassType: false, new List<string> { "TaxCode" }, database, iCompanyId);
				text4 = GetStandardQuery(suffix, oInput.StartDate, oInput.EndDate, num, num2, strScreenData, strTranData, text, bSales: true, iCompanyId, iJuris, oInput.EnableLocalCurrency, num10);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text4) : text4);
				dbCommand.CommandTimeout = 0;
				dataReader = database.ExecuteReader(dbCommand);
				vATReturnsReportData.BodyData = new Dictionary<string, VATReturnAmount>();
				num7 = (num8 = (num9 = 0.0));
				double num11 = 0.0;
				double num12 = 0.0;
				while (dataReader.Read())
				{
					text2 = Convert.ToString(dataReader["TaxCode"]);
					text3 = Convert.ToString(dataReader["Jurisdiction"]);
					num5 = Convert.ToDouble(dataReader["Vat"]);
					num3 = Convert.ToDouble(dataReader["Net"]);
					num4 = Convert.ToDouble(dataReader["Taxable"]);
					vATReturnAmount = null;
					if (text2 == "ZR")
					{
						num5 = 0.0;
					}
					if (num5 == 0.0)
					{
						num3 = Convert.ToDouble(dataReader["Net"]);
					}
					else
					{
						num3 = ((num4 == 0.0) ? (100.0 / num10 * num5) : num4);
					}
					if (text2 == null)
					{
						continue;
					}
					int length = text2.Length;
					if (length != 2)
					{
						if (length != 4 || !(text2 == "RCMS"))
						{
							continue;
						}
					}
					else
					{
						switch (text2[0])
						{
						case 'O':
							if (!(text2 == "OA"))
							{
								continue;
							}
							goto IL_0b54;
						case 'I':
							if (!(text2 == "IA"))
							{
								continue;
							}
							goto IL_0b54;
						case 'S':
							if (!(text2 == "SR") && !(text2 == "SD"))
							{
								continue;
							}
							if (num5 == 0.0 || text2 == "SD")
							{
								num3 = 0.0;
							}
							if (IsUAEJurisdiction(text3))
							{
								vATReturnsReportData.BodyData.TryGetValue(GetJurisdictionCode(text3), out vATReturnAmount);
								if (vATReturnAmount == null)
								{
									vATReturnAmount = new VATReturnAmount();
									vATReturnAmount.VATAmount += num5;
									vATReturnAmount.Amount += num3;
									vATReturnsReportData.BodyData.Add(GetJurisdictionCode(text3), vATReturnAmount);
								}
								else
								{
									vATReturnAmount.VATAmount += num5;
									vATReturnAmount.Amount += num3;
									vATReturnsReportData.BodyData[text3] = vATReturnAmount;
								}
								num7 += num3;
								num8 += num5;
								num9 += num6;
							}
							continue;
						case 'R':
							break;
						case 'Z':
							if (!(text2 == "ZR"))
							{
								if (text2 == "ZE")
								{
									vATReturnsReportData.ZeroRateExport += num3;
									num5 = 0.0;
								}
								continue;
							}
							if (num3 < 0.0)
							{
								num3 *= -1.0;
							}
							vATReturnsReportData.ZeroRateSupply += num3;
							num5 = 0.0;
							num7 += num3;
							continue;
						case 'E':
							if (text2 == "EX")
							{
								if (num3 < 0.0)
								{
									num3 *= -1.0;
								}
								vATReturnsReportData.ExemptedSupply += num3;
								num5 = 0.0;
								num7 += num3;
							}
							continue;
						case 'C':
							if (text2 == "CO")
							{
								vATReturnsReportData.CorrectionFromPreviousPeriod += num3;
								num5 = 0.0;
							}
							continue;
						case 'T':
							if (text2 == "TT")
							{
								if (num5 == 0.0)
								{
									num3 = 0.0;
								}
								if (vATReturnsReportData.TouristRefund == null)
								{
									vATReturnsReportData.TouristRefund = new VATReturnAmount();
								}
								vATReturnsReportData.TouristRefund.Amount += num3;
								vATReturnsReportData.TouristRefund.VATAmount += num5;
								num7 += num3;
								num8 += num5;
								num9 += num6;
							}
							continue;
						default:
							continue;
							IL_0b54:
							vATReturnsReportData.BodyData.TryGetValue(GetJurisdictionCode(text3), out vATReturnAmount);
							if (vATReturnAmount == null)
							{
								vATReturnAmount = new VATReturnAmount();
							}
							vATReturnAmount.Adjustments += num3;
							vATReturnsReportData.BodyData[text3] = vATReturnAmount;
							num3 = 0.0;
							continue;
						}
						if (!(text2 == "RC"))
						{
							continue;
						}
					}
					if (num5 == 0.0)
					{
						num3 = 0.0;
					}
					if (vATReturnsReportData.SupplierReverseCharge == null)
					{
						vATReturnsReportData.SupplierReverseCharge = new VATReturnAmount();
					}
					vATReturnsReportData.SupplierReverseCharge.Amount += num3;
					vATReturnsReportData.SupplierReverseCharge.VATAmount += num5;
					num7 += num3;
					num8 += num5;
					num9 += num6;
					if (text2 == "RCMS")
					{
						num11 += num3;
						num12 += num5;
					}
				}
				dataReader.Close();
				vATReturnsReportData.TotalSales = new VATReturnAmount();
				vATReturnsReportData.TotalSales.Amount = num7;
				vATReturnsReportData.TotalSales.VATAmount = num8;
				vATReturnsReportData.TotalSales.Adjustments = num9;
				vATReturnsReportData.FooterData = new VATReturnFooter();
				vATReturnsReportData.FooterData.DueTaxPeriod = num8 + num9;
				text4 = GetStandardQuery(suffix, oInput.StartDate, oInput.EndDate, num, num2, strScreenData, strTranData, text, bSales: false, iCompanyId, iJuris, oInput.EnableLocalCurrency, num10);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text4) : text4);
				dbCommand.CommandTimeout = 0;
				dataReader = database.ExecuteReader(dbCommand);
				num7 = num11;
				num8 = num12;
				num9 = 0.0;
				while (dataReader.Read())
				{
					text2 = Convert.ToString(dataReader["TaxCode"]);
					num3 = Convert.ToDouble(dataReader["Net"]);
					num5 = Convert.ToDouble(dataReader["Vat"]);
					num4 = Convert.ToDouble(dataReader["Taxable"]);
					vATReturnAmount = null;
					if (num5 == 0.0)
					{
						num3 = Convert.ToDouble(dataReader["Net"]);
					}
					else
					{
						num3 = ((num4 == 0.0) ? (100.0 / num10 * num5) : num4);
					}
					switch (text2)
					{
					case "SR":
					case "SR-REC":
						if (num5 == 0.0 && text2 != "SR-NON")
						{
							num3 = 0.0;
						}
						if (vATReturnsReportData.StandardRateExpenses == null)
						{
							vATReturnsReportData.StandardRateExpenses = new VATReturnAmount();
						}
						vATReturnsReportData.StandardRateExpenses.Amount += num3;
						vATReturnsReportData.StandardRateExpenses.VATAmount += num5;
						num7 += num3;
						num8 += num5;
						num9 += num6;
						break;
					case "RC":
					case "RCMS":
						if (num5 == 0.0)
						{
							num3 = 0.0;
						}
						if (vATReturnsReportData.SupplierReverseCharge == null)
						{
							vATReturnsReportData.SupplierReverseCharge = new VATReturnAmount();
						}
						vATReturnsReportData.SupplierReverseCharge.Amount += num3;
						vATReturnsReportData.SupplierReverseCharge.VATAmount += num5;
						num7 += num3;
						num8 += num5;
						num9 += num6;
						vATReturnsReportData.TotalSales.Amount += num3;
						vATReturnsReportData.TotalSales.VATAmount += num5;
						vATReturnsReportData.TotalSales.Adjustments += num6;
						vATReturnsReportData.FooterData.DueTaxPeriod += num5;
						break;
					case "IC":
						if (num5 == 0.0)
						{
							num3 = 0.0;
						}
						if (vATReturnsReportData.SupplierRCImport == null)
						{
							vATReturnsReportData.SupplierRCImport = new VATReturnAmount();
						}
						vATReturnsReportData.SupplierRCImport.Amount += num3;
						vATReturnsReportData.SupplierRCImport.VATAmount += num5;
						vATReturnsReportData.TotalSales.Amount += num3;
						vATReturnsReportData.TotalSales.VATAmount += num5;
						vATReturnsReportData.FooterData.DueTaxPeriod += num5;
						num7 += num3;
						num8 += num5;
						break;
					case "AM":
						if (num5 == 0.0)
						{
							num3 = 0.0;
						}
						if (vATReturnsReportData.SupplierImportCorrection == null)
						{
							vATReturnsReportData.SupplierImportCorrection = new VATReturnAmount();
						}
						vATReturnsReportData.SupplierImportCorrection.Amount += num3;
						vATReturnsReportData.SupplierImportCorrection.VATAmount += num5;
						vATReturnsReportData.TotalSales.Amount += Math.Abs(num3);
						vATReturnsReportData.TotalSales.VATAmount += Math.Abs(num5);
						vATReturnsReportData.FooterData.DueTaxPeriod += num5;
						break;
					}
				}
				dataReader.Close();
				vATReturnsReportData.TotalPurchase = new VATReturnAmount();
				vATReturnsReportData.TotalPurchase.Amount = num7;
				vATReturnsReportData.TotalPurchase.VATAmount = num8;
				vATReturnsReportData.TotalPurchase.Adjustments = num9;
				vATReturnsReportData.FooterData.RecoverableTaxPeriod = vATReturnsReportData.TotalPurchase.VATAmount;
				vATReturnsReportData.FooterData.NetVatDuePeriod = vATReturnsReportData.FooterData.DueTaxPeriod - vATReturnsReportData.FooterData.RecoverableTaxPeriod;
			}
		}
		catch (Exception ex)
		{
			string strCatchError = (vATReturnsReportData.Message = ex.Message);
			m_strCatchError = strCatchError;
		}
		return vATReturnsReportData;
	}

	private VATReturntHeader getVatReturnHeader(CountryVAT oCountry, VATAuditFileInput oInput, CalendarType oCalType, int iCompanyId, VATCompanyInfo oComp)
	{
		VATReturntHeader vATReturntHeader = new VATReturntHeader();
		Date date = new Date(oCalType);
		switch (oCountry)
		{
		case CountryVAT.UAE:
			if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
			{
				if (oComp != null)
				{
					vATReturntHeader.FormType = oComp.FormType;
					vATReturntHeader.DocumentLocator = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 14);
					vATReturntHeader.TaxFromFillType = oComp.TaxFormFilingType;
					date = date.GetToday(oCalType);
					vATReturntHeader.SubmissionDate = $"{date.Day:00}-{date.Month:00}-{date.Year}";
					vATReturntHeader.TRN = oComp.TRN;
					vATReturntHeader.TAAN = oComp.TAAN;
					vATReturntHeader.TAN = oComp.TAN;
					vATReturntHeader.TaxablePersonNameAr = oComp.TaxablePersonNameAr;
					vATReturntHeader.TaxablePersonNameEn = oComp.TaxablePersonNameEn;
					vATReturntHeader.TaxablePersonAddress = oComp.TaxablePersonAddress;
					vATReturntHeader.TaxAgencyName = oComp.TaxAgencyName;
					vATReturntHeader.TaxAgentName = oComp.TaxAgentName;
				}
			}
			else
			{
				vATReturntHeader.FormType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 13);
				vATReturntHeader.DocumentLocator = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 14);
				vATReturntHeader.TaxFromFillType = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 15);
				date = date.GetToday(oCalType);
				vATReturntHeader.SubmissionDate = $"{date.Day:00}-{date.Month:00}-{date.Year}";
				vATReturntHeader.TRN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 16);
				vATReturntHeader.TAAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 24);
				vATReturntHeader.TAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 21);
				vATReturntHeader.TaxablePersonNameAr = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 18);
				vATReturntHeader.TaxablePersonNameEn = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 17);
				vATReturntHeader.TaxablePersonAddress = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 19);
				vATReturntHeader.TaxAgencyName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 20);
				vATReturntHeader.TaxAgentName = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 23);
			}
			date.Value = oInput.StartDate;
			vATReturntHeader.VATReturnPeriod = $"{date.Day:00}/{date.Month:00}/{date.Year}";
			vATReturntHeader.PeriiodReferenceNo = $"{date.Month:00}-{date.Year}";
			date.Value = oInput.EndDate;
			vATReturntHeader.VATReturnPeriod += $" - {date.Day:00}/{date.Month:00}/{date.Year}";
			vATReturntHeader.TaxYear = $"{date.Day:00} {date.GetMonthName()} {date.Year}";
			break;
		case CountryVAT.NPL:
			vATReturntHeader.TaxablePersonAddress = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 19);
			vATReturntHeader.TRN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 16);
			vATReturntHeader.TAAN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 24);
			date.Value = oInput.StartDate;
			vATReturntHeader.VATReturnPeriod = $"For the Period ({date.Day:00}-{date.Month:00}-{date.Year}";
			date.Value = oInput.EndDate;
			vATReturntHeader.VATReturnPeriod += $" to {date.Day:00}-{date.Month:00}-{date.Year})";
			break;
		case CountryVAT.OM:
		{
			vATReturntHeader.TaxablePersonNameEn = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 17);
			vATReturntHeader.TRN = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.Info, 11);
			vATReturntHeader.TaxablePersonAddress = _focus.company(iCompanyId).getPreferenceText(PreferenceCategories.VAT, 19);
			date = date.GetToday(oCalType);
			vATReturntHeader.TaxYear = $"{date.Day:00} {date.GetMonthName()} {date.Year}";
			date.Value = oInput.StartDate;
			vATReturntHeader.VATReturnPeriod = $"{date.Day:00}/{date.Month:00}/{date.Year}";
			date.Value = oInput.EndDate;
			vATReturntHeader.VATReturnPeriod += $" - {date.Day:00}/{date.Month:00}/{date.Year}";
			VATReturntHeader vATReturntHeader2 = vATReturntHeader;
			string periiodReferenceNo;
			if (date.Month >= 4 && date.Month <= 6)
			{
				periiodReferenceNo = "Quarter 1";
			}
			else if (date.Month >= 7 && date.Month <= 9)
			{
				periiodReferenceNo = "Quarter 2";
			}
			else
			{
				periiodReferenceNo = ((date.Month >= 10 && date.Month <= 12) ? "Quarter 3" : "Quarter 4");
			}
			vATReturntHeader2.PeriiodReferenceNo = periiodReferenceNo;
			break;
		}
		}
		return vATReturntHeader;
	}

	public VATReturnData[] GetVATReturnData(VATAuditFileInput oInput, int iCompanyId, ref string sError)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		string text7 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text8 = $"tCore_Data{suffix}.mAmount2";
		string text9 = $"ISNULL(tCore_Data_FX{suffix}.fExchangeRate,1)";
		int num = ((_focus.company(iCompanyId).currency == null) ? 3 : _focus.company(iCompanyId).currency.NoOfDecimal);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		double num9 = 0.0;
		bool flag = false;
		List<int> list = new List<int>();
		QueryGenerator queryGenerator = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<VATReturnData> list2 = null;
		List<IdValuePair> list3 = new List<IdValuePair>();
		list2 = new List<VATReturnData>();
		num2 = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 15);
		num3 = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 22);
		num4 = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 26);
		num5 = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 27);
		if (num2 == 0 && num3 == 0)
		{
			sError = "VAT Input account not set in preference";
		}
		else
		{
			queryGenerator = new QueryGenerator();
			queryGenerator.Suffix = suffix;
			queryGenerator.m_iCompId = iCompanyId;
			try
			{
				if (oInput.FaTagId > 0)
				{
					text = queryGenerator.GetMasterUnderGroup(oInput.FaTagId, _focus.company(iCompanyId).faTagId, database, iCompanyId);
				}
				else if (_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) > 0)
				{
					text = queryGenerator.getMultiEntityTagId(oInput.CountryId, iCompanyId);
				}
				num7 = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre("select iMasterTypeId from cCore_MasterDef where sMasterName = 'PlaceOfSupply'") : "select iMasterTypeId from cCore_MasterDef where sMasterName = 'PlaceOfSupply'"));
				num6 = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre("select iMasterTypeId from cCore_MasterDef where sMasterName = 'Jurisdiction'") : "select iMasterTypeId from cCore_MasterDef where sMasterName = 'Jurisdiction'"));
				text2 = queryGenerator.CreateScreenDataView(Focus.Common.DataStructs.Module.Inventory, null, null, new List<string> { "Taxable" }, database, null, iCompanyId);
				text3 = queryGenerator.CreateTranDataView(new RepRecord
				{
					Module = Focus.Common.DataStructs.Module.CoreTransactions
				}, null, bClassType: false, new List<string> { "TaxCode" }, database, iCompanyId);
				text4 = queryGenerator.getVatInnerQuery(oInput.StartDate, oInput.EndDate, oInput.EnableLocalCurrency && oInput.FaTagId > 0, iCompanyId);
				text5 = queryGenerator.getVatOrderBy(oInput.CountryId);
				num8 = ((oInput.StartDate > 0) ? oInput.StartDate : _focus.company(iCompanyId).accountingDate);
				num9 = Convert.ToDouble(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"select ISNULL(MAX(fPerc), 5) FROM mCore_TaxRate where iAffectDate = (select MAX(iAffectDate) from mCore_TaxRate where iAffectDate <= {num8})") : $"select ISNULL(MAX(fPerc), 5) FROM mCore_TaxRate where iAffectDate = (select MAX(iAffectDate) from mCore_TaxRate where iAffectDate <= {num8})"));
				text6 = ((num7 > 0 && oInput.CountryId != CountryVAT.UAE) ? "vmCore_PlaceOfSupply" : "vmCore_Jurisdiction");
				num7 = ((num7 > 0 && num6 > 0 && oInput.CountryId != CountryVAT.OM && oInput.CountryId != CountryVAT.NPL && oInput.CountryId != CountryVAT.KSA) ? num6 : num7);
				if (!string.IsNullOrEmpty(text))
				{
					text = ((oInput.FaTagId <= 0) ? ("AND tCore_Data" + suffix + ".iFaTag IN(" + text + ")") : ("AND tCore_Data" + suffix + ".iFaTag IN" + text));
					if (oInput.EnableLocalCurrency)
					{
						text8 = string.Format("ISNULL(tCore_Data_FX{0}.fLocalAmount2, tCore_Data{0}.mAmount2)", suffix);
						text9 = $"ISNULL(tCore_Data_FX{suffix}.fLocalExchangeRate,1)";
					}
				}
				if (oInput.CountryId == CountryVAT.KSA || oInput.CountryId == CountryVAT.OM)
				{
					text7 = string.Format("SELECT a.iLinkPathId FROM mCore_LinkPath{0} a JOIN mCore_LinkNodes{0} b ON a.iToLinkId = b.iLinkId WHERE (b.iVoucherType & 0xff00) = 1792", suffix);
					dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
					while (dataReader.Read())
					{
						list.Add(Convert.ToInt32(dataReader["iLinkPathId"]));
						flag = true;
					}
					dataReader.Close();
					if (!flag)
					{
						text7 = $"SELECT iVoucherType FROM cCore_VoucherFields{suffix} WHERE (iVoucherType & 0xff00) = 1792 AND sFieldName = 'InvoiceNo' AND bHeader = 1";
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
						while (dataReader.Read())
						{
							list.Add(Convert.ToInt32(dataReader["iVoucherType"]));
						}
						dataReader.Close();
					}
					if (list.Count > 0)
					{
						text7 = queryGenerator.GetStandardQuery(oInput.StartDate, oInput.EndDate, num2, num3, text2, bSales: true, list.ToArray(), flag);
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
						while (dataReader.Read())
						{
							list3.Add(new IdValuePair(Convert.ToInt32(dataReader["iBodyId"]), Convert.ToDecimal(dataReader["Taxable"])));
						}
						dataReader.Close();
					}
					flag = false;
					list.Clear();
					text7 = string.Format("SELECT a.iLinkPathId FROM mCore_LinkPath{0} a JOIN mCore_LinkNodes{0} b ON a.iToLinkId = b.iLinkId WHERE (b.iVoucherType & 0xff00) = 6400", suffix);
					dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
					while (dataReader.Read())
					{
						list.Add(Convert.ToInt32(dataReader["iLinkPathId"]));
						flag = true;
					}
					dataReader.Close();
					if (!flag)
					{
						text7 = $"SELECT iVoucherType FROM cCore_VoucherFields{suffix} WHERE (iVoucherType & 0xff00) = 6400 AND sFieldName = 'InvoiceNo' AND bHeader = 1";
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
						while (dataReader.Read())
						{
							list.Add(Convert.ToInt32(dataReader["iVoucherType"]));
						}
						dataReader.Close();
					}
					if (list.Count > 0)
					{
						text7 = queryGenerator.GetStandardQuery(oInput.StartDate, oInput.EndDate, num2, num3, text2, bSales: false, list.ToArray(), flag);
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
						while (dataReader.Read())
						{
							list3.Add(new IdValuePair(Convert.ToInt32(dataReader["iBodyId"]), Convert.ToDecimal(dataReader["Taxable"])));
						}
						dataReader.Close();
					}
				}
				string empty = string.Empty;
				string text10 = text8;
				if (oInput.CountryId == CountryVAT.UAE)
				{
					text10 = string.Format("CASE WHEN tCore_Header{0}.iVoucherClass = {1} THEN -{2} ELSE {2} END", suffix, 4096, text8);
				}
				empty = $"WHEN ROUND(InnerTable.[VATAmt],{num}) = 0 AND vmCore_TaxCode.sCode != 'ZR' THEN 0 ";
				text7 = $"SELECT tCore_Data{suffix}.iBodyId,tCore_Header{suffix}.iDate[Date]\r\n                    ,SIGN(CASE WHEN InnerTable.[VATAmt] = 0 THEN ISNULL(vCore_ScreenData.Taxable,{text10}) ELSE InnerTable.[VATAmt] END) * ABS(CASE WHEN tCore_Data{suffix}.iCode IN({num4},{num5}) THEN InnerTable.Net \r\n                        WHEN tCore_Indta{suffix}.mGross IS NULL OR InnerTable.[VATAmt] = 0 THEN {text8} ELSE tCore_Indta{suffix}.mGross END)[Net]\r\n                    ,SIGN(CASE WHEN InnerTable.[VATAmt] = 0 THEN ISNULL(vCore_ScreenData.Taxable,{text10}) ELSE InnerTable.[VATAmt] END) * ABS(CASE {empty} \r\n                        WHEN vCore_ScreenData.Taxable IS NOT NULL THEN {text9} * vCore_ScreenData.Taxable ELSE CASE WHEN tCore_Data{suffix}.iCode IN({num4},{num5}) THEN ROUND((InnerTable.Net * 100 / (100 + CASE WHEN vmCore_TaxCode.sCode = 'SR5' THEN 5 ELSE {num9} END)), 2) \r\n                        ELSE {text9} * tCore_Data{suffix}.mOriginalAmount - (CASE WHEN tCore_Data{suffix}.mOriginalAmount < 0 THEN -InnerTable.[VATAmt] ELSE ABS(InnerTable.[VATAmt]) END) END END)[Taxable]\r\n                    ,InnerTable.[VATAmt][VAT]\r\n                    ,{text5}[Grouping],tCore_Header{suffix}.iVoucherClass, vmCore_TaxCode.sCode[TaxCode], {text6}.sName[Jurisdiction]\r\n                    ,CASE WHEN tCore_Header{suffix}.iVoucherClass = 1792 THEN 2 WHEN tCore_Header{suffix}.iVoucherClass = 6400 THEN 4 WHEN tCore_Header{suffix}.iVoucherClass NOT IN (768,6400,2560,1280) THEN 1 ELSE 3 END[VType]\r\n                    FROM \r\n                    (\r\n                        {text4}\r\n                    )InnerTable \r\n                    JOIN tCore_Data{suffix} ON InnerTable.iMainBodyId = tCore_Data{suffix}.iBodyId \r\n                    JOIn tCore_Header{suffix} ON tCore_Header{suffix}.iHeaderId = tCore_Data{suffix}.iHeaderId\r\n                    JOIN cCore_Vouchers{suffix} ON cCore_Vouchers{suffix}.iVoucherType = tCore_Header{suffix}.iVoucherType\r\n                    LEFT JOIN tCore_Data_FX{suffix} ON tCore_Data_FX{suffix}.iBodyId = tCore_Data{suffix}.iBodyId \r\n                    LEFT JOIN tCore_Indta{suffix}  ON tCore_Data{suffix}.iBodyId = tCore_Indta{suffix}.iBodyId  \r\n                    LEFT JOIN tCore_Data_Tags{suffix} Tag{num6} ON Tag{num6}.iBodyId = tCore_Data{suffix}.iBodyId \r\n                    LEFT JOIN vmCore_Jurisdiction ON vmCore_Jurisdiction.iMasterId = Tag{num6}.iTag{num6} \r\n                    LEFT JOIN vmCore_PlaceOfSupply ON vmCore_PlaceOfSupply.iMasterId = Tag{num6}.iTag{num7}\r\n                    LEFT JOIN\r\n                    (\r\n                        {text2}\r\n                    )vCore_ScreenData ON vCore_ScreenData.iBodyId = tCore_Data{suffix}.iBodyId\r\n                    LEFT JOIN \r\n                    (\r\n                        {text3}\r\n                    )vCore_TranData ON vCore_TranData.iBodyId = tCore_Data{suffix}.iBodyId\r\n                    JOIN vmCore_TaxCode ON vmCore_TaxCode.iMasterId = vCore_TranData.TaxCode AND vmCore_TaxCode.iTreeId = 0\r\n                    WHERE tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Data{suffix}.bSuspendUpdateFA = 0 \r\n                    AND tCore_Header{suffix}.bSuspended = 0 AND tCore_Data{suffix}.iAuthStatus < 2\r\n                    AND cCore_Vouchers{suffix}.bPostVAT = 1 AND tCore_Header{suffix}.iDate BETWEEN {oInput.StartDate} AND {oInput.EndDate}\r\n                    AND (tCore_Data{suffix}.iType = 0 OR (bInventory = 1 AND (iBookNo IN({num4},{num5}) OR iCode IN({num4},{num5}))))  \r\n                    {text}\r\n                    ORDER BY {text5}, tCore_Header{suffix}.iDate,tCore_Header{suffix}.iCreatedTime";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text7) : text7);
				dbCommand.CommandTimeout = 0;
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					for (num8 = 0; num8 < list3.Count; num8++)
					{
						if (list3[num8].ID == Convert.ToInt32(dataReader["iBodyId"]))
						{
							list2.Add(new VATReturnData
							{
								BodyId = Convert.ToInt32(dataReader["iBodyId"]),
								Type = (VATDataType)Convert.ToInt32(dataReader["VType"]),
								Grouping = Convert.ToString(dataReader["Grouping"]),
								TaxCode = Convert.ToString(dataReader["TaxCode"]),
								Jurisdiction = Convert.ToString(dataReader["Jurisdiction"]),
								AdjVATAmount = Convert.ToDecimal(dataReader["Vat"]),
								Adjustment = ((oInput.CountryId == CountryVAT.OM) ? Convert.ToDecimal(dataReader["Vat"]) : Convert.ToDecimal(list3[num8].Value))
							});
							break;
						}
					}
					if (num8 == list3.Count)
					{
						list2.Add(new VATReturnData
						{
							BodyId = Convert.ToInt32(dataReader["iBodyId"]),
							Type = (VATDataType)Convert.ToInt32(dataReader["VType"]),
							Grouping = Convert.ToString(dataReader["Grouping"]),
							TaxCode = Convert.ToString(dataReader["TaxCode"]),
							Jurisdiction = Convert.ToString(dataReader["Jurisdiction"]),
							VATAmount = Convert.ToDecimal(dataReader["Vat"]),
							NetAmount = Convert.ToDecimal(dataReader["Net"]),
							TaxableAmount = Convert.ToDecimal(dataReader["Taxable"])
						});
					}
				}
				dataReader.Close();
			}
			catch (Exception ex)
			{
				m_strCatchError = (sError = ex.Message);
			}
		}
		return list2.ToArray();
	}

	public IdValuePair[] GetVATVoucherCount(VATAuditFileInput oInput, int iCompanyId, ref string sError)
	{
		string text = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdValuePair> list = new List<IdValuePair>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = string.Format("select tCore_Header{0}.iVoucherClass, Count(tCore_Header{0}.iHeaderId)[Count] FROM \r\n                tCore_Header{0} JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n                JOIN cCore_Vouchers{0} WITH(READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n                WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA = 0 \r\n                AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2\r\n                AND cCore_Vouchers{0}.bPostVAT = 1 AND tCore_Header{0}.iDate BETWEEN {1} AND {2} \r\n                AND tCore_Data{0}.iType = 0 AND tCore_Header{0}.iVoucherClass IN({3},{4},{5},{6})\r\n                GROUP BY tCore_Header{0}.iVoucherClass", FConvert.GetSuffix(iCompanyId), oInput.StartDate, oInput.EndDate, 768, 3328, 3840, 4096);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdValuePair
				{
					ID = Convert.ToInt32(dataReader["iVoucherClass"]),
					Value = Convert.ToInt32(dataReader["Count"])
				});
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_strCatchError = (sError = ex.Message);
		}
		return list.ToArray();
	}

	public VATCompanyInfo getVATCompanyInfo(int iFaTagId, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		IDataReader dataReader = null;
		VATCompanyInfo vATCompanyInfo = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			vATCompanyInfo = new VATCompanyInfo();
			text = $"SELECT 'mu' + sModule + '_' + sMasterName + '_VAT_Settings' sVatTable FROM cCore_MasterDef WHERE iMasterTypeId = {_focus.company(iCompanyId).faTagId}";
			text2 = Convert.ToString(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
			text = $"SELECT FormType, TaxFormFilingType, TRN, TaxablePersonNameEnglish, TaxablePersonNameArabic, \r\n                    TaxablePersonAddress, TaxAgencyName, TAN, TaxAgentName, TAAN \r\n                    FROM {text2} WHERE iMasterId = {iFaTagId}";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			if (dataReader.Read())
			{
				vATCompanyInfo.FormType = Convert.ToString(dataReader["FormType"]);
				vATCompanyInfo.TaxFormFilingType = Convert.ToString(dataReader["TaxFormFilingType"]);
				vATCompanyInfo.TaxablePersonAddress = Convert.ToString(dataReader["TaxablePersonAddress"]);
				vATCompanyInfo.TRN = Convert.ToString(dataReader["TRN"]);
				vATCompanyInfo.TaxablePersonNameEn = Convert.ToString(dataReader["TaxablePersonNameEnglish"]);
				vATCompanyInfo.TaxablePersonNameAr = Convert.ToString(dataReader["TaxablePersonNameArabic"]);
				vATCompanyInfo.TaxAgencyName = Convert.ToString(dataReader["TaxAgencyName"]);
				vATCompanyInfo.TAN = Convert.ToString(dataReader["TAN"]);
				vATCompanyInfo.TaxAgentName = Convert.ToString(dataReader["TaxAgentName"]);
				vATCompanyInfo.TAAN = Convert.ToString(dataReader["TAAN"]);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_strCatchError = ex.Message;
		}
		return vATCompanyInfo;
	}

	private bool IsUAEJurisdiction(string sJurisdiction)
	{
		sJurisdiction = sJurisdiction.ToLower();
		if (!sJurisdiction.Contains("dubai") && !sJurisdiction.Contains("sharjah") && !sJurisdiction.Contains("dhabi") && !sJurisdiction.Contains("fujair") && !sJurisdiction.Contains("rak") && !sJurisdiction.Contains("khaima") && !sJurisdiction.Contains("quwain"))
		{
			return sJurisdiction.Contains("ajman");
		}
		return true;
	}

	private string GetStandardQuery(string sSuffix, int iStartDate, int iEndDate, int iVATInputAc, int iVATOutputAc, string strScreenData, string strTranData, string strFaTagIds, bool bSales, int iCompanyId, int iJuris, bool bLocalCurrency, double dVatPerc, int[] arrReturnLinkId = null, bool bIgnoreSalesReturn = false, bool bReturnLinkExist = false)
	{
		int num = ((_focus.company(iCompanyId).currency == null) ? 3 : _focus.company(iCompanyId).currency.NoOfDecimal);
		string text = null;
		string text2 = null;
		string text3 = "vmCore_Jurisdiction";
		string text4 = ((!bSales) ? $" AND tCore_Header{sSuffix}.iVoucherClass NOT IN({3328},{1792},{5632},{6144})" : $" AND tCore_Header{sSuffix}.iVoucherClass NOT IN({768},{6400},{2560},{1280})");
		if (m_iPlaceOfSupply > 0)
		{
			text3 = "vmCore_PlaceOfSupply";
			iJuris = m_iPlaceOfSupply;
		}
		if (bIgnoreSalesReturn)
		{
			text4 = ((!bSales) ? $" AND tCore_Header{sSuffix}.iVoucherClass NOT IN({3328},{1792},{5632},{6144},{6400})" : $" AND tCore_Header{sSuffix}.iVoucherClass NOT IN({768},{6400},{2560},{1280},{1792})");
		}
		if (arrReturnLinkId != null && arrReturnLinkId.Length != 0 && !bIgnoreSalesReturn)
		{
			text4 = $" AND tCore_Header{sSuffix}.iVoucherClass = {(bSales ? 1792 : 6400)}";
			text2 = ",BaseReturn.iDate";
		}
		string text5 = $"tCore_Data{sSuffix}.mAmount1";
		string text6 = $"tCore_Data{sSuffix}.mAmount2";
		string text7 = $"ISNULL(tCore_Data_FX{sSuffix}.fExchangeRate,1)";
		if (!string.IsNullOrEmpty(strFaTagIds))
		{
			if (strFaTagIds.Contains(","))
			{
				text4 = ((_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 16) <= 0) ? (text4 + $" AND tCore_Data{sSuffix}.iFaTag IN{strFaTagIds}") : (text4 + $" AND tCore_Data{sSuffix}.iFaTag IN({strFaTagIds})"));
			}
			else
			{
				text4 += $" AND tCore_Data{sSuffix}.iFaTag = {strFaTagIds}";
			}
			if (bLocalCurrency)
			{
				text5 = string.Format("ISNULL(tCore_Data_FX{0}.fLocalAmount1, tCore_Data{0}.mAmount1)", sSuffix);
				text6 = string.Format("ISNULL(tCore_Data_FX{0}.fLocalAmount2, tCore_Data{0}.mAmount2)", sSuffix);
				text7 = $"ISNULL(tCore_Data_FX{sSuffix}.fLocalExchangeRate,1)";
			}
		}
		if (bSales)
		{
			int preferenceValue = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.VAT, 26);
			text = string.Format(" SELECT (CASE WHEN iCode = {4} THEN iMainBodyId ELSE iBodyId END)[iBodyId],(CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END)[VATAmt]\r\n\t            ,CASE WHEN (CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END) <> 0 then 100.0/{7}*(CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END) ELSE 0 END[Net], 0 Adv\r\n\t            FROM\r\n\t            (\r\n\t\t            SELECT tCore_Data{0}.iMainBodyId,tCore_Data{0}.iBodyId,CASE WHEN iCode = {4} THEN iCode ELSE iBookNo END [iCode]\r\n\t\t            ,CASE WHEN iCode = {4} THEN {5} ELSE {6} END Amt\r\n\t\t            FROM tCore_Header{0}   \r\n\t\t            JOIN tCore_Data{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId  \r\n\t\t            JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t            WHERE  tCore_Header{0}.iVoucherClass NOT IN(768,6400,2560,1280)\r\n\t\t            AND tCore_Header{0}.iDate BETWEEN {1} AND {2}\r\n\t\t            AND tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n\t\t            AND cCore_Vouchers{0}.bPostVAT = 1\r\n\t\t            AND tCore_Data{0}.iType <> 5 AND 1 = CASE WHEN bInventory = 1 AND (iBookNo = {8} OR iCode = {8}) THEN 0 ELSE 1 END\r\n\t            )Temp\r\n                UNION\r\n                SELECT (CASE WHEN iCode = {4} THEN iMainBodyId ELSE iBodyId END)[iBodyId],(CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END)[VATAmt]\r\n\t            ,CASE WHEN (CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END) <> 0 then 100.0/{7}*(CASE WHEN iCode = {4} THEN Amt ELSE 0.0 END) ELSE 0 END[Net], 1 Adv\r\n\t            FROM\r\n\t            (\r\n\t\t            SELECT tCore_Data{0}.iBodyId iMainBodyId,tCore_Data{0}.iBodyId,CASE WHEN iCode = {4} THEN iCode ELSE iBookNo END [iCode]\r\n\t\t            ,CASE WHEN iCode = {4} THEN {5} ELSE {6} END Amt\r\n\t\t            FROM tCore_Header{0}   \r\n\t\t            JOIN tCore_Data{0}  on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId  \r\n\t\t            JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t            WHERE  tCore_Header{0}.iVoucherClass NOT IN(768,6400,2560,1280)\r\n\t\t            AND tCore_Header{0}.iDate BETWEEN {1} AND {2}\r\n\t\t            AND tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2 \r\n\t\t            AND cCore_Vouchers{0}.bPostVAT = 1\r\n\t\t            AND tCore_Data{0}.iType <> 5 AND bInventory = 1 AND (iBookNo = {8} OR iCode = {8})\r\n\t            )Temp\r\n\t            UNION\r\n\t            SELECT (CASE WHEN iCode = {3} THEN iMainBodyId ELSE iBodyId END)[iBodyId],-(CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END)[VATAmt],case when (CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END) <> 0 then 100.0/{7}*(CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END) else 0 end[Net], 0 Adv\r\n\t            FROM\r\n\t            (\r\n\t\t            SELECT tCore_Data{0}.iMainBodyId,tCore_Data{0}.iBodyId,CASE WHEN iCode = {3} THEN iCode ELSE iBookNo END [iCode]\r\n\t\t            ,CASE WHEN iCode = {3} THEN {5} ELSE {6} END Amt\r\n\t\t            FROM tCore_Header{0}   \r\n\t\t            JOIN tCore_Data{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId  \r\n\t\t            JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t            WHERE  tCore_Header{0}.iVoucherClass NOT IN(3328,1792,5632,6144)\r\n\t\t            AND tCore_Header{0}.iDate BETWEEN {1} AND {2}\r\n\t\t            AND tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n\t\t            AND tCore_Data{0}.iAuthStatus < 2 AND cCore_Vouchers{0}.bPostVAT = 1\r\n\t\t            AND tCore_Data{0}.iType <> 5\r\n\t            ) Temp ", sSuffix, iStartDate, iEndDate, iVATInputAc, iVATOutputAc, text5, text6, dVatPerc, preferenceValue);
		}
		else
		{
			text = string.Format("SELECT (CASE WHEN iCode = {3} THEN iMainBodyId ELSE iBodyId END)[iBodyId],\r\n                -(CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END)[VATAmt],case when (CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END) <> 0 then 100.0/{7}*(CASE WHEN iCode = {3} THEN Amt ELSE 0.0 END) else 0 end[Net], 0 Adv\r\n\t            FROM\r\n\t            (\r\n\t\t            SELECT tCore_Data{0}.iMainBodyId,tCore_Data{0}.iBodyId,CASE WHEN iCode = {3} THEN iCode ELSE iBookNo END [iCode]\r\n\t\t            ,CASE WHEN iCode = {3} THEN {5} ELSE {6} END Amt\r\n\t\t            FROM tCore_Header{0}   \r\n\t\t            JOIN tCore_Data{0} on tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId  \r\n\t\t            JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType\r\n                    LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t\t            WHERE  tCore_Header{0}.iVoucherClass NOT IN(3328,1792,5632,6144)\r\n\t\t            AND tCore_Header{0}.iDate BETWEEN {1} AND {2}\r\n\t\t            AND tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 \r\n\t\t            AND tCore_Data{0}.iAuthStatus < 2 AND cCore_Vouchers{0}.bPostVAT = 1\r\n\t\t            AND tCore_Data{0}.iType <> 5\r\n\t            ) Temp ", sSuffix, iStartDate, iEndDate, iVATInputAc, iVATOutputAc, text5, text6, dVatPerc);
		}
		string text8 = string.Format(",SUM(SIGN(InnerTable.[VATAmt])*CASE WHEN InnerTable.[VATAmt] = 0 THEN 0 ELSE {1} * ABS(tCore_Data{0}.mOriginalAmount) - ABS(InnerTable.[VATAmt]) END) [Taxable]", sSuffix, text7);
		if (!string.IsNullOrEmpty(strScreenData) && !strScreenData.Contains("0[Taxable]"))
		{
			text8 = string.Format(",SUM(SIGN(InnerTable.[VATAmt])*ABS(case when ISNULL(Adv , 0) = 1 then 100/{3}*InnerTable.[VATAmt] else CASE WHEN ROUND(InnerTable.[VATAmt], {1}) = 0 THEN 0 WHEN ScreenData.Taxable IS NOT NULL THEN CASE WHEN SemiAdjustTable.iHeaderId IS NOT NULL THEN case when iVoucherClass in (3328) then SemiAdjustTable.Amt + InnerTable.VATAmt else case when iVoucherClass in (768) then SemiAdjustTable.Amt - InnerTable.VATAmt else 100/{3}*InnerTable.[VATAmt] END END ELSE {2} * ScreenData.Taxable END ELSE {2} * ABS(tCore_Data{0}.mOriginalAmount) - ABS(InnerTable.[VATAmt]) END END))[Taxable]", sSuffix, num, text7, dVatPerc);
		}
		string text9 = string.Format(" \r\n                    LEFT JOIN \r\n                    (\r\n                        {1}\r\n                    )Temp{0} ON Temp{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                    JOIN vmCore_TaxCode WITH(READUNCOMMITTED) ON vmCore_TaxCode.iMasterId = Temp{0}.TaxCode", sSuffix, strTranData);
		if (!string.IsNullOrEmpty(strScreenData) && !strScreenData.Contains("0[Taxable]"))
		{
			text9 += string.Format(" LEFT JOIN\r\n                (\r\n                    {1}\r\n                )ScreenData ON ScreenData.iBodyId = tCore_Data{0}.iBodyId ", sSuffix, strScreenData);
		}
		if (arrReturnLinkId != null && arrReturnLinkId.Length != 0)
		{
			text9 = ((!bReturnLinkExist) ? (text9 + string.Format(" LEFT JOIN\r\n                    (\r\n\t                    SELECT a.iHeaderId, b.iDate FROM \r\n                        (\r\n\t                        SELECT iDate, a.iHeaderId, c.iVoucherType iBaseVoucher, (SELECT value FROM string_split(InvoiceNo, ':') \r\n\t                        ORDER BY (SELECT NULL) OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY) BaseVoucher \r\n                            FROM tCore_Header{0} a \r\n                            JOIN tCore_HeaderData{1}{0} b ON a.iHeaderId = b.iHeaderId\r\n\t                        JOIN cCore_Vouchers{0} c ON c.sAbbr = (SELECT TOP 1 value FROM string_split(InvoiceNo, ':'))\r\n\t                        WHERE iDate <= {2} AND LEN(InvoiceNo) > 4\r\n                        ) a JOIN tCore_Header{0} b ON a.iBaseVoucher = b.iVoucherType AND a.BaseVoucher = b.sVoucherNo\r\n                    )BaseReturn on BaseReturn.iHeaderId = tCore_Header{0}.iHeaderId", sSuffix, arrReturnLinkId[0], iEndDate)) : (text9 + string.Format(" LEFT JOIN\r\n                    (\r\n\t                    select distinct a.iHeaderId, e.iDate\r\n\t                    from tCore_Header{0} a \r\n\t                    join tCore_Data{0} b on a.iHeaderId = b.iHeaderId\r\n\t                    join tCore_Links{0} c on b.iTransactionId = c.iTransactionId and bBase = 0\r\n\t                    join tCore_Data{0} d on c.iRefId = d.iTransactionId and bBase = 0 \r\n\t                    join tCore_Header{0} e on d.iHeaderId = e.iHeaderId\r\n\t                    where iLinkId in ({1})\r\n                    )BaseReturn on BaseReturn.iHeaderId = tCore_Header{0}.iHeaderId", sSuffix, string.Join(",", arrReturnLinkId.Select((int p) =>
			{
				int num2 = p;
				return num2.ToString();
			}).ToArray()))));
		}
		return string.Format("SELECT vmCore_TaxCode.sCode[TaxCode], {14}.sName[Jurisdiction], SUM(InnerTable.[VATAmt])[VAT]\r\n            ,SUM(SIGN(CASE WHEN InnerTable.[VATAmt] = 0 THEN {12} ELSE InnerTable.[VATAmt] END)*ABS(case when ISNULL(Adv , 0) = 1 then (100/{15}*InnerTable.[VATAmt] + InnerTable.[VATAmt]) else CASE WHEN tCore_Indta{0}.mGross IS NULL OR InnerTable.[VATAmt] = 0 THEN {12} ELSE tCore_Indta{0}.mGross END END))[Net]\r\n            {8}{13}\r\n            FROM \r\n            (\r\n                SELECT v_Vat.iBodyId[iMainBodyId], SUM(v_Vat.[Net])[Net], SUM(v_Vat.VATAmt) [VATAmt], SUM(v_Vat.[Net]+v_Vat.VATAmt)[Total value], ISNULL(Adv, 0)Adv\r\n\t            FROM tCore_Data{0}   \r\n                LEFT JOIN\r\n                (\r\n\t                {9}\r\n                )v_Vat ON v_Vat.iBodyId = tCore_Data{0}.iBodyId \r\n                GROUP BY v_Vat.iBodyId, Adv\r\n            )InnerTable \r\n            JOIN tCore_Data{0}  ON tCore_Data{0}.iBodyId = InnerTable.iMainBodyId\r\n            JOIN tCore_Header{0}  ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n            JOIN cCore_Vouchers{0} WITH(READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n            LEFT JOIN tCore_Indta{0}  ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId  \r\n            LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId \r\n            LEFT JOIN tCore_Data_Tags{0} Tag{6} ON Tag{6}.iBodyId = tCore_Data{0}.iBodyId \r\n            LEFT JOIN {14} with(ReadUncommitted) ON {14}.iMasterId = Tag{6}.iTag{6} \r\n            LEFT JOIN\r\n            (\r\n                Select distinct h.iHeaderId, SUM(r.mAmount) Amt from tCore_Header{0} h  \r\n                inner join tCore_Data{0} d on h.iHeaderId= d.iHeaderId\r\n                inner join tCore_Refrn{0} r on d.iBodyId= r.iBodyId\r\n                where ROUND(h.fNet, {10}) <> ROUND(r.mBaseAmount, {10}) and iRefType < 2 and h.iDate between {1} and {2}\r\n                group by h.iHeaderId\r\n            )SemiAdjustTable ON SemiAdjustTable.iHeaderId = tCore_Header{0}.iHeaderId\r\n            {5}\r\n            WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA = 0 \r\n            AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2\r\n            AND cCore_Vouchers{0}.bPostVAT = 1 AND tCore_Header{0}.iDate BETWEEN {1} AND {2} \r\n            AND (tCore_Data{0}.iType = 0 OR ISNULL(Adv, 0) = 1) {7}\r\n            GROUP BY vmCore_TaxCode.sCode,{14}.sName{13}", sSuffix, iStartDate, iEndDate, iVATInputAc, iVATOutputAc, text9, iJuris, text4, text8, text, num, text5, text6, text2, text3, dVatPerc);
	}

	public string[] getGCCPalceOfSupply(int iCompanyId)
	{
		string text = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<string> list = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text = $"SELECT sName FROM vmCore_PlaceOfSupply WHERE iParentId = 12";
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		list = new List<string>();
		while (dataReader.Read())
		{
			list.Add(Convert.ToString(dataReader["sName"]));
		}
		dataReader.Close();
		return list.ToArray();
	}

	public VATSummaryData GetData(int iFromDate, int iToDate, int iFaTagId, int iCompanyId)
	{
		VATSummaryData vATSummaryData = new VATSummaryData();
		string strError = string.Empty;
		string strMasters = null;
		BLVAT bLVAT = new BLVAT();
		List<VATVouchers> list = new List<VATVouchers>();
		List<ClsVAT> list2 = new List<ClsVAT>();
		List<VATSummary> list3 = new List<VATSummary>();
		try
		{
			QueryGenerator queryGenerator = new QueryGenerator();
			queryGenerator.m_iCompId = iCompanyId;
			queryGenerator.Suffix = _focus.company(iCompanyId).suffix;
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			if (iFaTagId > 0)
			{
				strMasters = queryGenerator.GetMasterUnderGroup(iFaTagId, _focus.company(iCompanyId).faTagId, database, iCompanyId);
			}
			list = bLVAT.GetVATVouchers(iCompanyId, ref strError);
			if (list.Count > 0)
			{
				string text = "select iMasterTypeId from cCore_MasterDef where  sMasterName = 'Jurisdiction'";
				int iPlaceOfSupplyTagId = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
				for (int i = 0; i < list.Count; i++)
				{
					int voucherType = list[i].VoucherType;
					int voucherTypeClass = list[i].VoucherTypeClass;
					string salPurType = list[i].SalPurType;
					DataTable vATSummaryData2 = bLVAT.GetVATSummaryData(iCompanyId, voucherType, voucherTypeClass, salPurType, iFromDate, iToDate, strMasters, iPlaceOfSupplyTagId, ref strError);
					if (vATSummaryData2.Rows.Count > 0)
					{
						List<ClsVAT> list4 = ConvertToList<ClsVAT>(vATSummaryData2);
						if (list4.Count > 0)
						{
							list2 = list2.Concat(list4).ToList();
						}
					}
				}
				if (list2.Count > 0)
				{
					List<ClsVAT> list5 = new List<ClsVAT>();
					List<ClsVAT> list6 = new List<ClsVAT>();
					list5 = list2.Where((ClsVAT x) => x.SalPurType == "Sales").ToList();
					list6 = list2.Where((ClsVAT x) => x.SalPurType == "Purchases").ToList();
					if (list5.Count > 0)
					{
						List<VATSummary> list7 = (vATSummaryData.SalesData = bLVAT.GetVATSummaryGridData(list5, 0, ref strError));
						if (list7.Count > 0)
						{
							list3 = list3.Concat(list7).ToList();
						}
					}
					else
					{
						vATSummaryData.SalesData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 0, ref strError).ToList();
					}
					if (list6.Count > 0)
					{
						List<VATSummary> list8 = (vATSummaryData.PurchaseData = bLVAT.GetVATSummaryGridData(list6, 1, ref strError));
						if (list8.Count > 0)
						{
							list3 = list3.Concat(list8).ToList();
						}
					}
					else
					{
						vATSummaryData.PurchaseData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 1, ref strError).ToList();
					}
				}
			}
			if (list3.Count <= 0)
			{
				vATSummaryData.SalesData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 0, ref strError).ToList();
				vATSummaryData.PurchaseData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 1, ref strError).ToList();
				vATSummaryData.Message = "No data found";
			}
		}
		catch (Exception ex)
		{
			vATSummaryData.SalesData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 0, ref strError).ToList();
			vATSummaryData.PurchaseData = bLVAT.GetVATSummaryGridData(new List<ClsVAT>(), 1, ref strError).ToList();
			string strCatchError = (vATSummaryData.Message = ex.Message);
			m_strCatchError = strCatchError;
		}
		return vATSummaryData;
	}

	private List<T> ConvertToList<T>(DataTable dt)
	{
		List<string> columnNames = (from DataColumn c in dt.Columns
			select c.ColumnName).ToList();
		PropertyInfo[] properties = typeof(T).GetProperties();
		return dt.AsEnumerable().Select((DataRow row) =>
		{
			T val = Activator.CreateInstance<T>();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (columnNames.Contains(propertyInfo.Name))
				{
					PropertyInfo property = val.GetType().GetProperty(propertyInfo.Name);
					propertyInfo.SetValue(val, (row[propertyInfo.Name] == DBNull.Value) ? null : Convert.ChangeType(row[propertyInfo.Name], property.PropertyType));
				}
			}
			return val;
		}).ToList();
	}
}
