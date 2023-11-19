#r "nuget: Wcwidth"
open System.Globalization
open System
open System.Text
open Wcwidth

let convert (str: string) =
  UTF8Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(str))

let info (str: string) =
  let bytes = convert str
  let chars = Encoding.Unicode.GetChars(bytes)
  let cat = CharUnicodeInfo.GetUnicodeCategory(str, 0)
  let info = StringInfo(str)
  let len = info.LengthInTextElements
  let substr = info.SubstringByTextElements(0, len)

  printfn "%A" str
  printfn "Str length: %A" str.Length
  printfn "Text element length: %A" (StringInfo(str).LengthInTextElements)

  str
  |> String.iteri (fun i c ->
    printfn "Char %A: %A" i (Char.IsSurrogate(c))
  )

  printfn "Unicode category: %A" (cat)
  (if Char.IsHighSurrogate(str, 0) && Char.IsLowSurrogate(str, 1) then
    $"Yes: {StringInfo.GetNextTextElementLength(str)} Wcwidth of 2nd char: {UnicodeCalculator.GetWidth(str[1])}"
  else
    "No")
  |> printfn "Surrogate: %A"
  printfn "Wcwidth: %A" (UnicodeCalculator.GetWidth(Char.ConvertToUtf32(str, 0)))
  printfn "UTF32: %A" (Char.ConvertToUtf32(str, 0))
  printfn "Combining chars: %A" (StringInfo.ParseCombiningCharacters(str))


  printfn ""

info "↵"
info "\uD800\uDC00"
info "A"
info "✨"
info "🐂"
info "🚀"
info "✊"
info "⚔️"
info "🧑‍🤝‍🧑"
info "✍️"

//Console.SetCursorPosition(10, 10)
//Console.Write("✨")
//Console.SetCursorPosition(11, 10)
//Console.Write("Hello")
//Console.ReadKey()

//let bytes = Encoding.UTF8.GetBytes(s)
//printfn "%A" (Encoding.UTF8.IsSingleByte)
//printfn "%A" (convert s).Length
//printfn "%A" (convert s2).Length

//let enumRunes (str: string) =
//  str.EnumerateRunes().
//  |> printfn "%A"2728          ; Emoji                # E0.6   [1] (✨)       sparkles
//
//
//enumRunes s
//enumRunes s2

//let info = StringInfo(s)
//let info2 = StringInfo(s2)
//
//Text.UTF8Encoding().GetByteCount(s) |> printfn "%A"
//Text.UTF8Encoding().GetByteCount(s2) |> printfn "%A"

//printfn "s1: %A %A" (info.LengthInTextElements) (UTF8Encoding().GetByteCount(s))
//printfn "s2: %A %A" (info2.LengthInTextElements) (UTF8Encoding().GetByteCount(s2))
//info.SubstringByTextElements(0, 4) |> printfn "%A"
//info2.SubstringByTextElements(0, 4) |> printfn "%A"
//
//s[0..3] |> printfn "%A"
//s2[0..3] |> printfn "%A"
//
//let substringOf s =
//  let e = StringInfo.GetTextElementEnumerator(s)
//  let mutable width = 0
//  while e.MoveNext() && width < 4 do
//    printfn "%A " (e.GetTextElement().Length)
//    width <- width + e.GetTextElement().Length
//  s[0..e.ElementIndex - 1]
//
//printfn "Proper substr"
//substringOf s |> printfn "%A"
//substringOf s2 |> printfn "%A"
