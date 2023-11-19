[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Email
open Himavan
open System.Text
open System.Globalization

let FIELD_COUNT = 5
let SEPARATOR = "│"
let SEPARATOR_COLOR = Color.Black
let IGNORE = -1

type EmailIndexOf =
  | ID = 0
  | FLAGS = 1
  | SUBJECT = 2
  | FROM = 3
  | DATE = 4


let EmailColors = [Color.Red; Color.White; Color.Green; Color.Blue; Color.Yellow]
let EmailColumns = [
  { name = "ID"; color = EmailColors[int EmailIndexOf.ID] }
  { name = "FLAGS"; color = EmailColors[int EmailIndexOf.FLAGS] }
  { name = "SUBJECT"; color = EmailColors[int EmailIndexOf.SUBJECT] }
  { name = "FROM"; color = EmailColors[int EmailIndexOf.FROM] }
  { name = "DATE"; color = EmailColors[int EmailIndexOf.DATE] }
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


let flagsToString (flags: string list) =
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


let column (text: string) column (width: int) x separator bg =
  let text = if (String.length text) > width then (text[0..width - 1]) else text
  let padding = String.replicate (width - (String.length text)) " "
  //Logger.write "Renderer.Email" "column" $"x,y: {x},{Con.currY()}"
  //if x > IGNORE then System.Console.CursorLeft <- x
  if separator then Con.write SEPARATOR SEPARATOR_COLOR bg
  Con.write $"{text}{padding}" EmailColors[int column] bg


let subjectColumn subject widths (columnWidth: int) =
  Measure.unicodeColumn subject widths columnWidth


let writeSubject separator bg text =
  if separator then Con.write SEPARATOR SEPARATOR_COLOR bg
  Con.write text EmailColors[int EmailIndexOf.SUBJECT] bg


let from (address: Address) =
  if address.name <> "" then address.name else address.addr


let fieldOf email field =
  match field with
  | "id" -> email.id
  | "flags" -> flagsToString email.flags
  | "subject" -> email.subject
  | "from" -> from email.from
  | "date" -> email.date
  | _ -> failwith $"Invalid field `{field}` specified when attempting to access Email record"


let maxWidthOf lst field =
  lst
  |> List.map (fun (_, email) -> String.length (fieldOf email field))
  |> (fun lst -> String.length field :: lst)
  |> List.max


let headerColumn field width separator =
  let text = EmailColumns[int field].name
  let text = if (String.length text) > width then (text[0..width - 1]) else text
  let padding = String.replicate (width - (String.length text)) " "
  if separator then Con.write SEPARATOR SEPARATOR_COLOR Con.defaultBg
  Con.underline $"{text}{padding}" Color.White Con.defaultBg


let render y selected (emails: Map<string, Email>) =
  Con.moveTo 0 y

  let emailList =
    let emailCount = min (Con.height () - y) emails.Count
    emails
    |> Map.toList
    |> List.sortByDescending(fun (id, email) -> email.date)
    |> List.take emailCount

  let separatorCount = FIELD_COUNT - 1
  let idWidth = (maxWidthOf emailList "id")
  let flagsWidth = (maxWidthOf emailList "flags")
  let fromWidth = (maxWidthOf emailList "from")
  let dateWidth = (maxWidthOf emailList "date")

  let subjectFromWidth =
    Con.width () - separatorCount - idWidth - flagsWidth - dateWidth
  let fromWidth = min fromWidth (subjectFromWidth / 3 * 1)
  let subjectWidth = subjectFromWidth - fromWidth

  let idPosition = 0
  let flagsPosition = idWidth
  let subjectPosition = flagsPosition + 1 + flagsWidth
  let fromPosition = subjectPosition + 1 + subjectWidth
  let datePosition = fromPosition + 1 + fromWidth

  headerColumn EmailIndexOf.ID idWidth false
  headerColumn EmailIndexOf.FLAGS flagsWidth true
  headerColumn EmailIndexOf.SUBJECT subjectWidth true
  headerColumn EmailIndexOf.FROM fromWidth true
  headerColumn EmailIndexOf.DATE dateWidth true

  emailList
  |> List.iteri (fun i (id, email) ->
    //Con.moveTo 0 (y + i)
    let bg = if i = selected then Color.Black else Con.defaultBg

    column id EmailIndexOf.ID idWidth IGNORE false bg
    column (flagsToString email.flags) EmailIndexOf.FLAGS flagsWidth IGNORE true bg
    subjectColumn email.subject email.subjectCharWidths subjectWidth |> writeSubject true bg
    column (from email.from) EmailIndexOf.FROM fromWidth fromPosition true bg
    column email.date EmailIndexOf.DATE dateWidth IGNORE true bg
  )

  Con.clearToBottom (Con.currY())

