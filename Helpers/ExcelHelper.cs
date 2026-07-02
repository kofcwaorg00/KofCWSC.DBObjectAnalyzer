using ClosedXML.Excel;

namespace KofCWSC.DBObjectAnalyzer.Helpers;

public static class ExcelHelper
{
    public static void CreateTitle(
        IXLWorksheet worksheet,
        string title)
    {
        var cell = worksheet.Cell(1, 1);

        cell.Value = title;

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 16;

        worksheet.Range(1, 1, 1, 8).Merge();
    }

    public static void CreateHeader(
        IXLWorksheet worksheet,
        int row,
        params string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(row, i + 1);

            cell.Value = headers[i];

            cell.Style.Font.Bold = true;

            cell.Style.Fill.BackgroundColor =
                XLColor.LightSteelBlue;

            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            cell.Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
        }
    }

    public static void FreezeHeader(
        IXLWorksheet worksheet,
        int row)
    {
        worksheet.SheetView.FreezeRows(row);
    }

    public static void AutoFit(
        IXLWorksheet worksheet)
    {
        worksheet.Columns().AdjustToContents();
    }

    public static void CreateTable(
        IXLWorksheet worksheet,
        int firstRow,
        int firstColumn,
        int lastRow,
        int lastColumn)
    {
        var range = worksheet.Range(
            firstRow,
            firstColumn,
            lastRow,
            lastColumn);

        var table = range.CreateTable();

        table.Theme =
            XLTableTheme.TableStyleMedium2;

        table.ShowAutoFilter = true;
    }

    public static void HighlightCandidate(IXLCell cell)
    {
        cell.Style.Fill.BackgroundColor =
            XLColor.LightSalmon;
    }

    public static void HighlightReferenced(IXLCell cell)
    {
        cell.Style.Fill.BackgroundColor =
            XLColor.LightGreen;
    }
}