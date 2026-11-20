using System.IO.Compression;
using System.Text;

namespace MiniSoftware.Tests;

public class ExcelTableStyleTests
{
    [Fact]
    public void TableDataFormat_LayersOverWholeTableStyle()
    {
        // The table carries its own data format (dataDxfId, italic only) and a
        // table style whose wholeTable element defines a fill.
        using var stream = CreateWorkbookWithStyledTable();

        var sheet = Assert.Single(ExcelReader.ReadSheets(stream));

        var wholeTableFill = PdfColor.FromRgb(0xDD, 0xF4, 0xF9);
        Assert.All(new[] { sheet.Rows[1][0], sheet.Rows[1][1], sheet.Rows[2][0], sheet.Rows[2][1] }, cell =>
        {
            Assert.Equal(wholeTableFill, cell.FillColor);
            Assert.True(cell.Italic);
        });
        Assert.Null(sheet.Rows[1][2].FillColor);
        Assert.False(sheet.Rows[1][2].Italic);
    }

    private static MemoryStream CreateWorkbookWithStyledTable()
    {
        var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                    <Default Extension="xml" ContentType="application/xml"/>
                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                    <Override PartName="/xl/tables/table1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.table+xml"/>
                    <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                    <Override PartName="/xl/sharedStrings.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                    <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                    <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings" Target="sharedStrings.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <sheets>
                        <sheet name="Items" sheetId="1" r:id="rId1"/>
                    </sheets>
                </workbook>
                """);

            AddEntry(archive, "xl/worksheets/sheet1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                           xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <sheetData>
                        <row r="1"><c r="A1" t="s"><v>0</v></c><c r="B1" t="s"><v>1</v></c><c r="C1" t="s"><v>2</v></c></row>
                        <row r="2"><c r="A2" t="s"><v>3</v></c><c r="B2"><v>35</v></c><c r="C2" t="s"><v>2</v></c></row>
                        <row r="3"><c r="A3" t="s"><v>4</v></c><c r="B3"><v>25</v></c><c r="C3" t="s"><v>2</v></c></row>
                    </sheetData>
                    <tableParts count="1"><tablePart r:id="rId1"/></tableParts>
                </worksheet>
                """);

            AddEntry(archive, "xl/worksheets/_rels/sheet1.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/table" Target="../tables/table1.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/tables/table1.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <table xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                       id="1" name="Items" displayName="Items" ref="A1:B3" dataDxfId="0">
                    <tableColumns count="2">
                        <tableColumn id="1" name="Item"/>
                        <tableColumn id="2" name="Qty"/>
                    </tableColumns>
                    <tableStyleInfo name="Custom" showFirstColumn="0" showLastColumn="0" showRowStripes="0" showColumnStripes="0"/>
                </table>
                """);

            AddEntry(archive, "xl/styles.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                    <fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>
                    <fills count="2">
                        <fill><patternFill patternType="none"/></fill>
                        <fill><patternFill patternType="gray125"/></fill>
                    </fills>
                    <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
                    <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
                    <cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>
                    <dxfs count="2">
                        <dxf><font><i/></font></dxf>
                        <dxf><fill><patternFill><bgColor rgb="FFDDF4F9"/></patternFill></fill></dxf>
                    </dxfs>
                    <tableStyles count="1" defaultTableStyle="Custom">
                        <tableStyle name="Custom" pivot="0" count="1">
                            <tableStyleElement type="wholeTable" dxfId="1"/>
                        </tableStyle>
                    </tableStyles>
                </styleSheet>
                """);

            AddEntry(archive, "xl/sharedStrings.xml",
                """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <sst xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" count="5" uniqueCount="5">
                    <si><t>Item</t></si><si><t>Qty</t></si><si><t>Outside</t></si><si><t>Peonies</t></si><si><t>Tulips</t></si>
                </sst>
                """);
        }

        ms.Position = 0;
        return ms;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}
