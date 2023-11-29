[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Email
open Himavan
open System.Text
open System.Globalization

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


let column (text: string) (width: int) separator style =
  let text = if (String.length text) > width then (text[0..width - 1]) else text
  let padding = String.replicate (width - (String.length text)) " "
  if separator then Con.write SEPARATOR { style with fg = Color.BLACK }
  Con.write $"{text}{padding}" style


let subjectColumn subject widths (columnWidth: int) =
  Measure.unicodeColumn subject widths columnWidth


let writeSubject separator style text =
  if separator then Con.write SEPARATOR { style with fg = Color.BLACK }
  Con.write text style


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
  |> List.map (fun email -> String.length (fieldOf email field))
  |> (fun lst -> String.length field :: lst)
  |> List.max


let headerColumn text width separator =
  let text = if (String.length text) > width then (text[0..width - 1]) else text
  let padding = String.replicate (width - (String.length text)) " "
  if separator then Con.write SEPARATOR (Con.normalStyle Color.BLACK Color.DEFAULT)
  Con.write $"{text}{padding}" (Con.underline Color.WHITE Color.DEFAULT)


let renderAddress current (emails: Email list) x y screenWidth =
  let style = (Con.highlight Color.WHITE Color.BLACK false true)
  let text = (emails[current]).from.addr
  let width = screenWidth - x - 1
  let padding = String.replicate (width - text.Length) " "
  let text = $" {text}{padding}"
  Con.writeAt text x (y + 1) style


let render current (selected: string Set) (emails: Email list) show_addr =
  Con.moveTo 0 HEADER_START_Y

  let fieldCount = 5
  let SEPARATOR = 1
  let separatorCount = fieldCount - 1
  let idWidth = (maxWidthOf emails "id")
  let flagsWidth = (maxWidthOf emails "flags")
  let fromWidth = (maxWidthOf emails "from")
  let dateWidth = (maxWidthOf emails "date")
  let screenWidth = Con.width ()

  let subjectFromWidth =
    screenWidth - separatorCount - idWidth - flagsWidth - dateWidth
  let fromWidth = min fromWidth (subjectFromWidth / 3 * 1)
  let subjectWidth = subjectFromWidth - fromWidth
  let fromX = idWidth + SEPARATOR + flagsWidth + SEPARATOR + subjectWidth
  let fromY = current + FIRST_EMAIL_START_Y

  headerColumn "ID" idWidth false
  headerColumn "FLAGS" flagsWidth true
  headerColumn "SUBJECT" subjectWidth true
  headerColumn "FROM" fromWidth true
  headerColumn "DATE" dateWidth true

  Con.write "" (Con.normalStyle Color.DEFAULT Color.DEFAULT)

  emails
  |> List.iteri (fun i email ->
    let isCurrent = i = current
    let isSelected = Set.contains email.id selected
    let bg =
      if isCurrent || isSelected then Color.BLACK
      else Color.DEFAULT

    column email.id idWidth false (Con.highlight Color.RED bg isSelected isCurrent)
    column (flagsToString email.flags) flagsWidth true (Con.highlight Color.WHITE bg isSelected isCurrent)
    subjectColumn email.subject email.subjectCharWidths subjectWidth
    |> writeSubject true (Con.highlight Color.GREEN bg isSelected isCurrent)
    column (from email.from) fromWidth true (Con.highlight Color.BLUE bg isSelected isCurrent)
    column email.date dateWidth true (Con.highlight Color.YELLOW bg isSelected isCurrent)
  )

  Con.clearToBottom (Con.currY())

  if emails.Length > 0 && show_addr then
    renderAddress current emails fromX fromY screenWidth


