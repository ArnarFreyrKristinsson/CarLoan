namespace CarLoan.Domain.Models;

/// <summary>
/// Vehicle categories V1-V3. V1 and V2 share the general rate table, V3 uses the green table.
/// </summary>
public enum VehicleCategory
{
    /// <summary>V1 — petrol or diesel.</summary>
    PetrolOrDiesel,

    /// <summary>V2 — plug-in hybrid. Not green.</summary>
    PlugInHybrid,

    /// <summary>V3 — electric or hydrogen running on 100% renewable energy.</summary>
    ElectricOrHydrogen
}
