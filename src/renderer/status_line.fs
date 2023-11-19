module Himavan.Renderer.StatusLine

open Himavan

let START_Y = 1

let error message =
  Con.writeAt message 0 START_Y Color.Red Con.defaultBg

let notice message =
  Con.writeAt message 0 START_Y Color.Green Con.defaultBg
