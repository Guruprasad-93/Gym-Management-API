using Gym.Application.DTOs.Attendance;

namespace Gym.Application.Interfaces;

public interface IAttendanceReportExporter
{
    byte[] ExportDailyReportPdf(DailyAttendanceReportDto report);
    byte[] ExportMonthlyReportPdf(MonthlyAttendanceReportDto report);
    byte[] ExportMemberHistoryExcel(IReadOnlyList<MemberAttendanceDto> records, string memberName);
    byte[] ExportDailyReportExcel(DailyAttendanceReportDto report);
    byte[] ExportMonthlyReportExcel(MonthlyAttendanceReportDto report);
}
