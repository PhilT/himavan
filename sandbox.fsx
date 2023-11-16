open System.Globalization
open System
open System.Text

let s = "✨ Placement"
let s2 = "A↵ lacement"
let info = StringInfo(s)
let info2 = StringInfo(s2)

Text.UTF8Encoding().GetByteCount(s) |> printfn "%A"
Text.UTF8Encoding().GetByteCount(s2) |> printfn "%A"

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
//
//// 11
