using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Focus.Common.BL;
using Focus.Common.DataStructs;
using Focus.DataLayer;
using Focus.DatabaseFactory;
using Focus.HRMS.DS;
using Focus.Pronghorn.BL;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Focus.TranSettings.DataStructs;
using Focus.Transactions.BL;
using Focus.Transactions.DataStructs;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Focus.RD.BL;

public class InvoiceLayout : IInvoiceLayout
{
	public CalendarType m_objCalType;

	private Transaction _objTranData;

	private int _iLanguageId;

	private int _iAltLanguageId;

	private int _iCompanyId;

	public string m_sError;

	private bool m_bDeleteLayout = true;

	private int ERRORNO = -1;

	private List<IdValuePair> m_arrVoucherTypesValue;

	private bool m_bCallingFromExternal;

	private string m_strSuffix;

	private IdNamePair[] arrFields = new IdNamePair[152]
	{
		new IdNamePair
		{
			ID = 1,
			Name = "Document No",
			Tag = "tCore_Header.sVoucherNo"
		},
		new IdNamePair
		{
			ID = 175,
			Name = "Document no with abbr",
			Tag = "tCore_Header.sVoucherNo"
		},
		new IdNamePair
		{
			ID = 187,
			Name = "Document no (Transaction)",
			Tag = "tCore_Header.sVoucherNo"
		},
		new IdNamePair
		{
			ID = 2,
			Name = "Date",
			Tag = "tCore_Header.iDate"
		},
		new IdNamePair
		{
			ID = 3,
			Name = "Code",
			Tag = "tCore_Data.iCode"
		},
		new IdNamePair
		{
			ID = 4,
			Name = "BookNo",
			Tag = "tCore_Data.iBookNo"
		},
		new IdNamePair
		{
			ID = 5,
			Name = "Invoice No",
			Tag = "tCore_Data.iInvTag"
		},
		new IdNamePair
		{
			ID = 6,
			Name = "Due Date",
			Tag = "tCore_Data.iDueDate"
		},
		new IdNamePair
		{
			ID = 7,
			Name = "Body Due Date",
			Tag = "tCore_Data.iDueDate"
		},
		new IdNamePair
		{
			ID = 8,
			Name = "Production No",
			Tag = "tMrp_ProdOrder.sProdOrderNo"
		},
		new IdNamePair
		{
			ID = 194,
			Name = "BOM Name",
			Tag = "vCore_BOMVariant.BOMName"
		},
		new IdNamePair
		{
			ID = 9,
			Name = "Bom Size",
			Tag = "tCore_Header_2.fProdSize"
		},
		new IdNamePair
		{
			ID = 10,
			Name = "Header Currency",
			Tag = "mCore_Currency.sCode"
		},
		new IdNamePair
		{
			ID = 11,
			Name = "Exchange Rate",
			Tag = "tCore_Data_FX.fExchangeRate"
		},
		new IdNamePair
		{
			ID = 12,
			Name = "Body Code",
			Tag = "tCore_Data.iCode"
		},
		new IdNamePair
		{
			ID = 13,
			Name = "Body Batch No",
			Tag = "tCore_Batch.sBatchNo"
		},
		new IdNamePair
		{
			ID = 108,
			Name = "Body rate",
			Tag = "tCore_Batch.fRate"
		},
		new IdNamePair
		{
			ID = 14,
			Name = "Body Currency",
			Tag = "mCore_Currency.sCode"
		},
		new IdNamePair
		{
			ID = 15,
			Name = "Body Exchange Rate",
			Tag = "tCore_Data_FX.fExchangeRate"
		},
		new IdNamePair
		{
			ID = 16,
			Name = "Body Amount",
			Tag = "tCore_Data.mOriginalAmount"
		},
		new IdNamePair
		{
			ID = 18,
			Name = "Debit",
			Tag = "vtCore_DrCr.Dr"
		},
		new IdNamePair
		{
			ID = 19,
			Name = "Credit",
			Tag = "vtCore_DrCr.Cr"
		},
		new IdNamePair
		{
			ID = 20,
			Name = "Update Stock",
			Tag = "tCore_Header.bUpdateStocks"
		},
		new IdNamePair
		{
			ID = 21,
			Name = "Raise Cash Receipt",
			Tag = "tCore_Header.bPostCashEntry"
		},
		new IdNamePair
		{
			ID = 22,
			Name = "Reserve Quantity",
			Tag = "vCore_GetReservationDetails.ReservedQty"
		},
		new IdNamePair
		{
			ID = 89,
			Name = "Release Quantity",
			Tag = "vCore_GetReservationDetails.ReleasedQty"
		},
		new IdNamePair
		{
			ID = 115,
			Name = "Balance Reserve Quantity",
			Tag = "vCore_GetReservationDetails.BalanceQty"
		},
		new IdNamePair
		{
			ID = 146,
			Name = "Reservation Batch no",
			Tag = "vCore_GetReservationDetails.BatchNo"
		},
		new IdNamePair
		{
			ID = 197,
			Name = "Reservation Batch no",
			Tag = "vCore_GetReservationDetails.RMANo"
		},
		new IdNamePair
		{
			ID = 23,
			Name = "Item",
			Tag = "tCore_Indta.iProduct"
		},
		new IdNamePair
		{
			ID = 24,
			Name = "Unit",
			Tag = "tCore_Indta.iUnit"
		},
		new IdNamePair
		{
			ID = 25,
			Name = "Receipt/Issue",
			Tag = "tCore_Indta.iUnit"
		},
		new IdNamePair
		{
			ID = 26,
			Name = "Quantity",
			Tag = "tCore_Indta.fQuantity"
		},
		new IdNamePair
		{
			ID = 59,
			Name = "Quantity in base unit",
			Tag = "tCore_Indta.fQuantityInBase"
		},
		new IdNamePair
		{
			ID = 27,
			Name = "Rate",
			Tag = "tCore_Indta.mRate"
		},
		new IdNamePair
		{
			ID = 28,
			Name = "Gross",
			Tag = "tCore_Indta.mGross"
		},
		new IdNamePair
		{
			ID = 29,
			Name = "LC No",
			Tag = "tCore_Header_2.iLCNo"
		},
		new IdNamePair
		{
			ID = 30,
			Name = "Quantity breakup",
			Tag = ""
		},
		new IdNamePair
		{
			ID = 31,
			Name = "Issues/Receipts",
			Tag = "tCore_Indta.fQuantity"
		},
		new IdNamePair
		{
			ID = 32,
			Name = "Paymentterms",
			Tag = "tCore_Header_2.iPaymentTerms"
		},
		new IdNamePair
		{
			ID = 33,
			Name = "Header Tag",
			Tag = ""
		},
		new IdNamePair
		{
			ID = 34,
			Name = "Body Tag",
			Tag = ""
		},
		new IdNamePair
		{
			ID = 36,
			Name = "Mfg Date",
			Tag = "tCore_Batch.iMfDate"
		},
		new IdNamePair
		{
			ID = 37,
			Name = "Expiry Date",
			Tag = "tCore_Batch.iExpiryDate"
		},
		new IdNamePair
		{
			ID = 38,
			Name = "RMA No",
			Tag = "tCore_Rma.sRmaNo"
		},
		new IdNamePair
		{
			ID = 85,
			Name = "Bins",
			Tag = "vtCore_Bins.Bins"
		},
		new IdNamePair
		{
			ID = 130,
			Name = "Skid",
			Tag = "vtCore_Bins.sSkidNo"
		},
		new IdNamePair
		{
			ID = 195,
			Name = "Carton number",
			Tag = "vtCore_Bins.sCarton"
		},
		new IdNamePair
		{
			ID = 39,
			Name = "Body Book No",
			Tag = "tCore_Data.iBookNo"
		},
		new IdNamePair
		{
			ID = 40,
			Name = "Update FA",
			Tag = "tCore_Data.bUpdateFA"
		},
		new IdNamePair
		{
			ID = 41,
			Name = "Alternate Qty",
			Tag = "tCore_Indta.fAlternateQty"
		},
		new IdNamePair
		{
			ID = 42,
			Name = "Brs",
			Tag = "tCore_Data.bBrs"
		},
		new IdNamePair
		{
			ID = 43,
			Name = "Cancelled",
			Tag = "tCore_Header.bCancelled"
		},
		new IdNamePair
		{
			ID = 186,
			Name = "PDC Status",
			Tag = "tCore_Data.bPdc"
		},
		new IdNamePair
		{
			ID = 44,
			Name = "Checked",
			Tag = "tCore_Data.bChecked"
		},
		new IdNamePair
		{
			ID = 45,
			Name = "COGS",
			Tag = "tCore_Indta.mCogsValue"
		},
		new IdNamePair
		{
			ID = 46,
			Name = "Currrency Id",
			Tag = "tCore_Data_FX.iCurrencyId"
		},
		new IdNamePair
		{
			ID = 47,
			Name = "Entry Id",
			Tag = "tCore_Header.iEntryId"
		},
		new IdNamePair
		{
			ID = 48,
			Name = "FA Tag",
			Tag = "tCore_Data.iFaTag"
		},
		new IdNamePair
		{
			ID = 49,
			Name = "FxAmount1",
			Tag = "tCore_Data_FX.mFXAmount1"
		},
		new IdNamePair
		{
			ID = 156,
			Name = "Local footer amount",
			Tag = "tCore_Data_FX.mFXAmount3"
		},
		new IdNamePair
		{
			ID = 51,
			Name = "Inv Tag",
			Tag = "tCore_Data.iInvTag"
		},
		new IdNamePair
		{
			ID = 52,
			Name = "IsSetName",
			Tag = "tCore_Indta.bIsSetName"
		},
		new IdNamePair
		{
			ID = 53,
			Name = "Original Amount",
			Tag = "tCore_Data_FX.mFxAmount2"
		},
		new IdNamePair
		{
			ID = 54,
			Name = "Original Gross",
			Tag = "tCore_Data_FX.mOriginalGross"
		},
		new IdNamePair
		{
			ID = 55,
			Name = "Original Rate",
			Tag = "tCore_Data.mOriginalAmount"
		},
		new IdNamePair
		{
			ID = 50,
			Name = "Transaction amount",
			Tag = "tCore_Data_FX.mFxAmount2"
		},
		new IdNamePair
		{
			ID = 94,
			Name = "Local Gross ",
			Tag = "tCore_Data_FX.fLocalGross"
		},
		new IdNamePair
		{
			ID = 93,
			Name = "Local Net",
			Tag = "tCore_Data_FX.fLocalAmount2"
		},
		new IdNamePair
		{
			ID = 155,
			Name = "Local footer value",
			Tag = "tCore_Data_FX.mLocalAmount3"
		},
		new IdNamePair
		{
			ID = 145,
			Name = "Local Exchange Rate",
			Tag = "tCore_Data_FX.fLocalExchangeRate"
		},
		new IdNamePair
		{
			ID = 56,
			Name = "PostCashEntry",
			Tag = "tCore_Header.bPostCashEntry"
		},
		new IdNamePair
		{
			ID = 57,
			Name = "QC Status",
			Tag = "tCore_Indta.iQCStatus"
		},
		new IdNamePair
		{
			ID = 58,
			Name = "Qc Pending",
			Tag = "tCore_Indta.bQCPending"
		},
		new IdNamePair
		{
			ID = 59,
			Name = "Quantity In Base",
			Tag = "tCore_Indta.fQuantityInBase"
		},
		new IdNamePair
		{
			ID = 60,
			Name = "Special Meaning",
			Tag = "tCore_Data.bSpecialMeaning"
		},
		new IdNamePair
		{
			ID = 61,
			Name = "Suspended",
			Tag = "tCore_Header.bSuspended"
		},
		new IdNamePair
		{
			ID = 62,
			Name = "Suspended Base Saved",
			Tag = "tCore_Data.bSuspendBaseSaved"
		},
		new IdNamePair
		{
			ID = 63,
			Name = "Suspended Link Saved",
			Tag = "tCore_Data.bSuspendLinkSaved"
		},
		new IdNamePair
		{
			ID = 64,
			Name = "Suspended Reservation",
			Tag = "tCore_Data.bSuspendReservation"
		},
		new IdNamePair
		{
			ID = 65,
			Name = "Suspended Update FA",
			Tag = "tCore_Data.bSuspendUpdateFA"
		},
		new IdNamePair
		{
			ID = 66,
			Name = "Suspended Update Stocks",
			Tag = "tCore_Data.bSuspendUpdateStocks"
		},
		new IdNamePair
		{
			ID = 67,
			Name = "Stock Value",
			Tag = "tCore_Indta.mStockValue"
		},
		new IdNamePair
		{
			ID = 68,
			Name = "Tag Id",
			Tag = ""
		},
		new IdNamePair
		{
			ID = 69,
			Name = "Tag Value",
			Tag = ""
		},
		new IdNamePair
		{
			ID = 70,
			Name = "TDS Cert Prepared",
			Tag = "tCore_Header.bTDSCertPrepared"
		},
		new IdNamePair
		{
			ID = 71,
			Name = "TDS Paid",
			Tag = "tCore_Header.bTDSPaid"
		},
		new IdNamePair
		{
			ID = 72,
			Name = "Type",
			Tag = "tCore_Data.iType"
		},
		new IdNamePair
		{
			ID = 73,
			Name = "User Id",
			Tag = "mSec_Users.sLoginName"
		},
		new IdNamePair
		{
			ID = 74,
			Name = "Amount1",
			Tag = "tCore_Data.mAmount1"
		},
		new IdNamePair
		{
			ID = 75,
			Name = "Net Amount",
			Tag = "tCore_Data.mAmount2"
		},
		new IdNamePair
		{
			ID = 171,
			Name = "Voucher Amount",
			Tag = "tCore_Header.fOrigNet"
		},
		new IdNamePair
		{
			ID = 154,
			Name = "Footer Amount",
			Tag = "tCore_Data.mAmount3"
		},
		new IdNamePair
		{
			ID = 76,
			Name = "Serial No",
			Tag = "tCore_Data.iSerialNo"
		},
		new IdNamePair
		{
			ID = 77,
			Name = "Batch Quantity",
			Tag = "tCore_Batch.fQuantity"
		},
		new IdNamePair
		{
			ID = 78,
			Name = "Modified By",
			Tag = "mSec_Users.sLoginName"
		},
		new IdNamePair
		{
			ID = 79,
			Name = "Created Date",
			Tag = "tCore_Header.iCreatedDate"
		},
		new IdNamePair
		{
			ID = 80,
			Name = "Created Time",
			Tag = "tCore_Header.iCreatedTime"
		},
		new IdNamePair
		{
			ID = 81,
			Name = "Modified Date",
			Tag = "tCore_Header.iModifiedDate"
		},
		new IdNamePair
		{
			ID = 82,
			Name = "Modified Time",
			Tag = "tCore_Header.iModifiedTime"
		},
		new IdNamePair
		{
			ID = 101,
			Name = "Redeemed point",
			Tag = "vPos_MemberPoints.RedPoints"
		},
		new IdNamePair
		{
			ID = 102,
			Name = "Opening balance",
			Tag = "vPos_MemberPoints.OpeningBalance"
		},
		new IdNamePair
		{
			ID = 103,
			Name = "Point earned",
			Tag = "vPos_MemberPoints.Points"
		},
		new IdNamePair
		{
			ID = 104,
			Name = "Expiry date",
			Tag = "vPos_MemberPoints.iExpiryDate"
		},
		new IdNamePair
		{
			ID = 105,
			Name = "Point status",
			Tag = "vPos_MemberPoints.PointsStatus"
		},
		new IdNamePair
		{
			ID = 106,
			Name = "Points reversal",
			Tag = "vPos_MemberPoints.Reversal"
		},
		new IdNamePair
		{
			ID = 107,
			Name = "Points balance",
			Tag = "mPos_Member_PointsAvl.fPoints"
		},
		new IdNamePair
		{
			ID = 114,
			Name = "Voucher type",
			Tag = "tCore_Header.iVoucherType"
		},
		new IdNamePair
		{
			ID = 201,
			Name = "Voucher class",
			Tag = "tCore_Header.iVoucherClass"
		},
		new IdNamePair
		{
			ID = 157,
			Name = "Voucher name",
			Tag = "cCore_Vouchers.sName"
		},
		new IdNamePair
		{
			ID = 158,
			Name = "Voucher abbreviation",
			Tag = "cCore_Vouchers.sAbbr"
		},
		new IdNamePair
		{
			ID = 172,
			Name = "Voucher alias",
			Tag = "cCore_VouchersLanguage.sVoucherName"
		},
		new IdNamePair(118, "Scheme name", "vCore_SchemeApplied.SchemeName"),
		new IdNamePair(119, "Scheme type", "vCore_SchemeApplied.SchemeType"),
		new IdNamePair(120, "Scheme promotion type", "vCore_SchemeApplied.PromotionType"),
		new IdNamePair(121, "Scheme item", "vCore_SchemeApplied.ItemOrDVName"),
		new IdNamePair(122, "Scheme free quantity", "vCore_SchemeApplied.FreeQty"),
		new IdNamePair(123, "Scheme discount", "vCore_SchemeApplied.Discount"),
		new IdNamePair(124, "Scheme implementaion type", "vCore_SchemeApplied.ImplementationType"),
		new IdNamePair(125, "Scheme payment type", "vCore_SchemeApplied.PaymentType"),
		new IdNamePair(126, "Scheme bank/card name", "vCore_SchemeApplied.BankCardName"),
		new IdNamePair(83, "Reference Details", "vtCore_Refrn.sDetails"),
		new IdNamePair(112, "Reference Details", "vtCore_Refrn.sDetails"),
		new IdNamePair(133, "Reference Details", "vtCore_Refrn.sDetails"),
		new IdNamePair(134, "Reference Number", "vtCore_Refrn.sNumber"),
		new IdNamePair(135, "Reference Date", "vtCore_Refrn.[Reference Date]"),
		new IdNamePair(136, "Reference Amount", "vtCore_Refrn.mAmount"),
		new IdNamePair(137, "Reference Bill Number", "vtCore_Refrn.sBillNo"),
		new IdNamePair(138, "Reference Doc. Number", "vtCore_Refrn.sVoucherNo"),
		new IdNamePair(139, "Reference Due-Date", "vtCore_Refrn.iDueDate"),
		new IdNamePair(193, "Reference Due-Date Details", "vtCore_Refrn.iDueDate"),
		new IdNamePair(140, "Reference Account", "vtCore_Refrn.sName"),
		new IdNamePair(142, "Reference Local Amount", "vtCore_Refrn.mLocalAmount"),
		new IdNamePair(141, "Reference Base Amount", "vtCore_Refrn.mBaseAmount"),
		new IdNamePair(143, "Base Link doc. number", "vtCore_LinksData.sLinkVoucherNo"),
		new IdNamePair(144, "Base Link doc. date", "vtCore_LinksData.iLinkDate"),
		new IdNamePair(163, "Base Link status", "vtCore_LinksData.sLinkStatus"),
		new IdNamePair(173, "Link status", "vtCore_Links.sLinkStatus"),
		new IdNamePair(117, "Revision number", "vCore_Revision.sRevisionNo"),
		new IdNamePair(95, "Authorization status", "tCore_Header.iAuth"),
		new IdNamePair(153, "Authorize by", "AuthUser.sUserName"),
		new IdNamePair(161, "Authorize remarks", "tCore_TransAuthUsers.sRemarks"),
		new IdNamePair(164, "Authorize date", "tCore_TransAuthUsers.iAuthDate"),
		new IdNamePair(116, "Print count", "tCore_Header.iPrintCount"),
		new IdNamePair(166, "Email count", "tCore_Header.iEmailCount"),
		new IdNamePair(174, "Cheque status", "tCore_Header.bChequeReturn"),
		new IdNamePair
		{
			ID = 168,
			Name = "Opening stock quantity",
			Tag = "tCore_Indta.fQuantityInBase"
		},
		new IdNamePair
		{
			ID = 169,
			Name = "Opening stock value",
			Tag = "tCore_Indta.mStockValue"
		},
		new IdNamePair
		{
			ID = 170,
			Name = "Product image",
			Tag = "vrCore_Product.pImage"
		},
		new IdNamePair
		{
			ID = 198,
			Name = "Purchase Prediction",
			Tag = "tAifa_Inward.fPredict"
		},
		new IdNamePair
		{
			ID = 199,
			Name = "Sales Prediction",
			Tag = "tAifa_Outward.fPredict"
		},
		new IdNamePair
		{
			ID = 200,
			Name = "Stock Prediction",
			Tag = "tAifa_Stock.fPredict"
		}
	};

	private bool m_bLyteVersion;

	public string CurrentSuffix
	{
		get
		{
			return m_strSuffix;
		}
		set
		{
			m_strSuffix = value;
		}
	}

	public List<IdValuePair> VouchersValue
	{
		get
		{
			return m_arrVoucherTypesValue;
		}
		set
		{
			m_arrVoucherTypesValue = value;
		}
	}

	public bool LyteVersion
	{
		get
		{
			return m_bLyteVersion;
		}
		set
		{
			m_bLyteVersion = value;
		}
	}

	public string SavePOSBillFormat(POSPrintFormat objData, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		DbTransaction dbTransaction = null;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		string empty = string.Empty;
		DbConnection dbConnection;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				dbTransaction = dbConnection.BeginTransaction();
				dbCommand = database.DbProviderFactory.CreateCommand();
				dbCommand.Connection = dbConnection;
				dbCommand.CommandType = CommandType.Text;
				dbCommand.Transaction = dbTransaction;
				if (objData != null)
				{
					if (objData.PrintFormatId > 0)
					{
						empty = $"UPDATE cPos_BillPrintFormat SET sHeader='{objData.Header}', sFooter='{objData.Footer}',bPrintInvoiceNumberBarcodeAtLast={(objData.PrintInvoiceNumberBarcodeAtLast ? 1 : 0)},sTemplate='{objData.Template}',bImageLogo={(objData.PrintImageLogoAtStart ? 1 : 0)}\r\n                                    WHERE iPrintFormatId={objData.PrintFormatId}";
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
						database.ExecuteNonQuery(dbCommand, dbTransaction);
						stringBuilder.Append(string.Format(" DELETE cPos_BillPrintFormatBody WHERE iPrintFormatId={0};\r\n                            DELETE cPos_BillPrintFormatLogo WHERE iPrintFormatId = {0};", objData.PrintFormatId));
					}
					else
					{
						empty = $"INSERT INTO cPos_BillPrintFormat (sHeader,sFooter,bPrintInvoiceNumberBarcodeAtLast,sTemplate,bImageLogo) \r\n                                    VALUES ('{objData.Header}','{objData.Footer}',{(objData.PrintInvoiceNumberBarcodeAtLast ? 1 : 0)},'{objData.Template}',{(objData.PrintImageLogoAtStart ? 1 : 0)});SELECT @@IDENTITY";
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
						objData.PrintFormatId = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
					}
					for (num = 0; num < objData.POSPrintFormatBody.Length; num++)
					{
						stringBuilder.Append($" INSERT INTO cPos_BillPrintFormatBody (iPrintFormatId,iLineNumber,iFieldSequence,iFieldId,iFieldCharWidth,sFieldAlias,bWrapText,iDecimalInColumn, iSubParentId, sCaption) VALUES ({objData.PrintFormatId},{objData.POSPrintFormatBody[num].LineNumber},{objData.POSPrintFormatBody[num].FieldSequence},{objData.POSPrintFormatBody[num].FieldId},{objData.POSPrintFormatBody[num].FieldCharWidth}, '{objData.POSPrintFormatBody[num].FieldAlias}',{(objData.POSPrintFormatBody[num].IsTextWrap ? 1 : 0)},{objData.POSPrintFormatBody[num].DecimalInColumn},{objData.POSPrintFormatBody[num].SubParentId},'{objData.POSPrintFormatBody[num].Caption}')");
					}
					if (objData.PrintImageLogoAtStart)
					{
						stringBuilder.Append($"INSERT INTO cPos_BillPrintFormatLogo (iPrintFormatId,byImagesSource) \r\n                                VALUES ({objData.PrintFormatId},@Logo)");
					}
					if (stringBuilder.Length > 0)
					{
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
						if (objData.Image != null && objData.Image.Length != 0)
						{
							database.AddInParameter(dbCommand, "@Logo", DbType.Binary, objData.Image);
						}
						database.ExecuteNonQuery(dbCommand, dbTransaction);
					}
					dbTransaction.Commit();
				}
			}
			catch (Exception ex)
			{
				dbTransaction.Rollback();
				return ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
			return string.Empty;
		}
	}

	public POSPrintFormat LoadPOSBillFormat(int iFormatId, int iCompId)
	{
		POSPrintFormat pOSPrintFormat = new POSPrintFormat();
		List<POSPrintFormatBody> list = new List<POSPrintFormatBody>();
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				empty = $"SELECT iPrintFormatId,sHeader,sFooter,bPrintInvoiceNumberBarcodeAtLast,sTemplate,bImageLogo\r\n                    FROM cPos_BillPrintFormat WHERE iPrintFormatId={iFormatId}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				if (dataReader.Read())
				{
					pOSPrintFormat.PrintFormatId = Convert.ToInt32(dataReader["iPrintFormatId"]);
					pOSPrintFormat.Header = Convert.ToString(dataReader["sHeader"]);
					pOSPrintFormat.Footer = Convert.ToString(dataReader["sFooter"]);
					pOSPrintFormat.Template = Convert.ToString(dataReader["sTemplate"]);
					pOSPrintFormat.PrintInvoiceNumberBarcodeAtLast = Convert.ToBoolean(dataReader["bPrintInvoiceNumberBarcodeAtLast"]);
					pOSPrintFormat.PrintImageLogoAtStart = Convert.ToBoolean(dataReader["bImageLogo"]);
				}
				dataReader.Close();
				if (pOSPrintFormat.PrintImageLogoAtStart)
				{
					empty = $"SELECT byImagesSource FROM cPos_BillPrintFormatLogo WHERE iPrintFormatId={iFormatId}";
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					dataReader = database.ExecuteReader(dbCommand);
					if (dataReader.Read())
					{
						pOSPrintFormat.Image = ((dataReader["byImagesSource"] == DBNull.Value) ? null : ((byte[])dataReader["byImagesSource"]));
					}
					dataReader.Close();
				}
				empty = $"SELECT iPrintFormatDetailId,iLineNumber,iFieldSequence,iFieldId,iFieldCharWidth,sFieldAlias,bWrapText,iDecimalInColumn, iSubParentId, sCaption\r\n                        FROM cPos_BillPrintFormatBody WHERE iPrintFormatId  = {pOSPrintFormat.PrintFormatId}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					POSPrintFormatBody pOSPrintFormatBody = new POSPrintFormatBody();
					pOSPrintFormatBody.PrintFormatDetailId = Convert.ToInt32(dataReader["iPrintFormatDetailId"]);
					pOSPrintFormatBody.LineNumber = Convert.ToByte(dataReader["iLineNumber"]);
					pOSPrintFormatBody.FieldSequence = Convert.ToByte(dataReader["iFieldSequence"]);
					pOSPrintFormatBody.FieldId = Convert.ToInt32(dataReader["iFieldId"]);
					pOSPrintFormatBody.FieldCharWidth = Convert.ToInt16(dataReader["iFieldCharWidth"]);
					pOSPrintFormatBody.FieldAlias = Convert.ToString(dataReader["sFieldAlias"]);
					pOSPrintFormatBody.IsTextWrap = Convert.ToBoolean(dataReader["bWrapText"]);
					pOSPrintFormatBody.DecimalInColumn = Convert.ToByte(dataReader["iDecimalInColumn"]);
					pOSPrintFormatBody.SubParentId = Convert.ToInt32(dataReader["iSubParentId"]);
					pOSPrintFormatBody.Caption = Convert.ToString(dataReader["sCaption"]);
					list.Add(pOSPrintFormatBody);
				}
				dataReader.Close();
				pOSPrintFormat.POSPrintFormatBody = list.ToArray();
			}
			catch (Exception ex)
			{
				empty = ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return pOSPrintFormat;
	}

	public string DeletePOSBillFormat(int iTemplateId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		DbTransaction dbTransaction = null;
		string empty = string.Empty;
		DbConnection dbConnection;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				dbTransaction = dbConnection.BeginTransaction();
				dbCommand = database.DbProviderFactory.CreateCommand();
				dbCommand.Connection = dbConnection;
				dbCommand.CommandType = CommandType.Text;
				dbCommand.Transaction = dbTransaction;
				empty = string.Format("\r\n                    DELETE cPos_BillPrintFormatBody WHERE iPrintFormatId = {0};;\r\n                    DELETE cPos_BillPrintFormatLogo WHERE iPrintFormatId = {0};\r\n                    DELETE cPos_BillPrintFormat WHERE iPrintFormatId = {0}", iTemplateId);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				database.ExecuteNonQuery(dbCommand, dbTransaction);
				dbTransaction.Commit();
			}
			catch (Exception ex)
			{
				dbTransaction.Rollback();
				return ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return string.Empty;
	}

	public InventorySalesInfo[] GetSalesInfo(int iDate, int[] arrOutletIds, int iCompId)
	{
		List<InventorySalesInfo> list = new List<InventorySalesInfo>();
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				empty = string.Format("SELECT -1[OutletId], 'Total Sales - {3}'[OutletName], \r\n                    ISNULL(SUM(tCore_Data{0}.mAmount2*-1), 0)[Total Sales Amount],\r\n                    ISNULL(SUM(tCore_Indta{0}.fQuantity*-1), 0)[Total Sales Quantity],\r\n                    ISNULL(Count(tCore_Data{0}.iHeaderId), 0)[Total Transaction]\r\n                    FROM tCore_Header{0} \r\n                    JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                    JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId  \r\n                    WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1\r\n                    AND tCore_Header{0}.bSuspended = 0 AND tCore_Header{0}.iDate = {1}\r\n                    AND tCore_Header{0}.iVoucherClass = {2}   \r\n                    UNION\r\n                    SELECT mPos_Outlet.iMasterId[OutletId], mPos_Outlet.sName[OutletName], \r\n                    ISNULL(SUM(tCore_Data{0}.mAmount2*-1), 0)[Total Sales Amount],\r\n                    ISNULL(SUM(tCore_Indta{0}.fQuantity*-1), 0)[Total Sales Quantity],\r\n                    ISNULL(Count(tCore_Data{0}.iHeaderId), 0)[Total Transaction]\r\n                    FROM tCore_Header{0} \r\n                    JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType=tCore_Header{0}.iVoucherType \r\n                    JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId  \r\n                    LEFT JOIN mPos_Outlet ON mPos_Outlet.iMasterId = tCore_Data{0}.iInvTag  \r\n                    WHERE tCore_Data{0}.bUpdateFA = 1 AND tCore_Data{0}.bSuspendUpdateFA <> 1\r\n                    AND tCore_Header{0}.bSuspended = 0 AND tCore_Header{0}.iDate = {1}\r\n                    AND tCore_Header{0}.iVoucherClass = {2}   \r\n                    GROUP BY mPos_Outlet.iMasterId,mPos_Outlet.sName", FConvert.GetSuffix(iCompId), iDate, 3328, new Date(iDate, m_objCalType).ToString());
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					InventorySalesInfo inventorySalesInfo = new InventorySalesInfo();
					List<IdValuePair> list2 = new List<IdValuePair>();
					IdValuePair idValuePair = null;
					inventorySalesInfo.Id = Convert.ToInt32(dataReader["OutletId"]);
					inventorySalesInfo.Name = Convert.ToString(dataReader["OutletName"]);
					inventorySalesInfo.IsHide = false;
					idValuePair = new IdValuePair();
					idValuePair.ID = 1;
					idValuePair.Value = Convert.ToDouble(dataReader["Total Sales Amount"]);
					list2.Add(idValuePair);
					idValuePair = new IdValuePair();
					idValuePair.ID = 9;
					idValuePair.Value = Convert.ToDouble(dataReader["Total Sales Quantity"]);
					list2.Add(idValuePair);
					idValuePair = new IdValuePair();
					idValuePair.ID = 8;
					idValuePair.Value = Convert.ToInt32(dataReader["Total Transaction"]);
					list2.Add(idValuePair);
					inventorySalesInfo.Variables = list2.ToArray();
					list.Add(inventorySalesInfo);
				}
				dataReader.Close();
			}
			catch (Exception ex)
			{
				empty = ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return list.ToArray();
	}

	public IdNamePair[] GetVatFormsData(int iStartDate, int iEndDate, int iInventoryTag, int iCompId)
	{
		int num = 0;
		int num2 = -1;
		double num3 = 0.0;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		IdValuePair idValuePair = null;
		List<IdValuePair> list = new List<IdValuePair>();
		string empty = string.Empty;
		List<IdNamePair> list2 = new List<IdNamePair>();
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		List<IdNamePair> list3 = new List<IdNamePair>();
		List<IdNamePair> list4 = new List<IdNamePair>();
		List<IdNamePair> list5 = new List<IdNamePair>();
		IdNamePair idNamePair = null;
		string suffix = _focus.company(iCompId).suffix;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				num4 = _focus.company(iCompId).faTagId;
				num5 = _focus.company(iCompId).invTagId;
				num6 = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 14);
				if (iInventoryTag > 0)
				{
					if (num6 == num4)
					{
						text3 = $" AND tCore_Data{suffix}.iFaTag = {iInventoryTag}";
						text = "iFaTag";
					}
					else if (num6 == num5)
					{
						text3 = $" AND tCore_Data{suffix}.iInvTag = {iInventoryTag}";
						text = "iInvTag";
					}
					else
					{
						text2 = string.Format("JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId", suffix);
						text = $"iTag{iInventoryTag}";
					}
				}
				empty = string.Format("SELECT cCore_Vouchers{0}.iVoucherType,iColMap\r\n                    FROM cCore_VoucherScreenFields{0} \r\n                    JOIN cCore_Fields on cCore_VoucherScreenFields{0}.iUniqueId = cCore_Fields.iFieldId\r\n                    JOIN cCore_Vouchers{0} ON cCore_VoucherScreenFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType\r\n                    WHERE sCaption='VAT' AND bPostVAT = 1 ORDER BY cCore_Vouchers{0}.iVoucherType", suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					idValuePair = new IdValuePair(Convert.ToInt32(dataReader["iVoucherType"]), dataReader["iColMap"]);
					list.Add(idValuePair);
				}
				dataReader.Close();
				empty = $"SELECT iFieldId, sValue FROM cCore_PreferenceText{suffix}\r\n                                                WHERE iCategory = {25} and iFieldId >= {25}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					idNamePair = new IdNamePair();
					idNamePair.ID = Convert.ToInt32(dataReader["iFieldId"]);
					idNamePair.Name = Convert.ToString(dataReader["sValue"]);
					list5.Add(idNamePair);
				}
				dataReader.Close();
				int num7 = 151;
				int num8 = 251;
				IdNamePair idNamePair2 = null;
				if (list5 != null && list5.Count > 0)
				{
					for (num = 0; num < list5.Count; num++)
					{
						if (num % 2 == 0)
						{
							if (list5[num].Name.Length != 0)
							{
								idNamePair2 = new IdNamePair();
								idNamePair2.ID = num7;
								idNamePair2.Name = list5[num].Name;
								list3.Add(idNamePair2);
							}
							num7++;
						}
						else
						{
							if (list5[num].Name.Length != 0)
							{
								idNamePair2 = new IdNamePair();
								idNamePair2.ID = num8;
								idNamePair2.Name = list5[num].Name;
								list4.Add(idNamePair2);
							}
							num8++;
						}
					}
				}
				for (num = 0; num < list.Count; num++)
				{
					num2 = list[num].ID;
					empty = string.Format("SELECT iBookNo[Account], {8}, sum(mAmount2)[Amount2], ISNULL(sum(mInput{4}), 0)[VAT Input], ISNULL(sum(mVal{4}), 0)[VAT Output], sum(mGross)[Gross]\r\n                        FROM tCore_Header{0}\r\n                        JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n                        JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId\r\n                        JOIN tCore_Indta{0} ON tCore_Indta{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                        JOIN tCore_IndtaBodyScreenData{0} ON tCore_IndtaBodyScreenData{0}.iBodyId = tCore_Data{0}.iBodyId\r\n                        {6}\r\n                        WHERE cCore_Vouchers{0}.bPostVAT = 1 and bSuspended = 0\r\n                        AND tCore_Header{0}.iVoucherType = {5} {7} \r\n                        GROUP BY iBookNo, {8}", suffix, iStartDate, iEndDate, iInventoryTag, list[num].Value, num2, text2, text3, text);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					dataReader = database.ExecuteReader(dbCommand);
					while (dataReader.Read())
					{
						num3 = Convert.ToDouble(dataReader["Gross"]);
						Convert.ToDouble(dataReader["VAT Input"]);
						Convert.ToDouble(dataReader["VAT Output"]);
						if (IsSalesVoucher(num2))
						{
							if (num2 == 1792)
							{
								num3 *= -1.0;
							}
							idNamePair = new IdNamePair();
							idNamePair.ID = Convert.ToInt32(VATVariable.TotalSales);
							idNamePair.Tag = num3;
							list2.Add(idNamePair);
							for (num = 0; num < list3.Count; num++)
							{
								idNamePair = new IdNamePair();
								idNamePair.ID = list3[num].ID;
								idNamePair.Tag = num3;
								list2.Add(idNamePair);
							}
						}
						else if (IsPurchaseVoucher(num2))
						{
							if (num2 == 6400)
							{
								num3 *= -1.0;
							}
							idNamePair = new IdNamePair();
							idNamePair.ID = Convert.ToInt32(VATVariable.TotalPurhcase);
							idNamePair.Tag = num3;
							list2.Add(idNamePair);
							for (num = 0; num < list4.Count; num++)
							{
								idNamePair = new IdNamePair();
								idNamePair.ID = list4[num].ID;
								idNamePair.Tag = num3;
								list2.Add(idNamePair);
							}
						}
					}
					dataReader.Close();
				}
			}
			catch (Exception ex)
			{
				empty = ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return list2.ToArray();
	}

	private bool IsSalesVoucher(int iVoucherType)
	{
		if ((iVoucherType & 0xFF00) == 3328 || (iVoucherType & 0xFF00) == 6144 || (iVoucherType & 0xFF00) == 5632 || (iVoucherType & 0xFF00) == 1792)
		{
			return true;
		}
		return false;
	}

	private bool IsPurchaseVoucher(int iVoucherType)
	{
		if ((iVoucherType & 0xFF00) == 768 || (iVoucherType & 0xFF00) == 2560 || (iVoucherType & 0xFF00) == 1280 || (iVoucherType & 0xFF00) == 6400)
		{
			return true;
		}
		return false;
	}

	public IdNamePair[] UpdateModulePatch(Module[] arrModules, int iCompId)
	{
		List<IdNamePair> list = null;
		string[] array = null;
		int num = 0;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		list = new List<IdNamePair>();
		if (arrModules != null)
		{
			using (database.CreateConnection())
			{
				foreach (Module module in arrModules)
				{
					array = null;
					switch (module)
					{
					case Module.CoreTransactions:
						list.Add(new IdNamePair(1, "Transaction updated"));
						break;
					case Module.Inventory:
						array = new RDDbMntTables().GetRDAlters(FConvert.GetYearId(iCompId));
						list.Add(new IdNamePair(1, "Inventory updated"));
						break;
					case Module.Security:
						list.Add(new IdNamePair(1, "Security updated"));
						break;
					}
					if (array != null)
					{
						for (num = 0; num < array.Length; num++)
						{
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(array[num]) : array[num]);
							database.ExecuteNonQuery(dbCommand);
						}
					}
				}
			}
		}
		return list.ToArray();
	}

	public string SaveReportScheduler(ReportSchedule objRS, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		new StringBuilder();
		string empty = string.Empty;
		string result = string.Empty;
		_ = string.Empty;
		DbConnection dbConnection;
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				if (objRS.Schedule == null)
				{
					result = "Schedule value is null";
					return result;
				}
				if (objRS.ReportId != -1)
				{
					empty = $"SELECT iReportId FROM cCore_ReportSchedule{FConvert.GetSuffix(iCompId)} WHERE iReportId = {objRS.ReportId}";
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					empty = ((Convert.ToString(database.ExecuteScalar(dbCommand)).Length != 0) ? $"UPDATE cCore_ReportSchedule{FConvert.GetSuffix(iCompId)} SET biSchedule = @Schedule, biInput = @Input, iActive = {(objRS.IsPickEmailFromCustomer ? 1 : 0)}, iOutput = {objRS.Output}, sOutputTo = N'{objRS.OutputPath}' \r\n                                                            WHERE iReportId = {objRS.ReportId}" : $"INSERT INTO cCore_ReportSchedule{FConvert.GetSuffix(iCompId)}(iReportId, biSchedule, biInput, iActive, iOutput, sOutputTo)\r\n                                                            VALUES ( {objRS.ReportId}, @Schedule, @Input, {(objRS.IsPickEmailFromCustomer ? 1 : 0)}, {objRS.Output}, N'{objRS.OutputPath}' );SELECT @@IDENTITY");
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					database.AddInParameter(dbCommand, "@Schedule", DbType.Binary, objRS.Schedule);
					database.AddInParameter(dbCommand, "@Input", DbType.Binary, objRS.ReportInput);
					database.ExecuteNonQuery(dbCommand);
				}
			}
			catch (Exception ex)
			{
				result = ex.Message;
				return result;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return result;
	}

	public IdNamePair[] LoadReportSchedulers(int iReportId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		ReportSchedule reportSchedule = null;
		List<IdNamePair> list = null;
		list = new List<IdNamePair>();
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				empty = string.Format("Select tcore_scheduler.iModuleId, cCore_ReportSchedule{0}.iReportId[iReportId], sReportName, biSchedule, biInput, iActive, iOutput, ISNULL(sOutputTo,'')[sOutputTo]\r\n                    FROM cCore_ReportSchedule{0}\r\n                    JOIN cCore_Reports{0} ON cCore_ReportSchedule{0}.iReportId = cCore_Reports{0}.iReportId\r\n                    JOIN tcore_scheduler ON tcore_scheduler.iId = cCore_Reports{0}.iReportId", FConvert.GetSuffix(iCompId));
				if (iReportId > 0)
				{
					empty += $" WHERE cCore_Reports{FConvert.GetSuffix(iCompId)}.iReportId = {iReportId}";
				}
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					reportSchedule = new ReportSchedule();
					reportSchedule.Schedule = (byte[])dataReader["biSchedule"];
					reportSchedule.ReportInput = (byte[])dataReader["biInput"];
					reportSchedule.IsPickEmailFromCustomer = Convert.ToInt32(dataReader["iActive"]) == 1;
					reportSchedule.Output = Convert.ToInt32(dataReader["iOutput"]);
					reportSchedule.OutputPath = Convert.ToString(dataReader["sOutputTo"]);
					reportSchedule.ReportId = Convert.ToInt32(dataReader["iReportId"]);
					reportSchedule.ReportModule = (Module)Convert.ToInt32(dataReader["iModuleId"]);
					list.Add(new IdNamePair(Convert.ToInt32(dataReader["iReportId"]), Convert.ToString(dataReader["sReportName"]), reportSchedule));
				}
				dataReader.Close();
			}
			catch (Exception ex)
			{
				empty = ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return list.ToArray();
	}

	public string DeleteScheduler(int iReportId, int iCompId)
	{
		string empty = string.Empty;
		string result = string.Empty;
		Database database = null;
		DbCommand dbCommand = null;
		database = DatabaseWrapper.GetDatabase2(iCompId);
		try
		{
			empty = $"DELETE from cCore_ReportSchedule{FConvert.GetSuffix(iCompId)} WHERE iReportId = {iReportId}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			database.ExecuteNonQuery(dbCommand);
		}
		catch (Exception ex)
		{
			result = ex.Message;
		}
		return result;
	}

	public AppMenuData LoadSelectedAppMenus(int iMenuId, int iLanguageId, int iUserID, int iCompId)
	{
		AppMenuData appMenuData = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		try
		{
			empty = $"SELECT m.iMenuId, m.iType, m.bLeaf, l.sItemName, ISNULL(byParam, 0x)[byParam] FROM cCore_AppMenu m \r\n                                            JOIN cCore_AppMenuLanguage l ON m.iAppMenuId = l.iAppMenuId\r\n                                            JOIN cCore_AppMenuParam p ON p.iAppMenuId = l.iAppMenuId                                                \r\n                                            WHERE iLanguageId = {iLanguageId} AND iMenuId = {iMenuId} order by iSequence";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				appMenuData = new AppMenuData();
				appMenuData.MenuId = Convert.ToInt32(dataReader["iMenuId"]);
				appMenuData.Type = (AppType)Convert.ToByte(dataReader["iType"]);
				appMenuData.ItemName = Convert.ToString(dataReader["sItemName"]);
				appMenuData.Leaf = Convert.ToBoolean(dataReader["bLeaf"]);
				appMenuData.Parameters = (byte[])dataReader["byParam"];
				appMenuData.ActionId = LoadActionID(appMenuData.MenuId, iUserID, appMenuData.Type, iCompId);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			appMenuData.MenuId = -1;
			appMenuData.ItemName = ex.Message;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return appMenuData;
	}

	public AppMenuData[] LoadAppMenus(int iLanguageId, int iUserID, int iCompId, bool bForScreen = false)
	{
		List<AppMenuData> list = new List<AppMenuData>();
		AppMenuData appMenuData = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		try
		{
			empty = $"SELECT m.iMenuId, m.iType, m.bLeaf, ISNULL(m.iGroupId, 0)[iGroupId], ISNULL(g.iParentId,0)[iParentId], l.sItemName, ISNULL(byParam, 0x)[byParam] FROM cCore_AppMenu m \r\n                                                LEFT JOIN cCore_AppMenuGroup g ON g.iGroupId = m.iGroupId                                                \r\n                                                JOIN cCore_AppMenuLanguage l ON m.iAppMenuId = l.iAppMenuId\r\n                                                JOIN cCore_AppMenuParam p ON p.iAppMenuId = l.iAppMenuId   \r\n                                                \r\n                                                WHERE iLanguageId = {iLanguageId} order by iSequence";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				appMenuData = new AppMenuData();
				appMenuData.MenuId = Convert.ToInt32(dataReader["iMenuId"]);
				appMenuData.Type = (AppType)Convert.ToByte(dataReader["iType"]);
				appMenuData.ItemName = Convert.ToString(dataReader["sItemName"]);
				appMenuData.Leaf = Convert.ToBoolean(dataReader["bLeaf"]);
				appMenuData.GroupId = Convert.ToInt32(dataReader["iGroupId"]);
				appMenuData.ParentId = Convert.ToInt32(dataReader["iParentId"]);
				appMenuData.Parameters = (byte[])dataReader["byParam"];
				appMenuData.ActionId = LoadActionID(appMenuData.MenuId, iUserID, appMenuData.Type, iCompId);
				if (!bForScreen)
				{
					if (iUserID == 1)
					{
						list.Add(appMenuData);
					}
					else if (appMenuData.ActionId.Length != 0)
					{
						for (int i = 0; i < appMenuData.ActionId.Length; i++)
						{
							if (appMenuData.ActionId[i] == 1)
							{
								list.Add(appMenuData);
								break;
							}
						}
					}
					else if (appMenuData.Type == AppType.Group)
					{
						list.Add(appMenuData);
					}
				}
				else
				{
					list.Add(appMenuData);
				}
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			empty = ex.Message;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		if (!bForScreen)
		{
			int num = 0;
			int item = 0;
			bool flag = true;
			List<int> list2 = new List<int>();
			for (int j = 0; j < list.Count; j++)
			{
				flag = true;
				if (list[j].Type == AppType.Group && !list[j].Leaf)
				{
					num = list[j].GroupId;
					item = j;
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].GroupId == num && list[k].Leaf && list[k].Type != AppType.Group)
						{
							flag = true;
							break;
						}
						flag = false;
					}
				}
				if (!flag)
				{
					list2.Add(item);
				}
			}
			if (list2.Count > 0)
			{
				list2.Reverse();
				for (int l = 0; l < list2.Count; l++)
				{
					list.RemoveAt(list2[l]);
				}
			}
		}
		return list.ToArray();
	}

	public string SaveAppData(AppMenuData[] arrMenu, int iCompId)
	{
		int num = 0;
		string empty = string.Empty;
		string result = string.Empty;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		int num2 = 0;
		try
		{
			empty = $"DELETE FROM cCore_AppMenuParam\r\n                                           DELETE FROM cCore_AppMenuLanguage\r\n                                           DELETE FROM cCore_AppMenu";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			database.ExecuteNonQuery(dbCommand);
			for (num = 0; num < arrMenu.Length; num++)
			{
				empty = $" INSERT INTO cCore_AppMenu ( iMenuId, iType, bLeaf, iSequence, iModuleType, iGroupId ) \r\n                                                VALUES ({arrMenu[num].MenuId},{Convert.ToInt32(arrMenu[num].Type)},{(arrMenu[num].Leaf ? 1 : 0)},{arrMenu[num].Sequence},{Convert.ToInt32(arrMenu[num].ModuleType)},{arrMenu[num].GroupId});SELECT @@IDENTITY";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				num2 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				empty = $" INSERT INTO cCore_AppMenuLanguage ( iAppMenuId, iLanguageId, sItemName ) \r\n                                                VALUES ( {num2}, {arrMenu[num].LanguageId}, '{arrMenu[num].ItemName}')";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				database.ExecuteNonQuery(dbCommand);
				empty = $" INSERT INTO cCore_AppMenuParam ( iAppMenuId, byParam ) \r\n                                                VALUES ( {num2}, @Param )";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				if (arrMenu[num].Parameters != null)
				{
					database.AddInParameter(dbCommand, "@Param", DbType.Binary, arrMenu[num].Parameters);
				}
				else
				{
					database.AddInParameter(dbCommand, "@Param", DbType.Binary, null);
				}
				database.ExecuteNonQuery(dbCommand);
			}
		}
		catch (Exception ex)
		{
			result = ex.Message;
		}
		return result;
	}

	public IdNamePair SaveAppGroupData(IdNamePair objGetIdName, int iCompId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		string empty = string.Empty;
		_ = string.Empty;
		int num = 0;
		IdNamePair idNamePair = null;
		try
		{
			empty = $" INSERT INTO cCore_AppMenuGroup ( sGroupName ,iParentId ) \r\n                                            VALUES ( '{objGetIdName.Name}',{objGetIdName.Tag} );SELECT @@IDENTITY";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			num = Convert.ToInt32(database.ExecuteScalar(dbCommand));
			empty = $"SELECT iGroupId, sGroupName, iParentId from cCore_AppMenuGroup WHERE iGroupId = {num}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iGroupId"]);
				idNamePair.Name = Convert.ToString(dataReader["sGroupName"]);
				idNamePair.Tag = Convert.ToInt32(dataReader["iParentId"]);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			_ = ex.Message;
		}
		return idNamePair;
	}

	public string DeleteAppGroupData(IdNamePair[] arrGroupId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		int num = 0;
		string empty = string.Empty;
		string result = string.Empty;
		int num2 = 0;
		try
		{
			for (num = 0; num < arrGroupId.Length; num++)
			{
				empty = $"DELETE cCore_AppMenuGroup WHERE iGroupId = {arrGroupId[num].ID}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				database.ExecuteNonQuery(dbCommand);
				empty = $"SELECT iAppMenuId FROM cCore_AppMenuLanguage WHERE sItemName = '{arrGroupId[num].Name}'";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				num2 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				if (num2 > 0)
				{
					empty = string.Format("DELETE cCore_AppMenuParam WHERE iAppMenuId = {0} DELETE cCore_AppMenuLanguage WHERE iAppMenuId = {0} DELETE cCore_AppMenu WHERE iAppMenuId = {0}", num2);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					database.ExecuteNonQuery(dbCommand);
				}
			}
		}
		catch (Exception ex)
		{
			result = ex.Message;
		}
		return result;
	}

	private int[] LoadActionID(int iMenuId, int iUserId, AppType AppType, int iCompId)
	{
		string empty = string.Empty;
		List<int> list = new List<int>();
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		int num = 0;
		string empty2 = string.Empty;
		try
		{
			if (iUserId > 1)
			{
				empty = string.Format("Select RightsTable.iActionId[iActionId] from\r\n                                        (\t\r\n                                            select iMenuId, iActionId from mSec_ProfileDetails where iProfileId in \r\n\t                                        (\r\n    \t                                        select iProfileId from mSec_RolesProfiles a \r\n\t                                            join mSec_Users_Roles r on r.iERPRole = a.iRoleId where r.iUserId = {0} and r.iYearId = 0\r\n                                            )\r\n                                            union \r\n                                            select iMenuId, iActionId from mSec_RolesExtraRights a \r\n                                            join mSec_Users_Roles r on r.iERPRole = a.iRoleId where r.iUserId = {0} and r.iYearId = 0 and bAdd = 1 \r\n                                            except \r\n                                            select iMenuId, iActionId from mSec_RolesExtraRights a \r\n                                            join mSec_Users_Roles r on r.iERPRole = a.iRoleId where r.iUserId = {0} and r.iYearId = 0 and bAdd = 0\r\n                                            union \r\n                                            select iMenuId, 0 from mCore_Menu where bGroup = 1\r\n                                        ) as RightsTable\r\n                                        INNER JOIN mCore_Menu on mCore_Menu.iMenuId = RightsTable.iMenuId\r\n                                        {1} \r\n                                        ORDER BY iMenuGroupId, iMenuIndex, mCore_Menu.iMenuId", iUserId, AppType switch
				{
					AppType.Master => (object)$"Where mCore_Menu.iTypeId = {iMenuId} AND iType = 0", 
					AppType.Transaction => $"Where mCore_Menu.iTypeId = {iMenuId} AND iType = 1", 
					AppType.Alert => string.Format("Where mCore_Menu.iTypeId = 202 AND iType = {1}", iMenuId, 5), 
					_ => $"Where mCore_Menu.iMenuId = {iMenuId}", 
				});
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					num = Convert.ToInt32(dataReader["iActionId"]);
					list.Add(num);
				}
				dataReader.Close();
			}
		}
		catch
		{
		}
		return list.ToArray();
	}

	public IdNamePair[] LoadPictureDocumentField(int iVoucherType, int iFieldType, int iCompId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		List<IdNamePair> list = new List<IdNamePair>();
		IdNamePair idNamePair = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		try
		{
			empty = $"SELECT a.iFieldId[iFieldId], a.sFieldName[sFieldName] FROM cCore_VoucherFields{FConvert.GetSuffix(iCompId)} a JOIN cCore_Fields b\r\n                                           ON a.iUniqueId = b.iFieldId\r\n                                           WHERE iVoucherType = {iVoucherType} AND iDataTypeId = {iFieldType}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iFieldId"]);
				idNamePair.Name = Convert.ToString(dataReader["sFieldName"]);
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			idNamePair = new IdNamePair();
			idNamePair.ID = -1;
			idNamePair.Name = ex.Message;
			list.Add(idNamePair);
		}
		return list.ToArray();
	}

	public IdNamePair[] LoadAllTemplateName(int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		IdNamePair idNamePair = null;
		List<IdNamePair> list = new List<IdNamePair>();
		try
		{
			empty = $"SELECT iTemplateId, sTemplateName, ISNULL(iOutput,0)[iOutput] from cCore_PrintTemplate{FConvert.GetSuffix(iCompId)}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iTemplateId"]);
				idNamePair.Name = Convert.ToString(dataReader["sTemplateName"]);
				idNamePair.Tag = Convert.ToInt32(dataReader["iOutput"]);
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			idNamePair = new IdNamePair();
			idNamePair.ID = -1;
			idNamePair.Name = ex.Message;
			list.Add(idNamePair);
		}
		return list.ToArray();
	}

	public IdValuePair[] LoadReportTemplate(int iTemplateId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		IdValuePair idValuePair = null;
		List<IdValuePair> list = new List<IdValuePair>();
		try
		{
			empty = $"SELECT iReportId, ISNULL(byParam, 0x)[byParam] FROM cCore_PrintReportParam{FConvert.GetSuffix(iCompId)}\r\n                                            WHERE iTemplateId = {iTemplateId}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idValuePair = new IdValuePair();
				idValuePair.ID = Convert.ToInt32(dataReader["iReportId"]);
				idValuePair.Value = FConvert.ByteArrayToObject((byte[])dataReader["byParam"]);
				list.Add(idValuePair);
			}
			dataReader.Close();
		}
		catch
		{
		}
		return list.ToArray();
	}

	public string SaveReportTemplate(PrintTemplate objPrintTemplate, int iCompId)
	{
		int num = 0;
		int num2 = 0;
		string empty = string.Empty;
		string result = string.Empty;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		try
		{
			empty = $"SELECT iTemplateId FROM cCore_PrintTemplate{FConvert.GetSuffix(iCompId)} \r\n                                            WHERE sTemplateName = '{objPrintTemplate.TemplateName}'";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			num2 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
			if (num2 == 0)
			{
				empty = $"INSERT INTO cCore_PrintTemplate{FConvert.GetSuffix(iCompId)}( sTemplateName, iOutput )\r\n                                                VALUES ('{objPrintTemplate.TemplateName}', {objPrintTemplate.Output} );SELECT @@IDENTITY";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				num2 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				for (num = 0; num < objPrintTemplate.ReportParam.Length; num++)
				{
					empty = $"INSERT INTO cCore_PrintReportParam{FConvert.GetSuffix(iCompId)} ( iTemplateId, iReportId,  byParam)\r\n                                                    VALUES ( {num2}, {objPrintTemplate.ReportParam[num].ID}, @Param{num} )";
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					if (objPrintTemplate.ReportParam[num].Value != null)
					{
						database.AddInParameter(dbCommand, $"@Param{num}", DbType.Binary, FConvert.ObjectToByteArray(objPrintTemplate.ReportParam[num].Value));
					}
					else
					{
						database.AddInParameter(dbCommand, $"@Param{num}", DbType.Binary, null);
					}
					database.ExecuteNonQuery(dbCommand);
				}
			}
			else
			{
				if (objPrintTemplate.Output >= 0)
				{
					empty = $"UPDATE cCore_PrintTemplate{FConvert.GetSuffix(iCompId)} SET iOutput = {objPrintTemplate.Output} WHERE iTemplateId = {num2}";
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					database.ExecuteNonQuery(dbCommand);
				}
				empty = $"DELETE from cCore_PrintReportParam{FConvert.GetSuffix(iCompId)}\r\n                                                WHERE iTemplateId = {num2} ";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				database.ExecuteNonQuery(dbCommand);
				for (num = 0; num < objPrintTemplate.ReportParam.Length; num++)
				{
					empty = $"INSERT INTO cCore_PrintReportParam{FConvert.GetSuffix(iCompId)} ( iTemplateId, iReportId,  byParam)\r\n                                                    VALUES ( {num2}, {objPrintTemplate.ReportParam[num].ID}, @Param{num} )";
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					if (objPrintTemplate.ReportParam[num].Value != null)
					{
						database.AddInParameter(dbCommand, $"@Param{num}", DbType.Binary, FConvert.ObjectToByteArray(objPrintTemplate.ReportParam[num].Value));
					}
					else
					{
						database.AddInParameter(dbCommand, $"@Param{num}", DbType.Binary, null);
					}
					database.ExecuteNonQuery(dbCommand);
				}
			}
		}
		catch (Exception ex)
		{
			result = ex.Message;
		}
		return result;
	}

	public string DeleteReportTemplate(int iTemplateId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		string empty = string.Empty;
		try
		{
			empty = string.Format("DELETE from cCore_PrintReportParam{0}\r\n                                            WHERE iTemplateId = {1}\r\n                                            DELETE from cCore_PrintTemplate{0}\r\n                                            WHERE iTemplateId =  {1}", FConvert.GetSuffix(iCompId), iTemplateId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			database.ExecuteNonQuery(dbCommand);
		}
		catch
		{
		}
		return "";
	}

	public IdNamePair[] LoadAccountEmails(int[] arrFields, int iCompId)
	{
		string empty = string.Empty;
		IDataReader dataReader = null;
		IdNamePair idNamePair = null;
		List<IdNamePair> list = null;
		list = new List<IdNamePair>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompId);
			empty = ((!_focus.company(iCompId).isModuleImplemented(ModulesImplemented.SubLedger)) ? string.Format("SELECT iMasterId,sName,sEMail FROM vmCore_Account WHERE iMasterId IN ({0}) AND LEN(sEMail) > 4 AND iTreeId = 0", string.Join(",", arrFields.Select((int p) =>
			{
				int num = p;
				return num.ToString();
			}).ToArray())) : string.Format("SELECT iMasterId,sName,sEMail FROM vmCore_Subledger WHERE iMasterId IN ({0}) AND LEN(sEMail) > 4 AND iTreeId = 0", string.Join(",", arrFields.Select((int p) =>
			{
				int num = p;
				return num.ToString();
			}).ToArray())));
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iMasterId"]);
				idNamePair.Name = Convert.ToString(dataReader["sName"]);
				idNamePair.Tag = Convert.ToString(dataReader["sEMail"]);
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			idNamePair = new IdNamePair();
			idNamePair.ID = -1;
			idNamePair.Name = ex.Message;
		}
		return list.ToArray();
	}

	public string GetReportGroupHeading(uint iReportId, int iMasterId, int iCompId)
	{
		DbCommand dbCommand = null;
		string text = string.Empty;
		switch ((FocusReport)iReportId)
		{
		case FocusReport.Ledger:
		case FocusReport.SubLedger:
		case FocusReport.VendorStatements:
		case FocusReport.VendorDueDateAnalysis:
		case FocusReport.VendorAgeingDetailsBillwise:
		case FocusReport.VendorDetailAgeingByDueDate:
		case FocusReport.VendorOverdueAnalysis:
		case FocusReport.CustomerStatements:
		case FocusReport.CustomerDueDateAnalysis:
		case FocusReport.CustomerAgeingDetailsBillwise:
		case FocusReport.CustomerDetailAgeingByDueDate:
		case FocusReport.CustomerOverdueAnalysis:
			text = $"SELECT sName from vmCore_Account WHERE iMasterId  = {iMasterId}";
			break;
		case FocusReport.DayBook:
			return new Date(m_objCalType)
			{
				Value = iMasterId
			}.ToString();
		case FocusReport.LogBook:
			text = $"SELECT sLoginName from mSecUsers WHERE iUserId  = {iMasterId}";
			break;
		case FocusReport.StockLedger:
		case FocusReport.StockStatement:
		case FocusReport.StockValuation:
		case FocusReport.ReorderReport:
		case FocusReport.FastMovingStock:
		case FocusReport.SlowMovingStock:
		case FocusReport.RetailSalesProductWise:
		case FocusReport.SummarySalesByProduct:
		case FocusReport.FastMovingItemsOutletWise:
		case FocusReport.SlowMovingItemsOutletWise:
			text = $"SELECT sName from vmCore_Product WHERE iMasterId  = {iMasterId}";
			break;
		case FocusReport.StockBalanceByProduct:
		case FocusReport.StockBalanceByBins:
		case FocusReport.StockBalanceByProductByBins:
		case FocusReport.ExpiredStockByBins:
			text = $"SELECT sName from vmCore_Bins WHERE iMasterId  = {iMasterId}";
			break;
		case FocusReport.ABCAnalysisProdcut:
			return iMasterId switch
			{
				2 => "B Product", 
				3 => "C Product", 
				_ => "A Product", 
			};
		}
		if (!string.IsNullOrEmpty(text))
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			return Convert.ToString(database.ExecuteScalar(dbCommand));
		}
		return iMasterId.ToString();
	}

	public CurrencyDetail LoadCurrencyData(int iCurrencyId, int iCompanyId)
	{
		IDataReader dataReader = null;
		CurrencyDetail currencyDetail = null;
		string empty = string.Empty;
		currencyDetail = new CurrencyDetail();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			empty = $"SELECT sCode,sName,sCoinsName,sSymbol,iNoOfDecimals,fGeneralRoundOff,iRoundingType,sCurrencyUnit,sConnector,sCurrencySubUnit \r\n                ,sCurrencyUnitAlias,sConnectorAlias,sCurrencySubUnitAlias\r\n                FROM mCore_Currency WHERE iCurrencyId = {iCurrencyId}";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			if (dataReader.Read())
			{
				currencyDetail.Id = iCurrencyId;
				currencyDetail.Code = Convert.ToString(dataReader["sCode"]);
				currencyDetail.Name = Convert.ToString(dataReader["sName"]);
				currencyDetail.Coin = Convert.ToString(dataReader["sCoinsName"]);
				currencyDetail.Symbol = Convert.ToString(dataReader["sSymbol"]);
				currencyDetail.NoOfDecimal = Convert.ToByte(dataReader["iNoOfDecimals"]);
				currencyDetail.RoundOff = Convert.ToDouble(dataReader["fGeneralRoundOff"]);
				currencyDetail.RoundingType = (RoundingType)Convert.ToByte(dataReader["iRoundingType"]);
				currencyDetail.CurrencyUnit = Convert.ToString(dataReader["sCurrencyUnit"]);
				currencyDetail.Connector = Convert.ToString(dataReader["sConnector"]);
				currencyDetail.SubUnit = Convert.ToString(dataReader["sCurrencySubUnit"]);
				currencyDetail.CurrencyUnitAlias = Convert.ToString(dataReader["sCurrencyUnitAlias"]);
				currencyDetail.ConnectorAlias = Convert.ToString(dataReader["sConnectorAlias"]);
				currencyDetail.SubUnitAlias = Convert.ToString(dataReader["sCurrencySubUnitAlias"]);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			currencyDetail.Id = -1;
			currencyDetail.Name = ex.Message;
		}
		return currencyDetail;
	}

	public CurrencyDetail[] LoadAllCurrencyData(int iCompanyId)
	{
		IDataReader dataReader = null;
		CurrencyDetail currencyDetail = null;
		string empty = string.Empty;
		List<CurrencyDetail> list = new List<CurrencyDetail>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			empty = "SELECT iCurrencyId,sCode,sName,sCoinsName,sSymbol,iNoOfDecimals,fGeneralRoundOff,iRoundingType,sCurrencyUnit,sConnector,sCurrencySubUnit \r\n                ,sCurrencyUnitAlias,sConnectorAlias,sCurrencySubUnitAlias FROM mCore_Currency\r\n                ORDER BY CASE WHEN sCode = 'EUR' and iCurrencyId = 39 THEN 0 ELSE iCurrencyId END";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			while (dataReader.Read())
			{
				currencyDetail = new CurrencyDetail();
				currencyDetail.Id = Convert.ToInt32(dataReader["iCurrencyId"]);
				currencyDetail.Code = Convert.ToString(dataReader["sCode"]);
				currencyDetail.Name = Convert.ToString(dataReader["sName"]);
				currencyDetail.Coin = Convert.ToString(dataReader["sCoinsName"]);
				currencyDetail.Symbol = Convert.ToString(dataReader["sSymbol"]);
				currencyDetail.NoOfDecimal = Convert.ToByte(dataReader["iNoOfDecimals"]);
				currencyDetail.RoundOff = Convert.ToDouble(dataReader["fGeneralRoundOff"]);
				currencyDetail.RoundingType = (RoundingType)Convert.ToByte(dataReader["iRoundingType"]);
				currencyDetail.CurrencyUnit = Convert.ToString(dataReader["sCurrencyUnit"]);
				currencyDetail.Connector = Convert.ToString(dataReader["sConnector"]);
				currencyDetail.SubUnit = Convert.ToString(dataReader["sCurrencySubUnit"]);
				currencyDetail.CurrencyUnitAlias = Convert.ToString(dataReader["sCurrencyUnitAlias"]);
				currencyDetail.ConnectorAlias = Convert.ToString(dataReader["sConnectorAlias"]);
				currencyDetail.SubUnitAlias = Convert.ToString(dataReader["sCurrencySubUnitAlias"]);
				list.Add(currencyDetail);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			list.Add(new CurrencyDetail
			{
				Id = -1,
				Code = ex.Message
			});
		}
		return list.ToArray();
	}

	public string SavePrintingInvoice(LayoutInformation info, int iCompId)
	{
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		short num5 = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = database.CreateConnection();
		DbTransaction dbTransaction = null;
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = new StringBuilder();
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				dbTransaction = dbConnection.BeginTransaction();
				bool bNewData = info.Layout.ID == ERRORNO;
				if (info.Layout.ID != ERRORNO)
				{
					m_bDeleteLayout = false;
					empty = Delete(info.Layout.ID, MasterType.InvoiceDesigner, iCompId, dbConnection, dbTransaction);
					if (empty.Length > 0)
					{
						return empty;
					}
				}
				if (info.Layout.ID == ERRORNO)
				{
					empty = string.Format("INSERT INTO cCore_InvoiceLayout{24} (iReportId, sLayout, fPageWidth, fPageHeight, fLeftMargin, fTopMargin, \r\n                        fRightMargin, fBottomMargin, iPageUnit, iPageView, iModule, iReportType, \r\n                        sHtml, sPrinter, iModifierOption, fColumnMargin, fColumnWidth, iNoOfCopy, iFlag, iSubReportId, fColumnHeight,fRowMargin,iRowsPerPage,iColumnsPerPage)\r\n                            VALUES({0}, N'{1}', {2}, {3}, {4}, {5}, \r\n                            {6}, {7}, {8}, {9}, {10}, {11}, \r\n                            '{12}', '{13}',{14},{15},{16},{17}, {18}, {19},{20},\r\n                            {21},{22},{23});\r\n                            SELECT @@IDENTITY", info.ReportId, info.Layout.Name, info.PrintInfo.PageWidth, info.PrintInfo.PageHeight, info.PrintInfo.Margin.Left, info.PrintInfo.Margin.Top, info.PrintInfo.Margin.Right, info.PrintInfo.Margin.Bottom, (byte)info.PrintInfo.Unit, (byte)info.PrintInfo.View, (byte)info.Module, (byte)info.ReportType, GetSingleQuote(info.HtmlSource), info.DefaultPrinter, (byte)info.ModifierOption, info.HorizontalGap, info.ColumnWidth, info.NoOfCopies, GetFlagValue(info.IsPrintInDraftMode, info.IsSuspendNet, info.IsDonotPrintQuantityforModifiers, info.PrintApprovalHistoryAtTheEnd, info.PageNumberOption), info.SubReportId, info.ColumnHeight, info.VerticalGap, info.RowsPerPage, info.ColumnsPerPage, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num5 = 1;
					info.Layout.ID = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
				}
				else
				{
					empty = string.Format("UPDATE cCore_InvoiceLayout{24} SET sLayout=N'{0}', fPageWidth={1}, fPageHeight={2}, fLeftMargin={3},\r\n                                fTopMargin={4}, fRightMargin={5}, fBottomMargin={6}, iPageUnit={7}, iPageView={8}, iModule={9}, iReportType={10}, \r\n                                sHtml='{12}', sPrinter = '{13}', iModifierOption= {14}, fColumnMargin = {15}, fColumnWidth = {16}, iNoOfCopy = {17}, \r\n                                iFlag = {18}, iSubReportId = {19},fColumnHeight={20},fRowMargin={21},iRowsPerPage={22},iColumnsPerPage={23} \r\n                                where iLayoutId={11}", info.Layout.Name, info.PrintInfo.PageWidth, info.PrintInfo.PageHeight, info.PrintInfo.Margin.Left, info.PrintInfo.Margin.Top, info.PrintInfo.Margin.Right, info.PrintInfo.Margin.Bottom, (byte)info.PrintInfo.Unit, (byte)info.PrintInfo.View, (byte)info.Module, (byte)info.ReportType, info.Layout.ID, info.HtmlSource, info.DefaultPrinter, (byte)info.ModifierOption, info.HorizontalGap, info.ColumnWidth, info.NoOfCopies, GetFlagValue(info.IsPrintInDraftMode, info.IsSuspendNet, info.IsDonotPrintQuantityforModifiers, info.PrintApprovalHistoryAtTheEnd, info.PageNumberOption), info.SubReportId, info.ColumnHeight, info.VerticalGap, info.RowsPerPage, info.ColumnsPerPage, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num5 = 1;
					database.ExecuteNonQuery(dbCommand, dbTransaction);
				}
				if (info.Layout.ID <= 0)
				{
					dbTransaction.Rollback();
					return "!1Save Failed in Layout table";
				}
				for (num = 0; num < info.Pages.Length; num++)
				{
					empty = string.Format("INSERT INTO cCore_InvoicePage{2} (iLayoutId,bIsPageAfterPreviousPage) VALUES({0},{1});SELECT @@IDENTITY", info.Layout.ID, info.Pages[num].IsPageAfterPreviousPage ? 1 : 0, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num5 = 2;
					num3 = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
					num2 = 0;
					PageHeader[] pageHeader = info.Pages[num].PageHeader;
					foreach (PageHeader pageHeader2 in pageHeader)
					{
						if (pageHeader2.ImageSource == null)
						{
							empty = string.Format("INSERT INTO cCore_InvoiceHeader{24} (iHeaderId, iPageId, iControlType, sText, fLeft, \r\n                                fTop, fWidth, fHeight, fBorderThickness, iBorderColor, \r\n                                iBackColor, iTextColor, sTextFont, fFontSize, iFontWeights, \r\n                                iFontStyle, iVariable, iShowHeader, iCategoryId, iFieldId, \r\n                                iSubParentId, iFontEffect, bArabicDigit, iPixelInLine) \r\n                                VALUES ({0}, {1}, {2}, N'{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}', {13}, {14}, \r\n                                {15}, {16}, {17}, {18}, {19}, {20}, {21}, {22}, {23});", pageHeader2.UID, num3, Convert.ToInt16(pageHeader2.Type), GetSingleQuote(pageHeader2.Text), pageHeader2.Left, pageHeader2.Top, pageHeader2.Width, pageHeader2.Height, pageHeader2.BorderThickness, pageHeader2.BorderColor, pageHeader2.BackColor, pageHeader2.TextColor, pageHeader2.TextFont, pageHeader2.FontSize, Convert.ToInt16(pageHeader2.FontWeight), Convert.ToInt16(pageHeader2.FontStyle), Convert.ToInt16(pageHeader2.VariableType), Convert.ToInt16(pageHeader2.ShowOnPage), pageHeader2.MasterId, pageHeader2.FieldId, pageHeader2.SubParentId, Convert.ToByte(pageHeader2.FontEffect), pageHeader2.IsArabicDigit ? 1 : 0, (pageHeader2.StaticTextProperties != null) ? pageHeader2.StaticTextProperties.PixelInLinesWordWrap : 0, suffix);
							empty += $"INSERT INTO cCore_InvoiceBodyValue{suffix}(iHeaderId, iPageId, iFieldId, iValue) VALUES({pageHeader2.UID}, {num3}, {15}, {(pageHeader2.WordWrap ? 1 : 0)});";
							if (pageHeader2.UID > 2000 && pageHeader2.StaticTextProperties != null)
							{
								empty += $"INSERT INTO cCore_InvoiceNumericProperty{suffix} (iHeaderId, iPageId, iAlignment, iDecimalInColumn, iRoundOffType, \r\n                                    fRoundOffValue, bInsertCommas, iFunctionType, sSuffix, iAmtInWords, \r\n                                    iSign, iCaseStyle, sLabelforZeroVal, iDataType, sPrefix) \r\n                                    VALUES ({pageHeader2.UID}, {num3}, {pageHeader2.StaticTextProperties.Alignment}, {pageHeader2.StaticTextProperties.DecimalInColumn}, {(byte)pageHeader2.StaticTextProperties.RoundOffType}, {pageHeader2.StaticTextProperties.RoundUptoValue}, {(pageHeader2.StaticTextProperties.InsertComma ? 1 : 0)}, {(byte)pageHeader2.StaticTextProperties.FunctionType}, N'{pageHeader2.StaticTextProperties.SuffixForAmtInWords}', {(byte)pageHeader2.StaticTextProperties.AmountInWords}, {(byte)pageHeader2.StaticTextProperties.Sign}, {(byte)pageHeader2.StaticTextProperties.CaseStyle}, N'{pageHeader2.StaticTextProperties.Labelforzerovalue}', {(byte)pageHeader2.StaticTextProperties.DataType}, N'{pageHeader2.StaticTextProperties.Prefix}');";
								if (pageHeader2.Type == ControlType.Area && pageHeader2.AreaProperties != null)
								{
									empty += $"INSERT INTO cCore_InvoiceAreaControl{suffix} \r\n                                        (iPageId, iDisplayOnPage, bAutoExpand, sCondition, bDisplayBorder, iHeaderId,bIgnorePageAfterPreviousPage,iAreaPositionifBodyskip) VALUES \r\n                                        ({num3}, {(byte)pageHeader2.AreaProperties.PageSelect}, {(pageHeader2.AreaProperties.AutoExpand ? 1 : 0)}, '{pageHeader2.AreaProperties.Condition}', {(pageHeader2.AreaProperties.DisplayBorder ? 1 : 0)}, {pageHeader2.UID}, {(pageHeader2.AreaProperties.IgnorePageAfterPreviousPage ? 1 : 0)}, {pageHeader2.AreaProperties.AreaPositionIfBodySkip});";
									if (pageHeader2.AreaProperties.Format != null)
									{
										empty += SaveAreaFilter(pageHeader2.UID, pageHeader2.AreaProperties.Format.Conditions, iCompId, bIsBodyColumn: false);
									}
								}
							}
							if (pageHeader2.Type == ControlType.Table && pageHeader2.TableProperties != null)
							{
								empty += GetTableValueString(pageHeader2.TableProperties, pageHeader2.UID, num3, iCompId);
							}
							else if (pageHeader2.Type == ControlType.BodyCanvas)
							{
								if (pageHeader2.PageBodyClass.DefaultFont != null)
								{
									empty += $"INSERT INTO cCore_InvoiceBodyFont{suffix}(iHeaderId, iPageId, iFontType, byFont) VALUES({pageHeader2.UID}, {num3}, {(byte)0}, @DefaultFont);";
								}
								if (pageHeader2.PageBodyClass.HeadingFont != null)
								{
									empty += $"INSERT INTO cCore_InvoiceBodyFont{suffix}(iHeaderId, iPageId, iFontType, byFont) VALUES({pageHeader2.UID}, {num3}, {(byte)1}, @HeadingFont);";
								}
								if (pageHeader2.PageBodyClass.TotalFont != null)
								{
									empty += $"INSERT INTO cCore_InvoiceBodyFont{suffix}(iHeaderId, iPageId, iFontType, byFont) VALUES({pageHeader2.UID}, {num3}, {(byte)2}, @TotalFont);";
								}
							}
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty.ToString()) : empty.ToString());
							if (pageHeader2.PageBodyClass.DefaultFont != null)
							{
								database.AddInParameter(dbCommand, "@DefaultFont", DbType.Binary, FConvert.ObjectToByteArray(pageHeader2.PageBodyClass.DefaultFont));
							}
							if (pageHeader2.PageBodyClass.HeadingFont != null)
							{
								database.AddInParameter(dbCommand, "@HeadingFont", DbType.Binary, FConvert.ObjectToByteArray(pageHeader2.PageBodyClass.HeadingFont));
							}
							if (pageHeader2.PageBodyClass.TotalFont != null)
							{
								database.AddInParameter(dbCommand, "@TotalFont", DbType.Binary, FConvert.ObjectToByteArray(pageHeader2.PageBodyClass.TotalFont));
							}
						}
						else
						{
							empty = string.Format("INSERT INTO cCore_InvoiceHeader{23} (iHeaderId, iPageId, iControlType, sText,\r\n                                fLeft, fTop, fWidth, fHeight, fBorderThickness, iBorderColor, iBackColor, iTextColor, sTextFont, \r\n                                fFontSize, iFontWeights, iFontStyle, imgControl, iVariable, iShowHeader, iCategoryId, iFieldId, iFontEffect, bArabicDigit) \r\n                                VALUES ({0}, {1}, {2}, N'{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}', {13}, {14}, \r\n                                {15}, @image{16}, {17}, {18}, {19}, {20}, {21}, {22});", pageHeader2.UID, num3, Convert.ToInt16(pageHeader2.Type), GetSingleQuote(pageHeader2.Text), pageHeader2.Left, pageHeader2.Top, pageHeader2.Width, pageHeader2.Height, pageHeader2.BorderThickness, pageHeader2.BorderColor, pageHeader2.BackColor, pageHeader2.TextColor, pageHeader2.TextFont, pageHeader2.FontSize, Convert.ToInt16(pageHeader2.FontWeight), Convert.ToInt16(pageHeader2.FontStyle), num2, Convert.ToInt16(pageHeader2.VariableType), Convert.ToInt16(pageHeader2.ShowOnPage), pageHeader2.MasterId, pageHeader2.FieldId, Convert.ToByte(pageHeader2.FontEffect), pageHeader2.IsArabicDigit ? 1 : 0, suffix);
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty.ToString()) : empty.ToString());
							database.AddInParameter(dbCommand, $"@image{num2}", DbType.Binary, pageHeader2.ImageSource);
						}
						num2++;
						num5 = 3;
						database.ExecuteNonQuery(dbCommand, dbTransaction);
						if (pageHeader2.UID > 2000)
						{
							continue;
						}
						stringBuilder = new StringBuilder();
						PageBody[] pageBody = pageHeader2.PageBody;
						foreach (PageBody pageBody2 in pageBody)
						{
							stringBuilder = new StringBuilder();
							stringBuilder.Append($"INSERT INTO cCore_InvoiceBody{suffix} (iBodyId, iPageId, sColumn, sAlias, iDataType, \r\n                                        fColumnWidth, iDisplayColumnIndex, iSign, iCategoryId, iFieldId, \r\n                                        bWordWrap, bPrintUnderPrevColumn, sFormula, iSubParentId, bDontShowTotal, \r\n                                        iHeadingAlignment, bInsertCommas, bPrintInLine2, bHideColumn, byFont,sHeading2,\r\n                                        iFunction,sGroupName,sSuffix,bArabicDigit) \r\n                                        VALUES ({pageHeader2.UID}, {num3}, N'{pageBody2.Column}', N'{pageBody2.Alias}', {pageBody2.DataType}, \r\n                                        {pageBody2.ColumnWidth}, {pageBody2.ColumnIndex}, {Convert.ToInt16(pageBody2.Sign)}, {pageBody2.MasterId}, {pageBody2.FieldId}, \r\n                                        {Convert.ToByte(pageBody2.WordWrap)}, {Convert.ToByte(pageBody2.PrintUnderPreviousColumn)}, '{pageBody2.Formula}', {pageBody2.SubParentId}, {(pageBody2.DontShowTotal ? 1 : 0)}, \r\n                                        {Convert.ToByte(pageBody2.HeadingAlignment)}, {(pageBody2.InsertCommas ? 1 : 0)},{(pageBody2.PrintInLine2 ? 1 : 0)}, {(pageBody2.HideColumn ? 1 : 0)},@ColumnFont,N'{pageBody2.Heading2}',\r\n                                        {(int)pageBody2.FunctionType},N'{pageBody2.GroupName}',N'{pageBody2.Suffix}',{(pageBody2.IsArabicDigit ? 1 : 0)});SELECT @@IDENTITY");
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
							if (pageBody2.ColumnFont == null)
							{
								database.AddInParameter(dbCommand, "@ColumnFont", DbType.Binary, DBNull.Value);
							}
							else
							{
								database.AddInParameter(dbCommand, "@ColumnFont", DbType.Binary, FConvert.ObjectToByteArray(pageBody2.ColumnFont));
							}
							num5 = 4;
							num4 = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
							if (pageBody2.Condition != null)
							{
								empty = SaveAreaFilter(num4, pageBody2.Condition, iCompId, bIsBodyColumn: true);
								dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty.ToString()) : empty.ToString());
								database.ExecuteNonQuery(dbCommand, dbTransaction);
							}
							stringBuilder = new StringBuilder();
							stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceNumericProperty{0} (iHeaderId, iPageId, iAlignment, \r\n                                    iDecimalInColumn, iRoundOffType, fRoundOffValue, fFontSize, sSuffix, sPrefix) \r\n                                    VALUES ({1}, {2}, {3}, {4}, {5}, {6}, {7}, N'{8}', N'{9}');", suffix, pageHeader2.UID, num3, (byte)pageBody2.Alignment, pageBody2.DecimalInColumn, (byte)pageBody2.RoundOffType, pageBody2.RoundUptoValue, pageBody2.FontSize, pageBody2.Suffix, pageBody2.Prefix);
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
							num5 = 4;
							database.ExecuteNonQuery(dbCommand, dbTransaction);
						}
						empty = GetBodyPropertiesString(pageHeader2.PageBodyClass, pageHeader2.UID, num3, iCompId);
						if (empty.Length > 0)
						{
							dbCommand.Parameters.Clear();
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
							num5 = 5;
							database.ExecuteNonQuery(dbCommand, dbTransaction);
						}
					}
				}
				if (info.AttachDocuments != null && info.AttachDocuments.Length != 0)
				{
					stringBuilder = new StringBuilder();
					IdNamePair[] attachDocuments = info.AttachDocuments;
					foreach (IdNamePair idNamePair in attachDocuments)
					{
						stringBuilder.Append($"insert into cCore_InvoiceAttachments{suffix}(iLayoutId,iFieldId) Values({info.Layout.ID},{idNamePair.ID})");
					}
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
					num5 = 6;
					database.ExecuteScalar(dbCommand, dbTransaction);
				}
				new RDReport().SaveUserRestriction(info.Layout.ID, bUserOrRole: false, info.Users, database, dbTransaction, bNewData, LayoutSecurityType.Invoice);
				dbTransaction.Commit();
			}
			catch (Exception ex)
			{
				m_sError = ex.Message;
				dbTransaction.Rollback();
				return $"!{num5}{ex.Message}";
			}
			finally
			{
				if (dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
			return info.Layout.ID.ToString();
		}
	}

	private string SaveAreaFilter(int iFilterId, _Filter[] arrFilter, int iCompanyId, bool bIsBodyColumn)
	{
		byte b = 0;
		string text = null;
		if (arrFilter != null && arrFilter.Length != 0)
		{
			text = string.Empty;
			for (b = 0; b < arrFilter.Length; b++)
			{
				text += $"INSERT INTO cCore_ReportFilter{FConvert.GetSuffix(iCompanyId)}\r\n                        (iFilterGroupId, iFieldId, iOperator, sValue, iConjunction, iCompareWith, iType, iDataType) \r\n                        VALUES ({iFilterId}, {arrFilter[b].FieldId}, {(byte)arrFilter[b].Operator}, '{arrFilter[b].CompareValue}', {(byte)arrFilter[b].Conjuction}, {(byte)arrFilter[b].CompareWith}, {(byte)(bIsBodyColumn ? 4 : 3)}, {(byte)arrFilter[b].DataType});";
			}
		}
		return text;
	}

	public LayoutInformation LoadPrintingInvoice(int iLayoutId, int iCompId)
	{
		Database database = null;
		LayoutInformation layoutInformation = null;
		List<Page> list = null;
		List<PageHeader> list2 = null;
		List<PageBody> list3 = null;
		Page page = null;
		PageHeader pageHeader = null;
		PageBody pageBody = null;
		List<IdNamePair> list4 = null;
		string text = null;
		string suffix = FConvert.GetSuffix(iCompId);
		int num = -1;
		int num2 = 0;
		int num3 = 0;
		try
		{
			layoutInformation = new LayoutInformation();
			layoutInformation.Layout = new ComboData();
			text = $"SELECT iReportId,ISNULL(iSubReportId,0)[iSubReportId],sLayout,fPageWidth,fPageHeight,fLeftMargin,fTopMargin,\r\n                fRightMargin,fBottomMargin,iPageUnit,iPageView, iModule, iReportType, isnull(sPrinter,'')[sPrinter], isnull(iModifierOption,0)[iModifierOption],\r\n                isnull(fColumnMargin,0)fColumnMargin,isnull(fColumnWidth,0)fColumnWidth,isnull(iNoOfCopy,0)iNoOfCopy, isnull(iFlag,0)[iFlag],isnull(fColumnHeight,0)fColumnHeight,\r\n                isnull(fRowMargin,0)fRowMargin,isnull(iRowsPerPage,0)iRowsPerPage,isnull(iColumnsPerPage,0)iColumnsPerPage\r\n                FROM cCore_InvoiceLayout{suffix} WHERE iLayoutId = {iLayoutId}";
			database = DatabaseWrapper.GetDatabase2(iCompId);
			using (IDataReader dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
			{
				layoutInformation.Layout.ID = iLayoutId;
				if (dataReader.Read())
				{
					layoutInformation.ReportId = Convert.ToInt32(dataReader["iReportId"]);
					layoutInformation.SubReportId = Convert.ToInt32(dataReader["iSubReportId"]);
					layoutInformation.Layout.Name = Convert.ToString(dataReader["sLayout"]);
					layoutInformation.PrintInfo = new PrinterInfo();
					layoutInformation.PrintInfo.PageWidth = Convert.ToDouble(dataReader["fPageWidth"]);
					layoutInformation.PrintInfo.PageHeight = Convert.ToDouble(dataReader["fPageHeight"]);
					layoutInformation.PrintInfo.Margin = new Margin();
					layoutInformation.PrintInfo.Margin.Left = Convert.ToDouble(dataReader["fLeftMargin"]);
					layoutInformation.PrintInfo.Margin.Top = Convert.ToDouble(dataReader["fTopMargin"]);
					layoutInformation.PrintInfo.Margin.Right = Convert.ToDouble(dataReader["fRightMargin"]);
					layoutInformation.PrintInfo.Margin.Bottom = Convert.ToDouble(dataReader["fBottomMargin"]);
					layoutInformation.PrintInfo.Unit = (Unit)Convert.ToByte(dataReader["iPageUnit"]);
					layoutInformation.PrintInfo.View = (View)Convert.ToByte(dataReader["iPageView"]);
					layoutInformation.Module = (Module)Convert.ToByte(dataReader["iModule"]);
					layoutInformation.ReportType = (ReportList)Convert.ToByte(dataReader["iReportType"]);
					layoutInformation.DefaultPrinter = Convert.ToString(dataReader["sPrinter"]);
					if (layoutInformation.DefaultPrinter.Length == 0)
					{
						layoutInformation.DefaultPrinter = null;
					}
					layoutInformation.ModifierOption = (ModifierPrintOption)Convert.ToByte(dataReader["iModifierOption"]);
					layoutInformation.HorizontalGap = Convert.ToDouble(dataReader["fColumnMargin"]);
					layoutInformation.ColumnWidth = Convert.ToDouble(dataReader["fColumnWidth"]);
					layoutInformation.ColumnHeight = Convert.ToDouble(dataReader["fColumnHeight"]);
					layoutInformation.VerticalGap = Convert.ToDouble(dataReader["fRowMargin"]);
					layoutInformation.RowsPerPage = Convert.ToByte(dataReader["iRowsPerPage"]);
					layoutInformation.ColumnsPerPage = Convert.ToByte(dataReader["iColumnsPerPage"]);
					layoutInformation.NoOfCopies = Convert.ToByte(dataReader["iNoOfCopy"]);
					(layoutInformation.IsPrintInDraftMode, layoutInformation.IsSuspendNet, layoutInformation.IsDonotPrintQuantityforModifiers, layoutInformation.PrintApprovalHistoryAtTheEnd, layoutInformation.PageNumberOption) = SetFlagValue(Convert.ToInt64(dataReader["iFlag"]));
				}
				dataReader.Close();
			}
			text = $"select iFieldId from cCore_InvoiceAttachments{suffix} where iLayoutId = {layoutInformation.Layout.ID}";
			using (IDataReader dataReader2 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
			{
				list4 = new List<IdNamePair>();
				while (dataReader2.Read())
				{
					list4.Add(new IdNamePair(Convert.ToInt32(dataReader2["iFieldId"]), null));
				}
				dataReader2.Close();
			}
			layoutInformation.AttachDocuments = list4.ToArray();
			list = new List<Page>();
			text = string.Format("SELECT cCore_InvoicePage{0}.iPageId,ISNull(cCore_InvoicePage{0}.bIsPageAfterPreviousPage,0)[bIsPageAfterPreviousPage],cCore_InvoiceHeader{0}.iHeaderId,iControlType,sText,iCategoryId,cCore_InvoiceHeader{0}.iFieldId\r\n                ,fLeft,fTop,fWidth,fHeight, fBorderThickness,iBorderColor,iBackColor,iTextColor,sTextFont,cCore_InvoiceHeader{0}.fFontSize,iFontWeights,iFontStyle,imgControl,iVariable, iShowHeader,\r\n                ISNULL(iSubParentId,-1)[iSubParentId], ISNULL(iFontEffect,0)[iFontEffect], ISNULL(iValue,0)[iValue]\r\n                ,ISNULL(cCore_InvoiceNumericProperty{0}.iAlignment,0)[iAlignment],ISNULL(cCore_InvoiceNumericProperty{0}.iDecimalInColumn,0)[iDecimalInColumn], ISNULL(iRoundOffType,0)[iRoundOffType]\r\n                ,ISNULL(fRoundOffValue, 0)[fRoundOffValue],isnull(bInsertCommas,0)[bInsertCommas],ISNULL(iFunctionType,0)[iFunctionType], ISNULL(sSuffix,'')[sSuffix], \r\n                ISNULL(iAmtInWords,0)[iAmtInWords], ISNULL(iSign,0)[iSign], ISNULL(iCaseStyle,0)[iCaseStyle], ISNULL(sLabelforZeroVal,'')[sLabelforZeroVal], ISNULL(iDataType,0)[iDataType]\r\n                ,ISNULL(bArabicDigit,0)[bArabicDigit], ISNULL(iPixelInLine,0)[iPixelInLine], ISNULL(cCore_InvoiceNumericProperty{0}.sPrefix,'')[sPrefix]\r\n                FROM cCore_InvoiceHeader{0} \r\n                JOIN cCore_InvoicePage{0} ON cCore_InvoiceHeader{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                LEFT JOIN cCore_InvoiceBodyValue{0} ON cCore_InvoiceBodyValue{0}.iPageId = cCore_InvoicePage{0}.iPageId \r\n                    AND cCore_InvoiceBodyValue{0}.iHeaderId = cCore_InvoiceHeader{0}.iHeaderId AND cCore_InvoiceBodyValue{0}.iFieldId = {2}\r\n                LEFT JOIN cCore_InvoiceNumericProperty{0} ON cCore_InvoiceNumericProperty{0}.iPageId = cCore_InvoicePage{0}.iPageId \r\n                    AND cCore_InvoiceNumericProperty{0}.iHeaderId = cCore_InvoiceHeader{0}.iHeaderId AND cCore_InvoiceNumericProperty{0}.iHeaderId > 2000\r\n                WHERE cCore_InvoicePage{0}.iLayoutId = {1} \r\n                ORDER BY cCore_InvoicePage{0}.iPageId,fTop,fLeft;", suffix, iLayoutId, 15);
			using (IDataReader dataReader3 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
			{
				list2 = new List<PageHeader>();
				while (dataReader3.Read())
				{
					if (num != Convert.ToInt32(dataReader3["iPageId"]))
					{
						if (page != null)
						{
							page.PageHeader = list2.ToArray();
							list.Add(page);
							list2.Clear();
						}
						page = new Page();
						num = (page.PageId = Convert.ToInt32(dataReader3["iPageId"]));
						page.IsPageAfterPreviousPage = Convert.ToBoolean(dataReader3["bIsPageAfterPreviousPage"]);
					}
					pageHeader = new PageHeader();
					pageHeader.UID = Convert.ToInt32(dataReader3["iHeaderId"]);
					pageHeader.Type = (ControlType)Convert.ToInt32(dataReader3["iControlType"]);
					pageHeader.Text = Convert.ToString(dataReader3["sText"]);
					pageHeader.MasterId = Convert.ToInt32(dataReader3["iCategoryId"]);
					pageHeader.FieldId = Convert.ToInt32(dataReader3["iFieldId"]);
					pageHeader.SubParentId = Convert.ToInt32(dataReader3["iSubParentId"]);
					pageHeader.Left = Convert.ToDouble(dataReader3["fLeft"]);
					pageHeader.Top = Convert.ToDouble(dataReader3["fTop"]);
					pageHeader.Width = Convert.ToDouble(dataReader3["fWidth"]);
					pageHeader.Height = Convert.ToDouble(dataReader3["fHeight"]);
					pageHeader.BorderThickness = Convert.ToDouble(dataReader3["fBorderThickness"]);
					pageHeader.BorderColor = Convert.ToInt32(dataReader3["iBorderColor"]);
					pageHeader.BackColor = Convert.ToInt32(dataReader3["iBackColor"]);
					pageHeader.TextColor = Convert.ToInt32(dataReader3["iTextColor"]);
					pageHeader.TextFont = Convert.ToString(dataReader3["sTextFont"]);
					pageHeader.FontSize = Convert.ToDouble(dataReader3["fFontSize"]);
					pageHeader.FontWeight = Convert.ToInt16(dataReader3["iFontWeights"]);
					pageHeader.FontStyle = Convert.ToInt16(dataReader3["iFontStyle"]);
					pageHeader.FontEffect = Convert.ToInt16(dataReader3["iFontEffect"]);
					pageHeader.ImageSource = ((dataReader3["imgControl"] == DBNull.Value) ? null : ((byte[])dataReader3["imgControl"]));
					pageHeader.VariableType = (VariableType)Convert.ToInt16(dataReader3["iVariable"]);
					pageHeader.ShowOnPage = (ShowOnPage)Convert.ToInt16(dataReader3["iShowHeader"]);
					pageHeader.WordWrap = Convert.ToBoolean(dataReader3["iValue"]);
					pageHeader.IsArabicDigit = Convert.ToBoolean(dataReader3["bArabicDigit"]);
					pageHeader.StaticTextProperties = new StaticTextClass();
					PageHeader pageHeader2 = pageHeader;
					byte alignment = (pageHeader.StaticTextProperties.Alignment = Convert.ToByte(dataReader3["iAlignment"]));
					pageHeader2.Alignment = alignment;
					PageHeader pageHeader3 = pageHeader;
					alignment = (pageHeader.StaticTextProperties.DecimalInColumn = Convert.ToByte(dataReader3["iDecimalInColumn"]));
					pageHeader3.DecimalInColumn = alignment;
					pageHeader.StaticTextProperties.RoundOffType = (RoundingType)Convert.ToByte(dataReader3["iRoundOffType"]);
					pageHeader.StaticTextProperties.RoundUptoValue = Convert.ToDouble(dataReader3["fRoundOffValue"]);
					pageHeader.StaticTextProperties.InsertComma = Convert.ToBoolean(dataReader3["bInsertCommas"]);
					pageHeader.StaticTextProperties.AmountInWords = (AmountInWordsType)Convert.ToByte(dataReader3["iAmtInWords"]);
					pageHeader.StaticTextProperties.CaseStyle = (CaseStyle)Convert.ToByte(dataReader3["iCaseStyle"]);
					pageHeader.StaticTextProperties.FunctionType = (InvoiceFunction)Convert.ToByte(dataReader3["iFunctionType"]);
					pageHeader.StaticTextProperties.SuffixForAmtInWords = Convert.ToString(dataReader3["sSuffix"]);
					pageHeader.StaticTextProperties.Labelforzerovalue = Convert.ToString(dataReader3["sLabelforZeroVal"]);
					pageHeader.StaticTextProperties.Sign = (Sign)Convert.ToByte(dataReader3["iSign"]);
					pageHeader.StaticTextProperties.DataType = (MasterDataType)Convert.ToByte(dataReader3["iDataType"]);
					pageHeader.StaticTextProperties.IsArabicDigit = Convert.ToBoolean(dataReader3["bArabicDigit"]);
					pageHeader.StaticTextProperties.PixelInLinesWordWrap = Convert.ToInt32(dataReader3["iPixelInLine"]);
					pageHeader.StaticTextProperties.Prefix = Convert.ToString(dataReader3["sPrefix"]);
					pageHeader.IsImageBackground = ((pageHeader.VariableType == VariableType.Image) ? 1 : 0);
					list2.Add(pageHeader);
				}
				dataReader3.Close();
			}
			if (page != null)
			{
				page.PageHeader = list2.ToArray();
				list.Add(page);
			}
			for (int i = 0; i < list.Count; i++)
			{
				list2 = new List<PageHeader>();
				list2.AddRange(list[i].PageHeader);
				for (num2 = 0; num2 < list2.Count; num2++)
				{
					switch (list2[num2].Type)
					{
					case ControlType.Area:
					{
						text = string.Format("SELECT iDisplayOnPage,bAutoExpand,sCondition,bDisplayBorder,ISNULL(bIgnorePageAfterPreviousPage,0)[bIgnorePageAfterPreviousPage],iAreaPositionifBodyskip\r\n                                    FROM cCore_InvoiceAreaControl{0} \r\n                                    JOIN cCore_InvoicePage{0} ON cCore_InvoiceAreaControl{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                                    WHERE iLayoutId = {1} AND iHeaderId = {2} AND cCore_InvoiceAreaControl{0}.iPageId = {3}", suffix, iLayoutId, list2[num2].UID, list[i].PageId);
						using (IDataReader dataReader7 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
						{
							if (dataReader7.Read())
							{
								list2[num2].AreaProperties = new AreaClass();
								list2[num2].AreaProperties.AutoExpand = Convert.ToBoolean(dataReader7["bAutoExpand"]);
								list2[num2].AreaProperties.DisplayBorder = Convert.ToBoolean(dataReader7["bDisplayBorder"]);
								list2[num2].AreaProperties.Condition = Convert.ToString(dataReader7["sCondition"]);
								list2[num2].AreaProperties.PageSelect = (PageList)Convert.ToInt32(dataReader7["iDisplayOnPage"]);
								list2[num2].AreaProperties.IgnorePageAfterPreviousPage = Convert.ToBoolean(dataReader7["bIgnorePageAfterPreviousPage"]);
								list2[num2].AreaProperties.AreaPositionIfBodySkip = Convert.ToInt32(dataReader7["iAreaPositionifBodyskip"]);
								list2[num2].AreaProperties.Format = new _Format();
								list2[num2].AreaProperties.Format.Conditions = LoadReportFilter(list2[num2].UID, database, iCompId, bIsbodyColumn: false);
							}
							dataReader7.Close();
						}
						break;
					}
					case ControlType.Table:
					{
						text = string.Format("SELECT iFieldId, iValue \r\n                            FROM cCore_InvoiceBodyValue{0}\r\n                            JOIN cCore_InvoicePage{0} ON cCore_InvoiceBodyValue{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                            WHERE iLayoutId = {1} AND iHeaderId = {2} AND iFieldId IN({3}, {4}, {5}, {6}) ", suffix, iLayoutId, list2[num2].UID, 38, 9, 8, 17);
						using (IDataReader dataReader6 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
						{
							list2[num2].TableProperties = new TableClass();
							while (dataReader6.Read())
							{
								switch ((InvoiceField)Convert.ToInt32(dataReader6["iFieldId"]))
								{
								case InvoiceField.TextAlignment:
									list2[num2].TableProperties.Align = (TextOrientation)dataReader6["iValue"];
									break;
								case InvoiceField.TotalColumn:
									list2[num2].TableProperties.TotalColumn = Convert.ToInt32(dataReader6["iValue"]);
									break;
								case InvoiceField.TotalRow:
									list2[num2].TableProperties.TotalRow = Convert.ToInt32(dataReader6["iValue"]);
									break;
								case InvoiceField.IsEditable:
									list2[num2].TableProperties.IsEditable = Convert.ToBoolean(dataReader6["iValue"]);
									break;
								}
							}
							dataReader6.Close();
						}
						break;
					}
					case ControlType.BodyCanvas:
					{
						text = string.Format("SELECT iId,iCategoryId,iFieldId,sColumn,sAlias,iDataType,fColumnWidth,iDisplayColumnIndex,\r\n                            iSign,bWordWrap,bPrintUnderPrevColumn, ISNULL(sFormula,'')[sFormula],ISNULL(iSubParentId,-1)[iSubParentId],\r\n                            ISNULL(bDontShowTotal,0)[bDontShowTotal],ISNULL(iHeadingAlignment,0)[iHeadingAlignment], \r\n                            ISNULL(bInsertCommas,0)[bInsertCommas],ISNULL(bPrintInLine2,0)[bPrintInLine2], ISNULL(bHideColumn,0)[bHideColumn],byFont,ISNULL(sHeading2,'')[sHeading2],\r\n                            ISNULL(iFunction,0)[iFunction],ISNULL(sGroupName,'')[sGroupName],ISNULL(sSuffix,'')[sSuffix],ISNULL(bArabicDigit,0)[bArabicDigit]\r\n                            FROM cCore_InvoiceBody{0} \r\n                            JOIN cCore_InvoicePage{0} ON cCore_InvoiceBody{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                            WHERE cCore_InvoicePage{0}.iLayoutId = {1} AND iBodyId = {2} ORDER BY iDisplayColumnIndex", suffix, iLayoutId, list2[num2].UID);
						using (IDataReader dataReader4 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
						{
							list3 = new List<PageBody>();
							while (dataReader4.Read())
							{
								pageBody = new PageBody();
								pageBody.BodyId = list2[num2].UID;
								pageBody.MasterId = Convert.ToInt32(dataReader4["iCategoryId"]);
								pageBody.SubParentId = Convert.ToInt32(dataReader4["iSubParentId"]);
								pageBody.FieldId = Convert.ToInt32(dataReader4["iFieldId"]);
								pageBody.Column = Convert.ToString(dataReader4["sColumn"]);
								pageBody.Alias = Convert.ToString(dataReader4["sAlias"]);
								pageBody.DataType = Convert.ToInt16(dataReader4["iDataType"]);
								pageBody.ColumnWidth = Convert.ToDouble(dataReader4["fColumnWidth"]);
								pageBody.ColumnIndex = Convert.ToInt32(dataReader4["iDisplayColumnIndex"]);
								pageBody.Sign = (Sign)Convert.ToInt32(dataReader4["iSign"]);
								pageBody.WordWrap = Convert.ToBoolean(dataReader4["bWordWrap"]);
								pageBody.DontShowTotal = Convert.ToBoolean(dataReader4["bDontShowTotal"]);
								pageBody.InsertCommas = Convert.ToBoolean(dataReader4["bInsertCommas"]);
								pageBody.PrintUnderPreviousColumn = Convert.ToBoolean(dataReader4["bPrintUnderPrevColumn"]);
								pageBody.Formula = Convert.ToString(dataReader4["sFormula"]);
								pageBody.HeadingAlignment = (TextAlignment)Convert.ToByte(dataReader4["iHeadingAlignment"]);
								pageBody.PrintInLine2 = Convert.ToBoolean(dataReader4["bPrintInLine2"]);
								pageBody.HideColumn = Convert.ToBoolean(dataReader4["bHideColumn"]);
								pageBody.IsArabicDigit = Convert.ToBoolean(dataReader4["bArabicDigit"]);
								if (dataReader4["byFont"] != DBNull.Value && ((byte[])dataReader4["byFont"]).Length > 10)
								{
									pageBody.ColumnFont = (FontClass)FConvert.ByteArrayToObject((byte[])dataReader4["byFont"]);
								}
								if (dataReader4["iId"] != DBNull.Value)
								{
									pageBody.Condition = LoadReportFilter(Convert.ToInt32(dataReader4["iId"]), database, iCompId, bIsbodyColumn: true);
								}
								if (pageBody.Condition != null && pageBody.Condition.Length == 0)
								{
									pageBody.Condition = null;
								}
								pageBody.Heading2 = Convert.ToString(dataReader4["sHeading2"]);
								pageBody.FunctionType = (InvoiceFunction)Convert.ToInt32(dataReader4["iFunction"]);
								pageBody.GroupName = Convert.ToString(dataReader4["sGroupName"]);
								pageBody.Suffix = Convert.ToString(dataReader4["sSuffix"]);
								list3.Add(pageBody);
							}
							dataReader4.Close();
						}
						text = string.Format("SELECT ISNULL(iAlignment,0)[iAlignment],ISNULL(iDecimalInColumn,0)[iDecimalInColumn],ISNULL(iRoundOffType,0)[iRoundOffType],\r\n                                    ISNULL(fRoundOffValue, 0)[fRoundOffValue],ISNULL(fFontSize, 0)[fFontSize], ISNULL(sSuffix, '')[sSuffix], ISNULL(sPrefix, '')[sPrefix]\r\n                                    FROM cCore_InvoiceNumericProperty{0} JOIN cCore_InvoicePage{0} ON  cCore_InvoiceNumericProperty{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                                    WHERE iLayoutId = {1} AND iHeaderId = {2}", suffix, iLayoutId, list2[num2].UID);
						using (IDataReader dataReader5 = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text))
						{
							num3 = 0;
							while (dataReader5.Read())
							{
								if (num3 < list3.Count)
								{
									list3[num3].Alignment = (TextAlignment)Convert.ToInt32(dataReader5["iAlignment"]);
									list3[num3].DecimalInColumn = Convert.ToInt32(dataReader5["iDecimalInColumn"]);
									list3[num3].RoundOffType = (RoundingType)Convert.ToByte(dataReader5["iRoundOffType"]);
									list3[num3].RoundUptoValue = Convert.ToDouble(dataReader5["fRoundOffValue"]);
									list3[num3].FontSize = Convert.ToDouble(dataReader5["fFontSize"]);
									list3[num3].Suffix = Convert.ToString(dataReader5["sSuffix"]);
									list3[num3].Prefix = Convert.ToString(dataReader5["sPrefix"]);
								}
								num3++;
							}
							dataReader5.Close();
						}
						list2[num2].PageBodyClass = LoadBodyProperties(list2[num2].UID, iLayoutId, database, suffix);
						list2[num2].PageBody = list3.ToArray();
						break;
					}
					}
				}
				list[i].PageHeader = list2.ToArray();
			}
			layoutInformation.Pages = list.ToArray();
			bool bUserOrRole = true;
			layoutInformation.Users = new RDReport().LoadUserSecurity(iLayoutId, LayoutSecurityType.Invoice, database, ref bUserOrRole);
		}
		catch (Exception ex)
		{
			layoutInformation.Layout.ID = ERRORNO;
			layoutInformation.Layout.Name = (m_sError = ex.Message);
		}
		return layoutInformation;
	}

	public string DeletePrintInvoiceLayout(int iId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = database.CreateConnection();
		DbTransaction dbTransaction = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		StringBuilder stringBuilder = null;
		string empty = string.Empty;
		string text = string.Empty;
		string suffix = FConvert.GetSuffix(iCompId);
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				dbTransaction = dbConnection.BeginTransaction();
				dbCommand = database.DbProviderFactory.CreateCommand();
				dbCommand.Connection = dbConnection;
				dbCommand.CommandType = CommandType.Text;
				dbCommand.Transaction = dbTransaction;
				stringBuilder = new StringBuilder();
				empty = string.Format("SELECT iPageId FROM cCore_InvoicePage{1} WHERE iLayoutId={0};", iId, suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					text += ((text.Length == 0) ? string.Empty : ",");
					text += dataReader[0].ToString();
				}
				dataReader.Close();
				text = ((text.Length == 0) ? "0" : text);
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceNumericProperty{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceBody{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceHeader{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoicePage{1} WHERE iLayoutId = {0};", iId, suffix));
				if (m_bDeleteLayout)
				{
					stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceLayout{1} WHERE iLayoutId = {0};", iId, suffix));
				}
				dbCommand = database.DbProviderFactory.CreateCommand();
				dbCommand.Connection = dbConnection;
				dbCommand.CommandType = CommandType.Text;
				dbCommand.Transaction = dbTransaction;
				dbCommand.CommandText = (PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
				dbCommand.ExecuteNonQuery();
				dbTransaction.Commit();
			}
			catch (Exception ex)
			{
				m_sError = ex.Message;
				dbTransaction.Rollback();
				return ex.Message;
			}
			finally
			{
				if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
		}
		return string.Empty;
	}

	public object EvaluateInvoiceFormula(string sFormula, int iRowIndex, Transaction oTranData, StaticTextClass objTextClass, string[] arrCellValue, int iCompId)
	{
		int num = 0;
		string text = null;
		List<object> list = null;
		Database database = null;
		RDConversion rDConversion = null;
		Focus.Transactions.BL.Transactions transactions = null;
		List<string> list2 = new List<string>();
		database = DatabaseWrapper.GetDatabase2(iCompId);
		transactions = new Focus.Transactions.BL.Transactions();
		transactions.m_iCompId = iCompId;
		rDConversion = new RDConversion(m_objCalType);
		if (objTextClass != null && objTextClass.FunctionType > InvoiceFunction.None)
		{
			if (oTranData != null && oTranData.BodyData != null && oTranData.Header != null)
			{
				list = new List<object>();
				transactions.InitializeScreenList(iCompId, oTranData.Header.VoucherType, database);
				for (iRowIndex = 0; iRowIndex < oTranData.BodyData.Length; iRowIndex++)
				{
					list.Add(transactions.EvaluateBodyFormula(sFormula, iRowIndex, oTranData, database));
				}
				text = rDConversion.GetAggregateValue(list.ToArray(), objTextClass);
			}
		}
		else if (oTranData != null && oTranData.BodyData != null && oTranData.Header != null)
		{
			if (sFormula.ToLower().StartsWith("sqlsf"))
			{
				num = Convert.ToInt32(sFormula.Substring(5));
				TransBody[] bodyData = oTranData.BodyData;
				for (iRowIndex = 0; iRowIndex < bodyData.Length; iRowIndex++)
				{
					oTranData.BodyData = new TransBody[1] { bodyData[iRowIndex] };
					list2.Add(transactions.GetSqlFnText(oTranData, num, database));
				}
				text = string.Join('~'.ToString(), list2.ToArray());
				oTranData.BodyData = bodyData;
			}
			else
			{
				List<IdNamePair> list3 = new List<IdNamePair>();
				list3.Add(new IdNamePair(-1, "rowid", -1));
				if (arrCellValue != null)
				{
					for (int i = 0; i < arrCellValue.Length; i++)
					{
						list3.Add(new IdNamePair(i + 1, $"c{i + 1}", arrCellValue[i]));
					}
				}
				transactions.InitializeScreenList(iCompId, oTranData.Header.VoucherType, database);
				if (sFormula.ToLower().StartsWith("l") || sFormula.ToLower().StartsWith("tl") || sFormula.ToLower().Contains("lh"))
				{
					transactions.InitializeLayout(iCompId, oTranData.Header.VoucherType, database);
				}
				if (iRowIndex >= 0)
				{
					text = transactions.EvaluateBodyFormulaWithParam(sFormula, iRowIndex, oTranData, database, list3).ToString();
				}
				else
				{
					for (iRowIndex = 0; iRowIndex < oTranData.BodyData.Length; iRowIndex++)
					{
						text = transactions.EvaluateBodyFormulaWithParam(sFormula, iRowIndex, oTranData, database, list3).ToString();
						if (text == "NaN" || text.Contains("Infinity") || text.Contains("E+") || text.Contains("E-"))
						{
							text = "0";
						}
						list2.Add(text);
					}
					text = string.Join('~'.ToString(), list2.ToArray());
				}
			}
		}
		if (!string.IsNullOrEmpty(text) && FConvert.IsNumeric(text) && objTextClass.AmountInWords != AmountInWordsType.None)
		{
			bool flag = Convert.ToBoolean(_focus.company(iCompId).getPreferenceValue(PreferenceCategories.Misc, 12));
			switch (objTextClass.AmountInWords)
			{
			case AmountInWordsType.English:
				text = rDConversion.GetAmountInWords(text, 0, objTextClass, !flag);
				break;
			case AmountInWordsType.EnglishWithCurrency:
			case AmountInWordsType.ArabicWithCurrency:
			{
				int num2 = 0;
				CurrencyDetail oCurrDetail = null;
				if (oTranData != null && oTranData.BodyData != null && iRowIndex < oTranData.BodyData.Length)
				{
					num2 = oTranData.BodyData[iRowIndex].CurrencyId;
				}
				if (num2 > 0)
				{
					oCurrDetail = new InvoiceLayout
					{
						m_objCalType = m_objCalType
					}.LoadCurrencyData(num2, iCompId);
				}
				text = rDConversion.GetAmountInWords(text, (objTextClass.AmountInWords != AmountInWordsType.EnglishWithCurrency) ? 1 : 0, oCurrDetail, objTextClass, !flag);
				break;
			}
			case AmountInWordsType.Arabic:
				text = rDConversion.GetAmountInWords(text, 1, objTextClass);
				break;
			}
		}
		return text;
	}

	public string GetMasterValueFromField(int iFieldId, int iSubParentid, string sFilter, int iLanguageId, int iCompId)
	{
		IdValuePair[] mastersValueFromFields = GetMastersValueFromFields(iFieldId, iSubParentid, sFilter, iLanguageId, iCompId);
		if (mastersValueFromFields != null && mastersValueFromFields.Length != 0)
		{
			return Convert.ToString(mastersValueFromFields[0].Value);
		}
		return string.Empty;
	}

	public IdValuePair[] GetMastersValueFromFields(int iFieldId, int iSubParentid, string sFilter, int iLanguageId, int iCompId)
	{
		FieldInfoInput oInput = new FieldInfoInput
		{
			FieldId = iFieldId,
			SubParentId = iSubParentid,
			AltLanguageId = iLanguageId,
			MasterIds = sFilter
		};
		return GetMastersValueFromFields(oInput, iCompId);
	}

	public IdValuePair[] GetMastersValueFromFields(FieldInfoInput oInput, int iCompId)
	{
		int num = oInput.FieldId;
		int num2 = oInput.SubParentId;
		string masterIds = oInput.MasterIds;
		int num3 = oInput.AltLanguageId;
		string text = null;
		string text2 = null;
		string suffix = FConvert.GetSuffix(iCompId);
		string[] array = null;
		MasterDataType masterDataType = MasterDataType.Text;
		InvoiceLayout invoiceLayout = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdValuePair> list = null;
		Database database = null;
		database = DatabaseWrapper.GetDatabase2(iCompId);
		list = new List<IdValuePair>();
		if ((num & 0xFF0000) >> 16 == 128)
		{
			text2 = GetMasterLevelField((num2 == 3 || num2 == 4) ? 1 : num2, num & 0xFFFF, database);
			switch (num2)
			{
			case 1:
			case 3:
			case 4:
				text = "SELECT iMasterId," + text2 + "[sName] FROM vrCore_Account WHERE iMasterId IN(" + masterIds + ") AND iTreeId = 0";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
				}
				dataReader.Close();
				masterIds = string.Join('~'.ToString(), list);
				break;
			case 2:
				if (text2.StartsWith("ItemLevelMaster"))
				{
					text = ((!text2.StartsWith("ItemLevelMasterCode")) ? ("SELECT iMasterId," + text2 + "[sName] FROM dbo.[fCore_GetProductGroupNames]() ItemLevelMaster WHERE iMasterId IN(" + masterIds + ")") : ("SELECT iMasterId," + text2 + "[sName] FROM dbo.[fCore_GetProductGroupCodes]() ItemLevelMasterCode WHERE iMasterId IN(" + masterIds + ")"));
				}
				else
				{
					text = "SELECT iMasterId," + text2 + "[sName] FROM vrCore_Product WHERE iMasterId IN(" + masterIds + ") AND iTreeId = 0";
				}
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
				}
				dataReader.Close();
				masterIds = string.Join('~'.ToString(), list);
				break;
			}
		}
		else if (num3 == -3 && num == 130)
		{
			text = "SELECT DISTINCT iId[iMasterId],sSkidNo[sName] FROM tCore_Skid" + suffix + " WHERE iId IN(" + masterIds + ")";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
			}
			dataReader.Close();
			masterIds = string.Join('~'.ToString(), list);
		}
		else if (num3 == -2 && (num == 10 || num == 14))
		{
			if (num == 14)
			{
				switch ((CurrencyBodyPart)num2)
				{
				case CurrencyBodyPart.CurrencyConnector:
					text2 = "sConnector";
					break;
				case CurrencyBodyPart.CurrencySubUnit:
					text2 = "sCurrencySubUnit";
					break;
				case CurrencyBodyPart.CurrencyUnit:
					text2 = "sCurrencyUnit";
					break;
				case CurrencyBodyPart.CurrencyDenominationCode:
					text2 = "sDenominationCode";
					break;
				case CurrencyBodyPart.CurrencyDenominationValue:
					text2 = "fDenominationValue";
					break;
				}
				text = ((num2 != 4 && num2 != 5) ? ("SELECT iCurrencyId[iMasterId]," + text2 + "[sName] FROM mCore_Currency WHERE iCurrencyId IN(" + masterIds + ")") : ("SELECT iCurrencyId[iMasterId]," + text2 + "[sName] FROM mCore_DenominationDetails WHERE iCurrencyId IN(" + masterIds + ")"));
			}
			else
			{
				text2 = "sCode";
				switch ((CurrencyPart)num2)
				{
				case CurrencyPart.CurrencyName:
					text2 = "sName";
					break;
				case CurrencyPart.CurrencyCoin:
					text2 = "sCoinsName";
					break;
				case CurrencyPart.CurrencyNoOfDecimal:
					text2 = "iNoOfDecimals";
					break;
				case CurrencyPart.CurrencyRoundingType:
					text2 = "iRoundingType";
					break;
				case CurrencyPart.CurrencyRoundOff:
					text2 = "fGeneralRoundOff";
					break;
				case CurrencyPart.CurrencySymbol:
					text2 = "sSymbol";
					break;
				}
				text = "SELECT iCurrencyId[iMasterId]," + text2 + "[sName] FROM mCore_Currency WHERE iCurrencyId IN(" + masterIds + ")";
			}
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
			}
			dataReader.Close();
			masterIds = string.Join('~'.ToString(), list);
		}
		else if (num3 == -5)
		{
			text = $"SELECT sExternalTableName, sExternalValueMember, sExternalDisplayMember FROM cCore_VoucherFields{suffix} WHERE iFieldId = {num & 0xFFFFFF}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			text2 = (suffix = (masterIds = null));
			while (dataReader.Read())
			{
				text2 = Convert.ToString(dataReader["sExternalTableName"]);
				suffix = Convert.ToString(dataReader["sExternalDisplayMember"]);
				masterIds = Convert.ToString(dataReader["sExternalValueMember"]);
			}
			dataReader.Close();
			if (!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(masterIds))
			{
				text = string.Format("SELECT {2}[iMasterId], {0}[sName] FROM {1} WHERE {2} = {3}", suffix, text2, masterIds, oInput.MasterIds);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
				}
				dataReader.Close();
			}
		}
		else
		{
			invoiceLayout = new InvoiceLayout();
			if (num != 127)
			{
				num2 = ((num2 == 3 || num2 == 4) ? (-1) : num2);
			}
			if (num == 24 && num2 <= 0)
			{
				text2 = "mCore_Units.sName";
			}
			else
			{
				text2 = invoiceLayout.GetExtraFieldName(num, num2, -2, bVoucherClass: false, iCompId);
				if (oInput.LanguageId > 0 && text2.Contains(".sName") && num != 127)
				{
					array = text2.Split('.');
					array[0] = array[0].Replace("vr", "m") + "Language";
					text2 = string.Join(".", array);
					num3 = oInput.LanguageId;
					num = 127;
				}
			}
			if (!string.IsNullOrEmpty(text2))
			{
				if (num3 == -4 && !text2.EndsWith("Code"))
				{
					try
					{
						array = text2.Split('.');
						text = $"SELECT iMasterId,{array[1]}sCode[sName] FROM {array[0]} WHERE iMasterId IN({masterIds})";
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
						dataReader = database.ExecuteReader(dbCommand);
						list.Clear();
						while (dataReader.Read())
						{
							list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
						}
						dataReader.Close();
						text2 = string.Empty;
					}
					catch (Exception ex)
					{
						m_sError = ex.Message;
					}
				}
				if (!string.IsNullOrEmpty(text2))
				{
					array = text2.Split('.');
					try
					{
						if ((num & 0xFF0000) >> 16 == 131 || (num & 0xFF0000) >> 16 == 132)
						{
							int value = new Date(m_objCalType).GetToday(m_objCalType).Value;
							int num4 = num & 0xFFFF;
							bool flag = (num & 0xFF0000) >> 16 == 131;
							string[] array2 = masterIds.Split(',');
							text = string.Empty;
							for (int i = 0; i < array2.Length; i++)
							{
								text += string.Format("{4} SELECT {2}[iMasterId], ISNULL(dbo.fCore_GetProductRate({2}, 0, 0, {1}, 0, 0, {3}, 0, {0}, ''),0)[sName]", num4, value, array2[i], flag ? 1 : 0, (i > 0) ? " UNION ALL" : string.Empty);
							}
						}
						else
						{
							if (array[0].StartsWith("vrCore_"))
							{
								text = $"SELECT iDataTypeId FROM cCore_MasterFields a \r\n                                    JOIN cCore_Fields b ON a.iFieldId = b.iFieldId WHERE sFieldName = '{array[1]}'";
								masterDataType = (MasterDataType)Convert.ToInt32(database.ExecuteScalar(CommandType.Text, text));
							}
							text = $"SELECT iMasterId,{array[1]}[sName] FROM {array[0]} WHERE iMasterId IN({masterIds})";
							if (num == 127)
							{
								text += $" AND iLanguageId={num3}";
							}
						}
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
						dataReader = database.ExecuteReader(dbCommand);
						while (dataReader.Read())
						{
							if (num3 == -6 || dataReader.GetFieldType(1).FullName.Contains("System.Byte[]"))
							{
								if (_focus.company(iCompId).mongoEnable)
								{
									string text3 = Convert.ToString(dataReader["sName"]);
									if (!string.IsNullOrEmpty(text3))
									{
										byte[] docFromMongo = new FMongoDb(FConvert.CompanyCodeFromId(iCompId)).GetDocFromMongo(text3);
										if (docFromMongo != null)
										{
											list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToBase64String(docFromMongo)));
										}
									}
								}
								else
								{
									list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToBase64String((byte[])dataReader["sName"])));
								}
							}
							else if (text2 == "vrCore_Product.pImage" || masterDataType == MasterDataType.Picture || num == 170)
							{
								int preferenceValue = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.DocumentStorage, 1);
								string sId = Convert.ToString(dataReader["sName"]);
								byte[] array3 = null;
								if (!string.IsNullOrEmpty(sId))
								{
									if (_focus.company(iCompId).mongoEnable)
									{
										array3 = new FMongoDb(FConvert.CompanyCodeFromId(iCompId)).GetDocFromMongo(sId);
									}
									else if ((byte)preferenceValue == 3)
									{
										SMTPSettings objMailSettings = null;
										Focus.Common.BL.Utilities utilities = new Focus.Common.BL.Utilities();
										objMailSettings = utilities.LoadMailSettings(iCompId);
										array3 = System.Threading.Tasks.Task.Run(() => OneDriveUploader.DownloadFile(objMailSettings.TenantId, objMailSettings.ClientId, objMailSettings.ClientSecret, objMailSettings.FromUserName, sId)).Result;
									}
									else if ((byte)preferenceValue == 4)
									{
										string masterFileSystemPath = GetMasterFileSystemPath(iCompId, 2, Convert.ToInt32(dataReader["iMasterId"]));
										masterFileSystemPath += sId;
										if (File.Exists(masterFileSystemPath))
										{
											array3 = File.ReadAllBytes(masterFileSystemPath);
										}
									}
								}
								if (array3 != null)
								{
									list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToBase64String(array3)));
								}
								else
								{
									list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
								}
							}
							else
							{
								list.Add(new IdValuePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"])));
							}
						}
						dataReader.Close();
						masterIds = string.Join('~'.ToString(), list);
					}
					catch (Exception ex2)
					{
						m_sError = ex2.Message;
						masterIds = string.Empty;
					}
					finally
					{
						if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
						{
							dbCommand.Connection.Close();
						}
					}
				}
			}
			else
			{
				masterIds = string.Empty;
			}
		}
		return list.ToArray();
	}

	public string GetMasterFileSystemPath(int iCompId, int iMasterTypeId, int iMasterId)
	{
		string text = new Focus.Common.BL.Utilities().GetFileStorageFolderPath();
		string text2 = FConvert.CompanyCodeFromId(iCompId);
		if (!text.EndsWith("\\"))
		{
			text += "\\";
		}
		return text + text2 + "\\Masters\\" + iMasterTypeId + "\\" + iMasterId + "\\";
	}

	public string GetBodyPropertiesString(PageBodyClass objBody, int iHeaderId, int iPageId, int iCompanyId)
	{
		StringBuilder stringBuilder = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		if (objBody != null)
		{
			stringBuilder = new StringBuilder();
			if (objBody.GroupByOption != null && objBody.GroupByOption.GroupBy != null && objBody.GroupByOption.GroupBy.Length != 0)
			{
				IdNamePair[] groupBy = objBody.GroupByOption.GroupBy;
				foreach (IdNamePair idNamePair in groupBy)
				{
					stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyGroup{0}(iHeaderId, iPageId, iFieldId, iLevelType, iGroupType) \r\n                        VALUES({1}, {2}, {3}, {4}, {5});", suffix, iHeaderId, iPageId, idNamePair.ID, (int)idNamePair.Tag, (byte)1);
				}
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 49, objBody.GroupByOption.IsNewGroupOnDiffPrinter ? 1 : 0);
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 48, objBody.GroupByOption.IsNewGroupOnNewPage ? 1 : 0);
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 50, objBody.GroupByOption.IsPrintGroupHeading ? 1 : 0);
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 51, objBody.GroupByOption.IsPrintSubTotal ? 1 : 0);
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 64, objBody.GroupByOption.GenerateHierarchy ? 1 : 0);
			}
			else if (objBody.OrderBy != null && objBody.OrderBy.Length != 0)
			{
				IdNamePair[] groupBy = objBody.OrderBy;
				foreach (IdNamePair idNamePair2 in groupBy)
				{
					stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyGroup{0}(iHeaderId, iPageId, iFieldId, iLevelType, iGroupType) \r\n                        VALUES({1}, {2}, {3}, {4}, {5});", suffix, iHeaderId, iPageId, idNamePair2.ID, (int)idNamePair2.Tag, (byte)0, idNamePair2.Name);
				}
			}
			if (objBody.ClubByOption != null)
			{
				if (objBody.ClubByOption.ClubBy != null && objBody.ClubByOption.ClubBy.Length != 0)
				{
					IdNamePair[] groupBy = objBody.ClubByOption.ClubBy;
					foreach (IdNamePair idNamePair3 in groupBy)
					{
						stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyGroup{0}(iHeaderId, iPageId, iFieldId, iLevelType, iGroupType) \r\n                        VALUES({1}, {2}, {3}, {4}, {5});", suffix, iHeaderId, iPageId, idNamePair3.ID, (int)idNamePair3.Tag, (byte)2);
					}
				}
				stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 52, objBody.ClubByOption.IsDontClubIfRateDifferent ? 1 : 0);
			}
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 53, objBody.IsBodyLengthVariable ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 26, objBody.IsDisplayPrevPageBalance ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 29, objBody.IsDoNoPrintPartialFooter ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 27, objBody.IsHideItemUnderItemSet ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 30, objBody.IsPrefixColumnNamePUPC ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 54, objBody.IsPrintTotalOnEveryPage ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 21, objBody.IsDoNotShowGridHeader ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 20, (int)objBody.ShowGridLineType);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 28, objBody.IsSkipHeaderIfFooterSpill ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 23, objBody.IsSkipPageAfterInvoice ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 11, objBody.HeightPercent);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 10, objBody.WidthPercent);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 31, objBody.BodyPositionIfHeaderSkip);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 62, objBody.LineSpacing);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, sValue) VALUES({1}, {2}, {3}, '{4}');", suffix, iHeaderId, iPageId, 58, (objBody.ExternalModuleNamespace == null) ? "" : objBody.ExternalModuleNamespace);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, sValue) VALUES({1}, {2}, {3}, '{4}');", suffix, iHeaderId, iPageId, 59, (objBody.ExternalModuleClass == null) ? "" : objBody.ExternalModuleClass);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, sValue) VALUES({1}, {2}, {3}, '{4}');", suffix, iHeaderId, iPageId, 60, (objBody.ExternalModuleFunction == null) ? "" : objBody.ExternalModuleFunction);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 55, objBody.IsSkipLineBetweenRow ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 56, objBody.IsAddBalbfToPageTotal ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 57, objBody.IsDonotPrintPartialItem ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 22, objBody.IsAlternateRowColor ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 61, objBody.IsPrintTotalInsideGrid ? 1 : 0);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 63, objBody.IsPrintGrandTotal ? 1 : 0);
		}
		return stringBuilder.ToString();
	}

	public PageBodyClass LoadBodyProperties(int iHeaderId, int iLayoyutId, Database objDb, string strSuffix)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		PageBodyClass pageBodyClass = null;
		List<IdNamePair> list = null;
		List<IdNamePair> list2 = null;
		List<IdNamePair> list3 = null;
		string text = null;
		pageBodyClass = new PageBodyClass();
		text = string.Format("SELECT iFieldId, iLevelType, iGroupType, ''[sFieldName] FROM cCore_InvoiceBodyGroup{0} \r\n                JOIN cCore_InvoicePage{0} ON cCore_InvoiceBodyGroup{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                WHERE iLayoutId = {1} AND iHeaderId = {2} ORDER BY iGroupType", strSuffix, iLayoyutId, iHeaderId);
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDb.ExecuteReader(dbCommand);
		list = new List<IdNamePair>();
		list2 = new List<IdNamePair>();
		list3 = new List<IdNamePair>();
		while (dataReader.Read())
		{
			switch ((GroupByType)Convert.ToInt32(dataReader["iGroupType"]))
			{
			case GroupByType.GroupBy:
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iFieldId"]), string.Empty, Convert.ToInt32(dataReader["iLevelType"])));
				break;
			case GroupByType.OrderBy:
				list2.Add(new IdNamePair(Convert.ToInt32(dataReader["iFieldId"]), Convert.ToString(dataReader["sFieldName"]), Convert.ToInt32(dataReader["iLevelType"])));
				break;
			case GroupByType.ClubBy:
				list3.Add(new IdNamePair(Convert.ToInt32(dataReader["iFieldId"]), string.Empty, Convert.ToInt32(dataReader["iLevelType"])));
				break;
			}
		}
		dataReader.Close();
		pageBodyClass.GroupByOption = new GroupByOptions();
		pageBodyClass.GroupByOption.GroupBy = list.ToArray();
		pageBodyClass.ClubByOption = new ClubByOptions();
		pageBodyClass.ClubByOption.ClubBy = list3.ToArray();
		pageBodyClass.OrderBy = list2.ToArray();
		text = string.Format("select iFieldId,iValue,sValue from cCore_InvoiceBodyValue{0} \r\n                JOIN cCore_InvoicePage{0} ON cCore_InvoiceBodyValue{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                WHERE iLayoutId = {1} AND iHeaderId = {2}", strSuffix, iLayoyutId, iHeaderId);
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDb.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			switch ((InvoiceField)Convert.ToInt32(dataReader["iFieldId"]))
			{
			case InvoiceField.IsNewGroupOnDiffPrinter:
				pageBodyClass.GroupByOption.IsNewGroupOnDiffPrinter = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsNewGroupOnNewPage:
				pageBodyClass.GroupByOption.IsNewGroupOnNewPage = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPrintGroupHeading:
				pageBodyClass.GroupByOption.IsPrintGroupHeading = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPrintSubTotal:
				pageBodyClass.GroupByOption.IsPrintSubTotal = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.GenerateHierarchy:
				pageBodyClass.GroupByOption.GenerateHierarchy = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsBodyLengthVariable:
				pageBodyClass.IsBodyLengthVariable = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsDisplayPrevPageBalance:
				pageBodyClass.IsDisplayPrevPageBalance = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPartialFooter:
				pageBodyClass.IsDoNoPrintPartialFooter = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsHideItemUnderItemSet:
				pageBodyClass.IsHideItemUnderItemSet = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.PrintUnderPreviousColumn:
				pageBodyClass.IsPrefixColumnNamePUPC = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPrintTotalInEveryPage:
				pageBodyClass.IsPrintTotalOnEveryPage = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.ShowHeader:
				pageBodyClass.IsDoNotShowGridHeader = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.ShowGridLine:
				pageBodyClass.ShowGridLineType = (GridLineType)Convert.ToInt32(dataReader["iValue"]);
				break;
			case InvoiceField.IsFooterSplit:
				pageBodyClass.IsSkipHeaderIfFooterSpill = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsSkipPageAfterInvoice:
				pageBodyClass.IsSkipPageAfterInvoice = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.ScaleHeightPercentage:
				pageBodyClass.HeightPercent = Convert.ToInt32(dataReader["iValue"]);
				break;
			case InvoiceField.ScaleWidthPercentage:
				pageBodyClass.WidthPercent = Convert.ToInt32(dataReader["iValue"]);
				break;
			case InvoiceField.BodyPositionIfHeaderSkip:
				pageBodyClass.BodyPositionIfHeaderSkip = Convert.ToInt32(dataReader["iValue"]);
				break;
			case InvoiceField.LineSpacing:
				pageBodyClass.LineSpacing = Convert.ToInt32(dataReader["iValue"]);
				break;
			case InvoiceField.IsDontClubIfRateDifferent:
				pageBodyClass.ClubByOption.IsDontClubIfRateDifferent = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsSkipLineBetweenRow:
				pageBodyClass.IsSkipLineBetweenRow = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsAddBalbfToPageTotal:
				pageBodyClass.IsAddBalbfToPageTotal = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsDonotPrintPartialItem:
				pageBodyClass.IsDonotPrintPartialItem = Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.ExternalModuleNamespace:
				pageBodyClass.ExternalModuleNamespace = Convert.ToString(dataReader["sValue"]);
				break;
			case InvoiceField.ExternalModuleClass:
				pageBodyClass.ExternalModuleClass = Convert.ToString(dataReader["sValue"]);
				break;
			case InvoiceField.ExternalModuleFunction:
				pageBodyClass.ExternalModuleFunction = Convert.ToString(dataReader["sValue"]);
				break;
			case InvoiceField.AlternateRowColor:
				pageBodyClass.IsAlternateRowColor = dataReader["iValue"] != DBNull.Value && Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPrintTotalInsideGrid:
				pageBodyClass.IsPrintTotalInsideGrid = dataReader["iValue"] != DBNull.Value && Convert.ToBoolean(dataReader["iValue"]);
				break;
			case InvoiceField.IsPrintGrandTotal:
				pageBodyClass.IsPrintGrandTotal = dataReader["iValue"] != DBNull.Value && Convert.ToBoolean(dataReader["iValue"]);
				break;
			}
		}
		dataReader.Close();
		text = string.Format("SELECT iFontType, byFont FROM cCore_InvoiceBodyFont{0} \r\n                JOIN cCore_InvoicePage{0} ON cCore_InvoiceBodyFont{0}.iPageId = cCore_InvoicePage{0}.iPageId\r\n                WHERE iLayoutId = {1} AND iHeaderId = {2} ORDER BY iFontType", strSuffix, iLayoyutId, iHeaderId);
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDb.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			switch ((BodyFontType)Convert.ToInt32(dataReader["iFontType"]))
			{
			case BodyFontType.Default:
				pageBodyClass.DefaultFont = (FontClass)FConvert.ByteArrayToObject((dataReader["byFont"] == DBNull.Value) ? null : ((byte[])dataReader["byFont"]));
				break;
			case BodyFontType.Heading:
				pageBodyClass.HeadingFont = (FontClass)FConvert.ByteArrayToObject((dataReader["byFont"] == DBNull.Value) ? null : ((byte[])dataReader["byFont"]));
				break;
			case BodyFontType.Total:
				pageBodyClass.TotalFont = (FontClass)FConvert.ByteArrayToObject((dataReader["byFont"] == DBNull.Value) ? null : ((byte[])dataReader["byFont"]));
				break;
			}
		}
		dataReader.Close();
		return pageBodyClass;
	}

	public string GetTableValueString(TableClass objTable, int iHeaderId, int iPageId, int iCompanyId)
	{
		StringBuilder stringBuilder = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		if (objTable != null)
		{
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 38, (int)objTable.Align);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 9, objTable.TotalColumn);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 8, objTable.TotalRow);
			stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceBodyValue{0}(iHeaderId, iPageId, iFieldId, iValue) VALUES({1}, {2}, {3}, {4});", suffix, iHeaderId, iPageId, 17, objTable.IsEditable ? 1 : 0);
		}
		return stringBuilder.ToString();
	}

	private _Filter[] LoadReportFilter(int iFilterId, Database objDb, int iCompanyId, bool bIsbodyColumn)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string text = null;
		_Filter filter = null;
		List<_Filter> list = null;
		text = $"SELECT iFieldId, iOperator, sValue, iConjunction, iCompareWith, iType, iDataType \r\n                            FROM cCore_ReportFilter{FConvert.GetSuffix(iCompanyId)} WHERE iFilterGroupId = {iFilterId} AND iType = {(bIsbodyColumn ? 4 : 3)}";
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDb.ExecuteReader(dbCommand);
		list = new List<_Filter>();
		while (dataReader.Read())
		{
			filter = new _Filter();
			filter.FieldId = Convert.ToInt32(dataReader["iFieldId"]);
			filter.Operator = (Operator)Convert.ToInt32(dataReader["iOperator"]);
			filter.CompareValue = Convert.ToString(dataReader["sValue"]);
			filter.Conjuction = (Conjuction)Convert.ToInt32(dataReader["iConjunction"]);
			filter.CompareWith = (Focus.Common.DataStructs.CompareWith)Convert.ToInt32(dataReader["iCompareWith"]);
			filter.FilterType = (_FilterType)Convert.ToInt32(dataReader["iType"]);
			filter.DataType = (MasterDataType)Convert.ToInt32(dataReader["iDataType"]);
			list.Add(filter);
		}
		dataReader.Close();
		return list.ToArray();
	}

	private long GetFlagValue(bool bDraftMode, bool bSuspendNet, bool bPrintQuantityforModifiers, bool bPrintApprovalHistoryAtTheEnd, PagNoOption oPageno)
	{
		return (int)((uint)((int)oPageno << 8) | ((bPrintApprovalHistoryAtTheEnd ? 1u : 0u) << 4) | ((bPrintQuantityforModifiers ? 1u : 0u) << 3) | ((bSuspendNet ? 1u : 0u) << 2) | ((bDraftMode ? 1u : 0u) << 1));
	}

	private (bool, bool, bool, bool, PagNoOption) SetFlagValue(long iValue)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		PagNoOption item = (PagNoOption)((iValue >> 8) & 7);
		flag3 = ((iValue >> 4) & 1) == 1;
		flag2 = ((iValue >> 3) & 1) == 1;
		flag = ((iValue >> 2) & 1) == 1;
		return (((iValue >> 1) & 1) == 1, flag, flag2, flag3, item);
	}

	private string GetSingleQuote(string sText)
	{
		if (!string.IsNullOrEmpty(sText))
		{
			return sText.Replace("'", "''");
		}
		return string.Empty;
	}

	public UserContactDetails[] GetUsersContactDetails(int iHeaderId, int iUserId, int iCompanyId)
	{
		int num = 0;
		int num2 = 0;
		Database database = null;
		IDataReader dataReader = null;
		string text = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		UserContactDetails userContactDetails = null;
		List<UserContactDetails> list = null;
		list = new List<UserContactDetails>();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = $"select distinct case when bRole = 0 then isnull(sUserName,sLoginName) else sRoleName end [sUserName] \r\n            ,case when bRole = 0 then  e.iUserId else iRoleId end [iUserId]\r\n            ,sEmail,sPhone,sMobile, case when a.iStatus <> 2 then ISNULL(biSignature,0) else 0x end[biSignature]\r\n            ,0[bAuth],iAuthDate,iAuthTime,sRemarks,a.iStatus[iStatus]\r\n            from tCore_TransAuthUsers{suffix} a WITH (READUNCOMMITTED) \r\n            join tCore_TransAuth{suffix} b WITH (READUNCOMMITTED) on a.iAuthId = b.iAuthId\r\n            join tCore_Data{suffix} c on b.iBodyId = c.iBodyId \r\n            join tCore_Header{suffix} d on c.iHeaderId = d.iHeaderId \r\n            left join mSec_Users e WITH (READUNCOMMITTED) on a.iRoleOrUserId = e.iUserId and bRole = 0\r\n            left join mSec_RoleHeader f WITH (READUNCOMMITTED) on a.iRoleOrUserId = f.iRoleId and bRole = 1\r\n            where d.iHeaderId = {iHeaderId} and (a.iStatus = 1 OR a.iStatus = 2) \r\n            union\r\n            select distinct case when bRole = 0 then isnull(sUserName,sLoginName) else sRoleName end sLoginName ,case when bRole = 0 then  e.iUserId else iRoleId end userId\r\n            ,sEmail,sPhone,sMobile,ISNULL(biSignature,0)[biSignature],1,iAuthDate,iAuthTime,sRemarks,a.iStatus[iStatus]\r\n            from tCore_TransAuthUsers{suffix} a WITH (READUNCOMMITTED)\r\n            join tCore_TransAuth{suffix} b WITH (READUNCOMMITTED) on a.iAuthId = b.iAuthId\r\n            join tCore_Data{suffix} c on b.iBodyId = c.iBodyId \r\n            join tCore_Header{suffix} d on c.iHeaderId = d.iHeaderId \r\n            left join mSec_Users e WITH (READUNCOMMITTED) on a.iRoleOrUserId = e.iUserId and bRole = 0\r\n            left join mSec_RoleHeader f WITH (READUNCOMMITTED) on a.iRoleOrUserId = f.iRoleId and bRole = 1\r\n            where b.iStatus = 0 and a.iStatus = 0 and d.iHeaderId = {iHeaderId} \r\n            order by [bAuth],iAuthDate ,iAuthTime ";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				userContactDetails = new UserContactDetails();
				userContactDetails.UserId = Convert.ToInt32(dataReader["iUserId"]);
				userContactDetails.UserName = Convert.ToString(dataReader["sUserName"]);
				userContactDetails.MobileNumber = Convert.ToString(dataReader["sMobile"]);
				userContactDetails.PhoneNumber = Convert.ToString(dataReader["sPhone"]);
				userContactDetails.EmailId = Convert.ToString(dataReader["sEmail"]);
				userContactDetails.IsNextAuthUser = Convert.ToBoolean(dataReader["bAuth"]);
				userContactDetails.IsAuthUser = Convert.ToInt32(dataReader["iStatus"]) == 1;
				userContactDetails.Status = Convert.ToByte(dataReader["iStatus"]);
				userContactDetails.Remarks = Convert.ToString(dataReader["sRemarks"]);
				userContactDetails.Signature = (byte[])dataReader["biSignature"];
				list.Add(userContactDetails);
			}
			dataReader.Close();
			text = $"SELECT iUserId, iModifiedBy FROM tCore_Header{suffix} WHERE iHeaderId = {iHeaderId}";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			if (dataReader.Read())
			{
				num = Convert.ToInt32(dataReader["iUserId"]);
				num2 = Convert.ToInt32(dataReader["iModifiedBy"]);
			}
			dataReader.Close();
			text = $"SELECT iUserId,sUserName,sEmail,sPhone,sMobile,ISNULL(biSignature,0)[biSignature], CASE WHEN iUserType = 1 THEN iLinkId ELSE 0 END[EmployeeId] \r\n            FROM mSec_Users where iUserId IN({iUserId},{num},{num2})";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				userContactDetails = new UserContactDetails();
				userContactDetails.UserId = Convert.ToInt32(dataReader["iUserId"]);
				userContactDetails.UserName = Convert.ToString(dataReader["sUserName"]);
				userContactDetails.MobileNumber = Convert.ToString(dataReader["sMobile"]);
				userContactDetails.PhoneNumber = Convert.ToString(dataReader["sPhone"]);
				userContactDetails.EmailId = Convert.ToString(dataReader["sEmail"]);
				userContactDetails.Signature = (byte[])dataReader["biSignature"];
				userContactDetails.EmployeeId = Convert.ToInt32(dataReader["EmployeeId"]);
				userContactDetails.IsModifiedByUser = userContactDetails.UserId == num2;
				userContactDetails.IsCreatedByUser = userContactDetails.UserId == num;
				userContactDetails.IsCurrentUser = userContactDetails.UserId == iUserId;
				list.Add(userContactDetails);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			list.Add(new UserContactDetails
			{
				UserId = -1,
				UserName = ex.Message
			});
		}
		return list.ToArray();
	}

	public ApprovalHistoryDetail[] GetApprovalHistory(int iHeaderId, Dictionary<string, string> arrResources, int iCompanyId)
	{
		int num = -2;
		int num2 = 0;
		string text = null;
		string text2 = string.Empty;
		string text3 = string.Empty;
		Date date = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		Database database = null;
		IDataReader objReader = null;
		List<string> list = null;
		List<ApprovalHistoryDetail> list2 = null;
		string nameFromResource = FConvert.GetNameFromResource("Submitted by", arrResources);
		string nameFromResource2 = FConvert.GetNameFromResource("Authorized by", arrResources);
		string nameFromResource3 = FConvert.GetNameFromResource("Rejected by", arrResources);
		string nameFromResource4 = FConvert.GetNameFromResource("Pending with", arrResources);
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text = string.Format("SELECT iLevel ,CASE WHEN bRole = 0 THEN ISNULL(sUserName,sLoginName) ELSE sRoleName END [sUserName]\r\n            ,MAX(iAuthDate)[iAuthDate], MAX(iAuthTime)[iAuthTime], MAX(sRemarks)[sRemarks], tCore_TransAuthUsers{0}.iStatus\r\n            FROM tCore_TransAuthUsers{0} WITH (READUNCOMMITTED) \r\n            JOIN tCore_TransAuth{0} WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iAuthId = tCore_TransAuth{0}.iAuthId\r\n            JOIN tCore_Data{0} ON tCore_TransAuth{0}.iBodyId = tCore_Data{0}.iBodyId \r\n            JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId \r\n            LEFT JOIN mSec_Users WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iRoleOrUserId = mSec_Users.iUserId and bRole = 0\r\n            LEFT JOIN mSec_RoleHeader WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iRoleOrUserId = mSec_RoleHeader.iRoleId and bRole = 1\r\n            LEFT JOIN mCore_AuthorizationDetails{0} WITH (READUNCOMMITTED) ON tCore_TransAuth{0}.iAuthNodeId  = mCore_AuthorizationDetails{0}.iAuthorizationDetailId\r\n            WHERE tCore_Header{0}.iHeaderId = {1} AND tCore_TransAuthUsers{0}.iStatus <> {2}\r\n            GROUP BY bRole , sUserName,sLoginName, sRoleName ,iLevel,tCore_TransAuthUsers{0}.iStatus\r\n            UNION ALL\r\n            SELECT -1, ISNULL(sUserName,sLoginName), iDate,tCore_Header{0}.iCreatedTime,'',0\r\n            FROM tCore_Header{0} \r\n            JOIN mSec_Users WITH (READUNCOMMITTED) ON tCore_Header{0}.iUserId = mSec_Users.iUserId \r\n            WHERE tCore_Header{0}.iHeaderId = {1}\r\n            ORDER BY iLevel DESC, iStatus DESC", suffix, iHeaderId, 4);
		objReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		list2 = new List<ApprovalHistoryDetail>();
		list = new List<string>();
		while (objReader.Read())
		{
			if (num > -2 && num != Convert.ToInt32(objReader["iLevel"]))
			{
				list2.Add(new ApprovalHistoryDetail
				{
					Status = num2 switch
					{
						1 => AprrovalStatus.Authorized, 
						0 => AprrovalStatus.Pending, 
						_ => AprrovalStatus.Rejected, 
					},
					Date = ((num2 == 0) ? string.Empty : text3),
					Level = num + 1,
					Remarks = ((num2 == 0) ? string.Empty : text2),
					User = string.Format("{0} {1}", (num == -1) ? nameFromResource : (num2 switch
					{
						2 => (object)nameFromResource3, 
						1 => nameFromResource2, 
						_ => nameFromResource4, 
					}), string.Join(",", list))
				});
				list.Clear();
				text2 = (text3 = string.Empty);
				num2 = Convert.ToByte(objReader["iStatus"]);
			}
			if (Convert.ToByte(objReader["iStatus"]) >= 0)
			{
				if (Convert.ToByte(objReader["iStatus"]) > 0)
				{
					num2 = Convert.ToByte(objReader["iStatus"]);
				}
				if (!string.IsNullOrEmpty(Convert.ToString(objReader["sRemarks"])))
				{
					text2 = Convert.ToString(objReader["sRemarks"]);
				}
				if (Convert.ToInt32(objReader["iAuthDate"]) > 0)
				{
					date = new Date(m_objCalType);
					date.Value = Convert.ToInt32(objReader["iAuthDate"]);
					text3 = string.Format("{0} {1}", date.ToString("dd/MM/yyyy"), FConvert.IntToStringTime(Convert.ToInt32(Convert.ToInt32(objReader["iAuthTime"]))));
				}
			}
			if (Convert.ToInt32(objReader["iLevel"]) == -1 && Convert.ToInt32(objReader["iAuthDate"]) > 0)
			{
				date = new Date(m_objCalType);
				date.Value = Convert.ToInt32(objReader["iAuthDate"]);
				text3 = string.Format("{0} {1}", date.ToString("dd/MM/yyyy"), FConvert.IntToStringTime(Convert.ToInt32(Convert.ToInt32(objReader["iAuthTime"]))));
			}
			if (list.Where((string p) => p == Convert.ToString(objReader["sUserName"])).Count() == 0 && (list.Count <= 0 || num2 <= 0 || Convert.ToByte(objReader["iStatus"]) != 0))
			{
				list.Add(Convert.ToString(objReader["sUserName"]));
			}
			num = Convert.ToInt32(objReader["iLevel"]);
		}
		objReader.Close();
		if (num > -2 && list.Count > 0)
		{
			list2.Add(new ApprovalHistoryDetail
			{
				Status = num2 switch
				{
					1 => AprrovalStatus.Authorized, 
					0 => AprrovalStatus.Pending, 
					_ => AprrovalStatus.Rejected, 
				},
				Date = text3,
				Level = num + 1,
				Remarks = ((num2 == 0) ? string.Empty : text2),
				User = string.Format("{0} {1}", (num == -1) ? nameFromResource : (num2 switch
				{
					2 => (object)nameFromResource3, 
					1 => nameFromResource2, 
					_ => nameFromResource4, 
				}), string.Join(",", list))
			});
		}
		return list2.ToArray();
	}

	public ApprovalHistoryDetail[] getAuthorizationHistory(int iHeaderId, Dictionary<string, string> arrResources, string sDateFormat, CalendarType oCalType, double dClientOffset, int iCompanyId)
	{
		int num = 0;
		int num2 = 0;
		int iDate = new Date(m_objCalType).GetToday(oCalType).Value;
		int num3 = 0;
		string text = null;
		string text2 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text3 = "Created name";
		string text4 = "Modified by";
		string text5 = "Created date";
		string text6 = "Modified date";
		string text7 = "Rejected date";
		string text8 = "Stopped date";
		string text9 = "Approval date";
		string text10 = "Remarks";
		string text11 = "Level";
		string text12 = "Row";
		Database database = null;
		IDataReader dataReader = null;
		RDConversion rDConversion = null;
		ApprovalHistoryDetail approvalHistoryDetail = null;
		List<ApprovalHistoryDetail> list = null;
		list = new List<ApprovalHistoryDetail>();
		try
		{
			rDConversion = new RDConversion(m_objCalType);
			rDConversion.DateFormat = sDateFormat;
			if (arrResources != null)
			{
				text3 = FConvert.GetNameFromResource("lblCreatedBy", arrResources, text3);
				text4 = FConvert.GetNameFromResource("lblModifiedBy", arrResources, text4);
				text5 = FConvert.GetNameFromResource("lblCreatedDate", arrResources, text5);
				text6 = FConvert.GetNameFromResource("lblModifiedDate", arrResources, text6);
				text7 = FConvert.GetNameFromResource("lblRejectedDate", arrResources, text7);
				text8 = FConvert.GetNameFromResource("lblStoppedDate", arrResources, text8);
				text9 = FConvert.GetNameFromResource("lblApprovalDate", arrResources, text9);
				text10 = FConvert.GetNameFromResource("lblRemarks", arrResources, text10);
				text11 = FConvert.GetNameFromResource("lblLevel", arrResources, text11);
				text12 = FConvert.GetNameFromResource("lblRow", arrResources, text12);
			}
			num = new Date().GetToday(oCalType);
			num2 = new Time(DateTime.Now).Value;
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = string.Format("SELECT iLevel, sUserName, iAuthDate, iAuthTime, sRemarks, iStatus, iSerialNo\r\n                FROM\r\n                (\r\n\t                SELECT DISTINCT CASE WHEN mCore_AuthorizationDetails{0}.bLineWiseAuthoriazation = 1 AND LEN(tCore_TransAuthUsers{0}.sRemarks) > 0 THEN iSerialNo ELSE 0 END [iSerialNo]\r\n                    ,mCore_AuthorizationDetails{0}.iLevel, \r\n                    --CASE WHEN bRole = 0 THEN ISNULL(sUserName,sLoginName) ELSE sRoleName END[sUserName], \r\n                    CASE WHEN tCore_TransAuthUsers{0}.iStatus = 4 THEN ModifiedUser.sUserName WHEN bRole = 0 THEN ISNULL(mSec_Users.sUserName,mSec_Users.sLoginName) ELSE sRoleName END[sUserName],\r\n\t                CASE WHEN tCore_TransAuthUsers{0}.iStatus = 4 THEN tCore_Header{0}.iModifiedDate WHEN tCore_TransAuthUsers{0}.iStatus = 0 THEN {2} ELSE tCore_TransAuthUsers{0}.iAuthDate END[iAuthDate],\r\n\t                CASE WHEN tCore_TransAuthUsers{0}.iStatus = 4 THEN tCore_Header{0}.iModifiedTime WHEN tCore_TransAuthUsers{0}.iStatus = 0 THEN {3} ELSE tCore_TransAuthUsers{0}.iAuthTime END[iAuthTime], \r\n                    tCore_TransAuthUsers{0}.iStatus,tCore_TransAuthUsers{0}.sRemarks\r\n\t                FROM tCore_Header{0} \r\n\t                JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId \r\n\t                JOIN tCore_TransAuth{0} WITH (READUNCOMMITTED) ON tCore_TransAuth{0}.iBodyId = tCore_Data{0}.iBodyId \r\n\t                JOIN tCore_TransAuthUsers{0} WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iAuthId = tCore_TransAuth{0}.iAuthId AND tCore_TransAuth{0}.iStatus =  tCore_TransAuthUsers{0}.iStatus\r\n\t                JOIN cCore_Vouchers{0} WITH (READUNCOMMITTED) ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n\t                LEFT JOIN mSec_Users WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iRoleOrUserId = mSec_Users.iUserId and bRole = 0\r\n                    LEFT JOIN mSec_Users ModifiedUser WITH (READUNCOMMITTED) ON tCore_Header{0}.iModifiedBy = ModifiedUser.iUserId \r\n\t                LEFT JOIN mSec_RoleHeader WITH (READUNCOMMITTED) ON tCore_TransAuthUsers{0}.iRoleOrUserId = mSec_RoleHeader.iRoleId and bRole = 1\r\n\t                LEFT JOIN mCore_AuthorizationDetails{0} WITH (READUNCOMMITTED) ON tCore_TransAuth{0}.iAuthNodeId  = mCore_AuthorizationDetails{0}.iAuthorizationDetailId \r\n\t                WHERE tCore_Header{0}.iHeaderId = {1}  \r\n\t                UNION\r\n\t                SELECT 0, -1[iLevel],ISNULL(mSec_Users.sUserName,mSec_Users.sLoginName)[sUserName] , tCore_Header{0}.iCreatedDate,\r\n\t                tCore_Header{0}.iCreatedTime,-1[iStatus],''sRemarks\r\n\t                FROM tCore_Header{0} \r\n\t                JOIN mSec_Users WITH (READUNCOMMITTED) ON tCore_Header{0}.iUserId = mSec_Users.iUserId \r\n\t                WHERE tCore_Header{0}.iHeaderId = {1}\r\n                    UNION\r\n\t                SELECT 0, -2[iLevel],ISNULL(mSec_Users.sUserName,mSec_Users.sLoginName)[sUserName] , tCore_Header{0}.iModifiedDate,\r\n\t                tCore_Header{0}.iModifiedTime,-1[iStatus],''sRemarks\r\n\t                FROM tCore_Header{0} \r\n\t                JOIN mSec_Users WITH (READUNCOMMITTED) ON tCore_Header{0}.iModifiedBy = mSec_Users.iUserId \r\n\t                WHERE tCore_Header{0}.iModifiedDate <> tCore_Header{0}.iCreatedDate AND tCore_Header{0}.iModifiedTime <> tCore_Header{0}.iCreatedTime  AND\r\n                    tCore_Header{0}.iHeaderId = {1}\r\n                )TEMP\r\n                ORDER BY iAuthDate, iAuthTime ", suffix, iHeaderId, num, num2);
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				approvalHistoryDetail = new ApprovalHistoryDetail();
				approvalHistoryDetail.Level = Convert.ToInt32(dataReader["iLevel"]);
				text2 = rDConversion.SetDateFormat(StandardDataFormat.Default, Convert.ToInt32(dataReader["iAuthDate"]), null);
				approvalHistoryDetail.User = string.Format("{1} : {0}", Convert.ToString(dataReader["sUserName"]), text3);
				num3 = Convert.ToInt32(dataReader["iAuthTime"]);
				if (dClientOffset != 0.0)
				{
					FConvert.TranslateTimeZone(ref iDate, ref num3, dClientOffset, m_objCalType);
				}
				if (approvalHistoryDetail.Level == -1)
				{
					approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text5);
					list.Insert(0, approvalHistoryDetail);
					continue;
				}
				if (approvalHistoryDetail.Level == -2)
				{
					approvalHistoryDetail.User = string.Format("{1} : {0}", Convert.ToString(dataReader["sUserName"]), text4);
					approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text6);
					list.Add(approvalHistoryDetail);
					continue;
				}
				if (Convert.ToInt32(dataReader["iStatus"]) == 4)
				{
					approvalHistoryDetail.Level = -1;
					approvalHistoryDetail.User = string.Format("{1} : {0}", Convert.ToString(dataReader["sUserName"]), text4);
					approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text6);
				}
				else
				{
					approvalHistoryDetail.User = string.Format("{2} {0} : {1}", approvalHistoryDetail.Level + 1, Convert.ToString(dataReader["sUserName"]), text11);
					approvalHistoryDetail.Status = (AprrovalStatus)Convert.ToInt32(dataReader["iStatus"]);
					if (!string.IsNullOrEmpty(Convert.ToString(dataReader["sRemarks"])))
					{
						approvalHistoryDetail.Remarks = string.Format("{1} : {0} ({2} {3})", Convert.ToString(dataReader["sRemarks"]), text10, text12, Convert.ToInt32(dataReader["iSerialNo"]) + 1);
					}
					if (approvalHistoryDetail.Status == AprrovalStatus.Rejected)
					{
						approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text7);
					}
					else if (approvalHistoryDetail.Status == AprrovalStatus.Stopped)
					{
						approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text8);
					}
					else if (approvalHistoryDetail.Status != AprrovalStatus.Pending)
					{
						approvalHistoryDetail.Date = string.Format("{2} : {0} {1}", text2, Time.IntToTime(num3).ToShortTimeString(), text9);
					}
				}
				list.Add(approvalHistoryDetail);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			approvalHistoryDetail = new ApprovalHistoryDetail();
			approvalHistoryDetail.Level = -2;
			approvalHistoryDetail.Remarks = (m_sError = ex.Message);
			list.Add(approvalHistoryDetail);
		}
		finally
		{
			if (dataReader != null && !dataReader.IsClosed)
			{
				dataReader.Close();
			}
			dataReader = null;
			database = null;
			approvalHistoryDetail = null;
		}
		return list.ToArray();
	}

	public IdNamePair GetMasterFieldName(long iMasterId, int iMasterTypeId, int iLanguageId, int iCompId)
	{
		IdNamePair idNamePair = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		string text = null;
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompId);
			idNamePair = new IdNamePair();
			switch ((ExtraMasterType)iMasterTypeId)
			{
			case ExtraMasterType.CURRENCY:
				text = $"SELECT sName,sCode FROM mCore_Currency WHERE iCurrencyId = {iMasterId}";
				break;
			case ExtraMasterType.LCNUMBER:
				text = $"SELECT sLCNumber[sName],sLCDescription[sCode] FROM mCore_LetterOfCredit WHERE iLetterOfCreditId={iMasterId}";
				break;
			case ExtraMasterType.PMTTERMS:
				text = $"SELECT sPaymentDescription[sName],sPaymentCode[sCode] FROM mCore_PaymentTerms WHERE iPaymentTermId = {iMasterId}";
				break;
			case ExtraMasterType.NUMBERLIST:
				text = $"SELECT sDefaultValue[sName],sFieldName[sCode] FROM cCore_VoucherFields{FConvert.GetSuffix(iCompId)} WHERE iFieldId={iMasterId}";
				break;
			case ExtraMasterType.USER:
				text = $"SELECT sUserName[sName],sLoginName[sCode] FROM mSec_Users WHERE iUserId={iMasterId}";
				break;
			case ExtraMasterType.PRODBATCH:
				text = $"SELECT sProdOrderNo[sName],ISNULL(sRefOrderNo,'')[sCode] FROM tMrp_ProdOrder{FConvert.GetSuffix(iCompId)} WHERE iProdOrderId = {iMasterId}";
				break;
			case ExtraMasterType.PROD_PROCESS:
				text = $"SELECT DISTINCT sProcessName[sName],sProcessName[sCode] FROM vMRP_Process{FConvert.GetSuffix(iCompId)} WHERE iProcessId = {iMasterId}";
				break;
			default:
				text = $"SELECT ISNULL(cCore_MasterTables.sTableName, '')[sTableName] \r\n                            FROM cCore_MasterTables \r\n                            JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                            JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                            WHERE iMasterTypeId = {iMasterTypeId} AND cCore_MasterTables.bDefault = 1";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				empty = Convert.ToString(database.ExecuteScalar(dbCommand));
				text = null;
				if (empty.Length > 0)
				{
					text = ((iLanguageId <= 0) ? $"SELECT sName,sCode FROM {empty} WHERE iMasterId = {iMasterId}" : $"SELECT {empty}Language.sName, {empty}.sCode FROM {empty} \r\n                                JOIN {empty}Language ON {empty}.iMasterId = {empty}Language.iMasterId\r\n                                WHERE {empty}.iMasterId = {iMasterId} AND {empty}Language.iLanguageId = {iLanguageId}");
				}
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				if (dataReader.Read())
				{
					idNamePair.ID = (int)iMasterId;
					idNamePair.Name = Convert.ToString(dataReader["sName"]);
					idNamePair.Tag = Convert.ToString(dataReader["sCode"]);
				}
				dataReader.Close();
			}
		}
		catch (Exception ex)
		{
			idNamePair.ID = -1;
			idNamePair.Name = (m_sError = ex.Message);
		}
		return idNamePair;
	}

	public List<LCDetailsForReport> GetLCDetails(string strVoucherNo, int iCompanyId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		try
		{
			string empty = string.Empty;
			empty = string.Format(" Select  sVoucherNo,iDate,sLCNumber,sLCDescription,\r\n                                                Case When iTransactionType =1 then 'Purchase' else 'Sales' end TransactionType,\r\n                                                iAccountId CustOrVendorId, iIssueingBankId,iDateOfIssue,iExpiryDate,\r\n                                                Case When iLCTypeId =1 Then 'Inland' Else 'Foreign' End LCType ,mLCValue,iCurrencyId,\r\n                                                mExchangeRate,Case When iCreditLimitTypeId =1 Then 'Fixed' Else 'Revolving' End CreditLimitType,\r\n                                                Case When iRevolvingTypeId =1 Then 'Automatic' else 'Manual' end RevolvingType,\r\n                                                Case When iStatusID =4 then 'Created' When iStatusID =0 then 'Opened' \r\n                                                     When iStatusID =1 then 'Released' When iStatusID =2 then 'Closed' \r\n                                                     When iStatusID =3 then  'Amended' else 'Deleted' End [Status] ,\r\n                                                Case When IsNull(iAtSightUsanceType,0) =0 Then 'At Sight' Else 'Usance' End [Type] ,\r\n                                                     IsNull(iMarginAccount,0) iMarginAccount,IsNull(dMarginAmount,0) dMarginAmount,\r\n                                                     IsNull(sReceivingBank,'') sReceivingBank, IsNull(LC.iNoOfDays,0) iNoOfDays,\r\n                                                     IsNull(iPurOrSalTransId,0) iPurOrSalTransId,\r\n                                                Case When IsNull(LCPost.iPostingTypeId,0) =1  then 'Bank' \r\n                                                     When IsNull(LCPost.iPostingTypeId,0) =2 then 'TR' else '' end PaymentType,\r\n                                                     iReleasedDate,IsNull(sBLNo,'') sBLNo ,IsNull(iTRAccountId,0) TRAccount,\r\n                                                IsNull(iAcceptanceAccountId,0) iAcceptanceAccountId,\r\n                                                IsNull(sTRReferenceNo,'') sTRReferenceNo,IsNull(dTRAmount,0) dTRAmount ,\r\n                                                IsNull(iTRNOOfDays,0) iTRNOOfDays \r\n                                         from   mCore_LetterOfCredit LC  Left Join \r\n                                                tCore_LCPostingDetails LCPost On  LCPost.iLCId = LC.iLetterOfCreditId\r\n                                        where sVoucherNo = N'{1}'", FConvert.GetYearId(iCompanyId), strVoucherNo);
			IDataReader dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			List<LCDetailsForReport> list = new List<LCDetailsForReport>();
			while (dataReader.Read())
			{
				LCDetailsForReport lCDetailsForReport = new LCDetailsForReport();
				lCDetailsForReport.VoucherNo = Convert.ToString(dataReader["sVoucherNo"]);
				lCDetailsForReport.Date = Convert.ToInt32(dataReader["iDate"]);
				lCDetailsForReport.LCNumber = Convert.ToString(dataReader["sLCNumber"]);
				lCDetailsForReport.LCDescription = Convert.ToString(dataReader["sLCDescription"]);
				lCDetailsForReport.TransactionType = Convert.ToString(dataReader["TransactionType"]);
				lCDetailsForReport.CustOrVendorId = Convert.ToInt32(dataReader["CustOrVendorId"]);
				lCDetailsForReport.IssueingBankId = Convert.ToInt32(dataReader["iIssueingBankId"]);
				lCDetailsForReport.DateOfIssue = Convert.ToInt32(dataReader["iDateOfIssue"]);
				lCDetailsForReport.ExpiryDate = Convert.ToInt32(dataReader["iExpiryDate"]);
				lCDetailsForReport.LCType = Convert.ToString(dataReader["LCType"]);
				lCDetailsForReport.LCValue = Convert.ToDecimal(dataReader["mLCValue"]);
				lCDetailsForReport.CurrencyId = Convert.ToInt32(dataReader["iCurrencyId"]);
				lCDetailsForReport.ExchangeRate = Convert.ToDecimal(dataReader["mExchangeRate"]);
				lCDetailsForReport.CreditLimitType = Convert.ToString(dataReader["CreditLimitType"]);
				lCDetailsForReport.RevolvingType = Convert.ToString(dataReader["RevolvingType"]);
				lCDetailsForReport.Status = Convert.ToString(dataReader["Status"]);
				lCDetailsForReport.Type = Convert.ToString(dataReader["Type"]);
				lCDetailsForReport.MarginAccount = Convert.ToInt32(dataReader["iMarginAccount"]);
				lCDetailsForReport.MarginAmount = Convert.ToDecimal(dataReader["dMarginAmount"]);
				lCDetailsForReport.ReceivingBank = Convert.ToString(dataReader["sReceivingBank"]);
				lCDetailsForReport.NoOfDays = Convert.ToInt32(dataReader["iNoOfDays"]);
				lCDetailsForReport.PurOrSalTransId = Convert.ToInt64(dataReader["iPurOrSalTransId"]);
				lCDetailsForReport.PaymentType = Convert.ToString(dataReader["PaymentType"]);
				lCDetailsForReport.ReleasedDate = Convert.ToInt32(dataReader["iReleasedDate"]);
				lCDetailsForReport.BLNo = Convert.ToString(dataReader["sBLNo"]);
				lCDetailsForReport.TRAccount = Convert.ToInt32(dataReader["TRAccount"]);
				lCDetailsForReport.AcceptanceAccountId = Convert.ToInt32(dataReader["iAcceptanceAccountId"]);
				lCDetailsForReport.TRReferenceNo = Convert.ToString(dataReader["sTRReferenceNo"]);
				lCDetailsForReport.TRAmount = Convert.ToDecimal(dataReader["dTRAmount"]);
				lCDetailsForReport.TRNOOfDays = Convert.ToInt32(dataReader["iTRNOOfDays"]);
				list.Add(lCDetailsForReport);
			}
			dataReader.Close();
			return list;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return null;
		}
	}

	public IdNamePair[] GetMasterImageFields(int[] arrMasterTypes, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = null;
		list = new List<IdNamePair>();
		if (arrMasterTypes == null || arrMasterTypes.Length == 0)
		{
			return list.ToArray();
		}
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text2 = string.Join(",", arrMasterTypes.Select((int p) =>
			{
				int num = p;
				return num.ToString();
			}).ToArray());
			text = "SELECT cCore_MasterFields.iFieldId, cCore_Fields.sCaption, cCore_MasterTabs.iMasterId\r\n                FROM cCore_MasterFields\r\n                INNER JOIN cCore_Fields ON cCore_MasterFields.iFieldId = cCore_Fields.iFieldId\r\n                INNER JOIN cCore_MasterTables ON cCore_MasterFields.iTableId = cCore_MasterTables.iTableId\r\n                INNER JOIN cCore_MasterTabs ON cCore_MasterTables.iTabId = cCore_MasterTabs.iTabId\r\n                WHERE cCore_MasterTabs.iMasterId IN (" + text2 + ") AND iDataTypeId IN (" + string.Join(",", 7, 10) + ")\r\n                ORDER BY cCore_MasterTabs.iMasterId";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iFieldId"]), Convert.ToString(dataReader["sCaption"]), Convert.ToInt32(dataReader["iMasterId"])));
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public IdNamePair[] GetDocumentViewFieldData(IdNamePair[] arrField, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = null;
		FMongoDb fMongoDb = null;
		byte[] array = null;
		list = new List<IdNamePair>();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			for (int i = 0; i < arrField.Length; i++)
			{
				text = $"SELECT sFieldName, cCore_MasterTables.sTableName \r\n                    FROM cCore_MasterFields\r\n                    INNER JOIN cCore_Fields ON cCore_MasterFields.iFieldId = cCore_Fields.iFieldId\r\n                    INNER JOIN cCore_MasterTables ON cCore_MasterFields.iTableId = cCore_MasterTables.iTableId\r\n                    INNER JOIN cCore_MasterTabs ON cCore_MasterTables.iTabId = cCore_MasterTabs.iTabId\r\n                    WHERE cCore_MasterFields.iFieldId = {arrField[i].ID}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				if (dataReader.Read())
				{
					text2 = Convert.ToString(dataReader["sFieldName"]);
					text3 = Convert.ToString(dataReader["sTableName"]);
				}
				dataReader.Close();
				if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(Convert.ToString(arrField[i].Tag)))
				{
					continue;
				}
				text = $"SELECT {text2}[Data], {text2}Name[FileName] FROM {text3} WHERE iMasterId IN ({arrField[i].Tag})";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list.Add(new IdNamePair(arrField[i].ID, Convert.ToString(dataReader["FileName"]), dataReader["Data"]));
					if (_focus.company(iCompanyId).mongoEnable)
					{
						if (fMongoDb == null)
						{
							fMongoDb = new FMongoDb(FConvert.CompanyCodeFromId(iCompanyId));
						}
						array = fMongoDb.GetDocFromMongo(Convert.ToString(dataReader["Data"]));
						list[list.Count - 1].Tag = ((array != null) ? array : null);
					}
				}
				dataReader.Close();
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public PrintTagDetails GetAgeingDetails(AccountAgeingInput objInput, int iCompanyId)
	{
		int iGroupId = 0;
		int num = 0;
		string text = null;
		string text2 = null;
		string text3 = null;
		string sTagJoinTable = null;
		string text4 = null;
		string text5 = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		AccountAgeingDetails accountAgeingDetails = null;
		List<AccountAgeingDetails> list = null;
		List<string> list2 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		PrintTagDetails oReturnData = new PrintTagDetails();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			if (objInput.MasterIds != null && objInput.MasterIds.Length != 0)
			{
				text5 = string.Join(",", objInput.MasterIds.Select((int p) =>
				{
					int num21 = p;
					return num21.ToString();
				}).ToArray());
				if (objInput.MasterIds.Length == 1)
				{
					text = string.Format("SELECT iMasterId FROM dbo.[fCore_GetAccountClubBy]() WHERE iParentId={0} OR iMasterId={0}", objInput.MasterIds[0]);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					dataReader = database.ExecuteReader(dbCommand);
					List<int> list3 = new List<int>();
					while (dataReader.Read())
					{
						list3.Add(Convert.ToInt32(dataReader["iMasterId"]));
					}
					dataReader.Close();
					if (list3.Count > objInput.MasterIds.Length)
					{
						iGroupId = objInput.MasterIds[0];
						text5 = string.Join(",", list3.Select((int p) =>
						{
							int num21 = p;
							return num21.ToString();
						}).ToArray());
					}
				}
				objInput.StartDate = 0;
				if (objInput.Formulas != null && objInput.Formulas.Count > 0)
				{
					list2 = new List<string>();
					for (num = 0; num < objInput.Formulas.Count; num++)
					{
						if (objInput.Formulas[num].ToLower().Contains("agslab") || objInput.Formulas[num].ToLower().Contains("ageingslab"))
						{
							int num2 = 0;
							int num3 = 0;
							string[] array = objInput.Formulas[num].Split(new char[2] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
							array = array[1].Split(',', '-');
							if (array.Length == 1)
							{
								list2.Add(objInput.Formulas[num]);
								num3 = new Date(objInput.EndDate, m_objCalType).Month - Convert.ToInt32(array[0]) + 1;
								num2 = ((num3 > 0) ? num3 : (num3 + 12));
								text2 += $",SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = {num2} THEN mAmount ELSE 0 END)[{objInput.Formulas[num]}]";
								continue;
							}
							if (array.Length >= 2)
							{
								num2 = Convert.ToInt32(array[0]);
								num3 = Convert.ToInt32(array[1]);
							}
							if (num2 >= 0 && num3 > num2)
							{
								list2.Add(objInput.Formulas[num]);
								text2 += $",SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num2} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num3} THEN mAmount ELSE 0 END)[{objInput.Formulas[num]}]";
							}
						}
						else if (objInput.Formulas[num].ToLower().StartsWith("pslab"))
						{
							int num4 = 0;
							int num5 = 0;
							string[] array2 = objInput.Formulas[num].Split(new char[2] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
							if (array2.Length > 1)
							{
								array2 = array2[1].Split(',', '-');
							}
							if (array2.Length >= 2)
							{
								num4 = Convert.ToInt32(array2[0]);
								num5 = Convert.ToInt32(array2[1]);
								Date date = new Date(objInput.EndDate, m_objCalType).Add(-(num4 - 1), DateAdd.Months);
								date = new Date(date.Year, date.Month, 1, m_objCalType);
								Date date2 = date.Add(-(num5 - 1), DateAdd.Months);
								date2 = new Date(date2.Year, date2.Month, 1, m_objCalType);
								date = new Date(date.Year, date.Month, date.LastDayOfMonth(), m_objCalType);
								list2.Add(objInput.Formulas[num]);
								text2 += $",SUM(CASE WHEN tCore_Header{suffix}.iDate BETWEEN {date2.Value} AND {date.Value} THEN mAmount ELSE 0 END)[{objInput.Formulas[num]}]";
							}
						}
					}
				}
				if (objInput.AsOnDate > 0)
				{
					text3 = $"AND \r\n                    (\r\n                        (\r\n                            tCore_Refrn{suffix}.iCode IN({text5})\r\n                            AND\r\n                            (\r\n                                iRefType = 0 AND \r\n                                (tCore_Header{suffix}.iDate <= {objInput.EndDate} or tCore_Header{suffix}.iVoucherClass = {256})\r\n                            )\r\n                        )\r\n                        OR\r\n                        (\r\n                            iRefType = 2 AND iDate <= {objInput.AsOnDate} AND iRef IN \r\n                            (\r\n                                SELECT iRef FROM tCore_Refrn{suffix} a WITH (READUNCOMMITTED)\r\n                                JOIN tCore_Data{suffix} b ON a.iBodyId = b.iBodyId \r\n                                JOIN tCore_Header{suffix} c ON b.iHeaderId = c.iHeaderId\r\n                                WHERE a.iCode IN({text5}) AND a.iRefType = 0 AND c.iDate <= {objInput.EndDate}\r\n                            )\r\n\t                    )\r\n                    )";
				}
				if (objInput.Filters != null && objInput.Filters.Length != 0)
				{
					RDReport rDReport = new RDReport();
					List<IdValuePair> arrTagFilter = null;
					string sMultiAccountJoin = null;
					text4 = rDReport.GetSpecialFilter(objInput.Filters, ref arrTagFilter, database, iCompanyId, ref sTagJoinTable, ref sMultiAccountJoin);
				}
				int num6 = 30;
				int num7 = 60;
				int num8 = 90;
				int num9 = 120;
				int num10 = 150;
				int num11 = 180;
				int num12 = 210;
				int num13 = 0;
				int num14 = 30;
				string[] preferenceTexts = _focus.company(iCompanyId).getPreferenceTexts(PreferenceCategories.Print, 100, 150);
				if (preferenceTexts != null && preferenceTexts.Length != 0)
				{
					num = 0;
					while (num + 1 < preferenceTexts.Length && num14 <= 36)
					{
						if (FConvert.IsNumeric(preferenceTexts[num + 1]))
						{
							switch ((INVOICE_FIELD)num14)
							{
							case INVOICE_FIELD.INV_AGEING_SLAB1:
								num13 = (num6 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB2:
								num13 = (num7 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB3:
								num13 = (num8 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB4:
								num13 = (num9 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB5:
								num13 = (num10 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB6:
								num13 = (num11 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							case INVOICE_FIELD.INV_AGEING_SLAB7:
								num13 = (num12 = Convert.ToInt32(preferenceTexts[num + 1]));
								break;
							}
						}
						num += 2;
						num14++;
					}
				}
				else if (objInput.ReportInputs != null && objInput.ReportInputs.Length > 6)
				{
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 8);
					num6 = ((num13 > 0 && num13 != num6) ? num13 : 30);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 9);
					num7 = ((num13 > num6 && num13 != num7) ? num13 : 60);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 10);
					num8 = ((num13 > num7 && num13 != num8) ? num13 : 90);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 11);
					num9 = ((num13 > num8 && num13 != num9) ? num13 : 120);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 12);
					num10 = ((num13 > num9 && num13 != num10) ? num13 : 150);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 13);
					num11 = ((num13 > num10 && num13 != num11) ? num13 : 180);
					num13 = FConvert.GetInputValue(objInput.ReportInputs, 14);
					num12 = ((num13 > num11 && num13 != num12) ? num13 : 210);
				}
				if (num13 > 0 && num12 != num13)
				{
					num12 = num13;
				}
				text = $"SELECT vrCore_Account.iMasterId, vrCore_Account.sName\r\n                    , SUM(mAmount)[Balance]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) >= 0 AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num6} THEN mAmount ELSE 0 END)[Slab1]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num6} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num7} THEN mAmount ELSE 0 END)[Slab2]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num7} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num8} THEN mAmount ELSE 0 END)[Slab3]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num8} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num9} THEN mAmount ELSE 0 END)[Slab4]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num9} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num10} THEN mAmount ELSE 0 END)[Slab5]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num10} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num11} THEN mAmount ELSE 0 END)[Slab6]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num11} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num12} THEN mAmount ELSE 0 END)[Slab7]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num12} THEN mAmount ELSE 0 END)[GTSlab7]\r\n\r\n                    , SUM(tAmount)[Balance_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) >= 0 AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num6} THEN tAmount ELSE 0 END)[Slab1_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num6} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num7} THEN tAmount ELSE 0 END)[Slab2_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num7} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num8} THEN tAmount ELSE 0 END)[Slab3_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num8} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num9} THEN tAmount ELSE 0 END)[Slab4_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num9} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num10} THEN tAmount ELSE 0 END)[Slab5_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num10} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num11} THEN tAmount ELSE 0 END)[Slab6_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num11} AND DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) <= {num12} THEN tAmount ELSE 0 END)[Slab7_TC]\r\n                    ,SUM(CASE WHEN DATEDIFF(dd, dbo.IntToGregDate(tCore_Header{suffix}.iDate), dbo.IntToGregDate({objInput.EndDate})) > {num12} THEN tAmount ELSE 0 END)[GTSlab7_TC]\r\n                    {text2}\r\n                    FROM\r\n                    (\r\n                        SELECT iRef, tCore_Refrn{suffix}.iCode, SUM(CASE WHEN iRefType < 2 THEN tCore_Refrn{suffix}.iBodyId ELSE 0 END)[iBodyId], \r\n                        sum(case when (tCore_Header{suffix}.iVoucherClass not in (5888,7168) or tCore_Data{suffix}.bUpdateFA=1) then \r\n                        CASE WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n                        WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t\t                WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n                        WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff END ELSE 0 END)[mAmount]\r\n                        ,SUM(CASE WHEN (tCore_Header{suffix}.iVoucherClass NOT IN (5888,7168) OR tCore_Data{suffix}.bUpdateFA=1) THEN tCore_Refrn{suffix}.mAmount ELSE 0 END)[tAmount]\r\n\t                    FROM tCore_Refrn{suffix} WITH (READUNCOMMITTED)\r\n                        JOIN tCore_Data{suffix} ON tCore_Data{suffix}.iBodyId = tCore_Refrn{suffix}.iBodyId\r\n\t                    JOIN tCore_Header{suffix} ON tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId\r\n                        WHERE tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Header{suffix}.bSuspended = 0 AND tCore_Data{suffix}.iAuthStatus <2 \r\n                        --AND (((tCore_Header{suffix}.iVoucherClass not in(7168 ,5888)) and tCore_Data{suffix}.bUpdateFA=1) )\r\n                        AND tCore_Data{suffix}.bUpdateFA=1\r\n                        AND (tCore_Header{suffix}.iDate BETWEEN 0 AND {objInput.EndDate} )\r\n                        {text3}\r\n                        GROUP BY iRef, tCore_Refrn{suffix}.iCode\r\n                        Having sum(CASE WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n                        WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t                    WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t                    WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff END) <> 0\r\n                    ) ParentRef\r\n                    JOIN tCore_Data{suffix} ParentData ON ParentData.iBodyId = ParentRef.iBodyId\r\n                    JOIN tCore_Header{suffix} ParentHeader ON ParentData.iHeaderId = ParentHeader.iHeaderId\r\n                    LEFT JOIN tCore_Data{suffix} ON ParentRef.iBodyId = tCore_Data{suffix}.iBodyId\r\n                    LEFT JOIN tCore_Header{suffix} on tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId\r\n                    JOIN cCore_Vouchers{suffix} ON cCore_Vouchers{suffix}.iVoucherType = tCore_Header{suffix}.iVoucherType\r\n                    JOIN vrCore_Account ON vrCore_Account.iMasterId = ParentRef.iCode AND vrCore_Account.iTreeId = 0 {sTagJoinTable}\r\n                    WHERE tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Data{suffix}.iAuthStatus < 2\r\n                    AND tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Header{suffix}.bSuspended = 0\r\n                    AND tCore_Header{suffix}.iDate BETWEEN {objInput.StartDate} AND {objInput.EndDate} AND vrCore_Account.iMasterId IN({text5}) {text4}\r\n                    GROUP BY vrCore_Account.iMasterId, vrCore_Account.sName";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dataReader = database.ExecuteReader(dbCommand);
				list = new List<AccountAgeingDetails>();
				while (dataReader.Read())
				{
					accountAgeingDetails = new AccountAgeingDetails();
					accountAgeingDetails.AccountId = Convert.ToInt32(dataReader["iMasterId"]);
					accountAgeingDetails.AccountName = Convert.ToString(dataReader["sName"]);
					accountAgeingDetails.Balance = Convert.ToDouble(dataReader["Balance"]);
					accountAgeingDetails.Slab1 = Convert.ToDouble(dataReader["Slab1"]);
					accountAgeingDetails.Slab2 = Convert.ToDouble(dataReader["Slab2"]);
					accountAgeingDetails.Slab3 = Convert.ToDouble(dataReader["Slab3"]);
					accountAgeingDetails.Slab4 = Convert.ToDouble(dataReader["Slab4"]);
					accountAgeingDetails.Slab5 = Convert.ToDouble(dataReader["Slab5"]);
					accountAgeingDetails.Slab6 = Convert.ToDouble(dataReader["Slab6"]);
					accountAgeingDetails.Slab7 = Convert.ToDouble(dataReader["Slab7"]);
					accountAgeingDetails.GreterThanLastSlab = Convert.ToDouble(dataReader["GTSlab7"]);
					accountAgeingDetails.Slabs_TC = new double[9];
					accountAgeingDetails.Slabs_TC[0] = Convert.ToDouble(dataReader["Balance_TC"]);
					accountAgeingDetails.Slabs_TC[1] = Convert.ToDouble(dataReader["Slab1_TC"]);
					accountAgeingDetails.Slabs_TC[2] = Convert.ToDouble(dataReader["Slab2_TC"]);
					accountAgeingDetails.Slabs_TC[3] = Convert.ToDouble(dataReader["Slab3_TC"]);
					accountAgeingDetails.Slabs_TC[4] = Convert.ToDouble(dataReader["Slab4_TC"]);
					accountAgeingDetails.Slabs_TC[5] = Convert.ToDouble(dataReader["Slab5_TC"]);
					accountAgeingDetails.Slabs_TC[6] = Convert.ToDouble(dataReader["Slab6_TC"]);
					accountAgeingDetails.Slabs_TC[7] = Convert.ToDouble(dataReader["Slab7_TC"]);
					accountAgeingDetails.Slabs_TC[8] = Convert.ToDouble(dataReader["GTSlab7_TC"]);
					if (list2 != null && list2.Count > 0)
					{
						List<IdNamePair> list4 = new List<IdNamePair>();
						for (num = 0; num < list2.Count; num++)
						{
							list4.Add(new IdNamePair(num, list2[num], Convert.ToDouble(dataReader[list2[num] ?? ""])));
						}
						accountAgeingDetails.CustomSlabs = list4.ToArray();
					}
					list.Add(accountAgeingDetails);
				}
				dataReader.Close();
				if (objInput.ExtraFields != null)
				{
					if (list.Count > 0)
					{
						for (num = 0; num < list.Count; num++)
						{
							List<IdValuePair> list5 = new List<IdValuePair>();
							for (int num15 = 0; num15 < objInput.ExtraFields.Count; num15++)
							{
								string masterValueFromField = GetMasterValueFromField(objInput.ExtraFields[num15], (objInput.ExtraFields[num15] == 127) ? 1 : 4, list[num].AccountId.ToString(), objInput.LanguageId, iCompanyId);
								list5.Add(new IdValuePair(objInput.ExtraFields[num15], masterValueFromField));
							}
							list[num].AccountData = list5.ToArray();
						}
						for (int num16 = 0; num16 < objInput.MasterIds.Length; num16++)
						{
							for (num = 0; num < list.Count && (list[num].AccountId != objInput.MasterIds[num16] || list[num].AccountData == null); num++)
							{
							}
							if (num == list.Count)
							{
								List<IdValuePair> list6 = new List<IdValuePair>();
								for (int num17 = 0; num17 < objInput.ExtraFields.Count; num17++)
								{
									string masterValueFromField2 = GetMasterValueFromField(objInput.ExtraFields[num17], (objInput.ExtraFields[num17] == 127) ? 1 : 4, objInput.MasterIds[num16].ToString(), objInput.LanguageId, iCompanyId);
									list6.Add(new IdValuePair(objInput.ExtraFields[num17], masterValueFromField2));
								}
								accountAgeingDetails = new AccountAgeingDetails();
								accountAgeingDetails.AccountId = objInput.MasterIds[num16];
								accountAgeingDetails.AccountData = list6.ToArray();
								list.Add(accountAgeingDetails);
							}
						}
					}
					else
					{
						for (num = 0; num < objInput.MasterIds.Length; num++)
						{
							List<IdValuePair> list7 = new List<IdValuePair>();
							for (int num18 = 0; num18 < objInput.ExtraFields.Count; num18++)
							{
								string masterValueFromField3 = GetMasterValueFromField(objInput.ExtraFields[num18], (objInput.ExtraFields[num18] == 127) ? 1 : 4, objInput.MasterIds[num].ToString(), objInput.LanguageId, iCompanyId);
								list7.Add(new IdValuePair(objInput.ExtraFields[num18], masterValueFromField3));
							}
							AccountAgeingDetails accountAgeingDetails2 = new AccountAgeingDetails();
							accountAgeingDetails2.AccountId = objInput.MasterIds[num];
							accountAgeingDetails2.AccountData = list7.ToArray();
							list.Add(accountAgeingDetails2);
						}
					}
				}
				if (objInput.IncludeMonthlySlab)
				{
					text = $"SELECT vrCore_Account.iMasterId \r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 1 THEN mAmount ELSE 0 END)[Month1]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 2 THEN mAmount ELSE 0 END)[Month2]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 3 THEN mAmount ELSE 0 END)[Month3]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 4 THEN mAmount ELSE 0 END)[Month4]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 5 THEN mAmount ELSE 0 END)[Month5]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 6 THEN mAmount ELSE 0 END)[Month6]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 7 THEN mAmount ELSE 0 END)[Month7]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 8 THEN mAmount ELSE 0 END)[Month8]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 9 THEN mAmount ELSE 0 END)[Month9]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 10 THEN mAmount ELSE 0 END)[Month10]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 11 THEN mAmount ELSE 0 END)[Month11]\r\n                ,SUM(CASE WHEN DATEPART(mm, dbo.IntToGregDate(tCore_Header{suffix}.iDate)) = 12 THEN mAmount ELSE 0 END)[Month12]\r\n                FROM\r\n                (\r\n                    SELECT iRef, tCore_Refrn{suffix}.iCode, SUM(CASE WHEN iRefType < 2 THEN tCore_Refrn{suffix}.iBodyId ELSE 0 END)[iBodyId], \r\n                    sum(CASE WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n                        WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t\t                WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t\t                WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff END)[mAmount],\r\n                    SUM(tCore_Refrn{suffix}.mAmount) [mTAmount],\r\n\t                SUM((abs(tCore_Refrn{suffix}.mLocalAmount) - tCore_Refrn{suffix}.mLocalExchangeDiff) * sign(tCore_Refrn{suffix}.mLocalAmount)) [mLAmount],\r\n                    case max(CASE WHEN iRefType = 2 THEN tCore_Refrn{suffix}.iDueDate ELSE 0 END) when 0 then  dbo.DateToInt(getdate()) else max(CASE WHEN iRefType = 2 THEN tCore_Refrn{suffix}.iDueDate ELSE 0 END) end [iDueDate], \r\n                    sum(CASE WHEN iRefType < 2 THEN tCore_Refrn{suffix}.iDueDate ELSE 0 END) [iNewRefDueDate], \r\n\t                isnull(sum(CASE WHEN iRefType = 2 THEN 1 ELSE 0 END),1) iCount, \r\n\t                SUM(CASE WHEN iRefType < 2 THEN iRefType ELSE 0 END) [iRefType],\r\n\t                SUM(CASE WHEN iRefType < 2 THEN \r\n                        CASE WHEN mBaseAmount >= 0 AND mExchangeDiff >= 0 THEN mBaseAmount - mExchangeDiff \r\n                        WHEN mBaseAmount >= 0 AND mExchangeDiff <= 0 THEN mBaseAmount - mExchangeDiff \r\n\t\t                WHEN mBaseAmount < 0 AND mExchangeDiff >= 0 THEN mBaseAmount - mExchangeDiff \r\n\t\t                WHEN mBaseAmount < 0 AND mExchangeDiff <= 0 THEN mBaseAmount - mExchangeDiff END\r\n                        ELSE 0 END) [mInvoiceAmount],\r\n                    SUM(CASE WHEN iRefType < 2 THEN iCurrencyId ELSE 0 END) [iCurrencyId],\r\n\t                SUM(CASE WHEN iRefType < 2 THEN mAmount ELSE 0 END) [mTInvoiceAmount],\r\n                    SUM(CASE WHEN iRefType = 2 THEN mAmount ELSE 0 END) [mTAdjustedAmount]\r\n\t                FROM tCore_Refrn{suffix} WITH (READUNCOMMITTED)\r\n\t                JOIN tCore_Data{suffix} ON tCore_Data{suffix}.iBodyId = tCore_Refrn{suffix}.iBodyId\r\n\t                JOIN tCore_Header{suffix} ON tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId\r\n                    WHERE tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Data{suffix}.iAuthStatus IN(0,1)  AND tCore_Header{suffix}.bSuspended = 0 and (tCore_Header{suffix}.iVoucherClass not in (5888,7168) or bPdc=0 or tCore_Data{suffix}.bUpdateFA=1)\r\n                    AND (tCore_Header{suffix}.iDate BETWEEN {objInput.StartDate} AND {objInput.EndDate} or tCore_Header{suffix}.iVoucherType = 256) AND tCore_Data{suffix}.bUpdateFA = 1 \r\n                    GROUP BY iRef, tCore_Refrn{suffix}.iCode\r\n                    Having sum(CASE WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n                    WHEN tCore_Refrn{suffix}.mBaseAmount >= 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t                WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff >= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff \r\n\t                WHEN tCore_Refrn{suffix}.mBaseAmount < 0 AND tCore_Refrn{suffix}.mExchangeDiff <= 0 THEN tCore_Refrn{suffix}.mBaseAmount - tCore_Refrn{suffix}.mExchangeDiff END) <> 0\r\n                ) ParentRef\r\n                JOIN tCore_Data{suffix} ParentData ON ParentData.iBodyId = ParentRef.iBodyId\r\n                JOIN tCore_Header{suffix} ParentHeader ON ParentData.iHeaderId = ParentHeader.iHeaderId\r\n                LEFT JOIN tCore_Data{suffix} ON ParentRef.iBodyId = tCore_Data{suffix}.iBodyId\r\n                LEFT JOIN tCore_Header{suffix} on tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId\r\n                JOIN cCore_Vouchers{suffix} ON cCore_Vouchers{suffix}.iVoucherType = tCore_Header{suffix}.iVoucherType\r\n                JOIN vrCore_Account ON vrCore_Account.iMasterId = ParentRef.iCode AND vrCore_Account.iTreeId = 0 {sTagJoinTable}\r\n                WHERE tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Data{suffix}.iAuthStatus < 2\r\n                AND tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Header{suffix}.bSuspended = 0\r\n                AND tCore_Header{suffix}.iDate BETWEEN {objInput.StartDate} AND {objInput.EndDate} AND vrCore_Account.iMasterId IN({text5}) {text4}\r\n                GROUP BY vrCore_Account.iMasterId";
					database = DatabaseWrapper.GetDatabase2(iCompanyId);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					dataReader = database.ExecuteReader(dbCommand);
					List<IdValuePair> list8 = new List<IdValuePair>();
					while (dataReader.Read())
					{
						decimal[] value = new decimal[12]
						{
							Convert.ToDecimal(dataReader["Month1"]),
							Convert.ToDecimal(dataReader["Month2"]),
							Convert.ToDecimal(dataReader["Month3"]),
							Convert.ToDecimal(dataReader["Month4"]),
							Convert.ToDecimal(dataReader["Month5"]),
							Convert.ToDecimal(dataReader["Month6"]),
							Convert.ToDecimal(dataReader["Month7"]),
							Convert.ToDecimal(dataReader["Month8"]),
							Convert.ToDecimal(dataReader["Month9"]),
							Convert.ToDecimal(dataReader["Month10"]),
							Convert.ToDecimal(dataReader["Month11"]),
							Convert.ToDecimal(dataReader["Month12"])
						};
						IdValuePair idValuePair = new IdValuePair();
						idValuePair.ID = Convert.ToInt32(dataReader["iMasterId"]);
						idValuePair.Value = value;
						list8.Add(idValuePair);
					}
					dataReader.Close();
					string[] monthNames = new DateTimeFormatInfo().MonthNames;
					for (num = 0; num < list.Count; num++)
					{
						for (int num19 = 0; num19 < list8.Count; num19++)
						{
							if (list[num].AccountId == list8[num19].ID)
							{
								list[num].MonthlySlab = GetMonthlyAgeingSlab((decimal[])list8[num19].Value, monthNames);
								break;
							}
						}
					}
				}
				if (objInput.IncludePDC && !string.IsNullOrEmpty(text5))
				{
					string empty = string.Empty;
					text = string.Format("SELECT [iMasterId],SUM(Debit)+SUM(Credit)[Base],\r\n                        SUM(TranDr)+SUM(TranCr)[FX],SUM(LocalDr)+SUM(LocalCr)[Local]\r\n                        FROM\r\n                        (\t\t\r\n\t                        SELECT iBookNo[iMasterId],iFaTag,iInvTag,\r\n\t                        CASE WHEN mAmount2 < 0 THEN mAmount2 ELSE 0 END Debit, \r\n\t                        CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END Credit,\r\n                            CASE WHEN ISNULL(mFxAmount2,mAmount2) < 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranDr, \r\n\t                        CASE WHEN ISNULL(mFxAmount2,mAmount2) > 0 THEN ISNULL(mFxAmount2,mAmount2) ELSE 0 END TranCr,\r\n                            CASE WHEN ISNULL(fLocalAmount2,0) < 0 THEN ISNULL(fLocalAmount2,0) ELSE 0 END LocalDr, \r\n\t                        CASE WHEN ISNULL(fLocalAmount2,0) > 0 THEN ISNULL(fLocalAmount2,0) ELSE 0 END LocalCr\r\n\t                        FROM tCore_Data{0} \r\n\t                        JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                            LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t                        WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND (tCore_Header{0}.bChequeReturn = 0 OR tCore_Header{0}.bChequeReturn IS NULL)\r\n\t                        AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2\r\n\t                        AND \r\n\t                        (\r\n\t\t                        (tCore_Header{0}.iDate <= {2} AND tCore_Data{0}.bPdc = 0) {1}\r\n\t                        )\r\n\t                        AND ((tCore_Header{0}.iVoucherClass) = {3} OR (tCore_Header{0}.iVoucherClass) = {4}) \r\n                            UNION ALL\r\n\t                        SELECT iCode[iMasterId],iFaTag,iInvTag,\r\n\t                        CASE WHEN mAmount1 < 0 THEN mAmount1 ELSE 0 END Debit, \r\n\t                        CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END Credit,\r\n                            CASE WHEN ISNULL(mFXAmount1,mAmount1) < 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranDr, \r\n\t                        CASE WHEN ISNULL(mFXAmount1,mAmount1) > 0 THEN ISNULL(mFXAmount1,mAmount1) ELSE 0 END TranCr,\r\n                            CASE WHEN ISNULL(fLocalAmount1,0) < 0 THEN ISNULL(fLocalAmount1,0) ELSE 0 END LocalDr, \r\n\t                        CASE WHEN ISNULL(fLocalAmount1,0) > 0 THEN ISNULL(fLocalAmount1,0) ELSE 0 END LocalCr\r\n\t                        FROM tCore_Data{0} \r\n\t                        JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n                            LEFT JOIN tCore_Data_FX{0} ON tCore_Data_FX{0}.iBodyId = tCore_Data{0}.iBodyId\r\n\t                        WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND (tCore_Header{0}.bChequeReturn = 0 OR tCore_Header{0}.bChequeReturn IS NULL)\r\n\t                        AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus < 2\r\n\t                        AND \r\n\t                        (\r\n\t\t                        (tCore_Header{0}.iDate <= {2} AND tCore_Data{0}.bPdc = 0) {1}\r\n\t                        )\r\n\t                        AND ((tCore_Header{0}.iVoucherClass) = {3} OR (tCore_Header{0}.iVoucherClass) = {4}) \r\n                        )TempTable WHERE iMasterId IN({5})\r\n                        GROUP BY [iMasterId]", suffix, empty, objInput.EndDate, 7168, 5888, text5, 65280);
					database = DatabaseWrapper.GetDatabase2(iCompanyId);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					dataReader = database.ExecuteReader(dbCommand);
					List<IdDualValue> list9 = new List<IdDualValue>();
					while (dataReader.Read())
					{
						list9.Add(new IdDualValue(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToDecimal(dataReader["Base"]), Convert.ToDecimal(dataReader["FX"])));
					}
					dataReader.Close();
					for (num = 0; num < list.Count; num++)
					{
						for (int num20 = 0; num20 < list9.Count; num20++)
						{
							if (list[num].AccountId == list9[num20].Id)
							{
								list[num].PDC_Amount = Convert.ToDouble(list9[num20].Value1);
								list[num].PDC_Amount_TC = Convert.ToDouble(list9[num20].Value2);
								break;
							}
						}
					}
				}
				oReturnData.AgeingDetails = list.ToArray();
				GroupAccountBalance(iGroupId, database, objInput.ExtraFields, objInput.LanguageId, iCompanyId, ref oReturnData);
			}
			if (objInput.TransactionFields != null && objInput.TransactionFields.Count > 0 && (objInput.LoadVoucherType == BackTrackType.Transaction || objInput.LoadVoucherType == BackTrackType.TransactionHeaderId || (objInput.Filters != null && objInput.Filters.Length != 0)))
			{
				oReturnData.TransactionDetails = GetTransactionValues(objInput.TransactionFields, objInput.TransactionId, objInput.LoadVoucherType, text4, sTagJoinTable, database, iCompanyId);
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			ProngHorn.LogError(iCompanyId, 2, 5, "GetAgeingDetails::", m_sError);
		}
		return oReturnData;
	}

	private void GroupAccountBalance(int iGroupId, Database objDb, List<int> arrExtraFields, int iLanguageId, int iCompanyId, ref PrintTagDetails oReturnData)
	{
		if (iGroupId <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		List<IdNamePair> list = new List<IdNamePair>();
		List<IdNamePair> list2 = new List<IdNamePair>();
		AccountAgeingDetails accountAgeingDetails = new AccountAgeingDetails();
		accountAgeingDetails.AccountId = iGroupId;
		accountAgeingDetails.Slabs_TC = new double[9];
		for (num = 0; num < oReturnData.AgeingDetails.Length; num++)
		{
			accountAgeingDetails.Balance += oReturnData.AgeingDetails[num].Balance;
			accountAgeingDetails.Slab1 += oReturnData.AgeingDetails[num].Slab1;
			accountAgeingDetails.Slab2 += oReturnData.AgeingDetails[num].Slab2;
			accountAgeingDetails.Slab3 += oReturnData.AgeingDetails[num].Slab3;
			accountAgeingDetails.Slab4 += oReturnData.AgeingDetails[num].Slab4;
			accountAgeingDetails.Slab5 += oReturnData.AgeingDetails[num].Slab5;
			accountAgeingDetails.Slab6 += oReturnData.AgeingDetails[num].Slab6;
			accountAgeingDetails.Slab7 += oReturnData.AgeingDetails[num].Slab7;
			accountAgeingDetails.GreterThanLastSlab += oReturnData.AgeingDetails[num].GreterThanLastSlab;
			if (oReturnData.AgeingDetails[num].Slabs_TC != null && oReturnData.AgeingDetails[num].Slabs_TC.Length > 8)
			{
				accountAgeingDetails.Slabs_TC[0] += oReturnData.AgeingDetails[num].Slabs_TC[0];
				accountAgeingDetails.Slabs_TC[1] += oReturnData.AgeingDetails[num].Slabs_TC[1];
				accountAgeingDetails.Slabs_TC[2] += oReturnData.AgeingDetails[num].Slabs_TC[2];
				accountAgeingDetails.Slabs_TC[3] += oReturnData.AgeingDetails[num].Slabs_TC[3];
				accountAgeingDetails.Slabs_TC[4] += oReturnData.AgeingDetails[num].Slabs_TC[4];
				accountAgeingDetails.Slabs_TC[5] += oReturnData.AgeingDetails[num].Slabs_TC[5];
				accountAgeingDetails.Slabs_TC[6] += oReturnData.AgeingDetails[num].Slabs_TC[6];
				accountAgeingDetails.Slabs_TC[7] += oReturnData.AgeingDetails[num].Slabs_TC[7];
				accountAgeingDetails.Slabs_TC[8] += oReturnData.AgeingDetails[num].Slabs_TC[8];
			}
			if (oReturnData.AgeingDetails[num].CustomSlabs != null)
			{
				list.AddRange(oReturnData.AgeingDetails[num].CustomSlabs);
			}
		}
		for (num = 0; num < list.Count; num++)
		{
			for (num2 = 0; num2 < list2.Count; num2++)
			{
				if (list2[num2].Name == list[num].Name)
				{
					list2[num2].Tag = Convert.ToDouble(list2[num2].Tag) + Convert.ToDouble(list[num].Tag);
					break;
				}
			}
			if (num2 == list2.Count)
			{
				list2.Add(new IdNamePair(list2.Count, list[num].Name, list[num].Tag));
			}
		}
		if (list2.Count > 0)
		{
			accountAgeingDetails.CustomSlabs = list2.ToArray();
		}
		string text = $"select sName from mCore_Account where iMasterId={iGroupId}";
		accountAgeingDetails.AccountName = Convert.ToString(objDb.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
		if (arrExtraFields != null)
		{
			List<IdValuePair> list3 = new List<IdValuePair>();
			for (num2 = 0; num2 < arrExtraFields.Count; num2++)
			{
				string masterValueFromField = GetMasterValueFromField(arrExtraFields[num2], 4, iGroupId.ToString(), iLanguageId, iCompanyId);
				list3.Add(new IdValuePair(arrExtraFields[num2], masterValueFromField));
			}
			accountAgeingDetails.AccountData = list3.ToArray();
		}
		oReturnData.AgeingDetails = new AccountAgeingDetails[1] { accountAgeingDetails };
	}

	private List<DualIdPair> GetTransactionValues(List<FieldData> arrTransactionFields, int iTransactionId, BackTrackType oLoadType, string sReportFilter, string sTagJoinTable, Database oDb, int iCompanyId)
	{
		int num = 0;
		string text = null;
		string text2 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string strJoinQuery = null;
		QueryGenerator queryGenerator = null;
		RepRecord repRecord = null;
		IDataReader dataReader = null;
		string[] array = null;
		List<string> list = null;
		List<string> list2 = null;
		List<DualIdPair> list3 = null;
		FieldData[] array2 = null;
		try
		{
			iTransactionId = ((oLoadType != BackTrackType.Details) ? iTransactionId : 0);
			array2 = new FieldData[arrTransactionFields.Count];
			for (num = 0; num < arrTransactionFields.Count; num++)
			{
				array2[num] = new FieldData
				{
					FieldId = arrTransactionFields[num].FieldId,
					SubParentId = arrTransactionFields[num].SubParentId,
					ParentId = arrTransactionFields[num].ParentId,
					DataType = arrTransactionFields[num].DataType
				};
				if (arrTransactionFields[num].FieldId != 127)
				{
					array2[num].SubParentId = ((arrTransactionFields[num].SubParentId == 3 || arrTransactionFields[num].SubParentId == 4) ? (-1) : arrTransactionFields[num].SubParentId);
				}
			}
			array = GetExtraFieldNames(array2, -1, bVoucherClass: false, iCompanyId);
			for (num = 0; num < arrTransactionFields.Count && num < array.Length; num++)
			{
				if (array[num].StartsWith("vrCore_Account"))
				{
					array[num] = array[num].Replace("vrCore_Account.", "BookNo.");
				}
				arrTransactionFields[num].FieldName = array[num];
			}
			queryGenerator = new QueryGenerator();
			queryGenerator.Suffix = suffix;
			queryGenerator.m_iCompId = iCompanyId;
			repRecord = new RepRecord();
			list = new List<string>();
			list2 = new List<string>();
			list.AddRange(string.Format("tCore_Header{0},tCore_Data{0},cCore_Vouchers{0}", suffix).Split(','));
			queryGenerator.UpdateJoinQuery(repRecord, arrTransactionFields.ToArray(), list, ref list2, ref list, ref strJoinQuery, null, oDb, bPreviousYear: false);
			if (iTransactionId > 0 || !string.IsNullOrEmpty(sReportFilter))
			{
				for (num = 0; num < arrTransactionFields.Count && num < array.Length; num++)
				{
					if (array[num].StartsWith("mSec_Users"))
					{
						if (arrTransactionFields[num].FieldId == 73)
						{
							array[num] = array[num].Replace("mSec_Users.", "EUser.");
						}
						else if (arrTransactionFields[num].FieldId == 78)
						{
							array[num] = array[num].Replace("mSec_Users.", "MUser.");
						}
					}
				}
				text2 = string.Join(",", array);
				if (!string.IsNullOrEmpty(sReportFilter))
				{
					text2 = $"TOP 1 {text2}";
				}
				if (!string.IsNullOrEmpty(sTagJoinTable) && !strJoinQuery.Contains(sTagJoinTable))
				{
					strJoinQuery += sTagJoinTable;
				}
				text = "SELECT " + text2 + " FROM tCore_Header" + suffix + "\r\n                    JOIN tCore_Data" + suffix + " ON tCore_Data" + suffix + ".iHeaderId = tCore_Header" + suffix + ".iHeaderId\r\n                    JOIN cCore_Vouchers" + suffix + " WITH (ReadUncommitted) ON cCore_Vouchers" + suffix + ".iVoucherType = tCore_Header" + suffix + ".iVoucherType \r\n                    " + strJoinQuery;
				if (string.IsNullOrEmpty(sReportFilter))
				{
					text = ((oLoadType != BackTrackType.Transaction) ? (text + $" WHERE tCore_Header{suffix}.iHeaderId = {iTransactionId}") : (text + $" WHERE tCore_Data{suffix}.iBodyId = {iTransactionId}"));
				}
				else
				{
					text = text + " WHERE 1=1 " + sReportFilter;
				}
				list3 = new List<DualIdPair>();
				dataReader = oDb.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				while (dataReader.Read())
				{
					for (num = 0; num < arrTransactionFields.Count && num < array.Length; num++)
					{
						switch (arrTransactionFields[num].DataType)
						{
						case MasterDataType.Date:
						{
							string objValue = new Date(Convert.ToInt32(dataReader[num]), m_objCalType).ToString();
							list3.Add(new DualIdPair(arrTransactionFields[num].FieldId, arrTransactionFields[num].SubParentId, objValue));
							break;
						}
						case MasterDataType.Time:
							list3.Add(new DualIdPair(arrTransactionFields[num].FieldId, arrTransactionFields[num].SubParentId, FConvert.IntToStringTime(Convert.ToInt32(dataReader[num]))));
							break;
						default:
							list3.Add(new DualIdPair(arrTransactionFields[num].FieldId, arrTransactionFields[num].SubParentId, dataReader[num]));
							break;
						}
					}
				}
				dataReader.Close();
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list3;
	}

	private string GetMonthlyAgeingSlab(decimal[] arrMonth, string[] arrMonthNames)
	{
		string text = string.Empty;
		decimal num = 0m;
		for (int i = 0; i < arrMonth.Length; i++)
		{
			num = arrMonth[i];
			if (num != 0m)
			{
				text = ((!(num < 0m)) ? (text + arrMonthNames[i] + " " + num.AddDecimalInColumn(2) + " Cr") : (text + arrMonthNames[i] + " " + (num * 1m).AddDecimalInColumn(2) + " Dr"));
			}
		}
		return text;
	}

	public LongIdNamePair[] GetTableViewListData(ListControlData objListControlData, int iLanguageId, int iCompanyId)
	{
		string text = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		LongIdNamePair longIdNamePair = null;
		List<LongIdNamePair> list = null;
		FConvert.GetSuffix(iCompanyId);
		text = "SELECT";
		text = (string.IsNullOrEmpty(objListControlData.PrimaryColumn) ? (text + " 0[iId]") : (text + " " + objListControlData.PrimaryColumn + "[iId]"));
		text = (string.IsNullOrEmpty(objListControlData.DisplayColumn) ? (text + ",''[sName]") : (text + "," + objListControlData.DisplayColumn + "[sName]"));
		if (!string.IsNullOrEmpty(objListControlData.MandatoryFields))
		{
			text = text + "," + objListControlData.MandatoryFields;
		}
		text = text + " FROM " + objListControlData.TableView;
		if (!string.IsNullOrEmpty(objListControlData.Filter))
		{
			text = text + " WHERE " + objListControlData.Filter;
		}
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = database.ExecuteReader(dbCommand);
		list = new List<LongIdNamePair>();
		while (dataReader.Read())
		{
			longIdNamePair = new LongIdNamePair(Convert.ToInt64(dataReader["iId"]), Convert.ToString(dataReader["sName"]));
			if (!string.IsNullOrEmpty(objListControlData.MandatoryFields))
			{
				List<string> list2 = new List<string>();
				for (int i = 2; i < dataReader.FieldCount; i++)
				{
					list2.Add(Convert.ToString(dataReader[i]));
				}
				longIdNamePair.Tag = string.Join(",", list2.ToArray());
			}
			list.Add(longIdNamePair);
		}
		dataReader.Close();
		return list.ToArray();
	}

	public OrderByData[] GetOrderByVoucher(OrderByInput oInput, int iCompanyId)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		List<string> list = null;
		string[] array = null;
		Database database = null;
		List<FieldData> list2 = null;
		IDataReader dataReader = null;
		List<OrderByData> list3 = null;
		list3 = new List<OrderByData>();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text2 = _focus.company(iCompanyId).suffix;
			text5 = _focus.company(iCompanyId).faTagName;
			text6 = _focus.company(iCompanyId).invTagName;
			list2 = new List<FieldData>();
			for (num = 0; num < oInput.FieldId.Length; num++)
			{
				int num4 = Convert.ToInt32(oInput.FieldId[num].Value);
				if ((oInput.FieldId[num].ID & 0xFF0000) >> 16 != 128)
				{
					if (num4 == 3 || num4 == 4 || num4 == 12 || num4 == 39)
					{
						num4 = 0;
					}
					else if (oInput.FieldId[num].ID == 35)
					{
						oInput.FieldId[num].ID = 143;
					}
				}
				list2.Add(new FieldData
				{
					FieldId = oInput.FieldId[num].ID,
					SubParentId = num4
				});
			}
			m_bCallingFromExternal = true;
			array = GetExtraFieldNames(list2.ToArray(), oInput.VoucherType, bVoucherClass: false, iCompanyId);
			m_bCallingFromExternal = false;
			if (array.Length == 1 && array[0] == "''" && list2.Count == 1)
			{
				text = string.Format("SELECT 'tCore_IndtaBodyScreenData{0}.mInput'+CAST(iColMap as varchar(5)) FROM cCore_VoucherScreenFields{0} WHERE iFieldId = {1} & 0X00FFFFFF AND bFooter = 0 AND iVoucherType = {2}", text2, list2[0].FieldId, oInput.VoucherType);
				array[0] = Convert.ToString(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
				num3 = 6;
			}
			list = new List<string>();
			if (FConvert.IsItTransfer(oInput.VoucherType))
			{
				flag = true;
			}
			for (num = 0; num < array.Length; num++)
			{
				for (num2 = 0; num2 < list.Count && !(list[num2] == array[num]); num2++)
				{
				}
				if (num2 != list.Count)
				{
					continue;
				}
				if (array[num].StartsWith("vr"))
				{
					if (!array[num].StartsWith("vrCore_Product") && !array[num].StartsWith("vrCore_Account"))
					{
						if (array[num].StartsWith("vrCore_Bins"))
						{
							if (flag && !oInput.IsToMasterIIDST)
							{
								list.Add("tCore_Data" + text2 + ".iMainBodyId");
							}
							text4 = text4 + " LEFT JOIN tCore_Bins" + text2 + " ON tCore_Data" + text2 + ".iBodyId = tCore_Bins" + text2 + ".iBodyId\r\n                                LEFT JOIN vrCore_Bins WITH (ReadUncommitted) ON vrCore_Bins.iMasterId = tCore_Bins" + text2 + ".iBin";
						}
						else if (!string.IsNullOrEmpty(text5) && array[num].StartsWith(text5))
						{
							text4 = text4 + " LEFT JOIN " + text5 + " WITH (ReadUncommitted) ON " + text5 + ".iMasterId = tCore_Data" + text2 + ".iFaTag AND " + text5 + ".iTreeId = 0";
						}
						else if (!string.IsNullOrEmpty(text6) && array[num].StartsWith(text6))
						{
							text4 = text4 + " LEFT JOIN " + text6 + " WITH (ReadUncommitted) ON " + text6 + ".iMasterId = tCore_Data" + text2 + ".iInvTag AND " + text6 + ".iTreeId = 0";
						}
						else if (num < oInput.FieldId.Length)
						{
							int num5 = Convert.ToInt32(oInput.FieldId[num].Value);
							string arg = array[num].Split('.')[0];
							text4 += string.Format(" LEFT JOIN tCore_Data_Tags{0} tags{2} ON tags{2}.iBodyId = tCore_Data{0}.iBodyId \r\n                                    LEFT JOIN {1} ON {1}.iMasterId = tags{2}.iTag{2} ", text2, arg, num5);
						}
					}
				}
				else if (array[num].StartsWith("vCore_TranData"))
				{
					text4 = text4 + " LEFT JOIN vCore_TranData" + text2 + " ON vCore_TranData" + text2 + ".iBodyId = tCore_Data" + text2 + ".iBodyId";
				}
				else if (array[num].StartsWith("vtCore_LinksData"))
				{
					text4 = text4 + " LEFT JOIN vtCore_LinksData" + text2 + " ON vtCore_LinksData" + text2 + ".iBodyId = tCore_Data" + text2 + ".iBodyId";
				}
				else if (array[num].StartsWith("tCore_IndtaBodyScreenData"))
				{
					text4 = text4 + " LEFT JOIN tCore_IndtaBodyScreenData" + text2 + " WITH (ReadUncommitted) ON tCore_IndtaBodyScreenData" + text2 + ".iBodyId = tCore_Data" + text2 + ".iBodyId";
				}
				else if (array[num].StartsWith("tCore_Batch"))
				{
					text4 = text4 + " LEFT JOIN tCore_Batch" + text2 + " WITH (ReadUncommitted) ON tCore_Batch" + text2 + ".iBodyId = tCore_Data" + text2 + ".iBodyId";
				}
				else if (array[num].StartsWith("vtCore_Bins"))
				{
					if (flag && !oInput.IsToMasterIIDST)
					{
						list.Add("tCore_Data" + text2 + ".iMainBodyId");
					}
					text4 = text4 + " LEFT JOIN vtCore_Bins" + text2 + " WITH (ReadUncommitted) ON vtCore_Bins" + text2 + ".iBodyId = tCore_Data" + text2 + ".iBodyId";
				}
				list.Add(array[num]);
			}
			text4 += $" WHERE tCore_Header{text2}.iHeaderId = {oInput.HeaderId}";
			if (flag)
			{
				text4 += string.Format(" AND tCore_Data{0}.iMainBodyId {1} 0", text2, oInput.IsToMasterIIDST ? "<>" : "=");
			}
			if (list.Count > 0)
			{
				text3 = string.Join(",", list);
				if (list2.Count == 1 && num3 == 0)
				{
					if (list2[0].FieldId < 5000)
					{
						num3 = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"SELECT iDataTypeId FROM cCore_Fields WHERE iFieldId = {list2[0].FieldId}") : $"SELECT iDataTypeId FROM cCore_Fields WHERE iFieldId = {list2[0].FieldId}"));
					}
					else
					{
						num3 = (((list2[0].FieldId & 0x1000000) <= 0 && (list2[0].FieldId & 0x4000000) <= 0) ? Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"SELECT b.iDataTypeId FROM cCore_VoucherScreenFields{text2} a join cCore_Fields b on a.iUniqueId = b.iFieldId WHERE a.iFieldId = {list2[0].FieldId} & 0X00FFFFFF and iVoucherType = {oInput.VoucherType}") : $"SELECT b.iDataTypeId FROM cCore_VoucherScreenFields{text2} a join cCore_Fields b on a.iUniqueId = b.iFieldId WHERE a.iFieldId = {list2[0].FieldId} & 0X00FFFFFF and iVoucherType = {oInput.VoucherType}")) : Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"SELECT b.iDataTypeId FROM cCore_VoucherFields{text2} a join cCore_Fields b on a.iUniqueId = b.iFieldId WHERE a.iFieldId = {list2[0].FieldId} & 0X00FFFFFF and iVoucherType = {oInput.VoucherType}") : $"SELECT b.iDataTypeId FROM cCore_VoucherFields{text2} a join cCore_Fields b on a.iUniqueId = b.iFieldId WHERE a.iFieldId = {list2[0].FieldId} & 0X00FFFFFF and iVoucherType = {oInput.VoucherType}")));
					}
				}
				if (list.Count == 1 && num3 == 6)
				{
					text4 = text4 + " ORDER BY LEN(" + text3 + "), " + text3;
					text3 = "," + list[0] + "[sParticular]";
				}
				else
				{
					text4 = text4 + " ORDER BY " + text3.Replace(",''", string.Empty);
					text3 = string.Empty;
					for (num2 = 0; num2 < list.Count; num2++)
					{
						text3 += string.Format(",{0}[sParticular{1}]", list[num2], (num2 > 0) ? num2.ToString() : "");
					}
				}
			}
			else
			{
				text3 = ",sName[sParticular]";
				text4 += " ORDER BY CASE WHEN PATINDEX('%[0-9]%', sName) > 0 THEN LEFT(sName, PATINDEX('%[0-9]%', sName) - 1) ELSE sName END,\r\n                    CASE WHEN PATINDEX('%[0-9]%', sName) > 0 THEN SUBSTRING(sName, PATINDEX('%[0-9]%', sName), LEN(sName)) ELSE sName END";
			}
			text4 += ",iSerialNo";
			string text7 = (oInput.IsToMasterIIDST ? ("tCore_Data" + text2 + ".iMainBodyId") : ("tCore_Data" + text2 + ".iBodyId"));
			text = ((!FConvert.IsItSales(oInput.VoucherType) || text3.StartsWith(",vrCore_Account.")) ? ("SELECT " + text7 + ", vrCore_Account.sName, vrCore_Account.iMasterId" + text3 + "\r\n                    FROM tCore_Header" + text2 + " \r\n                    JOIN tCore_Data" + text2 + " ON tCore_Header" + text2 + ".iHeaderId = tCore_Data" + text2 + ".iHeaderId\r\n                    JOIN vrCore_Account ON (tCore_Data" + text2 + ".iCode = vrCore_Account.iMasterId) AND vrCore_Account.iTreeId = 0 ") : ("SELECT " + text7 + "[iBodyId],vrCore_Product.sName, vrCore_Product.iMasterId" + text3 + "\r\n                    FROM tCore_Header" + text2 + " \r\n                    JOIN tCore_Data" + text2 + " ON tCore_Header" + text2 + ".iHeaderId = tCore_Data" + text2 + ".iHeaderId\r\n                    JOIN tCore_Indta" + text2 + " ON tCore_Data" + text2 + ".iBodyId = tCore_Indta" + text2 + ".iBodyId\r\n                    JOIN vrCore_Product ON tCore_Indta" + text2 + ".iProduct = vrCore_Product.iMasterId AND vrCore_Product.iTreeId = 0\r\n                    LEFT JOIN vrCore_Account ON (tCore_Data" + text2 + ".iCode = vrCore_Account.iMasterId) AND vrCore_Account.iTreeId = 0"));
			text += text4;
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				text3 = Convert.ToString(dataReader["sParticular"]);
				if (list.Count > 1)
				{
					for (num2 = 1; num2 < list.Count; num2++)
					{
						text3 += $",{dataReader[$"sParticular{num2}"]}";
					}
				}
				list3.Add(new OrderByData
				{
					BodyId = Convert.ToInt32(dataReader["iBodyId"]),
					ID = Convert.ToInt32(dataReader["iMasterId"]),
					Name = Convert.ToString(dataReader["sName"]),
					Particulars = text3
				});
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list3.ToArray();
	}

	public IdNamePair GetAttachmentData(IdNamePair objInput, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		int num = 0;
		bool flag = true;
		Database database = null;
		IDataReader dataReader = null;
		IdNamePair idNamePair = null;
		byte[] array = null;
		FMongoDb fMongoDb = null;
		idNamePair = new IdNamePair();
		try
		{
			if (objInput != null && objInput.ID > 0 && objInput.Tag != null && !string.IsNullOrEmpty(objInput.Name))
			{
				database = DatabaseWrapper.GetDatabase2(iCompanyId);
				text3 = FConvert.GetSuffix(iCompanyId);
				if (Convert.ToInt32(objInput.Tag) == 3)
				{
					text = $"SELECT TOP 1 'vr'+sModule+'_'+sMasterName+'.'+sFieldName\r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {Convert.ToInt32(objInput.Name) & 0xFFFFFF}";
					objInput.Name = Convert.ToString(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
					if (!string.IsNullOrEmpty(objInput.Name))
					{
						string[] array2 = objInput.Name.Split('.');
						text = string.Format("SELECT {1}, {1}Name FROM {0} WHERE iMasterId = {2}", array2[0], objInput.Name, objInput.ID);
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
						if (dataReader.Read())
						{
							idNamePair.ID = objInput.ID;
							idNamePair.Name = Convert.ToString(dataReader[1]);
							idNamePair.Tag = ((dataReader[1] == DBNull.Value) ? null : dataReader[0]);
						}
						dataReader.Close();
					}
				}
				else
				{
					switch ((LoadTransactionBy)(byte)Convert.ToInt32(objInput.Tag))
					{
					case LoadTransactionBy.BodyId:
						text = string.Format("SELECT iVoucherType FROM tCore_Header{0} JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId WHERE iBodyId = {1}", text3, objInput.ID);
						break;
					case LoadTransactionBy.TransactionId:
						text = string.Format("SELECT iVoucherType FROM tCore_Header{0} JOIN tCore_Data{0} ON tCore_Header{0}.iHeaderId = tCore_Data{0}.iHeaderId WHERE iTransactionId = {1}", text3, objInput.ID);
						break;
					case LoadTransactionBy.HeaderId:
						text = $"SELECT iVoucherType FROM tCore_Header{text3} WHERE iHeaderId = {objInput.ID}";
						break;
					}
					num = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
					if (num > 0 && FConvert.IsNumeric(objInput.Name))
					{
						text = string.Format("SELECT bHeader, sFieldName FROM cCore_VoucherFields{0} WHERE iUniqueId = {2} AND iVoucherType = {1}", text3, num, Convert.ToInt32(objInput.Name) & 0xFFFFFF);
						dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
						if (dataReader.Read())
						{
							flag = Convert.ToBoolean(dataReader["bHeader"]);
							objInput.Name = Convert.ToString(dataReader["sFieldName"]);
						}
						dataReader.Close();
						switch ((LoadTransactionBy)(byte)Convert.ToInt32(objInput.Tag))
						{
						case LoadTransactionBy.BodyId:
							if (flag)
							{
								text2 = $"SELECT iHeaderId FROM tCore_Data{text3} WHERE iBodyId = {objInput.ID}";
							}
							break;
						case LoadTransactionBy.TransactionId:
							text2 = ((!flag) ? $"SELECT iBodyId FROM tCore_Data{text3} WHERE iTransactionId = {objInput.ID}" : $"SELECT iHeaderId FROM tCore_Data{text3} WHERE iTransactionId = {objInput.ID}");
							break;
						case LoadTransactionBy.HeaderId:
							if (!flag)
							{
								text2 = $"SELECT TOP 1 iBodyId FROM tCore_Data{text3} WHERE iHeaderId = {objInput.ID}";
							}
							break;
						}
						if (!string.IsNullOrEmpty(text2))
						{
							objInput.ID = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2));
						}
						if (!string.IsNullOrEmpty(objInput.Name))
						{
							text = ((!flag) ? string.Format("SELECT {2} FROM tCore_Data{1}{0} WHERE iBodyId = {3}", text3, num, objInput.Name, objInput.ID) : string.Format("SELECT {2}, {2}File FROM tCore_Docs{1}{0} WHERE iHeaderId = {3}", text3, num, objInput.Name, objInput.ID));
							dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
							if (dataReader.Read())
							{
								idNamePair.ID = objInput.ID;
								if (_focus.company(iCompanyId).mongoEnable)
								{
									idNamePair.Name = Convert.ToString(dataReader[0]);
									if (!string.IsNullOrEmpty(idNamePair.Name))
									{
										if (fMongoDb == null)
										{
											fMongoDb = new FMongoDb(FConvert.CompanyCodeFromId(iCompanyId));
										}
										array = fMongoDb.GetDocFromMongo(idNamePair.Name);
										if (array != null)
										{
											if (flag)
											{
												idNamePair.Tag = array;
											}
											else
											{
												idNamePair.Tag = ((array.GetType() == typeof(byte[])) ? FConvert.ByteArrayToObject(array) : null);
												if (idNamePair.Tag != null && idNamePair.Tag.GetType() == typeof(DocumentData))
												{
													idNamePair.Name = ((DocumentData)idNamePair.Tag).FileName;
													idNamePair.Tag = ((DocumentData)idNamePair.Tag).FileData;
												}
											}
										}
									}
									if (flag)
									{
										idNamePair.Name = Convert.ToString(dataReader[1]);
									}
								}
								else if (flag)
								{
									idNamePair.Name = Convert.ToString(dataReader[1]);
									idNamePair.Tag = ((dataReader[1] == DBNull.Value) ? null : dataReader[0]);
								}
								else
								{
									idNamePair.Tag = ((dataReader[0] == DBNull.Value) ? null : FConvert.ByteArrayToObject((byte[])dataReader[0]));
									if (idNamePair.Tag != null && idNamePair.Tag.GetType() == typeof(DocumentData))
									{
										idNamePair.Name = ((DocumentData)idNamePair.Tag).FileName;
										idNamePair.Tag = ((DocumentData)idNamePair.Tag).FileData;
									}
								}
							}
							dataReader.Close();
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			idNamePair.ID = -1;
			idNamePair.Name = (m_sError = ex.Message);
		}
		return idNamePair;
	}

	public IdNamePair GetAttachmentFromHeaderId(IdNamePair objInput, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		Database database = null;
		IDataReader dataReader = null;
		IdNamePair idNamePair = null;
		idNamePair = new IdNamePair();
		if (objInput != null && objInput.ID > 0 && objInput.Tag != null && !string.IsNullOrEmpty(objInput.Name))
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text2 = FConvert.GetSuffix(iCompanyId);
			text = string.Format("SELECT {2}, {2}File FROM tCore_Docs{1}{0} WHERE iHeaderId = {3}", text2, objInput.Tag, objInput.Name, objInput.ID);
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			if (dataReader.Read())
			{
				idNamePair.ID = objInput.ID;
				idNamePair.Name = Convert.ToString(dataReader[1]);
				idNamePair.Tag = ((dataReader[1] == DBNull.Value) ? null : dataReader[0]);
			}
			dataReader.Close();
		}
		return idNamePair;
	}

	public string GetPictureFromHeaderId(IdNamePair objInput, int iCompanyId)
	{
		string text = null;
		string text2 = null;
		Database database = null;
		IDataReader dataReader = null;
		string result = string.Empty;
		object sId = null;
		byte[] array = null;
		if (objInput != null && objInput.ID > 0 && objInput.Tag != null && !string.IsNullOrEmpty(objInput.Name))
		{
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text2 = FConvert.GetSuffix(iCompanyId);
			text = string.Format("SELECT {2} FROM tCore_Docs{1}{0} WHERE iHeaderId = {3}", text2, objInput.Tag, objInput.Name, objInput.ID);
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			if (dataReader.Read())
			{
				sId = ((dataReader[0] == DBNull.Value) ? null : dataReader[0]);
			}
			dataReader.Close();
			int preferenceValue = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.DocumentStorage, 1);
			if (sId != null)
			{
				if (_focus.company(iCompanyId).mongoEnable)
				{
					array = new FMongoDb(FConvert.CompanyCodeFromId(iCompanyId)).GetDocFromMongo(Convert.ToString(sId));
				}
				else if ((byte)preferenceValue == 3)
				{
					SMTPSettings objMailSettings = null;
					Focus.Common.BL.Utilities utilities = new Focus.Common.BL.Utilities();
					objMailSettings = utilities.LoadMailSettings(iCompanyId);
					array = System.Threading.Tasks.Task.Run(() => OneDriveUploader.DownloadFile(objMailSettings.TenantId, objMailSettings.ClientId, objMailSettings.ClientSecret, objMailSettings.FromUserName, Convert.ToString(sId))).Result;
				}
			}
			result = ((array == null) ? Convert.ToBase64String((byte[])sId) : Convert.ToBase64String(array));
		}
		return result;
	}

	public IdNamePair[] GetBOMExtraFields(int iCompanyId)
	{
		string text = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = null;
		list = new List<IdNamePair>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = $"SELECT Id, sCaption FROM mMRP_ExtraFields WHERE iScreenId = 2 AND iFieldType IN({1},{6})";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["Id"]), Convert.ToString(dataReader["sCaption"])));
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public int[] LoadUserLayouts(int iReportId, LayoutSecurityType oType, int iUserId, int iCompanyId, ref string sError)
	{
		bool flag = false;
		string suffix = FConvert.GetSuffix(iCompanyId);
		List<int> list = new List<int>();
		try
		{
			if (iUserId > 0)
			{
				Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
				string text = ((oType != LayoutSecurityType.Invoice) ? string.Format("SELECT ISNULL(iUserOrRoleId,0)[iUserOrRoleId], cCore_ReportLayouts{0}.iLayoutId \r\n                        FROM cCore_ReportLayouts{0} \r\n                        LEFT JOIN cCore_ReportUserSecurity{0} ON cCore_ReportUserSecurity{0}.iLayoutId = cCore_ReportLayouts{0}.iLayoutId AND ISNULL(cCore_ReportUserSecurity{0}.iType, 0) = 0\r\n                        WHERE iReportId = {2} ", suffix, iUserId, iReportId) : string.Format("SELECT ISNULL(iUserOrRoleId, 0)[iUserOrRoleId], cCore_InvoiceLayout{0}.iLayoutId \r\n                        FROM cCore_InvoiceLayout{0} \r\n                        LEFT JOIN cCore_ReportUserSecurity{0} ON cCore_ReportUserSecurity{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId AND ISNULL(cCore_ReportUserSecurity{0}.iType, 0) = 1\r\n                        WHERE iReportId = {2} ", suffix, iUserId, iReportId));
				IDataReader dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				while (dataReader.Read())
				{
					flag = true;
					if (Convert.ToInt32(dataReader["iUserOrRoleId"]) == iUserId || Convert.ToInt32(dataReader["iUserOrRoleId"]) == 0)
					{
						list.Add(Convert.ToInt32(dataReader["iLayoutId"]));
					}
				}
				dataReader.Close();
				if (flag && list.Count == 0)
				{
					list.Add(-2);
				}
			}
		}
		catch (Exception ex)
		{
			m_sError = (sError = ex.Message);
		}
		return list.ToArray();
	}

	public List<DualIdNamePair> UpdateFieldBeforePrint(List<DualIdNamePair> arrFields, Transaction objTranData, int iUserId, int iCompanyId)
	{
		int num = 0;
		if (arrFields != null)
		{
			for (num = 0; num < arrFields.Count; num++)
			{
				switch ((InvoiceFieldType)arrFields[num].Id)
				{
				case InvoiceFieldType.LetterOfCredit:
					arrFields[num].Value = GetLCDetails(objTranData.Header.DocNo, iCompanyId);
					break;
				case InvoiceFieldType.User:
					arrFields[num].Value = GetUsersContactDetails(objTranData.Header.HeaderId, iUserId, iCompanyId);
					break;
				}
			}
		}
		return arrFields;
	}

	public List<IdNamePair> UpdatePrintData(LayoutInformation oLayout, Transaction objTranData, int iLanguageId, int iAltLanguageId, int iCompanyId, ref List<HeaderGroup> arrHeaderGroup, ref List<HeaderGroup> arrBodyGroup)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		PageHeader pageHeader = null;
		PageBody pageBody = null;
		List<IdNamePair> list = null;
		_objTranData = objTranData;
		_iLanguageId = iLanguageId;
		_iAltLanguageId = iAltLanguageId;
		_iCompanyId = iCompanyId;
		list = new List<IdNamePair>();
		for (num2 = 0; num2 < oLayout.Pages.Length; num2++)
		{
			for (num = 0; num < oLayout.Pages[num2].PageHeader.Length; num++)
			{
				pageHeader = oLayout.Pages[num2].PageHeader[num];
				switch (pageHeader.Type)
				{
				case ControlType.Formula:
					list.Add(new IdNamePair(pageHeader.FieldId, pageHeader.Text, Convert.ToString(EvaluateInvoiceFormula(pageHeader.Text, -1, _objTranData, pageHeader.StaticTextProperties, null, _iCompanyId))));
					break;
				case ControlType.Statictext:
					SetValueText(ref arrHeaderGroup, ref arrBodyGroup, pageHeader, 0);
					break;
				case ControlType.Textblock:
					SetCurrentValue(ref arrHeaderGroup, ref arrBodyGroup, pageHeader.Text, pageHeader.StaticTextProperties, pageHeader.FieldId, pageHeader.SubParentId, pageHeader.StaticTextProperties.Sign, bIsFromHeader: true, (short)pageHeader.StaticTextProperties.DataType);
					break;
				case ControlType.BodyCanvas:
					for (num3 = 0; num3 < pageHeader.PageBody.Length; num3++)
					{
						pageBody = pageHeader.PageBody[num3];
						StaticTextClass staticTextClass = new StaticTextClass();
						staticTextClass.DecimalInColumn = (byte)pageBody.DecimalInColumn;
						staticTextClass.InsertComma = pageBody.InsertCommas;
						staticTextClass.Sign = pageBody.Sign;
						staticTextClass.RoundOffType = pageBody.RoundOffType;
						staticTextClass.RoundUptoValue = pageBody.RoundUptoValue;
						if (pageBody.FieldId == 111)
						{
							list.Add(new IdNamePair(pageBody.BodyId, pageBody.Formula, Convert.ToString(EvaluateInvoiceFormula(pageBody.Formula, -1, _objTranData, pageHeader.StaticTextProperties, null, _iCompanyId))));
						}
						else
						{
							SetCurrentValue(ref arrHeaderGroup, ref arrBodyGroup, pageBody.Column, staticTextClass, pageBody.FieldId, pageBody.SubParentId, pageBody.Sign, bIsFromHeader: false, pageBody.DataType);
						}
					}
					break;
				}
			}
		}
		return list;
	}

	private void SetCurrentValue(ref List<HeaderGroup> arrHeaderGroup, ref List<HeaderGroup> arrBodyGroup, string strVariables, StaticTextClass objClass, int iFieldId, int iSubParentId, Sign oSign, bool bIsFromHeader = false, short iDataType = 0)
	{
		string sValue = null;
		string[] arrData = null;
		if (!string.IsNullOrEmpty(strVariables))
		{
			arrData = strVariables.Split('.');
			UpdateFieldValue(ref arrData);
		}
		GetValueFromGroup(ref arrHeaderGroup, arrData, objClass, iFieldId, iSubParentId, bBodyGroup: false, bIsFromHeader, iDataType, ref sValue);
		if (sValue == null)
		{
			GetValueFromGroup(ref arrBodyGroup, arrData, objClass, iFieldId, iSubParentId, bBodyGroup: true, bIsFromHeader, iDataType, ref sValue);
		}
	}

	private void GetValueFromGroup(ref List<HeaderGroup> arrGroup, string[] arrData, StaticTextClass objTextClass, int iFieldId, int iSubParentId, bool bBodyGroup, bool bIsCallingFromHeader, short iDataType, ref string sValue)
	{
		int num = 0;
		int num2 = -1;
		if (arrGroup == null)
		{
			return;
		}
		for (num = 0; num < arrGroup.Count; num++)
		{
			if (arrGroup[num].GroupName.ToLower() == arrData[0].ToLower())
			{
				num2 = SetDataTypeOnValue(arrGroup[num].Fields, arrData[0], arrData[1], objTextClass, iFieldId, iSubParentId, bBodyGroup, bIsCallingFromHeader, iDataType, ref sValue);
				if (num2 > -1 && !string.IsNullOrEmpty(sValue))
				{
					TemplateFields obj = arrGroup[num].Fields[num2];
					object[] values = sValue.Split('.');
					obj.Values = values;
				}
			}
			if (sValue == null)
			{
				num2 = SetDataTypeOnValue(arrGroup[num].Fields, arrData[0], arrData[1], objTextClass, iFieldId, iSubParentId, bBodyGroup, bIsCallingFromHeader, iDataType, ref sValue);
				if (num2 > -1 && !string.IsNullOrEmpty(sValue))
				{
					TemplateFields obj2 = arrGroup[num].Fields[num2];
					object[] values = sValue.Split('.');
					obj2.Values = values;
				}
			}
			if (sValue != null)
			{
				break;
			}
		}
	}

	private int SetDataTypeOnValue(TemplateFields[] arrFields, string sHeader, string sField, StaticTextClass objTextClass, int iFieldId, int iSubParentId, bool bBodyGroup, bool bIsCallingFromHeader, short iDataType, ref string sValue)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		string text = "<B>";
		for (num = 0; num < arrFields.Length; num++)
		{
			if (arrFields[num].Name.ToLower() == sHeader.ToLower())
			{
				flag2 = true;
				break;
			}
		}
		if (((num == arrFields.Length) & bBodyGroup) && iSubParentId > 0 && iDataType == 12)
		{
			for (num = 0; num < arrFields.Length; num++)
			{
				if (arrFields[num].DataType == MasterDataType.Master && arrFields[num].MasterType == iSubParentId)
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2 && num < arrFields.Length && arrFields[num].DataType == MasterDataType.Master && iFieldId > 0 && !string.IsNullOrEmpty(sValue))
		{
			int subFieldId = RDCommon.GetSubFieldId(ref iFieldId);
			switch (iFieldId)
			{
			case 10:
			case 14:
				sValue = GetMasterValueFromField(10, subFieldId, sValue, -2, _iCompanyId);
				break;
			case 130:
				sValue = GetMasterValueFromField(iFieldId, subFieldId, sValue, -3, _iCompanyId);
				break;
			default:
			{
				if (sValue.Contains(text))
				{
					sValue = sValue.Replace(text, string.Empty);
					flag = true;
				}
				int altLanguageId = (sField.EndsWith(" Code") ? (-4) : _iAltLanguageId);
				IdValuePair[] mastersValueFromFields = GetMastersValueFromFields(new FieldInfoInput
				{
					FieldId = iFieldId,
					SubParentId = iSubParentId,
					AltLanguageId = altLanguageId,
					LanguageId = _iLanguageId,
					MasterIds = sField
				}, _iCompanyId);
				if (mastersValueFromFields != null && mastersValueFromFields.Length != 0)
				{
					sValue = Convert.ToString(mastersValueFromFields[0].Value);
				}
				if (flag)
				{
					sValue = text + sValue;
				}
				break;
			}
			}
		}
		return num;
	}

	private string SetValueText(ref List<HeaderGroup> arrHeaderGroup, ref List<HeaderGroup> arrBodyGroup, PageHeader info, int itemIndex)
	{
		int num = -1;
		int num2 = -1;
		List<string> list = null;
		string text = "&&";
		string text2 = "<<";
		string empty = string.Empty;
		string empty2 = string.Empty;
		string text3 = info.Text;
		if (text3.Contains(text))
		{
			num = text3.IndexOf(text, 0, text3.Length) + 2;
			num2 = text3.Length;
			if (num2 == -1 || num == -1)
			{
				return text3;
			}
			empty = text3.Substring(num, num2 - num);
			empty = empty.Replace("\n", "");
			empty = empty.Replace("\r", "");
			SetCurrentValue(ref arrHeaderGroup, ref arrBodyGroup, empty, info.StaticTextProperties, info.FieldId, info.SubParentId, info.StaticTextProperties.Sign, bIsFromHeader: true, (short)info.StaticTextProperties.DataType);
			if (empty2.Length == 0)
			{
				text3 = string.Empty;
			}
			else if (num == 2)
			{
				text3 = empty2;
			}
			else
			{
				text3 = text3.Replace(empty, string.Empty);
				text3 = text3.Replace(text, "");
			}
		}
		else if (text3.Contains(text2))
		{
			list = new List<string>();
			string[] array = text3.Split(',');
			foreach (string text4 in array)
			{
				if (!string.IsNullOrEmpty(text4))
				{
					num = text3.IndexOf(text2, 0, text4.Length) + text2.Length;
					num2 = text4.Length - (text3.Contains(text2) ? text2.Length : 0);
					if (num2 == -1 || num == -1)
					{
						list.Add(text3);
					}
					empty = text4.Substring(num, num2 - num);
					empty = empty.Replace("\n", "");
					empty = empty.Replace("\r", "");
					SetCurrentValue(ref arrHeaderGroup, ref arrBodyGroup, empty, info.StaticTextProperties, info.FieldId, info.SubParentId, info.StaticTextProperties.Sign, bIsFromHeader: true, (short)info.StaticTextProperties.DataType);
					if (empty2.Length > 0)
					{
						list.Add(empty2);
					}
				}
			}
			text3 = string.Join(",", list.ToArray());
		}
		return text3;
	}

	private void UpdateFieldValue(ref string[] arrData)
	{
		if (arrData != null && arrData.Length > 2)
		{
			string text = string.Empty;
			string text2 = arrData[0];
			for (int i = 1; i < arrData.Length; i++)
			{
				text += string.Format("{0}{1}", (i > 1) ? "." : "", arrData[i]);
			}
			arrData = new string[2] { text2, text };
		}
	}

	public string Save(LayoutInformation info, int iCompId)
	{
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		short num4 = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbConnection dbConnection = database.CreateConnection();
		DbTransaction dbTransaction = null;
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = new StringBuilder();
		using (dbConnection = database.CreateConnection())
		{
			try
			{
				dbConnection.Open();
				dbTransaction = dbConnection.BeginTransaction();
				if (info.Layout.ID != ERRORNO)
				{
					m_bDeleteLayout = false;
					empty = Delete(info.Layout.ID, MasterType.InvoiceDesigner, iCompId, dbConnection, dbTransaction);
					if (empty.Length > 0)
					{
						return empty;
					}
				}
				if (info.Layout.ID == ERRORNO)
				{
					empty = string.Format("INSERT INTO cCore_InvoiceLayout{13} (iReportId, sLayout, fPageWidth, fPageHeight, \r\n                            fLeftMargin, fTopMargin, fRightMargin, fBottomMargin, iPageUnit, iPageView, iModule, iReportType, sHtml)\r\n                            VALUES({0}, N'{1}', {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}');\r\n                            SELECT @@IDENTITY", info.ReportId, info.Layout.Name, info.PrintInfo.PageWidth, info.PrintInfo.PageHeight, info.PrintInfo.Margin.Left, info.PrintInfo.Margin.Top, info.PrintInfo.Margin.Right, info.PrintInfo.Margin.Bottom, (byte)info.PrintInfo.Unit, (byte)info.PrintInfo.View, (byte)info.Module, (byte)info.ReportType, info.HtmlSource, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num4 = 1;
					info.Layout.ID = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
				}
				else
				{
					empty = string.Format("UPDATE cCore_InvoiceLayout{13} SET sLayout=N'{0}', fPageWidth={1}, fPageHeight={2}, fLeftMargin={3},\r\n                                fTopMargin={4}, fRightMargin={5}, fBottomMargin={6}, iPageUnit={7}, iPageView={8}, iModule={9}, iReportType={10}, sHtml='{12} '\r\n                                where iLayoutId={11}", info.Layout.Name, info.PrintInfo.PageWidth, info.PrintInfo.PageHeight, info.PrintInfo.Margin.Left, info.PrintInfo.Margin.Top, info.PrintInfo.Margin.Right, info.PrintInfo.Margin.Bottom, (byte)info.PrintInfo.Unit, (byte)info.PrintInfo.View, (byte)info.Module, (byte)info.ReportType, info.Layout.ID, info.HtmlSource, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num4 = 1;
					database.ExecuteNonQuery(dbCommand, dbTransaction);
				}
				if (info.Layout.ID <= 0)
				{
					dbTransaction.Rollback();
					return "!1Save Failed in Layout table";
				}
				for (num = 0; num < info.Pages.Length; num++)
				{
					empty = string.Format("INSERT INTO cCore_InvoicePage{1} (iLayoutId) VALUES({0});SELECT @@IDENTITY", info.Layout.ID, suffix);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					num4 = 2;
					num3 = Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction));
					num2 = 0;
					PageHeader[] pageHeader = info.Pages[num].PageHeader;
					foreach (PageHeader pageHeader2 in pageHeader)
					{
						if (pageHeader2.ImageSource == null)
						{
							empty = string.Format("INSERT INTO cCore_InvoiceHeader{21} (iHeaderId, iPageId, iControlType, sText, \r\n                                fLeft, fTop, fWidth, fHeight, fBorderThickness, iBorderColor, iBackColor, iTextColor, sTextFont, \r\n                                fFontSize, iFontWeights, iFontStyle, iVariable, iShowHeader, iCategoryId, iFieldId, bArabicDigit) \r\n                                VALUES ({0}, {1}, {2}, N'{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}', {13}, {14}, \r\n                                {15}, {16}, {17}, {18}, {19}, {20});", pageHeader2.UID, num3, Convert.ToInt16(pageHeader2.Type), pageHeader2.Text, pageHeader2.Left, pageHeader2.Top, pageHeader2.Width, pageHeader2.Height, pageHeader2.BorderThickness, pageHeader2.BorderColor, pageHeader2.BackColor, pageHeader2.TextColor, pageHeader2.TextFont, pageHeader2.FontSize, Convert.ToInt16(pageHeader2.FontWeight), Convert.ToInt16(pageHeader2.FontStyle), Convert.ToInt16(pageHeader2.VariableType), Convert.ToInt16(pageHeader2.ShowOnPage), pageHeader2.MasterId, pageHeader2.FieldId, Convert.ToByte(pageHeader2.IsArabicDigit), suffix);
							if (pageHeader2.UID > 2000)
							{
								empty += $"INSERT INTO cCore_InvoiceNumericProperty{suffix} (iHeaderId, iPageId, iAlignment, \r\n                                    iDecimalInColumn, iRoundOffType, fRoundOffValue) VALUES ({pageHeader2.UID}, {num3}, {pageHeader2.Alignment}, {pageHeader2.DecimalInColumn}, {(byte)pageHeader2.RoundOffType}, {pageHeader2.RoundUptoValue});";
							}
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty.ToString()) : empty.ToString());
						}
						else
						{
							empty = string.Format("INSERT INTO cCore_InvoiceHeader{22} (iHeaderId, iPageId, iControlType, sText,\r\n                                fLeft, fTop, fWidth, fHeight, fBorderThickness, iBorderColor, iBackColor, iTextColor, sTextFont, \r\n                                fFontSize, iFontWeights, iFontStyle, imgControl, iVariable, iShowHeader, iCategoryId, iFieldId, bArabicDigit) \r\n                                VALUES ({0}, {1}, {2}, N'{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}', {13}, {14}, \r\n                                {15}, @image{16}, {17}, {18}, {19}, {20}, {21});", pageHeader2.UID, num3, Convert.ToInt16(pageHeader2.Type), pageHeader2.Text, pageHeader2.Left, pageHeader2.Top, pageHeader2.Width, pageHeader2.Height, pageHeader2.BorderThickness, pageHeader2.BorderColor, pageHeader2.BackColor, pageHeader2.TextColor, pageHeader2.TextFont, pageHeader2.FontSize, Convert.ToInt16(pageHeader2.FontWeight), Convert.ToInt16(pageHeader2.FontStyle), num2, Convert.ToInt16(pageHeader2.VariableType), Convert.ToInt16(pageHeader2.ShowOnPage), pageHeader2.MasterId, pageHeader2.FieldId, Convert.ToByte(pageHeader2.IsArabicDigit), suffix);
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty.ToString()) : empty.ToString());
							database.AddInParameter(dbCommand, $"@image{num2}", DbType.Binary, pageHeader2.ImageSource);
						}
						num2++;
						num4 = 3;
						database.ExecuteNonQuery(dbCommand, dbTransaction);
						if (pageHeader2.UID <= 2000)
						{
							stringBuilder = new StringBuilder();
							PageBody[] pageBody = pageHeader2.PageBody;
							foreach (PageBody pageBody2 in pageBody)
							{
								stringBuilder.Append(string.Format("INSERT INTO cCore_InvoiceBody{0} (iBodyId, iPageId, sColumn, sAlias, \r\n                                    iDataType, fColumnWidth, iDisplayColumnIndex, iSign, iCategoryId, iFieldId, bWordWrap, \r\n                                    bPrintUnderPrevColumn, iFunction, bArabicDigit) \r\n                                    VALUES ({1}, {2}, N'{3}', N'{4}', {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14});", suffix, pageHeader2.UID, num3, pageBody2.Column, pageBody2.Alias, pageBody2.DataType, pageBody2.ColumnWidth, pageBody2.ColumnIndex, Convert.ToInt16(pageBody2.Sign), pageBody2.MasterId, pageBody2.FieldId, Convert.ToByte(pageBody2.WordWrap), Convert.ToByte(pageBody2.PrintUnderPreviousColumn), (byte)pageBody2.RoundOffType, pageBody2.RoundUptoValue, (int)pageBody2.FunctionType, Convert.ToByte(pageBody2.IsArabicDigit)));
								stringBuilder.AppendFormat("INSERT INTO cCore_InvoiceNumericProperty{0} (iHeaderId, iPageId, iAlignment, \r\n                                        iDecimalInColumn, iRoundOffType, fRoundOffValue) VALUES ({1}, {2}, {3}, {4}, {5}, {6});", suffix, pageHeader2.UID, num3, (byte)pageBody2.Alignment, pageBody2.DecimalInColumn, (byte)pageBody2.RoundOffType, pageBody2.RoundUptoValue);
							}
							if (stringBuilder.Length > 0)
							{
								dbCommand.Parameters.Clear();
								dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
								num4 = 4;
								database.ExecuteNonQuery(dbCommand, dbTransaction);
							}
						}
					}
				}
				dbTransaction.Commit();
			}
			catch (Exception ex)
			{
				dbTransaction.Rollback();
				m_sError = ex.Message;
				return $"!{num4}{ex.Message}";
			}
			finally
			{
				if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
				{
					dbCommand.Connection.Close();
				}
				if (dbConnection.State != ConnectionState.Closed)
				{
					dbConnection.Close();
				}
			}
			return info.Layout.ID.ToString();
		}
	}

	public ReturnStatus SaveScreenCustomization(ScreenCustomization oData, int iCompId)
	{
		ReturnStatus result = ReturnStatus.NoError;
		string empty = string.Empty;
		string suffix = FConvert.GetSuffix(iCompId);
		DbCommand dbCommand = null;
		Database database = null;
		DbTransaction dbTransaction = null;
		DbConnection dbConnection = null;
		database = DatabaseWrapper.GetDatabase2(iCompId);
		try
		{
			using (dbConnection = database.CreateConnection())
			{
				dbConnection.Open();
				using (dbTransaction = dbConnection.BeginTransaction())
				{
					if (oData.ScreenId > 0)
					{
						empty = $"Delete from cCore_ScreenCustomization{suffix} where iScreenId = {oData.ScreenId}";
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
						database.ExecuteNonQuery(dbCommand, dbTransaction);
						if (oData.FieldIds != null && oData.FieldIds.Length != 0)
						{
							for (int i = 0; i < oData.FieldIds.Length; i++)
							{
								empty = $"INSERT INTO cCore_ScreenCustomization{suffix}(iScreenId, iFieldId) VALUES ({oData.ScreenId},{oData.FieldIds[i]})";
								dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
								database.ExecuteNonQuery(dbCommand, dbTransaction).ToString();
							}
						}
					}
					dbTransaction.Commit();
				}
				dbConnection.Close();
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			dbTransaction.Rollback();
			result = ReturnStatus.WrongTableDef;
		}
		finally
		{
			if (dbConnection != null && dbConnection.State == ConnectionState.Open)
			{
				dbConnection.Close();
			}
		}
		return result;
	}

	public Output LoadScreenCustomization(int oData, int iCompId)
	{
		Output output = new Output();
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		ScreenCustomization screenCustomization = new ScreenCustomization();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<int> list = new List<int>();
		new List<_Parameter>();
		string empty = string.Empty;
		try
		{
			empty = $"SELECT iScreenId, iFieldId FROM cCore_ScreenCustomization{FConvert.GetSuffix(iCompId)} WHERE iScreenId= {oData}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				screenCustomization.ScreenId = Convert.ToInt32(dataReader["iScreenId"]);
				list.Add(Convert.ToInt32(dataReader["iFieldId"]));
			}
			if (list != null && list.Count > 0)
			{
				screenCustomization.FieldIds = list.ToArray();
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			screenCustomization.ScreenId = 0;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		output.ReturnData = screenCustomization;
		return output;
	}

	public ReportInputParameters LoadReportParameter(int iReportId, ReportType oType, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		ReportInputParameters reportInputParameters = new ReportInputParameters();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		_Parameter parameter = null;
		RDDefault rDDefault = null;
		int num = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		string[] array = null;
		List<_Parameter> list = new List<_Parameter>();
		string empty = string.Empty;
		bool bInvSlabs = false;
		bool bFaSlabs = false;
		bool bVat = false;
		bool bPdc = false;
		bool bSubledger = false;
		bool bBudget = false;
		bool bFixAsset = false;
		bool bWms = false;
		try
		{
			ReportSepecificParam(iReportId, ref bInvSlabs, ref bFaSlabs, ref bVat, ref bPdc, ref bBudget, ref bFixAsset, ref bWms, ref bSubledger);
			rDDefault = new RDDefault();
			if (bSubledger)
			{
				rDDefault.IsSubLedgerEnabled = _focus.company(iCompId).isModuleImplemented(ModulesImplemented.SubLedger);
			}
			if (bPdc)
			{
				rDDefault.IsDisplayPDCBasedOnDueDate = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.PDC, 4) > 0;
			}
			if (bInvSlabs)
			{
				rDDefault.InventorySlabs = _focus.company(iCompId).getPreferenceTexts(PreferenceCategories.Print, 150, 200);
			}
			if (bFaSlabs)
			{
				rDDefault.FinanceSlabs = _focus.company(iCompId).getPreferenceTexts(PreferenceCategories.Print, 100, 150);
			}
			if (bVat)
			{
				rDDefault.VATTagId = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 14);
				rDDefault.VATTagName = _focus.company(iCompId).master(rDDefault.VATTagId).caption;
			}
			if (bBudget)
			{
				rDDefault.BudgetTag1Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 4);
				rDDefault.BudgetTag2Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 5);
				rDDefault.BudgetTag3Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			}
			rDDefault.Masters = (from p in _focus.company(iCompId).getAllMasters()
				select new IdNamePair(p.ID, (!string.IsNullOrEmpty(p.ExtraInfo)) ? p.ExtraInfo : p.Name, p.Tag)).ToArray();
			rDDefault.FaTagId = _focus.company(iCompId).faTagId;
			rDDefault.InvTagId = _focus.company(iCompId).invTagId;
			rDDefault.FaTagName = _focus.company(iCompId).master(rDDefault.FaTagId).caption;
			rDDefault.InvTagName = _focus.company(iCompId).master(rDDefault.InvTagId).caption;
			if (bFixAsset)
			{
				rDDefault.iFixAssTag0 = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.FixedAssets, 18);
			}
			if (bWms)
			{
				rDDefault.iInvAllocTag = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 16);
			}
			reportInputParameters.ReportId = (uint)iReportId;
			empty = $"SELECT sReportName, iModule, ISNULL(iSourceType,12)[iSourceType], iReportType FROM cCore_Reports{suffix} WHERE iReportId = {iReportId}";
			if (iReportId < RDCommon.RD_STARTID)
			{
				empty += $" AND iReportType = {(int)oType}";
			}
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				reportInputParameters.ReportName = rDDefault.GetReportName((uint)iReportId);
				if (iReportId == 656)
				{
					string text = _focus.company(iCompId).invTagName;
					if (!string.IsNullOrEmpty(text))
					{
						num = text.IndexOf('_');
						text = text.Remove(0, num + 1);
					}
					reportInputParameters.ReportName = reportInputParameters.ReportName.Replace("Tag", text);
				}
				if (string.IsNullOrEmpty(reportInputParameters.ReportName))
				{
					reportInputParameters.ReportName = Convert.ToString(dataReader["sReportName"]);
				}
				reportInputParameters.ReportModule = (Module)Convert.ToInt32(dataReader["iModule"]);
				reportInputParameters.SourceType = (DataSourceType)Convert.ToInt32(dataReader["iSourceType"]);
				reportInputParameters.ReportType = (ReportType)Convert.ToInt32(dataReader["iReportType"]);
			}
			else
			{
				reportInputParameters.SourceType = rDDefault.GetReportSourceType((uint)iReportId);
				reportInputParameters.ReportName = rDDefault.GetReportName((uint)iReportId);
				reportInputParameters.ReportModule = rDDefault.GetReportModule((uint)iReportId);
				reportInputParameters.Parameters = rDDefault.GetReportInputs((uint)iReportId);
			}
			dataReader.Close();
			switch ((FocusReport)iReportId)
			{
			case FocusReport.ProfitAndLoss:
				if (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.Misc, 8) == 1)
				{
					reportInputParameters.ReportName = "Income and Expeneses statement";
				}
				break;
			case FocusReport.StockValuationByFATag:
				reportInputParameters.ReportName = reportInputParameters.ReportName.Replace("FA Tag", rDDefault.FaTagName);
				break;
			case FocusReport.StockBalanceByWarehouse:
				reportInputParameters.ReportName = reportInputParameters.ReportName.Replace("warehouse", rDDefault.InvTagName);
				break;
			case FocusReport.SalesGroupedByDepartment:
			case FocusReport.PurchasesGroupedByDepartment:
				reportInputParameters.ReportName = reportInputParameters.ReportName.Replace("department", rDDefault.FaTagName);
				break;
			}
			if (reportInputParameters.ReportType == ReportType.Detail || reportInputParameters.ReportType == ReportType.Cubes)
			{
				empty = $"select iTranSetId,iVoucherType FROM cCore_ReportTransactionSet{suffix} WHERE iReportId = {iReportId}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				if (dataReader.Read())
				{
					reportInputParameters.TransactionSet = (TransactionSetType)Convert.ToInt32(dataReader["iTranSetId"]);
					reportInputParameters.LinkId = Convert.ToInt32(dataReader["iVoucherType"]);
				}
				dataReader.Close();
				list.AddRange(GetParamOfTransactionSets(reportInputParameters.TransactionSet, reportInputParameters.LinkId, iCompId, database));
				empty = $"SELECT iParameterId, sFieldName, iControlType, iFieldType, sValue, iSelectionMode, \r\n                        ISNULL(iFieldId,0)[iFieldId], ISNULL(sDefault,'')[sDefaultValue], bGroup\r\n                    FROM cCore_ReportParameter{suffix} WHERE iReportId = {iReportId}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					parameter = new _Parameter();
					parameter.ParameterId = Convert.ToInt32(dataReader["iParameterId"]);
					parameter.FieldName = Convert.ToString(dataReader["sFieldName"]);
					parameter.ControlId = Convert.ToInt32(dataReader["iControlType"]);
					parameter.FieldId = Convert.ToInt32(dataReader["iFieldId"]);
					parameter.FieldType = (_FieldType)Convert.ToInt16(dataReader["iFieldType"]);
					parameter.SelectionValue = Convert.ToString(dataReader["sValue"]);
					parameter.SelectionMode = (_SelectionMode)Convert.ToInt32(dataReader["iSelectionMode"]);
					parameter.DefaultValue = Convert.ToString(dataReader["sDefaultValue"]);
					parameter.IsGroupMaster = Convert.ToBoolean(dataReader["bGroup"]);
					if (parameter.ControlId == 10)
					{
						array = parameter.SelectionValue.Split(',');
						if (array.Length > 1)
						{
							for (num = 0; num < array.Length; num++)
							{
								if (!string.IsNullOrEmpty(array[num]))
								{
									switch (num)
									{
									case 0:
										parameter.FieldVariable = array[num];
										break;
									case 1:
										parameter.SelectionValue = array[num];
										break;
									}
								}
							}
						}
					}
					else if (parameter.ControlId == 6 && parameter.FieldId == 0)
					{
						parameter.FieldId = parameter.ParameterId;
					}
					list.Add(parameter);
				}
				dataReader.Close();
				reportInputParameters.Parameters = list.ToArray();
			}
			else
			{
				IdNamePair reportSpecificInput = getReportSpecificInput(iReportId, iCompId);
				reportInputParameters.Parameters = rDDefault.GetReportInputs((uint)iReportId, reportSpecificInput, iCompId);
			}
		}
		catch (Exception ex)
		{
			reportInputParameters.ReportId = 0u;
			string sError = (reportInputParameters.ReportName = ex.Message);
			m_sError = sError;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return reportInputParameters;
	}

	private void ReportSepecificParam(int iReport, ref bool bInvSlabs, ref bool bFaSlabs, ref bool bVat, ref bool bPdc, ref bool bBudget, ref bool bFixAsset, ref bool bWms, ref bool bSubledger)
	{
		switch ((FocusReport)iReport)
		{
		case FocusReport.StockAgeingAnalysis:
		case FocusReport.AgeingOfPendingDocument:
		case FocusReport.AgeingOfPendingDocumentByDueDate:
		case FocusReport.AgeingAnalysisByBatch:
		case FocusReport.AgeingAnalysisByRMA:
		case FocusReport.AgeingStockByProductByBins:
			bInvSlabs = true;
			break;
		case FocusReport.VendorAgeingSummaryBillwise:
		case FocusReport.VendorAgeingDetailsBillwise:
		case FocusReport.VendorDetailAgeingByDueDate:
		case FocusReport.VendorSummaryAgeingByDueDate:
		case FocusReport.VendorOverdueAnalysis:
		case FocusReport.VendorOverdueSummary:
		case FocusReport.CustomerAgeingSummaryBillwise:
		case FocusReport.CustomerAgeingDetailsBillwise:
		case FocusReport.CustomerDetailAgeingByDueDate:
		case FocusReport.CustomerSummaryAgeingByDueDate:
		case FocusReport.CustomerOverdueAnalysis:
		case FocusReport.CustomerOverdueSummary:
			bSubledger = (bFaSlabs = true);
			break;
		case FocusReport.VATPurchaseAccount:
		case FocusReport.VATSalesAccount:
		case FocusReport.VATSalesByCustomer:
		case FocusReport.VATMonthlyReport:
			bVat = true;
			break;
		case FocusReport.Ledger:
			bPdc = true;
			break;
		case FocusReport.SubLedger:
		case FocusReport.CustomerBillwiseSummary:
		case FocusReport.VendorListingofOutstandingBills:
		case FocusReport.VendorStatements:
		case FocusReport.VendorDueDateAnalysis:
		case FocusReport.CustomerListingofOutstandingBills:
		case FocusReport.CustomerStatements:
		case FocusReport.CustomerDueDateAnalysis:
		case FocusReport.VendorBillwiseSummary:
			bPdc = (bSubledger = true);
			break;
		case FocusReport.AdvancedBudgetReport:
		case FocusReport.RevisedBudgetReport:
		case FocusReport.BudgetAuthorizationReport:
		case FocusReport.PreCommittedBudgetReport:
			bBudget = true;
			break;
		case FocusReport.FADepreciationSchedule:
		case FocusReport.FATransferOfAssets:
		case FocusReport.FAAddAssetsValue:
		case FocusReport.FAReduceAssetValue:
		case FocusReport.FAComponentAdded:
		case FocusReport.FAComponentReduced:
		case FocusReport.FADisposalOfAssets:
			bFixAsset = true;
			break;
		case FocusReport.WMSCurrentStockReport:
		case FocusReport.WMSSKUwiseInvTransReport:
		case FocusReport.WMSExpiryDateReport:
		case FocusReport.WMSInventoryBalReport:
		case FocusReport.WMSPalletInPalletOutRpt:
		case FocusReport.WMSEstimatedBillingReport:
			bWms = true;
			break;
		}
	}

	private IdNamePair getReportSpecificInput(int iReportId, int iCompanyId)
	{
		int num = 0;
		IdNamePair idNamePair = null;
		switch ((FocusReport)iReportId)
		{
		case FocusReport.TrialBalance:
		case FocusReport.ProfitAndLoss:
		case FocusReport.TradingAccount:
		case FocusReport.TradingAndProfitAndLoss:
		case FocusReport.BalanceSheet:
		case FocusReport.FinalAccountSchedules:
		case FocusReport.FundsFlow:
		case FocusReport.CashFlow:
		case FocusReport.CashFlowAnalysis:
		case FocusReport.AdvanceCashFlow:
		case FocusReport.ReceivableAndPaybleBalance:
		{
			num = _focus.company(iCompanyId).maxLevelAccount;
			idNamePair = new IdNamePair();
			idNamePair.ID = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.PDC, 2);
			idNamePair.Name = "Linear";
			idNamePair.Tag = num + 1;
			for (int m = 0; m <= num; m++)
			{
				idNamePair.Name += $",{m + 1}";
			}
			idNamePair.ExtraInfo = _focus.company(iCompanyId).currency.Code;
			break;
		}
		case FocusReport.StockValuation:
		{
			num = _focus.company(iCompanyId).maxLevelItem;
			idNamePair = new IdNamePair();
			idNamePair.ID = 0;
			idNamePair.Name = "None";
			idNamePair.Tag = num;
			for (int i = 0; i <= num; i++)
			{
				idNamePair.Name += $",{i + 1}";
			}
			break;
		}
		case FocusReport.TagwiseStockReport:
		{
			num = _focus.company(iCompanyId).maxLevelItem;
			idNamePair = new IdNamePair();
			idNamePair.ID = 1;
			idNamePair.Name = "1";
			idNamePair.Tag = num;
			for (int j = 1; j <= num; j++)
			{
				idNamePair.Name += $",{j + 1}";
			}
			break;
		}
		case FocusReport.FADepreciationSchedule:
		{
			num = Convert.ToByte(GetMaxLevelOfMaster(601, 0, iCompanyId));
			idNamePair = new IdNamePair();
			idNamePair.ID = 0;
			idNamePair.Name = "None";
			idNamePair.Tag = num;
			for (int k = 0; k <= num; k++)
			{
				idNamePair.Name += $",{k + 1}";
			}
			break;
		}
		case FocusReport.MultiLevelStockMovement:
		{
			num = _focus.company(iCompanyId).maxLevelItem;
			idNamePair = new IdNamePair();
			idNamePair.ID = 0;
			idNamePair.Name = "1";
			idNamePair.Tag = num;
			for (int l = 1; l <= num; l++)
			{
				idNamePair.Name += $",{l + 1}";
			}
			break;
		}
		case FocusReport.VATDetailedReport:
			idNamePair = new IdNamePair();
			idNamePair.ID = _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.Misc, 10);
			switch (idNamePair.ID)
			{
			case 7:
				idNamePair.ID = 1;
				idNamePair.Name = "UAE";
				break;
			case 74:
				idNamePair.ID = 3;
				idNamePair.Name = "OMAN";
				break;
			case 69:
				idNamePair.ID = 4;
				idNamePair.Name = "NEPAL";
				break;
			case 85:
				idNamePair.ID = 0;
				idNamePair.Name = "KSA";
				break;
			case 111:
				idNamePair.ID = 2;
				idNamePair.Name = "BAHRAIN";
				break;
			}
			break;
		case FocusReport.Ledger:
		case FocusReport.SubLedger:
		case FocusReport.LedgerDetail:
			idNamePair = new IdNamePair();
			idNamePair.ID = ((_focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.PDC, 2) > 0) ? 2 : 0);
			break;
		case FocusReport.WMSDateWiseShipInShipOutQuantity:
			idNamePair = new IdNamePair();
			idNamePair.Tag = _focus.company(iCompanyId).isModuleImplemented(ModulesImplemented.WMS3PL);
			break;
		}
		return idNamePair;
	}

	private byte GetMaxLevelOfMaster(int iMasterTypeId, int iTreeId, int iComanyId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iComanyId);
		DbCommand dbCommand = null;
		int num = 0;
		if (num <= 0)
		{
			string text = null;
			string text2 = null;
			switch (iMasterTypeId)
			{
			case 1:
				text = "mCore_AccountTreeDetails";
				break;
			case 2:
				text = "mCore_ProductTreeDetails";
				break;
			default:
				text2 = $"select 'm'+sModule+'_'+sMasterName+'TreeDetails' from cCore_MasterDef where iMasterTypeId={iMasterTypeId}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
				text = Convert.ToString(database.ExecuteScalar(dbCommand));
				break;
			}
			text2 = $"SELECT MAX(ISNULL(iLevel,0)) FROM {text} WHERE iTreeId = {iTreeId}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
			num = Convert.ToByte(database.ExecuteScalar(dbCommand));
		}
		return (byte)num;
	}

	private _Parameter[] GetParamOfTransactionSets(TransactionSetType objTransactionSet, int iTagId, int iCompId, Database objDb)
	{
		string text = null;
		IdNamePair idNamePair = null;
		RDDefault rDDefault = null;
		List<_Parameter> list = new List<_Parameter>();
		switch (objTransactionSet)
		{
		case TransactionSetType.AccountingTransactionsOfAccountingTag:
		case TransactionSetType.InventoryTransactionsOfAccountingTag:
			iTagId = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 0);
			break;
		case TransactionSetType.AccountingTransactionsOfInventoryTag:
		case TransactionSetType.InventoryTransactionsOfInventoryTag:
			iTagId = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 1);
			break;
		case TransactionSetType.AccountingTransactionsOfanAccount:
		case TransactionSetType.AccountingTransactionsOfSelectedAccounts:
		case TransactionSetType.InventoryTransactionsOfaProduct:
		case TransactionSetType.InventoryTransactionsOfSelectedProducts:
		case TransactionSetType.AllAccountsWithOpeningBalance:
		case TransactionSetType.AllProductsWithOpeningStock:
			idNamePair = new IdNamePair();
			break;
		}
		if (iTagId > 0 && objTransactionSet != TransactionSetType.None)
		{
			idNamePair = new IdNamePair();
			idNamePair.ID = iTagId;
			text = $"SELECT sCaption[sMasterName] FROM cCore_MasterLanguage WHERE iLinkId = {iTagId} AND iLinkTypeId = 0 AND iLanguageId = 0";
			idNamePair.Name = Convert.ToString(objDb.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
		}
		if (idNamePair != null || objTransactionSet == TransactionSetType.None)
		{
			rDDefault = new RDDefault();
			rDDefault.IsSubLedgerEnabled = _focus.company(iCompId).isModuleImplemented(ModulesImplemented.SubLedger);
			rDDefault.InventorySlabs = _focus.company(iCompId).getPreferenceTexts(PreferenceCategories.Print, 150, 50);
			rDDefault.FinanceSlabs = _focus.company(iCompId).getPreferenceTexts(PreferenceCategories.Print, 100, 50);
			rDDefault.VATTagId = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 14);
			rDDefault.VATTagName = _focus.company(iCompId).master(rDDefault.VATTagId).name;
			rDDefault.Masters = _focus.company(iCompId).getAllMasters();
			rDDefault.BudgetTag1Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 4);
			rDDefault.BudgetTag2Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 5);
			rDDefault.BudgetTag3Id = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Budgets, 6);
			if (objTransactionSet == TransactionSetType.None)
			{
				list.AddRange(rDDefault.GetReportInputs((uint)iTagId));
			}
			else
			{
				list.AddRange(rDDefault.GetReportInputs(objTransactionSet, idNamePair));
			}
		}
		return list.ToArray();
	}

	public LayoutInformation Load(int iLayoutId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		LayoutInformation layoutInformation = new LayoutInformation();
		layoutInformation.Layout = new ComboData();
		layoutInformation.Layout.ID = iLayoutId;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		IDataReader dataReader2 = null;
		string empty = string.Empty;
		string text = string.Empty;
		string suffix = FConvert.GetSuffix(iCompId);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		try
		{
			empty = string.Format("SELECT iReportId,sLayout,fPageWidth,fPageHeight,fLeftMargin,fTopMargin,\r\n                fRightMargin,fBottomMargin,iPageUnit,iPageView, iModule, iReportType FROM cCore_InvoiceLayout{1} WHERE iLayoutId={0};", iLayoutId, suffix);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				layoutInformation.ReportId = Convert.ToInt32(dataReader["iReportId"]);
				layoutInformation.Layout.Name = Convert.ToString(dataReader["sLayout"]);
				layoutInformation.PrintInfo = new PrinterInfo();
				layoutInformation.PrintInfo.PageWidth = Convert.ToDouble(dataReader["fPageWidth"]);
				layoutInformation.PrintInfo.PageHeight = Convert.ToDouble(dataReader["fPageHeight"]);
				layoutInformation.PrintInfo.Margin = new Margin();
				layoutInformation.PrintInfo.Margin.Left = Convert.ToDouble(dataReader["fLeftMargin"]);
				layoutInformation.PrintInfo.Margin.Top = Convert.ToDouble(dataReader["fTopMargin"]);
				layoutInformation.PrintInfo.Margin.Right = Convert.ToDouble(dataReader["fRightMargin"]);
				layoutInformation.PrintInfo.Margin.Bottom = Convert.ToDouble(dataReader["fBottomMargin"]);
				layoutInformation.PrintInfo.Unit = (Unit)Convert.ToByte(dataReader["iPageUnit"]);
				layoutInformation.PrintInfo.View = (View)Convert.ToByte(dataReader["iPageView"]);
				layoutInformation.Module = (Module)Convert.ToByte(dataReader["iModule"]);
				layoutInformation.ReportType = (ReportList)Convert.ToByte(dataReader["iReportType"]);
			}
			dataReader.Close();
			empty = string.Format("SELECT iPageId FROM cCore_InvoicePage{1} WHERE iLayoutId = {0}", iLayoutId, suffix);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				text += ((text.Length == 0) ? string.Empty : ",");
				text += dataReader[0].ToString();
			}
			dataReader.Close();
			text = ((text.Length == 0) ? "0" : text);
			string[] array = text.Split(',');
			layoutInformation.Pages = new Page[array.Length];
			for (num = 0; num < array.Length; num++)
			{
				layoutInformation.Pages[num] = new Page();
				empty = string.Format("SELECT COUNT(1) FROM cCore_InvoiceHeader{1} WHERE iPageId = {0}", array[num], suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				num3 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				if (num3 <= 0)
				{
					continue;
				}
				empty = string.Format("SELECT cCore_InvoiceHeader{1}.iHeaderId,iControlType,sText,iCategoryId,iFieldId,fLeft,fTop,fWidth,fHeight,\r\n                        fBorderThickness,iBorderColor,iBackColor,iTextColor,sTextFont,fFontSize,iFontWeights,iFontStyle,imgControl,iVariable, iShowHeader, bArabicDigit                      \r\n                        FROM cCore_InvoiceHeader{1} \r\n                        WHERE cCore_InvoiceHeader{1}.iPageId={0} ORDER BY fTop,fLeft;", array[num], suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				layoutInformation.Pages[num].PageHeader = new PageHeader[num3];
				num2 = 0;
				while (dataReader.Read())
				{
					layoutInformation.Pages[num].PageHeader[num2] = new PageHeader();
					layoutInformation.Pages[num].PageHeader[num2].UID = Convert.ToInt32(dataReader["iHeaderId"]);
					layoutInformation.Pages[num].PageHeader[num2].Type = (ControlType)Convert.ToInt32(dataReader["iControlType"]);
					layoutInformation.Pages[num].PageHeader[num2].Text = Convert.ToString(dataReader["sText"]);
					layoutInformation.Pages[num].PageHeader[num2].MasterId = Convert.ToInt32(dataReader["iCategoryId"]);
					layoutInformation.Pages[num].PageHeader[num2].FieldId = Convert.ToInt32(dataReader["iFieldId"]);
					layoutInformation.Pages[num].PageHeader[num2].Left = Convert.ToDouble(dataReader["fLeft"]);
					layoutInformation.Pages[num].PageHeader[num2].Top = Convert.ToDouble(dataReader["fTop"]);
					layoutInformation.Pages[num].PageHeader[num2].Width = Convert.ToDouble(dataReader["fWidth"]);
					layoutInformation.Pages[num].PageHeader[num2].Height = Convert.ToDouble(dataReader["fHeight"]);
					layoutInformation.Pages[num].PageHeader[num2].BorderThickness = Convert.ToDouble(dataReader["fBorderThickness"]);
					layoutInformation.Pages[num].PageHeader[num2].BorderColor = Convert.ToInt32(dataReader["iBorderColor"]);
					layoutInformation.Pages[num].PageHeader[num2].BackColor = Convert.ToInt32(dataReader["iBackColor"]);
					layoutInformation.Pages[num].PageHeader[num2].TextColor = Convert.ToInt32(dataReader["iTextColor"]);
					layoutInformation.Pages[num].PageHeader[num2].TextFont = Convert.ToString(dataReader["sTextFont"]);
					layoutInformation.Pages[num].PageHeader[num2].FontSize = Convert.ToDouble(dataReader["fFontSize"]);
					layoutInformation.Pages[num].PageHeader[num2].FontWeight = Convert.ToInt16(dataReader["iFontWeights"]);
					layoutInformation.Pages[num].PageHeader[num2].FontStyle = Convert.ToInt16(dataReader["iFontStyle"]);
					layoutInformation.Pages[num].PageHeader[num2].ImageSource = ((dataReader["imgControl"] == DBNull.Value) ? null : ((byte[])dataReader["imgControl"]));
					layoutInformation.Pages[num].PageHeader[num2].VariableType = (VariableType)Convert.ToInt16(dataReader["iVariable"]);
					layoutInformation.Pages[num].PageHeader[num2].ShowOnPage = (ShowOnPage)Convert.ToInt16(dataReader["iShowHeader"]);
					layoutInformation.Pages[num].PageHeader[num2].IsArabicDigit = dataReader["bArabicDigit"] != DBNull.Value && (bool)dataReader["bArabicDigit"];
					empty = string.Format("SELECT ISNULL(iAlignment,0)[iAlignment],ISNULL(iDecimalInColumn,0)[iDecimalInColumn],ISNULL(iRoundOffType,0)[iRoundOffType],ISNULL(fRoundOffValue, 0)[fRoundOffValue]\r\n                            FROM cCore_InvoiceNumericProperty{1} where iPageId = {0} AND iHeaderId > 2000 AND iHeaderId={2}", array[num], suffix, layoutInformation.Pages[num].PageHeader[num2].UID);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					dataReader2 = database.ExecuteReader(dbCommand);
					if (dataReader2.Read())
					{
						layoutInformation.Pages[num].PageHeader[num2].Alignment = Convert.ToInt32(dataReader2["iAlignment"]);
						layoutInformation.Pages[num].PageHeader[num2].DecimalInColumn = Convert.ToByte(dataReader2["iDecimalInColumn"]);
						layoutInformation.Pages[num].PageHeader[num2].RoundOffType = (RoundingType)Convert.ToByte(dataReader2["iRoundOffType"]);
						layoutInformation.Pages[num].PageHeader[num2].RoundUptoValue = Convert.ToDouble(dataReader2["fRoundOffValue"]);
					}
					dataReader2.Close();
					if (layoutInformation.Pages[num].PageHeader[num2].UID <= 2000)
					{
						empty = string.Format("SELECT COUNT(1) FROM cCore_InvoiceBody{2} WHERE iPageId={0} and iBodyId={1}", array[num], layoutInformation.Pages[num].PageHeader[num2].UID, suffix);
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
						num3 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
						if (num3 > 0)
						{
							empty = string.Format("SELECT iBodyId,iCategoryId,iFieldId,sColumn,sAlias,iDataType,fColumnWidth,iDisplayColumnIndex,\r\n                                    iSign,bWordWrap,bPrintUnderPrevColumn,ISNULL(iFunction,0)[iFunction],ISNULL(bArabicDigit,0)[bArabicDigit]\r\n                                    FROM cCore_InvoiceBody{2} \r\n                                    WHERE cCore_InvoiceBody{2}.iPageId={0} and iBodyId={1}", array[num], layoutInformation.Pages[num].PageHeader[num2].UID, suffix);
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
							dataReader2 = database.ExecuteReader(dbCommand);
							layoutInformation.Pages[num].PageHeader[num2].PageBody = new PageBody[num3];
							int num4 = 0;
							while (dataReader2.Read())
							{
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4] = new PageBody();
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].BodyId = Convert.ToInt32(dataReader2["iBodyId"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].MasterId = Convert.ToInt32(dataReader2["iCategoryId"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].FieldId = Convert.ToInt32(dataReader2["iFieldId"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].Column = Convert.ToString(dataReader2["sColumn"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].Alias = Convert.ToString(dataReader2["sAlias"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].DataType = Convert.ToInt16(dataReader2["iDataType"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].ColumnWidth = Convert.ToDouble(dataReader2["fColumnWidth"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].ColumnIndex = Convert.ToInt32(dataReader2["iDisplayColumnIndex"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].Sign = (Sign)Convert.ToInt32(dataReader2["iSign"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].WordWrap = Convert.ToBoolean(dataReader2["bWordWrap"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].PrintUnderPreviousColumn = Convert.ToBoolean(dataReader2["bPrintUnderPrevColumn"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].FunctionType = (InvoiceFunction)Convert.ToInt32(dataReader2["iFunction"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].IsArabicDigit = Convert.ToBoolean(dataReader2["bArabicDigit"]);
								num4++;
							}
							dataReader2.Close();
							empty = $"SELECT ISNULL(iAlignment,0)[iAlignment],ISNULL(iDecimalInColumn,0)[iDecimalInColumn],ISNULL(iRoundOffType,0)[iRoundOffType],ISNULL(fRoundOffValue, 0)[fRoundOffValue]\r\n                                    FROM cCore_InvoiceNumericProperty{suffix} WHERE iPageId = {array[num]} AND iHeaderId = {layoutInformation.Pages[num].PageHeader[num2].UID}";
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
							dataReader2 = database.ExecuteReader(dbCommand);
							num4 = 0;
							while (dataReader2.Read())
							{
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].Alignment = (TextAlignment)Convert.ToInt32(dataReader2["iAlignment"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].DecimalInColumn = Convert.ToInt32(dataReader2["iDecimalInColumn"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].RoundOffType = (RoundingType)Convert.ToByte(dataReader2["iRoundOffType"]);
								layoutInformation.Pages[num].PageHeader[num2].PageBody[num4].RoundUptoValue = Convert.ToDouble(dataReader2["fRoundOffValue"]);
								num4++;
							}
							dataReader2.Close();
						}
					}
					num2++;
				}
				dataReader.Close();
			}
		}
		catch (Exception ex)
		{
			layoutInformation.Layout.ID = ERRORNO;
			string sError = (layoutInformation.Layout.Name = ex.Message);
			m_sError = sError;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return layoutInformation;
	}

	public int GetReportInvoiceLayoutId(int iReportId, int iLayoutId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		string text = null;
		int num = 0;
		int num2 = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		if (iReportId < RDCommon.RD_STARTID)
		{
			text = $"SELECT iLayoutId FROM cCore_InvoiceLayout{suffix} WHERE iModule = 0 AND iReportId = {iLayoutId}";
		}
		else
		{
			bool flag = false;
			text = $"SELECT iFieldId, sValue FROM cCore_ReportExtraValues{suffix} WHERE iLayoutId = {iLayoutId}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			IDataReader dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				if (3 == Convert.ToInt32(dataReader["iFieldId"]))
				{
					iReportId = Convert.ToInt32(dataReader["sValue"]);
					flag = true;
					break;
				}
			}
			dataReader.Close();
			if (!flag)
			{
				text = string.Format("SELECT ISNULL(iVoucherType,0)[LinkId] FROM cCore_ReportTransactionSet{0} WITH (READUNCOMMITTED)\r\n                    JOIN cCore_Reports{0} WITH (READUNCOMMITTED) ON cCore_ReportTransactionSet{0}.iVoucherType=cCore_Reports{0}.iReportId\r\n                    WHERE cCore_ReportTransactionSet{0}.iReportId={1} AND iTranSetId NOT IN ({2},{3})", suffix, iReportId, 13, 14);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				num2 = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				if (num2 > 0)
				{
					long num3 = 1L;
					text = string.Format("SELECT iLayoutId FROM cCore_ReportLayouts{0} WITH (READUNCOMMITTED)\r\n                    WHERE iReportId = {1} ORDER BY iFlag DESC", suffix, num2, num3);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					num = Convert.ToInt32(database.ExecuteScalar(dbCommand));
					if (num > 0)
					{
						iReportId = num;
					}
					num = 0;
				}
			}
			text = $"SELECT iLayoutId FROM cCore_InvoiceLayout{suffix} WHERE iReportId = {iReportId}";
		}
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		return Convert.ToInt32(database.ExecuteScalar(dbCommand));
	}

	public LayoutInformation LoadReportInvoiceLayout(int iReportId, int iLayoutId, int iCompId)
	{
		int num = GetReportInvoiceLayoutId(iReportId, iLayoutId, iCompId);
		if (num == 0)
		{
			num = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Print, 16);
			if (num > 0)
			{
				num = GetReportInvoiceLayoutId(0, 0, iCompId);
			}
		}
		if (num > 0)
		{
			return LoadPrintingInvoice(num, iCompId);
		}
		return null;
	}

	public int getCurrencySeparator(int iCurrencyId, int iCompanyId)
	{
		string text = null;
		int result = 0;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = $"SELECT ISNULL(iNumericSeperator,0)[iNumericSeperator] FROM mCore_Currency WHERE iCurrencyId = {iCurrencyId}";
			result = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return result;
	}

	public string DeleteLayout(int iLayoutId, Module objModule, int iCompId)
	{
		Database database = null;
		DbConnection dbConnection = null;
		DbTransaction dbTransaction = null;
		DbCommand dbCommand = null;
		string text = null;
		object obj = null;
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompId);
			dbConnection = database.CreateConnection();
			dbConnection.Open();
			dbTransaction = dbConnection.BeginTransaction();
			text = DeleteRD(iLayoutId, MasterType.ReportCustomization, database, iCompId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			database.ExecuteNonQuery(dbCommand, dbTransaction);
			text = string.Format("DELETE FROM cCore_ReportLayouts{1} WHERE iLayoutId = {0}", iLayoutId, FConvert.GetSuffix(iCompId));
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			database.ExecuteNonQuery(dbCommand, dbTransaction);
			text = "SELECT MAX(ISNULL(iLayoutId,0)) FROM cCore_ReportLayouts" + FConvert.GetSuffix(iCompId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			obj = database.ExecuteScalar(dbCommand, dbTransaction);
			if (obj != null && obj != DBNull.Value)
			{
				text = string.Format("dbcc CHECKIDENT('cCore_ReportLayouts{1}', RESEED,  {0})", Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction)), FConvert.GetSuffix(iCompId));
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				database.ExecuteNonQuery(dbCommand, dbTransaction);
			}
			text = "SELECT MAX(ISNULL(iColumnId,0)) FROM cCore_ReportColumns" + FConvert.GetSuffix(iCompId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			obj = database.ExecuteScalar(dbCommand, dbTransaction);
			if (obj != null && obj != DBNull.Value)
			{
				text = string.Format("dbcc CHECKIDENT('cCore_ReportColumns{1}', RESEED,  {0})", Convert.ToInt32(database.ExecuteScalar(dbCommand, dbTransaction)), FConvert.GetSuffix(iCompId));
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				database.ExecuteNonQuery(dbCommand, dbTransaction);
			}
			Delete(iLayoutId, MasterType.InvoiceDesigner, iCompId, dbConnection, dbTransaction, (int)objModule);
			dbTransaction.Commit();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			dbTransaction.Rollback();
			return ex.Message;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
			if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
			{
				dbConnection.Close();
			}
		}
		return string.Empty;
	}

	public ComboData[] LoadLayouts(Module ModuleType, int iReportId, LayoutType type, int iCompanyId)
	{
		return type switch
		{
			LayoutType.InvoiceDesigner => Load(string.Format("SELECT iLayoutId, sLayout FROM cCore_InvoiceLayout{2} \r\n                        WHERE iModule={0} AND iReportId={1}", (int)ModuleType, iReportId, FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId), 
			LayoutType.InvoiceBarcode => Load(string.Format("SELECT iLayoutId, sLayout FROM cCore_InvoiceLayout{2} \r\n                        WHERE iModule={0} AND iReportId={1} AND iSubReportId = 1", (int)ModuleType, iReportId, FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId), 
			LayoutType.ReportCustomization => Load(string.Format("SELECT iLayoutId, sLayoutName FROM cCore_ReportLayouts{3} \r\n                        JOIN cCore_Reports{3} ON cCore_ReportLayouts{3}.iReportId = cCore_Reports{3}.iReportId \r\n                        WHERE iModule={0} AND cCore_Reports{3}.iReportId={1}", (int)ModuleType, iReportId, 0, FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId), 
			LayoutType.LoadVouchers => Load($"SELECT iVoucherType, sName FROM cCore_Vouchers{FConvert.GetSuffix(iCompanyId)} WHERE iVoucherType & 0XFF00 = {iReportId}", 0, iCompanyId), 
			LayoutType.LoadFAVouchers => Load($"SELECT iVoucherType, sName FROM cCore_Vouchers{FConvert.GetSuffix(iCompanyId)} WHERE bUpdateFA=1", 0, iCompanyId), 
			LayoutType.LoadInventoryVouchers => Load($"SELECT iVoucherType, sName FROM cCore_Vouchers{FConvert.GetSuffix(iCompanyId)} WHERE bUpdateInv=1", 0, iCompanyId), 
			LayoutType.PosBillPrintFormat => Load($"SELECT iPrintFormatId,sTemplate FROM cPos_BillPrintFormat", 0, iCompanyId), 
			LayoutType.StartOfWeek => Load($"SELECT iWeekStartFrom,sCalName FROM mCal_Calendar WHERE iCalId  = {iReportId}", (int)type, iCompanyId), 
			LayoutType.BankCashType => Load($"SELECT 0[iMasterId],'CASH'[sName] FROM mPos_BankCardType Union SELECT iMasterId,sName FROM mPos_BankCardType Where iMasterId > 0", 0, iCompanyId), 
			LayoutType.CodeBookNo => Load(string.Format("select distinct iBookNo,sName\r\n                    from(select iBookNo,sName\r\n                        from \r\n                        tCore_Data{0} \r\n                        join mCore_Account on mCore_Account.iMasterId = tCore_Data{0}.iBookNo\r\n                        where iBodyId = {1} OR iHeaderId = {1}\r\n                        union all\r\n                        select iCode, sName\r\n                        from \r\n                        tCore_Data{0} \r\n                        join mCore_Account on mCore_Account.iMasterId = tCore_Data{0}.iCode\r\n                        where iBodyId = {1} OR iHeaderId = {1})temp", FConvert.GetSuffix(iCompanyId), iReportId), 0, iCompanyId), 
			LayoutType.VoucherFields => Load(string.Format("SELECT cCore_VoucherFields{0}.iUniqueId, cCore_VoucherFields{0}.sFieldName \r\n                        from cCore_VoucherFields{0} \r\n                        JOIN cCore_Fields ON cCore_VoucherFields{0}.iUniqueId = cCore_Fields.iFieldId\r\n                        WHERE iVoucherType={1} and iDataTypeId = {2}", FConvert.GetSuffix(iCompanyId), iReportId, 6), 0, iCompanyId), 
			LayoutType.WorkFlowCount => Load($"SELECT Count(iLinkPathId)[Length],''[Blank] FROM vmCore_Links{FConvert.GetSuffix(iCompanyId)} WHERE iWorkFlowId = {iReportId}", 0, iCompanyId), 
			LayoutType.LinkWorkFlow => Load($"SELECT iLinkPathId,LinkName FROM vmCore_Links{FConvert.GetSuffix(iCompanyId)}", 0, iCompanyId), 
			LayoutType.WorkFlowPath => GetWorkflowPath(iReportId, iCompanyId), 
			LayoutType.CashFlowTemplate => Load(string.Format("SELECT iTemplateId,sTemplateName FROM cCore_CashFlowTemplate{0} ORDER BY iTemplateId DESC", FConvert.GetSuffix(iCompanyId), iReportId), 0, iCompanyId), 
			LayoutType.QuotationAnalysis => Load(string.Format("SELECT iLayoutId,sLayoutName FROM cCore_QuotationLayouts{0} ORDER BY iLayoutId DESC", FConvert.GetSuffix(iCompanyId), iReportId), 0, iCompanyId), 
			LayoutType.ProfileRights => Load(string.Format("SELECT iProfileId, sProfileName FROM mSec_ProfileHeader ORDER BY iProfileId DESC", FConvert.GetSuffix(iCompanyId), iReportId), 0, iCompanyId), 
			_ => null, 
		};
	}

	public IdNamePair[] LoadCrossReferenceData(Module oModuleType, uint iReportId, int iId, BackTrackType oBackTrackType, int iCompanyId)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = null;
		Database database = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = null;
		try
		{
			list = new List<IdNamePair>();
			database = DatabaseWrapper.GetDatabase2(iCompanyId);
			if (oBackTrackType == BackTrackType.None)
			{
				oBackTrackType = new RDDefault().GetBackTrackType(iReportId, oModuleType);
			}
			switch (oBackTrackType)
			{
			case BackTrackType.Transaction:
			case BackTrackType.TransactionHeaderId:
				text = string.Format("SELECT iCode, iBookNo, ISNULL(iProduct, 0)[iProduct] \r\n                    FROM tCore_Data{0}\r\n                    LEFT JOIN tCore_Indta{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId\r\n                    WHERE tCore_Data{0}.{2} = {1}", FConvert.GetSuffix(iCompanyId), iId, (oBackTrackType == BackTrackType.Transaction) ? "iBodyId" : "iHeaderId");
				dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				if (dataReader.Read())
				{
					num = Convert.ToInt32(dataReader["iCode"]);
					num2 = Convert.ToInt32(dataReader["iBookNo"]);
					num3 = Convert.ToInt32(dataReader["iProduct"]);
				}
				dataReader.Close();
				break;
			case BackTrackType.AccountDetail:
				num2 = iId;
				break;
			case BackTrackType.ProductDetail:
				num3 = iId;
				break;
			default:
				if (oModuleType == Module.Inventory)
				{
					num3 = iId;
				}
				else
				{
					num2 = iId;
				}
				break;
			}
			text = $"SELECT iMasterId, case when iMasterId > 0 THEN sName + ' ['+sCode + ']' ELSE sName END [sName]FROM mCore_Account WHERE iMasterId IN (0, {num}, {num2}) ";
			if (oModuleType == Module.CoreTransactions)
			{
				text += " ORDER BY iMasterId DESC";
			}
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"]), 1));
			}
			dataReader.Close();
			text = $"SELECT iMasterId, case when iMasterId > 0 THEN sName + ' ['+sCode + ']' ELSE sName END [sName]FROM mCore_Product WHERE iMasterId IN (0, {num3}) ";
			if (oModuleType == Module.Inventory)
			{
				text += " ORDER BY iMasterId DESC";
			}
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"]), 2));
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			list.Add(new IdNamePair(-1, ex.Message, 1));
		}
		return list.ToArray();
	}

	public ComboData[] LoadCrossReferenceMasters(Module oModuleType, uint iReportId, int iId, BackTrackType oBackTrackType, int iCompanyId)
	{
		RDDefault rDDefault = new RDDefault();
		string text = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		if (oBackTrackType == BackTrackType.None)
		{
			oBackTrackType = rDDefault.GetBackTrackType(iReportId, oModuleType);
		}
		return Load(oBackTrackType switch
		{
			BackTrackType.Transaction => string.Format("SELECT DISTINCT iBookNo, sName FROM\r\n                    (\r\n                        SELECT iBookNo,sName FROM tCore_Data{0} \r\n                        JOIN mCore_Account ON mCore_Account.iMasterId = tCore_Data{0}.iBookNo WHERE iBodyId = {1}\r\n                        UNION ALL\r\n                        SELECT iCode, sName FROM tCore_Data{0} \r\n                        JOIN mCore_Account ON mCore_Account.iMasterId = tCore_Data{0}.iCode WHERE iBodyId = {1}\r\n                    )temp", suffix, iId), 
			BackTrackType.TransactionHeaderId => string.Format("SELECT DISTINCT iBookNo, sName FROM\r\n                    (\r\n                        SELECT iBookNo,sName FROM tCore_Data{0} \r\n                        JOIN mCore_Account ON mCore_Account.iMasterId = tCore_Data{0}.iBookNo WHERE iHeaderId = {1}\r\n                        UNION ALL\r\n                        SELECT iCode, sName FROM tCore_Data{0} \r\n                        JOIN mCore_Account ON mCore_Account.iMasterId = tCore_Data{0}.iCode WHERE iHeaderId = {1}\r\n                    )temp", suffix, iId), 
			BackTrackType.Details => $"SELECT iMasterId, sName FROM mCore_Account WHERE iMasterId = {iId}", 
			_ => string.Format("SELECT iMasterId, sName FROM mCore_Account WHERE iMasterId = 0", iId), 
		}, 0, iCompanyId);
	}

	private ComboData[] GetWorkflowPath(int iWorkflowId, int iCompanyId)
	{
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		int num = 0;
		string text = null;
		string text2 = null;
		List<string> list = null;
		List<ComboData> list2 = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text = $"SELECT iSeq, sName FROM dbo.fCore_GetLinkChain{FConvert.GetSuffix(iCompanyId)}({iWorkflowId}) ORDER BY iSeq";
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = database.ExecuteReader(dbCommand);
		list = new List<string>();
		list2 = new List<ComboData>();
		while (dataReader.Read())
		{
			if (Convert.ToInt32(dataReader["iSeq"]) == 0)
			{
				text2 = Convert.ToString(dataReader["sName"]) + " VS ";
			}
			else if (num > 0 && num != Convert.ToInt32(dataReader["iSeq"]))
			{
				list2.Add(new ComboData(num, text2 + string.Join(" VS ", list.ToArray())));
				list.Clear();
				list.Add(Convert.ToString(dataReader["sName"]));
			}
			else
			{
				list.Add(Convert.ToString(dataReader["sName"]));
			}
			num = Convert.ToInt32(dataReader["iSeq"]);
		}
		dataReader.Close();
		if (list.Count > 0)
		{
			list2.Add(new ComboData(num, text2 + string.Join(" VS ", list.ToArray())));
		}
		return list2.ToArray();
	}

	public IdNamePair[] LoadMastersWithTransactions(Module oModule, int iUserId, _Filter[] arrReportFilter, int iLanguageId, int iCompanyId)
	{
		List<IdNamePair> arrTagFilter = null;
		List<int> list = null;
		Database database = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		string text = null;
		int num = 0;
		string text2 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text3 = string.Empty;
		string text4 = string.Empty;
		List<FieldData> arrMasterField = null;
		List<MasterRights> list2 = null;
		list = new List<int>();
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		if (iUserId > 1)
		{
			list2 = new COptionbase().CheckRoleMasters(iUserId, "2,3,6,7", ref arrMasterField, database, iCompanyId);
			for (num = 0; num < list2.Count; num++)
			{
				if (oModule == Module.Inventory && list2[num].ID == 2)
				{
					text3 = ((!list2[num].Exclude) ? (" WHERE iProduct IN(" + list2[num].Tag + ")") : (" WHERE iProduct NOT IN(" + list2[num].Tag + ")"));
					text4 = text3;
					break;
				}
			}
		}
		text3 += GetFilterOnTree(arrReportFilter, oModule == Module.CoreTransactions, database, ref arrTagFilter, iCompanyId, oModule == Module.CoreTransactions);
		bool flag = false;
		bool flag2 = false;
		if (arrTagFilter.Count > 0)
		{
			for (num = 0; num < arrTagFilter.Count; num++)
			{
				if (!string.IsNullOrEmpty(arrTagFilter[num].Name))
				{
					if (string.IsNullOrEmpty(text3))
					{
						text3 = " WHERE";
						text4 = " WHERE";
					}
					else if (!string.IsNullOrEmpty(arrTagFilter[num].ExtraInfo) && arrTagFilter[num].ExtraInfo.ToLower() == "or")
					{
						text3 += " OR";
						text4 += " OR";
					}
					else
					{
						text3 += " AND";
						text4 += " AND";
					}
					if (Convert.ToInt32(arrTagFilter[num].Tag) == 1)
					{
						text3 += $" tCore_Data{suffix}.iFaTag IN({arrTagFilter[num].Name})";
						flag = true;
					}
					else if (Convert.ToInt32(arrTagFilter[num].Tag) == 2)
					{
						text3 += $" tCore_Data{suffix}.iInvTag IN({arrTagFilter[num].Name})";
						text4 += string.Format(" iInvTag IN({1})", suffix, arrTagFilter[num].Name);
						flag = true;
					}
					else if (arrTagFilter[num].ID > 2)
					{
						text3 += string.Format(" tCore_Data_Tags{0}.iTag{2} IN({1})", suffix, arrTagFilter[num].Name, arrTagFilter[num].ID);
						flag2 = true;
					}
					else
					{
						text3 += " 1=1";
						text4 += " 1=1";
					}
				}
			}
		}
		else if (!string.IsNullOrEmpty(text3) && oModule == Module.CoreTransactions)
		{
			text3 = (text4 = string.Empty);
		}
		if (flag && oModule == Module.Inventory)
		{
			text2 = string.Format(" JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = tCore_Indta{0}.iBodyId", suffix);
		}
		if (flag2)
		{
			text2 = ((oModule != Module.CoreTransactions) ? (text2 + string.Format(" JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Indta{0}.iBodyId", suffix)) : (text2 + string.Format(" JOIN tCore_Data_Tags{0} ON tCore_Data_Tags{0}.iBodyId = tCore_Data{0}.iBodyId", suffix)));
		}
		switch (oModule)
		{
		case Module.CoreTransactions:
			text = "SELECT iBookNo[iAccount] FROM tCore_Data" + suffix + " " + text2 + " " + text3 + "\r\n                    UNION SELECT iCode FROM tCore_Data" + suffix + " " + text2 + " " + text3;
			if (FConvert.GetYearId(iCompanyId) > 0)
			{
				text = text + " UNION SELECT iAccount FROM tCore_abalsOb" + suffix + " " + text4;
			}
			break;
		case Module.Inventory:
			text = string.Format("Select DISTINCT iProduct FROM tCore_Indta{0} {2}\r\n                JOIN vrCore_Product ON vrCore_Product.iMasterId = tCore_Indta{0}.iProduct {1}", suffix, text3, text2);
			if (FConvert.GetYearId(iCompanyId) > 0)
			{
				text += string.Format(" union select iProduct from tCore_ibalsOb{0} WITH (READUNCOMMITTED) \r\nJOIN vrCore_Product ON vrCore_Product.iMasterId = tCore_ibalsOb{0}.iProduct {1}", FConvert.GetSuffix(iCompanyId), text4);
			}
			break;
		}
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			list.Add(Convert.ToInt32(dataReader[0]));
		}
		dataReader.Close();
		List<masterTree> list3 = null;
		if (list3 == null)
		{
			list3 = loadMasterTree((oModule == Module.CoreTransactions) ? 1 : 2, iLanguageId, database);
		}
		if (list.Count == 0 && arrReportFilter != null && arrReportFilter.Length != 0)
		{
			return new IdNamePair[1]
			{
				new IdNamePair(0, string.Empty, 0)
			};
		}
		return (from p in list3
			join m in list on p.id equals m
			orderby p.sequence
			select new IdNamePair(p.id, p.name + " " + p.code, p.sequence)).ToArray();
	}

	public IdNamePair[] LoadRefrnMastersWithTransactions(RepRecord objRec, int iCompanyId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		List<MasterRights> list = null;
		string xmlMaster = getXmlMaster(string.Join(",", objRec.Masters.Select((ComboData p) => p.ID.ToString())));
		int yearId = FConvert.GetYearId(iCompanyId);
		List<FieldData> arrMasterField = null;
		_ = string.Empty;
		int num = 0;
		string text = $"{xmlMaster}\r\n            SELECT tCore_Data_{yearId}.iCode\r\n            FROM @XML.nodes('/Master/r') T(c) \r\n            JOIN tCore_Refrn_{yearId} ON tCore_Refrn_{yearId}.iCode = T.c.value('./@id', 'int')\r\n            JOIN tCore_Data_{yearId} ON tCore_Refrn_{yearId}.iBodyId = tCore_Data_{yearId}.iBodyId\r\n            JOIN tCore_Header_{yearId} ON tCore_Data_{yearId}.iHeaderId = tCore_Header_{yearId}.iHeaderId\r\n            WHERE tCore_Header_{yearId}.bSuspended = 0";
		List<IdNamePair> list2 = new List<IdNamePair>();
		DbCommand sqlStringCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		sqlStringCommand.CommandTimeout = 0;
		IDataReader dataReader = database.ExecuteReader(sqlStringCommand);
		while (dataReader.Read())
		{
			list2.Add(new IdNamePair(Convert.ToInt32(dataReader[0]), string.Empty, 0));
		}
		dataReader.Close();
		if (objRec.UserId > 1 && _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.Print, 36) == 0)
		{
			if (_focus.company(iCompanyId).user(objRec.UserId).RoleMasters == null)
			{
				list = new COptionbase().CheckRoleMasters(objRec.UserId, "2,3,6,7", ref arrMasterField, database, iCompanyId);
				_focus.company(iCompanyId).user(objRec.UserId).RoleMasters = new UserRoleField(list, arrMasterField);
			}
			list = _focus.company(iCompanyId).user(objRec.UserId).RoleMasters.masterRestrictions;
			for (num = 0; num < list.Count; num++)
			{
				if (list[num].ID != 1)
				{
					continue;
				}
				if (Convert.ToString(list[num].Tag).Split(',').Length < 1000)
				{
					if (list[num].Exclude)
					{
						_ = " WHERE mCore_Account_Props.iMasterId NOT IN(" + list[num].Tag + ")";
					}
					else
					{
						_ = " WHERE mCore_Account_Props.iMasterId IN(" + list[num].Tag + ")";
					}
				}
				break;
			}
		}
		List<masterTree> list3 = null;
		if (list3 == null)
		{
			list3 = loadMasterTree(1, objRec.LanguageId, database);
		}
		return (from p in list3
			join m in list2 on p.id equals m.ID
			orderby p.sequence
			select new IdNamePair(m.ID, p.sequence.ToString(), m.Tag)).ToArray();
	}

	public IdNamePair[] LoadLedgerMastersWithTransactions(RepRecord objRec, int iCompanyId)
	{
		int num = 0;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		string text2 = string.Empty;
		string text3 = null;
		List<FieldData> arrMasterField = null;
		List<MasterRights> list = null;
		List<IdNamePair> list2 = null;
		List<IdNamePair> arrTagFilter = null;
		Database database = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text3 = getXmlMaster(string.Join(", ", objRec.Masters.Select((ComboData p) => p.ID.ToString())));
		text = string.Format("{1} \r\n            SELECT iCode, CASE WHEN iDate < {2} OR iVoucherType  = 256 THEN -2 ELSE 0 END[OpeningData]\r\n            FROM @XML.nodes('/Master/r') T(c) \r\n            JOIN tCore_Data{0} ON tCore_Data{0}.iCode = T.c.value('./@id', 'int')\r\n            JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n            WHERE tCore_Header{0}.bSuspended = 0\r\n            UNION\r\n            SELECT iBookNo, CASE WHEN iDate < {2} OR iVoucherType = 256 THEN -2 ELSE 0 END[OpeningData]\r\n            FROM @XML.nodes('/Master/r') T(c) \r\n            JOIN tCore_Data{0} ON tCore_Data{0}.iBookNo = T.c.value('./@id', 'int')\r\n            JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n            WHERE tCore_Header{0}.bSuspended = 0", suffix, text3, objRec.StartingDate);
		if (FConvert.GetYearId(iCompanyId) > 0)
		{
			text += $" UNION SELECT iAccount, -2[OpeningData] FROM tCore_abalsOb{suffix} WHERE (mBalance <> 0 OR mBalanceFX <> 0 OR mBalanceLocal <> 0) ";
		}
		list2 = new List<IdNamePair>();
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			list2.Add(new IdNamePair(Convert.ToInt32(dataReader[0]), string.Empty, Convert.ToInt32(dataReader[1])));
		}
		dataReader.Close();
		if (objRec.UserId > 1 && _focus.company(iCompanyId).getPreferenceValue(PreferenceCategories.Print, 36) == 0)
		{
			if (_focus.company(iCompanyId).user(objRec.UserId).RoleMasters == null)
			{
				list = new COptionbase().CheckRoleMasters(objRec.UserId, "2,3,6,7", ref arrMasterField, database, iCompanyId);
				_focus.company(iCompanyId).user(objRec.UserId).RoleMasters = new UserRoleField(list, arrMasterField);
			}
			list = _focus.company(iCompanyId).user(objRec.UserId).RoleMasters.masterRestrictions;
			for (num = 0; num < list.Count; num++)
			{
				if (list[num].ID == 1)
				{
					if (Convert.ToString(list[num].Tag).Split(',').Length < 1000)
					{
						text2 = ((!list[num].Exclude) ? (" WHERE mCore_Account_Props.iMasterId IN(" + list[num].Tag + ")") : (" WHERE mCore_Account_Props.iMasterId NOT IN(" + list[num].Tag + ")"));
					}
					break;
				}
			}
		}
		text2 += GetFilterOnTree(objRec.FilterSource, IsAccount: true, database, ref arrTagFilter, iCompanyId);
		if (ProngHorn.USESOCKET(iCompanyId) && ProngHorn.GetSocketFlag(iCompanyId).MastersEnabled)
		{
			for (num = 0; num < list2.Count; num++)
			{
				if (Convert.ToInt32(list2[num].Tag) != -2)
				{
					MastersGetTypeRetParam masterInfos = ProngHorn.GetMasterInfos(1, list2[num].ID, iCompanyId);
					list2[num].Tag = masterInfos.Consolidationoftransactions;
				}
			}
		}
		else
		{
			text3 = getXmlMaster(string.Join(",", list2.Select((IdNamePair p) => p.ID.ToString())));
			if (!string.IsNullOrEmpty(text3))
			{
				text = $"{text3} SELECT mCore_Account_Props.iMasterId, ISNULL(mCore_Account_Props.iConsolidationoftransactions,0)[iValue] \r\n                FROM @XML.nodes('/Master/r') T(c) JOIN mCore_Account_Props ON mCore_Account_Props.iMasterId = T.c.value('./@id', 'int') {text2}";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dbCommand.CommandTimeout = 0;
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					for (num = 0; num < list2.Count; num++)
					{
						if (list2[num].ID == Convert.ToInt32(dataReader[0]))
						{
							if (Convert.ToInt32(list2[num].Tag) != -2)
							{
								list2[num].Tag = Convert.ToInt32(dataReader[1]);
							}
							break;
						}
					}
				}
				dataReader.Close();
			}
		}
		List<masterTree> list3 = null;
		if (list3 == null)
		{
			list3 = loadMasterTree(1, objRec.LanguageId, database);
		}
		return (from p in list3
			join m in list2 on p.id equals m.ID
			orderby p.sequence
			select new IdNamePair(m.ID, p.sequence.ToString(), m.Tag)).ToArray();
	}

	public IdNamePair[] LoadLedgerMastersWithTransactions(int iUserId, int iStartDate, _Filter[] arrReportFilter, int iCompanyId)
	{
		int num = 0;
		List<IdNamePair> list = null;
		Database database = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		string text2 = string.Empty;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = string.Empty;
		List<FieldData> arrMasterField = null;
		List<MasterRights> list2 = null;
		Dictionary<int, bool> dictionary = null;
		List<int> list3 = null;
		List<IdNamePair> arrTagFilter = null;
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text4 = GetFilterOnTree(arrReportFilter, IsAccount: true, database, ref arrTagFilter, iCompanyId);
		if (arrTagFilter != null && arrTagFilter.Count > 0)
		{
			for (num = 0; num < arrTagFilter.Count; num++)
			{
				if (!string.IsNullOrEmpty(arrTagFilter[num].Name) && arrTagFilter[num].ID == _focus.company(iCompanyId).faTagId)
				{
					text3 += $"{((num > 0) ? ','.ToString() : string.Empty)}{arrTagFilter[num].Name}";
				}
			}
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = $" AND tCore_Data{suffix}.iFaTag IN({text3})";
			}
		}
		if (iUserId > 1)
		{
			list2 = new COptionbase().CheckRoleMasters(iUserId, "2,3,6,7", ref arrMasterField, database, iCompanyId);
			for (num = 0; num < list2.Count; num++)
			{
				if (list2[num].ID == 1)
				{
					if (Convert.ToString(list2[num].Tag).Split(',').Length < 1000)
					{
						text6 = ((!list2[num].Exclude) ? (" WHERE mCore_Account_Props.iMasterId IN(" + list2[num].Tag + ")") : (" WHERE mCore_Account_Props.iMasterId NOT IN(" + list2[num].Tag + ")"));
					}
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text6))
		{
			text6 = "WHERE 1=1 ";
		}
		if (!string.IsNullOrEmpty(text4))
		{
			text3 += text4;
			text5 = string.Format("JOIN vrCore_Account BookNo ON BookNo.iMasterId = tCore_Data{0}.iBookNo \r\n                JOIN vrCore_Account Code ON Code.iMasterId = tCore_Data{0}.iCode ", suffix);
		}
		text6 += text3;
		if (FConvert.GetYearId(iCompanyId) > 0)
		{
			text2 = " UNION SELECT iAccount FROM tCore_abalsOb" + suffix + " \r\n                    WHERE (mBalance <> 0 OR mBalanceFX <> 0 OR mBalanceLocal <> 0) ";
		}
		text = $"SELECT DISTINCT iCode\r\n            FROM tCore_Data{suffix}\r\n            JOIN tCore_Header{suffix} ON tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId {text5}\r\n            WHERE tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Header{suffix}.bSuspended = 0 \r\n            AND tCore_Data{suffix}.iAuthStatus < 2 AND (iDate < {iStartDate} OR iVoucherType  = 256) {text3}\r\n            {text2}";
		dictionary = new Dictionary<int, bool>();
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			dictionary.Add(Convert.ToInt32(dataReader[0]), value: true);
		}
		dataReader.Close();
		text = $"SELECT iCode \r\n\t        FROM tCore_Data{suffix}\r\n\t        JOIN tCore_Header{suffix} ON tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId {text5}\r\n\t        WHERE tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Header{suffix}.bSuspended = 0 \r\n\t        AND tCore_Data{suffix}.iAuthStatus < 2 AND iDate >= {iStartDate} AND iVoucherType <> 256 {text3}\r\n\t        UNION\r\n\t        select iBookNo\r\n\t        FROM tCore_Data{suffix}\r\n\t        JOIN tCore_Header{suffix} ON tCore_Data{suffix}.iHeaderId = tCore_Header{suffix}.iHeaderId {text5}\r\n\t        WHERE tCore_Data{suffix}.bUpdateFA = 1 AND tCore_Data{suffix}.bSuspendUpdateFA <> 1 AND tCore_Header{suffix}.bSuspended = 0 \r\n\t        AND tCore_Data{suffix}.iAuthStatus < 2 AND iDate >= {iStartDate} AND iVoucherType <> 256 {text3}";
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		list3 = new List<int>();
		while (dataReader.Read())
		{
			list3.Add(Convert.ToInt32(dataReader[0]));
		}
		dataReader.Close();
		for (num = 0; num < list3.Count; num++)
		{
			dictionary.Remove(list3[num]);
		}
		text = string.Format("SELECT mCore_Account_Props.iMasterId, ISNULL(mCore_Account_Props.iConsolidationoftransactions,0)[Value] \r\n                FROM tCore_Data{0}\r\n                JOIN mCore_Account_Props ON tCore_Data{0}.iBookNo = mCore_Account_Props.iMasterId \r\n                {1} {2}\r\n                UNION \r\n                SELECT mCore_Account_Props.iMasterId, ISNULL(mCore_Account_Props.iConsolidationoftransactions,0)[Value] \r\n                FROM tCore_Data{0}\r\n                JOIN mCore_Account_Props ON mCore_Account_Props.iMasterId = tCore_Data{0}.iCode \r\n                {1} {2}", suffix, text5, text6);
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		list = new List<IdNamePair>();
		while (dataReader.Read())
		{
			list.Add(new IdNamePair(Convert.ToInt32(dataReader[0]), Convert.ToInt32(dataReader[1])));
		}
		dataReader.Close();
		bool value = false;
		for (num = 0; num < list.Count; num++)
		{
			dictionary.TryGetValue(list[num].ID, out value);
			if (value)
			{
				list[num].Tag = -2;
				dictionary.Remove(list[num].ID);
			}
		}
		for (num = 0; num < dictionary.Count; num++)
		{
			list.Add(new IdNamePair(dictionary.Keys.ElementAt(num), "0", -2));
		}
		List<masterTree> list4 = null;
		if (list4 == null)
		{
			list4 = loadMasterTree(1, 0, database);
		}
		return (from p in list4
			join m in list on p.id equals m.ID
			orderby p.sequence
			select new IdNamePair(m.ID, p.sequence.ToString(), m.Tag)).ToArray();
	}

	public IdNamePair[] LoadAccountsWithTransactions(int iMasterId, int iEndDate, _Filter[] arrReportFilter, int iCompanyId)
	{
		List<IdValuePair> list = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<masterTree> list2 = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		string text2 = string.Empty;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		List<IdNamePair> arrTagFilter = null;
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		if (iMasterId > 0)
		{
			text3 += $" AND mCore_Account_Props.iMasterId = {iMasterId}";
		}
		else
		{
			text4 = GetFilterOnTree(arrReportFilter, IsAccount: true, database, ref arrTagFilter, iCompanyId);
			if (arrTagFilter != null && arrTagFilter.Count > 0)
			{
				for (int i = 0; i < arrTagFilter.Count; i++)
				{
					if (!string.IsNullOrEmpty(arrTagFilter[i].Name) && arrTagFilter[i].ID == _focus.company(iCompanyId).faTagId)
					{
						text3 += $"{((i > 0) ? ','.ToString() : string.Empty)}{arrTagFilter[i].Name}";
					}
				}
				if (!string.IsNullOrEmpty(text3))
				{
					text3 = $" AND tCore_Data{suffix}.iFaTag IN({text3})";
				}
			}
		}
		if (!string.IsNullOrEmpty(text4))
		{
			text3 += text4;
			text5 = string.Format("JOIN vrCore_Account BookNo ON BookNo.iMasterId = tCore_Data{0}.iBookNo \r\n                JOIN vrCore_Account Code ON Code.iMasterId = tCore_Data{0}.iCode ", suffix);
		}
		if (FConvert.GetYearId(iCompanyId) > 0)
		{
			text2 = " UNION SELECT iAccount, CASE WHEN mCore_Account_Props.iDisplayCreditTotal > 0 THEN mCore_Account_Props.iDisplayCreditTotal + 3 ELSE mCore_Account_Props.iConsolidationoftransactions END[iConsolidationoftransactions]\r\n                    FROM tCore_abalsOb" + suffix + " \r\n                    JOIN mCore_Account_Props ON mCore_Account_Props.iMasterId = tCore_abalsOb" + suffix + ".iAccount\r\n                    WHERE (mBalance <> 0 OR mBalanceFX <> 0 OR mBalanceLocal <> 0) ";
		}
		text = string.Format("SELECT iCode, CASE WHEN mCore_Account_Props.iDisplayCreditTotal > 0 THEN mCore_Account_Props.iDisplayCreditTotal + 3 ELSE mCore_Account_Props.iConsolidationoftransactions END[iConsolidationoftransactions]\r\n            FROM tCore_Header{0} \r\n            JOIN tCore_Data{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId \r\n            JOIN mCore_Account_Props ON tCore_Data{0}.iCode = mCore_Account_Props.iMasterId {1} \r\n            WHERE {5} tCore_Header{0}.bSuspended = 0 \r\n            AND tCore_Data{0}.iAuthStatus < 2 AND iDate <= {3} {4}\r\n            {2}\r\n            UNION\r\n            SELECT iBookNo, CASE WHEN mCore_Account_Props.iDisplayCreditTotal > 0 THEN mCore_Account_Props.iDisplayCreditTotal + 3 ELSE mCore_Account_Props.iConsolidationoftransactions END[iConsolidationoftransactions]\r\n            FROM tCore_Header{0} \r\n            JOIN tCore_Data{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId \r\n            JOIN mCore_Account_Props ON tCore_Data{0}.iBookNo = mCore_Account_Props.iMasterId {1} \r\n            WHERE {5} tCore_Header{0}.bSuspended = 0 \r\n            AND tCore_Data{0}.iAuthStatus < 2 AND iDate <= {3} {4}\r\n            {2}", suffix, text5, text2, iEndDate, text3, (iEndDate == int.MaxValue) ? string.Empty : $"tCore_Data{suffix}.bUpdateFA = 1 AND");
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dbCommand.CommandTimeout = 0;
		dataReader = database.ExecuteReader(dbCommand);
		list = new List<IdValuePair>();
		while (dataReader.Read())
		{
			list.Add(new IdValuePair(Convert.ToInt32(dataReader[0]), Convert.ToInt32(dataReader[1])));
		}
		dataReader.Close();
		if (iMasterId > 0)
		{
			list2 = new List<masterTree>();
			list2.Add(new masterTree
			{
				id = iMasterId,
				sequence = 0
			});
		}
		else if (list2 == null)
		{
			suffix = $"CompId:{iCompanyId}-500-1-MasterTree";
			object buffer = _focus.company(iCompanyId).getBuffer(suffix);
			if (buffer != null)
			{
				list2 = (List<masterTree>)buffer;
			}
			else
			{
				list2 = loadMasterTree(1, 0, database);
				_focus.company(iCompanyId).setBuffer(suffix, list2);
			}
		}
		return (from p in list2
			join m in list on p.id equals m.ID
			orderby p.sequence
			select new IdNamePair(m.ID, p.sequence.ToString(), p.name, m.Value)).ToArray();
	}

	private List<masterTree> loadMasterTree(int iTypeId, int iLanguageId, Database objDb)
	{
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		string text = null;
		List<masterTree> list = new List<masterTree>();
		text = iTypeId switch
		{
			1 => "Core_Account", 
			2 => "Core_Product", 
			_ => Convert.ToString(objDb.ExecuteScalar($"SELECT sModule+'_'+ sMasterName FROM cCore_MasterDef WHERE iMasterTypeId = {iTypeId}")), 
		};
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(string.Format("SELECT iMasterId,sName,sCode,iSeq FROM dbo.f{0}_Get{1}TreeSequenceByLangId(0,0,{2}) WHERE bGroup=0  ORDER BY iSeq", text.Substring(0, text.IndexOf("_")), text.Substring(text.IndexOf("_") + 1), iLanguageId)) : string.Format("SELECT iMasterId,sName,sCode,iSeq FROM dbo.f{0}_Get{1}TreeSequenceByLangId(0,0,{2}) WHERE bGroup=0  ORDER BY iSeq", text.Substring(0, text.IndexOf("_")), text.Substring(text.IndexOf("_") + 1), iLanguageId));
		dbCommand.CommandTimeout = 0;
		dataReader = objDb.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			masterTree masterTree2 = new masterTree();
			masterTree2.id = Convert.ToInt32(dataReader["iMasterId"]);
			masterTree2.sequence = Convert.ToInt32(dataReader["iSeq"]);
			masterTree2.name = Convert.ToString(dataReader["sName"]);
			masterTree2.code = Convert.ToString(dataReader["sCode"]);
			list.Add(masterTree2);
		}
		dataReader.Close();
		return list;
	}

	public IdNamePair[] LoadSubLedgerMastersWithTransactions(int iUserId, int iMasterId, _Filter[] arrReportFilter, int iCompanyId)
	{
		List<IdNamePair> list = null;
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		bool flag = false;
		string text = null;
		int iLanguageId = 0;
		string text2 = string.Empty;
		List<int> list2 = new List<int>();
		List<FieldData> arrMasterField = null;
		List<MasterRights> list3 = null;
		List<IdNamePair> arrTagFilter = null;
		list = new List<IdNamePair>();
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		if (iMasterId > 0)
		{
			text = $"SELECT iDisplayCreditTotal FROM mCore_Account_Props WHERE iMasterId = {iMasterId}";
			flag = Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text)) == 5;
			if (!flag)
			{
				text = $"WITH RecursionCTE (iMasterId, iParentId, bGroup)  AS  \r\n                        (   SELECT a.iMasterId, b.iParentId, a.bGroup\r\n                            FROM mCore_Account a join mCore_AccountTreeDetails b on a.iMasterId = b.iMasterId \r\n                            WHERE a.iMasterId = {iMasterId} AND iTreeId = 0\r\n                            UNION ALL  \r\n                            SELECT R1.iMasterId, R1.iParentId, R1.bGroup\r\n                            FROM vmCore_Account AS R1\r\n                            JOIN RecursionCTE AS R2 ON R1.iParentId = R2.iMasterId\r\n                            WHERE iTreeId = 0\r\n                        )SELECT iMasterId  FROM RecursionCTE WHERE bGroup = 0 ";
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				dbCommand.CommandTimeout = 0;
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					list2.Add(Convert.ToInt32(dataReader[0]));
				}
				dataReader.Close();
			}
		}
		if (!flag)
		{
			if (iUserId > 1)
			{
				list3 = new COptionbase().CheckRoleMasters(iUserId, "2,3,6,7", ref arrMasterField, database, iCompanyId);
				for (int i = 0; i < list3.Count; i++)
				{
					if (list3[i].ID == 1)
					{
						if (Convert.ToString(list3[i].Tag).Split(',').Length < 1000)
						{
							if (list3[i].Exclude)
							{
								text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + " AND Code.iMasterId NOT IN(" + list3[i].Tag + ")") : (" WHERE Code.iMasterId NOT IN(" + list3[i].Tag + ")"));
							}
							else
							{
								text2 = ((!string.IsNullOrEmpty(text2)) ? (text2 + " AND Code.iMasterId IN(" + list3[i].Tag + ")") : (" WHERE Code.iMasterId IN(" + list3[i].Tag + ")"));
							}
						}
						break;
					}
				}
			}
			text2 += GetFilterOnTree(arrReportFilter, IsAccount: true, database, ref arrTagFilter, iCompanyId);
			text = string.Format("SELECT Code.iMasterId, Code.sName, Code.sCode\r\n                    FROM tCore_Data{0} \r\n                    JOIN mCore_Account Code ON Code.iMasterId = tCore_Data{0}.iBookNo {1}\r\n                    UNION \r\n                    SELECT Code.iMasterId, Code.sName, Code.sCode\r\n                    FROM tCore_Data{0} \r\n                    JOIN mCore_Account Code ON Code.iMasterId = tCore_Data{0}.iCode {1}", FConvert.GetSuffix(iCompanyId), text2);
			if (FConvert.GetYearId(iCompanyId) > 0)
			{
				text += string.Format(" UNION SELECT iAccount,Code.sName,Code.sCode FROM tCore_abalsOb{0} \r\n                    JOIN mCore_Account Code ON Code.iMasterId = tCore_abalsOb{0}.iAccount", FConvert.GetSuffix(iCompanyId));
			}
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iMasterId"]), Convert.ToString(dataReader["sName"]), Convert.ToString(dataReader["sCode"])));
			}
			dataReader.Close();
			if (list2.Count > 0)
			{
				list = (from p in list
					join m in list2 on p.ID equals m
					select p).ToList();
			}
			else if (iMasterId > 0)
			{
				list = list.Where((IdNamePair p) => p.ID == iMasterId).ToList();
			}
		}
		List<masterTree> list4 = null;
		if (list4 == null)
		{
			list4 = loadMasterTree(1, iLanguageId, database);
		}
		return (from p in list4
			join m in list on p.id equals m.ID
			orderby p.sequence
			select new IdNamePair(m.ID, p.sequence.ToString(), $"{m.Name} [{m.Tag}]")).ToArray();
	}

	public IdNamePair[] LoadMastersForBillwise(int iDate, uint ReportId, ComboData[] arrSelectedMasters, _Filter[] arrReportFilter, int iCompanyId)
	{
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string text = null;
		string suffix = FConvert.GetSuffix(iCompanyId);
		byte yearId = FConvert.GetYearId(iCompanyId);
		List<int> list = new List<int>();
		database = DatabaseWrapper.GetDatabase2(iCompanyId);
		string arg = "mCore_Account";
		if (_focus.company(iCompanyId).isModuleImplemented(ModulesImplemented.SubLedger))
		{
			arg = "mCore_Subledger";
		}
		if (yearId == 0)
		{
			switch ((FocusReport)ReportId)
			{
			case FocusReport.CustomerStatements:
			case FocusReport.CustomerDueDateAnalysis:
				text = string.Format("Select distinct iCode from tCore_Refrn{0} \r\n                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                            where iRefType < 2 and mCore_Account.iAccountType in (5,7)", suffix, iDate, arg);
				break;
			case FocusReport.CustomerBillwiseSummary:
			case FocusReport.CustomerListingofOutstandingBills:
			case FocusReport.CustomerAgeingSummaryBillwise:
			case FocusReport.CustomerAgeingDetailsBillwise:
			case FocusReport.CustomerDetailAgeingByDueDate:
			case FocusReport.CustomerSummaryAgeingByDueDate:
			case FocusReport.CustomerOverdueAnalysis:
			case FocusReport.CustomerOverdueSummary:
				text = string.Format(" Select distinct iCode from(\r\n                            SELECT tCore_Refrn{0}.iCode,iRef,\r\n\t                            sum(tCore_Refrn{0}.mAmount) [mAmount]\r\n                                FROM tCore_Refrn{0} WITH (READUNCOMMITTED)\r\n                                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = tCore_Refrn{0}.iBodyId\r\n                                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n\t                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                                WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus <2 \r\n                                AND tCore_Data{0}.bUpdateFA=1 \r\n\t                            and mCore_Account.iAccountType in (5,7) and tCore_Header{0}.iDate<={1} --for customers\r\n\t                            group by tCore_Refrn{0}.iCode, iRef\r\n                                Having sum(tCore_Refrn{0}.mAmount) <> 0 \r\n\t                            ) A\r\n\t                            order by iCode", suffix, iDate, arg);
				break;
			case FocusReport.VendorStatements:
			case FocusReport.VendorDueDateAnalysis:
				text = string.Format("Select distinct iCode from tCore_Refrn{0} \r\n                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                            where iRefType < 2 and mCore_Account.iAccountType in (6,7)", suffix, iDate, arg);
				break;
			case FocusReport.VendorListingofOutstandingBills:
			case FocusReport.VendorAgeingSummaryBillwise:
			case FocusReport.VendorAgeingDetailsBillwise:
			case FocusReport.VendorDetailAgeingByDueDate:
			case FocusReport.VendorSummaryAgeingByDueDate:
			case FocusReport.VendorOverdueAnalysis:
			case FocusReport.VendorOverdueSummary:
			case FocusReport.VendorBillwiseSummary:
				text = string.Format(" Select distinct iCode from(\r\n                            SELECT tCore_Refrn{0}.iCode,iRef,\r\n\t                            sum(tCore_Refrn{0}.mAmount) [mAmount]\r\n                                FROM tCore_Refrn{0} WITH (READUNCOMMITTED)\r\n                                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = tCore_Refrn{0}.iBodyId\r\n                                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n\t                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                                WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus <2 \r\n                                AND tCore_Data{0}.bUpdateFA=1 \r\n\t                            and mCore_Account.iAccountType in (6,7) and tCore_Header{0}.iDate<={1} --for Vendor\r\n                                group by tCore_Refrn{0}.iCode, iRef\r\n                                Having sum(tCore_Refrn{0}.mAmount) <> 0 \r\n\t                       \r\n                            ) A\r\n\t                            order by iCode", suffix, iDate, arg);
				break;
			}
		}
		else
		{
			switch ((FocusReport)ReportId)
			{
			case FocusReport.CustomerStatements:
			case FocusReport.CustomerDueDateAnalysis:
				text = string.Format(" Select distinct iCode from (\r\n                        Select iCode from tCore_Refrn{0} r\r\n                        inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                        where iRefType < 2 and mCore_Account.iAccountType in (5,7)\r\n                        union all \r\n                        Select iCode from tCore_RefrnOb{0} r\r\n                        inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                        Where mCore_Account.iAccountType in (5,7)\r\n                        ) Ref\r\n                    ", suffix, iDate, arg);
				break;
			case FocusReport.CustomerBillwiseSummary:
			case FocusReport.CustomerListingofOutstandingBills:
			case FocusReport.CustomerAgeingSummaryBillwise:
			case FocusReport.CustomerAgeingDetailsBillwise:
			case FocusReport.CustomerDetailAgeingByDueDate:
			case FocusReport.CustomerSummaryAgeingByDueDate:
			case FocusReport.CustomerOverdueAnalysis:
			case FocusReport.CustomerOverdueSummary:
				text = string.Format(" Select distinct iCode from(\r\n                            SELECT tCore_Refrn{0}.iCode,iRef,\r\n\t                            sum(tCore_Refrn{0}.mAmount) [mAmount]\r\n                                FROM tCore_Refrn{0} WITH (READUNCOMMITTED)\r\n                                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = tCore_Refrn{0}.iBodyId\r\n                                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n\t                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                                WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus <2 \r\n                                AND tCore_Data{0}.bUpdateFA=1 \r\n\t                            and mCore_Account.iAccountType in (5,7) and tCore_Header{0}.iDate<={1} --for Vendor\r\n                                group by tCore_Refrn{0}.iCode, iRef\r\n                                Having sum(tCore_Refrn{0}.mAmount) <> 0 \r\n\t                        union all\r\n\t                             Select iCode,iRef, [mAmount] from tCore_RefrnOb{0} r\r\n\t                             inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                                 where mCore_Account.iAccountType in (5,7) \r\n                            ) A\r\n\t                            order by iCode", suffix, iDate, arg);
				break;
			case FocusReport.VendorStatements:
			case FocusReport.VendorDueDateAnalysis:
				text = string.Format(" Select distinct iCode from (\r\n                        Select iCode from tCore_Refrn{0} r\r\n                        inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                        where iRefType < 2 and mCore_Account.iAccountType in (6,7)\r\n                        union all \r\n                        Select iCode from tCore_RefrnOb{0} r\r\n                        inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                        Where mCore_Account.iAccountType in (6,7)\r\n                        ) Ref\r\n                    ", suffix, iDate, arg);
				break;
			case FocusReport.VendorListingofOutstandingBills:
			case FocusReport.VendorAgeingSummaryBillwise:
			case FocusReport.VendorAgeingDetailsBillwise:
			case FocusReport.VendorDetailAgeingByDueDate:
			case FocusReport.VendorSummaryAgeingByDueDate:
			case FocusReport.VendorOverdueAnalysis:
			case FocusReport.VendorOverdueSummary:
			case FocusReport.VendorBillwiseSummary:
				text = string.Format(" Select distinct iCode from(\r\n                            SELECT tCore_Refrn{0}.iCode,iRef,\r\n\t                            sum(tCore_Refrn{0}.mAmount) [mAmount]\r\n                                FROM tCore_Refrn{0} WITH (READUNCOMMITTED)\r\n                                JOIN tCore_Data{0} ON tCore_Data{0}.iBodyId = tCore_Refrn{0}.iBodyId\r\n                                JOIN tCore_Header{0} ON tCore_Data{0}.iHeaderId = tCore_Header{0}.iHeaderId\r\n\t                            inner join {2} mCore_Account on mCore_Account.iMasterId= tCore_Refrn{0}.iCode\r\n                                WHERE tCore_Data{0}.bSuspendUpdateFA <> 1 AND tCore_Header{0}.bSuspended = 0 AND tCore_Data{0}.iAuthStatus <2 \r\n                                AND tCore_Data{0}.bUpdateFA=1 \r\n\t                            and mCore_Account.iAccountType in (6,7) and tCore_Header{0}.iDate<={1} --for Vendor\r\n                                group by tCore_Refrn{0}.iCode, iRef\r\n                                Having sum(tCore_Refrn{0}.mAmount) <> 0 \r\n\t                        union all\r\n\t                             Select iCode,iRef, [mAmount] from tCore_RefrnOb{0} r\r\n\t                             inner join {2} mCore_Account on mCore_Account.iMasterId= r.iCode\r\n                                 where mCore_Account.iAccountType in (6,7) \r\n                            ) A\r\n\t                            order by iCode", suffix, iDate, arg);
				break;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(Convert.ToInt32(dataReader[0]));
			}
			dataReader.Close();
		}
		else if (arrSelectedMasters != null)
		{
			list.AddRange(arrSelectedMasters.Select((ComboData p) => p.ID).ToArray());
		}
		int iTypeId = 1;
		if (_focus.company(iCompanyId).isModuleImplemented(ModulesImplemented.SubLedger))
		{
			iTypeId = 50;
		}
		List<masterTree> list2 = null;
		if (list2 == null)
		{
			suffix = $"CompId:{iCompanyId}-500-1-MasterTree";
			object buffer = _focus.company(iCompanyId).getBuffer(suffix);
			if (buffer != null)
			{
				list2 = (List<masterTree>)buffer;
			}
			else
			{
				list2 = loadMasterTree(iTypeId, 0, database);
				_focus.company(iCompanyId).setBuffer(suffix, list2);
			}
		}
		if (arrSelectedMasters != null)
		{
			list = ((arrSelectedMasters != null) ? (from p in arrSelectedMasters
				join m in list on p.ID equals m
				select p.ID).ToList() : new List<int>());
		}
		return (from p in list2
			join m in list on p.id equals m
			orderby p.sequence
			select new IdNamePair(p.id, p.name + " " + p.code, p.sequence)).ToArray();
	}

	public int getAccountProperty(int iMasterId, int iCompanyId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		string text = $"SELECT iDisplayCreditTotal FROM mCore_Account_Props WHERE iMasterId = {iMasterId}";
		return Convert.ToInt32(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
	}

	private string GetFilterOnTree(_Filter[] arrFilter, bool IsAccount, Database objDb, ref List<IdNamePair> arrTagFilter, int iCompanyId, bool bFilterTagOnly = false)
	{
		string result = string.Empty;
		arrTagFilter = new List<IdNamePair>();
		if (arrFilter != null && arrFilter.Length != 0)
		{
			int num = 0;
			bool flag = false;
			List<_Filter> list = new List<_Filter>();
			QueryGenerator queryGenerator = new QueryGenerator();
			for (byte b = 0; b < arrFilter.Length; b++)
			{
				if (!arrFilter[b].IsGroup)
				{
					if (b > 0 && arrFilter[b].Conjuction != Conjuction.And)
					{
						list.Clear();
						arrTagFilter.Clear();
						break;
					}
					flag = false;
					if (IsAccount && !bFilterTagOnly)
					{
						if (arrFilter[b].SubParentId == 3 || arrFilter[b].SubParentId == 12 || arrFilter[b].SubParentId == 4 || arrFilter[b].SubParentId == 39)
						{
							list.Add(arrFilter[b]);
							flag = true;
						}
					}
					else if (!bFilterTagOnly && arrFilter[b].SubParentId == 23 && (arrFilter[b].FieldId & 0xFF0000) >> 16 != 128 && (arrFilter[b].FieldId & 0xFF0000) >> 16 != 131 && (arrFilter[b].FieldId & 0xFF0000) >> 16 != 132)
					{
						list.Add(arrFilter[b]);
						flag = true;
					}
					if (!flag && (arrFilter[b].DataType == MasterDataType.Master || arrFilter[b].DataType == MasterDataType.Number))
					{
						num = Convert.ToInt32(objDb.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"SELECT iMasterId FROM vmuCore_MasterFields WHERE iFieldId = {arrFilter[b].FieldId}") : $"SELECT iMasterId FROM vmuCore_MasterFields WHERE iFieldId = {arrFilter[b].FieldId}"));
						if (num > 0 && num != 12 && num != 11)
						{
							if ((num == _focus.company(iCompanyId).faTagId) & IsAccount)
							{
								arrTagFilter.Add(new IdNamePair(num, _focus.company(iCompanyId).getFinanceFilter(arrFilter), Convert.ToString(arrFilter[b].Conjuction), 1));
							}
							if (num == _focus.company(iCompanyId).invTagId && !IsAccount)
							{
								arrTagFilter.Add(new IdNamePair(num, _focus.company(iCompanyId).getInventoryFilter(arrFilter), Convert.ToString(arrFilter[b].Conjuction), 2));
							}
							if (num != _focus.company(iCompanyId).faTagId && num != _focus.company(iCompanyId).invTagId)
							{
								arrTagFilter.Add(new IdNamePair(num, arrFilter[b].CompareValue, Convert.ToString(arrFilter[b].Conjuction), 0));
							}
						}
					}
				}
			}
			if (list.Count > 0)
			{
				result = queryGenerator.filter_string(list.ToArray(), objDb, iCompanyId);
			}
		}
		return result;
	}

	private string getXmlMaster(string strParamValues)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(strParamValues))
		{
			string[] array = strParamValues.Split(',');
			stringBuilder.Append("DECLARE @XML AS XML\r\nSET @XML = '<Master>");
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.AppendFormat("<r id=\"{0}\"/>", array[i]);
			}
			stringBuilder.Append("</Master>'\r\n");
		}
		return stringBuilder.ToString();
	}

	public int GetTotalPages(int iCompanyId)
	{
		DbCommand dbCommand = null;
		string text = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		text = $"Select COUNT(*)  FROM tCore_Data{FConvert.GetSuffix(iCompanyId)}";
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		return Convert.ToInt32(database.ExecuteScalar(dbCommand));
	}

	public ComboData[] LoadMasters(string strParameter, MasterType type, int iCompanyId)
	{
		switch (type)
		{
		case MasterType.Masters:
			return Load("SELECT iMasterTypeId, sMasterName FROM cCore_MasterDef", (int)type, iCompanyId);
		case MasterType.SQLViews:
			strParameter = (string.IsNullOrEmpty(strParameter) ? "v" : strParameter);
			return Load($"select object_id, name from sys.objects where type= '{strParameter}' order by name", (int)type, iCompanyId);
		case MasterType.InvoiceDesigner:
			return Load(string.Format("SELECT iLayoutId, sLayout FROM cCore_InvoiceLayout{1} WHERE iReportId={0}", strParameter, FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId);
		case MasterType.ReportDesigner:
			return Load(string.Format("SELECT cCore_Reports{0}.iReportId, sReportName FROM cCore_ReportLayouts{0} JOIN cCore_Reports{0} ON cCore_ReportLayouts{0}.iReportId = cCore_Reports{0}.iReportId WHERE iReportType = 1 OR iReportType = 2", FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId);
		case MasterType.ReportCustomization:
			return Load(string.Format("SELECT iLayoutId, sLayoutName FROM cCore_ReportLayouts{1} JOIN cCore_Reports{1} ON cCore_ReportLayouts{1}.iReportId = cCore_Reports{1}.iReportId WHERE cCore_Reports{1}.iReportId={0}", strParameter, FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId);
		case MasterType.Reports:
			return Load(string.Format("SELECT cCore_Reports{0}.iReportId, sReportName FROM cCore_ReportLayouts{0} JOIN cCore_Reports{0} ON cCore_ReportLayouts{0}.iReportId = cCore_Reports{0}.iReportId WHERE iReportType=0", FConvert.GetSuffix(iCompanyId)), (int)type, iCompanyId);
		case MasterType.RDReports:
			return Load($"SELECT iReportId, sReportName FROM cCore_Reports{FConvert.GetSuffix(iCompanyId)} WHERE iReportType > 0 AND iReportType <> {4} AND iReportId BETWEEN 70000 AND 80000", 0, iCompanyId);
		default:
			return null;
		}
	}

	public TranMasterData[] LoadTranMasters(int[] arrMasterId, bool bMainLevelMasterOnly, int iCompanyId)
	{
		List<TranMasterData> list = new List<TranMasterData>();
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		TranMasterData tranMasterData = null;
		int num = 0;
		int num2 = 0;
		int num3 = -2;
		bool flag = false;
		IdNamePair idNamePair = null;
		string text = null;
		string empty = string.Empty;
		try
		{
			if (arrMasterId != null && arrMasterId.Length != 0)
			{
				empty = " WHERE iMasterId IN (";
				for (num = 0; num < arrMasterId.Length; num++)
				{
					empty += string.Format("{0}{1}", (num == 0) ? "" : ",", arrMasterId[num]);
				}
				empty += ")";
				if (!bMainLevelMasterOnly)
				{
					text = string.Format("SELECT DISTINCT iLinkMasterId FROM vmuCore_MasterFields {0} AND iDataTypeId=12\r\n                            UNION\r\n                            SELECT iMasterId FROM vmuCore_MasterFields {0}", empty);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					dataReader = database.ExecuteReader(dbCommand);
					empty = " WHERE iMasterId IN (";
					num = 0;
					while (dataReader.Read())
					{
						empty += string.Format("{0}{1}", (num == 0) ? "" : ",", dataReader[0]);
						num++;
					}
					empty += ((num == 0) ? "0)" : ")");
					dataReader.Close();
				}
				empty += " AND bNotAvailableForReports = 0";
			}
			else
			{
				empty = " WHERE 1=1";
			}
			text = $"SELECT iMasterId, sMasterName, iFieldId, CASE WHEN iFieldId=300055 THEN 'Supplier Code' ELSE sCaption END [sCaption], \r\n                        iDataTypeId, iLinkMasterId, sModule, sDefaultValue, sFieldName\r\n                        FROM vmuCore_MasterFields join cCore_MasterDef on vmuCore_MasterFields.iMasterId = cCore_MasterDef.iMasterTypeId\r\n                        {empty} AND sCaption Not in('MasterId','iRowIndex','iLocationId','Group','Status','Rounding Type','No of decimals',\r\n                        'Is Attribute', 'Do Not Restrict','CreatedBy','Modified By','Created Date', \r\n                        'Modified Date', 'Edited From', 'Closing Date')\r\n                        AND bInternalStdField = 0 \r\n                        AND bTableType = 0\r\n                        ORDER BY iMasterId,case when sFieldName = 'sName' or sFieldName = 'sCode' then '' else sCaption END, iFieldId";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = Convert.ToInt32(dataReader["iMasterId"]);
				tranMasterData.MasterName = Convert.ToString(dataReader["sMasterName"]);
				tranMasterData.FieldId = Convert.ToInt32(dataReader["iFieldId"]);
				tranMasterData.Caption = Convert.ToString(dataReader["sCaption"]);
				tranMasterData.FieldName = Convert.ToString(dataReader["sFieldName"]);
				tranMasterData.TypeId = Convert.ToInt32(dataReader["iDataTypeId"]);
				tranMasterData.LinkId = Convert.ToInt32(dataReader["iLinkMasterId"]);
				tranMasterData.DefaultValue = Convert.ToString(dataReader["sDefaultValue"]);
				if (num3 != tranMasterData.MasterId)
				{
					if (idNamePair != null)
					{
						if (list.Count >= num2)
						{
							list.Insert(num2, GetMasterAliasField(idNamePair.ID, idNamePair.Name, Convert.ToString(idNamePair.Tag)));
						}
						list.AddRange(GetMasterLevelFields(idNamePair.ID, idNamePair.Name, Convert.ToString(idNamePair.Tag), database, iCompanyId));
						if (num3 == 2)
						{
							list.Insert(num2, GetItemBudgetField(idNamePair.ID, idNamePair.Name, Convert.ToString(idNamePair.Tag)));
							TranMasterData[] prefMasterFields = GetPrefMasterFields(num3, idNamePair.Name, database, iCompanyId);
							if (prefMasterFields != null)
							{
								list.AddRange(prefMasterFields);
							}
							flag = true;
						}
					}
					idNamePair = new IdNamePair(tranMasterData.MasterId, tranMasterData.MasterName, Convert.ToString(dataReader["sModule"]));
					num3 = tranMasterData.MasterId;
				}
				if (tranMasterData.Caption.ToLower() == "code")
				{
					num2 = list.Count + 1;
				}
				list.Add(tranMasterData);
			}
			dataReader.Close();
			if (list.Count > 0 && idNamePair != null)
			{
				if (list.Count > num2)
				{
					list.Insert(num2, GetMasterAliasField(idNamePair.ID, idNamePair.Name, Convert.ToString(idNamePair.Tag)));
				}
				list.AddRange(GetMasterLevelFields(idNamePair.ID, idNamePair.Name, Convert.ToString(idNamePair.Tag), database, iCompanyId));
				if (!flag && arrMasterId != null && arrMasterId.Length != 0 && arrMasterId[0] == 2)
				{
					TranMasterData[] prefMasterFields2 = GetPrefMasterFields(arrMasterId[0], idNamePair.Name, database, iCompanyId);
					if (prefMasterFields2 != null)
					{
						list.AddRange(prefMasterFields2);
					}
				}
			}
		}
		catch (Exception ex)
		{
			tranMasterData = new TranMasterData();
			tranMasterData.MasterId = -1;
			string sError = (tranMasterData.MasterName = ex.Message);
			m_sError = sError;
			list.Add(tranMasterData);
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	private TranMasterData[] GetMasterLevelFields(int iMasterTypeId, string sName, string sModule, Database objDb, int iCompanyId)
	{
		int num = 0;
		DbCommand dbCommand = null;
		TranMasterData tranMasterData = null;
		List<TranMasterData> list = null;
		string text = null;
		if (num <= 0)
		{
			string empty = string.Empty;
			switch (iMasterTypeId)
			{
			case 1:
				empty = "mCore_AccountTreeDetails";
				break;
			case 2:
				empty = "mCore_ProductTreeDetails";
				break;
			default:
				text = $"select 'm'+sModule+'_'+sMasterName+'TreeDetails' from cCore_MasterDef where iMasterTypeId={iMasterTypeId}";
				dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
				empty = Convert.ToString(objDb.ExecuteScalar(dbCommand));
				break;
			}
			text = $"SELECT ISNULL(MAX(iLevel),0) FROM {empty} WHERE iTreeId = 0";
			dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			num = Convert.ToInt32(objDb.ExecuteScalar(dbCommand));
		}
		list = new List<TranMasterData>();
		if (num > 0)
		{
			if (iMasterTypeId == 1 || iMasterTypeId == 2)
			{
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 8388709;
				tranMasterData.Caption = "Group Name";
				TranMasterData tranMasterData2 = tranMasterData;
				int typeId = (tranMasterData.LinkId = 0);
				tranMasterData2.TypeId = typeId;
				list.Add(tranMasterData);
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 8388710;
				tranMasterData.Caption = "Group Code";
				TranMasterData tranMasterData3 = tranMasterData;
				typeId = (tranMasterData.LinkId = 0);
				tranMasterData3.TypeId = typeId;
				list.Add(tranMasterData);
			}
			for (int i = 0; i < num; i++)
			{
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 0x800000 | (i + 1);
				tranMasterData.Caption = $"Group Level {i + 1}";
				TranMasterData tranMasterData4 = tranMasterData;
				int typeId = (tranMasterData.LinkId = 0);
				tranMasterData4.TypeId = typeId;
				list.Add(tranMasterData);
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 0x800000 | (i + 1 + 255);
				tranMasterData.Caption = $"Group Level {i + 1} Code";
				TranMasterData tranMasterData5 = tranMasterData;
				typeId = (tranMasterData.LinkId = 0);
				tranMasterData5.TypeId = typeId;
				list.Add(tranMasterData);
			}
		}
		return list.ToArray();
	}

	private TranMasterData[] GetPrefMasterFields(int iMasterTypeId, string sName, Database objDb, int iCompId)
	{
		DbCommand dbCommand = null;
		TranMasterData tranMasterData = null;
		List<TranMasterData> list = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		int num = 0;
		int num2 = 0;
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		string empty2 = string.Empty;
		int num3 = 1;
		int num4 = 1;
		try
		{
			list = new List<TranMasterData>();
			empty = $"SELECT sValue FROM cCore_PreferenceText{FConvert.GetSuffix(iCompId)} WHERE iCategory = {Convert.ToInt32(PreferenceCategories.Masters)}";
			dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = objDb.ExecuteReader(dbCommand);
			list3.Add("Buying Rate");
			list2.Add("Selling Rate");
			while (dataReader.Read())
			{
				empty2 = Convert.ToString(dataReader["sValue"]);
				if (string.IsNullOrEmpty(empty2))
				{
					empty2 = string.Format("{0} Val {1}", (num % 2 == 1) ? "Buying" : "Selling", (num % 2 == 1) ? num3 : num4);
				}
				if (num % 2 == 1)
				{
					list3.Add(empty2);
					num3++;
				}
				else
				{
					list2.Add(empty2);
					num4++;
				}
				num++;
			}
			dataReader.Close();
			if (list3.Count == 1 && list2.Count == 1)
			{
				for (num = 0; num < 13; num++)
				{
					empty2 = string.Format("{0} Val {1}", "Buying", num3);
					list3.Add(empty2);
					num3++;
				}
				for (num = 0; num < 13; num++)
				{
					empty2 = string.Format("{0} Val {1}", "Selling", num4);
					list2.Add(empty2);
					num4++;
				}
			}
			for (num2 = 0; num2 < list3.Count; num2++)
			{
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 0x830000 | num2;
				tranMasterData.Caption = list3[num2];
				tranMasterData.TypeId = 6;
				tranMasterData.LinkId = 0;
				list.Add(tranMasterData);
			}
			for (num2 = 0; num2 < list2.Count; num2++)
			{
				tranMasterData = new TranMasterData();
				tranMasterData.MasterId = iMasterTypeId;
				tranMasterData.MasterName = sName;
				tranMasterData.FieldId = 0x840000 | num2;
				tranMasterData.Caption = list2[num2];
				tranMasterData.TypeId = 6;
				tranMasterData.LinkId = 0;
				list.Add(tranMasterData);
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	private TranMasterData GetMasterAliasField(int iMasterTypeId, string sName, string sModule)
	{
		TranMasterData tranMasterData = new TranMasterData
		{
			MasterId = iMasterTypeId,
			MasterName = sName,
			FieldId = 127,
			Caption = "Alias"
		};
		int typeId = (tranMasterData.LinkId = 0);
		tranMasterData.TypeId = typeId;
		return tranMasterData;
	}

	private TranMasterData GetItemBudgetField(int iMasterTypeId, string sName, string sModule)
	{
		TranMasterData tranMasterData = new TranMasterData
		{
			MasterId = iMasterTypeId,
			MasterName = sName,
			FieldId = 160,
			Caption = "Budget"
		};
		int typeId = (tranMasterData.LinkId = 0);
		tranMasterData.TypeId = typeId;
		return tranMasterData;
	}

	public string Delete(int iId, MasterType type, int iCompId, DbConnection objCon = null, DbTransaction objTran = null, int iModuleType = -1)
	{
		bool flag = objCon == null;
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		StringBuilder stringBuilder = null;
		string suffix = FConvert.GetSuffix(iCompId);
		string empty = string.Empty;
		string text = string.Empty;
		try
		{
			if (flag)
			{
				objCon = database.CreateConnection();
				objCon.Open();
				objTran = objCon.BeginTransaction();
			}
			dbCommand = database.DbProviderFactory.CreateCommand();
			dbCommand.Connection = objCon;
			dbCommand.CommandType = CommandType.Text;
			dbCommand.Transaction = objTran;
			stringBuilder = new StringBuilder();
			switch (type)
			{
			case MasterType.InvoiceDesigner:
				if (iModuleType > -1)
				{
					empty = string.Format("SELECT iLayoutId FROM cCore_InvoiceLayout{1} WHERE iReportId={0} and iModule = {2};", iId, suffix, iModuleType);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
					iId = Convert.ToInt32(database.ExecuteScalar(dbCommand));
				}
				empty = string.Format("SELECT iPageId FROM cCore_InvoicePage{1} WHERE iLayoutId={0};", iId, suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					text += ((text.Length == 0) ? string.Empty : ",");
					text += dataReader[0].ToString();
				}
				dataReader.Close();
				text = ((text.Length == 0) ? "0" : text);
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceNumericProperty{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceBody{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceHeader{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceAreaControl{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceBodyFont{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoicePage{1} WHERE iLayoutId = {0};", iId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceBodyValue{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportFilter{1} WHERE iType = 3 AND iFilterGroupId = {0};", iId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceAttachments{1} WHERE iLayoutId = {0};", iId, suffix));
				if (m_bDeleteLayout)
				{
					stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceLayout{1} WHERE iLayoutId = {0};", iId, suffix));
				}
				break;
			case MasterType.ReportDesigner:
			case MasterType.ReportCustomization:
				empty = string.Format("SELECT iColumnId FROM cCore_ReportColumns{1} WHERE iLayoutId={0}", iId, suffix);
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = database.ExecuteReader(dbCommand);
				empty = string.Empty;
				while (dataReader.Read())
				{
					empty += ((empty.Length > 0) ? ", " : string.Empty);
					empty += dataReader[0].ToString();
				}
				dataReader.Close();
				if (type == MasterType.ReportDesigner)
				{
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportParameter{1} WHERE iLayoutId = {0};", iId, suffix));
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportTransactionSet{1} WHERE iLayoutId = {0};", iId, suffix));
				}
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportGrouping{1} WHERE iLayoutId = {0};", iId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportColumns{1} WHERE iLayoutId = {0};", iId, suffix));
				if (empty.Length > 0)
				{
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportColumnsFunction{1} WHERE iColumnId IN ({0});", empty, suffix));
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportFormatting{1} WHERE iColumnId IN ({0}) AND bRowFormatting=0;", empty, suffix));
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportColumnsOrder{1} WHERE iColumnId IN ({0});", empty, suffix));
					stringBuilder.Append(string.Format("DELETE FROM cCore_ReportColumnsFilter{1} WHERE iColumnId IN ({0});", empty, suffix));
				}
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportFormatting{1} WHERE iColumnId = {0} AND bRowFormatting=1;", iId, suffix));
				break;
			}
			dbCommand = database.DbProviderFactory.CreateCommand();
			dbCommand.Connection = objCon;
			dbCommand.CommandType = CommandType.Text;
			dbCommand.Transaction = objTran;
			dbCommand.CommandText = (PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			dbCommand.ExecuteNonQuery();
			if (flag)
			{
				objTran.Commit();
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			objTran.Rollback();
			return ex.Message;
		}
		finally
		{
			if (flag)
			{
				if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
				{
					dbCommand.Connection.Close();
				}
				if (objCon != null && objCon.State != ConnectionState.Closed)
				{
					objCon.Close();
				}
			}
		}
		return string.Empty;
	}

	public object[] GetReportData(string strSqlQuery, int iColumnCount, int iCompanyId)
	{
		List<object> list = new List<object>();
		Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
		DbCommand dbCommand = null;
		try
		{
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(strSqlQuery) : strSqlQuery);
			IDataReader dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				object[] array = new object[iColumnCount];
				for (int i = 0; i < iColumnCount; i++)
				{
					array[i] = dataReader[i];
				}
				list.Add(array);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			list.Clear();
			list.Add(new object[2]
			{
				ERRORNO,
				(m_sError = ex.Message)
			});
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	public ComboData GetReportNameFromId(int iReportId, int iCompId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		ComboData comboData = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre($"SELECT sReportName, iModule FROM cCore_Reports{FConvert.GetSuffix(iCompId)} WHERE iReportId = {iReportId}") : $"SELECT sReportName, iModule FROM cCore_Reports{FConvert.GetSuffix(iCompId)} WHERE iReportId = {iReportId}");
			dataReader = database.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				comboData = new ComboData();
				comboData.ID = Convert.ToInt32(dataReader["iModule"]);
				comboData.Name = Convert.ToString(dataReader["sReportName"]);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			comboData = new ComboData();
			comboData.ID = ERRORNO;
			comboData.Name = (m_sError = ex.Message);
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return comboData;
	}

	private IdNamePair[] GetColumnSchemas(string strParam, string sConnectionString, int iCompanyId)
	{
		Database database = null;
		DbCommand dbCommand = null;
		List<IdNamePair> list = new List<IdNamePair>();
		string empty = string.Empty;
		try
		{
			database = (string.IsNullOrEmpty(sConnectionString) ? DatabaseWrapper.GetDatabase2(iCompanyId) : DatabaseWrapper.GetSQLDatabase(sConnectionString));
			if (strParam.ToLower().StartsWith("exec"))
			{
				return GetColumnSchemaForSP(strParam, database, iCompanyId);
			}
			empty = "if exists (select 1 from sys.objects where name = 'temp' and type = 'u')\r\n                        DROP table temp";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			database.ExecuteNonQuery(dbCommand);
			replace_inputvariables(null, null, ref strParam, iCompanyId);
			RemoveOrderBy(ref strParam);
			empty = $"SELECT * INTO temp FROM \r\n                        ( \r\n                            SELECT * FROM \r\n                            ( \r\n                                {strParam} \r\n                            )TempTable WHERE 1 = 2 \r\n                        )a;\r\n                        SELECT ORDINAL_POSITION,COLUMN_NAME,CASE WHEN DATA_TYPE = 'numeric' OR DATA_TYPE = 'decimal' THEN 6 \r\n                        WHEN DATA_TYPE = 'varchar' OR DATA_TYPE = 'nvarchar' THEN 0\r\n                        ELSE 6 END[TypeId] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'temp'";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			IDataReader dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader[0]), Convert.ToString(dataReader[1]), (MasterDataType)Convert.ToInt32(dataReader[2])));
			}
			dataReader.Close();
			empty = "DROP table temp";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			database.ExecuteNonQuery(dbCommand);
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			list.Clear();
			list.Add(new IdNamePair(ERRORNO, ex.Message, 0));
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	private IdNamePair[] GetColumnSchemaForSP(string strSPQuery, Database objDb, int iCompanyId)
	{
		int num = 1;
		IDataReader dataReader = null;
		List<IdNamePair> list = new List<IdNamePair>();
		string[] array = strSPQuery.Split(' ');
		string text = null;
		if (array.Length > 1)
		{
			text = array[1];
			string text2 = "SELECT name, system_type_name, error_type\r\n                FROM sys.dm_exec_describe_first_result_set_for_object\r\n                (\r\n                  OBJECT_ID('" + text + "'),\r\n                  NULL\r\n                );";
			dataReader = objDb.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
			while (dataReader.Read())
			{
				if (dataReader["error_type"] == DBNull.Value)
				{
					string text3 = Convert.ToString(dataReader["system_type_name"]);
					MasterDataType masterDataType = MasterDataType.Fraction;
					if (text3.Contains("varchar"))
					{
						masterDataType = MasterDataType.Text;
					}
					list.Add(new IdNamePair(num++, Convert.ToString(dataReader["name"]), masterDataType));
				}
				else if (Convert.ToInt32(dataReader["error_type"]) == 4)
				{
					break;
				}
			}
			dataReader.Close();
			if (list.Count == 0)
			{
				text = strSPQuery;
				replace_inputvariables(null, null, ref text, iCompanyId);
				text2 = text;
				using DbCommand dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
				dbCommand.CommandTimeout = 150;
				dataReader = objDb.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
				for (int i = 0; i < dataReader.FieldCount; i++)
				{
					list.Add(new IdNamePair(i + 1, dataReader.GetName(i), MasterDataType.Text));
				}
				dataReader.Close();
			}
		}
		return list.ToArray();
	}

	public IdNamePair[] LoadColumnSchemas(string strQuery, string sConnectionString, int iCompId)
	{
		return GetColumnSchemas(strQuery, sConnectionString, iCompId);
	}

	private void RemoveOrderBy(ref string strQuery)
	{
		int num = -1;
		int num2 = -1;
		int num3 = strQuery.ToLower().IndexOf("order by ");
		int num4 = strQuery.ToLower().LastIndexOf("order by ");
		if (num3 == num4 && strQuery.ToLower().Contains("order by ") && !strQuery.ToLower().Contains("row_number() over") && !strQuery.ToLower().Contains(" over (") && !strQuery.ToLower().Contains(" over("))
		{
			num2 = strQuery.Length;
			num = strQuery.ToLower().IndexOf("order by ");
			if (num > -1 && num2 - num > 0)
			{
				strQuery = strQuery.Remove(num, num2 - num);
			}
		}
	}

	private ComboData[] Load(string strQuery, int type, int iCompanyId)
	{
		List<ComboData> list = new List<ComboData>();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		ComboData comboData = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(strQuery) : strQuery);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				comboData = new ComboData();
				comboData.ID = Convert.ToInt32(dataReader[0]);
				comboData.Name = Convert.ToString(dataReader[1]);
				list.Add(comboData);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			list.Clear();
			comboData = new ComboData();
			comboData.ID = ERRORNO;
			comboData.Name = (m_sError = ex.Message);
			list.Add(comboData);
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	private string DeleteRD(int iLayoutId, MasterType type, Database objDb, int iCompId)
	{
		string suffix = FConvert.GetSuffix(iCompId);
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		StringBuilder stringBuilder = null;
		string empty = string.Empty;
		string text = string.Empty;
		try
		{
			dbCommand = objDb.DbProviderFactory.CreateCommand();
			dbCommand.CommandType = CommandType.Text;
			stringBuilder = new StringBuilder();
			switch (type)
			{
			case MasterType.InvoiceDesigner:
				empty = string.Format("SELECT iPageId FROM cCore_InvoicePage{1} WHERE iLayoutId={0};", iLayoutId, suffix);
				dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
				dataReader = objDb.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					text += ((text.Length == 0) ? string.Empty : ",");
					text += dataReader[0].ToString();
				}
				dataReader.Close();
				text = ((text.Length == 0) ? "0" : text);
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceBody{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceHeader{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceNumericProperty{1} WHERE iPageId IN ({0});", text, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_InvoicePage{1} WHERE iLayoutId = {0};", iLayoutId, suffix));
				if (m_bDeleteLayout)
				{
					stringBuilder.Append(string.Format("DELETE FROM cCore_InvoiceLayout{1} WHERE iLayoutId = {0};", iLayoutId, suffix));
				}
				break;
			case MasterType.ReportDesigner:
			case MasterType.ReportCustomization:
				stringBuilder.Append(string.Format("\r\n                            DELETE FROM cCore_ReportColumnsFunction{1} WHERE iColumnId IN(SELECT iColumnId FROM cCore_ReportColumns{1} WHERE iLayoutId = {0});\r\n                            DELETE FROM cCore_ReportFilter{1} WHERE iFilterGroupId in(select iFilterGroupId FROM cCore_ReportColumnsFilter{1} \r\n                            WHERE iColumnId IN(SELECT iColumnId FROM cCore_ReportColumns{1} WHERE iLayoutId = {0}));", iLayoutId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportFilter{1} WHERE iFilterGroupId IN(SELECT iColumnId FROM cCore_ReportColumns{1} WHERE iLayoutId = {0});", iLayoutId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportColumns{1} WHERE iLayoutId = {0};", iLayoutId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportUserSecurity{1} WHERE iLayoutId = {0};", iLayoutId, suffix));
				stringBuilder.Append(string.Format("DELETE FROM cCore_ReportFormatting{1} WHERE iLayoutId = {0};", iLayoutId, suffix));
				break;
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return $"0{ex.Message}";
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return stringBuilder.ToString();
	}

	public LayoutInformation[] ExportBillPrinting(Module objModule, int iCompId)
	{
		string empty = string.Empty;
		empty = "SELECT iLayoutId FROM cCore_InvoiceLayout" + FConvert.GetSuffix(iCompId);
		empty += $" WHERE iModule={objModule};";
		return ExportLayouts(empty, iCompId);
	}

	public LayoutInformation[] ExportRD(int iVoucherType, int iCompId)
	{
		string empty = string.Empty;
		empty = ((iVoucherType != -1) ? string.Format("SELECT iLayoutId FROM cCore_InvoiceLayout{1} WHERE iReportId={0};", iVoucherType, FConvert.GetSuffix(iCompId)) : ("SELECT iLayoutId FROM cCore_InvoiceLayout" + FConvert.GetSuffix(iCompId)));
		return ExportLayouts(empty, iCompId);
	}

	public string ImportRD(LayoutInformation[] arrLayouts, int iCompId)
	{
		string text = string.Empty;
		if (arrLayouts != null)
		{
			for (int i = 0; i < arrLayouts.Length; i++)
			{
				text += Save(arrLayouts[i], iCompId);
			}
		}
		return text;
	}

	private LayoutInformation[] ExportLayouts(string strQuery, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		List<LayoutInformation> list = new List<LayoutInformation>();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(strQuery) : strQuery);
		dataReader = database.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			int iLayoutId = Convert.ToInt32(dataReader[0]);
			list.Add(Load(iLayoutId, iCompId));
		}
		dataReader.Close();
		return list.ToArray();
	}

	public IdNamePair[] GetDataStatistics(int iUserId, int iCompId)
	{
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		IdNamePair idNamePair = null;
		string text = null;
		List<IdNamePair> list = null;
		list = new List<IdNamePair>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompId);
			text = string.Format("SELECT cCore_Vouchers{0}.iVoucherType, sName, COUNT(tCore_Header{0}.iHeaderId)[Total] FROM tCore_Header{0} \r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = tCore_Header{0}.iVoucherType\r\n                    GROUP BY cCore_Vouchers{0}.iVoucherType,sName\r\n                    ORDER BY COUNT(tCore_Header{0}.iHeaderId) DESC", FConvert.GetSuffix(iCompId));
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iVoucherType"]);
				idNamePair.Name = Convert.ToString(dataReader["sName"]);
				idNamePair.Tag = dataReader["Total"];
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			idNamePair = new IdNamePair();
			idNamePair.ID = -1;
			idNamePair.Name = (m_sError = ex.Message);
			idNamePair.Tag = -1;
			list.Clear();
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	public DataStatsData[] GetDataStatistics(DataStatsInput objInput, int iCompId)
	{
		Database database = null;
		IDataReader dataReader = null;
		DataStatsData dataStatsData = null;
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		string text7 = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		List<DataStatsData> list = null;
		List<DataStatsData> list2 = null;
		list = new List<DataStatsData>();
		try
		{
			database = DatabaseWrapper.GetDatabase2(iCompId);
			if (objInput.GroupByTag > 2)
			{
				text = $"SELECT 'm'+sModule+'_'+sMasterName  FROM cCore_MasterDef  WHERE iMasterTypeId={objInput.GroupByTag}";
				text3 = Convert.ToString(database.ExecuteScalar(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text));
			}
			text6 = "ORDER BY COUNT(tCore_Header" + suffix + ".iHeaderId) DESC";
			text5 = ", COUNT(DISTINCT tCore_Header" + suffix + ".iHeaderId)[Total]";
			if (!string.IsNullOrEmpty(text3))
			{
				text2 = "JOIN tCore_Data" + suffix + " ON tCore_Data" + suffix + ".iHeaderId = tCore_Header" + suffix + ".iHeaderId AND tCore_Data" + suffix + ".iType = 0";
				text4 = "," + text3 + ".sName," + text3 + ".iMasterId";
				text5 = ", COUNT(CASE WHEN iSerialNo = 0 THEN 1 ELSE NULL END)[Total]";
				switch (objInput.GroupByType)
				{
				case DataStatGroupBy.GroupByDepartment:
					text2 = text2 + " JOIN " + text3 + " WITH (READUNCOMMITTED) ON " + text3 + ".iMasterId = tCore_Data" + suffix + ".iFaTag";
					text6 = "ORDER BY cCore_Vouchers" + suffix + ".sName, " + text3 + ".sName, COUNT(tCore_Header" + suffix + ".iHeaderId) DESC ";
					break;
				case DataStatGroupBy.DepartmentGroupByVoucher:
					text2 = text2 + " JOIN " + text3 + " WITH (READUNCOMMITTED) ON " + text3 + ".iMasterId = tCore_Data" + suffix + ".iFaTag";
					text6 = "ORDER BY " + text3 + ".sName, cCore_Vouchers" + suffix + ".sName, COUNT(tCore_Header" + suffix + ".iHeaderId) DESC ";
					break;
				case DataStatGroupBy.GroupByWarehouse:
					text2 = text2 + " JOIN " + text3 + " WITH (READUNCOMMITTED) ON " + text3 + ".iMasterId = tCore_Data" + suffix + ".iInvTag";
					text6 = "ORDER BY cCore_Vouchers" + suffix + ".sName, " + text3 + ".sName, COUNT(tCore_Header" + suffix + ".iHeaderId) DESC ";
					break;
				case DataStatGroupBy.WarehouseGroupByVoucher:
					text2 = text2 + " JOIN " + text3 + " WITH (READUNCOMMITTED) ON " + text3 + ".iMasterId = tCore_Data" + suffix + ".iInvTag";
					text6 = "ORDER BY " + text3 + ".sName, cCore_Vouchers" + suffix + ".sName, COUNT(tCore_Header" + suffix + ".iHeaderId) DESC ";
					break;
				}
			}
			text7 = " WHERE tCore_Header" + suffix + ".bVersion = 0";
			if (objInput.TagFilters != null && objInput.TagFilters.Length != 0)
			{
				for (num = 0; num < objInput.TagFilters.Length; num++)
				{
					if (objInput.TagFilters[num].ID == 1)
					{
						text7 += $" AND tCore_Data{suffix}.iFaTag = {objInput.TagFilters[num].Value}";
					}
					else if (objInput.TagFilters[num].ID == 2)
					{
						text7 += $" AND tCore_Data{suffix}.iInvTag = {objInput.TagFilters[num].Value}";
					}
				}
				if (objInput.GroupByType == DataStatGroupBy.Default)
				{
					text2 = "JOIN tCore_Data" + suffix + " ON tCore_Data" + suffix + ".iHeaderId = tCore_Header" + suffix + ".iHeaderId";
				}
			}
			text = $"SELECT cCore_Vouchers{suffix}.iVoucherType, ISNULL(cCore_VouchersLanguage{suffix}.sVoucherName, cCore_Vouchers{suffix}.sName)[VoucherName]\r\n                    ,cCore_Vouchers{suffix}.bInventory {text5} {text4}\r\n                    FROM tCore_Header{suffix}\r\n                    JOIN cCore_Vouchers{suffix} WITH (READUNCOMMITTED) ON cCore_Vouchers{suffix}.iVoucherType = tCore_Header{suffix}.iVoucherType \r\n                    LEFT JOIN cCore_VouchersLanguage{suffix} WITH (READUNCOMMITTED) ON cCore_Vouchers{suffix}.iVoucherType = cCore_VouchersLanguage{suffix}.iVoucherId  AND iLanguageId = {objInput.LanguageId}\r\n                    {text2} {text7}\r\n                    GROUP BY cCore_Vouchers{suffix}.iVoucherType,cCore_Vouchers{suffix}.sName,ISNULL(cCore_VouchersLanguage{suffix}.sVoucherName, cCore_Vouchers{suffix}.sName)\r\n                    ,cCore_Vouchers{suffix}.bInventory {text4} {text6} ";
			dataReader = database.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			while (dataReader.Read())
			{
				dataStatsData = new DataStatsData();
				dataStatsData.VoucherType = Convert.ToInt32(dataReader["iVoucherType"]);
				dataStatsData.VoucherName = Convert.ToString(dataReader["VoucherName"]);
				if (Convert.ToBoolean(dataReader["bInventory"]))
				{
					dataStatsData.VoucherName += "*";
				}
				dataStatsData.TotalCount = Convert.ToInt32(dataReader["Total"]);
				if (!string.IsNullOrEmpty(text4))
				{
					dataStatsData.TagName = Convert.ToString(dataReader["sName"]);
					dataStatsData.TagId = Convert.ToInt32(dataReader["iMasterId"]);
				}
				list.Add(dataStatsData);
			}
			dataReader.Close();
			if (!string.IsNullOrEmpty(text4))
			{
				list2 = new List<DataStatsData>();
				text4 = null;
				num2 = (num4 = (num3 = 0));
				bool flag = false;
				bool flag2 = objInput.GroupByType == DataStatGroupBy.DepartmentGroupByVoucher || objInput.GroupByType == DataStatGroupBy.WarehouseGroupByVoucher;
				for (num = list.Count - 1; num >= 0; num--)
				{
					flag = num2 > 0 && num2 != list[num].VoucherType;
					if (flag2)
					{
						flag = num3 > 0 && num3 != list[num].TagId;
					}
					if (flag)
					{
						dataStatsData = new DataStatsData();
						dataStatsData.VoucherType = num2;
						dataStatsData.VoucherName = (flag2 ? text3 : text4);
						dataStatsData.TotalCount = num4;
						dataStatsData.TagGroup = GetReportRowHeader(list[num].VoucherType, IsGroup: true);
						list2.Add(dataStatsData);
						num4 = 0;
						text4 = list[num].VoucherName;
					}
					text3 = list[num].TagName;
					text4 = list[num].VoucherName;
					if (!string.IsNullOrEmpty(list[num].TagName) | flag2)
					{
						if (flag2)
						{
							list[num].TagName = list[num].VoucherName;
							list[num].TagGroup = GetReportRowHeader(list[num].VoucherType, IsGroup: false);
							list2.Add(list[num]);
						}
						else if (!string.IsNullOrEmpty(list[num].TagName))
						{
							list[num].TagGroup = GetReportRowHeader(list[num].VoucherType, IsGroup: false);
							list2.Add(list[num]);
						}
					}
					num4 += list[num].TotalCount;
					num2 = list[num].VoucherType;
					num3 = list[num].TagId;
				}
				if (num2 > 0 && num4 > 0)
				{
					dataStatsData = new DataStatsData();
					dataStatsData.VoucherType = num2;
					dataStatsData.VoucherName = (flag2 ? text3 : text4);
					dataStatsData.TotalCount = num4;
					dataStatsData.TagGroup = GetReportRowHeader(num2, IsGroup: true);
					list2.Add(dataStatsData);
				}
				list2.Reverse();
			}
		}
		catch (Exception ex)
		{
			dataStatsData = new DataStatsData();
			dataStatsData.VoucherType = -1;
			dataStatsData.VoucherName = (m_sError = ex.Message);
			list.Clear();
			list.Add(dataStatsData);
		}
		if (list2 != null)
		{
			return list2.ToArray();
		}
		return list.ToArray();
	}

	private ReportRowHeader GetReportRowHeader(int iId, bool IsGroup)
	{
		ReportRowHeader reportRowHeader = new ReportRowHeader();
		reportRowHeader.Id = iId;
		reportRowHeader.TreeData = new ReportTreeInfo();
		reportRowHeader.TreeData.IsGroup = IsGroup;
		reportRowHeader.TreeData.GroupLevel = ((!IsGroup) ? 1 : 0);
		reportRowHeader.RowType = (IsGroup ? ReportRowType.TreeGroup : ReportRowType.Normal);
		FontClass defaultFont = FConvert.GetDefaultFont();
		defaultFont.FontWeight = 1;
		reportRowHeader.Font = defaultFont;
		return reportRowHeader;
	}

	public IdNamePair[] GetReportList(int iUserId, int iCompId)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		List<IdNamePair> list = new List<IdNamePair>();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		try
		{
			empty = $"SELECT iReportId,sReportName, iModule FROM cCore_Reports{FConvert.GetSuffix(iCompId)} WHERE iReportType <> {(byte)0} AND iModule <> {0} ORDER BY iModule";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair
				{
					ID = Convert.ToInt32(dataReader["iReportId"]),
					Name = Convert.ToString(dataReader["sReportName"]),
					Tag = (Module)Convert.ToInt32(dataReader["iModule"])
				});
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			list.Add(new IdNamePair
			{
				ID = -1,
				Name = ex.Message,
				Tag = -1
			});
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	public IdNamePair[] GetMasterList(int MasterTypeId, int iCompId, bool bSequence = false)
	{
		Database database = DatabaseWrapper.GetDatabase2(iCompId);
		List<IdNamePair> list = new List<IdNamePair>();
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		string empty = string.Empty;
		string text = null;
		string sMaster = null;
		try
		{
			text = GetMasterTable(database, MasterTypeId, ref sMaster);
			if (string.IsNullOrEmpty(text))
			{
				text = "mPos_Outlet";
			}
			empty = ((!bSequence) ? $"SELECT iMasterId,sName,sCode FROM {text} WHERE iMasterId>0 and iStatus<>5 ORDER BY iMasterId" : ("SELECT iMasterId, sName, sCode, iSeq FROM dbo.fCore_Get" + sMaster + "TreeSequence(0, 0) WHERE bGroup = 0  ORDER BY iSeq"));
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				list.Add(new IdNamePair
				{
					ID = Convert.ToInt32(dataReader["iMasterId"]),
					Name = Convert.ToString(dataReader["sName"]),
					Tag = dataReader["sCode"]
				});
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			list.Add(new IdNamePair
			{
				ID = -1,
				Name = ex.Message,
				Tag = -1
			});
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return list.ToArray();
	}

	private string GetMasterTable(Database objDb, int iMasterTypeId, ref string sMaster)
	{
		IDataReader dataReader = null;
		string result = string.Empty;
		string empty = string.Empty;
		try
		{
			empty = $"SELECT sTableName, sMasterName FROM cCore_MasterTables \r\n                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                WHERE bMainTable = 1 and iMasterTypeId={iMasterTypeId} ";
			dataReader = objDb.ExecuteReader(CommandType.Text, PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			if (dataReader.Read())
			{
				result = Convert.ToString(dataReader["sTableName"]);
				sMaster = Convert.ToString(dataReader["sMasterName"]);
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return result;
	}

	public string DeleteVoucherInvoiceLayout(int iLayoutId, int iCompId)
	{
		Database database = null;
		database = DatabaseWrapper.GetDatabase2(iCompId);
		return DeleteVoucherInvoiceLayout(iLayoutId, IsDeleteLayout: true, database, null, iCompId);
	}

	private string DeleteVoucherInvoiceLayout(int iLayoutId, bool IsDeleteLayout, Database objDb, DbTransaction objTran, int iCompId)
	{
		DbCommand dbCommand = null;
		string empty = string.Empty;
		empty = string.Format("SELECT COUNT(1) FROM cCore_DocumentSetBody{0} base \r\n                JOIN cCore_InvoiceLayout{0} ON base.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n            WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}", FConvert.GetSuffix(iCompId), iLayoutId);
		dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
		if (Convert.ToInt32(objDb.ExecuteScalar(dbCommand)) > 0)
		{
			return "Layout used in document set.\r\nFirst delete the document set.";
		}
		try
		{
			empty = string.Format("DELETE base FROM cCore_InvoiceAreaControl{0} base\r\n                    JOIN cCore_InvoicePage{0} ON cCore_InvoicePage{0}.iPageId = base.iPageId\r\n                    JOIN cCore_InvoiceLayout{0} ON cCore_InvoicePage{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}\r\n\r\n                    DELETE base FROM cCore_InvoiceNumericProperty{0} base  \r\n                    JOIN cCore_InvoicePage{0} ON cCore_InvoicePage{0}.iPageId = base .iPageId\r\n                    JOIN cCore_InvoiceLayout{0} ON cCore_InvoicePage{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}\r\n\r\n                    DELETE base FROM cCore_InvoiceBody{0} base  \r\n                    JOIN cCore_InvoicePage{0} ON cCore_InvoicePage{0}.iPageId = base .iPageId\r\n                    JOIN cCore_InvoiceLayout{0} ON cCore_InvoicePage{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}\r\n\r\n                    DELETE base FROM cCore_InvoiceHeader{0} base  \r\n                    JOIN cCore_InvoicePage{0} ON cCore_InvoicePage{0}.iPageId = base .iPageId\r\n                    JOIN cCore_InvoiceLayout{0} ON cCore_InvoicePage{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}\r\n\r\n                    DELETE base FROM cCore_InvoiceBodyFont{0} base  \r\n                    JOIN cCore_InvoicePage{0} ON cCore_InvoicePage{0}.iPageId = base .iPageId\r\n                    JOIN cCore_InvoiceLayout{0} ON cCore_InvoicePage{0}.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}\r\n\r\n                    DELETE base FROM cCore_InvoicePage{0} base \r\n                    JOIN cCore_InvoiceLayout{0} ON base.iLayoutId = cCore_InvoiceLayout{0}.iLayoutId\r\n                    WHERE cCore_InvoiceLayout{0}.iLayoutId = {1};\r\n                    ", FConvert.GetSuffix(iCompId), iLayoutId);
			if (IsDeleteLayout)
			{
				empty += string.Format("DELETE FROM cCore_InvoiceLayout{0} \r\n                        WHERE cCore_InvoiceLayout{0}.iLayoutId = {1}", FConvert.GetSuffix(iCompId), iLayoutId);
			}
			dbCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dbCommand.CommandTimeout = 0;
			if (objTran != null)
			{
				objDb.ExecuteNonQuery(dbCommand, objTran);
			}
			else
			{
				objDb.ExecuteNonQuery(dbCommand);
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return ex.Message;
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State != ConnectionState.Closed)
			{
				dbCommand.Connection.Close();
			}
		}
		return string.Empty;
	}

	public int SaveFilterCustomizeFields(FilterCustomizationFields CustomizeFields, int CompId)
	{
		Database database = null;
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		try
		{
			database = DatabaseWrapper.GetDatabase2(CompId);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("SELECT COUNT(1) FROM cCore_FilterCustomization Where iFilterId={0} AND iSubFilterId={1};", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			bool flag = Convert.ToInt32(database.ExecuteScalar(dbCommand)) == 0;
			stringBuilder = new StringBuilder();
			stringBuilder.Append("DECLARE @iHeaderFilterId int;");
			if (flag)
			{
				stringBuilder.AppendFormat("INSERT INTO cCore_FilterCustomization(iFilterId,iSubFilterId) values({0},{1}); SELECT @iHeaderFilterId= @@IDENTITY;", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			}
			else
			{
				stringBuilder.AppendFormat("SELECT @iHeaderFilterId=iId FROM cCore_FilterCustomization WHERE iFilterId={0} AND iSubFilterId={1};", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			}
			stringBuilder.AppendFormat("DELETE FROM cCore_FilterCustomizationBody\r\n                                            WHERE iFilterId=(SELECT iId FROM cCore_FilterCustomization Header\r\n                                                                    WHERE Header.iFilterId={0} AND Header.iSubFilterId={1});", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			if (CustomizeFields.FieldIdSubParentId != null && CustomizeFields.FieldIdSubParentId.Length != 0)
			{
				IdValuePair[] fieldIdSubParentId = CustomizeFields.FieldIdSubParentId;
				for (int i = 0; i < fieldIdSubParentId.Length; i++)
				{
					stringBuilder.AppendFormat("INSERT INTO cCore_FilterCustomizationBody(iFilterId,iFieldId,iSubParentId) \r\n                    VALUES(@iHeaderFilterId,{0},{1});", fieldIdSubParentId[i].ID, fieldIdSubParentId[i].Value);
				}
			}
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			database.ExecuteNonQuery(dbCommand);
			return 1;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return 0;
	}

	public int[] GetFilterCustomizeFields(int iFilterId, int iSubFilterId, int CompanyId)
	{
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		IDataReader dataReader = null;
		List<int> list = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(CompanyId);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Select Body.iFieldId from cCore_FilterCustomizationBody Body inner join cCore_FilterCustomization Header on Body.iFilterId=Header.iId where Header.iFilterId={0} and Header.iSubFilterId={1}", iFilterId, iSubFilterId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<int>();
			while (dataReader.Read())
			{
				list.Add(Convert.ToInt32(dataReader[0]));
			}
			if (!dataReader.IsClosed)
			{
				dataReader.Close();
			}
			return (list.Count > 0) ? list.ToArray() : null;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return null;
	}

	public IdValuePair[] GetFilterCustomizeFieldsSubParentIds(int iFilterId, int iSubFilterId, int CompanyId)
	{
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		IDataReader dataReader = null;
		List<IdValuePair> list = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(CompanyId);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Select Body.iFieldId, Body.iSubParentId from cCore_FilterCustomizationBody Body inner join cCore_FilterCustomization Header on Body.iFilterId=Header.iId where Header.iFilterId={0} and Header.iSubFilterId={1}", iFilterId, iSubFilterId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<IdValuePair>();
			while (dataReader.Read())
			{
				list.Add(new IdValuePair(Convert.ToInt32(dataReader[0]), Convert.ToInt32(dataReader[1])));
			}
			if (!dataReader.IsClosed)
			{
				dataReader.Close();
			}
			return (list.Count > 0) ? list.ToArray() : null;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return null;
	}

	private void replace_inputvariables(_Parameter[] arrParams, RepRecord objRec, ref string strQuery, int iCompId)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text = null;
		string empty = string.Empty;
		string text2 = "@iStartDate";
		string text3 = "@iEndDate";
		string text4 = "@iUserId";
		string text5 = "''";
		char value = '@';
		if (strQuery.Length <= 0)
		{
			return;
		}
		if (strQuery.Contains(text2))
		{
			if (objRec != null)
			{
				strQuery = strQuery.Replace(text2, objRec.StartingDate.ToString());
			}
			else
			{
				strQuery = strQuery.Replace(text2, "0");
			}
		}
		if (strQuery.Contains(text3))
		{
			if (objRec != null && objRec.EndingDate > 0)
			{
				strQuery = strQuery.Replace(text3, objRec.EndingDate.ToString());
			}
			else
			{
				strQuery = strQuery.Replace(text3, "0x3FFFFFFF");
			}
		}
		if (strQuery.Contains(text4))
		{
			if (objRec != null && objRec.UserId > 0)
			{
				strQuery = strQuery.Replace(text4, objRec.UserId.ToString());
			}
			else
			{
				strQuery = strQuery.Replace(text4, text5);
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
						empty = string.Join(",", objRec.Masters.Select((ComboData p) => p.ID.ToString()).ToArray());
						num3 = 1;
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
					num3 = Convert.ToInt32(FConvert.GetInputValue(objRec.Inputs, arrParams[num].FieldId));
					empty = num3.ToString();
				}
				if (empty.Length > 0 && num3 > 0)
				{
					strQuery = strQuery.Replace(arrParams[num].FieldVariable, empty);
				}
				else
				{
					strQuery = strQuery.Replace(arrParams[num].FieldVariable, $"{text5} OR 1=1");
				}
			}
			return;
		}
		int num4 = 0;
		while (strQuery.Contains(value) && num4 <= 255)
		{
			num2 = strQuery.IndexOf(value);
			if (num2 > -1)
			{
				for (num = num2; num < strQuery.Length && strQuery[num] != ' ' && strQuery[num] != '\r' && strQuery[num] != '\n' && strQuery[num] != ')' && strQuery[num] != '<' && strQuery[num] != '>' && strQuery[num] != '!' && strQuery[num] != '=' && num < strQuery.Length; num++)
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
						strQuery = strQuery.Replace(text, $"{text5} OR 1=1");
					}
					else
					{
						strQuery = strQuery.Replace(text, text5);
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

	public int SaveFilterCustomizeFieldsWithNewColumns(FilterCustomizationFields CustomizeFields, int CompId)
	{
		Database database = null;
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		try
		{
			database = DatabaseWrapper.GetDatabase2(CompId);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("SELECT COUNT(1) FROM cCore_FilterCustomization Where iFilterId={0} AND iSubFilterId={1};", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			bool flag = Convert.ToInt32(database.ExecuteScalar(dbCommand)) == 0;
			stringBuilder = new StringBuilder();
			stringBuilder.Append("DECLARE @iHeaderFilterId int;");
			if (flag)
			{
				stringBuilder.AppendFormat("INSERT INTO cCore_FilterCustomization(iFilterId,iSubFilterId) values({0},{1}); SELECT @iHeaderFilterId= @@IDENTITY;", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			}
			else
			{
				stringBuilder.AppendFormat("SELECT @iHeaderFilterId=iId FROM cCore_FilterCustomization WHERE iFilterId={0} AND iSubFilterId={1};", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			}
			stringBuilder.AppendFormat("DELETE FROM cCore_FilterCustomizationBody\r\n                                            WHERE iFilterId=(SELECT iId FROM cCore_FilterCustomization Header\r\n                                                                    WHERE Header.iFilterId={0} AND Header.iSubFilterId={1});", CustomizeFields.FilterId, CustomizeFields.SubFilterId);
			RDCustomization_TreeData[] rDFields = CustomizeFields.RDFields;
			for (int i = 0; i < rDFields.Length; i++)
			{
				stringBuilder.AppendFormat("INSERT INTO cCore_FilterCustomizationBody(iFilterId,iFieldId,iSubParentId,\r\niSequenceId,sName,bGroup,iLevel,iParentId,iDataTypeId,iMasterLink,iExtraValue,sExtraValue,TableName,PrimaryField,DisplayField,sExtraFieldName) \r\n                    VALUES(@iHeaderFilterId,{0},{1},{2},'{3}',{4},{5},{6},{7},{8},{9},'{10}','{11}','{12}','{13}','{14}');", rDFields[i].iFieldId, rDFields[i].iSubParentId, rDFields[i].iSequenceId, rDFields[i].sName, rDFields[i].bGroup ? 1 : 0, rDFields[i].iLevel, rDFields[i].iParentId, rDFields[i].iDataTypeId, rDFields[i].iMasterLink, rDFields[i].iExtraValue, rDFields[i].sExtraValue, rDFields[i].TableName, rDFields[i].PrimaryField, rDFields[i].DisplayField, rDFields[i].sExtraFieldName);
			}
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			database.ExecuteNonQuery(dbCommand);
			return 1;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return 0;
	}

	public RDCustomization_TreeData[] GetFilterCustomizeFieldsSubParentIdsWithRDColumns(int iFilterId, int iSubFilterId, int CompanyId)
	{
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		IDataReader dataReader = null;
		List<RDCustomization_TreeData> list = null;
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(CompanyId);
			stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Select Body.iFieldId, Body.iSubParentId,iSequenceId,sName,bGroup,iLevel,iParentId,iDataTypeId,iMasterLink,iExtraValue,sExtraValue,TableName,PrimaryField,DisplayField,sExtraFieldName from cCore_FilterCustomizationBody Body inner join cCore_FilterCustomization Header on Body.iFilterId=Header.iId where Header.iFilterId={0} and Header.iSubFilterId={1}", iFilterId, iSubFilterId);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<RDCustomization_TreeData>();
			while (dataReader.Read())
			{
				list.Add(new RDCustomization_TreeData
				{
					iFieldId = (int)dataReader["iFieldId"],
					iSubParentId = (int)dataReader["iSubParentId"],
					iSequenceId = Convert.ToInt32((dataReader["iSequenceId"] == DBNull.Value) ? ((object)0) : dataReader["iSequenceId"]),
					sName = Convert.ToString(dataReader["sName"]),
					bGroup = Convert.ToBoolean((dataReader["iLevel"] == DBNull.Value) ? ((object)false) : dataReader["bGroup"]),
					iLevel = Convert.ToInt32((dataReader["iLevel"] == DBNull.Value) ? ((object)0) : dataReader["iLevel"]),
					iParentId = Convert.ToInt32((dataReader["iParentId"] == DBNull.Value) ? ((object)0) : dataReader["iParentId"]),
					iDataTypeId = Convert.ToInt32((dataReader["iDataTypeId"] == DBNull.Value) ? ((object)0) : dataReader["iDataTypeId"]),
					iMasterLink = Convert.ToInt32((dataReader["iMasterLink"] == DBNull.Value) ? ((object)0) : dataReader["iMasterLink"]),
					iExtraValue = Convert.ToInt32((dataReader["iExtraValue"] == DBNull.Value) ? ((object)0) : dataReader["iExtraValue"]),
					sExtraValue = Convert.ToString(dataReader["sExtraValue"]),
					TableName = Convert.ToString(dataReader["TableName"]),
					PrimaryField = Convert.ToString(dataReader["PrimaryField"]),
					DisplayField = Convert.ToString(dataReader["DisplayField"]),
					sExtraFieldName = Convert.ToString(dataReader["sExtraFieldName"])
				});
			}
			if (!dataReader.IsClosed)
			{
				dataReader.Close();
			}
			return (list.Count > 0) ? list.ToArray() : null;
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return null;
	}

	public IdNamePair[] GetStandardRates(int iCompanyId)
	{
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = new List<IdNamePair>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = string.Format("SELECT DISTINCT mCore_StdRate{0}.iProductId, sName, iDate[Date], mRate[Rate] \r\n                FROM mCore_StdRate{0} \r\n                JOIN mCore_Product ON mCore_Product.iMasterId =  mCore_StdRate{0}.iProductId \r\n                WHERE iProductId > 0\r\n                ORDER BY sName, iDate", suffix);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<IdNamePair>();
			while (dataReader.Read())
			{
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iProductId"]), Convert.ToString(dataReader["sName"]), Convert.ToString(dataReader["Date"]), dataReader["Rate"]));
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public IdNamePair[] GetAllWorkflows(int iUserId, int iCompanyId)
	{
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		string text2 = null;
		int num = 0;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = new List<IdNamePair>();
		List<ComboData> list2 = new List<ComboData>();
		try
		{
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = string.Format("SELECT mCore_WorkFlow{0}.iWorkFlowId, mCore_WorkFlow{0}.sName\r\n                , vmCore_Links{0}.LinkName , vmCore_Links{0}.iLinkPathId\r\n                FROM mCore_WorkFlow{0}\r\n                JOIN vmCore_Links{0} ON mCore_WorkFlow{0}.iWorkFlowId = vmCore_Links{0}.iWorkFlowId \r\n                ORDER by 1", suffix);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<IdNamePair>();
			while (dataReader.Read())
			{
				if (!string.IsNullOrEmpty(text2) && text2 != Convert.ToString(dataReader["sName"]))
				{
					list.Add(new IdNamePair(num, text2, list2.ToArray()));
					list2.Clear();
				}
				num = Convert.ToInt32(dataReader["iWorkFlowId"]);
				text2 = Convert.ToString(dataReader["sName"]);
				list2.Add(new ComboData(Convert.ToInt32(dataReader["iLinkPathId"]), Convert.ToString(dataReader["LinkName"])));
			}
			dataReader.Close();
			if (!string.IsNullOrEmpty(text2) && num > 0)
			{
				list.Add(new IdNamePair(num, text2, list2.ToArray()));
				list2.Clear();
			}
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public IdNamePair[] GetAllLockUnlock(int iUserId, int iCompanyId)
	{
		string suffix = FConvert.GetSuffix(iCompanyId);
		string text = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		List<IdNamePair> list = new List<IdNamePair>();
		List<string> list2 = new List<string>();
		try
		{
			if (m_objCalType == CalendarType.None)
			{
				m_objCalType = CalendarType.Gregorean;
			}
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			text = string.Format("SELECT mCore_LockDatabase{0}.iVoucherType, sName + ' ['+sAbbr + ']' [sName], iLockFlag, iReadDate, iWriteDate, iPrintDate  \r\n                FROM mCore_LockDatabase{0} \r\n                JOIN cCore_Vouchers{0} ON mCore_LockDatabase{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n                ORDER BY 1", suffix);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = database.ExecuteReader(dbCommand);
			list = new List<IdNamePair>();
			while (dataReader.Read())
			{
				list2.Clear();
				int num = Convert.ToInt32(dataReader["iLockFlag"]);
				if ((num & 1) > 0)
				{
					list2.Add(string.Format("Read Lock : {0}", new Date(m_objCalType).IntToDate(Convert.ToInt32(dataReader["iReadDate"])).ToShortDateString()));
				}
				if ((num & 2) > 0)
				{
					list2.Add(string.Format("Write Lock : {0}", new Date(m_objCalType).IntToDate(Convert.ToInt32(dataReader["iWriteDate"])).ToShortDateString()));
				}
				if ((num & 4) > 0)
				{
					list2.Add(string.Format("Print Lock : {0}", new Date(m_objCalType).IntToDate(Convert.ToInt32(dataReader["iPrintDate"])).ToShortDateString()));
				}
				list.Add(new IdNamePair(Convert.ToInt32(dataReader["iVoucherType"]), Convert.ToString(dataReader["sName"]), list2.ToArray()));
			}
			dataReader.Close();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
		}
		return list.ToArray();
	}

	public string[] GetExtraFieldNames(FieldData[] arrFieldId, int iVoucherType, bool bVoucherClass, int iComp, bool bCallingFromFilter = false)
	{
		uint num = 0u;
		uint num2 = 0u;
		_ = string.Empty;
		string[] array = null;
		List<string> list = new List<string>();
		if (arrFieldId != null)
		{
			for (num = 0u; num < arrFieldId.Length; num++)
			{
				array = GetExtraFieldName(arrFieldId[num].FieldId, arrFieldId[num].SubParentId, iVoucherType, bVoucherClass, iComp, bCallingFromFilter, arrFieldId[num].ParentId, arrFieldId[num].DataType).Split('\n');
				for (num2 = 0u; num2 < array.Length; num2++)
				{
					if (array[num2].Length > 0 && !string.IsNullOrEmpty(array[num2]))
					{
						if (array[num2].StartsWith("vCore_BodyScreenData") && ((!string.IsNullOrEmpty(arrFieldId[num].FieldName) && arrFieldId[num].FieldName.EndsWith("_Input")) || (!string.IsNullOrEmpty(arrFieldId[num].ColumnAlias) && arrFieldId[num].ColumnAlias.EndsWith("_Input")) || bCallingFromFilter))
						{
							string[] array2 = array[num2].Split('.');
							if (array2.Length >= 2)
							{
								array2[1] = array2[1].Replace("[", "");
								array2[1] = array2[1].Replace("]", "");
								array2[1] = string.Format("[{0}{1}]", array2[1], "_Input");
							}
							array[num2] = string.Join(".", array2);
						}
						list.Add(array[num2]);
						break;
					}
					list.Add("''");
				}
			}
		}
		return list.ToArray();
	}

	public string GetExtraFieldName(int FieldId, int iSubParentId, int iVoucherType, bool bVoucherClass, int iComp, bool bCallingFromFilter = false, int iParentId = 0, MasterDataType iDataType = MasterDataType.Text)
	{
		Database database = null;
		DbCommand dbCommand = null;
		IDataReader dataReader = null;
		int iSubFieldId = 0;
		string text = ((!string.IsNullOrEmpty(m_strSuffix)) ? m_strSuffix : FConvert.GetSuffix(iComp));
		string text2 = string.Empty;
		string text3 = string.Empty;
		string text4 = null;
		try
		{
			if (m_arrVoucherTypesValue != null && m_arrVoucherTypesValue.Count > 1)
			{
				string text5 = string.Join(",", from p in m_arrVoucherTypesValue
					where Convert.ToBoolean(p.Value)
					select p.ID.ToString());
				string text6 = string.Join(",", from p in m_arrVoucherTypesValue
					where !Convert.ToBoolean(p.Value)
					select p.ID.ToString());
				if (!string.IsNullOrEmpty(text5) && !string.IsNullOrEmpty(text6))
				{
					text4 += $" AND ((iVoucherType & 0xFF00) IN ({text5}) OR iVoucherType IN ({text6}))";
				}
				else
				{
					if (!string.IsNullOrEmpty(text5))
					{
						text4 += $" AND (iVoucherType & 0xFF00) IN ({text5})";
					}
					if (!string.IsNullOrEmpty(text6))
					{
						text4 += $" AND iVoucherType IN ({text6})";
					}
				}
			}
			else if (iVoucherType > 0)
			{
				if (bVoucherClass)
				{
					text4 = ((iVoucherType != 8704) ? $" AND (iVoucherType & 0xFF00) = {iVoucherType}" : $" AND (iVoucherType & 0xFF00) IN ({8704},{3584})");
				}
				else
				{
					text4 = $" AND iVoucherType = {iVoucherType}";
				}
			}
			database = DatabaseWrapper.GetDatabase2(iComp, -1, bReplicationServer: true);
			if (FieldId != -2)
			{
				iSubFieldId = RDCommon.GetSubFieldId(ref FieldId);
			}
			if (iDataType == MasterDataType.DocumentViewer || iDataType == MasterDataType.Picture)
			{
				text2 = (string.IsNullOrEmpty(text4) ? string.Format("SELECT CASE WHEN bHeader = 1 THEN 'vCore_TranHeaderDocs' + '{0}' + '.'+sFieldName ELSE 'vCore_TranDataDocs' + '{0}' + '.'+sFieldName END\r\n                                FROM cCore_VoucherFields{0} WHERE iUniqueId={1}", text, FieldId & 0xFFFFFF) : string.Format("SELECT CASE WHEN bHeader = 1 THEN 'vCore_TranHeaderDocs' + '{0}' + '.'+sFieldName ELSE 'vCore_TranDataDocs' + '{0}' + '.'+sFieldName END\r\n                                FROM cCore_VoucherFields{0} WHERE (iUniqueId={1} OR iFieldId={1}) {2}", text, FieldId & 0xFFFFFF, text4));
				text2 += " GROUP BY iVoucherType,sFieldName,bHeader ORDER BY COUNT(iVoucherType) DESC";
				text3 = string.Empty;
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
				dataReader = database.ExecuteReader(dbCommand);
				if (dataReader.Read())
				{
					text3 = dataReader.GetString(0);
				}
				dataReader.Close();
				if (!string.IsNullOrEmpty(text3))
				{
					return text3;
				}
			}
			if (FieldId < 5000)
			{
				switch (FieldId)
				{
				case 127:
					if (iVoucherType == -2)
					{
						iSubFieldId = iSubParentId;
						text2 = $"SELECT 'm'+sModule+'_'+sMasterName+'Language.sName' \r\n                            FROM cCore_MasterDef WHERE iMasterTypeId = {iSubFieldId}";
					}
					else
					{
						iSubFieldId = GetMasterTypeIdFromField(iSubParentId, database);
						text2 = $"SELECT top 1 'm'+sModule+'_'+sMasterName+'Language.sName' \r\n                            FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                            JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                            JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                            WHERE cCore_MasterFields.iTableId = {iSubFieldId}";
					}
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
					dataReader = database.ExecuteReader(dbCommand);
					if (dataReader.Read())
					{
						text3 = dataReader.GetString(0);
					}
					dataReader.Close();
					if (string.IsNullOrEmpty(text3))
					{
						text2 = $"SELECT top 1 'm'+sModule+'_'+sMasterName+'Language.sName' \r\n                            FROM cCore_MasterDef WHERE iMasterTypeId = {iSubFieldId}";
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
						dataReader = database.ExecuteReader(dbCommand);
						if (dataReader.Read())
						{
							text3 = dataReader.GetString(0);
						}
						dataReader.Close();
					}
					if (string.IsNullOrEmpty(text3))
					{
						text2 = string.Format("SELECT top 1 'm'+sModule+'_'+sMasterName+'Language.sName' FROM cCore_VoucherFields{0}\r\n                            JOIN cCore_MasterDef ON cCore_VoucherFields{0}.iMasterLink = cCore_MasterDef.iMasterTypeId WHERE iUniqueId = {1}", text, iSubFieldId);
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
						dataReader = database.ExecuteReader(dbCommand);
						if (dataReader.Read())
						{
							text3 = dataReader.GetString(0);
						}
						dataReader.Close();
					}
					break;
				case 160:
					return "vrCore_Product.sCode";
				default:
					text3 = GetTableName(FieldId, iSubFieldId, iComp);
					break;
				}
			}
			else if (FieldId > 5000 || (FieldId & 0x10000000) > 0 || (FieldId & 0x20000000) > 0)
			{
				if (iSubParentId <= 0)
				{
					iSubParentId = FieldId;
				}
				if ((iSubParentId & 0x1000000) > 0 || (iSubParentId & 0x4000000) > 0)
				{
					if (!bCallingFromFilter)
					{
						text2 = $"SELECT 'vr' + sModule + '_' +sMasterName FROM cCore_VoucherFields{text} \r\n                        JOIN cCore_MasterDef ON cCore_VoucherFields{text}.iMasterLink = cCore_MasterDef.iMasterTypeId\r\n                        WHERE iUniqueId = {iSubParentId & 0xFFFFFF}";
						if (!string.IsNullOrEmpty(text4))
						{
							text2 += text4;
						}
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
						dataReader = database.ExecuteReader(dbCommand);
						if (dataReader.Read())
						{
							text3 = dataReader.GetString(0);
						}
						dataReader.Close();
						if (!string.IsNullOrEmpty(text3))
						{
							text2 = $"SELECT sFieldName FROM cCore_MasterFields WHERE iFieldId = {FieldId & 0xFFFFFF}";
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
							dataReader = database.ExecuteReader(dbCommand);
							if (dataReader.Read())
							{
								text3 = text3 + "." + dataReader.GetString(0);
							}
							dataReader.Close();
						}
					}
					if (text3.Split('.').Length <= 1)
					{
						text2 = (string.IsNullOrEmpty(text4) ? string.Format("SELECT CASE WHEN bHeader = 1 THEN 'vCore_TranHeaderData' + '{0}'+'.'+sFieldName ELSE 'vCore_TranData' + '{0}'+'.'+sFieldName END\r\n                                FROM cCore_VoucherFields{0} WHERE iUniqueId={1}", text, iSubParentId & 0xFFFFFF) : string.Format("SELECT CASE WHEN bHeader = 1 THEN 'vCore_TranHeaderData' + '{0}'+'.'+sFieldName ELSE 'vCore_TranData' + '{0}'+'.'+sFieldName END\r\n                                FROM cCore_VoucherFields{0} WHERE (iUniqueId={1} OR iFieldId={1}) {2}", text, iSubParentId & 0xFFFFFF, text4));
						text2 += " GROUP BY sFieldName,bHeader ORDER BY COUNT(iVoucherType) DESC";
						text3 = string.Empty;
						dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
						dataReader = database.ExecuteReader(dbCommand);
						while (dataReader.Read())
						{
							if (!string.IsNullOrEmpty(text3))
							{
								string[] array = text3.Split('.');
								text3 = ((array.Length < 2) ? $"vCore_TranAllData{text}.{dataReader.GetString(0).Split('.')[1]}" : $"vCore_TranAllData{text}.{array[1]}");
							}
							else
							{
								text3 += dataReader.GetString(0);
							}
						}
						dataReader.Close();
					}
				}
				if (string.IsNullOrEmpty(text3))
				{
					if ((FieldId & 0xFF0000) >> 16 == 128)
					{
						text3 = GetMasterLevelField(iSubParentId, FieldId & 0xFFFF, database);
					}
					else if ((FieldId & 0xFF0000) >> 16 == 131 || (FieldId & 0xFF0000) >> 16 == 132)
					{
						text3 = GetProductLevelField((FieldId & 0xFF0000) >> 16, FieldId & 0xFFFF);
					}
					else
					{
						if (iVoucherType != -2 && (iSubParentId == 3 || iSubParentId == 12))
						{
							text2 = $"SELECT 'Code.'+cCore_MasterFields.sFieldName \r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {FieldId & 0xFFFFFF}";
						}
						else if (iVoucherType != -2 && (iSubParentId == 4 || iSubParentId == 39))
						{
							text2 = $"SELECT 'BookNo.'+cCore_MasterFields.sFieldName \r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {FieldId & 0xFFFFFF}";
						}
						else if (iSubParentId == 84)
						{
							text2 = $"SELECT 'vrCore_Bins2.'+cCore_MasterFields.sFieldName \r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {FieldId & 0xFFFFFF}";
						}
						else if (iSubParentId > 5000 || (iSubParentId & 0x10000000) > 0 || (iSubParentId & 0x20000000) > 0)
						{
							text2 = $"SELECT 'vr'+sModule+'_'+sMasterName+'.'+sFieldName \r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {iSubParentId & 0xFFFFFF}";
						}
						if (!string.IsNullOrEmpty(text2))
						{
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
							dataReader = database.ExecuteReader(dbCommand);
							if (dataReader.Read())
							{
								text3 = dataReader.GetString(0);
							}
							dataReader.Close();
						}
						bool flag = false;
						if (text3.Contains("BookNo.") || text3.Contains("Code."))
						{
							flag = true;
						}
						if (!flag && (string.IsNullOrEmpty(text3) || FieldId > 5000 || (FieldId & 0x10000000) > 0 || (FieldId & 0x20000000) > 0))
						{
							text2 = string.Format("SELECT 'vr'+sModule+'_'+sMasterName+'.'+sFieldName+'{1}'\r\n                                FROM cCore_MasterFields JOIN cCore_MasterTables ON cCore_MasterTables.iTableId = cCore_MasterFields.iTableId\r\n                                JOIN cCore_MasterTabs ON cCore_MasterTabs.iTabId = cCore_MasterTables.iTabId\r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterTabs.iMasterId\r\n                                WHERE iFieldId = {0}", FieldId & 0xFFFFFF, (iDataType == MasterDataType.DocumentViewer) ? "Name" : "");
							dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
							dataReader = database.ExecuteReader(dbCommand);
							if (dataReader.Read())
							{
								string text7 = dataReader.GetString(0);
								if (!string.IsNullOrEmpty(text3))
								{
									if (!text7.Contains(".sName"))
									{
										text3 = ((!(text3 != text7) || !text7.Contains(".sCode")) ? text7 : $"{text3}sCode");
									}
									else if (iParentId > 0 && text3.Contains("vrCore_Account"))
									{
										switch (iParentId)
										{
										case 4:
										case 39:
											text3 = text3.Replace("vrCore_Account", "BookNo");
											break;
										case 3:
										case 12:
											text3 = text3.Replace("vrCore_Account", "Code");
											break;
										}
									}
								}
								else
								{
									text3 = text7;
								}
							}
							dataReader.Close();
						}
					}
				}
			}
			if (string.IsNullOrEmpty(text3))
			{
				text2 = string.Format("SELECT distinct case when bFooter = 0 then 'vCore_BodyScreenData' else 'vCore_FooterData' end + '{0}' +'.['+sCaption+']' \r\n                            FROM cCore_VoucherScreenFields{0} a join cCore_Fields b on a.iUniqueId = b.iFieldId WHERE iUniqueId={1}", text, FieldId & 0xFFFFFF);
				if (!string.IsNullOrEmpty(text4))
				{
					text2 += text4;
				}
				dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text2) : text2);
				dataReader = database.ExecuteReader(dbCommand);
				while (dataReader.Read())
				{
					text3 += dataReader.GetString(0);
					text3 += "\n";
				}
				dataReader.Close();
			}
		}
		catch (Exception ex)
		{
			text3 = "!." + ex.Message;
		}
		return text3;
	}

	private string GetMasterLevelField(int iSubParentId, int iLevel, Database objDb)
	{
		int num = 1;
		byte b = 0;
		if (iLevel > 255)
		{
			iLevel -= 255;
			b = 1;
		}
		else if (iLevel == 101 || iLevel == 102)
		{
			b = ((iLevel != 101) ? ((byte)1) : ((byte)0));
			if (iSubParentId == 23 || iSubParentId == 2)
			{
				return string.Format("fCore_GetProductImmidiateParent(0).Parent{0}", (b == 1) ? "Code" : "Name");
			}
			return string.Format("fCore_GetAccountImmidiateParent(0).Parent{0}", (b == 1) ? "Code" : "Name");
		}
		switch (iSubParentId)
		{
		case 3:
			return $"dbo.fCore_GetLevelMaster{num}(Code.iMasterId, {iLevel - ((iLevel > 0) ? 1 : 0)}, {b})";
		case 4:
			return $"dbo.fCore_GetLevelMaster{num}(BookNo.iMasterId, {iLevel - ((iLevel > 0) ? 1 : 0)}, {b})";
		case 23:
			if (m_bCallingFromExternal)
			{
				return string.Format("dbo.fCore_GetLevelMaster2(vrCore_Product.iMasterId, {1}, {2})", num, iLevel - ((iLevel > 0) ? 1 : 0), b);
			}
			return string.Format("ItemLevelMaster{1}.L{0}", iLevel - ((iLevel > 0) ? 1 : 0), (b > 0) ? "Code" : "");
		default:
		{
			num = GetMasterTypeIdFromField(iSubParentId, objDb);
			if (num == 2)
			{
				if (m_bCallingFromExternal)
				{
					return string.Format("dbo.fCore_GetLevelMaster2(vrCore_Product.iMasterId, {1}, {2})", num, iLevel - ((iLevel > 0) ? 1 : 0), b);
				}
				return string.Format("ItemLevelMaster{1}.L{0}", iLevel - ((iLevel > 0) ? 1 : 0), (b > 0) ? "Code" : "");
			}
			string text = $"SELECT 'vr'+sModule+'_'+sMasterName\r\n                                FROM cCore_MasterDef WHERE iMasterTypeId = {iSubParentId & 0xFFFFFF}";
			DbCommand sqlStringCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			text = Convert.ToString(objDb.ExecuteScalar(sqlStringCommand));
			if (num > 0 && !string.IsNullOrEmpty(text))
			{
				return string.Format("dbo.fCore_GetLevelMaster{0}({2}.iMasterId, {1}, {3})", num, iLevel - ((iLevel > 0) ? 1 : 0), text, b);
			}
			text = $"SELECT cCore_MasterDef.iMasterTypeId, 'vr'+sModule+'_'+sMasterName\r\n                                FROM cCore_MasterFields \r\n                                JOIN cCore_MasterDef ON cCore_MasterDef.iMasterTypeId = cCore_MasterFields.iLinkMasterId\r\n                                WHERE iFieldId = {iSubParentId & 0xFFFFFF}";
			sqlStringCommand = objDb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			IDataReader dataReader = objDb.ExecuteReader(sqlStringCommand);
			if (dataReader.Read())
			{
				num = dataReader.GetInt32(0);
				text = dataReader.GetString(1);
			}
			dataReader.Close();
			if (num > 0 && !string.IsNullOrEmpty(text))
			{
				return string.Format("dbo.fCore_GetLevelMaster{0}({2}.iMasterId, {1}, {3})", num, iLevel - ((iLevel > 0) ? 1 : 0), text, b);
			}
			return string.Empty;
		}
		}
	}

	private string GetProductLevelField(int FieldId, int iLevel)
	{
		return "vrCore_Product.iMasterId";
	}

	public int GetMasterTypeIdFromField(int iFieldId, Database objDB)
	{
		switch ((VTFIELDID)iFieldId)
		{
		case VTFIELDID.FDF_CODE:
		case VTFIELDID.FDF_BOOKNO:
			return 1;
		case VTFIELDID.FDF_BODYPRODUCT:
			return 2;
		case VTFIELDID.FDF_BODYUNITS:
			return 11;
		default:
			return iFieldId & 0xFFFFFF;
		}
	}

	private string GetTableName(int FieldId, int iSubFieldId, int iComp)
	{
		string result = string.Empty;
		string text = ((!string.IsNullOrEmpty(m_strSuffix)) ? m_strSuffix : FConvert.GetSuffix(iComp));
		string[] array = Convert.ToString(arrFields[GetIndex(FieldId)].Tag).Split('.');
		if (array != null && array.Length > 1)
		{
			if (FieldId < 100 || FieldId > 107)
			{
				switch (FieldId)
				{
				case 174:
					return $"CASE WHEN bChequeReturn = 1 THEN 'Return' ELSE '' END";
				case 57:
					return string.Format("CASE WHEN tCore_Indta{0}.iQCStatus = 0 THEN 'N.A' WHEN tCore_Indta{0}.iQCStatus = 1 THEN 'Inspection' WHEN tCore_Indta{0}.iQCStatus = 2 THEN 'Failed ' ELSE 'Passed' END", text);
				case 10:
				case 14:
					return (CurrencyPart)iSubFieldId switch
					{
						CurrencyPart.CurrencyCoin => $"{array[0]}.sCoinsName", 
						CurrencyPart.CurrencyName => $"{array[0]}.sName", 
						CurrencyPart.CurrencyNoOfDecimal => $"{array[0]}.iNoOfDecimals", 
						CurrencyPart.CurrencyRoundingType => $"{array[0]}.iRoundingType", 
						CurrencyPart.CurrencyRoundOff => $"{array[0]}.fGeneralRoundOff", 
						CurrencyPart.CurrencySymbol => $"{array[0]}.sSymbol", 
						_ => $"{array[0]}.sCode", 
					};
				default:
					array[0] = array[0] + text;
					break;
				case 73:
				case 78:
				case 167:
				case 170:
				case 194:
					break;
				}
			}
			result = $"{array[0]}.{array[1]}";
		}
		return result;
	}

	private int GetIndex(int Id)
	{
		int num = 0;
		for (num = 0; num < arrFields.Length; num++)
		{
			if (arrFields[num].ID == Id)
			{
				return num;
			}
		}
		return 0;
	}

	public DocumentField[] GetDocFieldsOfTransactionSetForPayroll(Module oModule, int iTranSetType, int iVoucherType, int iLanguageId, int iCompId)
	{
		try
		{
			return new PayReportDS().GetDocFieldsOfTransactionSetForPayroll(oModule, iTranSetType, iVoucherType, iCompId);
		}
		catch (Exception ex)
		{
			return new DocumentField[1]
			{
				new DocumentField
				{
					iFieldId = -1,
					sFieldName = ex.Message
				}
			};
		}
	}

	public IdNamePair[] GetTransactionSetForPayroll()
	{
		try
		{
			return new PayReportDS().GetTransactionSetForPayroll();
		}
		catch (Exception ex)
		{
			return new IdNamePair[1]
			{
				new IdNamePair(-1, ex.Message)
			};
		}
	}

	public DocumentField[] GetDocFieldsOfTransactionSet(Module oModule, int iTranSetType, int iVoucherType, int iLanguageId, int iCompId)
	{
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		DocumentField documentField = null;
		List<DocumentField> list = null;
		string text = null;
		switch (oModule)
		{
		case Module.Payroll:
			return GetDocFieldsOfTransactionSetForPayroll(oModule, iTranSetType, iVoucherType, iLanguageId, iCompId);
		case Module.Production:
			if (iVoucherType == 650)
			{
				return GetDocFieldsForProduction(2, iCompId);
			}
			break;
		}
		if (oModule == Module.Quality && iVoucherType == 684)
		{
			return GetDocFieldsForProduction(12, iCompId);
		}
		switch ((TransactionSetType)iTranSetType)
		{
		case TransactionSetType.AccountingTransactions:
		case TransactionSetType.AccountingTransactionsOfanAccount:
		case TransactionSetType.AccountingTransactionsOfAccountingTag:
		case TransactionSetType.AccountingTransactionsOfInventoryTag:
		case TransactionSetType.AccountingTransactionsOfaTag:
		case TransactionSetType.AccountingTransactionsOfSelectedAccounts:
		case TransactionSetType.AllAccountingTransactions:
		case TransactionSetType.BillReference:
			return GetDocFields(TranSetType.Accounting, iCompId, iLanguageId);
		case TransactionSetType.InventoryTransactions:
		case TransactionSetType.InventoryTransactionsOfaProduct:
		case TransactionSetType.InventoryTransactionsOfAccountingTag:
		case TransactionSetType.InventoryTransactionsOfInventoryTag:
		case TransactionSetType.InventoryTransactionsOfaTag:
		case TransactionSetType.InventoryTransactionsOfSelectedProducts:
			return GetDocFields(TranSetType.Inventory, iCompId, iLanguageId);
		case TransactionSetType.PendingLinks:
			return GetDocFields(TranSetType.PendingLink, iCompId, iLanguageId);
		case TransactionSetType.AllTransactionsOfDocumentClass:
		case TransactionSetType.AllTransactionsOfDocumentType:
			return GetDocFields((iTranSetType == 13) ? TranSetType.VoucherClass : TranSetType.Voucher, iCompId, iLanguageId, iVoucherType);
		case TransactionSetType.AllAccounts:
		case TransactionSetType.AllAccountsTagwise:
		case TransactionSetType.AllAccountsWithOpeningBalance:
			return GetDocFields(TranSetType.Account, iCompId, iLanguageId);
		case TransactionSetType.AllProducts:
		case TransactionSetType.AllProductsTagwise:
		case TransactionSetType.AllProductsWithOpeningStock:
			return GetDocFields(TranSetType.Product, iCompId, iVoucherType);
		case TransactionSetType.AllFixedAssets:
			return GetDocFields(TranSetType.FixedAssets, iCompId, iVoucherType);
		default:
			list = new List<DocumentField>();
			if (text != null)
			{
				try
				{
					Database database = DatabaseWrapper.GetDatabase2(iCompId, -1, bReplicationServer: true);
					dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
					dataReader = database.ExecuteReader(dbCommand);
					while (dataReader.Read())
					{
						documentField = new DocumentField();
						documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
						documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
						documentField.iDataTypeId = Convert.ToByte(dataReader["iDataTypeId"]);
						documentField.iMasterLink = Convert.ToInt32(dataReader["iMasterLink"]);
						list.Add(documentField);
					}
					dataReader.Close();
				}
				catch (Exception ex)
				{
					documentField = new DocumentField();
					documentField.iFieldId = -1;
					documentField.sFieldName = ex.Message;
					list.Add(documentField);
				}
				finally
				{
					if (dbCommand.Connection.State != ConnectionState.Closed)
					{
						dbCommand.Connection.Close();
					}
				}
			}
			return list.ToArray();
		}
	}

	private DocumentField[] GetDocFields(TranSetType objType, int iCompId, int iLanguageId, int iVoucherType = -1)
	{
		List<DocumentField> list = null;
		DocumentField[] array = null;
		VoucherTypeData voucherTypeData = null;
		PreferenceSettingData preferenceSettingData = null;
		VoucherSettingData voucherSettingData = null;
		IdNamePair[] array2 = null;
		Database database = null;
		database = DatabaseWrapper.GetDatabase2(iCompId, -1, bReplicationServer: true);
		list = new List<DocumentField>();
		voucherTypeData = GetVoucherTypeData(iVoucherType);
		preferenceSettingData = GetPreferenceSettingData(database, iCompId);
		voucherSettingData = GetVoucherSettingData(iVoucherType, database, iCompId);
		array2 = GetMasterTagsInVoucher(iVoucherType, objType, database, iCompId);
		array = GetExtraFields(iVoucherType, objType, iLanguageId, database, iCompId);
		if (objType != TranSetType.FixedAssets)
		{
			list.AddRange(GetHeaderFields(voucherTypeData, preferenceSettingData, voucherSettingData));
		}
		if (objType == TranSetType.Accounting || objType == TranSetType.Voucher || objType == TranSetType.VoucherClass)
		{
			list.AddRange(GetReferenceFields());
		}
		list.AddRange(GetMasterTags(array2, bHeader: true));
		if (array != null && array != null)
		{
			list.AddRange(array.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.LAY_HDR)) == Convert.ToInt32(VTFIELDFLAG.LAY_HDR)).ToArray());
		}
		if (objType != TranSetType.FixedAssets)
		{
			list.AddRange(GetBodyFields(voucherTypeData, preferenceSettingData, voucherSettingData, objType));
			list.AddRange(GetAIFAFields(database, iCompId));
		}
		list.AddRange(GetMasterTags(array2, bHeader: false));
		if (array != null && array != null)
		{
			list.AddRange(array.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.LAY_BODY)) == Convert.ToInt32(VTFIELDFLAG.LAY_BODY)).ToArray());
			list.AddRange(array.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.SCR_BODY)) == Convert.ToInt32(VTFIELDFLAG.SCR_BODY)).ToArray());
		}
		if (array != null && array != null)
		{
			list.AddRange(array.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.SCR_FTR)) == Convert.ToInt32(VTFIELDFLAG.SCR_FTR)).ToArray());
		}
		return list.ToArray();
	}

	private PreferenceSettingData GetPreferenceSettingData(Database objDB, int iCompId)
	{
		return new PreferenceSettingData
		{
			iInvTag = _focus.company(iCompId).getPreferenceValue(PreferenceCategories.Tag, 1),
			bInputUnitsPref = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.Inventory, 9) > 0),
			bAlternateUnit = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.Inventory, 10) > 0),
			bRMASupport = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.RMA, 0) > 0),
			bRMAB4Qty = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.RMA, 4) > 0),
			bEnableBatches = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.InventoryBatchNumber, 0) > 0),
			bBatchExpiry = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.InventoryBatchNumber, 8) > 0),
			bBatchMfg = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.InventoryBatchNumber, 16) > 0),
			bEnableBins = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.Bins, 13) > 0),
			bIssRptProcessWise = (_focus.company(iCompId).getPreferenceValue(PreferenceCategories.MRP, 14) > 0)
		};
	}

	private VoucherTypeData GetVoucherTypeData(int iVoucherType)
	{
		VoucherTypeData voucherTypeData = null;
		if (iVoucherType > 0)
		{
			voucherTypeData = new VoucherTypeData();
			voucherTypeData.iMaskedVoucher = ((iVoucherType == 1022) ? ((short)iVoucherType) : ((short)(iVoucherType & Convert.ToUInt16(VTTYPE.VOUCHERMASK))));
			voucherTypeData.bPurchaseTypeVoucher = voucherTypeData.iMaskedVoucher == 768 || voucherTypeData.iMaskedVoucher == 6400 || voucherTypeData.iMaskedVoucher == 2304 || voucherTypeData.iMaskedVoucher == 8192 || voucherTypeData.iMaskedVoucher == 2560 || voucherTypeData.iMaskedVoucher == 1280;
			voucherTypeData.bSalesTypeVoucher = FConvert.IsItSales(iVoucherType);
			voucherTypeData.bCashBankVoucher = voucherTypeData.iMaskedVoucher == 4608 || voucherTypeData.iMaskedVoucher == 4864 || voucherTypeData.iMaskedVoucher == 5120 || voucherTypeData.iMaskedVoucher == 7168 || voucherTypeData.iMaskedVoucher == 5888;
			voucherTypeData.bJournalVoucher = voucherTypeData.iMaskedVoucher == 3584 || voucherTypeData.iMaskedVoucher == 8448 || voucherTypeData.iMaskedVoucher == 3840 || voucherTypeData.iMaskedVoucher == 4096 || voucherTypeData.iMaskedVoucher == 256 || voucherTypeData.iMaskedVoucher == 4352 || voucherTypeData.iMaskedVoucher == 8704;
			voucherTypeData.bStockTypeVoucher = voucherTypeData.iMaskedVoucher == 2048 || voucherTypeData.iMaskedVoucher == 5376 || voucherTypeData.iMaskedVoucher == 512 || voucherTypeData.iMaskedVoucher == 7936 || voucherTypeData.iMaskedVoucher == 2816 || FConvert.IsItTransfer(voucherTypeData.iMaskedVoucher);
			voucherTypeData.bSalPurInvRet = voucherTypeData.iMaskedVoucher == 3328 || voucherTypeData.iMaskedVoucher == 1792 || voucherTypeData.iMaskedVoucher == 768 || voucherTypeData.iMaskedVoucher == 6400;
			voucherTypeData.bOutward = voucherTypeData.iMaskedVoucher == 3328 || voucherTypeData.iMaskedVoucher == 6400 || voucherTypeData.iMaskedVoucher == 6144 || voucherTypeData.iMaskedVoucher == 5376 || voucherTypeData.iMaskedVoucher == 3072;
		}
		return voucherTypeData;
	}

	private VoucherSettingData GetVoucherSettingData(int iVoucherType, Database objDB, int iCompId)
	{
		VoucherSettingData voucherSettingData = null;
		string text = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		ulong num = 0uL;
		ulong num2 = 0uL;
		if (iVoucherType > 0)
		{
			voucherSettingData = new VoucherSettingData();
			text = string.Format("SELECT \r\n                bInventory, " + FDataLayer.SQL_ISNULL("iDueDate", "0") + "iDueDate, " + FDataLayer.SQL_ISNULL("bUpdateFA", "0") + "bUpdateFA, " + FDataLayer.SQL_ISNULL("bUpdateInv", "0") + "bUpdateInv," + FDataLayer.SQL_ISNULL("bAccInLine", "0") + "bAccInLine, " + FDataLayer.SQL_ISNULL("iCurrencyPos", "0") + "iCurrencyPos, " + FDataLayer.SQL_ISNULL("bInputExchangeRate", "0") + "bInputExchangeRate," + FDataLayer.SQL_ISNULL("iReserveStock", "0") + "iReserveStock, " + FDataLayer.SQL_ISNULL("iBatchPick", "0") + "iBatchPick, " + FDataLayer.SQL_ISNULL("bUpdateInvCanBeChanged", "0") + "bUpdateInvCanBeChanged, " + FDataLayer.SQL_ISNULL("iInvFlag", "0") + "iInvFlag, " + FDataLayer.SQL_ISNULL("iMiscFlag", "0") + "iMiscFlag, " + FDataLayer.SQL_ISNULL("iARAPBillAdj", "0") + "iARAPBillAdj," + FDataLayer.SQL_ISNULL("iARAPOptions", "0") + "iARAPOptions," + FDataLayer.SQL_ISNULL("iProdRateOpt", "0") + "iProdRateOpt, " + FDataLayer.SQL_ISNULL("iReserveStock", "0") + "iReserveStock, " + FDataLayer.SQL_ISNULL("bEnableQC", "0") + "bEnableQC  \r\n                   FROM cCore_Vouchers{0} WHERE iVoucherType = {1}", FConvert.GetSuffix(iCompId), iVoucherType);
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				num = Convert.ToUInt64(dataReader["iInvFlag"]);
				num2 = Convert.ToUInt64(dataReader["iMiscFlag"]);
				voucherSettingData.iBatch = Convert.ToByte(dataReader["iBatchPick"]);
				voucherSettingData.iReserveStock = Convert.ToByte(dataReader["iReserveStock"]);
				voucherSettingData.iCurrencyPos = Convert.ToByte(dataReader["iCurrencyPos"]);
				voucherSettingData.iDueDate = Convert.ToByte(dataReader["iDueDate"]);
				voucherSettingData.bInventory = Convert.ToBoolean(dataReader["bInventory"]);
				voucherSettingData.bUpdateFA = Convert.ToBoolean(dataReader["bUpdateFA"]);
				voucherSettingData.bAccountInLine = Convert.ToBoolean(dataReader["bAccInLine"]);
				voucherSettingData.bInputExchangeRate = Convert.ToBoolean(dataReader["bInputExchangeRate"]);
				voucherSettingData.bUpdateInvCanBeChanged = Convert.ToBoolean(dataReader["bUpdateInvCanBeChanged"]);
				voucherSettingData.bAccInLine = Convert.ToBoolean(dataReader["bAccInLine"]);
				voucherSettingData.bUpdateInv = Convert.ToBoolean(dataReader["bUpdateInv"]);
				voucherSettingData.iARAPMode = Convert.ToInt32(dataReader["iARAPBillAdj"]);
				voucherSettingData.iProdRateOpt = Convert.ToByte(dataReader["iProdRateOpt"]);
				voucherSettingData.bEnableQC = Convert.ToBoolean(dataReader["bEnableQC"]);
			}
			dataReader.Close();
			voucherSettingData.bDontInputProduct = (num & 0x80) != 0;
			voucherSettingData.bDontInputQtyRate = (num & 0x100) != 0;
			voucherSettingData.bHideRateAndGross = (num & 0x400) != 0;
			voucherSettingData.bRateGrossHiddenForNormalUsers = (num & 0x800) != 0;
			voucherSettingData.bInputBins = (num & 0x4000) != 0;
			voucherSettingData.bNoBinPopup = (num & 0x200) != 0;
			voucherSettingData.bBatchB4Qty = (num & 4) != 0;
			voucherSettingData.bDontReadBatchNumbers = (voucherSettingData.iBatch & 2) != 0;
			voucherSettingData.bDeptAppropriation = (num2 & 8) != 0;
			voucherSettingData.bPostDiscount = (num2 & 0x10000) != 0;
			voucherSettingData.bBillB4Qty = (num2 & 0x8000) != 0;
			voucherSettingData.bDontRestrictAcc1Type = (num2 & 0x200) != 0;
			voucherSettingData.bDontRestrictAcc2Type = (num2 & 0x400) != 0;
			voucherSettingData.bDontInputDate = (num2 & 0x40) != 0;
			voucherSettingData.bInputPmtTerms = (num2 & 0x20000) != 0;
			voucherSettingData.bVersion = (num2 & 0x40000) != 0;
			voucherSettingData.bInputBatchEvenIfStockIsNotUpdated = (num & 0x8000000) != 0;
		}
		return voucherSettingData;
	}

	private DocumentField[] GetHeaderFields(VoucherTypeData objVoucherTypeData, PreferenceSettingData objPreferenceSettingData, VoucherSettingData objVoucherSettingData)
	{
		int iFieldId = 0;
		string sFieldName = null;
		bool flag = false;
		List<DocumentField> list = new List<DocumentField>();
		list.Add(GetFieldObject(1, "DocNo", MasterDataType.Text, -1));
		list.Add(GetFieldObject(187, "DocNo transaction", MasterDataType.Text, -1));
		list.Add(GetFieldObject(175, "DocNo with abbr", MasterDataType.Text, -1));
		list.Add(GetFieldObject(2, "Date", MasterDataType.Date, -1));
		list.Add(GetFieldObject(79, "Created date", MasterDataType.Date, -1));
		list.Add(GetFieldObject(80, "Created time", MasterDataType.Time, -1));
		list.Add(GetFieldObject(73, "Created user", MasterDataType.Text, -1));
		list.Add(GetFieldObject(114, "Voucher type", MasterDataType.ExternalTable, -1));
		list.Add(GetFieldObject(201, "Voucher class", MasterDataType.ExternalTable, -1));
		list.Add(GetFieldObject(157, "Voucher name", MasterDataType.Text, -1));
		list.Add(GetFieldObject(158, "Voucher abbreviation", MasterDataType.Text, -1));
		list.Add(GetFieldObject(172, "Voucher alias", MasterDataType.Text, -1));
		if (!m_bLyteVersion)
		{
			list.Add(GetFieldObject(81, "Modified date", MasterDataType.Date, -1));
			list.Add(GetFieldObject(82, "Modified time", MasterDataType.Time, -1));
			list.Add(GetFieldObject(78, "Modified user", MasterDataType.Text, -1));
			list.Add(GetFieldObject(153, "Authorize by", MasterDataType.Text, -1));
			list.Add(GetFieldObject(95, "Authorize status", MasterDataType.Text, -1));
			list.Add(GetFieldObject(161, "Authorize remarks", MasterDataType.Text, -1));
			list.Add(GetFieldObject(164, "Authorize date", MasterDataType.Date, -1));
			list.Add(GetFieldObject(116, "Print count", MasterDataType.Number, -1));
			list.Add(GetFieldObject(166, "Email count", MasterDataType.Number, -1));
		}
		list.Add(GetFieldObject(61, "Suspended", MasterDataType.Boolean, -1));
		flag = false;
		if (objVoucherTypeData != null && objVoucherTypeData.iMaskedVoucher != 1022)
		{
			if (objVoucherTypeData.iMaskedVoucher == 8448 || objVoucherTypeData.iMaskedVoucher == 3840 || objVoucherTypeData.iMaskedVoucher == 4096)
			{
				flag = true;
				sFieldName = "Account";
				iFieldId = 4;
			}
			else if (objVoucherTypeData.bCashBankVoucher)
			{
				flag = true;
				sFieldName = "CashBankAC";
				if (!objVoucherSettingData.bDontRestrictAcc1Type)
				{
					iFieldId = 4;
				}
			}
			else if ((objVoucherTypeData.bSalPurInvRet || objVoucherTypeData.iMaskedVoucher == 2816 || objVoucherTypeData.iMaskedVoucher == 5632 || objVoucherTypeData.iMaskedVoucher == 2560 || objVoucherTypeData.iMaskedVoucher == 6144 || objVoucherTypeData.iMaskedVoucher == 1280 || objVoucherTypeData.iMaskedVoucher == 8960 || objVoucherTypeData.iMaskedVoucher == 9216) && objVoucherSettingData.bUpdateFA && !objVoucherSettingData.bAccInLine)
			{
				flag = true;
				sFieldName = ((objVoucherTypeData.iMaskedVoucher == 3328 || objVoucherTypeData.iMaskedVoucher == 1792 || objVoucherTypeData.iMaskedVoucher == 5632 || objVoucherTypeData.iMaskedVoucher == 6144) ? "SalesAC" : "PurchaseAC");
				iFieldId = 3;
			}
			else if (objVoucherSettingData.bUpdateFA && objVoucherTypeData.bStockTypeVoucher && !objVoucherSettingData.bAccInLine)
			{
				sFieldName = ((objVoucherTypeData.iMaskedVoucher == 512 || objVoucherTypeData.iMaskedVoucher == 2048) ? "PurchaseAC" : "StockAC");
				iFieldId = 3;
			}
		}
		else
		{
			list.Add(GetFieldObject(4, "Account", MasterDataType.Master, 1));
			flag = true;
			sFieldName = "Account2";
			iFieldId = 3;
		}
		if (flag)
		{
			list.Add(GetFieldObject(iFieldId, sFieldName, MasterDataType.Master, 1));
		}
		if (objVoucherTypeData != null)
		{
			if (objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || (objVoucherTypeData.iMaskedVoucher == 2816 && objVoucherSettingData.bUpdateFA))
			{
				if (objVoucherTypeData.bPurchaseTypeVoucher)
				{
					sFieldName = "VendorAC";
				}
				else
				{
					sFieldName = (objVoucherTypeData.bSalesTypeVoucher ? "CustomerAC" : "WIPAC");
				}
				list.Add(GetFieldObject(4, sFieldName, MasterDataType.Master, 1));
			}
			else if (objVoucherTypeData.bStockTypeVoucher && objVoucherTypeData.iMaskedVoucher != 2816)
			{
				sFieldName = "PartyAC";
				list.Add(GetFieldObject(4, sFieldName, MasterDataType.Master, 1));
			}
		}
		else
		{
			list.Add(GetFieldObject(4, "Account", MasterDataType.Master, 1));
		}
		list.Add(GetFieldObject(8, "Production No", MasterDataType.Text, -1));
		list.Add(GetFieldObject(194, "BOM Name", MasterDataType.Text, -1));
		if (objPreferenceSettingData.bIssRptProcessWise)
		{
			list.Add(GetFieldObject(167, "SubProcess", MasterDataType.Master, 307));
		}
		list.Add(GetFieldObject(9, "ProdQty", MasterDataType.Fraction, -1));
		if (objVoucherTypeData == null || objVoucherTypeData.bSalPurInvRet)
		{
			list.Add(GetFieldObject(20, "UpdateStock", MasterDataType.Boolean, -1));
			list.Add(GetFieldObject(21, "RaiseReceipt", MasterDataType.Boolean, -1));
			list.Add(GetFieldObject(29, "LCNO", MasterDataType.Master, -1));
		}
		if (objVoucherSettingData == null || objVoucherSettingData.bInputPmtTerms)
		{
			list.Add(GetFieldObject(32, "Payment terms", MasterDataType.Text, -1));
		}
		if (objVoucherSettingData == null || objVoucherSettingData.iDueDate == 1)
		{
			if (objVoucherTypeData == null)
			{
				sFieldName = "Due date";
			}
			else
			{
				sFieldName = ((objVoucherTypeData.iMaskedVoucher == 5888 || objVoucherTypeData.iMaskedVoucher == 7168) ? "Maturity date" : "Due date");
			}
			list.Add(GetFieldObject(6, sFieldName, MasterDataType.Date, -1));
		}
		if (objVoucherTypeData != null && FConvert.IsItTransfer(objVoucherTypeData.iMaskedVoucher))
		{
			list.Add(GetFieldObject(31, "IssuesReceipts", MasterDataType.Text, -1));
			list.Add(GetFieldObject(86, "InvTag", MasterDataType.Master, objPreferenceSettingData.iInvTag));
		}
		if (objVoucherSettingData == null || objVoucherSettingData.iCurrencyPos == 1)
		{
			list.Add(GetFieldObject(10, "Currency", MasterDataType.Master, -1));
			if (objVoucherSettingData == null || objVoucherSettingData.bInputExchangeRate)
			{
				list.Add(GetFieldObject(11, "ExchangeRate", MasterDataType.Fraction, -1));
			}
		}
		if (!m_bLyteVersion)
		{
			list.Add(GetFieldObject(42, "Reconciled", MasterDataType.Boolean, -1));
		}
		if (m_bLyteVersion && (objVoucherSettingData == null || objVoucherSettingData.bVersion))
		{
			sFieldName = "Revision number";
			list.Add(GetFieldObject(117, sFieldName, MasterDataType.Number, -1));
		}
		list.Add(GetFieldObject(174, "Cheque status", MasterDataType.Text, -1));
		return list.ToArray();
	}

	private DocumentField GetFieldObject(int iFieldId, string sFieldName, MasterDataType oType, int iMasterType)
	{
		return new DocumentField
		{
			sFieldName = sFieldName,
			iDataTypeId = (byte)oType,
			iFieldId = iFieldId,
			iMasterLink = iMasterType
		};
	}

	private DocumentField[] GetBodyFields(VoucherTypeData objVoucherTypeData, PreferenceSettingData objPreferenceSettingData, VoucherSettingData objVoucherSettingData, TranSetType objType = TranSetType.Accounting)
	{
		bool flag = false;
		List<DocumentField> list = new List<DocumentField>();
		if (objVoucherTypeData != null && objVoucherTypeData.iMaskedVoucher == 1022)
		{
			list.Add(GetFieldObject(3, "StockAC", MasterDataType.Master, 1));
			list.Add(GetFieldObject(39, "StockAdjAC", MasterDataType.Master, 1));
			list.Add(GetFieldObject(181, "BalanceQty", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(182, "AvgRate", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(183, "BalanceStkVal", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(184, "NewAvgRate", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(185, "NewStkVal", MasterDataType.Fraction, -1));
		}
		else if ((objVoucherTypeData != null && (objVoucherTypeData.bCashBankVoucher || objVoucherTypeData.bJournalVoucher)) || (objVoucherSettingData != null && objVoucherSettingData.bAccountInLine))
		{
			string text = "Account2";
			if (objVoucherTypeData.bStockTypeVoucher)
			{
				text = ((objVoucherTypeData.iMaskedVoucher == 512 || objVoucherTypeData.iMaskedVoucher == 2048) ? "PurchaseAC" : "StockAC");
			}
			else if (objVoucherTypeData.bPurchaseTypeVoucher)
			{
				text = ((objVoucherTypeData.iMaskedVoucher != 3328 && objVoucherTypeData.iMaskedVoucher != 1792 && objVoucherTypeData.iMaskedVoucher != 5632) ? "PurchaseAC" : "SalesAC");
			}
			else
			{
				text = ((objVoucherTypeData.iMaskedVoucher == 8704) ? "DrAccount" : "Account2");
			}
			list.Add(GetFieldObject(3, text, MasterDataType.Master, 1));
		}
		if (objVoucherTypeData != null && objVoucherTypeData.iMaskedVoucher == 8704)
		{
			list.Add(GetFieldObject(4, "CrAccount", MasterDataType.Master, 1));
		}
		if (objVoucherTypeData != null && (objVoucherTypeData.bCashBankVoucher || objVoucherTypeData.iMaskedVoucher == 3840 || objVoucherTypeData.iMaskedVoucher == 4096 || objVoucherTypeData.iMaskedVoucher == 8704))
		{
			if (objVoucherSettingData == null || (objVoucherSettingData.bUpdateFA && objVoucherSettingData.bBillB4Qty))
			{
				list.Add(GetFieldObject(536870995, "Reference", MasterDataType.Text, -1));
			}
			list.Add(GetFieldObject(16, "Amount", MasterDataType.Fraction, -1));
		}
		else if (objVoucherTypeData == null && objType == TranSetType.PendingLink)
		{
			list.Add(GetFieldObject(16, "Amount", MasterDataType.Fraction, -1));
		}
		if (objVoucherTypeData == null || (objVoucherTypeData.bJournalVoucher && objVoucherTypeData.iMaskedVoucher != 8704 && objVoucherTypeData.iMaskedVoucher != 3840 && objVoucherTypeData.iMaskedVoucher != 4096))
		{
			list.Add(GetFieldObject(18, "Debit", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(19, "Credit", MasterDataType.Fraction, -1));
		}
		if (objType == TranSetType.Accounting || (objVoucherTypeData != null && (objVoucherTypeData.iMaskedVoucher == 5888 || objVoucherTypeData.iMaskedVoucher == 7168)))
		{
			list.Add(GetFieldObject(186, "PDC Status", MasterDataType.Text, -1));
		}
		if (objVoucherSettingData == null || (!objVoucherSettingData.bDontInputProduct && objVoucherTypeData != null && FConvert.IsItSales(objVoucherTypeData.iMaskedVoucher)))
		{
			list.Add(GetFieldObject(23, "Item", MasterDataType.Master, 2));
			list.Add(GetFieldObject(67, "Stock Value", MasterDataType.Fraction, -1));
		}
		if ((objVoucherTypeData != null && FConvert.IsItSales(objVoucherTypeData.iMaskedVoucher) && objVoucherSettingData != null && !objVoucherSettingData.bDontInputProduct) || (objPreferenceSettingData != null && objPreferenceSettingData.bInputUnitsPref))
		{
			list.Add(GetFieldObject(24, "Unit", MasterDataType.Master, 11));
		}
		if (objVoucherTypeData != null && objVoucherTypeData.iMaskedVoucher == 2816)
		{
			list.Add(GetFieldObject(31, "IssRct", MasterDataType.NumberList, -1));
		}
		if ((((objType == TranSetType.Inventory || objType == TranSetType.PendingLink || objType == TranSetType.Accounting) && objPreferenceSettingData.bRMASupport) || (objVoucherTypeData != null && (objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher) && objPreferenceSettingData.bRMASupport)) && (objVoucherTypeData == null || (objVoucherTypeData.iMaskedVoucher != 2560 && objVoucherTypeData.iMaskedVoucher != 5632 && objVoucherTypeData.iMaskedVoucher != 2304 && objVoucherTypeData.iMaskedVoucher != 7424)))
		{
			list.Add(GetFieldObject(38, "RMA", MasterDataType.Text, -1));
		}
		flag = false;
		bool flag2 = objVoucherSettingData?.bInputBatchEvenIfStockIsNotUpdated ?? false;
		if (flag2 || (objPreferenceSettingData.bEnableBatches && (objVoucherTypeData == null || objVoucherSettingData.bUpdateInv)))
		{
			flag = (flag2 = true);
			list.Add(GetFieldObject(13, "Batch", MasterDataType.Text, -1));
			list.Add(GetFieldObject(108, "Batch rate", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(36, "Manufacturing date", MasterDataType.Date, -1));
			list.Add(GetFieldObject(37, "Expiry date", MasterDataType.Date, -1));
		}
		if (objVoucherTypeData == null || ((objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher || objVoucherTypeData.iMaskedVoucher == 2816) && !objVoucherSettingData.bDontInputQtyRate))
		{
			list.Add(GetFieldObject(26, "Quantity", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(59, "Quantity in base unit", MasterDataType.Fraction, -1));
			if (objPreferenceSettingData.bAlternateUnit)
			{
				list.Add(GetFieldObject(41, "Alternate quantity", MasterDataType.Fraction, -1));
			}
			list.Add(GetFieldObject(168, "Opening stock quantity", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(169, "Opening stock value", MasterDataType.Fraction, -1));
		}
		if (!m_bLyteVersion && (objVoucherSettingData == null || objVoucherSettingData.iReserveStock > 0))
		{
			list.Add(GetFieldObject(22, "Reserve quantity", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(89, "Release quantity", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(115, "Balance reserve quantity", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(146, "Reservation Batch no", MasterDataType.Text, -1));
			list.Add(GetFieldObject(197, "Reservation RMA no", MasterDataType.Text, -1));
		}
		if (!m_bLyteVersion && (objVoucherTypeData == null || objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher || objVoucherTypeData.bCashBankVoucher || objVoucherTypeData.iMaskedVoucher == 2816))
		{
			list.Add(GetFieldObject(143, "Base Link doc. number", MasterDataType.Text, -1));
			list.Add(GetFieldObject(144, "Base Link doc. date", MasterDataType.Date, -1));
			list.Add(GetFieldObject(163, "Base Link status", MasterDataType.Text, -1));
			list.Add(GetFieldObject(173, "Link status", MasterDataType.Text, -1));
		}
		if (objVoucherSettingData != null && objVoucherSettingData.iCurrencyPos == 2)
		{
			list.Add(GetFieldObject(14, "Currency", MasterDataType.Master, -1));
			if (!m_bLyteVersion && objVoucherSettingData != null && objVoucherSettingData.bInputExchangeRate)
			{
				list.Add(GetFieldObject(15, "Exchange rate", MasterDataType.Fraction, -1));
			}
		}
		if (objVoucherTypeData == null || ((objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher || objVoucherTypeData.iMaskedVoucher == 2816) && !objVoucherSettingData.bDontInputQtyRate && !objVoucherSettingData.bHideRateAndGross))
		{
			list.Add(GetFieldObject(27, "Rate", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(55, "Rate original", MasterDataType.Fraction, -1));
		}
		if (objVoucherTypeData == null || ((objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher || objVoucherTypeData.iMaskedVoucher == 2816) && !objVoucherSettingData.bHideRateAndGross))
		{
			list.Add(GetFieldObject(28, "Gross", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(54, "Gross original", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(94, "Gross local", MasterDataType.Fraction, -1));
		}
		if (objVoucherSettingData != null && objVoucherSettingData.bEnableQC)
		{
			list.Add(GetFieldObject(57, "QCStatus", MasterDataType.Text, -1));
		}
		if (!m_bLyteVersion && (objVoucherTypeData == null || ((objVoucherTypeData.bSalesTypeVoucher || objVoucherTypeData.bPurchaseTypeVoucher || objVoucherTypeData.bStockTypeVoucher) && (objVoucherSettingData.bInputBins || objPreferenceSettingData.bEnableBins) && (objVoucherSettingData.bUpdateInv || FConvert.IsItTransfer(objVoucherTypeData.iMaskedVoucher)))) && (objVoucherTypeData == null || (objVoucherTypeData.iMaskedVoucher != 2560 && objVoucherTypeData.iMaskedVoucher != 5632 && objVoucherTypeData.iMaskedVoucher != 2304 && objVoucherTypeData.iMaskedVoucher != 7424)))
		{
			list.Add(GetFieldObject(85, "Bin detail", MasterDataType.Text, -1));
			list.Add(GetFieldObject(130, "Skid", MasterDataType.Text, -1));
			list.Add(GetFieldObject(195, "Carton number", MasterDataType.Text, -1));
			list.Add(GetFieldObject(162, "Bins", MasterDataType.Master, 12));
		}
		if (!m_bLyteVersion && !flag2 && (objVoucherTypeData == null || ((flag || objVoucherSettingData.bInputBins) && !objVoucherTypeData.bOutward)))
		{
			if (objVoucherSettingData == null || objVoucherSettingData.bInputBins || objPreferenceSettingData.bBatchMfg)
			{
				list.Add(GetFieldObject(36, "Manufacturing date", MasterDataType.Date, -1));
			}
			if (objVoucherSettingData == null || objVoucherSettingData.bInputBins || objPreferenceSettingData.bBatchExpiry)
			{
				list.Add(GetFieldObject(37, "Expiry date", MasterDataType.Date, -1));
			}
		}
		if (objVoucherSettingData != null && objVoucherSettingData.iDueDate == 2)
		{
			List<DocumentField> list2 = list;
			InvoiceLayout invoiceLayout = this;
			string sFieldName;
			if (objVoucherTypeData == null)
			{
				sFieldName = "Due date";
			}
			else
			{
				sFieldName = ((objVoucherTypeData.iMaskedVoucher == 5888 || objVoucherTypeData.iMaskedVoucher == 7168) ? "Maturity date" : "Due date");
			}
			list2.Add(invoiceLayout.GetFieldObject(7, sFieldName, MasterDataType.Date, -1));
		}
		if (objType == TranSetType.Inventory || (objVoucherTypeData != null && FConvert.IsItTransfer(objVoucherTypeData.iMaskedVoucher)))
		{
			list.Add(GetFieldObject(87, "InvTag2", MasterDataType.Master, objPreferenceSettingData.iInvTag));
			list.Add(GetFieldObject(84, "Bins2", MasterDataType.Master, 12));
		}
		list.Add(GetFieldObject(75, "Net amount", MasterDataType.Fraction, -1));
		list.Add(GetFieldObject(53, "Net amount original", MasterDataType.Fraction, -1));
		list.Add(GetFieldObject(93, "Net amount local", MasterDataType.Fraction, -1));
		list.Add(GetFieldObject(171, "Voucher amount", MasterDataType.Fraction, -1));
		list.Add(GetFieldObject(154, "Footer amount", MasterDataType.Fraction, -1));
		if (objVoucherTypeData == null || objVoucherTypeData.bSalPurInvRet)
		{
			list.Add(GetFieldObject(45, "COGS ", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(54, "Original gross", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(53, "Original amount", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(50, "Transaction amount", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(156, "Transaction footer amount", MasterDataType.Fraction, -1));
			if (!m_bLyteVersion)
			{
				list.Add(GetFieldObject(93, "Local net amount", MasterDataType.Fraction, -1));
				list.Add(GetFieldObject(94, "Local gross amount", MasterDataType.Fraction, -1));
				list.Add(GetFieldObject(145, "Local exchange rate", MasterDataType.Fraction, -1));
				list.Add(GetFieldObject(155, "Local footer amount", MasterDataType.Fraction, -1));
			}
		}
		if (!m_bLyteVersion)
		{
			list.Add(GetFieldObject(76, "Serial no", MasterDataType.Number, -1));
		}
		return list.ToArray();
	}

	private IdNamePair[] GetMasterTagsInVoucher(int iVoucherType, TranSetType objType, Database objDB, int iCompId)
	{
		IdNamePair idNamePair = new IdNamePair();
		string text = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		List<IdNamePair> list = new List<IdNamePair>();
		try
		{
			text = ((objType != TranSetType.FixedAssets) ? string.Format("SELECT DISTINCT iMasterType, sCaption[sMasterName], 1[iPos] FROM cCore_VoucherMasters{0} \r\n                    JOIN cCore_MasterDef ON iMasterType = iMasterTypeId\r\n                    JOIn cCore_MasterLanguage ON cCore_MasterLanguage.iLinkId = iMasterType AND iLanguageId = 0 AND cCore_MasterLanguage.iLinkTypeId = 0\r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = cCore_VoucherMasters{0}.iVoucherType\r\n                    {1} ORDER BY iMasterType", FConvert.GetSuffix(iCompId), GetFilterOfTranSetType(objType, iVoucherType, iCompId)) : $"SELECT iMasterTypeId[iMasterType], sMasterName, 0[iPos]\r\n                    FROM cCore_MasterDef where iMasterTypeId = {601}");
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iMasterType"]);
				idNamePair.Name = Convert.ToString(dataReader["sMasterName"]);
				idNamePair.Tag = dataReader["iPos"];
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch
		{
			return null;
		}
		finally
		{
			if (dataReader != null && !dataReader.IsClosed)
			{
				dataReader.Close();
			}
		}
		return list.ToArray();
	}

	private DocumentField[] GetExtraFields(int iVoucherType, TranSetType objType, int iLanguageId, Database objDB, int iCompId)
	{
		string text = null;
		string suffix = FConvert.GetSuffix(iCompId);
		bool flag = false;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		DocumentField documentField = null;
		List<DocumentField> list = new List<DocumentField>();
		text = string.Format("SELECT DISTINCT cCore_Fields.iFieldId , ISNULL(sAlias, sCaption)[sCaption], iDataTypeId, iMasterLink,sDefaultValue,MAX(cCore_VoucherFields{0}.iFieldId)[FieldId]\r\n            FROM cCore_VoucherFields{0} JOIN cCore_Fields ON cCore_VoucherFields{0}.iUniqueId=cCore_Fields.iFieldId \r\n            JOIN cCore_Vouchers{0} ON cCore_VoucherFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n            LEFT JOIN cCore_FieldsLanguage{0} ON cCore_FieldsLanguage{0}.iFieldId = cCore_Fields.iFieldId AND iLanguageId = {2}\r\n            {1} AND cCore_Vouchers{0}.iVoucherType <> 0X3FF AND cCore_Vouchers{0}.iVoucherType <> 0X3FE\r\n            AND bNotAvailableForReports = 0\r\n            GROUP BY cCore_Fields.iFieldId, ISNULL(sAlias, sCaption), iDataTypeId, sDefaultValue,iMasterLink\r\n            ORDER BY MAX(cCore_VoucherFields{0}.iFieldId)", suffix, GetFilterOfTranSetType(objType, iVoucherType, iCompId), iLanguageId);
		dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDB.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			documentField = new DocumentField();
			documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
			documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
			documentField.sDefaultValue = Convert.ToString(dataReader["sDefaultValue"]);
			documentField.iDataTypeId = Convert.ToByte(dataReader["iDataTypeId"]);
			documentField.iMasterLink = Convert.ToInt32(dataReader["iMasterLink"]);
			documentField.iFieldId |= 16777216;
			list.Add(documentField);
		}
		dataReader.Close();
		if (FConvert.IsItSales(iVoucherType))
		{
			text = string.Format("SELECT DISTINCT cCore_Fields.iFieldId, ISNULL(sAlias, sCaption)[sCaption], bFooter \r\n                FROM cCore_VoucherScreenFields{0} \r\n                JOIN cCore_Fields ON cCore_VoucherScreenFields{0}.iUniqueId=cCore_Fields.iFieldId \r\n                LEFT JOIN cCore_FieldsLanguage{0} ON cCore_FieldsLanguage{0}.iFieldId = cCore_Fields.iFieldId AND iLanguageId = {2}\r\n                JOIN cCore_Vouchers{0} ON cCore_VoucherScreenFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n                {1} AND iDataTypeId<>0 AND bNotAvailableForReports = 0 AND cCore_Vouchers{0}.iVoucherType <> 0x3ff  AND cCore_Vouchers{0}.iVoucherType <> 0x3fe", suffix, GetFilterOfTranSetType(objType, iVoucherType, iCompId), iLanguageId);
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				documentField = new DocumentField();
				documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
				documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
				documentField.iDataTypeId = 6;
				flag = !Convert.ToBoolean(dataReader["bFooter"]);
				documentField.iFieldId |= ((!flag) ? 33554432 : 134217728);
				list.Add(documentField);
				if (flag)
				{
					documentField = new DocumentField();
					documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
					documentField.sFieldName = Convert.ToString(dataReader["sCaption"]) + "_Input";
					documentField.iDataTypeId = 6;
					flag = !Convert.ToBoolean(dataReader["bFooter"]);
					documentField.iFieldId |= 33554432;
					list.Add(documentField);
				}
			}
			dataReader.Close();
			text = string.Format("SELECT DISTINCT ISNULL(sAlias, sCaption)[sCaption], bAllowDebitAccount, bAllowCreditAccount, iColMap\r\n                FROM cCore_VoucherScreenFields{0} \r\n                JOIN cCore_Fields ON cCore_VoucherScreenFields{0}.iUniqueId=cCore_Fields.iFieldId \r\n                JOIN cCore_Vouchers{0} ON cCore_VoucherScreenFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n                LEFT JOIN cCore_FieldsLanguage{0} ON cCore_FieldsLanguage{0}.iFieldId = cCore_Fields.iFieldId AND iLanguageId = {2}\r\n                {1} AND iDataTypeId<>0 AND bNotAvailableForReports = 0 AND cCore_Vouchers{0}.iVoucherType <> 0x3ff\r\n                AND cCore_Vouchers{0}.iVoucherType <> 0x3fe\r\n                AND bFooter = 1 AND bPostToAccount = 1 AND (bAllowDebitAccount = 1 OR bAllowCreditAccount = 1)", suffix, GetFilterOfTranSetType(objType, iVoucherType, iCompId), iLanguageId);
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				if (Convert.ToBoolean(dataReader["bAllowDebitAccount"]))
				{
					documentField = new DocumentField();
					documentField.iDataTypeId = 12;
					documentField.iMasterLink = 1;
					documentField.iFieldId = 134218729 + Convert.ToInt32(dataReader["iColMap"]);
					documentField.sFieldName = Convert.ToString(dataReader["sCaption"]) + "Dr A/C";
					list.Add(documentField);
				}
				if (Convert.ToBoolean(dataReader["bAllowCreditAccount"]))
				{
					documentField = new DocumentField();
					documentField.iDataTypeId = 12;
					documentField.iMasterLink = 1;
					documentField.iFieldId = 134218730 + Convert.ToInt32(dataReader["iColMap"]);
					documentField.sFieldName = Convert.ToString(dataReader["sCaption"]) + "Cr A/C";
					list.Add(documentField);
				}
			}
			dataReader.Close();
		}
		return list.ToArray();
	}

	private DocumentField[] GetMasterTags(IdNamePair[] arrMasterTags, bool bHeader)
	{
		short num = 0;
		DocumentField documentField = null;
		List<DocumentField> list = new List<DocumentField>();
		if (arrMasterTags != null)
		{
			for (num = 0; num < arrMasterTags.Length; num++)
			{
				documentField = new DocumentField();
				documentField.sFieldName = arrMasterTags[num].Name;
				documentField.iDataTypeId = 12;
				documentField.iMasterLink = arrMasterTags[num].ID;
				if (bHeader)
				{
					documentField.iFieldId = 0x10000000 | arrMasterTags[num].ID;
					if ((byte)Convert.ToInt32(arrMasterTags[num].Tag) == 0)
					{
						list.Add(documentField);
					}
				}
				else
				{
					documentField.iFieldId = 0x20000000 | arrMasterTags[num].ID;
					if ((byte)Convert.ToInt32(arrMasterTags[num].Tag) == 1)
					{
						list.Add(documentField);
					}
				}
			}
		}
		return list.ToArray();
	}

	private string GetFilterOfTranSetType(TranSetType objType, int iVoucherType, int iCompId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (objType)
		{
		case TranSetType.PendingLink:
			stringBuilder.AppendFormat(" 1 = 1 ");
			break;
		case TranSetType.Accounting:
			stringBuilder.AppendFormat(" bUpdateFA = 1 ");
			break;
		case TranSetType.Inventory:
			stringBuilder.AppendFormat(" bUpdateInv = 1 ");
			break;
		case TranSetType.VoucherClass:
			if (iVoucherType == 8704)
			{
				stringBuilder.AppendFormat(" cCore_Vouchers{0}.iVoucherType & {1}  IN ({2,3}) ", FConvert.GetSuffix(iCompId), 65280, 8704, 3584);
			}
			else
			{
				stringBuilder.AppendFormat(" cCore_Vouchers{0}.iVoucherType & {2}  = {1} ", FConvert.GetSuffix(iCompId), iVoucherType, 65280);
			}
			break;
		case TranSetType.Voucher:
			if (iVoucherType > 0)
			{
				stringBuilder.AppendFormat(" cCore_Vouchers{0}.iVoucherType  = {1} ", FConvert.GetSuffix(iCompId), iVoucherType);
			}
			break;
		case TranSetType.FixedAssets:
			stringBuilder.Append(" 1 = 0 ");
			break;
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Insert(0, EnumUtils.DescriptionOf(SQLKey.WHERE));
		}
		return stringBuilder.ToString();
	}

	private DocumentField[] GetReferenceFields()
	{
		return new List<DocumentField>
		{
			GetFieldObject(133, "Reference Details", MasterDataType.Text, -1),
			GetFieldObject(134, "Reference Number", MasterDataType.Text, -1),
			GetFieldObject(135, "Reference Date", MasterDataType.Text, -1),
			GetFieldObject(141, "Reference Base Amount", MasterDataType.Text, -1),
			GetFieldObject(137, "Reference Bill Number", MasterDataType.Text, -1),
			GetFieldObject(139, "Reference Due-Date", MasterDataType.Date, -1),
			GetFieldObject(193, "Reference Due-Date Details", MasterDataType.Text, -1)
		}.ToArray();
	}

	private DocumentField[] GetAIFAFields(Database objDB, int iCompId)
	{
		List<DocumentField> list = new List<DocumentField>();
		DbCommand dbCommand = null;
		string text = $"SELECT COUNT(1) FROM cCore_Reports{FConvert.GetSuffix(iCompId)} WHERE sReportName = 'AIFA SALES REPORT'";
		dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		if (Convert.ToInt32(objDB.ExecuteScalar(dbCommand)) > 0)
		{
			list.Add(GetFieldObject(198, "Prediction Purchase", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(199, "Prediction Sales", MasterDataType.Fraction, -1));
			list.Add(GetFieldObject(200, "Prediction Stock", MasterDataType.Fraction, -1));
		}
		return list.ToArray();
	}

	public DocumentField[] GetDocFieldsOfTransactionSets(_TransactionSet[] arrTranSets, int iCompId)
	{
		byte b = 0;
		int[] array = null;
		string strVouchers = string.Empty;
		Database database = null;
		VoucherTypeData voucherTypeData = null;
		PreferenceSettingData preferenceSettingData = null;
		VoucherSettingData voucherSettingData = null;
		DocumentField[] array2 = null;
		IdNamePair[] array3 = null;
		List<DocumentField> list = null;
		database = DatabaseWrapper.GetDatabase2(iCompId, -1, bReplicationServer: true);
		list = new List<DocumentField>();
		array = GetAllVouchersOfTransactionSets(arrTranSets, database, iCompId, ref strVouchers);
		preferenceSettingData = GetPreferenceSettingData(database, iCompId);
		array3 = GetMasterTagsInVouchers(strVouchers, database, iCompId);
		array2 = GetExtraFieldsOfVouchers(array, strVouchers, database, iCompId);
		for (b = 0; b < array.Length; b++)
		{
			voucherTypeData = GetVoucherTypeData(array[b]);
			voucherSettingData = GetVoucherSettingData(array[b], database, iCompId);
			GetDistinctFields(ref list, GetHeaderFields(voucherTypeData, preferenceSettingData, voucherSettingData));
			GetDistinctFields(ref list, GetMasterTags(array3, bHeader: true));
			GetDistinctFields(ref list, GetBodyFields(voucherTypeData, preferenceSettingData, voucherSettingData));
			GetDistinctFields(ref list, GetMasterTags(array3, bHeader: false));
		}
		if (array2 != null && array2 != null)
		{
			GetDistinctFields(ref list, array2.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.LAY_HDR)) == Convert.ToInt32(VTFIELDFLAG.LAY_HDR)).ToArray());
		}
		if (array2 != null && array2 != null)
		{
			GetDistinctFields(ref list, array2.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.LAY_BODY)) == Convert.ToInt32(VTFIELDFLAG.LAY_BODY)).ToArray());
			GetDistinctFields(ref list, array2.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.SCR_BODY)) == Convert.ToInt32(VTFIELDFLAG.SCR_BODY)).ToArray());
		}
		if (array2 != null && array2 != null)
		{
			GetDistinctFields(ref list, array2.Where((DocumentField p) => (p.iFieldId & Convert.ToInt32(VTFIELDFLAG.SCR_FTR)) == Convert.ToInt32(VTFIELDFLAG.SCR_FTR)).ToArray());
		}
		return list.ToArray();
	}

	public IdNamePair[] GetTransactionSetTypes(Module oModule, int iCompanyId)
	{
		if (oModule == Module.Payroll)
		{
			return GetTransactionSetForPayroll();
		}
		return EnumUtils.GetEnumMembers(TransactionSetType.None, IsDisplayDescription: true);
	}

	private int[] GetAllVouchersOfTransactionSets(_TransactionSet[] arrTranSets, Database objDB, int iCompId, ref string strVouchers)
	{
		byte b = 0;
		string suffix = FConvert.GetSuffix(iCompId);
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		StringBuilder stringBuilder = null;
		List<int> list = null;
		for (b = 0; b < arrTranSets.Length; b++)
		{
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder();
				stringBuilder.Append(" WHERE");
			}
			else
			{
				stringBuilder.Append(" OR");
			}
			switch (arrTranSets[b].TransactionSetId)
			{
			case TransactionSetType.AccountingTransactions:
			case TransactionSetType.AccountingTransactionsOfanAccount:
			case TransactionSetType.AccountingTransactionsOfSelectedAccounts:
				stringBuilder.AppendFormat(" (cCore_Vouchers{0}.bUpdateFA = 1)", suffix);
				break;
			case TransactionSetType.AccountingTransactionsOfAccountingTag:
				stringBuilder.AppendFormat(" (cCore_VoucherMasters{0}.iMasterType = {1} AND cCore_Vouchers{0}.bUpdateFA = 1)", suffix, _focus.company(iCompId).faTagId);
				break;
			case TransactionSetType.AccountingTransactionsOfaTag:
				stringBuilder.AppendFormat(" (cCore_VoucherMasters{0}.iMasterType = {1} AND cCore_Vouchers{0}.bUpdateFA = 1)", suffix, arrTranSets[b].VoucherType);
				break;
			case TransactionSetType.AccountingTransactionsOfInventoryTag:
				stringBuilder.AppendFormat(" (cCore_VoucherMasters{0}.iMasterType = {1} AND cCore_Vouchers{0}.bUpdateFA = 1)", suffix, _focus.company(iCompId).invTagId);
				break;
			case TransactionSetType.AllTransactionsOfDocumentClass:
				stringBuilder.AppendFormat(" (cCore_Vouchers{0}.iVoucherType & {1} = {2} and cCore_Vouchers{0}.bUpdateFA = 1)", suffix, Convert.ToInt32(VTTYPE.VOUCHERMASK), arrTranSets[b].VoucherType);
				break;
			case TransactionSetType.AllTransactionsOfDocumentType:
				stringBuilder.AppendFormat(" (cCore_Vouchers{0}.iVoucherType = {1} and cCore_Vouchers{0}.bUpdateFA = 1)", suffix, arrTranSets[b].VoucherType);
				break;
			case TransactionSetType.InventoryTransactions:
			case TransactionSetType.InventoryTransactionsOfaProduct:
			case TransactionSetType.InventoryTransactionsOfSelectedProducts:
				stringBuilder.AppendFormat(" tCore_Header{0}.bUpdateStocks = 1", suffix);
				break;
			case TransactionSetType.InventoryTransactionsOfAccountingTag:
				stringBuilder.AppendFormat(" (cCore_VoucherMasters{0}.iMasterType = {1} AND tCore_Header{0}.bUpdateStocks = 1)", suffix, _focus.company(iCompId).faTagId);
				break;
			case TransactionSetType.InventoryTransactionsOfaTag:
				stringBuilder.AppendFormat(" cCore_VoucherMasters{0}.iMasterType = {1} AND tCore_Header{0}.bUpdateStocks = 1", suffix, arrTranSets[b].VoucherType);
				break;
			case TransactionSetType.InventoryTransactionsOfInventoryTag:
				stringBuilder.AppendFormat(" (cCore_VoucherMasters{0}.iMasterType = {1} AND tCore_Header{0}.bUpdateStocks = 1)", suffix, _focus.company(iCompId).invTagId);
				break;
			default:
				stringBuilder.Append(" (1=1)");
				break;
			}
		}
		stringBuilder.Insert(0, string.Format("SELECT DISTINCT cCore_Vouchers{0}.iVoucherType FROM cCore_Vouchers{0}\r\n            LEFT JOIN cCore_VoucherMasters{0} ON cCore_VoucherMasters{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType", suffix));
		list = new List<int>();
		dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(stringBuilder.ToString()) : stringBuilder.ToString());
		dataReader = objDB.ExecuteReader(dbCommand);
		while (dataReader.Read())
		{
			list.Add(Convert.ToInt32(dataReader["iVoucherType"]));
			if (strVouchers.Length > 0)
			{
				strVouchers += ",";
			}
			strVouchers += string.Format("{0}", dataReader["iVoucherType"]);
		}
		dataReader.Close();
		return list.ToArray();
	}

	private IdNamePair[] GetMasterTagsInVouchers(string strVouchers, Database objDB, int iCompId)
	{
		IdNamePair idNamePair = new IdNamePair();
		string text = null;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		List<IdNamePair> list = new List<IdNamePair>();
		try
		{
			text = string.Format("SELECT DISTINCT iMasterType, sMasterName, iPos FROM cCore_VoucherMasters{0} \r\n                    JOIN cCore_MasterDef ON iMasterType = iMasterTypeId\r\n                    JOIN cCore_Vouchers{0} ON cCore_Vouchers{0}.iVoucherType = cCore_VoucherMasters{0}.iVoucherType\r\n                    WHERE cCore_Vouchers{0}.iVoucherType IN ({1}) ORDER BY iMasterType", FConvert.GetSuffix(iCompId), strVouchers);
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				idNamePair = new IdNamePair();
				idNamePair.ID = Convert.ToInt32(dataReader["iMasterType"]);
				idNamePair.Name = Convert.ToString(dataReader["sMasterName"]);
				idNamePair.Tag = dataReader["iPos"];
				list.Add(idNamePair);
			}
			dataReader.Close();
		}
		catch
		{
			return null;
		}
		finally
		{
			if (dataReader != null && !dataReader.IsClosed)
			{
				dataReader.Close();
			}
		}
		return list.ToArray();
	}

	private DocumentField[] GetExtraFieldsOfVouchers(int[] arrVouchers, string strVouchers, Database objDB, int iCompId)
	{
		byte b = 0;
		string text = null;
		bool flag = false;
		IDataReader dataReader = null;
		DbCommand dbCommand = null;
		DocumentField documentField = null;
		List<DocumentField> list = null;
		text = string.Format("SELECT DISTINCT cCore_Fields.iFieldId , sCaption, iDataTypeId, iMasterLink,1[bHeader]\r\n            FROM cCore_VoucherFields{0} JOIN cCore_Fields ON cCore_VoucherFields{0}.iUniqueId=cCore_Fields.iFieldId \r\n            JOIN cCore_Vouchers{0} ON cCore_VoucherFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n            WHERE cCore_Vouchers{0}.iVoucherType IN({1}) ", FConvert.GetSuffix(iCompId), strVouchers);
		dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
		dataReader = objDB.ExecuteReader(dbCommand);
		list = new List<DocumentField>();
		while (dataReader.Read())
		{
			documentField = new DocumentField();
			documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
			documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
			documentField.iDataTypeId = Convert.ToByte(dataReader["iDataTypeId"]);
			documentField.iMasterLink = Convert.ToInt32(dataReader["iMasterLink"]);
			flag = Convert.ToBoolean(dataReader["bHeader"]);
			documentField.iFieldId |= ((!flag) ? 16777216 : 67108864);
			list.Add(documentField);
		}
		dataReader.Close();
		strVouchers = string.Empty;
		for (b = 0; b < arrVouchers.Length; b++)
		{
			if (FConvert.IsItSales(arrVouchers[b]))
			{
				if (strVouchers.Length > 0)
				{
					strVouchers += ",";
				}
				strVouchers += $"{arrVouchers[b]}";
			}
		}
		if (strVouchers != null)
		{
			text = string.Format("SELECT DISTINCT cCore_Fields.iFieldId, sCaption, bFooter \r\n                FROM cCore_VoucherScreenFields{0} \r\n                JOIN cCore_Fields ON cCore_VoucherScreenFields{0}.iUniqueId=cCore_Fields.iFieldId \r\n                JOIN cCore_Vouchers{0} ON cCore_VoucherScreenFields{0}.iVoucherType = cCore_Vouchers{0}.iVoucherType \r\n                WHERE cCore_Vouchers{0}.iVoucherType IN({1})", FConvert.GetSuffix(iCompId), strVouchers);
			dbCommand = objDB.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dataReader = objDB.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				documentField = new DocumentField();
				documentField.iFieldId = Convert.ToInt32(dataReader["iFieldId"]);
				documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
				documentField.iDataTypeId = 6;
				flag = !Convert.ToBoolean(dataReader["bFooter"]);
				documentField.iFieldId |= ((!flag) ? 33554432 : 134217728);
				list.Add(documentField);
			}
			dataReader.Close();
		}
		return list.ToArray();
	}

	private void GetDistinctFields(ref List<DocumentField> arrTotalFields, DocumentField[] arrFields)
	{
		byte b = 0;
		byte b2 = 0;
		for (b = 0; b < arrFields.Length; b++)
		{
			b2 = 0;
			while (b2 < arrTotalFields.Count && arrTotalFields[b2].iFieldId != arrFields[b].iFieldId)
			{
				b2++;
			}
			if (b2 >= arrTotalFields.Count)
			{
				arrTotalFields.Add(arrFields[b]);
			}
		}
	}

	public DocumentField[] GetDocFieldsForProduction(int iScreenId, int iCompanyId)
	{
		try
		{
			string empty = string.Empty;
			Database database = DatabaseWrapper.GetDatabase2(iCompanyId);
			IDataReader dataReader = null;
			DbCommand dbCommand = null;
			List<DocumentField> list = new List<DocumentField>();
			empty = string.Format($"select Id,sCaption,iFieldType,iMasterId from mMRP_ExtraFields where iScreenId = {iScreenId}");
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(empty) : empty);
			dbCommand.CommandTimeout = 0;
			dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				DocumentField documentField = new DocumentField();
				documentField.iFieldId = Convert.ToInt32(dataReader["Id"]);
				documentField.sFieldName = Convert.ToString(dataReader["sCaption"]);
				documentField.iDataTypeId = Convert.ToByte(dataReader["iFieldType"]);
				documentField.iMasterLink = Convert.ToInt32(dataReader["iMasterId"]);
				list.Add(documentField);
			}
			dataReader.Close();
			return list.ToArray();
		}
		catch (Exception ex)
		{
			m_sError = ex.Message;
			return new DocumentField[0];
		}
	}
}
