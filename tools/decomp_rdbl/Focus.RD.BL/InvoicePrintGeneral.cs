using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Focus.Common.BL;
using Focus.Common.DataStructs;
using Focus.DatabaseFactory;
using Focus.Masters.DataStructs;
using Focus.RD.DataStructs;
using Focus.TranSettings.BL;
using Focus.TranSettings.DataStructs;
using Focus.Transactions.BL;
using Focus.Transactions.DataStructs;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Focus.RD.BL;

public class InvoicePrintGeneral
{
	private bool IsDontConvertField;

	private bool m_bIsAppliedHIUS;

	private bool m_bPaymentTerm;

	private bool m_bLinkToLC;

	private bool IsMasterCode;

	private bool m_bFormulaAdded;

	private bool m_bClubByWithCondition;

	private bool IsIgnoreDayInExpiry;

	private bool IsPrintZeroValueAsNumeric;

	private bool IsNumericSeparator;

	private byte PrintBooleanAs;

	private bool m_bBarcodePrint;

	private bool m_bPrintUnAuthorizeDocument = true;

	private bool m_bMillionSeperator;

	private int m_iVoucherType;

	private int m_iLayoutId;

	private int m_iCompanyId;

	private int m_iLanguageId;

	private int m_iAltLanguageId;

	private int m_iDefaultCurrencyId;

	private int m_iUserId;

	private int m_iOrderByBins;

	private int m_iOrderByCount;

	private int m_iInvTag;

	private int m_iFaTag;

	private double m_dClientOffset;

	private string m_strDateFormat;

	private TransactionPrintCommon m_objTransactionPrintCommon = new TransactionPrintCommon();

	private CalendarType m_objCalType = CalendarType.Gregorean;

	private LayoutInformation m_layoutInfo;

	private Transaction m_objTranData;

	private CurrencyDetail m_oCurrDetail;

	private InvoicePreferences m_oPrintPreferences;

	private string[] m_arrAgeingSlabs;

	private byte[] m_arrCompanyLogo;

	private IdNamePair[] m_arrMasters;

	private IdNamePair[] m_arrDefaultFields;

	private IdNamePair[] m_arrGroupedData;

	private IdNamePair[] m_arrMiscellaneousValue;

	private VWVoucherFields[] m_arrDocumentFields;

	private List<int> m_arrNonDefaultRow;

	private List<int> m_arrGroupingRow;

	private Dictionary<string, string> m_arrResources;

	private List<IdNamePair> m_arrDocumentData;

	private List<IdValuePair> m_arrNewLevelsOrderBy;

	private List<HeaderGroup> m_arrHeaderGroup = new List<HeaderGroup>();

	private List<HeaderGroup> m_arrBodyGroup = new List<HeaderGroup>();

	private List<IdValuePair> m_arrBodyGroupClubOrderBy;

	private List<IdValuePair> m_arrExternalModuleDataAll;

	public int CompanyId
	{
		get
		{
			return m_iCompanyId;
		}
		set
		{
			m_iCompanyId = value;
		}
	}

	public int LanguageId
	{
		get
		{
			return m_iLanguageId;
		}
		set
		{
			m_iLanguageId = value;
		}
	}

	public int AlternateLaguageId
	{
		get
		{
			return m_iAltLanguageId;
		}
		set
		{
			m_iAltLanguageId = value;
		}
	}

	public int LayoutId
	{
		get
		{
			return m_iLayoutId;
		}
		set
		{
			m_iLayoutId = value;
		}
	}

	public int UserId
	{
		get
		{
			return m_iUserId;
		}
		set
		{
			m_iUserId = value;
		}
	}

	public LayoutInformation LayoutInfo
	{
		get
		{
			return m_layoutInfo;
		}
		set
		{
			m_layoutInfo = value;
		}
	}

	public Transaction TransactionData
	{
		get
		{
			return m_objTranData;
		}
		set
		{
			m_objTranData = value;
			if (m_objTranData != null)
			{
				m_objTranData.BodyData = FilterProducts();
			}
		}
	}

	public InvoicePreferences InvoicePreferences
	{
		get
		{
			return m_oPrintPreferences;
		}
		set
		{
			m_oPrintPreferences = value;
			InitPreferences(m_oPrintPreferences);
		}
	}

	public byte[] CompanyLogo
	{
		get
		{
			return m_arrCompanyLogo;
		}
		set
		{
			m_arrCompanyLogo = value;
		}
	}

	public CalendarType DefaultCalendar
	{
		get
		{
			return m_objCalType;
		}
		set
		{
			m_objCalType = value;
		}
	}

	public IdNamePair[] Masters
	{
		get
		{
			return m_arrMasters;
		}
		set
		{
			m_arrMasters = value;
		}
	}

	public VWVoucherFields[] DcoumentFields
	{
		get
		{
			return m_arrDocumentFields;
		}
		set
		{
			m_arrDocumentFields = value;
		}
	}

	public List<HeaderGroup> HeaderData
	{
		get
		{
			return m_arrHeaderGroup;
		}
		set
		{
			m_arrHeaderGroup = value;
		}
	}

	public List<HeaderGroup> BodyData
	{
		get
		{
			return m_arrBodyGroup;
		}
		set
		{
			m_arrBodyGroup = value;
		}
	}

	public Dictionary<string, string> ResourceMessage
	{
		get
		{
			return m_arrResources;
		}
		set
		{
			m_arrResources = value;
		}
	}

	public string DateFormat
	{
		get
		{
			return m_strDateFormat;
		}
		set
		{
			m_strDateFormat = value;
		}
	}

	public string[] AgeingSlabs
	{
		get
		{
			return m_arrAgeingSlabs;
		}
		set
		{
			m_arrAgeingSlabs = value;
		}
	}

	public bool BarcodePrint
	{
		get
		{
			return m_bBarcodePrint;
		}
		set
		{
			m_bBarcodePrint = value;
		}
	}

	public List<int> NonDefaultRow
	{
		get
		{
			return m_arrNonDefaultRow;
		}
		set
		{
			m_arrNonDefaultRow = value;
		}
	}

	private void InitPreferences(InvoicePreferences m_oPrintPreferences)
	{
		m_iDefaultCurrencyId = m_oPrintPreferences.Misc_DefaultCurrencyId;
		m_iInvTag = m_oPrintPreferences.Tag_Inventory;
		m_iFaTag = m_oPrintPreferences.Tag_Accounts;
		IsPrintZeroValueAsNumeric = m_oPrintPreferences.Print_PrintZeroValueAsNumeric > 0;
		IsNumericSeparator = m_oPrintPreferences.Misc_NumericSeparator > 0;
		PrintBooleanAs = (byte)m_oPrintPreferences.Print_PrintBooleanAs;
		m_bMillionSeperator = m_oPrintPreferences.Misc_NumericSeparator > 0;
		IsIgnoreDayInExpiry = m_oPrintPreferences.Batch_IgnoreDaysInExpiry > 0;
		m_oCurrDetail = (CurrencyDetail)CallServeRequest(RDMethods.LoadCurrencyData, m_iDefaultCurrencyId);
	}

