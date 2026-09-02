using CateringApp.Models.Entity;
using System.Collections.Generic;

namespace CateringApp.Models.ViewModel;

public class PesananPagedResult
{
    public List<Pesanan> Items { get; set; } = new();
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
