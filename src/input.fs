module Himavan.Input
open System
type K = ConsoleKey

let SpecialKeys =
  [
    K.Backspace, "<bs>"
    K.Tab, "<tab>"
    K.Enter, "<cr>"
    K.Spacebar, "<space>"
    K.UpArrow, "<up>"
    K.DownArrow, "<down>"
  ]
  |> Map.ofList

let wait (keys: Keys) =
  match Con.nextChar () with
  | Some(ch) ->
    let key =
      if SpecialKeys.ContainsKey(ch.Key) then
        SpecialKeys[ch.Key]
      else
        ch.KeyChar.ToString()

    Logger.write "Input" "wait" $"{key}"

    if keys.ContainsKey(key) then
      Some(keys[key])
    else
      None
  | None ->
    None
