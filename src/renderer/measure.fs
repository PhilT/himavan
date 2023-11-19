module Himavan.Renderer.Measure

open System.Globalization
open System
open Wcwidth

let calculateCharWidths text =
  let en = StringInfo.GetTextElementEnumerator(text)

  let rec loop totalWidth (lst: int list) =
    let another = en.MoveNext()
    if another then
      let ch = en.GetTextElement()

      let width =
        if ch.Length = 2 && not (Char.IsSurrogatePair(ch, 0)) then
          2
        else
          [0..ch.Length - 1]
          |> Seq.sumBy(fun i ->
            if Char.IsLowSurrogate(ch, i) then
              0
            else
              UnicodeCalculator.GetWidth(Char.ConvertToUtf32(ch, i))
          )
      loop (totalWidth + width) (totalWidth + width :: lst)
    else
      lst |> List.rev

  loop 0 List.empty


let unicodeColumn text widths columnWidth =
  let i = widths |> List.findIndexBack(fun w -> w <= columnWidth)
  let length = widths[i]
  let padding = String.replicate (columnWidth - length) " "
  let text = StringInfo(text).SubstringByTextElements(0, i + 1)
  $"{text}{padding}"
