using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adriva.Extensions.Reporting.Abstractions;
using ClosedXML.Excel;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Excel;

public class XlsxReportRenderer : ReportRenderer
{
    private readonly IEnumerable<IReportRepository> Repositories;

    private static string NormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Report";
        }
        else
        {
            StringBuilder titleBuffer = new();

            foreach (var charTitle in title)
            {
                if (char.IsLetterOrDigit(charTitle) || ' ' == charTitle)
                {
                    titleBuffer.Append(charTitle);
                }
                else if (char.IsWhiteSpace(charTitle))
                {
                    titleBuffer.Append(' ');
                }
            }

            return titleBuffer.ToString();
        }
    }

    public XlsxReportRenderer(IEnumerable<IReportRepository> repositories)
    {
        this.Repositories = repositories;
    }

    protected virtual void RenderHeaders(IXLWorksheet worksheet, ReadOnlyCollection<DataColumn> columns)
    {
        var headerRange = worksheet.Cell(1, 1).InsertData(columns.Select(c => c.DisplayName ?? c.Name), true);
        headerRange.Style.Font.Bold = true;
    }

    protected virtual void RenderData(IXLWorksheet worksheet, ReadOnlyCollection<DataColumn> columns, ReadOnlyCollection<DataRow> rows)
    {
        object[] dataArray = new object[columns.Count];

        worksheet.Cell(2, 1).InsertData(rows.Select(r =>
        {
            int loop = 0;
            foreach (var column in columns)
            {
                dataArray[loop++] = r[column.Name.ToUpperInvariant()];
            }
            return dataArray;
        }));
    }

    protected virtual XlsxReportRendererOptions ResolveOptions(OutputDefinition outputDefinition)
    {
        var rendererOptionsJson = outputDefinition.Options?
                                                    .Children<JProperty>()
                                                    .FirstOrDefault(x => x.Name.Equals(XlsxReportRendererOptions.KeyName, StringComparison.OrdinalIgnoreCase));

        if (null != rendererOptionsJson?.Value)
        {
            return rendererOptionsJson.Value.ToObject<XlsxReportRendererOptions>();
        }
        else
        {
            return new XlsxReportRendererOptions();
        }
    }

    protected virtual void ApplyOptions(IXLWorksheet worksheet, XlsxReportRendererOptions options)
    {
        if (options.CreateTable)
        {
            var reportTable = worksheet.RangeUsed().CreateTable("ReportTable");
            reportTable.Theme = XLTableTheme.TableStyleLight1;
        }
    }

    protected virtual void RenderData(string title, XlsxReportRendererOptions rendererOptions, ReportOutput output, Stream stream)
    {
        using (var workbook = new XLWorkbook(new LoadOptions() { RecalculateAllFormulas = false }))
        {
            workbook.CalculationOnSave = false;

            var worksheet = workbook.AddWorksheet(title);

            if (null != output?.DataSet && null != output.DataSet.Columns && null != output.DataSet.Rows)
            {
                if (0 < output.DataSet.Columns.Count)
                {
                    this.RenderHeaders(worksheet, output.DataSet.Columns);
                }

                if (0 < output.DataSet.Rows.Count)
                {
                    this.RenderData(worksheet, output.DataSet.Columns, output.DataSet.Rows);
                    this.ApplyOptions(worksheet, rendererOptions);
                }
            }

            workbook.SaveAs(stream);
        }
    }

    protected virtual async Task<RepositoryFile> ResolveTemplateFileAsync(string templatePath)
    {
        RepositoryFile repositoryFile = RepositoryFile.NotExists;

        foreach (var repository in this.Repositories)
        {
            try
            {
                return await repository.GetRepositoryFileAsync(templatePath, false);
            }
            catch
            {
                repositoryFile = RepositoryFile.NotExists;
            }
        }

        return repositoryFile;
    }

    protected virtual async ValueTask RenderDataFromTemplateAsync(string title, XlsxReportRendererOptions rendererOptions, ReportOutput output, Stream stream)
    {
        RepositoryFile templateFile = await this.ResolveTemplateFileAsync(rendererOptions.TemplatePath);

        if (RepositoryFile.NotExists.Equals(templateFile))
        {
            throw new IOException($"Template file '{rendererOptions.TemplatePath}' could not be found in registered repositories.");
        }

        using (var workbook = new XLWorkbook(templateFile.Base, new LoadOptions() { RecalculateAllFormulas = false }))
        {
            workbook.CalculationOnSave = false;

            IXLTable targetTable = null;

            try
            {
                targetTable = workbook.Table(rendererOptions.TargetTableName);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentOutOfRangeException($"XlsxReportRenderer couldn't find a table named '{rendererOptions.TargetTableName}' in template file '{rendererOptions.TemplatePath}'.", e);
            }

            var columns = output.DataSet.Columns;
            var rows = output.DataSet.Rows;

            object[] dataArray = new object[columns.Count];

            targetTable.ReplaceData(rows.Select(r =>
            {
                int loop = 0;
                foreach (var column in columns)
                {
                    dataArray[loop++] = r[column.Name.ToUpperInvariant()];
                }
                return dataArray;
            }), true);

            workbook.SaveAs(stream, new SaveOptions()
            {
                EvaluateFormulasBeforeSaving = false
            });
        }
    }

    public override async ValueTask RenderAsync(string title, OutputDefinition outputDefinition, ReportOutput output, Stream stream)
    {
        title = XlsxReportRenderer.NormalizeTitle(title);

        XlsxReportRendererOptions rendererOptions = this.ResolveOptions(outputDefinition);

        if (string.IsNullOrWhiteSpace(rendererOptions.TemplatePath))
        {
            this.RenderData(title, rendererOptions, output, stream);
        }
        else
        {
            await this.RenderDataFromTemplateAsync(title, rendererOptions, output, stream);
        }
    }
}
