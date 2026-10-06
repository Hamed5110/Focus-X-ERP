namespace Focus.RD.BL;

public class InvoicePreferences
{
	public int Print_PrintZeroValueAsNumeric { get; set; }

	public int Misc_NumericSeparator { get; set; }

	public int Inventory_InputUnitInTransactions { get; set; }

	public int Tag_Inventory { get; set; }

	public int Tag_Accounts { get; set; }

	public int Print_PrintBooleanAs { get; set; }

	public int Misc_DefaultCurrencyId { get; set; }

	public int Batch_IgnoreDaysInExpiry { get; set; }

	public string FileFormat { get; set; }
}
