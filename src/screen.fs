module Himavan.Screen

open System
open Spectre.Console
open Spectre.Console.Rendering
open System.Collections.Generic

type Column = {
  name: string
  color: Color
}

type EmailColumnIndices =
  | ID = 0
  | FLAGS = 1
  | SUBJECT = 2
  | FROM = 3
  | DATE = 4

let EmailColumns = [
  { name = "ID"; color = Color.Red }
  { name = "FLAGS"; color = Color.White }
  { name = "SUBJECT"; color = Color.Green }
  { name = "FROM"; color = Color.Blue }
  { name = "DATE"; color = Color.Yellow }
]


let availableFlags =
  [
    "Unseen", "✷"   // 0x2737
    "Answered", "↵" // 0x21B5
    "Flagged", "★"  // 0x2605
    "Deleted", "✘"  // 0x2717
    "Draft", "✍"    // 0x270D
    "Seen", ""
  ]
  |> Map.ofList


let displayFlags (flags: string list) =
  let mutable flagString =
    if not (List.contains "Seen" flags) then
      availableFlags["Unseen"]
    else
      ""

  flags
  |> List.iter (fun desc ->
    flagString <- $"{flagString}{availableFlags[desc]}"
  )

  flagString


let tabs selected (tabs: string list) =
  let layout: IEnumerable<IRenderable> =
    List.mapi (fun i item ->
      let inverted = if i = selected then Decoration.Invert else Decoration.None
      Text($" {item} ", Style(Color.Blue, Color.Default, inverted))
    ) tabs

  let columns = Columns(layout).Collapse()
  columns.Padding <- Padding(0)
  columns


let setupTable () =
  let table = Table()
  table.Expand <- true
  table.Border <- TableBorder.Minimal
  table.BorderColor(Color.Black) |> ignore

  List.iter (fun column ->
    table.AddColumn(column.name, (fun c ->
      c.Padding <- Padding(0)
    )) |> ignore
  ) EmailColumns

  //table.Columns[int EmailColumnIndices.ID].NoWrap <- true
  //table.Columns[int EmailColumnIndices.FROM].NoWrap <- true
  //table.Columns[int EmailColumnIndices.FLAGS].NoWrap <- true
  //table.Columns[int EmailColumnIndices.DATE].NoWrap <- true

  table


let text text (color: Color) cropped =
  let text = Text(text, Style(color))
  if cropped then text.Overflow <- Overflow.Crop
  text


let toColumns (email: Email) : IEnumerable<IRenderable> =
  [
    text email.id EmailColumns[0].color false
    text (displayFlags(email.flags)) EmailColumns[1].color false
    text email.subject EmailColumns[2].color true
    text email.from.name EmailColumns[3].color false
    text email.date EmailColumns[4].color false
  ]




let setup () =
  let layout = Layout("Root").SplitRows(
    Layout("Folders"),
    Layout("Status"),
    Layout("Main")
  )

  let folders: IEnumerable<Rendering.IRenderable> = []
  let foldersRow = Columns(folders)
  let statusRow = Text("This is a text", Color.Green)
  let table = setupTable ()

  foldersRow.Collapse() |> ignore

  layout["Folders"].Size <- 1
  layout["Status"].Size <- 1

  layout["Folders"].Update(foldersRow) |> ignore
  layout["Status"].Update(statusRow) |> ignore
  layout["Main"].Update(table) |> ignore
  layout, table


let update state (layout: Layout, table: Table) =
  let tabs = tabs state.currentFolder state.folders

  state.emails[state.folders[state.currentFolder]]
  |> Map.toList
  |> List.sortByDescending(fun (id, email) -> email.date)
  |> List.iter (fun (id, email) ->
    table.AddRow(toColumns email) |> ignore
  )

  layout["Folders"].Update(tabs) |> ignore


let refresh (layout: Layout, table: Table) =
  AnsiConsole.Write(layout)

