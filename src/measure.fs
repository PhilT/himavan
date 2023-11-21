module Himavan.Measure

open System.Globalization
open System
open Wcwidth
open System.Text

open Himavan

let REPLACEMENT_CHAR = "�"

let replaceCombiningChars text =
  let en = StringInfo.GetTextElementEnumerator(text)
  let rec loop newText =
    let another = en.MoveNext()
    if another then
      let ch = en.GetTextElement()
      (if ch.Length > 2 then REPLACEMENT_CHAR else ch) :: newText
      |> loop
    else
      newText

  loop []
  |> List.rev
  |> (fun textArray -> String.Join("", textArray))


let calculateCharWidths text =
  let en = StringInfo.GetTextElementEnumerator(text)

  let rec loop totalWidth (lst: int list) =
    let another = en.MoveNext()
    if another then
      let ch = en.GetTextElement()

      let width =
        if ch.Length > 1 && not (Char.IsSurrogate(ch, 0)) then
          ch.Length
        else
          UnicodeCalculator.GetWidth(Char.ConvertToUtf32(ch, 0))
      loop (totalWidth + width) (totalWidth + width :: lst)
    else
      lst |> List.rev

  loop 0 []


let unicodeColumn text widths columnWidth =
  if text = "" then
    String.replicate columnWidth " "
  else
    let i = widths |> List.findIndexBack(fun w -> w <= columnWidth)
    let length = widths[i]
    let padding = String.replicate (columnWidth - length) " "
    let text = StringInfo(text).SubstringByTextElements(0, i + 1)
    $"{text}{padding}"
