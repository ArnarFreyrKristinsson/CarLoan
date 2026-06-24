using CarLoan.Application;
using CarLoan.Domain.Calculators;
using CarLoan.Domain.Models;
using CarLoan.UI.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ILoanTerms>(new LoanTerms(
    MinimumLoanAmount: 750_000m,
    PurchasePrice: 2_000_000m,
    DownPayment: 500_000m,
    LoanPeriodInMonths: 6,
    LoanRatio: 75m));
builder.Services.AddSingleton<ILoanCalculator, LoanCalculator>();
builder.Services.AddSingleton<ILoanApplicationService, LoanApplicationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
