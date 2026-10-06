using System.Collections.Generic;
using System.Data;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public class ReportRecordSet
{
	private IDataReader objReader;

	private IdNamePair[] arrColumn;

	private IdValuePair[] arrOutput;

	private FieldData[] arrExColumn;

	private string sSessionId;

	private int iFieldCount;

	private int iCompanyId;

	private string sReportName;

	private long lStartDateTime;

	private string objKey;

	private Stack<ComboData> arrMasters;

	public string KeyValue
	{
		get
		{
			return objKey;
		}
		set
		{
			objKey = value;
		}
	}

	public string SessionId
	{
		get
		{
			return sSessionId;
		}
		set
		{
			sSessionId = value;
		}
	}

	public int FieldCount
	{
		get
		{
			return iFieldCount;
		}
		set
		{
			iFieldCount = value;
		}
	}

	public int CompanyId
	{
		get
		{
			return iCompanyId;
		}
		set
		{
			iCompanyId = value;
		}
	}

	public string ReportName
	{
		get
		{
			return sReportName;
		}
		set
		{
			sReportName = value;
		}
	}

	public long StartDate
	{
		get
		{
			return lStartDateTime;
		}
		set
		{
			lStartDateTime = value;
		}
	}

	public IdNamePair[] Columns
	{
		get
		{
			return arrColumn;
		}
		set
		{
			arrColumn = value;
		}
	}

	public FieldData[] ExtraColumns
	{
		get
		{
			return arrExColumn;
		}
		set
		{
			arrExColumn = value;
		}
	}

	public IDataReader Reader
	{
		get
		{
			return objReader;
		}
		set
		{
			objReader = value;
		}
	}

	public Stack<ComboData> SelectedMasters
	{
		get
		{
			return arrMasters;
		}
		set
		{
			arrMasters = value;
		}
	}

	public IdValuePair[] BalanceOutput
	{
		get
		{
			return arrOutput;
		}
		set
		{
			arrOutput = value;
		}
	}
}
