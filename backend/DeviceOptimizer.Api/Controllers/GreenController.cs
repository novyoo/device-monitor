using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;
using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Api.Controllers
{
    [ApiController]
    [Route("api/green")]
    [Authorize]
    public class GreenController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public GreenController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet("report")]
        public async Task<ActionResult<GreenReportDto>> GetReport()
        {
            var report = await BuildReportAsync();
            return Ok(report);
        }

        [HttpGet("report/pdf")]
        public async Task<IActionResult> DownloadReportPdf()
        {
            var report = await BuildReportAsync();
            var pdfBytes = BuildPdf(report);
            return File(pdfBytes, "application/pdf", "fleetpulse-green-report.pdf");
        }

        private async Task<GreenReportDto> BuildReportAsync()
        {
            var user = (await _userManager.GetUserAsync(User))!;
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var tenants = await _db.Tenants
                .Where(t => isAdmin || t.Id == user.TenantId)
                .ToListAsync();
            var devices = await _db.Devices.ToListAsync();
            var footprints = await _db.ModelFootprints.ToListAsync();
            var manufacturingKgCo2eByModel = footprints.ToDictionary(f => f.Model, f => f.ManufacturingKgCo2e);

            var tenantSummaries = new List<GreenTenantSummaryDto>();
            var fleetAvoidedCo2Kg = 0.0;

            foreach (var tenant in tenants)
            {
                var tenantDevices = devices.Where(d => d.TenantId == tenant.Id).ToList();
                var tenantAvoidedCo2Kg = 0.0;

                foreach (var device in tenantDevices)
                {
                    var manufacturingKgCo2e = manufacturingKgCo2eByModel.TryGetValue(device.Model, out var kg) ? kg : 0;
                    tenantAvoidedCo2Kg += GreenReportCalculator.AvoidedCo2ForDevice(device.PurchaseDate, manufacturingKgCo2e);
                }

                var tenantResult = GreenReportCalculator.Summarize(tenantAvoidedCo2Kg);
                tenantSummaries.Add(new GreenTenantSummaryDto
                {
                    TenantName = tenant.Name,
                    DeviceCount = tenantDevices.Count,
                    AvoidedCo2Kg = tenantResult.AvoidedCo2Kg,
                    TreesEquivalent = tenantResult.TreesEquivalent,
                    CarKmEquivalent = tenantResult.CarKmEquivalent
                });

                fleetAvoidedCo2Kg += tenantAvoidedCo2Kg;
            }

            var fleetResult = GreenReportCalculator.Summarize(fleetAvoidedCo2Kg);

            return new GreenReportDto
            {
                FleetAvoidedCo2Kg = fleetResult.AvoidedCo2Kg,
                FleetTreesEquivalent = fleetResult.TreesEquivalent,
                FleetCarKmEquivalent = fleetResult.CarKmEquivalent,
                Tenants = tenantSummaries.OrderByDescending(t => t.AvoidedCo2Kg).ToList()
            };
        }

        private static byte[] BuildPdf(GreenReportDto report)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Text("PULSLE Green Report").FontSize(20).Bold();

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().Text($"Generated {DateTime.UtcNow:MMMM d, yyyy}");

                        column.Item().Text(
                            $"Fleet-wide, keeping devices in service longer than the typical 3-year replacement " +
                            $"cycle has avoided an estimated {report.FleetAvoidedCo2Kg:0.0} kg of CO2e so far — " +
                            $"about the same as {report.FleetTreesEquivalent:0.0} trees absorbing CO2 for a year, " +
                            $"or {report.FleetCarKmEquivalent:0} km not driven by an average car.");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Tenant").Bold();
                                header.Cell().Text("Devices").Bold();
                                header.Cell().Text("CO2 Avoided (kg)").Bold();
                                header.Cell().Text("Trees/year").Bold();
                                header.Cell().Text("Car km").Bold();
                            });

                            foreach (var tenant in report.Tenants)
                            {
                                table.Cell().Text(tenant.TenantName);
                                table.Cell().Text(tenant.DeviceCount.ToString());
                                table.Cell().Text(tenant.AvoidedCo2Kg.ToString("0.0"));
                                table.Cell().Text(tenant.TreesEquivalent.ToString("0.0"));
                                table.Cell().Text(tenant.CarKmEquivalent.ToString("0"));
                            }
                        });

                        column.Item().Text(
                            "These figures are estimates based on published manufacturer environmental reports for " +
                            "typical laptop manufacturing emissions, not measurements of these exact devices.")
                            .FontSize(9).Italic();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}
