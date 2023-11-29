module Himavan.Renderer.StatusLine

open Himavan

let clearStatusLine () =
  Con.clearLine STATUS_LINE_START_Y


let info message =
  clearStatusLine ()
  Con.writeAt message 0 STATUS_LINE_START_Y (Con.normalStyle Color.YELLOW Color.DEFAULT)

let notice message =
  clearStatusLine ()
  Con.writeAt message 0 STATUS_LINE_START_Y (Con.normalStyle Color.GREEN Color.DEFAULT)

let error message =
  clearStatusLine ()
  Con.writeAt message 0 STATUS_LINE_START_Y (Con.normalStyle Color.RED Color.DEFAULT)
