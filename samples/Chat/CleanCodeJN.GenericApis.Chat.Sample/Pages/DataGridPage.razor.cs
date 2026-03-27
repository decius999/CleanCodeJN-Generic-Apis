namespace CleanCodeJN.GenericApis.Chat.Sample.Pages;

public partial class DataGridPage
{
    private string Endpoint { get; } = "https://localhost:7132/graphql";

    private Dictionary<string, string> InvoiceHeaders { get; } = new()
    {
        ["Id"] = "Rechnungs-ID",
        ["CustomerId"] = "Kunden-ID",
        ["Amount"] = "Betrag (€)",
    };
}
