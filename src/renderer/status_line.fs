module Himavan.Renderer.StatusLine

open Himavan


let error message =
  Con.writeAt message 0 STATUS_LINE_START_Y (Con.normalStyle Color.RED Color.DEFAULT)

let notice message =
  Con.writeAt message 0 STATUS_LINE_START_Y (Con.normalStyle Color.GREEN Color.DEFAULT)
