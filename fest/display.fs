module Display

open System

Console.Clear()
printfn ""

let BLOCK = "\u2588"
let bar width = List.init width (fun _ -> BLOCK) |> String.concat ""
let log color msg =
  Console.ForegroundColor <- color
  printf "%s" msg
  Console.ResetColor()

let green = log ConsoleColor.Green
let red = log ConsoleColor.Red
let printBorder width = log ConsoleColor.DarkGray (bar width)

let progressBar passed =
  let color = if passed then green else red
  "     " + bar (Console.WindowWidth - 10) + "\n"
  |> color

let text text =
  printfn $"\n{text}"


