[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Email
open Himavan
open System.Text

let FIELD_COUNT = 5
let SEPARATOR = "│"
let SEPARATOR_COLOR = Color.Black

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


let column (text: string) column (width: int) separator =
  let text = if (String.length text) > width then (text[0..width - 1]) else text
  let padding = String.replicate (width - (String.length text)) " "
  Con.write $"{text}{padding}" EmailColors[int column] Con.defaultBg
  if separator then Con.write SEPARATOR SEPARATOR_COLOR Con.defaultBg


let subjectColumn (email: Email) (columnWidth: int) separator =
  let text =
    if email.subjectTotalWidth < columnWidth then
      let padding = String.replicate (columnWidth - email.subjectTotalWidth) " "
      $"{email.subject}{padding}"
    else
      let rec loop length i =
        let newLength = length + email.subjectCharWidths[i]
        if newLength > columnWidth then
          let padding = String.replicate (columnWidth - length) " "
          $"{email.subject[0..length - 1]}{padding}"
        elif newLength = columnWidth then
          email.subject[0..i]
        else
          loop (newLength) (i + 1)

      loop 0 0
  Con.write text EmailColors[int EmailIndexOf.SUBJECT] Con.defaultBg
  if separator then Con.write SEPARATOR SEPARATOR_COLOR Con.defaultBg


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
  Con.underline $"{text}{padding}" Color.White Con.defaultBg
  if separator then Con.write SEPARATOR SEPARATOR_COLOR Con.defaultBg


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

  headerColumn EmailIndexOf.ID idWidth true
  headerColumn EmailIndexOf.FLAGS flagsWidth true
  headerColumn EmailIndexOf.SUBJECT subjectWidth true
  headerColumn EmailIndexOf.FROM fromWidth true
  headerColumn EmailIndexOf.DATE dateWidth false

  emailList
  |> List.iteri (fun i (id, email) ->
    column id EmailIndexOf.ID idWidth true
    column (flagsToString email.flags) EmailIndexOf.FLAGS flagsWidth true
    subjectColumn email subjectWidth true
    column (from email.from) EmailIndexOf.FROM fromWidth true
    column email.date EmailIndexOf.DATE dateWidth false
  )

