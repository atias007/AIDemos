using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SemanticKernel;

internal class SupplierPlugin(NorthwindContext context)
{
    [KernelFunction("get_suppliers")]
    [Description("Gets a list of all suppliers and their active status")]
    [return: Description("List of suppliers information")]
    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await context.Suppliers.ToListAsync();
    }

    [KernelFunction("change_supplier_state")]
    [Description("Changes the active status of the supplier")]
    public async Task<Supplier?> ChangeStateAsync(int id, bool active)
    {
        var supplier = await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierID == id);
        if (supplier == null) { return null; }

        await context.Suppliers
             .Where(s => s.SupplierID == id)
             .ExecuteUpdateAsync(l => l.SetProperty(p => p.Active, active));

        supplier.Active = active;
        return supplier;
    }
}