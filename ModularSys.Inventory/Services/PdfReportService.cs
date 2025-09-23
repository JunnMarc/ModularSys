using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ModularSys.Inventory.Models;
using ModularSys.Inventory.Interface;

namespace ModularSys.Inventory.Services;

public class PdfReportService
{
    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] GenerateAnalyticalReportPdf(AnalyticalReportRequest request, object reportData)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Height(100)
                    .Background(Colors.Blue.Lighten3)
                    .Padding(20)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("ModularSys Enterprise")
                                .FontSize(20)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken2);
                            
                            column.Item().Text("123 Business District, Metro Manila, Philippines")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                            
                            column.Item().Text("Phone: +63 2 8123 4567 | Email: info@modularsys.com")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().Column(column =>
                        {
                            column.Item().AlignRight().Text($"{request.ReportType.ToString().Replace("_", " ")} Report")
                                .FontSize(16)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken2);
                            
                            column.Item().AlignRight().Text($"Report Date: {DateTime.Now:MMMM dd, yyyy}")
                                .FontSize(9);
                            
                            column.Item().AlignRight().Text($"Period: {request.StartDate:MMM dd} - {request.EndDate:MMM dd, yyyy}")
                                .FontSize(9);
                            
                            column.Item().AlignRight().Text("Prepared by: System Administrator")
                                .FontSize(9);
                        });
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Executive Summary
                        column.Item().Text("Executive Summary")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(10).Row(row =>
                        {
                            switch (request.ReportType)
                            {
                                case AnalyticalReportType.InventoryValuation when reportData is InventoryValuationReport valuation:
                                    AddInventoryValuationSummary(row, valuation);
                                    break;
                                case AnalyticalReportType.CostOfGoodsSold when reportData is COGSReport cogs:
                                    AddCOGSSummary(row, cogs);
                                    break;
                                case AnalyticalReportType.InventoryTurnover when reportData is InventoryTurnoverReport turnover:
                                    AddTurnoverSummary(row, turnover);
                                    break;
                                default:
                                    row.RelativeItem().Text("Report data will be displayed here.")
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken1);
                                    break;
                            }
                        });

                        // Detailed Analysis
                        column.Item().PaddingTop(20).Text("Detailed Analysis")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(10).Column(detailColumn =>
                        {
                            switch (request.ReportType)
                            {
                                case AnalyticalReportType.InventoryValuation when reportData is InventoryValuationReport valuation:
                                    AddInventoryValuationDetails(detailColumn, valuation);
                                    break;
                                case AnalyticalReportType.CostOfGoodsSold when reportData is COGSReport cogs:
                                    AddCOGSDetails(detailColumn, cogs);
                                    break;
                                case AnalyticalReportType.InventoryTurnover when reportData is InventoryTurnoverReport turnover:
                                    AddTurnoverDetails(detailColumn, turnover);
                                    break;
                            }
                        });

                        // GAAP Compliance Notes
                        column.Item().PaddingTop(20).Text("GAAP Compliance Notes")
                            .FontSize(14)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken2);

                        column.Item().PaddingTop(10).Column(complianceColumn =>
                        {
                            complianceColumn.Item().Text("• This report follows Generally Accepted Accounting Principles (GAAP)")
                                .FontSize(10);
                            complianceColumn.Item().Text("• Inventory valuation uses Weighted Average Cost method")
                                .FontSize(10);
                            complianceColumn.Item().Text("• COGS calculation: Beginning Inventory + Purchases - Ending Inventory")
                                .FontSize(10);
                            complianceColumn.Item().Text("• All figures are in Philippine Pesos (₱)")
                                .FontSize(10);
                        });
                    });

                page.Footer()
                    .Height(50)
                    .Padding(20)
                    .Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on {DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);

                        row.RelativeItem().AlignRight().Text("ModularSys Inventory Management System")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Darken1);
                    });
            });
        }).GeneratePdf();
    }

    private static void AddInventoryValuationSummary(RowDescriptor row, InventoryValuationReport report)
    {
        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Total Inventory Value")
                .FontSize(10)
                .FontColor(Colors.Green.Darken2);
            column.Item().Text($"₱{report.TotalInventoryValue:N2}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Categories")
                .FontSize(10)
                .FontColor(Colors.Blue.Darken2);
            column.Item().Text($"{report.CategoryBreakdown.Count}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Total SKUs")
                .FontSize(10)
                .FontColor(Colors.Orange.Darken2);
            column.Item().Text($"{report.Items.Count}")
                .FontSize(16)
                .SemiBold();
        });
    }

    private static void AddCOGSSummary(RowDescriptor row, COGSReport report)
    {
        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Net Sales")
                .FontSize(10)
                .FontColor(Colors.Green.Darken2);
            column.Item().Text($"₱{report.NetSales:N2}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Red.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("COGS")
                .FontSize(10)
                .FontColor(Colors.Red.Darken2);
            column.Item().Text($"₱{report.CostOfGoodsSold:N2}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Gross Profit")
                .FontSize(10)
                .FontColor(Colors.Blue.Darken2);
            column.Item().Text($"₱{report.GrossProfit:N2}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Purple.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Profit Margin")
                .FontSize(10)
                .FontColor(Colors.Purple.Darken2);
            column.Item().Text($"{report.GrossProfitMargin:F1}%")
                .FontSize(16)
                .SemiBold();
        });
    }

    private static void AddTurnoverSummary(RowDescriptor row, InventoryTurnoverReport report)
    {
        row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Turnover Ratio")
                .FontSize(10)
                .FontColor(Colors.Green.Darken2);
            column.Item().Text($"{report.OverallTurnoverRatio:F1}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Days in Inventory")
                .FontSize(10)
                .FontColor(Colors.Blue.Darken2);
            column.Item().Text($"{report.DaysSalesInInventory:F0}")
                .FontSize(16)
                .SemiBold();
        });

        row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("Avg Inventory Value")
                .FontSize(10)
                .FontColor(Colors.Orange.Darken2);
            column.Item().Text($"₱{report.AverageInventoryValue:N2}")
                .FontSize(16)
                .SemiBold();
        });
    }

    private static void AddInventoryValuationDetails(ColumnDescriptor column, InventoryValuationReport report)
    {
        column.Item().PaddingBottom(5).Text("Category Breakdown")
            .FontSize(12)
            .SemiBold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Products").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Quantity").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total Value").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("% of Total").SemiBold();
            });

            foreach (var category in report.CategoryBreakdown.Take(10))
            {
                table.Cell().Padding(5).Text(category.CategoryName);
                table.Cell().Padding(5).Text($"{category.ProductCount}");
                table.Cell().Padding(5).Text($"{category.TotalQuantity:N0}");
                table.Cell().Padding(5).Text($"₱{category.TotalValue:N2}");
                table.Cell().Padding(5).Text($"{category.PercentageOfTotal:F1}%");
            }
        });
    }

    private static void AddCOGSDetails(ColumnDescriptor column, COGSReport report)
    {
        column.Item().PaddingBottom(5).Text("COGS by Category")
            .FontSize(12)
            .SemiBold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("COGS").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Revenue").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Gross Profit").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Margin %").SemiBold();
            });

            foreach (var category in report.CategoryBreakdown.Take(10))
            {
                table.Cell().Padding(5).Text(category.CategoryName);
                table.Cell().Padding(5).Text($"₱{category.COGS:N2}");
                table.Cell().Padding(5).Text($"₱{category.Revenue:N2}");
                table.Cell().Padding(5).Text($"₱{category.GrossProfit:N2}");
                table.Cell().Padding(5).Text($"{category.GrossProfitMargin:F1}%");
            }
        });
    }

    private static void AddTurnoverDetails(ColumnDescriptor column, InventoryTurnoverReport report)
    {
        column.Item().PaddingBottom(5).Text("Top Products by Turnover")
            .FontSize(12)
            .SemiBold();

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Product").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Category").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Turnover Ratio").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Days in Inventory").SemiBold();
                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Classification").SemiBold();
            });

            foreach (var product in report.ProductTurnover.Take(10))
            {
                table.Cell().Padding(5).Text(product.ProductName);
                table.Cell().Padding(5).Text(product.Category);
                table.Cell().Padding(5).Text($"{product.TurnoverRatio:F2}");
                table.Cell().Padding(5).Text($"{product.DaysSalesInInventory:F0}");
                table.Cell().Padding(5).Text(product.TurnoverClassification);
            }
        });
    }
}
