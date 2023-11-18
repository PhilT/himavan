module Himavan.Tests.Renderer
open Fest
open Himavan.Renderer

let tests = [
  test "subjectColumn fits width when less than columnWidth" <| fun () ->
    let subject = Measure.unicodeColumn "Testing" [1; 2; 3; 4; 5; 6; 7] 10
    assertEqual "Testing   " subject

  test "subjectColumn fits width when greater than columnWidth" <| fun () ->
    let subject = Measure.unicodeColumn "Longer Test" [1; 2; 3; 4; 5; 6; 7; 8; 9; 10; 11] 10
    assertEqual "Longer Tes" subject

  test "subjectColumn fits with unicode characters" <| fun () ->
    let widths = Measure.calculateCharWidths "✨ test✨"
    let subject = Measure.unicodeColumn "✨ test✨" (widths) 10
    assertEqual "✨ test✨ " subject
]
