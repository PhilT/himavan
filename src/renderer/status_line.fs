module Himavan.Renderer.StatusLine

open Himavan


let error message =
  Con.writeAt message 0 STATUS_LINE_START_Y Color.Red Con.defaultBg

let notice message =
  Con.writeAt message 0 STATUS_LINE_START_Y Color.Green Con.defaultBg
