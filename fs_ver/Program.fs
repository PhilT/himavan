open Spectre.Console
open Spectre.Console.Rendering
open System.Text
open System
open System.Collections.Generic

//let defaultBackground = Console.BackgroundColor
//Console.ForegroundColor <- ConsoleColor.Red
//Console.Clear()
//Console.SetCursorPosition(20, 20)
//Console.WriteLine("Hello Console!")
//
//Console.SetCursorPosition(20, 15)
//Console.BackgroundColor <- ConsoleColor.Black
//Console.WriteLine("Testing!")
//
//Console.BackgroundColor <- defaultBackground
//Console.ForegroundColor <- ConsoleColor.DarkRed
//Console.SetCursorPosition(30, 15)
//Console.WriteLine("And ⚑ again")
//
//Console.ForegroundColor <- ConsoleColor.
//Console.SetCursorPosition(30, 30)
//printfn "%A, %A" Console.WindowWidth Console.WindowHeight

type CompactTableBorder() =
  inherit TableBorder()
  override this.GetPart(part: TableBorderPart) =
    match part with
    | TableBorderPart.HeaderTopLeft -> String.Empty
    | TableBorderPart.HeaderTop -> String.Empty
    | TableBorderPart.HeaderTopSeparator -> String.Empty
    | TableBorderPart.HeaderTopRight -> String.Empty
    | TableBorderPart.HeaderLeft -> String.Empty
    | TableBorderPart.HeaderSeparator -> "│"
    | TableBorderPart.HeaderRight -> String.Empty
    | TableBorderPart.HeaderBottomLeft -> String.Empty
    | TableBorderPart.HeaderBottom -> "─"
    | TableBorderPart.HeaderBottomSeparator -> "┼"
    | TableBorderPart.HeaderBottomRight -> String.Empty
    | TableBorderPart.CellLeft -> String.Empty
    | TableBorderPart.CellSeparator -> "│"
    | TableBorderPart.CellRight -> String.Empty
    | TableBorderPart.FooterTopLeft -> String.Empty
    | TableBorderPart.FooterTop -> "─"
    | TableBorderPart.FooterTopSeparator -> "┼"
    | TableBorderPart.FooterTopRight -> String.Empty
    | TableBorderPart.FooterBottomLeft -> String.Empty
    | TableBorderPart.FooterBottom -> String.Empty
    | TableBorderPart.FooterBottomSeparator -> String.Empty
    | TableBorderPart.FooterBottomRight -> String.Empty
    | _ -> raise (InvalidOperationException("Unknown border part."))

type Column = {
  name: string
  color: Color
}

//AnsiConsole.Markup("[underline red]Hello[/] [bold blue]World![/][blue]World![/]")

let currentBackground = AnsiConsole.Background
let columns = [
  { name = "ID"; color = Color.Red }
  { name = "FLAGS"; color = Color.White }
  { name = "SUBJECT"; color = Color.Green }
  { name = "FROM"; color = Color.Blue }
  { name = "DATE"; color = Color.Yellow }
]

let firstRow = ["6822"; "✷"; "Senior Ruby Contract - Fully Remote"; "Josh Caulfield"; "2023-11-13 10:19"]

let table = Table()
table.Expand <- true
table.HideHeaders() |> ignore
table.Border <- TableBorder.Minimal
table.BorderColor(Color.Black) |> ignore

List.iter (fun column ->
  table.AddColumn(column.name) |> ignore
) columns

let items: IEnumerable<Rendering.IRenderable> =
  List.map2 (fun column field ->
    let style = Style(column.color)
    Text(field, style)
  ) columns firstRow

table.AddRow(items).Padding(0,0) |> ignore

let foldersLayout = Layout("Folders")
let status = Layout("Status")
let main = Layout("Main")
let layout = Layout("Root").SplitRows(
  foldersLayout,
  status,
  main
)
let folders: IEnumerable<Rendering.IRenderable> = [
  Text(" INBOX ", Style(Color.Blue, Color.Black))
  Text(" Archive ", Color.Blue)
  Text(" Invest ", Color.Blue)
  Text(" Spam ", Color.Blue)
  Text(" Trash ", Color.Blue)
]

let foldersRow = Columns(folders)
let statusRow = Text("This is a text", Color.Green)

foldersLayout.Size <- 1
status.Size <- 1

foldersLayout.Update(foldersRow) |> ignore
status.Update(statusRow) |> ignore
main.Update(table) |> ignore

AnsiConsole.Write(layout)
Console.ReadKey() |> ignore
