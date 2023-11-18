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
      let width = UnicodeCalculator.GetWidth(Char.ConvertToUtf32(ch, 0))
      loop (totalWidth + width) (totalWidth + width :: lst)
    else
      lst |> List.rev

  loop 0 List.empty


let unicodeColumn text widths columnWidth =
  let i = widths |> List.findIndexBack(fun w -> w <= columnWidth)
  let length = widths[i]
  //printfn "i: %A" i
  //printfn "length: %A" length
  //printfn "additional space: %A" (columnWidth - length)
  let padding = String.replicate (columnWidth - length) " "
  let subject = StringInfo(text).SubstringByTextElements(0, i + 1)
  $"{subject}{padding}"