	public void GetTranObject()
	{
		_ = string.Empty;
		_ = string.Empty;
		PageBodyClass objLayoutBody = null;
		PageBody[] arrBodyColumns = null;
		IdNamePair[] arrGroupData = null;
		List<HeaderGroup> list = null;
		List<IdValuePair> arrBodySequence = null;
		IdNamePair[] arrGroupDataHIUS = null;
		List<IdValuePair> arrBodySequenceHIUS = null;
		List<TransBody> arrTransDataHIUS = null;
		List<TransBody> arrTransData = null;
		List<IdValuePair> list2 = null;
		int iBodyCount = 0;
		int iGroupingType = -1;
		m_bPaymentTerm = false;
		if (m_layoutInfo == null && m_iLayoutId > 0)
		{
			m_layoutInfo = (LayoutInformation)CallServeRequest(RDMethods.LoadPrintInvoiceLayout, m_iLayoutId);
		}
		if (m_layoutInfo.Pages != null && m_layoutInfo.Pages.Length != 0)
		{
			m_arrExternalModuleDataAll = GetExtenalModuledataAll(m_layoutInfo.Pages[0].PageHeader);
		}
		if (m_iVoucherType == 0)
		{
			if (m_objTranData != null && m_objTranData.Header != null)
			{
				m_iVoucherType = m_objTranData.Header.VoucherType;
			}
			else if (m_layoutInfo.ReportId > 0)
			{
				m_iVoucherType = m_layoutInfo.ReportId;
			}
		}
		ArrangeDataByHideItemUnderSet(ref arrGroupDataHIUS, ref arrBodySequenceHIUS, ref arrTransDataHIUS);
		m_bIsAppliedHIUS = arrGroupDataHIUS != null && arrGroupDataHIUS.Length != 0;
		list2 = GetGroupByBody(m_layoutInfo, ref iBodyCount);
		if (iBodyCount > 1 && list2 != null && list2.Count > 0)
		{
			List<IdValuePair> list3 = new List<IdValuePair>();
			TransBody[] bodyData = m_objTranData.BodyData;
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i].Value != null && list2[i].Value.GetType() == typeof(PageHeader))
				{
					m_bClubByWithCondition = IsConditionWithClubBy(m_layoutInfo);
					ArrangeGroupingData(m_layoutInfo, ref arrGroupData, ref iGroupingType, ref objLayoutBody, ref arrBodySequence, ref arrTransData, ref arrBodyColumns, (PageHeader)list2[i].Value);
					AdjustDataByHIUS(arrGroupDataHIUS, ref arrGroupData, arrBodySequenceHIUS, ref arrBodySequence, arrTransDataHIUS, arrTransData);
					list3.Add(new IdValuePair(list2[i].ID, GetVoucherFields("Body", InvoiceFieldType.Body, arrGroupData, objLayoutBody, iGroupingType, m_layoutInfo != null && m_layoutInfo.IsSuspendNet, arrBodySequence, arrBodyColumns)));
					m_arrGroupedData = arrGroupData;
				}
				m_bClubByWithCondition = false;
				arrGroupData = null;
				iGroupingType = 0;
				objLayoutBody = null;
				arrBodySequence = null;
				arrTransData = null;
				arrBodyColumns = null;
				m_bFormulaAdded = false;
				if (list2.Count > 1 && !IsConditionWithClubBy(m_layoutInfo))
				{
					m_objTranData.BodyData = bodyData;
				}
			}
			if (list3.Count > 0)
			{
				m_arrBodyGroupClubOrderBy = list3;
				if (!IsConditionWithClubBy(m_layoutInfo) && !FConvert.IsItTransfer(m_objTranData.Header.VoucherType))
				{
					m_objTranData.BodyData = bodyData;
					bodyData = null;
				}
			}
		}
		else
		{
			m_bClubByWithCondition = IsConditionWithClubBy(m_layoutInfo);
			ArrangeGroupingData(m_layoutInfo, ref arrGroupData, ref iGroupingType, ref objLayoutBody, ref arrBodySequence, ref arrTransData, ref arrBodyColumns);
			AdjustDataByHIUS(arrGroupDataHIUS, ref arrGroupData, arrBodySequenceHIUS, ref arrBodySequence, arrTransDataHIUS, arrTransData);
			m_arrGroupedData = arrGroupData;
		}
		if (m_arrHeaderGroup == null)
		{
			m_arrHeaderGroup = new List<HeaderGroup>();
		}
		if (m_layoutInfo != null && m_layoutInfo.SubReportId == 1)
		{
			m_arrHeaderGroup.AddRange(GetBarcodeFields());
		}
		m_arrHeaderGroup.AddRange(GetVoucherFields("Header", InvoiceFieldType.Header));
		m_arrBodyGroup = GetVoucherFields("Body", InvoiceFieldType.Body, arrGroupData, objLayoutBody, iGroupingType, m_layoutInfo != null && m_layoutInfo.IsSuspendNet, arrBodySequence, arrBodyColumns);
		if (m_layoutInfo != null && m_layoutInfo.AttachDocuments != null && m_layoutInfo.AttachDocuments.Length != 0)
		{
			m_arrDocumentData = GetAttachedDocument(GetAttachmentFields(m_arrHeaderGroup, m_arrBodyGroup), m_arrHeaderGroup, m_arrBodyGroup);
		}
		if (m_bPaymentTerm)
		{
			list = GetVoucherFields("Payment terms fields", InvoiceFieldType.PaymentTerms);
			m_arrBodyGroup.AddRange(list);
		}
		list = GetBaseVoucherFields(bLoadFields: false);
		if (list != null)
		{
			m_arrBodyGroup.AddRange(list);
		}
		list = GetVoucherFields("Reference fields", InvoiceFieldType.Reference);
		if (list != null)
		{
			m_arrBodyGroup.AddRange(list);
		}
		if (m_bLinkToLC)
		{
			list = GetVoucherFields("LC fields", InvoiceFieldType.LetterOfCredit);
			m_arrBodyGroup.AddRange(list);
		}
		if (m_iVoucherType == 3331)
		{
			m_arrBodyGroup.AddRange(GetVoucherFields("Voucher specific", InvoiceFieldType.POS));
		}
		m_arrBodyGroup.AddRange(GetVoucherFields("User details", InvoiceFieldType.User));
		m_arrBodyGroup.AddRange(GetCompanyFields((m_objTranData == null) ? "Company fields" : "Company", 4));
		ChangeBodyDataIfAppliedClubBy(arrBodySequence);
		m_arrMiscellaneousValue = GetCustomerAgeingValues(m_layoutInfo);
	}

	private object CallServeRequest(RDMethods oMethod, params object[] arrParams)
	{
		InvoiceLayout invoiceLayout = null;
		switch (oMethod)
		{
		case RDMethods.LoadCurrencyData:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.LoadCurrencyData(Convert.ToInt32(arrParams[0]), m_iCompanyId);
		case RDMethods.EvaluateInvoiceFormula:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.EvaluateInvoiceFormula((string)arrParams[0], (int)arrParams[1], (Transaction)arrParams[2], (StaticTextClass)arrParams[3], (string[])arrParams[4], m_iCompanyId);
		case RDMethods.GetMasterValueFromField:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			if (arrParams[0].GetType() == typeof(FieldInfoInput))
			{
				FieldInfoInput fieldInfoInput = (FieldInfoInput)arrParams[0];
				return invoiceLayout.GetMasterValueFromField(fieldInfoInput.FieldId, fieldInfoInput.SubParentId, fieldInfoInput.MasterIds, fieldInfoInput.AltLanguageId, m_iCompanyId);
			}
			return invoiceLayout.GetMasterValueFromField((int)arrParams[0], (int)arrParams[1], (string)arrParams[2], (int)arrParams[3], m_iCompanyId);
		case RDMethods.GetMasterValuesFromFields:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			if (arrParams[0].GetType() == typeof(FieldInfoInput))
			{
				return invoiceLayout.GetMastersValueFromFields((FieldInfoInput)arrParams[0], m_iCompanyId);
			}
			return invoiceLayout.GetMastersValueFromFields((int)arrParams[0], (int)arrParams[1], (string)arrParams[2], (int)arrParams[3], m_iCompanyId);
		case RDMethods.LoadPrintInvoiceLayout:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.LoadPrintingInvoice((int)arrParams[0], m_iCompanyId);
		case RDMethods.GetUsersContactDetails:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetUsersContactDetails((int)arrParams[0], (int)arrParams[1], m_iCompanyId);
		case RDMethods.GetOrderByVoucher:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetOrderByVoucher((OrderByInput)arrParams[0], m_iCompanyId);
		case RDMethods.GetAgeingDetails:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetAgeingDetails((AccountAgeingInput)arrParams[0], m_iCompanyId);
		case RDMethods.GetFieldName:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetMasterFieldName(Convert.ToInt64(arrParams[0]), (int)arrParams[1], (int)arrParams[2], m_iCompanyId);
		case RDMethods.GetLCDetails:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetLCDetails((string)arrParams[0], m_iCompanyId);
		case RDMethods.GetMasterImageFields:
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			return invoiceLayout.GetMasterImageFields((int[])arrParams[0], m_iCompanyId);
		case RDMethods.GetDocumentViewFieldData:
		{
			invoiceLayout = new InvoiceLayout();
			invoiceLayout.m_objCalType = m_objCalType;
			List<IdNamePair> list = new List<IdNamePair>();
			if (arrParams[0].GetType() == typeof(IdValuePair))
			{
				list.AddRange((IdNamePair[])((IdValuePair)arrParams[0]).Value);
			}
			else
			{
				list.AddRange((IdNamePair[])arrParams[0]);
			}
			return invoiceLayout.GetDocumentViewFieldData(list.ToArray(), m_iCompanyId);
		}
		case RDMethods.DonotPrintProductForZeroRate:
			return new RDReport
			{
				m_objCalType = m_objCalType
			}.DonotPrintProductForZeroRate((List<IdNamePair>)arrParams[0], m_iCompanyId);
		default:
			return null;
		}
	}

	private object CallServeRequestTrans(TransMethods oMethod, params object[] arrParams)
	{
		return oMethod switch
		{
			TransMethods.LoadVoucherById => (object)new Focus.Transactions.BL.Transactions
			{
				m_objCalType = m_objCalType
			}.LoadVoucherById((int)arrParams[0], (long)arrParams[1], (LoadTransactionBy)arrParams[2], m_iCompanyId, m_iUserId), 
			TransMethods.LoadVoucher => new Focus.Transactions.BL.Transactions
			{
				m_objCalType = m_objCalType
			}.LoadVoucher((int)arrParams[0], (string)arrParams[1], m_iCompanyId, m_iUserId), 
			TransMethods.GetDocFields => new VoucherWiz
			{
				m_objCalType = m_objCalType
			}.GetDocFields((int)arrParams[0], m_iCompanyId, m_iUserId, m_iLanguageId), 
			TransMethods.CheckTransaction => new Focus.Transactions.BL.Transactions
			{
				m_objCalType = m_objCalType
			}.GetConditions((Transaction)arrParams[1], (AdvanceFilterQuery[])arrParams[0], m_iCompanyId), 
			TransMethods.GetPaymentTermDetails => new VoucherPopups
			{
				m_objCalType = m_objCalType
			}.GetPaymentTermDetails((int)arrParams[0], m_iCompanyId), 
			TransMethods.GetAccountNames => new VoucherPopups
			{
				m_objCalType = m_objCalType
			}.GetAccountNames((int[])arrParams[0], m_iCompanyId), 
			TransMethods.LoadPreference => new Focus.TranSettings.BL.Preference().LoadPreference(m_iCompanyId), 
			_ => null, 
		};
	}

	private object CallServeRequestMaster(MastersMethods oMethod, params object[] arrParams)
	{
		string strErrMsg = null;
		if (oMethod == MastersMethods.GetCompanyOtherFields)
		{
			m_arrCompanyLogo = GetCompanyLogo(m_iCompanyId, ref strErrMsg);
			return GetCompanyOtherFields(m_iCompanyId, ref strErrMsg);
		}
		return null;
	}

	private CompanyExtraField[] GetCompanyOtherFields(int iCompanyID, ref string strErrMsg)
	{
		DbCommand dbCommand = null;
		Database database = DatabaseWrapper.GetDatabase2(iCompanyID);
		List<CompanyExtraField> list = new List<CompanyExtraField>();
		try
		{
			string text = string.Format("declare @RowCount int\r\n                            declare @LoopCount int\r\n                            set @LoopCount=1\r\n                            declare @table table (iRow int identity(1,1), iLanguageId int,sDescription varchar(100))\r\n                            insert into @table\r\n                            select iLanguageId ,sDescription from cCore_LanguageSupported  With (ReadUncommitted) \r\n                            set @RowCount =@@rowcount\r\n                            --select * from @table\r\n                            declare @LangString varchar(100)\r\n                            set @LangString=''\r\n                            declare @desc varchar(100)\r\n                            while @LoopCount < = @RowCount\r\n                            begin\r\n                             select @desc=sDescription from @table where iRow=@LoopCount\r\n                             set @LangString=@LangString+@desc+','\r\n                             set @LoopCount=@LoopCount+1\r\n                            end\r\n                             declare @Currency varchar(100)\r\n                            declare @DefLang varchar(100)\r\n\t\t\t\t\t        select @DefLang=sDescription from cCore_LanguageSupported With (ReadUncommitted)   where bDefault=1\r\n                            select @Currency=sName from mCore_Currency With (ReadUncommitted)  where iCurrencyId=(select iValue from cCore_PreferenceVal_{0} With (ReadUncommitted)  where iCategory=4 and iFieldId=10)\r\n                            select sCompanyName,iAccountingDate,pLogo,isnull(c.sName,'') iCountryId,isnull(@Currency,'') iCurrency,@DefLang DefLang,substring(@LangString,0,len(@LangString)) SupLang\r\n                            from mCore_Company With (ReadUncommitted) \r\n                            join tCore_Company_Details  With (ReadUncommitted) on tCore_Company_Details.iCompanyId=mCore_Company.iCompanyId\r\n\t                        left join mCore_Country c  With (ReadUncommitted) on c.iMasterId=tCore_Company_Details.iCountryId\r\n                            where iYearId={0}", 0);
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			using (IDataReader dataReader = database.ExecuteReader(dbCommand))
			{
				if (dataReader.Read())
				{
					CompanyExtraField companyExtraField = new CompanyExtraField();
					companyExtraField.FieldName = "Company Name";
					companyExtraField.Value = dataReader.GetString(0);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
					companyExtraField = new CompanyExtraField();
					companyExtraField.DataType = 4;
					companyExtraField.FieldName = "Accounting Date";
					companyExtraField.Value = dataReader.GetInt32(1);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
					companyExtraField = new CompanyExtraField();
					companyExtraField.DataType = 0;
					companyExtraField.FieldName = "Country";
					companyExtraField.Value = dataReader.GetString(3);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
					companyExtraField = new CompanyExtraField();
					companyExtraField.DataType = 0;
					companyExtraField.FieldName = "Currency";
					companyExtraField.Value = dataReader.GetString(4);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
					companyExtraField = new CompanyExtraField();
					companyExtraField.DataType = 0;
					companyExtraField.FieldName = "DefaultLang";
					companyExtraField.Value = dataReader.GetString(5);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
					companyExtraField = new CompanyExtraField();
					companyExtraField.FieldName = "SupportedLang";
					companyExtraField.Value = dataReader.GetString(6);
					companyExtraField.TabCaption = "General";
					list.Add(companyExtraField);
				}
			}
			text = $"select iFieldId,sCaption,iDataTypeId,isnull(iLinkMasterId,0)iLinkMasterId,isnull(sExternalTableName,'')sExternalTableName,sFieldName from cCore_OtherMasterFields OMF With (ReadUncommitted) \r\n                           join cCore_OtherMasterTables OMT  With (ReadUncommitted) on OMT.iTableId=OMF.iTableId";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			using (IDataReader dataReader2 = database.ExecuteReader(dbCommand))
			{
				while (dataReader2.Read())
				{
					CompanyExtraField companyExtraField2 = new CompanyExtraField();
					companyExtraField2.FieldId = dataReader2.GetInt32(0);
					companyExtraField2.FieldName = dataReader2.GetString(1);
					companyExtraField2.Value = getFieldData(dataReader2.GetString(5), "vmCore_OtherCompanyField", database);
					companyExtraField2.DataType = dataReader2.GetInt32(2);
					companyExtraField2.LinkMasterId = dataReader2.GetInt32(3);
					companyExtraField2.LinkExtTableName = dataReader2.GetString(4);
					list.Add(companyExtraField2);
				}
			}
			return list.ToArray();
		}
		catch (Exception ex)
		{
			strErrMsg = ex.Message;
			return list.ToArray();
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State == ConnectionState.Open)
			{
				dbCommand.Connection.Close();
			}
		}
	}

	private object getFieldData(string sFieldName, string sTableName, Database objdb)
	{
		object result = null;
		DbCommand dbCommand = null;
		try
		{
			string text = $"select {sFieldName} from {sTableName} With (ReadUncommitted)";
			dbCommand = objdb.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			using IDataReader dataReader = objdb.ExecuteReader(dbCommand);
			if (dataReader.Read())
			{
				result = dataReader.GetValue(0);
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			if (dbCommand != null && dbCommand.Connection != null && dbCommand.Connection.State == ConnectionState.Open)
			{
				dbCommand.Connection.Close();
			}
		}
		return result;
	}

	private byte[] GetCompanyLogo(int iCompanyID, ref string strErrMsg)
	{
		DbCommand dbCommand = null;
		byte[] result = null;
		string text = "";
		Database database = DatabaseWrapper.GetDatabase2(iCompanyID);
		int yearId = FConvert.GetYearId(iCompanyID);
		try
		{
			text = $"select pLogo from mCore_Company With(Readuncommitted)\r\n                                    join tCore_Company_Details With(Readuncommitted) on tCore_Company_Details.iCompanyId=mCore_Company.iCompanyId\r\n                                    where iYearId={yearId}";
			dbCommand = database.GetSqlStringCommand(PostgreSQL.bUsePostgreSql ? PostgreSQL.ConvertSqlToPostgre(text) : text);
			dbCommand.CommandTimeout = 0;
			using IDataReader dataReader = database.ExecuteReader(dbCommand);
			while (dataReader.Read())
			{
				result = dataReader["pLogo"] as byte[];
			}
			return result;
		}
		catch (Exception ex)
		{
			strErrMsg = ex.Message;
			return null;
		}
		finally
		{
			if (dbCommand.Connection != null && dbCommand.Connection.State == ConnectionState.Open)
			{
				dbCommand.Connection.Close();
			}
		}
	}

	private object CallServeRequestCommon(CommonMethods oMethod, params object[] arrParams)
	{
		return oMethod switch
		{
			CommonMethods.LoadBarCode => (object)new Filter().LoadBarCode(m_iCompanyId), 
			CommonMethods.GetBarcodeTextTrans => new Filter().GetBarcodeTextTrans(new Transaction[1] { (Transaction)arrParams[0] }, m_iCompanyId), 
			_ => null, 
		};
	}

	private bool IsConditionWithClubBy(LayoutInformation oLayoutInfo)
	{
		if (oLayoutInfo != null && oLayoutInfo.Pages != null && oLayoutInfo.Pages.Length != 0 && oLayoutInfo.Pages[0].PageHeader != null && oLayoutInfo.Pages[0].PageHeader.Length != 0)
		{
			PageHeader[] pageHeader = oLayoutInfo.Pages[0].PageHeader;
			foreach (PageHeader pageHeader2 in pageHeader)
			{
				if (pageHeader2.UID > 2000 || pageHeader2.PageBodyClass == null || pageHeader2.PageBodyClass.ClubByOption == null || pageHeader2.PageBodyClass.ClubByOption.ClubBy == null || pageHeader2.PageBodyClass.ClubByOption.ClubBy.Length == 0)
				{
					continue;
				}
				for (int j = 0; j < pageHeader2.PageBody.Length; j++)
				{
					if (pageHeader2.PageBody[j].Condition != null && pageHeader2.PageBody[j].Condition.Length != 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private List<IdValuePair> GetExtenalModuledataAll(PageHeader[] arrPageHeader)
	{
		Focus.Common.DataStructs.Scheduler scheduler = new Focus.Common.DataStructs.Scheduler();
		List<IdValuePair> list = new List<IdValuePair>();
		object[,] array = null;
		for (int i = 0; i < arrPageHeader.Length; i++)
		{
			if (arrPageHeader[i].PageBodyClass != null && !string.IsNullOrEmpty(arrPageHeader[i].PageBodyClass.ExternalModuleNamespace) && !string.IsNullOrEmpty(arrPageHeader[i].PageBodyClass.ExternalModuleClass) && !string.IsNullOrEmpty(arrPageHeader[i].PageBodyClass.ExternalModuleFunction))
			{
				array = (object[,])scheduler.DynamicDllFunctionInvoke(arrPageHeader[i].PageBodyClass.ExternalModuleNamespace.Contains(".dll") ? arrPageHeader[i].PageBodyClass.ExternalModuleNamespace : (arrPageHeader[i].PageBodyClass.ExternalModuleNamespace + ".dll"), arrPageHeader[i].PageBodyClass.ExternalModuleClass + "." + arrPageHeader[i].PageBodyClass.ExternalModuleFunction, m_objTranData, arrPageHeader[i].PageBody);
				list.Add(new IdValuePair(arrPageHeader[i].UID, array));
			}
		}
		return list;
	}

	private void ArrangeDataByHideItemUnderSet(ref IdNamePair[] arrGroupDataHIUS, ref List<IdValuePair> arrBodySequenceHIUS, ref List<TransBody> arrTransDataHIUS)
	{
		int num = -1;
		int num2 = 0;
		int num3 = 0;
		arrTransDataHIUS = new List<TransBody>();
		List<IdNamePair> list = new List<IdNamePair>();
		List<TransBody> list2 = null;
		if (!FConvert.IsItSales(m_iVoucherType) || m_layoutInfo == null || m_objTranData == null || m_layoutInfo.Pages.Length == 0)
		{
			return;
		}
		PageHeader[] pageHeader = m_layoutInfo.Pages[0].PageHeader;
		foreach (PageHeader pageHeader2 in pageHeader)
		{
			if (pageHeader2.UID > 2000 || pageHeader2.PageBodyClass == null)
			{
				continue;
			}
			if (!pageHeader2.PageBodyClass.IsHideItemUnderItemSet)
			{
				break;
			}
			if (m_objTranData.BodyData.Where((TransBody p) => p.Sales != null && p.Sales.Product == p.Sales.SetId).ToArray().Count() > 0)
			{
				m_objTranData.BodyData = m_objTranData.BodyData.Where((TransBody p) => p.RowType != _RowType.Default || p.Sales.Product == p.Sales.SetId || p.Sales.SetId == 0).ToArray();
				return;
			}
			int[] array = (from p in m_objTranData.BodyData
				where p.RowType == _RowType.Default && p.Sales != null && p.Sales.SetId > 0
				group p by p.Sales.SetId into p
				select p.Key into p
				orderby p
				select p).ToArray();
			if (array == null || m_objTranData == null)
			{
				break;
			}
			list2 = new List<TransBody>();
			arrBodySequenceHIUS = new List<IdValuePair>();
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				for (num2 = 0; num2 < m_objTranData.BodyData.Length; num2++)
				{
					if (m_objTranData.BodyData[num2].RowType != _RowType.Default)
					{
						continue;
					}
					num3 = m_objTranData.BodyData[num2].Sales.SetId;
					if (array[num4] == num3)
					{
						if (num != array[num4])
						{
							list.Add(new IdNamePair(list2.Count, "", "HIUS TYPE"));
							arrBodySequenceHIUS.Add(new IdValuePair(list2.Count, num2));
							num = array[num4];
						}
						list2.Add(m_objTranData.BodyData[num2]);
					}
				}
			}
			if (list2.Count > 0)
			{
				arrTransDataHIUS = list2;
			}
			break;
		}
		arrGroupDataHIUS = list.ToArray();
	}

	private List<IdValuePair> GetGroupByBody(LayoutInformation oLayoutInfo, ref int iBodyCount)
	{
		List<IdValuePair> list = new List<IdValuePair>();
		if (oLayoutInfo != null && oLayoutInfo.Pages != null && oLayoutInfo.Pages.Length != 0 && oLayoutInfo.Pages[0].PageHeader != null && oLayoutInfo.Pages[0].PageHeader.Length != 0)
		{
			PageHeader pageHeader = null;
			for (int i = 0; i < oLayoutInfo.Pages.Length; i++)
			{
				for (int j = 0; j < oLayoutInfo.Pages[i].PageHeader.Length; j++)
				{
					pageHeader = oLayoutInfo.Pages[i].PageHeader[j];
					if (pageHeader.UID <= 2000 && pageHeader.PageBodyClass != null)
					{
						iBodyCount++;
						if ((pageHeader.PageBodyClass.OrderBy != null && pageHeader.PageBodyClass.OrderBy.Length != 0) || (pageHeader.PageBodyClass.GroupByOption != null && pageHeader.PageBodyClass.GroupByOption.GroupBy != null && pageHeader.PageBodyClass.GroupByOption.GroupBy.Length != 0) || (pageHeader.PageBodyClass.ClubByOption != null && pageHeader.PageBodyClass.ClubByOption.ClubBy != null && pageHeader.PageBodyClass.ClubByOption.ClubBy.Length != 0))
						{
							list.Add(new IdValuePair(pageHeader.UID, pageHeader));
						}
					}
				}
			}
		}
		return list;
	}

	private TransBody[] FilterProducts()
	{
		List<TransBody> list = null;
		IdNamePair[] array = null;
		list = new List<TransBody>();
		if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0 && m_objTranData.BodyData[0].Sales != null)
		{
			if (m_layoutInfo == null && m_iLayoutId > 0)
			{
				m_layoutInfo = (LayoutInformation)CallServeRequest(RDMethods.LoadPrintInvoiceLayout, m_iLayoutId);
			}
			array = (IdNamePair[])CallServeRequest(RDMethods.DonotPrintProductForZeroRate, (from p in m_objTranData.BodyData
				where p.RowType == _RowType.Default
				select new IdNamePair(p.Sales.Product, p.BodyId.ToString(), p.Sales.Rate)).ToList());
			list.AddRange(m_objTranData.BodyData.OrderBy((TransBody p) => p.RowType));
			if (array != null && array.Length != list.Count)
			{
				m_arrNonDefaultRow = new List<int>();
				for (int num = 0; num < list.Count; num++)
				{
					int num2;
					for (num2 = 0; num2 < array.Length && !(list[num].BodyId.ToString() == array[num2].Name); num2++)
					{
					}
					if (num2 >= array.Length)
					{
						m_arrNonDefaultRow.Add(num);
					}
				}
			}
		}
		else
		{
			list.AddRange(m_objTranData.BodyData.OrderBy((TransBody p) => p.RowType));
			m_arrNonDefaultRow = new List<int>();
			for (int num = 0; num < list.Count; num++)
			{
				if (list[num].RowType != _RowType.Default)
				{
					m_arrNonDefaultRow.Add(num);
				}
			}
		}
		return list.ToArray();
	}

	private List<HeaderGroup> GetVoucherFields(string sHeader, InvoiceFieldType iGroupIdType, IdNamePair[] arrOrderedData = null, PageBodyClass objLayoutBody = null, int iGroupingType = -1, bool bDontIncludeSuspendAmount = false, List<IdValuePair> arrBodySequence = null, PageBody[] arrBodyColumns = null)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		HeaderGroup headerGroup = null;
		TemplateFields objField = null;
		TranMasterInfo tranMasterInfo = null;
		List<HeaderGroup> list = null;
		List<TemplateFields> list2 = null;
		List<object> list3 = null;
		List<object> arrRateValues = null;
		List<IdNamePair> list4 = null;
		int num = 0;
		List<IdNamePair> list5 = new List<IdNamePair>();
		list = new List<HeaderGroup>();
		if (m_iVoucherType == 0)
		{
			if (m_objTranData != null && m_objTranData.Header != null)
			{
				m_iVoucherType = m_objTranData.Header.VoucherType;
			}
			else if (m_layoutInfo.ReportId > 0)
			{
				m_iVoucherType = m_layoutInfo.ReportId;
			}
		}
		short num2 = (short)(m_iVoucherType & Convert.ToUInt16(VTTYPE.VOUCHERMASK));
		if (m_arrDefaultFields == null)
		{
			m_arrDefaultFields = GetDocFields(m_iVoucherType, bLinkVoucher: false);
			list4 = new List<IdNamePair>();
			for (num = 0; num < m_arrDefaultFields.Length; num++)
			{
				if ((num2 == 4096 || num2 == 3840) && m_arrDefaultFields[num].ID == 4 && ((TranMasterInfo)m_arrDefaultFields[num].Tag).MasterType == 1)
				{
					m_arrDefaultFields[num].Name = ((num2 == 4096) ? "Cr Account" : "Dr Account");
				}
				if (m_arrDefaultFields[num].ID == 13 && !FConvert.IsItInwardVoucher(num2))
				{
					flag = true;
				}
				if (m_arrDefaultFields[num].ID == 85)
				{
					flag2 = true;
					if (!FConvert.IsItInwardVoucher(num2))
					{
						flag4 = true;
					}
				}
				if (m_arrDefaultFields[num].ID == 84)
				{
					flag3 = true;
				}
				if ((m_arrDefaultFields[num].ID & Convert.ToInt32(VTFIELDFLAG.SCR_BODY)) == Convert.ToInt32(VTFIELDFLAG.SCR_BODY))
				{
					IdNamePair item = new IdNamePair(m_arrDefaultFields[num].ID * -1, m_arrDefaultFields[num].Name + " Value", m_arrDefaultFields[num].Tag);
					list4.Add(item);
				}
			}
			IdNamePair[] arrDefaultFields = m_arrDefaultFields;
			if (arrDefaultFields != null && arrDefaultFields.Length != 0)
			{
				Array.Resize(ref m_arrDefaultFields, m_arrDefaultFields.Length + 8);
				m_arrDefaultFields[m_arrDefaultFields.Length - 8] = new IdNamePair(47, "Created by", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 0,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 7] = new IdNamePair(179, "Created by name", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 0,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 6] = new IdNamePair(79, "Created date", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 4,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 5] = new IdNamePair(80, "Created time", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 5,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 4] = new IdNamePair(78, "Modified by", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 0,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 3] = new IdNamePair(180, "Modified by name", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 0,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 2] = new IdNamePair(81, "Modified date", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 4,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 1] = new IdNamePair(82, "Modified time", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 5,
					MasterType = 0,
					Filter = string.Empty
				});
				Array.Resize(ref m_arrDefaultFields, m_arrDefaultFields.Length + 5 + (flag ? 2 : 0));
				m_arrDefaultFields[m_arrDefaultFields.Length - 5] = new IdNamePair(75, "Net", new TranMasterInfo
				{
					IsBodyField = true,
					DataType = 6,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 4] = new IdNamePair(75, "Net (base currency)", new TranMasterInfo
				{
					IsBodyField = true,
					DataType = 6,
					MasterType = -1,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 3] = new IdNamePair(75, "Net (local currency)", new TranMasterInfo
				{
					IsBodyField = true,
					DataType = 6,
					MasterType = -2,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 2] = new IdNamePair(116, "Print count", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 1,
					MasterType = 0,
					Filter = string.Empty
				});
				m_arrDefaultFields[m_arrDefaultFields.Length - 1] = new IdNamePair(117, "Revision number", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 1,
					MasterType = 0,
					Filter = string.Empty
				});
				if (flag)
				{
					m_arrDefaultFields[m_arrDefaultFields.Length - 6] = new IdNamePair(36, "Manufacturing date", new TranMasterInfo
					{
						IsBodyField = true,
						DataType = 0,
						MasterType = 0,
						Filter = string.Empty
					});
					m_arrDefaultFields[m_arrDefaultFields.Length - 7] = new IdNamePair(37, "Expiry date", new TranMasterInfo
					{
						IsBodyField = true,
						DataType = 0,
						MasterType = 0,
						Filter = string.Empty
					});
				}
				list5.AddRange(m_arrDefaultFields);
				list5.Add(new IdNamePair(95, "Authorize status", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 0,
					MasterType = 0,
					Filter = string.Empty
				}));
				list5.Add(new IdNamePair(164, "Authorize date", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 4,
					MasterType = 0,
					Filter = string.Empty
				}));
				list5.Add(new IdNamePair(171, "Voucher net", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 6,
					MasterType = 0,
					Filter = string.Empty
				}));
				if (m_arrDefaultFields.Where((IdNamePair p) => p.ID == 35).Count() > 0)
				{
					IdNamePair idNamePair = (from p in m_arrDefaultFields
						where p.ID == 35
						select (p)).FirstOrDefault();
					list5.Add(new IdNamePair(143, "Base doc no", idNamePair.Tag));
				}
				m_arrDefaultFields = list5.ToArray();
				list5.Clear();
				if (flag4)
				{
					list5.AddRange(m_arrDefaultFields);
					list5.Add(new IdNamePair(130, "Bin Skid no", new TranMasterInfo
					{
						IsBodyField = true,
						DataType = 12,
						MasterType = 0,
						Filter = string.Empty
					}));
					m_arrDefaultFields = list5.ToArray();
					list5.Clear();
				}
				if (flag2)
				{
					list5.AddRange(m_arrDefaultFields);
					list5.Add(new IdNamePair(195, "Carton number", new TranMasterInfo
					{
						IsBodyField = true,
						DataType = 0,
						MasterType = 0,
						Filter = string.Empty
					}));
					m_arrDefaultFields = list5.ToArray();
					list5.Clear();
				}
				if (flag3)
				{
					list5.AddRange(m_arrDefaultFields);
					list5.Add(new IdNamePair(196, "Carton number-2", new TranMasterInfo
					{
						IsBodyField = true,
						DataType = 0,
						MasterType = 0,
						Filter = string.Empty
					}));
					m_arrDefaultFields = list5.ToArray();
					list5.Clear();
				}
				if (list4.Count > 0)
				{
					list5.AddRange(m_arrDefaultFields);
					list5.AddRange(list4);
					m_arrDefaultFields = list5.ToArray();
					list5.Clear();
				}
				list5.AddRange(m_arrDefaultFields);
				list5.Add(new IdNamePair(166, "Email count", new TranMasterInfo
				{
					IsBodyField = false,
					DataType = 1,
					MasterType = 0,
					Filter = string.Empty
				}));
				m_arrDefaultFields = list5.ToArray();
				list5.Clear();
			}
		}
		headerGroup = new HeaderGroup();
		headerGroup.MasterId = (int)iGroupIdType;
		headerGroup.GroupName = sHeader;
		list2 = new List<TemplateFields>();
		switch (iGroupIdType)
		{
		case InvoiceFieldType.User:
		{
			if (m_objTranData == null)
			{
				break;
			}
			UserContactDetails[] array = null;
			if (m_bPrintUnAuthorizeDocument)
			{
				array = (UserContactDetails[])CallServeRequest(RDMethods.GetUsersContactDetails, m_objTranData.Header.HeaderId, m_iUserId, m_iCompanyId);
			}
			IdNamePair[] lCFields = m_objTransactionPrintCommon.GetUserSpecificFields(array != null && array.Length > 1);
			foreach (IdNamePair idNamePair5 in lCFields)
			{
				objField = new TemplateFields();
				objField.FieldId = idNamePair5.ID;
				objField.Name = idNamePair5.Name;
				if (idNamePair5.Name.Contains("Employee"))
				{
					objField.DataType = MasterDataType.Master;
					objField.MasterType = 800;
				}
				else if (idNamePair5.Name.ToLower().Contains("signature"))
				{
					objField.DataType = MasterDataType.Picture;
				}
				objField.Values = m_objTransactionPrintCommon.GetUserSpecificValues(idNamePair5.ID, array, m_iUserId, m_objTranData);
				list2.Add(objField);
			}
			break;
		}
		case InvoiceFieldType.POS:
		{
			IdNamePair[] lCFields = m_objTransactionPrintCommon.GetVoucherSpecificFields();
			foreach (IdNamePair idNamePair6 in lCFields)
			{
				objField = new TemplateFields();
				objField.FieldId = idNamePair6.ID;
				objField.Name = idNamePair6.Name;
				objField.Values = GetVoucherSpecificValues(idNamePair6.ID);
				list2.Add(objField);
			}
			break;
		}
		case InvoiceFieldType.Reference:
			if (m_objTranData != null && m_objTranData.Header != null && m_objTranData.Header.Flags != null && (m_objTranData.Header.Flags.UpdateFA || (m_objTranData.Header.VoucherType & 0xFF00) == 5888 || (m_objTranData.Header.VoucherType & 0xFF00) == 7168))
			{
				IdNamePair[] lCFields = m_objTransactionPrintCommon.GetReferenceFields();
				foreach (IdNamePair idNamePair4 in lCFields)
				{
					objField = new TemplateFields();
					objField.FieldId = idNamePair4.ID;
					objField.Name = idNamePair4.Name;
					objField.DataType = (MasterDataType)idNamePair4.Tag;
					objField.Values = GetReferenceValues(idNamePair4.ID);
					list2.Add(objField);
				}
				break;
			}
			return null;
		case InvoiceFieldType.LetterOfCredit:
		{
			List<LCDetailsForReport> lstLCDetails = null;
			if (m_objTranData != null)
			{
				lstLCDetails = (List<LCDetailsForReport>)CallServeRequest(RDMethods.GetLCDetails, m_objTranData.Header.DocNo);
			}
			IdNamePair[] lCFields = m_objTransactionPrintCommon.GetLCFields();
			foreach (IdNamePair idNamePair2 in lCFields)
			{
				objField = new TemplateFields();
				objField.FieldId = idNamePair2.ID;
				objField.Name = idNamePair2.Name;
				objField.DataType = (MasterDataType)idNamePair2.Tag;
				if (objField.DataType == MasterDataType.Master)
				{
					objField.MasterType = 1;
				}
				objField.Values = m_objTransactionPrintCommon.GetLCValues(lstLCDetails, idNamePair2.ID);
				list2.Add(objField);
			}
			break;
		}
		case InvoiceFieldType.PaymentTerms:
		{
			List<PaymentTermForReport> lstPmtTerms = null;
			if (m_objTranData != null)
			{
				lstPmtTerms = (List<PaymentTermForReport>)CallServeRequestTrans(TransMethods.GetPaymentTermDetails, m_objTranData.Header.PmtTerm);
			}
			IdNamePair[] lCFields = m_objTransactionPrintCommon.GetPaymentTermFields();
			foreach (IdNamePair idNamePair3 in lCFields)
			{
				objField = new TemplateFields();
				objField.FieldId = idNamePair3.ID;
				objField.Name = idNamePair3.Name;
				objField.DataType = (MasterDataType)idNamePair3.Tag;
				objField.Values = m_objTransactionPrintCommon.GetPaymentTermsValues(lstPmtTerms, idNamePair3.ID);
				list2.Add(objField);
			}
			break;
		}
		case InvoiceFieldType.Header:
		case InvoiceFieldType.Body:
			IsDontConvertField = true;
			for (num = 0; num < m_arrDefaultFields.Length; num++)
			{
				list3 = new List<object>();
				objField = new TemplateFields();
				objField.FieldId = m_arrDefaultFields[num].ID;
				objField.Name = m_arrDefaultFields[num].Name;
				tranMasterInfo = (TranMasterInfo)m_arrDefaultFields[num].Tag;
				objField.DataType = (MasterDataType)tranMasterInfo.DataType;
				objField.MasterType = tranMasterInfo.MasterType;
				if (objField.FieldId == 37 || objField.FieldId == 36 || objField.FieldId == 85 || objField.FieldId == 84)
				{
					objField.DataType = MasterDataType.Text;
				}
				if (objField.DataType == MasterDataType.Master && objField.MasterType > 1 && objField.FieldId < 5000 && objField.FieldId != 24 && m_arrMasters != null)
				{
					string text = (from p in m_arrMasters
						where p.ID == objField.MasterType
						select p.Name).FirstOrDefault();
					if (m_arrDefaultFields[num].Name.Contains("~InvTag~"))
					{
						objField.Name = m_arrDefaultFields[num].Name.Replace("~InvTag~", text);
					}
					else
					{
						objField.Name = text;
					}
				}
				IsDontConvertField = true;
				if (objField.DataType == MasterDataType.NumberList)
				{
					objField.MasterType = -1;
					IsDontConvertField = false;
				}
				if (objField.FieldId == 32)
				{
					m_bPaymentTerm = true;
				}
				if (!tranMasterInfo.IsBodyField && iGroupIdType == InvoiceFieldType.Header)
				{
					if (objField.FieldId == 32 || objField.FieldId == 47 || objField.FieldId == 78 || objField.FieldId == 179 || objField.FieldId == 180)
					{
						objField.DataType = MasterDataType.Text;
					}
					if (m_objTranData != null)
					{
						if (objField.FieldId == 10 || objField.FieldId == 32 || objField.FieldId == 47 || objField.FieldId == 78 || objField.FieldId == 179 || objField.FieldId == 180)
						{
							IsDontConvertField = false;
							if (objField.FieldId == 47 || objField.FieldId == 78)
							{
								IsMasterCode = true;
							}
						}
						if (objField.FieldId == 8 || objField.FieldId == 167)
						{
							IsDontConvertField = false;
							objField.DataType = MasterDataType.Text;
						}
						list3.Add(GetHeaderValue(objField.FieldId, objField.DataType, objField.MasterType, m_objTranData));
						IsDontConvertField = true;
						IsMasterCode = false;
					}
					else
					{
						list3.Add("0");
					}
					objField.Values = list3.ToArray();
					list2.Add(objField);
				}
				else
				{
					if (!tranMasterInfo.IsBodyField)
					{
						continue;
					}
					if ((num2 == 8448 || num2 == 4096 || num2 == 3840) && m_arrDefaultFields[num].ID == 12)
					{
						objField.Name = "Account2";
					}
					if (!m_bFormulaAdded)
					{
						list2.Add(m_objTransactionPrintCommon.AddFormulaField(m_objTranData));
						m_bFormulaAdded = true;
					}
					if (iGroupIdType != InvoiceFieldType.Body)
					{
						continue;
					}
					if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
					{
						int num3 = 0;
						int num4 = 0;
						int num5 = 0;
						for (int num6 = 0; num6 < m_objTranData.BodyData.Length; num6++)
						{
							if (m_objTranData.BodyData[num6].RowType != _RowType.Default && ((m_objTranData.BodyData[num6].RowType != _RowType.AccountToPost && m_objTranData.BodyData[num6].RowType != _RowType.Appropriated) || objField.FieldId != 75))
							{
								continue;
							}
							if (objField.FieldId == 75 && m_objTranData.BodyData[num6].Flags.SuspendFA && m_objTranData.BodyData[num6].AuthStatus == _AuthStatus.Authorized)
							{
								list3.Add(0);
								continue;
							}
							if (objField.FieldId == 75 && m_objTranData.BodyData[0].Book != m_objTranData.BodyData[num6].Book && FConvert.IsItSales(num2))
							{
								list3.Add(0);
								continue;
							}
							if (arrOrderedData == null && iGroupingType == -1)
							{
								if (FConvert.IsItTransfer(m_objTranData.Header.VoucherType))
								{
									if (m_iOrderByBins > 0)
									{
										if (num5 >= m_iOrderByBins / 2)
										{
											continue;
										}
									}
									else
									{
										int num7 = m_objTranData.BodyData.Where((TransBody p) => p.RowType == _RowType.Default).Count();
										if (num6 >= num7 / 2)
										{
											continue;
										}
									}
								}
								if (m_objTranData.BodyData[num6].Sales != null && !m_objTransactionPrintCommon.IsPrintModiferProduct(m_objTranData.BodyData[num6].Sales.POSData, m_objTranData.BodyData[num6].Sales.Rate, m_layoutInfo.ModifierOption))
								{
									list3.Add(0);
									continue;
								}
							}
							if ((m_objTranData.BodyData[num6].Flags.SuspendFA & bDontIncludeSuspendAmount) && objField.FieldId == 75)
							{
								list3.Add(0);
								continue;
							}
							if (objField.FieldId == 26 && m_objTranData.BodyData[num6].Sales != null && m_objTranData.BodyData[num6].Sales.POSData != null && m_objTranData.BodyData[num6].Sales.POSData.ModifierId > 0)
							{
								if (m_objTranData != null && !m_layoutInfo.IsDonotPrintQuantityforModifiers)
								{
									list3.Add(GetBodyValue(objField.FieldId, objField.DataType, objField.MasterType, m_objTranData, num6, num4, m_iOrderByBins));
								}
								else
								{
									list3.Add(string.Empty);
								}
								continue;
							}
							if (num3 != m_objTranData.BodyData[num6].BodyId)
							{
								num4 = 0;
								num5++;
							}
							else
							{
								num4++;
							}
							list3.Add(GetBodyValue(objField.FieldId, objField.DataType, objField.MasterType, m_objTranData, num6, num4, m_iOrderByBins));
							num3 = m_objTranData.BodyData[num6].BodyId;
						}
					}
					else
					{
						list3.Add("0");
					}
					if (arrOrderedData != null && !m_bClubByWithCondition)
					{
						if (iGroupingType == 1)
						{
							ArrangeGroupByData(arrOrderedData, objLayoutBody, objField.FieldId, objField.DataType, arrBodySequence, arrBodyColumns, ref list3);
						}
						else
						{
							ArrangeClubByData(arrOrderedData, objLayoutBody, objField.FieldId, objField.Name, objField.DataType, arrRateValues, arrBodySequence, arrBodyColumns, ref list3);
						}
					}
					objField.Values = list3.ToArray();
					list2.Add(objField);
				}
			}
			break;
		}
		headerGroup.Fields = list2.ToArray();
		list.Add(headerGroup);
		return list;
	}

	private void ArrangeGroupByData(IdNamePair[] arrOrderedData, PageBodyClass objBodyClass, int iFieldId, MasterDataType objType, List<IdValuePair> arrBodySequence, PageBody[] arrBodyColumns, ref List<object> arrValues)
	{
		decimal[] array = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		if (objBodyClass.GroupByOption == null || (!objBodyClass.GroupByOption.IsPrintGroupHeading && !objBodyClass.GroupByOption.IsPrintSubTotal))
		{
			return;
		}
		if (objType == MasterDataType.Fraction && objBodyClass.GroupByOption.IsPrintSubTotal)
		{
			array = new decimal[arrOrderedData.Length];
			for (num = 0; num < arrOrderedData.Length; num++)
			{
				if (iFieldId != 27)
				{
					num3 = ((num < arrOrderedData.Length - 1) ? arrOrderedData[num + 1].ID : arrValues.Count);
					for (num2 = arrOrderedData[num].ID; num2 < num3; num2++)
					{
						array[num] += Convert.ToDecimal(arrValues[num2]);
					}
				}
			}
		}
		for (num = 0; num < arrOrderedData.Length; num++)
		{
			if (m_arrNonDefaultRow != null && m_arrNonDefaultRow.Count > 0)
			{
				m_arrNonDefaultRow.Clear();
			}
			if (objType == MasterDataType.Fraction)
			{
				if (objBodyClass.GroupByOption.IsPrintSubTotal)
				{
					arrValues.Insert(arrOrderedData[num].ID + num, $"<B>{array[num]}");
				}
				else
				{
					arrValues.Insert(arrOrderedData[num].ID + num, 0);
				}
			}
			else if (objBodyClass.GroupByOption.IsPrintGroupHeading && arrOrderedData[num].ID + num < arrValues.Count)
			{
				arrValues.Insert(arrOrderedData[num].ID + num, "<B>" + arrOrderedData[num].Name);
			}
			else
			{
				arrValues.Insert(arrOrderedData[num].ID + num, string.Empty);
			}
		}
	}

	private void ArrangeClubByData(IdNamePair[] arrOrderedData, PageBodyClass objBodyClass, int iFieldId, string sFieldName, MasterDataType objType, List<object> arrRateValues, List<IdValuePair> arrBodySequence, PageBody[] arrBodyColumns, ref List<object> arrValues)
	{
		decimal[] array = null;
		string[] array2 = null;
		List<IdValuePair> list = null;
		int num = 0;
		int i = 0;
		int num2 = 0;
		string text = "";
		if (objBodyClass.ClubByOption == null)
		{
			return;
		}
		if (objType == MasterDataType.Fraction || objType == MasterDataType.Number)
		{
			array = new decimal[arrOrderedData.Length];
			for (num = 0; num < arrOrderedData.Length; num++)
			{
				List<decimal> list2 = new List<decimal>();
				text = "";
				num2 = ((num < arrOrderedData.Length - 1) ? arrOrderedData[num + 1].ID : arrValues.Count);
				int num3 = ((num != 0) ? arrOrderedData[num].ID : 0);
				if (iFieldId == 27 && arrOrderedData[num].Tag != null)
				{
					text = Convert.ToString(arrOrderedData[num].Tag);
				}
				for (i = num3; i < num2; i++)
				{
					if (i < arrValues.Count)
					{
						if (string.IsNullOrEmpty(Convert.ToString(arrValues[i])))
						{
							arrValues[i] = 0;
						}
						list2.Add(Convert.ToDecimal(arrValues[i]));
					}
				}
				array[num] = 0m;
				if (list2.Count > 0)
				{
					switch (GetColumnFunctionType(iFieldId, sFieldName, arrBodyColumns, text))
					{
					case InvoiceFunction.Avg:
						array[num] = list2.Sum() / (decimal)list2.Count;
						break;
					case InvoiceFunction.Count:
						array[num] = list2.Count;
						break;
					case InvoiceFunction.Max:
						array[num] = list2.Max();
						break;
					case InvoiceFunction.Min:
						array[num] = list2.Min();
						break;
					default:
						array[num] = list2.Sum();
						break;
					}
				}
			}
		}
		else if (iFieldId == 13)
		{
			array2 = new string[arrOrderedData.Length];
			for (num = 0; num < arrOrderedData.Length; num++)
			{
				List<string> list3 = new List<string>();
				num2 = ((num < arrOrderedData.Length - 1) ? arrOrderedData[num + 1].ID : arrValues.Count);
				for (i = arrOrderedData[num].ID; i < num2; i++)
				{
					if (i < arrValues.Count)
					{
						list3.Add(Convert.ToString(arrValues[i]));
					}
				}
				array2[num] = string.Join(",", list3.Distinct().ToArray());
			}
		}
		list = new List<IdValuePair>();
		for (num = 0; num < arrOrderedData.Length; num++)
		{
			if (objType == MasterDataType.Fraction || objType == MasterDataType.Number)
			{
				list.Add(new IdValuePair(arrOrderedData[num].ID, array[num]));
			}
			else
			{
				if (arrOrderedData[num].ID >= arrValues.Count)
				{
					continue;
				}
				if (iFieldId == 23 || iFieldId == 12)
				{
					if (arrOrderedData[num].Name == "$")
					{
						list.Add(new IdValuePair(arrOrderedData[num].ID, $"<B>{arrValues[arrOrderedData[num].ID]}~{arrValues[arrOrderedData[num].ID]}"));
					}
					else
					{
						list.Add(new IdValuePair(arrOrderedData[num].ID, $"<B>{arrOrderedData[num].Name}~{arrValues[arrOrderedData[num].ID]}"));
					}
					continue;
				}
				if (sFieldName.ToLower() == "set name")
				{
					list.Add(new IdValuePair(arrOrderedData[num].ID, $"{arrValues[arrOrderedData[num].ID]}"));
					continue;
				}
				switch (iFieldId)
				{
				case 24:
					list.Add(new IdValuePair(arrOrderedData[num].ID, $"{arrValues[arrOrderedData[num].ID]}"));
					continue;
				case 13:
					if (array2 != null && num < array2.Length)
					{
						list.Add(new IdValuePair(arrOrderedData[num].ID, array2[num]));
						continue;
					}
					break;
				}
				if ((objType == MasterDataType.Text || objType == MasterDataType.Date || (objType == MasterDataType.Master && (int)m_arrNewLevelsOrderBy[i].Value != (iFieldId & 0xFFFFFF))) && num < arrValues.Count)
				{
					list.Add(new IdValuePair(arrOrderedData[num].ID, $"{arrValues[arrOrderedData[num].ID]}"));
				}
				else
				{
					if (m_arrNewLevelsOrderBy == null)
					{
						continue;
					}
					for (i = 0; i < m_arrNewLevelsOrderBy.Count; i++)
					{
						bool flag = false;
						if ((objType != MasterDataType.Master) ? (m_arrNewLevelsOrderBy[i].ID == iFieldId || m_arrNewLevelsOrderBy[i].ID == (iFieldId & 0xFFFFFF)) : ((int)m_arrNewLevelsOrderBy[i].Value == iFieldId || (int)m_arrNewLevelsOrderBy[i].Value == (iFieldId & 0xFFFFFF)))
						{
							list.Add(new IdValuePair(arrOrderedData[num].ID, arrOrderedData[num].Name));
							break;
						}
					}
				}
			}
		}
		if (m_iOrderByCount > 1)
		{
			arrValues = list.Select((IdValuePair p) => p.Value).ToList();
			return;
		}
		arrValues = (from p in list
			join q in arrBodySequence on p.ID equals q.ID
			orderby q.Value
			select p.Value).ToList();
	}

	private InvoiceFunction GetColumnFunctionType(int iFieldId, string sFieldName, PageBody[] arrBodyColumn, string sRowType)
	{
		if (arrBodyColumn != null)
		{
			int num = 0;
			if (iFieldId == 27 && sRowType != "HIUS TYPE")
			{
				return InvoiceFunction.Avg;
			}
			for (num = 0; num < arrBodyColumn.Length; num++)
			{
				if (iFieldId == arrBodyColumn[num].FieldId)
				{
					return arrBodyColumn[num].FunctionType;
				}
			}
			if (!string.IsNullOrEmpty(sFieldName))
			{
				for (num = 0; num < arrBodyColumn.Length; num++)
				{
					string text = arrBodyColumn[num].Column;
					string[] array = text.Split('.');
					if (array.Length == 2)
					{
						text = array[1];
					}
					if (sFieldName == text)
					{
						return arrBodyColumn[num].FunctionType;
					}
				}
			}
		}
		return InvoiceFunction.Sum;
	}

	private void AdjustDataByHIUS(IdNamePair[] arrGroupDataHIUS, ref IdNamePair[] arrGroupData, List<IdValuePair> arrBodySequenceHIUS, ref List<IdValuePair> arrBodySequence, List<TransBody> arrTransDataHIUS, List<TransBody> arrTransData)
	{
		List<TransBody> list = new List<TransBody>();
		List<IdNamePair> list2 = new List<IdNamePair>();
		if (arrGroupDataHIUS != null && arrGroupDataHIUS.Length != 0)
		{
			list.AddRange(arrTransDataHIUS);
			list2.AddRange(arrGroupDataHIUS);
			if (arrGroupData != null && arrGroupData.Length != 0)
			{
				for (int i = 0; i < arrGroupData.Length; i++)
				{
					arrGroupData[i].ID = arrGroupData[i].ID + list.Count;
				}
				for (int j = 0; j < arrBodySequence.Count; j++)
				{
					arrBodySequence[j].ID = arrBodySequence[j].ID + list.Count;
				}
				list2.AddRange(arrGroupData);
				arrBodySequenceHIUS.AddRange(arrBodySequence);
				list.AddRange(arrTransData);
			}
			else
			{
				for (int k = 0; k < m_objTranData.BodyData.Length; k++)
				{
					if (m_objTranData.BodyData[k].RowType == _RowType.Default && m_objTranData.BodyData[k].Sales.SetId == 0)
					{
						list2.Add(new IdNamePair(list.Count, "$"));
						arrBodySequenceHIUS.Add(new IdValuePair(list.Count, k));
						list.Add(m_objTranData.BodyData[k]);
					}
				}
			}
			arrGroupData = list2.ToArray();
			arrBodySequence = arrBodySequenceHIUS;
			m_objTranData.BodyData = list.ToArray();
		}
		else if (arrTransData != null && arrTransData.Count > 0)
		{
			m_objTranData.BodyData = arrTransData.ToArray();
		}
	}

	private IdNamePair[] OrderByData(OrderByData[] arrOrderByData, ref Transaction objTranData, ref List<TransBody> arrTransData, bool IsDontClubIfRateDifferent, bool IsDontClubIfUnitPresent, ref List<IdValuePair> arrBodySequence, int iOrderByCount, bool IsInventory, bool bGenerateHierarchy)
	{
		string text = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<IdNamePair> list = null;
		List<TransBody> list2 = null;
		list = new List<IdNamePair>();
		if (arrOrderByData != null && objTranData != null)
		{
			string[] array = null;
			string[] array2 = null;
			if (iOrderByCount > 1)
			{
				m_iOrderByCount = arrOrderByData.Length;
				if (!bGenerateHierarchy)
				{
					for (num = 0; num < arrOrderByData.Length; num++)
					{
						array2 = arrOrderByData[num].Particulars.Split(',');
						arrOrderByData[num].Particulars = string.Format("{1}~{0}", arrOrderByData[num].Name, array2[0]);
					}
				}
				iOrderByCount = 1;
			}
			list2 = new List<TransBody>();
			arrBodySequence = new List<IdValuePair>();
			array2 = ((iOrderByCount > 1) ? new string[iOrderByCount] : null);
			for (num = 0; num < arrOrderByData.Length; num++)
			{
				for (num2 = 0; num2 < objTranData.BodyData.Length; num2++)
				{
					if (objTranData.BodyData[num2].RowType != _RowType.Default)
					{
						continue;
					}
					num3 = objTranData.BodyData[num2].BodyId;
					if (arrOrderByData[num].BodyId != num3)
					{
						continue;
					}
					bool flag = true;
					int num4 = 0;
					bool flag2 = false;
					if (iOrderByCount > 1)
					{
						array = arrOrderByData[num].Particulars.Split(',');
						if (array2.Length == array.Length)
						{
							for (num4 = 0; num4 < array2.Length; num4++)
							{
								if (array2[num4] != array[num4])
								{
									if (array2.Length > num4 && array2[num4] != array[num4])
									{
										for (int i = num4 + 1; i < array2.Length; i++)
										{
											array2[i] = null;
										}
									}
									flag2 = true;
									array2[num4] = array[num4];
									break;
								}
								array2[num4] = array[num4];
							}
							num4 = ((num4 != array2.Length) ? num4 : 0);
						}
						arrOrderByData[num].Particulars = (string.IsNullOrEmpty(array[num4]) ? array[0] : array[num4]);
						num4 = 0;
					}
					else
					{
						flag2 = text != arrOrderByData[num].Particulars;
					}
					if (flag2)
					{
						if (bGenerateHierarchy)
						{
							array = arrOrderByData[num].Particulars.Split(',');
							list.Add(new IdNamePair(list2.Count, array[0]));
						}
						else
						{
							list.Add(new IdNamePair(list2.Count, arrOrderByData[num].Particulars));
						}
						arrBodySequence.Add(new IdValuePair(list2.Count, num2));
						text = arrOrderByData[num].Particulars;
					}
					else
					{
						flag = false;
						if (IsDontClubIfUnitPresent && objTranData.BodyData[num2].Sales != null)
						{
							for (num4 = list2.Count - 1; num4 >= 0; num4--)
							{
								if (objTranData.BodyData[num2].Sales.Unit != list2[num4].Sales.Unit && arrOrderByData[num].ID == (IsInventory ? list2[num4].Sales.Product : list2[num4].Code))
								{
									flag = true;
								}
								else if (objTranData.BodyData[num2].Sales.Unit == list2[num4].Sales.Unit && arrOrderByData[num].ID == (IsInventory ? list2[num4].Sales.Product : list2[num4].Code))
								{
									flag = false;
									break;
								}
							}
						}
						if (IsDontClubIfRateDifferent && objTranData.BodyData[num2].Sales != null)
						{
							for (num4 = list2.Count - 1; num4 >= 0; num4--)
							{
								if (objTranData.BodyData[num2].Sales.Rate != list2[num4].Sales.Rate && arrOrderByData[num].ID == (IsInventory ? list2[num4].Sales.Product : list2[num4].Code))
								{
									flag = true;
								}
								else if (objTranData.BodyData[num2].Sales.Rate == list2[num4].Sales.Rate && arrOrderByData[num].ID == (IsInventory ? list2[num4].Sales.Product : list2[num4].Code))
								{
									flag = false;
									break;
								}
							}
						}
						if (flag)
						{
							list.Add(new IdNamePair(list2.Count, arrOrderByData[num].Particulars));
							arrBodySequence.Add(new IdValuePair(list2.Count, num2));
						}
					}
					if (flag)
					{
						list2.Add(objTranData.BodyData[num2]);
						continue;
					}
					if (!IsDontClubIfRateDifferent && !IsDontClubIfUnitPresent)
					{
						list2.Add(objTranData.BodyData[num2]);
						continue;
					}
					if (num4 == -1)
					{
						list2.Add(objTranData.BodyData[num2]);
						continue;
					}
					list2.Insert(num4 + 1, objTranData.BodyData[num2]);
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].ID >= num4 + 1)
						{
							list[j].ID++;
						}
					}
					for (int k = 0; k < arrBodySequence.Count; k++)
					{
						if (arrBodySequence[k].ID >= num4 + 1)
						{
							arrBodySequence[k].ID++;
						}
					}
				}
			}
			if (list2.Count > 0)
			{
				arrTransData = list2;
				arrTransData.AddRange(objTranData.BodyData.Where((TransBody p) => p.RowType != _RowType.Default).ToArray());
			}
		}
		return list.ToArray();
	}

	private void ArrangeGroupingData(LayoutInformation oLayoutInfo, ref IdNamePair[] arrGroupData, ref int iGroupingType, ref PageBodyClass objLayoutBody, ref List<IdValuePair> arrBodySequence, ref List<TransBody> arrTransData, ref PageBody[] arrBodyColumns, PageHeader oBodyControl = null)
	{
		bool flag = FConvert.IsItSales(m_iVoucherType);
		bool isDontClubIfRateDifferent = false;
		bool bGenerateHierarchy = false;
		OrderByData[] array = null;
		List<int> list = null;
		m_arrNewLevelsOrderBy = null;
		List<int> list2 = null;
		bool flag2 = false;
		if (m_objTranData == null)
		{
			return;
		}
		if (oBodyControl != null)
		{
			objLayoutBody = oBodyControl.PageBodyClass;
			arrBodyColumns = oBodyControl.PageBody;
			if (oBodyControl.PageBodyClass.OrderBy != null && oBodyControl.PageBodyClass.OrderBy.Length != 0)
			{
				iGroupingType = 0;
				list = new List<int>();
				m_arrNewLevelsOrderBy = new List<IdValuePair>();
				IdNamePair[] orderBy = oBodyControl.PageBodyClass.OrderBy;
				foreach (IdNamePair idNamePair in orderBy)
				{
					list.Add(idNamePair.ID);
					m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair.ID, idNamePair.Tag));
				}
			}
			if (oBodyControl.PageBodyClass.GroupByOption != null && oBodyControl.PageBodyClass.GroupByOption.GroupBy != null && oBodyControl.PageBodyClass.GroupByOption.GroupBy.Length != 0)
			{
				bGenerateHierarchy = oBodyControl.PageBodyClass.GroupByOption.GenerateHierarchy;
				iGroupingType = 1;
				list = new List<int>();
				m_arrNewLevelsOrderBy = new List<IdValuePair>();
				IdNamePair[] orderBy = oBodyControl.PageBodyClass.GroupByOption.GroupBy;
				foreach (IdNamePair idNamePair2 in orderBy)
				{
					list.Add(idNamePair2.ID);
					m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair2.ID, idNamePair2.Tag));
				}
			}
			if (oBodyControl.PageBodyClass.ClubByOption != null && oBodyControl.PageBodyClass.ClubByOption.ClubBy != null && oBodyControl.PageBodyClass.ClubByOption.ClubBy.Length != 0)
			{
				iGroupingType = 2;
				list = new List<int>();
				m_arrNewLevelsOrderBy = new List<IdValuePair>();
				IdNamePair[] orderBy = oBodyControl.PageBodyClass.ClubByOption.ClubBy;
				foreach (IdNamePair idNamePair3 in orderBy)
				{
					list.Add(idNamePair3.ID);
					m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair3.ID, idNamePair3.Tag));
				}
			}
		}
		else if (oLayoutInfo != null && oLayoutInfo.Pages != null && oLayoutInfo.Pages.Length != 0 && oLayoutInfo.Pages[0].PageHeader != null && oLayoutInfo.Pages[0].PageHeader.Length != 0)
		{
			PageHeader[] pageHeader = oLayoutInfo.Pages[0].PageHeader;
			foreach (PageHeader pageHeader2 in pageHeader)
			{
				if (pageHeader2.UID > 2000 || pageHeader2.PageBodyClass == null)
				{
					continue;
				}
				objLayoutBody = pageHeader2.PageBodyClass;
				arrBodyColumns = pageHeader2.PageBody;
				if (pageHeader2.PageBodyClass.OrderBy != null && pageHeader2.PageBodyClass.OrderBy.Length != 0)
				{
					iGroupingType = 0;
					list = new List<int>();
					m_arrNewLevelsOrderBy = new List<IdValuePair>();
					IdNamePair[] orderBy = pageHeader2.PageBodyClass.OrderBy;
					foreach (IdNamePair idNamePair4 in orderBy)
					{
						list.Add(idNamePair4.ID);
						m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair4.ID, idNamePair4.Tag));
					}
				}
				if (pageHeader2.PageBodyClass.GroupByOption != null && pageHeader2.PageBodyClass.GroupByOption.GroupBy != null && pageHeader2.PageBodyClass.GroupByOption.GroupBy.Length != 0)
				{
					iGroupingType = 1;
					list = new List<int>();
					m_arrNewLevelsOrderBy = new List<IdValuePair>();
					IdNamePair[] orderBy = pageHeader2.PageBodyClass.GroupByOption.GroupBy;
					foreach (IdNamePair idNamePair5 in orderBy)
					{
						list.Add(idNamePair5.ID);
						m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair5.ID, idNamePair5.Tag));
					}
				}
				if (pageHeader2.PageBodyClass.ClubByOption != null && pageHeader2.PageBodyClass.ClubByOption.ClubBy != null && pageHeader2.PageBodyClass.ClubByOption.ClubBy.Length != 0)
				{
					iGroupingType = 2;
					list = new List<int>();
					m_arrNewLevelsOrderBy = new List<IdValuePair>();
					IdNamePair[] orderBy = pageHeader2.PageBodyClass.ClubByOption.ClubBy;
					foreach (IdNamePair idNamePair6 in orderBy)
					{
						list.Add(idNamePair6.ID);
						m_arrNewLevelsOrderBy.Add(new IdValuePair(idNamePair6.ID, idNamePair6.Tag));
					}
				}
				if (list != null && list.Count > 0)
				{
					break;
				}
			}
		}
		if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
		{
			list2 = new List<int>();
			int num = -1;
			TransBody[] bodyData = m_objTranData.BodyData;
			foreach (TransBody transBody in bodyData)
			{
				list2.Add(transBody.BodyId);
				if (transBody.Sales != null && transBody.Sales.Unit > 0)
				{
					flag2 = num > 0 && num != transBody.Sales.Unit;
					num = transBody.Sales.Unit;
				}
			}
		}
		if (list2 != null && list2.Count > 0 && list != null && list.Count > 0)
		{
			OrderByInput orderByInput = new OrderByInput();
			orderByInput.HeaderId = m_objTranData.Header.HeaderId;
			orderByInput.VoucherType = m_objTranData.Header.VoucherType;
			orderByInput.FieldId = m_arrNewLevelsOrderBy.ToArray();
			array = (OrderByData[])CallServeRequest(RDMethods.GetOrderByVoucher, orderByInput);
			flag = (!flag || Convert.ToInt32(m_arrNewLevelsOrderBy[0].Value) != 1) && flag;
			flag2 = Convert.ToInt32(m_arrNewLevelsOrderBy[0].Value) == 2 && flag2;
		}
		if (FConvert.IsItTransfer(m_iVoucherType) || iGroupingType < 2)
		{
			flag2 = (isDontClubIfRateDifferent = false);
		}
		switch (iGroupingType)
		{
		case 0:
			if (array != null && array.Length != 0)
			{
				OrderByData(array, ref m_objTranData, ref arrTransData, isDontClubIfRateDifferent, flag2, ref arrBodySequence, 1, flag, bGenerateHierarchy);
			}
			break;
		case 1:
		case 2:
			if (array == null || array.Length == 0)
			{
				break;
			}
			if (FConvert.IsItSales(m_iVoucherType) && iGroupingType == 2)
			{
				isDontClubIfRateDifferent = objLayoutBody != null && objLayoutBody.ClubByOption != null && FConvert.IsItSales(m_iVoucherType) && objLayoutBody.ClubByOption.IsDontClubIfRateDifferent;
			}
			arrGroupData = OrderByData(array, ref m_objTranData, ref arrTransData, isDontClubIfRateDifferent, flag2, ref arrBodySequence, list.Count, flag, bGenerateHierarchy);
			if (m_arrGroupingRow == null && iGroupingType == 1)
			{
				m_arrGroupingRow = new List<int>();
				for (int k = 0; k < arrGroupData.Length; k++)
				{
					m_arrGroupingRow.Add(arrGroupData[k].ID + k);
				}
			}
			break;
		}
		m_iOrderByBins = 0;
		if (arrTransData != null && arrTransData.Count > m_objTranData.BodyData.Length)
		{
			m_iOrderByBins = m_objTranData.BodyData.Length;
		}
	}

	private List<IdNamePair> GetAttachmentFields(List<HeaderGroup> lstHeaderGrp, List<HeaderGroup> lstBodyGrp)
	{
		List<IdNamePair> list = new List<IdNamePair>();
		List<int> list2 = new List<int>();
		new List<TemplateFields>();
		List<TemplateFields> totalFields = GetTotalFields(lstHeaderGrp, lstBodyGrp);
		if (m_arrDefaultFields != null)
		{
			list = m_arrDefaultFields.Where((IdNamePair p) => (p.Tag as TranMasterInfo).DataType == 10 || (p.Tag as TranMasterInfo).DataType == 7).ToList();
		}
		list2 = (from p in totalFields
			where p.DataType == MasterDataType.Master
			select p.MasterType).ToList();
		if (m_iVoucherType > 0)
		{
			list2.Add(1);
			if (FConvert.IsItSales(m_iVoucherType))
			{
				list2.Add(2);
			}
		}
		if (list2.Count > 0)
		{
			list.AddRange((IdNamePair[])CallServeRequest(RDMethods.GetMasterImageFields, list2.ToArray()));
		}
		return list;
	}

	private List<IdNamePair> GetAttachedDocument(List<IdNamePair> arrAttFields, List<HeaderGroup> lstHeaderGrp, List<HeaderGroup> lstBodyGrp)
	{
		List<TemplateFields> list = new List<TemplateFields>();
		List<IdNamePair> list2 = null;
		List<IdNamePair> list3 = new List<IdNamePair>();
		List<object> list4 = new List<object>();
		if (m_objTranData != null)
		{
			list2 = new List<IdNamePair>();
			if (m_objTranData.HeaderDocs != null && m_objTranData.HeaderDocs.Length != 0)
			{
				for (int i = 0; i < m_objTranData.HeaderDocs.Length; i++)
				{
					if ((m_objTranData.HeaderDocs[i].Tag as IdNamePair).Name != string.Empty)
					{
						list2.Add(new IdNamePair(m_objTranData.HeaderDocs[i].ID, (m_objTranData.HeaderDocs[i].Tag as IdNamePair).Name, (m_objTranData.HeaderDocs[i].Tag as IdNamePair).Tag));
					}
				}
			}
			if (m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
			{
				for (int j = 0; j < m_objTranData.BodyData.Length; j++)
				{
					if (m_objTranData.BodyData[j].BodyExtra == null || m_objTranData.BodyData[j].BodyExtra.Length == 0 || m_objTranData.BodyData[j].RowType != _RowType.Default)
					{
						continue;
					}
					for (int k = 0; k < m_objTranData.BodyData[j].BodyExtra.Length; k++)
					{
						if (m_objTranData.BodyData[j].BodyExtra[k].Tag != null && m_objTranData.BodyData[j].BodyExtra[k].Tag.GetType() == typeof(DocumentData))
						{
							list2.Add(new IdNamePair(m_objTranData.BodyData[j].BodyExtra[k].ID, ((DocumentData)m_objTranData.BodyData[j].BodyExtra[k].Tag).FileName, ((DocumentData)m_objTranData.BodyData[j].BodyExtra[k].Tag).FileData));
						}
					}
				}
			}
			list = GetTotalFields(lstHeaderGrp, lstBodyGrp);
			if (arrAttFields != null && arrAttFields.Count > 0)
			{
				foreach (IdNamePair obj in arrAttFields)
				{
					if (obj.Tag.GetType() == typeof(int) && (from p in list
						where p.MasterType == Convert.ToInt32(obj.Tag)
						select p.Values).ToList().Count > 0)
					{
						list4 = (from p in list
							where p.MasterType == Convert.ToInt32(obj.Tag)
							select p.Values).First().ToList();
						list3.Add(new IdNamePair(obj.ID, list4));
					}
				}
			}
			IdValuePair idValuePair = null;
			if (list3.Count > 0)
			{
				for (int num = 0; num < list3.Count; num++)
				{
					list3[num].Tag = string.Join(",", ((List<object>)list3[num].Tag).ToArray());
				}
				idValuePair = new IdValuePair();
				idValuePair.Value = list3.ToArray();
				list2.AddRange((IdNamePair[])CallServeRequest(RDMethods.GetDocumentViewFieldData, idValuePair));
			}
		}
		return list2;
	}

	private object GetHeaderValue(int iFieldId, MasterDataType objType, int iMasterType, Transaction objTran)
	{
		int iIndex = 0;
		switch ((VTFIELDID)iFieldId)
		{
		case VTFIELDID.FDF_PRINTCOUNT:
			return objTran.Header.PrintCount;
		case VTFIELDID.FDF_EMAILCOUNT:
			return objTran.Header.EmailCount;
		case VTFIELDID.FDF_AUTH_STATUS:
			if (objTran.AuthData != null && objTran.AuthData.Length != 0)
			{
				for (iIndex = 0; iIndex < objTran.AuthData.Length; iIndex++)
				{
					if (objTran.AuthData[iIndex].Status == 1)
					{
						continue;
					}
					if (objTran.AuthData[iIndex].Status != 0)
					{
						if (objTran.AuthData[iIndex].Status != 2)
						{
							if (objTran.AuthData[iIndex].Status != 3)
							{
								if (objTran.AuthData[iIndex].Status != 4)
								{
									if (objTran.AuthData[iIndex].Status != 1)
									{
										return string.Empty;
									}
									return "Authorized";
								}
								return "Undo authorization";
							}
							return "Stopped";
						}
						return "Rejected";
					}
					return "Pending";
				}
			}
			else if (objTran.Header.HeaderId == 0 || objTran.BodyData != null)
			{
				for (iIndex = 0; iIndex < objTran.BodyData.Length; iIndex++)
				{
					if (objTran.BodyData[iIndex].AuthStatus == _AuthStatus.Authorized)
					{
						continue;
					}
					if (objTran.BodyData[iIndex].AuthStatus != _AuthStatus.Pending)
					{
						if (objTran.BodyData[iIndex].AuthStatus != _AuthStatus.Rejected)
						{
							if (objTran.BodyData[iIndex].AuthStatus != _AuthStatus.Stopped)
							{
								if (objTran.BodyData[iIndex].AuthStatus != _AuthStatus.Edited)
								{
									if (objTran.BodyData[iIndex].AuthStatus != _AuthStatus.Authorized)
									{
										return string.Empty;
									}
									return "Authorized";
								}
								return "Undo authorization";
							}
							return "Stopped";
						}
						return "Rejected";
					}
					return "Pending";
				}
			}
			return "Authorized";
		case VTFIELDID.FDF_AUTH_DATE:
			if (objTran.AuthData != null && objTran.AuthData.Length != 0)
			{
				for (iIndex = 0; iIndex < objTran.AuthData.Length; iIndex++)
				{
					if (objTran.AuthData[iIndex].Status != 1)
					{
						return string.Empty;
					}
				}
			}
			return ConvertValue(objTran.Header.AuthDate, objType, iMasterType);
		case VTFIELDID.FDF_REVISION_NO:
			return (objTran.Header.Versions != null && objTran.Header.Versions.Length != 0) ? objTran.Header.Versions.Length : 0;
		case VTFIELDID.FDF_DOCUMENTNO:
			return objTran.Header.DocNo;
		case VTFIELDID.FDF_DATE:
			return ConvertValue(objTran.Header.Date, objType, iMasterType);
		case VTFIELDID.FDF_VOUCHER_AMOUNT:
			return objTran.Header.Net;
		case VTFIELDID.FDF_ENTRY_ID:
		case VTFIELDID.FDF_CREATED_BY_NAME:
			return GetMasterValue(-5, objTran.Header.EnteredBy);
		case VTFIELDID.FDF_CREATED_DATE:
			return ConvertValue(objTran.Header.CreatedDate, objType, iMasterType);
		case VTFIELDID.FDF_TIME:
		{
			int iDate2 = new Date(m_objCalType).GetToday(m_objCalType).Value;
			int iTime2 = objTran.Header.CreatedTime;
			FConvert.TranslateTimeZone(ref iDate2, ref iTime2, m_dClientOffset, m_objCalType);
			if (iTime2 != objTran.Header.CreatedTime)
			{
				return FConvert.IntToStringTime(iTime2);
			}
			objTran.Header.CreatedTime = iTime2;
			return ConvertValue(objTran.Header.CreatedTime, objType, iMasterType);
		}
		case VTFIELDID.FDF_MODIFIED_BY:
		case VTFIELDID.FDF_MODIFIED_BY_NAME:
			return GetMasterValue(-5, objTran.Header.ModifiedBy);
		case VTFIELDID.FDF_MODIFIED_DATE:
			return ConvertValue(objTran.Header.ModifiedDate, objType, iMasterType);
		case VTFIELDID.FDF_MODIFIED_TIME:
		{
			int iDate = new Date(m_objCalType).GetToday(m_objCalType).Value;
			int iTime = objTran.Header.ModifiedTime;
			FConvert.TranslateTimeZone(ref iDate, ref iTime, m_dClientOffset, m_objCalType);
			if (iTime != objTran.Header.ModifiedTime)
			{
				return FConvert.IntToStringTime(iTime);
			}
			objTran.Header.ModifiedTime = iTime;
			return ConvertValue(objTran.Header.ModifiedTime, objType, iMasterType);
		}
		case VTFIELDID.FDF_BOOKNO:
			return GetMasterValue(1, objTran.BodyData[iIndex].Book);
		case VTFIELDID.FDF_CODE:
			return GetMasterValue(1, objTran.BodyData[iIndex].Code);
		case VTFIELDID.FDF_AGAINSTINVOIVENO:
			return objTran.BodyData[iIndex].InvTag;
		case VTFIELDID.FDF_DUEDATE:
			return ConvertValue(objTran.BodyData[iIndex].DueDate, objType, iMasterType);
		case VTFIELDID.FDF_HEADERCURRENCY:
			return objTran.BodyData[iIndex].CurrencyId;
		case VTFIELDID.FDF_EXCHANGERATE:
			return objTran.BodyData[iIndex].ExchangeRate;
		case VTFIELDID.FDF_UPDATESTOCKS:
			return ConvertValue(objTran.Header.Flags.UpdateInv, objType, iMasterType);
		case VTFIELDID.FDF_UPDATEFA:
			return ConvertValue(objTran.Header.Flags.UpdateFA, objType, iMasterType);
		case VTFIELDID.FDF_LCNUMBER:
			return GetMasterValue(-2, objTran.Header.LC);
		case VTFIELDID.FDF_PMTTERMS:
			return GetMasterValue(-4, objTran.Header.PmtTerm);
		case VTFIELDID.FDF_HEADERINVTAG:
			return GetMasterValue(m_iInvTag, objTran.BodyData[iIndex].InvTag);
		case VTFIELDID.FDF_BODYINVTAG:
		{
			int num2 = objTran.BodyData.Where((TransBody p) => p.RowType == _RowType.Default).Count();
			iIndex = ((num2 / 2 + iIndex < num2) ? (num2 / 2 + iIndex) : iIndex);
			return GetMasterValue(m_iInvTag, (iIndex + 1 < objTran.BodyData.Length) ? objTran.BodyData[iIndex + 1].InvTag : objTran.BodyData[iIndex].InvTag);
		}
		case VTFIELDID.FDF_RCTISSUESELECTION:
			if (!(objTran.BodyData[iIndex].Sales.Quantity < 0m))
			{
				return "Receipts";
			}
			return "Issues";
		case VTFIELDID.FDF_PRODBATCH:
			return GetMasterValue(-6, objTran.Header.ProdOrderId);
		case VTFIELDID.FDF_BOMSIZE:
			return objTran.Header.ProductionSize;
		case VTFIELDID.FDF_PROD_PROCESS:
			return GetMasterValue(-7, objTran.Header.ProcessId);
		case VTFIELDID.FDF_RAISECASHCONTRA:
			return ConvertValue(objTran.Header.Flags.PostCashEntry, objType, iMasterType);
		default:
			if ((iFieldId & 0x10000000) > 0)
			{
				if (m_iInvTag == (iFieldId & 0xFFFFFF))
				{
					return GetMasterValue(iFieldId & 0xFFFFFF, objTran.BodyData[iIndex].InvTag);
				}
				if (m_iFaTag == (iFieldId & 0xFFFFFF))
				{
					return GetMasterValue(iFieldId & 0xFFFFFF, objTran.BodyData[iIndex].FATag);
				}
				if (objTran.BodyData[0].Tags != null && objTran.BodyData[0].Tags.Length != 0)
				{
					for (iIndex = 0; iIndex < objTran.BodyData[0].Tags.Length; iIndex++)
					{
						if (objTran.BodyData[0].Tags[iIndex].ID == (iFieldId & 0xFFFFFF))
						{
							return GetMasterValue(iFieldId & 0xFFFFFF, Convert.ToInt32(objTran.BodyData[0].Tags[iIndex].Tag));
						}
					}
				}
			}
			if (objTran.HeaderExtra != null)
			{
				for (iIndex = 0; iIndex < objTran.HeaderExtra.Length; iIndex++)
				{
					if (objTran.HeaderExtra[iIndex].ID == iFieldId)
					{
						return ConvertValue(objTran.HeaderExtra[iIndex].Tag, objType, (iMasterType == -1) ? (iFieldId & 0xFFFFFF) : iMasterType);
					}
				}
			}
			if (objTran.Footer != null)
			{
				for (iIndex = 0; iIndex < objTran.Footer.Length; iIndex++)
				{
					if (objTran.Footer[iIndex].FieldId == (iFieldId & 0xFFFFFF))
					{
						decimal num = Convert.ToDecimal(objTran.Footer[iIndex].Value);
						return ConvertValue(num, objType, iMasterType);
					}
					if ((iFieldId & Convert.ToInt32(VTFIELDFLAG.SCR_FTR)) != Convert.ToInt32(VTFIELDFLAG.SCR_FTR))
					{
						continue;
					}
					if ((iFieldId & 0xFFFFFF) - 1001 == iIndex)
					{
						FooterData[] array = objTran.Footer.Where((FooterData p) => p.ColMap == objTran.Footer[iIndex].ColMap).ToArray();
						if (array.Length != 0 && array[0].Account1 > 0)
						{
							return GetMasterValue(1, array[0].Account1);
						}
					}
					else if ((iFieldId & 0xFFFFFF) - 2001 == iIndex)
					{
						FooterData[] array2 = objTran.Footer.Where((FooterData p) => p.ColMap == objTran.Footer[iIndex].ColMap).ToArray();
						if (array2.Length != 0 && array2[0].Account2 > 0)
						{
							return GetMasterValue(1, array2[0].Account2);
						}
					}
				}
			}
			return null;
		}
	}

	private object GetBodyValue(int iFieldId, MasterDataType objType, int iMasterType, Transaction objTran, int iRowIndex, int iBinIndex = 0, int iOrderByBins = 0)
	{
		int num = 0;
		string text = null;
		if (objTran.BodyData != null && iRowIndex < objTran.BodyData.Length)
		{
			switch ((VTFIELDID)iFieldId)
			{
			case VTFIELDID.FDF_DEFSTAT:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.BinsData != null && objTran.BodyData[iRowIndex].Sales.BinsData.Length > iBinIndex)
				{
					return objTran.BodyData[iRowIndex].Sales.BinsData[iBinIndex].Flag switch
					{
						1 => (object)"Inspection", 
						2 => "Hold", 
						4 => "Damaged", 
						_ => "Available", 
					};
				}
				return "Available";
			case VTFIELDID.FDF_CODE:
			case VTFIELDID.FDF_BODYCODE:
				return GetMasterValue(1, objTran.BodyData[iRowIndex].Code);
			case VTFIELDID.FDF_BOOKNO:
			case VTFIELDID.FDF_BODYBOOKNO:
				return GetMasterValue(1, objTran.BodyData[iRowIndex].Book);
			case VTFIELDID.FDF_BODYDUEDATE:
				return ConvertValue(objTran.BodyData[iRowIndex].DueDate, objType, iMasterType);
			case VTFIELDID.FDF_EXCHANGERATE:
				return objTran.BodyData[iRowIndex].ExchangeRate;
			case VTFIELDID.FDF_BODYBATCHNO:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.BatchNo != null)
				{
					return objTran.BodyData[iRowIndex].Sales.BatchNo.BatchNo;
				}
				break;
			case VTFIELDID.FDF_BODYCURRENCY:
				return GetMasterValue(-1, objTran.BodyData[iRowIndex].CurrencyId);
			case VTFIELDID.FDF_BODYEXCHANGERATE:
				return objTran.BodyData[iRowIndex].ExchangeRate;
			case VTFIELDID.FDF_BODYAMOUNT:
				return objTran.BodyData[iRowIndex].OriginalAmounts[0];
			case VTFIELDID.FDF_BODYNET:
				switch (iMasterType)
				{
				case -1:
					return objTran.BodyData[iRowIndex].Amounts[1];
				case -2:
					if (objTran.BodyData[iRowIndex].LocalCurrencyData != null)
					{
						return objTran.BodyData[iRowIndex].LocalCurrencyData.Amounts[1];
					}
					return 0;
				default:
					return objTran.BodyData[iRowIndex].OriginalAmounts[0];
				}
			case VTFIELDID.FDF_CREDITDAYSBASEDDISCOUNT:
			case VTFIELDID.FDF_BODYJRNDEBIT:
				if (objTran.BodyData[iRowIndex].OriginalAmounts != null && objTran.BodyData[iRowIndex].OriginalAmounts.Length > 1)
				{
					if (objTran.BodyData[iRowIndex].OriginalAmounts[0] < 0m)
					{
						return objTran.BodyData[iRowIndex].OriginalAmounts[1];
					}
					return 0;
				}
				break;
			case VTFIELDID.FDF_BODYJRNCREDIT:
				if (objTran.BodyData[iRowIndex].OriginalAmounts != null && objTran.BodyData[iRowIndex].OriginalAmounts.Length > 1)
				{
					if (objTran.BodyData[iRowIndex].OriginalAmounts[1] < 0m)
					{
						return objTran.BodyData[iRowIndex].OriginalAmounts[0];
					}
					return 0;
				}
				break;
			case VTFIELDID.FDF_RESERVEQUANTITY:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.Reservation != null)
				{
					return PrintReservationDetails(objTran.BodyData[iRowIndex].Sales.Reservation);
				}
				break;
			case VTFIELDID.FDF_MAXQTYRELEASE:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.StockToRelease != null)
				{
					return objTran.BodyData[iRowIndex].Sales.StockToRelease.Quantity;
				}
				break;
			case VTFIELDID.FDF_BODYPRODUCT:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return GetMasterValue(2, objTran.BodyData[iRowIndex].Sales.Product);
				}
				break;
			case VTFIELDID.FDF_BODYUNITS:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return GetMasterValue(11, objTran.BodyData[iRowIndex].Sales.Unit);
				}
				break;
			case VTFIELDID.FDF_RFID:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return objTran.BodyData[iRowIndex].Sales.RFID;
				}
				break;
			case VTFIELDID.FDF_BODYQUANTITY:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					if (objTran.BodyData[iRowIndex].Sales.BinsData != null && iBinIndex < objTran.BodyData[iRowIndex].Sales.BinsData.Length && iOrderByBins > 0)
					{
						return objTran.BodyData[iRowIndex].Sales.BinsData[iBinIndex].Quantity;
					}
					return objTran.BodyData[iRowIndex].Sales.Quantity;
				}
				break;
			case VTFIELDID.FDF_BODYALTQUANTITY:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return objTran.BodyData[iRowIndex].Sales.AlternateQuantity;
				}
				break;
			case VTFIELDID.FDF_LINK:
			case VTFIELDID.FDF_BASE_LINK_DOC_NUMBER:
				text = string.Empty;
				if (objTran.BodyData[iRowIndex].Links != null && objTran.BodyData[iRowIndex].Links.Length != 0)
				{
					for (num = 0; num < objTran.BodyData[iRowIndex].Links.Length; num++)
					{
						if ((iMasterType & 0xFFFF) != objTran.BodyData[iRowIndex].Links[num].VoucherType)
						{
							continue;
						}
						text = objTran.BodyData[iRowIndex].Links[num].VoucherNo;
						if (!string.IsNullOrEmpty(text))
						{
							int num3 = text.IndexOf(":") + 1;
							int length = text.Length;
							if (num3 > 0 && length > num3)
							{
								text = text.Substring(num3, length - num3);
							}
						}
						break;
					}
				}
				return text;
			case VTFIELDID.FDF_MFGDATE:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.BatchNo != null && objTran.BodyData[iRowIndex].Sales.BatchNo.MfgDate.Value > 0)
				{
					return new RDConversion(m_objCalType)
					{
						DateFormat = m_strDateFormat
					}.SetDateFormat(StandardDataFormat.Default, objTran.BodyData[iRowIndex].Sales.BatchNo.MfgDate.Value, "/");
				}
				break;
			case VTFIELDID.FDF_EXPDATE:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.BatchNo != null && objTran.BodyData[iRowIndex].Sales.BatchNo.ExpDate.Value > 0)
				{
					string text2 = objTran.BodyData[iRowIndex].Sales.BatchNo.ExpDate.ToString();
					if (IsIgnoreDayInExpiry)
					{
						return objTran.BodyData[iRowIndex].Sales.BatchNo.ExpDate.Month + "/" + objTran.BodyData[iRowIndex].Sales.BatchNo.ExpDate.Year;
					}
					return new RDConversion(m_objCalType)
					{
						DateFormat = m_strDateFormat
					}.SetDateFormat(StandardDataFormat.Default, objTran.BodyData[iRowIndex].Sales.BatchNo.ExpDate.Value, "/");
				}
				break;
			case VTFIELDID.FDF_BATCHRATE:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.BatchNo != null)
				{
					return objTran.BodyData[iRowIndex].Sales.BatchNo.BatchRate;
				}
				break;
			case VTFIELDID.FDF_RMANO:
				if (objTran.BodyData[iRowIndex].Sales != null && objTran.BodyData[iRowIndex].Sales.RMANos != null)
				{
					if (m_bBarcodePrint)
					{
						return string.Join(", ", objTran.BodyData[iRowIndex].Sales.RMANos);
					}
					return string.Join(", ", GetRmaNoSets(objTran.BodyData[iRowIndex].Sales.RMANos));
				}
				break;
			case VTFIELDID.FDF_BODYGROSS:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return objTran.BodyData[iRowIndex].Sales.OrigGross;
				}
				break;
			case VTFIELDID.FDF_BODYRATE:
				if (objTran.BodyData[iRowIndex].Sales != null)
				{
					return objTran.BodyData[iRowIndex].Sales.OrigRate;
				}
				break;
			case VTFIELDID.FDF_REFERENCE:
			{
				if (objTran.BodyData[iRowIndex].BillwiseData == null)
				{
					break;
				}
				List<string> list = new List<string>();
				for (num = 0; num < objTran.BodyData[iRowIndex].BillwiseData.Length; num++)
				{
					if (objTran.BodyData[iRowIndex].BillwiseData[num].iRefType == 0)
					{
						list.Add("New Reference");
					}
					else if (objTran.BodyData[iRowIndex].BillwiseData[num].iRefType == 1)
					{
						list.Add("On Account");
					}
					else
					{
						list.Add(objTran.BodyData[iRowIndex].BillwiseData[num].sReference);
					}
				}
				return string.Join(", ", list.ToArray());
			}
			case VTFIELDID.FDF_BINS:
				if (objTran.BodyData[iRowIndex].Sales == null || objTran.BodyData[iRowIndex].Sales.BinsData == null)
				{
					break;
				}
				text = string.Empty;
				if (iBinIndex < objTran.BodyData[iRowIndex].Sales.BinsData.Length && iOrderByBins > 0)
				{
					text = GetMasterValue(12, objTran.BodyData[iRowIndex].Sales.BinsData[iBinIndex].BinId, bConvert: true);
				}
				else
				{
					for (num = 0; num < objTran.BodyData[iRowIndex].Sales.BinsData.Length; num++)
					{
						if (num > 0)
						{
							text += ",";
						}
						text += GetMasterValue(12, objTran.BodyData[iRowIndex].Sales.BinsData[num].BinId, bConvert: true);
					}
				}
				return text;
			case VTFIELDID.FDF_SKID:
				if (objTran.BodyData[iRowIndex].Sales == null || objTran.BodyData[iRowIndex].Sales.BinsData == null)
				{
					break;
				}
				text = string.Empty;
				for (num = 0; num < objTran.BodyData[iRowIndex].Sales.BinsData.Length; num++)
				{
					if (num > 0)
					{
						text += ",";
					}
					text = objTran.BodyData[iRowIndex].Sales.BinsData[num].SkidId.ToString();
				}
				return text;
			case VTFIELDID.FDF_CARTON:
				if (objTran.BodyData[iRowIndex].Sales == null || objTran.BodyData[iRowIndex].Sales.BinsData == null)
				{
					break;
				}
				text = string.Empty;
				for (num = 0; num < objTran.BodyData[iRowIndex].Sales.BinsData.Length; num++)
				{
					if (num > 0)
					{
						text += ",";
					}
					text = objTran.BodyData[iRowIndex].Sales.BinsData[num].Carton;
				}
				return text;
			case VTFIELDID.FDF_BINS2:
			{
				if (objTran.BodyData[iRowIndex].Sales == null || objTran.BodyData[iRowIndex].Sales.BinsData == null)
				{
					break;
				}
				text = string.Empty;
				TransBody[] array = objTran.BodyData.Where((TransBody p) => p.RowType == _RowType.Default).ToArray();
				iRowIndex = array.Length / 2 + iRowIndex;
				if (iRowIndex >= array.Length)
				{
					iRowIndex = 0;
				}
				for (num = 0; num < array[iRowIndex].Sales.BinsData.Length; num++)
				{
					if (num > 0)
					{
						text += ",";
					}
					text = GetMasterValue(12, array[iRowIndex].Sales.BinsData[num].BinId, bConvert: true);
				}
				return text;
			}
			case VTFIELDID.FDF_BODYINVTAG:
				if (FConvert.IsItTransfer(objTran.Header.VoucherType))
				{
					int num2 = objTran.BodyData.Where((TransBody p) => p.RowType == _RowType.Default).Count();
					iRowIndex = num2 / 2 + iRowIndex;
					if (iRowIndex >= num2)
					{
						iRowIndex = 0;
					}
					return GetMasterValue(m_iInvTag, objTran.BodyData[iRowIndex].InvTag);
				}
				return GetMasterValue(m_iInvTag, objTran.BodyData[iRowIndex].InvTag);
			case VTFIELDID.FDF_RCTISSUESELECTION:
				if (!(objTran.BodyData[iRowIndex].Sales.Quantity < 0m))
				{
					return "Receipts";
				}
				return "Issues";
			default:
				if ((iFieldId & 0x20000000) > 0)
				{
					if (m_iInvTag == (iFieldId & 0xFFFFFF))
					{
						return GetMasterValue(iFieldId & 0xFFFFFF, objTran.BodyData[iRowIndex].InvTag);
					}
					if (m_iFaTag == (iFieldId & 0xFFFFFF))
					{
						return GetMasterValue(iFieldId & 0xFFFFFF, objTran.BodyData[iRowIndex].FATag);
					}
					if (objTran.BodyData[iRowIndex].Tags != null && objTran.BodyData[iRowIndex].Tags.Length != 0)
					{
						for (num = 0; num < objTran.BodyData[iRowIndex].Tags.Length; num++)
						{
							if (objTran.BodyData[iRowIndex].Tags[num].ID == (iFieldId & 0xFFFFFF) || objTran.BodyData[iRowIndex].Tags[num].ID == iFieldId)
							{
								return GetMasterValue(iFieldId & 0xFFFFFF, (int)objTran.BodyData[iRowIndex].Tags[num].Tag);
							}
						}
					}
				}
				if (objTran.BodyData[iRowIndex].BodyExtra != null)
				{
					for (num = 0; num < objTran.BodyData[iRowIndex].BodyExtra.Length; num++)
					{
						if (objTran.BodyData[iRowIndex].BodyExtra[num].ID == iFieldId)
						{
							return ConvertValue(objTran.BodyData[iRowIndex].BodyExtra[num].Tag, objType, (iMasterType == -1) ? (iFieldId & 0xFFFFFF) : iMasterType);
						}
					}
				}
				if (objTran.BodyData[iRowIndex].Sales == null || objTran.BodyData[iRowIndex].Sales.ScreenData == null)
				{
					break;
				}
				for (num = 0; num < objTran.BodyData[iRowIndex].Sales.ScreenData.Length; num++)
				{
					if (iFieldId < 0)
					{
						if (objTran.BodyData[iRowIndex].Sales.ScreenData[num].FieldId == ((iFieldId * -1) & 0xFFFFFF) || objTran.BodyData[iRowIndex].Sales.ScreenData[num].FieldId == iFieldId * -1)
						{
							return ConvertValue(objTran.BodyData[iRowIndex].Sales.ScreenData[num].Value, objType, iMasterType);
						}
					}
					else if (objTran.BodyData[iRowIndex].Sales.ScreenData[num].FieldId == (iFieldId & 0xFFFFFF) || objTran.BodyData[iRowIndex].Sales.ScreenData[num].FieldId == iFieldId)
					{
						return ConvertValue(objTran.BodyData[iRowIndex].Sales.ScreenData[num].Input, objType, iMasterType);
					}
				}
				break;
			}
		}
		return null;
	}

	private object ConvertValue(object oValue, MasterDataType oDataType, int iMasterType)
	{
		bool flag = false;
		if (IsDontConvertField)
		{
			if (oDataType == MasterDataType.Date && Convert.ToString(oValue) == "0")
			{
				return string.Empty;
			}
			if (oDataType == MasterDataType.Boolean)
			{
				oValue = ((Convert.ToString(oValue).ToLower() == "true" || Convert.ToString(oValue) == "1") ? 1 : 0);
				FConvert.PrintBooleanAs(PrintBooleanAs, ref oValue, m_arrResources);
			}
			return oValue;
		}
		try
		{
			switch (oDataType)
			{
			case MasterDataType.Date:
				oValue = new Date(Convert.ToInt32(oValue), m_objCalType).ToString();
				break;
			case MasterDataType.Time:
				oValue = FConvert.IntToStringTime(Convert.ToInt32(oValue));
				break;
			case MasterDataType.DateTime:
				oValue = new FDateTime(Convert.ToInt64(oValue), m_objCalType);
				break;
			case MasterDataType.Master:
				oValue = GetMasterValue(iMasterType, Convert.ToInt32(oValue));
				break;
			case MasterDataType.NumberList:
			{
				string[] array = GetMasterValue(-3, iMasterType).Split(',');
				foreach (string text in array)
				{
					if (flag)
					{
						oValue = text;
						break;
					}
					if (text == Convert.ToString(oValue))
					{
						flag = true;
					}
				}
				break;
			}
			case MasterDataType.Boolean:
				FConvert.PrintBooleanAs(PrintBooleanAs, ref oValue, m_arrResources);
				break;
			case MasterDataType.Fraction:
			case MasterDataType.Picture:
			case MasterDataType.StringList:
			case MasterDataType.DocumentViewer:
			case MasterDataType.UpdatedTime:
				break;
			}
		}
		catch
		{
		}
		return oValue;
	}

	private string GetMasterValue(int iMasterTypeId, long iMasterId, bool bConvert = false)
	{
		if (IsDontConvertField && !bConvert)
		{
			return Convert.ToString(iMasterId);
		}
		IdNamePair idNamePair = (IdNamePair)CallServeRequest(RDMethods.GetFieldName, iMasterId, iMasterTypeId, 0);
		if (!IsMasterCode)
		{
			return idNamePair.Name;
		}
		return Convert.ToString(idNamePair.Tag);
	}

	private string GetMasterCode(int iMasterTypeId, int iMasterId, int iLanguageId)
	{
		IdNamePair idNamePair = (IdNamePair)CallServeRequest(RDMethods.GetFieldName, iMasterId, iMasterTypeId, iLanguageId);
		if (iLanguageId > 0)
		{
			return idNamePair.Name;
		}
		return Convert.ToString(idNamePair.Tag);
	}

	private string[] GetRmaNoSets(string[] arrRmaNo)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string text = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<DualIdPair> list = new List<DualIdPair>();
		List<string> list2 = new List<string>();
		try
		{
			arrRmaNo = arrRmaNo.OrderBy((string p) => p).ToArray();
			for (num = 0; num < arrRmaNo.Length; num++)
			{
				empty = (empty2 = (text = (empty3 = string.Empty)));
				if (!string.IsNullOrEmpty(arrRmaNo[num]))
				{
					num2 = arrRmaNo[num].Length;
					while (num2 > 0 && arrRmaNo[num][num2 - 1] >= '0' && arrRmaNo[num][num2 - 1] <= '9')
					{
						empty2 += arrRmaNo[num][num2 - 1];
						num2--;
					}
					empty = arrRmaNo[num].Substring(0, num2);
					for (num2 = empty2.Length; num2 > 0; num2--)
					{
						text += empty2[num2 - 1];
					}
					empty2 = text;
				}
				if (empty2.StartsWith("0"))
				{
					for (num2 = 0; num2 < empty2.Length && empty2[num2] == '0'; num2++)
					{
						empty3 += empty2[num2];
					}
					empty += empty3;
					if (!string.IsNullOrEmpty(empty3) && empty3.Length < empty2.Length)
					{
						empty2 = empty2.Substring(empty3.Length, empty2.Length - empty3.Length);
					}
				}
				if (!string.IsNullOrEmpty(empty2))
				{
					num3 = Convert.ToInt32(empty2);
					for (num2 = 0; num2 < list.Count; num2++)
					{
						if (Convert.ToString(list[num2].Value) == empty && num3 == list[num2].SubId + 1)
						{
							list[num2].SubId = num3;
							break;
						}
					}
					if (num2 == list.Count)
					{
						list.Add(new DualIdPair(num3, num3, empty));
					}
				}
				else
				{
					list.Add(new DualIdPair(0, 0, arrRmaNo[num]));
				}
			}
			if (list.Count > 0)
			{
				for (num = 0; num < list.Count; num++)
				{
					if (list[num].SubId == list[num].Id && list[num].Id > 0)
					{
						list2.Add($"{list[num].Value}{list[num].Id}");
					}
					else if (list[num].Id < list[num].SubId && list[num].Id > 0)
					{
						list2.Add(string.Format("{0}{1}-{0}{2}", list[num].Value, list[num].Id, list[num].SubId));
					}
					else if (list[num].Id < list[num].SubId && list[num].Id == 0)
					{
						list2.Add(string.Format("{0}-{0}{1}", list[num].Value, list[num].SubId));
					}
					else
					{
						list2.Add(Convert.ToString(list[num].Value));
					}
				}
			}
			else
			{
				list2.AddRange(arrRmaNo);
			}
		}
		catch
		{
			list2.Clear();
			list2.AddRange(arrRmaNo);
		}
		return list2.ToArray();
	}

	private object[] GetVoucherSpecificValues(int iFieldId)
	{
		List<object> list = new List<object>();
		if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
		{
			for (int i = 0; i < m_objTranData.BodyData.Length; i++)
			{
				if (m_objTranData.BodyData[i].Sales == null || !m_objTransactionPrintCommon.IsPrintModiferProduct(m_objTranData.BodyData[i].Sales.POSData, m_objTranData.BodyData[i].Sales.Rate, m_layoutInfo.ModifierOption))
				{
					continue;
				}
				switch (iFieldId)
				{
				case 0:
					if (m_objTranData.BodyData[i].Sales.POSData == null || (m_objTranData.BodyData[i].Sales.POSData != null && m_objTranData.BodyData[i].Sales.POSData.ModifierId == 0))
					{
						list.Add(GetMasterValue(2, m_objTranData.BodyData[i].Sales.Product, bConvert: true));
					}
					else
					{
						list.Add(string.Empty);
					}
					break;
				case 1:
					if (m_objTranData.BodyData[i].Sales.POSData != null && m_objTranData.BodyData[i].Sales.POSData.ModifierId > 0)
					{
						list.Add(GetMasterValue(2, m_objTranData.BodyData[i].Sales.Product, bConvert: true));
					}
					else
					{
						list.Add(string.Empty);
					}
					break;
				case 2:
					if (m_objTranData.BodyData[i].Sales.POSData == null || (m_objTranData.BodyData[i].Sales.POSData != null && m_objTranData.BodyData[i].Sales.POSData.ModifierId == 0))
					{
						list.Add(GetMasterCode(2, m_objTranData.BodyData[i].Sales.Product, m_iAltLanguageId));
					}
					else
					{
						list.Add(string.Empty);
					}
					break;
				case 3:
					if (m_objTranData.BodyData[i].Sales != null && m_objTranData.BodyData[i].Sales.Product > 0)
					{
						list.Add(GetMasterValue(-8, m_objTranData.BodyData[i].Sales.Product, bConvert: true));
					}
					else
					{
						list.Add(string.Empty);
					}
					break;
				}
			}
		}
		else
		{
			list.Add("0");
		}
		return list.ToArray();
	}

	private object[] GetReferenceValues(int iFieldId)
	{
		string text = null;
		decimal num = 0m;
		int num2 = 0;
		List<string> list = new List<string>();
		List<string> list2 = null;
		new RDConversion(m_objCalType).DateFormat = m_strDateFormat;
		int iDecimalInColumn = ((m_oCurrDetail != null && m_oCurrDetail.NoOfDecimal > 0) ? m_oCurrDetail.NoOfDecimal : 2);
		list2 = new List<string>();
		if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
		{
			for (int i = 0; i < m_objTranData.BodyData.Length; i++)
			{
				num = 0m;
				list2.Clear();
				if (m_objTranData.BodyData[i].BillwiseData != null)
				{
					for (int j = 0; j < m_objTranData.BodyData[i].BillwiseData.Length; j++)
					{
						switch ((BillwiseInvoiceField)(byte)iFieldId)
						{
						case BillwiseInvoiceField.ActualAmount:
							num = ((m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo == null) ? 0m : m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.OrginalTransactionAmount);
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
							}
							break;
						case BillwiseInvoiceField.AdjustedAmount:
							num = ((m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo == null) ? 0m : m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.AdjustedTransactionAmount);
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
							}
							break;
						case BillwiseInvoiceField.ReferenceAmount:
							if (m_objTranData.BodyData[i].BillwiseData[j].iRefType == 0)
							{
								num = m_objTranData.BodyData[i].BillwiseData[j].OriginalAmountTC;
								if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
								{
									list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
								}
							}
							break;
						case BillwiseInvoiceField.BillAmount:
							if (m_objTranData.BodyData[i].BillwiseData[j].iRefType == 0)
							{
								num = m_objTranData.BodyData[i].BillwiseData[j].OriginalAmountTC;
								if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
								{
									list2.Add($"New reference : {RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString()}");
								}
							}
							else
							{
								if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo == null)
								{
									break;
								}
								num = m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.AdjustedTransactionAmount;
								if (((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric) && m_objTranData.BodyData[i].BillwiseData[j].sReference != null)
								{
									num2 = m_objTranData.BodyData[i].BillwiseData[j].sReference.IndexOf(" : ");
									string text2 = null;
									if (num2 > 0)
									{
										text2 = m_objTranData.BodyData[i].BillwiseData[j].sReference.Substring(0, num2);
										list2.Add($"{text2} : {RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString()}");
									}
								}
							}
							break;
						case BillwiseInvoiceField.BalanceAmount:
							num = ((m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo == null) ? 0m : m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.BalanceTransactionAmount);
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
							}
							else
							{
								list2.Add("");
							}
							break;
						case BillwiseInvoiceField.DiscountAmount:
							num = ((m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo == null) ? 0m : m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.DiscountTransactionAmount);
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
							}
							break;
						case BillwiseInvoiceField.DueDate:
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sReference))
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].dDueDate.ToString());
							}
							break;
						case BillwiseInvoiceField.Reference:
							if (m_objTranData.BodyData[i].BillwiseData[j].iRefType == 0)
							{
								list2.Add("New Reference");
							}
							else if (m_objTranData.BodyData[i].BillwiseData[j].iRefType == 1)
							{
								list2.Add("On Account");
							}
							else
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].sReference);
							}
							break;
						case BillwiseInvoiceField.DocNumber:
							if (m_objTranData.BodyData[i].BillwiseData[j].sReference != null)
							{
								num2 = m_objTranData.BodyData[i].BillwiseData[j].sReference.IndexOf(" : ");
								if (num2 > 0)
								{
									list2.Add(m_objTranData.BodyData[i].BillwiseData[j].sReference.Substring(0, num2));
								}
							}
							break;
						case BillwiseInvoiceField.Date:
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sReference))
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].dDocDate.ToString());
							}
							break;
						case BillwiseInvoiceField.Account:
							IsDontConvertField = false;
							list2.Add(GetMasterValue(1, m_objTranData.BodyData[i].BillwiseData[j].iCode));
							IsDontConvertField = true;
							break;
						case BillwiseInvoiceField.Narration:
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sNarration))
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].sNarration);
							}
							break;
						case BillwiseInvoiceField.BillNumber:
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sBillNo))
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].sBillNo);
							}
							else if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sReference))
							{
								list2.Add("");
							}
							break;
						case BillwiseInvoiceField.TotalActualAmt:
							if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo != null)
							{
								num += m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.OrginalTransactionAmount;
							}
							else
							{
								num += m_objTranData.BodyData[i].BillwiseData[j].mBAIDInvoiceCurrency;
							}
							break;
						case BillwiseInvoiceField.TotalAdjustedAmt:
							if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo != null)
							{
								num += m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.AdjustedTransactionAmount;
							}
							else
							{
								num += m_objTranData.BodyData[i].BillwiseData[j].mAAIDInvoiceCurrency;
							}
							break;
						case BillwiseInvoiceField.TotalDiscountAmt:
							if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo != null)
							{
								num += m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.DiscountTransactionAmount;
							}
							break;
						case BillwiseInvoiceField.ReferenceSalesAccount:
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sSalesAcccount))
							{
								list2.Add(m_objTranData.BodyData[i].BillwiseData[j].sSalesAcccount);
							}
							break;
						case BillwiseInvoiceField.ReferenceWithAdjAmount:
							if (m_objTranData.BodyData[i].BillwiseData[j].iRefType != 0)
							{
								text = ((m_objTranData.BodyData[i].BillwiseData[j].iRefType != 1) ? m_objTranData.BodyData[i].BillwiseData[j].sReference : "On Account");
							}
							else
							{
								text = "New Reference";
							}
							if (!string.IsNullOrEmpty(m_objTranData.BodyData[i].BillwiseData[j].sBillNo))
							{
								text = text + " " + m_objTranData.BodyData[i].BillwiseData[j].sBillNo;
							}
							num = 0m;
							if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo != null)
							{
								num = m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.AdjustedTransactionAmount;
							}
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add($"{text} {RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator)}");
							}
							else
							{
								list2.Add(text);
							}
							break;
						case BillwiseInvoiceField.PreviousAdjustAmount:
							num = m_objTranData.BodyData[i].BillwiseData[j].dPreviousAdjustedAmount;
							if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
							{
								list2.Add(RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString());
							}
							break;
						case BillwiseInvoiceField.InvoiceAdvanceAdjusted:
							if (m_objTranData.BodyData[i].BillwiseData[j].iRefType == 2)
							{
								if (m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo != null)
								{
									num += m_objTranData.BodyData[i].BillwiseData[j].BillBaseInfo.AdjustedTransactionAmount;
								}
								else
								{
									num += m_objTranData.BodyData[i].BillwiseData[j].mAAIDInvoiceCurrency;
								}
							}
							break;
						}
					}
				}
				if (list2.Count > 0 || m_objTranData.BodyData[i].RowType == _RowType.Default)
				{
					if (iFieldId <= 7 || iFieldId == 10 || iFieldId == 11 || iFieldId == 15 || iFieldId == 16 || iFieldId == 18 || iFieldId == 17 || iFieldId == 14)
					{
						text = string.Join("\r\n", list2.ToArray());
					}
					else if ((!IsPrintZeroValueAsNumeric && num != 0m) || IsPrintZeroValueAsNumeric)
					{
						text = RDCommon.InsertComma(num, iDecimalInColumn, IsNumericSeparator).ToString();
					}
					list.Add(text);
				}
			}
		}
		else
		{
			list.Add("0");
		}
		return list.ToArray();
	}

	private IdNamePair[] GetCustomerAgeingValues(LayoutInformation objLayoutInfo)
	{
		List<IdNamePair> list = new List<IdNamePair>();
		bool bMonthly = false;
		if (IsAgeingFieldsInLayout(objLayoutInfo, ref bMonthly) && m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.Header != null && m_objTranData.BodyData.Length != 0 && m_objTranData.BodyData[0].Book > 0)
		{
			AccountAgeingInput accountAgeingInput = new AccountAgeingInput();
			accountAgeingInput.MasterIds = new int[1] { m_objTranData.BodyData[0].Book };
			accountAgeingInput.EndDate = m_objTranData.Header.Date;
			accountAgeingInput.LanguageId = m_iAltLanguageId;
			accountAgeingInput.IncludeMonthlySlab = bMonthly;
			AccountAgeingDetails[] array = null;
			array = ((PrintTagDetails)CallServeRequest(RDMethods.GetAgeingDetails, accountAgeingInput)).AgeingDetails;
			if (array != null && array.Length != 0)
			{
				string text = null;
				list.Add(new IdNamePair(29, "Ageing month-wise", array[0].MonthlySlab));
				list.Add(new IdNamePair(28, "Ageing balance", array[0].Balance));
				if (m_arrAgeingSlabs != null && m_arrAgeingSlabs.Length != 0)
				{
					int num = 0;
					int num2 = 30;
					num = 0;
					while (num + 1 < m_arrAgeingSlabs.Length && num2 <= 36)
					{
						switch ((INVOICE_FIELD)num2)
						{
						case INVOICE_FIELD.INV_AGEING_SLAB1:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab1));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB2:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab2));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB3:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab3));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB4:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab4));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB5:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab5));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB6:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab6));
							break;
						case INVOICE_FIELD.INV_AGEING_SLAB7:
							list.Add(new IdNamePair(num2, m_arrAgeingSlabs[num], array[0].Slab7));
							break;
						}
						text = m_arrAgeingSlabs[num + 1];
						num += 2;
						num2++;
					}
				}
				else
				{
					list.Add(new IdNamePair(30, "Ageing slab1", array[0].Slab1));
					list.Add(new IdNamePair(31, "Ageing slab2", array[0].Slab2));
					list.Add(new IdNamePair(32, "Ageing slab3", array[0].Slab3));
					list.Add(new IdNamePair(33, "Ageing slab4", array[0].Slab4));
					list.Add(new IdNamePair(34, "Ageing slab5", array[0].Slab5));
					list.Add(new IdNamePair(35, "Ageing slab6", array[0].Slab6));
					list.Add(new IdNamePair(36, "Ageing slab7", array[0].Slab7));
					text = "Ageing slab7";
				}
				list.Add(new IdNamePair(40, "> " + text, array[0].GreterThanLastSlab));
			}
		}
		return list.ToArray();
	}

	private List<HeaderGroup> GetBaseVoucherFields(bool bLoadFields)
	{
		bool flag = false;
		int num = 0;
		int num2 = 0;
		string text = null;
		HeaderGroup headerGroup = null;
		TemplateFields templateFields = null;
		TranMasterInfo tranMasterInfo = null;
		Transaction transaction = null;
		IdNamePair[] array = null;
		List<TemplateFields> list = null;
		List<HeaderGroup> list2 = null;
		List<object> list3 = null;
		List<int> list4 = null;
		bool flag2 = false;
		if (m_objTranData != null && m_objTranData.BodyData != null && m_objTranData.BodyData.Length != 0)
		{
			list2 = new List<HeaderGroup>();
			list4 = new List<int>();
			int iRowIndex = 0;
			for (num2 = 0; num2 < m_objTranData.BodyData.Length; num2++)
			{
				TransBody transBody = m_objTranData.BodyData[num2];
				if (transBody.RowType != _RowType.Default || transBody.Links == null || transBody.Links.Length == 0)
				{
					continue;
				}
				if (!bLoadFields && !IsBaseVoucherFieldsInLayout(transBody.Links[0].VoucherType))
				{
					return null;
				}
				DataLink[] links = transBody.Links;
				foreach (DataLink dataLink in links)
				{
					flag2 = false;
					foreach (int item in list4)
					{
						if (item == dataLink.VoucherType)
						{
							flag2 = true;
							break;
						}
					}
					list4.Add(dataLink.VoucherType);
					if (transaction == null || (!string.IsNullOrEmpty(dataLink.VoucherNo) && dataLink.VoucherNo != text))
					{
						text = dataLink.VoucherNo;
						iRowIndex = 0;
						transaction = (Transaction)CallServeRequestTrans(TransMethods.LoadVoucherById, dataLink.VoucherType, dataLink.RefId, LoadTransactionBy.TransactionId);
					}
					if (transaction != null && transaction.BodyData != null)
					{
						for (int j = 0; j < transaction.BodyData.Length; j++)
						{
							if (transaction.BodyData[j].TransactionId == dataLink.RefId)
							{
								iRowIndex = j;
								break;
							}
						}
					}
					if (flag2)
					{
						UpdatebaseBody(transaction, array, dataLink.UsedValue, m_objTranData.BodyData.Length, iRowIndex++, ref list2);
						continue;
					}
					if (array == null)
					{
						array = GetDocFields(dataLink.VoucherType, bLinkVoucher: true);
						for (num = 0; num < array?.Length; num++)
						{
							if (array[num].ID == 13 && !FConvert.IsItInwardVoucher(dataLink.VoucherType))
							{
								flag = true;
								break;
							}
						}
						if (array != null && array.Length != 0)
						{
							Array.Resize(ref array, array.Length + 1 + (flag ? 2 : 0));
							array[array.Length - 1] = new IdNamePair(75, "Net", new TranMasterInfo
							{
								IsBodyField = true,
								DataType = 6,
								MasterType = 0,
								Filter = string.Empty
							});
							if (flag)
							{
								array[array.Length - 2] = new IdNamePair(36, "Manufacturing date", new TranMasterInfo
								{
									IsBodyField = true,
									DataType = 0,
									MasterType = 0,
									Filter = string.Empty
								});
								array[array.Length - 3] = new IdNamePair(37, "Expiry date", new TranMasterInfo
								{
									IsBodyField = true,
									DataType = 0,
									MasterType = 0,
									Filter = string.Empty
								});
							}
						}
					}
					headerGroup = new HeaderGroup();
					headerGroup.MasterId = dataLink.VoucherType;
					headerGroup.GroupName = $"Base {EnumUtils.DescriptionOf((VTTYPE)dataLink.VoucherType)}";
					list = new List<TemplateFields>();
					IsDontConvertField = true;
					for (num = 0; num < array.Length; num++)
					{
						list3 = new List<object>();
						templateFields = new TemplateFields();
						templateFields.FieldId = array[num].ID;
						templateFields.Name = array[num].Name;
						tranMasterInfo = (TranMasterInfo)array[num].Tag;
						templateFields.DataType = (MasterDataType)tranMasterInfo.DataType;
						if (templateFields.DataType == MasterDataType.Date || templateFields.DataType == MasterDataType.Time)
						{
							templateFields.Name = array[num].Name + " " + EnumUtils.DescriptionOf((VTTYPE)dataLink.VoucherType);
						}
						templateFields.MasterType = tranMasterInfo.MasterType;
						if (!tranMasterInfo.IsBodyField)
						{
							list3.Add(GetHeaderValue(templateFields.FieldId, templateFields.DataType, templateFields.MasterType, transaction));
							templateFields.Values = list3.ToArray();
							list.Add(templateFields);
						}
						else
						{
							list3.Add(GetBodyValue(templateFields.FieldId, templateFields.DataType, templateFields.MasterType, transaction, iRowIndex));
							templateFields.Values = list3.ToArray();
							list.Add(templateFields);
						}
					}
					headerGroup.Fields = list.ToArray();
					list2.Add(headerGroup);
				}
			}
			return list2;
		}
		return null;
	}

	private string PrintReservationDetails(Reservations objReservations)
	{
		if (objReservations.ReserveType == 2)
		{
			if (((StockReservationList)objReservations).StockList.Count > 0)
			{
				return string.Format("Stock:" + ((StockReservationList)objReservations).StockList.Sum((StockReservationDetails p) => p.ReserveQty));
			}
		}
		else if (objReservations.ReserveType == 3)
		{
			if (objReservations.ReserveByBatch)
			{
				if (objReservations.ReserveByBin)
				{
					if (objReservations.ReserveByRMA)
					{
						_ = ((BatchReservationList)objReservations).BatchList.Count;
						_ = 0;
						return string.Empty;
					}
					if (((BatchReservationList)objReservations).BatchList.Count > 0)
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("Batch-Bin:");
						for (int num = 0; num < ((BatchReservationList)objReservations).BatchList.Count; num++)
						{
							if (((BatchReservationList)objReservations).BatchList[num].ReserveQty > 0m)
							{
								if (num > 0)
								{
									stringBuilder.Append(",");
								}
								stringBuilder.Append(((BatchReservationList)objReservations).BatchList[num].BatchNo + "-");
								stringBuilder.Append(((BatchReservationList)objReservations).BatchList[num].Bins + ":");
								stringBuilder.Append(((BatchReservationList)objReservations).BatchList[num].ReserveQty);
							}
						}
						return stringBuilder.ToString();
					}
				}
				else if (objReservations.ReserveByRMA)
				{
					if (((RMAReservationList)objReservations).RMAList.Count > 0)
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						stringBuilder2.Append("RMA:");
						for (int num2 = 0; num2 < ((RMAReservationList)objReservations).RMAList.Count; num2++)
						{
							if (((RMAReservationList)objReservations).RMAList[num2].Selected)
							{
								if (num2 > 0)
								{
									stringBuilder2.Append(",");
								}
								stringBuilder2.Append(((RMAReservationList)objReservations).RMAList[num2].RMANo);
							}
						}
						return stringBuilder2.ToString();
					}
				}
				else if (((BatchReservationList)objReservations).BatchList.Count > 0)
				{
					StringBuilder stringBuilder3 = new StringBuilder();
					stringBuilder3.Append("Batch:");
					for (int num3 = 0; num3 < ((BatchReservationList)objReservations).BatchList.Count; num3++)
					{
						if (((BatchReservationList)objReservations).BatchList[num3].ReserveQty > 0m)
						{
							if (num3 > 0)
							{
								stringBuilder3.Append(",");
							}
							stringBuilder3.Append(((BatchReservationList)objReservations).BatchList[num3].BatchNo + ":");
							stringBuilder3.Append(((BatchReservationList)objReservations).BatchList[num3].ReserveQty);
						}
					}
					return stringBuilder3.ToString();
				}
			}
			else if (objReservations.ReserveByBin)
			{
				if (objReservations.ReserveByRMA)
				{
					return string.Empty;
				}
				if (((BinReservationList)objReservations).BinList.Count > 0)
				{
					StringBuilder stringBuilder4 = new StringBuilder();
					stringBuilder4.Append("Bin:");
					for (int num4 = 0; num4 < ((BinReservationList)objReservations).BinList.Count; num4++)
					{
						if (((BinReservationList)objReservations).BinList[num4].ReserveQty > 0m)
						{
							if (num4 > 0)
							{
								stringBuilder4.Append(",");
							}
							stringBuilder4.Append(((BinReservationList)objReservations).BinList[num4].Bins + ":");
							stringBuilder4.Append(((BinReservationList)objReservations).BinList[num4].ReserveQty);
						}
					}
					return stringBuilder4.ToString();
				}
			}
			else if (((RMAReservationList)objReservations).RMAList.Count > 0)
			{
				StringBuilder stringBuilder5 = new StringBuilder();
				stringBuilder5.Append("RMA:");
				for (int num5 = 0; num5 < ((RMAReservationList)objReservations).RMAList.Count; num5++)
				{
					if (((RMAReservationList)objReservations).RMAList[num5].Selected)
					{
						if (num5 > 0)
						{
							stringBuilder5.Append(",");
						}
						stringBuilder5.Append(((RMAReservationList)objReservations).RMAList[num5].RMANo);
					}
				}
				return stringBuilder5.ToString();
			}
		}
		return string.Empty;
	}

	private void UpdatebaseBody(Transaction objTran, IdNamePair[] arrDocFields, decimal dUsedValue, int iBodyLength, int iRowIndex, ref List<HeaderGroup> arrGroup)
	{
		List<object> list = null;
		for (int i = 0; i < arrGroup[0].Fields.Length; i++)
		{
			for (int j = 0; j < arrDocFields.Length; j++)
			{
				if (arrGroup[0].Fields[i].FieldId == arrDocFields[j].ID)
				{
					list = new List<object>();
					list.AddRange(arrGroup[0].Fields[i].Values);
					if (arrGroup[0].Fields[i].FieldId == 26)
					{
						list.Add(dUsedValue);
					}
					else if (((TranMasterInfo)arrDocFields[j].Tag).IsBodyField)
					{
						list.Add(GetBodyValue(arrGroup[0].Fields[i].FieldId, arrGroup[0].Fields[i].DataType, arrGroup[0].Fields[i].MasterType, objTran, iRowIndex));
					}
					else
					{
						list.Add(GetHeaderValue(arrGroup[0].Fields[i].FieldId, arrGroup[0].Fields[i].DataType, arrGroup[0].Fields[i].MasterType, objTran));
					}
					arrGroup[0].Fields[i].Values = list.ToArray();
					break;
				}
			}
		}
	}

	private bool IsAgeingFieldsInLayout(LayoutInformation objLayoutInformation, ref bool bMonthly)
	{
		bool result = false;
		if (objLayoutInformation != null && objLayoutInformation.Pages != null && objLayoutInformation.Pages.Length != 0 && objLayoutInformation.Pages[0].PageHeader != null)
		{
			for (int i = 0; i < objLayoutInformation.Pages[0].PageHeader.Length; i++)
			{
				if (objLayoutInformation.Pages[0].PageHeader[i].Type == ControlType.BodyCanvas && objLayoutInformation.Pages[0].PageHeader[i].PageBody != null)
				{
					for (int j = 0; j < objLayoutInformation.Pages[0].PageHeader[i].PageBody.Length; j++)
					{
						if (objLayoutInformation.Pages[0].PageHeader[i].PageBody[j].Column.ToLower().StartsWith("miscellaneous."))
						{
							result = true;
							if (objLayoutInformation.Pages[0].PageHeader[i].PageBody[j].Column.ToLower() == "miscellaneous.ageing month-wise")
							{
								bMonthly = true;
							}
						}
					}
				}
				if (objLayoutInformation.Pages[0].PageHeader[i].Type == ControlType.Textblock && objLayoutInformation.Pages[0].PageHeader[i].Text.ToLower().StartsWith("miscellaneous."))
				{
					result = true;
					if (objLayoutInformation.Pages[0].PageHeader[i].Text.ToLower() == "miscellaneous.ageing month-wise")
					{
						bMonthly = true;
					}
				}
				if (bMonthly)
				{
					break;
				}
			}
		}
		return result;
	}

	private bool IsBaseVoucherFieldsInLayout(int iVoucherType)
	{
		if (iVoucherType > 0 && m_layoutInfo != null && m_layoutInfo.Pages != null && m_layoutInfo.Pages.Length != 0 && m_layoutInfo.Pages[0].PageHeader != null)
		{
			string value = $"Base {EnumUtils.DescriptionOf((VTTYPE)iVoucherType)}";
			for (int i = 0; i < m_layoutInfo.Pages[0].PageHeader.Length; i++)
			{
				if (m_layoutInfo.Pages[0].PageHeader[i].UID <= 2000 && m_layoutInfo.Pages[0].PageHeader[i].PageBody != null)
				{
					for (int j = 0; j < m_layoutInfo.Pages[0].PageHeader[i].PageBody.Length; j++)
					{
						if (m_layoutInfo.Pages[0].PageHeader[i].PageBody[j].Column.Contains(value))
						{
							return true;
						}
					}
				}
				if (m_layoutInfo.Pages[0].PageHeader[i].Text.Contains(value))
				{
					return true;
				}
			}
		}
		return false;
	}

	private List<HeaderGroup> GetCompanyFields(string sHeader, int iGroupId)
	{
		int num = 0;
		HeaderGroup headerGroup = null;
		TemplateFields templateFields = null;
		List<TemplateFields> list = null;
		List<HeaderGroup> list2 = null;
		CompanyExtraField[] array = null;
		list2 = new List<HeaderGroup>();
		array = (CompanyExtraField[])CallServeRequestMaster(MastersMethods.GetCompanyOtherFields);
		if (array != null && array.Length != 0)
		{
			headerGroup = new HeaderGroup();
			headerGroup.MasterId = iGroupId;
			headerGroup.GroupName = sHeader;
			list = new List<TemplateFields>();
			templateFields = new TemplateFields();
			templateFields.FieldId = 101;
			templateFields.Name = "Company Logo";
			if (m_arrCompanyLogo != null && m_arrCompanyLogo.Length > 10)
			{
				templateFields.DataType = MasterDataType.Picture;
				templateFields.Values = new object[1] { m_arrCompanyLogo };
			}
			else
			{
				templateFields.DataType = MasterDataType.Text;
				templateFields.Values = new object[1] { string.Empty };
			}
			list.Add(templateFields);
			for (num = 0; num < array.Length; num++)
			{
				templateFields = new TemplateFields();
				templateFields.FieldId = array[num].FieldId;
				templateFields.Name = array[num].FieldName;
				templateFields.DataType = (MasterDataType)array[num].DataType;
				templateFields.MasterType = ((templateFields.DataType == MasterDataType.Master) ? array[num].LinkMasterId : (-1));
				if (templateFields.DataType == MasterDataType.Master)
				{
					templateFields.Name = "Company " + array[num].FieldName;
				}
				templateFields.Values = new object[1] { array[num].Value };
				list.Add(templateFields);
			}
			headerGroup.Fields = list.ToArray();
			list2.Add(headerGroup);
		}
		return list2;
	}

	private List<TemplateFields> GetTotalFields(List<HeaderGroup> lstHeaderGrp, List<HeaderGroup> lstBodyGrp)
	{
		List<TemplateFields> list = new List<TemplateFields>();
		if (lstHeaderGrp != null && lstHeaderGrp.Count > 0)
		{
			for (int i = 0; i < lstHeaderGrp.Count; i++)
			{
				list.AddRange(lstHeaderGrp[i].Fields);
			}
		}
		if (lstBodyGrp != null && lstBodyGrp.Count > 0)
		{
			for (int i = 0; i < lstBodyGrp.Count; i++)
			{
				list.AddRange(lstBodyGrp[i].Fields);
			}
		}
		return list;
	}

	private IdNamePair[] GetDocFields(int iVoucherType, bool bLinkVoucher, bool IsCaption = true)
	{
		VWVoucherFields[] array = m_arrDocumentFields;
		if ((m_arrDocumentFields == null) | bLinkVoucher)
		{
			if (bLinkVoucher)
			{
				array = (VWVoucherFields[])CallServeRequestTrans(TransMethods.GetDocFields, iVoucherType, m_iLanguageId);
			}
			else if (m_arrDocumentFields == null)
			{
				m_arrDocumentFields = (VWVoucherFields[])CallServeRequestTrans(TransMethods.GetDocFields, iVoucherType, m_iLanguageId);
				array = m_arrDocumentFields;
			}
		}
		if (array == null)
		{
			array = new VWVoucherFields[0];
		}
		return array.Select((VWVoucherFields p) => new IdNamePair
		{
			ID = p.iFieldId,
			Name = ((!IsCaption) ? p.sFieldName : ((!string.IsNullOrEmpty(p.sCaption)) ? p.sCaption : ((p.iFieldId == 23) ? "Item" : p.sFieldName))),
			Tag = new TranMasterInfo
			{
				IsBodyField = p.bBody,
				DataType = p.iDataTypeId,
				MasterType = ((p.iMasterTypeId == -1 && p.iMasterLink > 0) ? p.iMasterLink : p.iMasterTypeId),
				Filter = ((string.IsNullOrEmpty(p.sFilterCondition) && !string.IsNullOrEmpty(p.sDefaultValue)) ? p.sDefaultValue : p.sFilterCondition)
			}
		}).ToArray();
	}

	private void ChangeBodyDataIfAppliedClubBy(List<IdValuePair> arrBodySequence)
	{
		if (arrBodySequence != null && arrBodySequence.Count > 0)
		{
			arrBodySequence = arrBodySequence.OrderBy((IdValuePair p) => p.Value).ToList();
			List<TransBody> list = new List<TransBody>();
			for (int num = 0; num < arrBodySequence.Count; num++)
			{
				list.Add(m_objTranData.BodyData[arrBodySequence[num].ID]);
			}
		}
	}

	private List<HeaderGroup> GetBarcodeFields()
	{
		HeaderGroup headerGroup = null;
		List<HeaderGroup> list = null;
		List<TemplateFields> list2 = null;
		list = new List<HeaderGroup>();
		headerGroup = new HeaderGroup();
		headerGroup.MasterId = 2;
		headerGroup.GroupName = "Barcode";
		list2 = new List<TemplateFields>();
		IsDontConvertField = true;
		list2.Add(GetTemplateField(26, "Quantity", MasterDataType.Fraction));
		list2.Add(GetTemplateField(41, "Alternate Quantity", MasterDataType.Fraction));
		list2.Add(GetTemplateField(28, "Gross", MasterDataType.Fraction));
		list2.Add(GetTemplateField(27, "Rate", MasterDataType.Fraction));
		list2.Add(GetTemplateField(38, "RMA No", MasterDataType.Text));
		list2.Add(GetTemplateField(13, "Batch No", MasterDataType.Text));
		list2.Add(GetTemplateField(24, "Unit", MasterDataType.Master));
		list2.Add(GetTemplateField(23, "Item", MasterDataType.Master));
		BarCodeTemplate[] array = (BarCodeTemplate[])CallServeRequestCommon(CommonMethods.LoadBarCode);
		string[] arrBarcode = (string[])CallServeRequestCommon(CommonMethods.GetBarcodeTextTrans, m_objTranData);
		DistinctBarcode(ref arrBarcode);
		for (int i = 0; i < array.Length && i < 5; i++)
		{
			list2.Add(GetTemplateField(array[i].TemplateId, "Barcode [" + array[i].TemplateName + "]", MasterDataType.Text, arrBarcode));
		}
		IsDontConvertField = false;
		headerGroup.Fields = list2.ToArray();
		list.Add(headerGroup);
		return list;
	}

	private TemplateFields GetTemplateField(int iFieldId, string sFieldName, MasterDataType oDataType, string[] arrBarcode = null)
	{
		int num = 0;
		string text = null;
		List<object> list = new List<object>();
		TemplateFields templateFields = new TemplateFields();
		templateFields.FieldId = iFieldId;
		templateFields.Name = sFieldName;
		templateFields.DataType = oDataType;
		templateFields.MasterType = ((oDataType == MasterDataType.Master) ? 2 : (-1));
		if (arrBarcode != null && arrBarcode.Length != 0)
		{
			for (num = 0; num < arrBarcode.Length; num++)
			{
				text = arrBarcode[num].Replace("*", "");
				list.Add(text);
			}
		}
		else if (m_objTranData != null)
		{
			for (num = 0; num < m_objTranData.BodyData.Length; num++)
			{
				list.Add(GetBodyValue(templateFields.FieldId, templateFields.DataType, templateFields.MasterType, m_objTranData, num));
			}
		}
		else
		{
			list.Add(0);
		}
		templateFields.Values = list.ToArray();
		return templateFields;
	}

	private void DistinctBarcode(ref string[] arrBarcode)
	{
		string text = null;
		List<string> list = new List<string>();
		if (arrBarcode == null || arrBarcode.Length == 0)
		{
			return;
		}
		for (int i = 0; i < arrBarcode.Length; i++)
		{
			if (arrBarcode[i] != text)
			{
				list.Add(arrBarcode[i]);
				text = arrBarcode[i];
			}
		}
		arrBarcode = list.ToArray();
	}
}
