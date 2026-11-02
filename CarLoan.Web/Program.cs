using CarLoan.Application;
using CarLoan.Domain.Calculators;
using CarLoan.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The engine is in-process and stateless, so one instance serves every circuit (docs/api-contract.md).
builder.Services.AddSingleton<IMultiLenderLoanApplicationService>(
    new MultiLenderLoanApplicationService(new LoanCalculator(), LenderProfiles.Build()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
