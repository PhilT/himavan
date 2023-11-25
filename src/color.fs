module Himavan.Color
open System

let ESC = "\x1b["
let NORMAL = "0"
let BOLD = "1"
let DIM = "2"
let UNDERLINE = "4"

let BLACK   = 30
let RED     = 31
let GREEN   = 32
let YELLOW  = 33
let BLUE    = 34
let MAGENTA = 35
let CYAN    = 36
let WHITE   = 37
let DEFAULT = 39
let RESET   = 0
let BACKGRND = 10

let combineAttrs (lst: string list) =
  let attrs = String.Join(";", lst)
  $"{ESC}{attrs}m"


let paint (fg: int) (bg: int) (style: string) =
  combineAttrs [$"{style}"; $"{fg}"; $"{bg + BACKGRND}"]



