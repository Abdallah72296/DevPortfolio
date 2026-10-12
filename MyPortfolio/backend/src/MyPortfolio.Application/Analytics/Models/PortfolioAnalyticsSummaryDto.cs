namespace MyPortfolio.Application.Analytics.Models;

public record PortfolioAnalyticsSummaryDto(
    int MonthlyPortfolioViews,
    int MonthlyCvDownloads,
    int MonthlyContactMessages,
    int UnreadContactMessages,
    int TotalContactMessages,
    DateTime PeriodStartUtc);
