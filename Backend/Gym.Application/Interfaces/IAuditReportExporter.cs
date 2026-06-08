using Gym.Application.DTOs.Audit;

namespace Gym.Application.Interfaces;

public interface IAuditReportExporter
{
    byte[] ExportPdf(IReadOnlyList<AuditLogDto> logs, string title);
    byte[] ExportExcel(IReadOnlyList<AuditLogDto> logs, string title);
}
