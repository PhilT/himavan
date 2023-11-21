module Himavan.Tests.Renderer
open Fest
open Himavan.Renderer
open Himavan

let tests = [
  test "subjectColumn fits width when less than columnWidth" <| fun () ->
    let text = "Testing"
    let widths = Measure.calculateCharWidths text
    let subject = Measure.unicodeColumn text widths 10
    assertEqual "Testing   " subject

  test "subjectColumn fits width when greater than columnWidth" <| fun () ->
    let text = "Longer Test"
    let widths = Measure.calculateCharWidths text
    let subject = Measure.unicodeColumn text widths 10
    assertEqual "Longer Tes" subject

  test "subjectColumn fits with unicode characters" <| fun () ->
    let text = "✨ test✨"
    let widths = Measure.calculateCharWidths text
    let subject = Measure.unicodeColumn text (widths) 10
    assertEqual $"{text} " subject

  test "subjectColumn fits unicode chars that wcwidth reports as single width" <| fun () ->
    let text = "⚔️"
    let widths = Measure.calculateCharWidths text
    let subject = Measure.unicodeColumn text (widths) 3
    assertEqual $"{text} " subject

  test "subjectColumn fits unicode chars that reports length as 2 but is in fact 1" <| fun () ->
    let text = "𐀀"
    let widths = Measure.calculateCharWidths text
    let subject = Measure.unicodeColumn text (widths) 3
    assertEqual $"{text}  " subject

// FIXME: Later. Kitty doesn't render them correctly yet (adds too much space)
// Could be related to font choice. Needs more investigation.
//
//  test "subjectColumn handles surrogates" <| fun () ->
//    let text = "👨‍👩‍👧‍👦"
//    printfn "%A" text
//    let widths = Measure.calculateCharWidths text
//    printfn "%A" widths
//    let subject = Measure.unicodeColumn text (widths) 10
//    assertEqual $"{text}    " subject
//
//  test "subjectColumn handles another surrogate case" <| fun () ->
//    let text = "🏃‍♀️"
//    printfn "%A" text
//    let widths = Measure.calculateCharWidths text
//    printfn "%A" widths
//    let subject = Measure.unicodeColumn text (widths) 10
//    assertEqual $"{text}      " subject
]
