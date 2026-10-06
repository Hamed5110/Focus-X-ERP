using System.Collections.Generic;
using System.Linq;
using Focus.Common.DataStructs;
using Focus.RD.DataStructs;

namespace Focus.RD.BL;

public class ReportPageSet
{
	private bool bEndOfFile;

	private string objKey;

	private string objSession;

	private IdNamePair[] arrColumn;

	private FieldData[] arrExColumn;

	private Dictionary<int, LineData[]> arrPageData;

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
			return objSession;
		}
		set
		{
			objSession = value;
		}
	}

	public bool EndOfFile
	{
		get
		{
			return bEndOfFile;
		}
		set
		{
			bEndOfFile = value;
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

	public LineData[] GetPageData(int iPageIndex)
	{
		LineData[] value = null;
		if (arrPageData != null && arrPageData.ContainsKey(iPageIndex))
		{
			arrPageData.TryGetValue(iPageIndex, out value);
		}
		return value;
	}

	public void SetPageData(int iPageIndex, LineData[] oPageData)
	{
		if (arrPageData == null)
		{
			arrPageData = new Dictionary<int, LineData[]>();
		}
		if (arrPageData.ContainsKey(iPageIndex))
		{
			List<LineData> list = arrPageData[iPageIndex].ToList();
			list.AddRange(oPageData);
			arrPageData[iPageIndex] = list.ToArray();
		}
		else
		{
			arrPageData.Add(iPageIndex, oPageData);
		}
	}

	public ReportPageSet(string oKey, string oSession)
	{
		objKey = oKey;
		objSession = oSession;
	}

	public void CloseReader()
	{
		if (arrPageData != null)
		{
			arrPageData.Clear();
			arrPageData = null;
		}
	}
}
