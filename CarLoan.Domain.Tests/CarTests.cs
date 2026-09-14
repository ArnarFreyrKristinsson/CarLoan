using CarLoan.Domain.Models;
using Xunit;

namespace CarLoan.Domain.Tests;

public class CarTests
{
    [Fact]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenAgeInYearsIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Car(CarCondition.Used, VehicleCategory.PetrolOrDiesel, -1));
    }

    [Fact]
    public void Constructor_CreatesCar_WhenValidValuesProvided()
    {
        var car = new Car(CarCondition.Used, VehicleCategory.ElectricOrHydrogen, 5);

        Assert.Equal(CarCondition.Used, car.Condition);
        Assert.Equal(VehicleCategory.ElectricOrHydrogen, car.Category);
        Assert.Equal(5, car.AgeInYears);
    }

    [Fact]
    public void Constructor_CreatesCar_WhenAgeInYearsIsZero()
    {
        var car = new Car(CarCondition.New, VehicleCategory.PetrolOrDiesel, 0);

        Assert.Equal(0, car.AgeInYears);
    }
}
