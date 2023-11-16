module Himavan.Con

open System

let defaultBg = Console.BackgroundColor
let defaultFg = Console.ForegroundColor

let write (text: string) fg bg =
  Console.ForegroundColor <- fg
  Console.BackgroundColor <- bg
  Console.Write(text)


let moveTo x y =
  Console.SetCursorPosition(x, y)


let writeAt (text: string) x y fg bg =
  moveTo x y
  write text fg bg


let nextChar () =
  if Console.KeyAvailable then
    let key = Console.ReadKey()
    Some(key.KeyChar)
  else
    None


let getChar () =
  let key = Console.ReadKey()
  key.KeyChar

let clear () = Console.Clear()
let reset () = Console.ResetColor()

let currX () = Console.CursorLeft
let currY () = Console.CursorTop

let width () = Console.WindowWidth
let height () = Console.WindowHeight
let hideCursor () = Console.CursorVisible <- false
let showCursor () = Console.CursorVisible <- true


let clearTo currY toY =
  writeAt (String.replicate (width() * (toY - currY)) " ") 0 currY defaultFg defaultBg


let clearLine y =
  clearTo y (y + 1)


let clearToBottom fromY =
  clearTo fromY (height () - fromY)


let setup () =
  clear ()
  hideCursor ()


let teardown () =
  reset ()
  showCursor ()
  clear ()

