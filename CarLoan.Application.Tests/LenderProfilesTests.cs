namespace CarLoan.Application.Tests;

public class LenderProfilesTests
{
    [Fact]
    public void Build_ReturnsConfiguredLykillProfile_WhenCalled()
    {
        var profiles = LenderProfiles.Build();

        var lykill = profiles["Lykill"];

        Assert.Equal("Lykill", lykill.Name);
        Assert.Equal(4, lykill.Rules.Count);
        Assert.Equal(10.35m, lykill.RateProvider.GetInterestRate(1000000m));
        Assert.Equal(12.20m, lykill.RateProvider.GetInterestRate(150000m));
    }
}
