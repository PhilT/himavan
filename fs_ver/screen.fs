module Screen

open Mindmagma.Curses
open System.Text

type nc = NCurses

// From curses.h
//let CURSOR_INVISIBLE = 0

//let NCURSES_ATTR_SHIFT = 8
//let A_NORMAL      = 0u
//let A_STANDOUT    = 1u <<< (8 + NCURSES_ATTR_SHIFT)
//let A_UNDERLINE   = 1u <<< (9 + NCURSES_ATTR_SHIFT)
//let A_REVERSE     = 1u <<< (10 + NCURSES_ATTR_SHIFT)
//let A_BLINK       = 1u <<< (11 + NCURSES_ATTR_SHIFT)
//let A_DIM         = 1u <<< (12 + NCURSES_ATTR_SHIFT)
//let A_BOLD        = 1u <<< (13 + NCURSES_ATTR_SHIFT)
//let A_ALTCHARSET  = 1u <<< (14 + NCURSES_ATTR_SHIFT)
//let A_INVIS       = 1u <<< (15 + NCURSES_ATTR_SHIFT)
//let A_PROTECT     = 1u <<< (16 + NCURSES_ATTR_SHIFT)

let COLOR_CURRENT = -1s
//let COLOR_BLACK   = 0s
//let COLOR_RED     = 1s
//let COLOR_GREEN   = 2s
//let COLOR_YELLOW  = 3s
//let COLOR_BLUE    = 4s
//let COLOR_MAGENTA = 5s
//let COLOR_CYAN    = 6s
//let COLOR_WHITE   = 7s

// End curses.h

let RED = 1
let GREEN = 2
let BLUE = 3
let YELLOW = 4
let WHITE = 5
let BLACK = 6

// TODO: Rename to LIST_COLORS or something
let COLORS = [RED; WHITE; GREEN; BLUE; YELLOW]

let RED_ON_BLACK = 11
let GREEN_ON_BLACK = 12
let BLUE_ON_BLACK = 13
let YELLOW_ON_BLACK = 14
let WHITE_ON_BLACK = 15
let BLACK_ON_BLACK = 16

let FIELD_COUNT = 5
let SEPARATOR = "│"
let STATUS_LINE = 1
let HEADER_ROW = 2
let FIRST_EMAIL_ROW = 3

let FLAGGED_UNSEEN = "✷"
let FLAGGED_IMPORTANT = "⚑"


let setup() =
  let screen = nc.InitScreen()
  nc.NoEcho()
  nc.Clear()
  nc.StartColor()
  nc.UseDefaultColors()
  nc.SetCursor(CursesCursorState.INVISIBLE) |> ignore
  nc.InitPair(int16 RED, CursesColor.RED, COLOR_CURRENT) |> ignore
  nc.InitPair(int16 GREEN, CursesColor.GREEN, COLOR_CURRENT) |> ignore
  nc.InitPair(int16 BLUE, CursesColor.BLUE, COLOR_CURRENT) |> ignore
  nc.InitPair(int16 YELLOW, CursesColor.YELLOW, COLOR_CURRENT) |> ignore
  nc.InitPair(int16 WHITE, CursesColor.WHITE, COLOR_CURRENT) |> ignore
  nc.InitPair(int16 BLACK, CursesColor.BLACK, COLOR_CURRENT) |> ignore

  nc.InitPair(int16 RED_ON_BLACK, CursesColor.RED, CursesColor.BLACK) |> ignore
  nc.InitPair(int16 GREEN_ON_BLACK, CursesColor.GREEN, CursesColor.BLACK) |> ignore
  nc.InitPair(int16 BLUE_ON_BLACK, CursesColor.BLUE, CursesColor.BLACK) |> ignore
  nc.InitPair(int16 YELLOW_ON_BLACK, CursesColor.YELLOW, CursesColor.BLACK) |> ignore
  nc.InitPair(int16 WHITE_ON_BLACK, CursesColor.WHITE, CursesColor.BLACK) |> ignore
  nc.InitPair(int16 BLACK_ON_BLACK, CursesColor.BLACK, CursesColor.BLACK) |> ignore
  screen


let teardown() =
  nc.EndWin()


let pos screen =
  let y, x = nc.GetYX(screen)
  x, y


let curr_x screen =
  let x, _y = pos screen
  x


let curr_y screen =
  let _x, y = pos screen
  y


let color color highlight =
  let c = if highlight then color + 10 else color
  nc.AttributeOn(nc.ColorPair(c)) |> ignore


let putfield (field: string) c (highlight: bool) =
  color c highlight
  nc.AddString(field) |> ignore


let putline screen (fields: string list) (highlight: bool) (columns: int list) =
  if fields[1].Contains(FLAGGED_UNSEEN) then
    nc.AttributeOn(CursesAttribute.BOLD)
  else
    nc.AttributeOff(CursesAttribute.BOLD)

  //putfield "" BLACK highlight
  //wipeline screen

  nc.GetChar() |> ignore
  for i in 0..FIELD_COUNT - 1 do
    nc.Move(curr_y screen, columns[i]) |> ignore
    if i > 0 then putfield SEPARATOR BLACK highlight

    putfield fields[i] COLORS[i] highlight

    nc.AttributeOff(CursesAttribute.BOLD) |> ignore


let notice (text: string) =
  nc.Move(STATUS_LINE, 0) |> ignore
  putfield text GREEN false


let error (text: string) =
  nc.Move(STATUS_LINE, 0) |> ignore
  putfield text RED false


let renderFolders screen folders curr_folder =
  nc.Move(0, 0) |> ignore
  folders
  |> List.iteri (fun i folder ->
    let (color, highlight) =
      if i = curr_folder then
        nc.AttributeOn(CursesAttribute.BOLD)
        (WHITE, true)
      else
        nc.AttributeOff(CursesAttribute.BOLD)
        (BLUE, true)

    let folder = $" {folder} "
    putfield folder color highlight
    nc.Move(0, ((curr_x screen) + 1)) |> ignore
  )
  nc.AttributeOff(CursesAttribute.BOLD)
