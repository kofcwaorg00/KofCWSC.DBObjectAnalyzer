using ClosedXML.Excel;
using KofCWSC.DBObjectAnalyzer.Models;

namespace KofCWSC.DBObjectAnalyzer.Excel;

public class ExcelWorksheet
{
    private readonly IXLWorksheet _worksheet;

    private int _currentRow;

    private int _columnCount;

    public ExcelWorksheet(XLWorkbook workbook, string worksheetName)
    {
        _worksheet = workbook.Worksheets.Add(worksheetName);

        _currentRow = 1;
    }

    public IXLWorksheet Worksheet => _worksheet;

    public int CurrentRow => _currentRow;

    public void Title(string title)
    {
        var cell = _worksheet.Cell(_currentRow, 1);

        cell.Value = title;

        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 16;

        _currentRow += 2;
    }

    public void Headers(params string[] headers)
    {
        _columnCount = headers.Length;

        for (int column = 0; column < headers.Length; column++)
        {
            var cell = _worksheet.Cell(_currentRow, column + 1);

            cell.Value = headers[column];

            cell.Style.Font.Bold = true;

            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            cell.Style.Fill.BackgroundColor =
                XLColor.LightSteelBlue;

            cell.Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
        }

        _currentRow++;
    }

    public void Row(params object?[] values)
    {
        for (int column = 0; column < values.Length; column++)
        {
            _worksheet.Cell(_currentRow, column + 1).Value =
                values[column]?.ToString() ?? string.Empty;
        }

        _currentRow++;
    }

    public void HighlightStatus(
        bool candidate,
        int column)
    {
        var cell = _worksheet.Cell(_currentRow - 1, column);

        cell.Style.Fill.BackgroundColor =
            candidate
                ? XLColor.LightSalmon
                : XLColor.LightGreen;
    }

    public void AutoFit()
    {
        _worksheet.Columns().AdjustToContents();
    }

    public void FreezeHeader()
    {
        //
        // Title occupies Row 1.
        // Blank Row 2.
        // Header Row 3.
        //
        _worksheet.SheetView.FreezeRows(3);
    }

    public void CreateTable()
    {
        if (_columnCount == 0)
            return;

        int headerRow = 3;

        int lastRow = _currentRow - 1;

        if (lastRow <= headerRow)
            return;

        var range = _worksheet.Range(
            headerRow,
            1,
            lastRow,
            _columnCount);

        var table = range.CreateTable();

        table.Theme = XLTableTheme.TableStyleMedium2;

        table.ShowAutoFilter = true;
    }

    public void SetColumnWidth(
        int column,
        double width)
    {
        _worksheet.Column(column).Width = width;
    }

    public void CenterColumn(
        int column)
    {
        _worksheet.Column(column)
            .Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
    }

    public void RightAlignColumn(
        int column)
    {
        _worksheet.Column(column)
            .Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Right;
    }

    public void BoldRow(
        int row)
    {
        _worksheet.Row(row)
            .Style.Font.Bold = true;
    }

    public void Finish()
    {
        CreateTable();

        FreezeHeader();

        AutoFit();
    }
}