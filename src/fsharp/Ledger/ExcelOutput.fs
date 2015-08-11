﻿module ExcelOutput

open InputTypes
open TextOutput
open OfficeOpenXml

type Destination = ExcelPackage option

let newFile (filename:string) =
    let f = new System.IO.FileInfo(filename)
    if f.Exists then
        f.Delete()
        new System.IO.FileInfo(filename)
    else
        f


let destination (filename: string) =
    match filename with
    | "" -> None
    | arg -> let filename = if arg.EndsWith(".xlsx") then arg else (arg + ".xlsx")
             let file = newFile (filename)
             let package = new ExcelPackage(file)
             Some package

let setHeader (cell: ExcelRangeBase) (header: string) =
    cell.Value <- header
    cell.Style.Font.Bold <- true
    cell.AutoFitColumns();

type Excel =
    static member setValue((cell: ExcelRangeBase),(str: string)) =
        cell.Value <- str

    static member setValue((cell: ExcelRangeBase),(a: Amount)) =
        match a with
        | AUD a ->
            cell.Value <- (0.01 * (float a))
            cell.Style.Numberformat.Format <- "\"$\"#,##0.00;[Red]\"$\"#,##0.00"

    static member write((report : ReportRegister.Report), (destination : Destination)) =
        match destination with
            | None  -> ()
            | Some package ->
                let worksheet = package.Workbook.Worksheets.Add("Running Balance - " + report.account)
                let mutable rowCount = 2
                worksheet.View.ShowGridLines <- true
                (setHeader worksheet.Cells.[1, 1] "Date")
                (setHeader worksheet.Cells.[1, 2] "Amount")
                (setHeader worksheet.Cells.[1, 3] "Balance")
                (setHeader worksheet.Cells.[1, 4] "Account")
                (setHeader worksheet.Cells.[1, 5] "Description")
                worksheet.View.FreezePanes(2,1)
                for line in report.lines do
                    Excel.setValue(worksheet.Cells.[rowCount, 1], line.date)
                    Excel.setValue(worksheet.Cells.[rowCount, 2], line.amount)
                    Excel.setValue(worksheet.Cells.[rowCount, 3], line.balance)
                    Excel.setValue(worksheet.Cells.[rowCount, 4], line.account)
                    Excel.setValue(worksheet.Cells.[rowCount, 5], line.description)
                    rowCount <- rowCount + 1
                for c in 1..5 do
                    worksheet.Column(c).AutoFit(0.0)
                package.Save()

    static member writeLine((line: ReportBalancesByDate.ReportBalancesByDateLine), (ws : ExcelWorksheet), (indent: int), (nextRow: int)) =
        let mutable column = 1
        for balance in line.amounts.balances do
            (Excel.setValue (ws.Cells.[nextRow, column], balance))
            column <- column + 1
        column <- column + 1
        for balance in line.amounts.differences do
            (Excel.setValue (ws.Cells.[nextRow, column], balance))
            column <- column + 1
        column <- column + 1
        Excel.setValue (ws.Cells.[nextRow, column+indent], line.account)
        if indent <> 0 then
            ws.Row(nextRow).OutlineLevel <- (indent)
        let rowAfterChildren = Excel.writeLines (line.subAccounts, ws, indent+1, nextRow+1)
        if rowAfterChildren <> nextRow then
            ws.Row(nextRow).Collapsed <- true
        rowAfterChildren

    static member writeLines((lines : ReportBalancesByDate.ReportBalancesByDateLine list), (ws : ExcelWorksheet), (indent: int), (nextRow: int)) =
        match lines with
            | [] -> nextRow
            | first::rest -> Excel.writeLines(rest, ws, indent, Excel.writeLine(first, ws, indent, nextRow))

    static member write((report : ReportBalancesByDate.ReportBalancesByDate), (destination : Destination)) =
        match destination with
            | None  -> ()
            | Some package ->
                let numDates = report.dates.Length
                let worksheet = package.Workbook.Worksheets.Add("Balances by Date")
                (setHeader worksheet.Cells.[1, 1] "Balance")
                for i in 1 .. numDates do
                    (setHeader worksheet.Cells.[2, i] report.dates.[i-1])
                (setHeader worksheet.Cells.[1, numDates+2] "Change")
                for i in 2 .. numDates do
                    (setHeader worksheet.Cells.[2, numDates+i] report.dates.[i-1])
                (setHeader worksheet.Cells.[2, numDates*2+2] "Account")
                Excel.writeLines(report.lines, worksheet, 0, 3) |> ignore
                worksheet.View.FreezePanes(3, 1)
                worksheet.OutLineSummaryBelow <- false
                package.Save()

    static member writeLine((line: ReportBalances.Line),
                            (ws : ExcelWorksheet),
                            (indent: int),
                            (nextRow: int)) =
      (Excel.setValue (ws.Cells.[nextRow, 1], line.balance))
      Excel.setValue (ws.Cells.[nextRow, 2+indent], line.account)
      if indent <> 0 then
        ws.Row(nextRow).OutlineLevel <- (indent)
      let rowAfterChildren = Excel.writeLines (line.subAccounts, ws, indent+1, nextRow+1)
      if rowAfterChildren <> nextRow then
        ws.Row(nextRow).Collapsed <- true
      rowAfterChildren

    static member writeLines((lines : ReportBalances.Line list),
                             (ws : ExcelWorksheet),
                             (indent: int),
                             (nextRow: int)) =
        match lines with
            | [] -> nextRow
            | first::rest -> Excel.writeLines(rest, ws, indent, Excel.writeLine(first, ws, indent, nextRow))

    static member write((report : ReportBalances.Report), (destination : Destination)) =
        match destination with
        | None -> ()
        | Some package ->
            let worksheet = package.Workbook.Worksheets.Add("Balances")
            (setHeader worksheet.Cells.[1, 1] "Balance")
            (setHeader worksheet.Cells.[1, 2] "Account")
            Excel.writeLines(report.lines, worksheet, 0, 2) |> ignore
            worksheet.View.FreezePanes(2, 1)
            worksheet.OutLineSummaryBelow <- false
            package.Save()

    static member writeLine((line: ReportChartOfAccounts.Line),
                            (ws : ExcelWorksheet),
                            (indent: int),
                            (nextRow: int)) =
      Excel.setValue (ws.Cells.[nextRow, 1+indent], line.account)
      if indent <> 0 then
        ws.Row(nextRow).OutlineLevel <- (indent)
      // Deliberately avoid collapsing hierarchy. If we're looking, we probably want to
      // emphasise details, and it's easy to manually hide them if that's what is wanted.
      Excel.writeLines (line.subAccounts, ws, indent+1, nextRow+1)

    static member writeLines((lines : ReportChartOfAccounts.Line list),
                             (ws : ExcelWorksheet),
                             (indent: int),
                             (nextRow: int)) =
        match lines with
            | [] -> nextRow
            | first::rest -> Excel.writeLines(rest, ws, indent, Excel.writeLine(first, ws, indent, nextRow))

    static member write((report : ReportChartOfAccounts.Report), (destination : Destination)) =
        match destination with
            | None -> ()
            | Some package ->
                let worksheet = package.Workbook.Worksheets.Add("Chart Of Accounts")
                (setHeader worksheet.Cells.[1, 1] "Account")
                Excel.writeLines(report.lines, worksheet, 0, 2) |> ignore
                worksheet.View.FreezePanes(2, 1)
                worksheet.OutLineSummaryBelow <- false
                package.Save()
