module Himavan.Con

open System

let ANSI_NORMAL = "\x1b[0m"
let ANSI_UNDERLINE = "\x1b[4m"

let defaultBg = Console.BackgroundColor
let defaultFg = Console.ForegroundColor

let write (text: string) fg bg =
  Console.ForegroundColor <- fg
  Console.BackgroundColor <- bg
  Console.Write(text)


let underline (text: string) fg bg =
  write $"{ANSI_UNDERLINE}{text}{ANSI_NORMAL}" fg bg

let moveTo x y =
  Console.SetCursorPosition(x, y)


let writeAt (text: string) x y fg bg =
  moveTo x y
  write text fg bg


let nextChar () =
  if Console.KeyAvailable then
    let key = Console.ReadKey(true)
    Some(key)
  else
    None


let getChar () =
  let key = Console.ReadKey(true)
  key.KeyChar

let clear () = Console.Clear()
let reset () = Console.ResetColor()

let currX () = Console.CursorLeft
let currY () = Console.CursorTop

let width () = Console.WindowWidth
let height () = Console.WindowHeight
let hideCursor () = Console.CursorVisible <- false
let showCursor () = Console.CursorVisible <- true


let clearTo fromY toY =
  let cells = width() * (toY - fromY)
  Logger.write "Con" "clearTo" $"{fromY} {toY} {cells}"
  writeAt (String.replicate cells " ") 0 fromY defaultFg defaultBg


let clearLine y =
  clearTo y (y + 1)


let clearToBottom fromY =
  if fromY < height() then
    clearTo fromY (height ())


let setup () =
  Console.OutputEncoding <- System.Text.Encoding.UTF8

  clear ()
  hideCursor ()


let teardown () =
  reset ()
  clear ()
  showCursor ()

