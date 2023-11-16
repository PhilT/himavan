[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Email
open Himavan

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

open System.Globalization
let utf8Length str = StringInfo(str).LengthInTextElements
let utf8Substr str len = StringInfo(str).SubstringByTextElements(0, len)

let column (text: string) column (width: int) separator =
  let text = if (utf8Length text) > width then (utf8Substr text width) else text
  let padding = String.replicate (width - (utf8Length text)) " "
  Con.write $"{text}{padding}" EmailColors[int column] Con.defaultBg
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


let maxWidthOf list field =
  list
  |> List.map (fun (_, email) -> String.length (fieldOf email field))
  |> List.max


let render y selected (emails: Map<string, Email>) =
  Con.moveTo 0 y

  let emailList =
    emails
    |> Map.toList
    |> List.sortByDescending(fun (id, email) -> email.date)
    |> List.take (Con.height () - y)

  let separatorCount = FIELD_COUNT - 1
  let idWidth = (maxWidthOf emailList "id")
  let flagsWidth = (maxWidthOf emailList "flags")
  let fromWidth = (maxWidthOf emailList "from")
  let dateWidth = (maxWidthOf emailList "date")

  let subjectFromWidth =
    Con.width () - separatorCount - idWidth - flagsWidth - dateWidth
  let fromWidth = min fromWidth (subjectFromWidth / 3 * 1)
  let subjectWidth = subjectFromWidth - fromWidth

  emailList
  |> List.iteri (fun i (id, email) ->
    column id EmailIndexOf.ID idWidth true
    column (flagsToString email.flags) EmailIndexOf.FLAGS flagsWidth true
    column email.subject EmailIndexOf.SUBJECT subjectWidth true
    column (from email.from) EmailIndexOf.FROM fromWidth true
    column email.date EmailIndexOf.DATE dateWidth false
  )

