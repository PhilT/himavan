module Himavan.Con

open System

let normalStyle fg bg =
  { fg = fg; bg = bg; props = Color.NORMAL }


let underline fg bg =
  { fg = fg; bg = bg; props = Color.UNDERLINE }


let defaultStyle =
  { fg = Color.DEFAULT; bg = Color.DEFAULT; props = Color.NORMAL }


let highlight fg bg highlight current =
  {
    fg =
      if highlight && current then Color.YELLOW
      elif highlight then Color.WHITE
      else fg
    bg = bg
    props = if highlight then Color.BOLD else Color.NORMAL
  }


let write (text: string) style =
  let prefix = Color.paint style.fg style.bg style.props
  let suffix = Color.paint style.fg style.bg $"{Color.DEFAULT}"
  Console.Write($"{prefix}{text}{suffix}")


let moveTo x y =
  Console.SetCursorPosition(x, y)


let writeAt (text: string) x y style =
  moveTo x y
  write text style


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
  writeAt (String.replicate cells " ") 0 fromY defaultStyle


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

