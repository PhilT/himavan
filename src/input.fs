module Himavan.Input
open System
type K = ConsoleKey

let SpecialKeys =
  [
    K.Backspace, "<bs>"
    K.Tab, "<tab>"
    K.Enter, "<cr>"
    K.Escape, "<esc>"
    K.Spacebar, "<space>"
    K.UpArrow, "<up>"
    K.DownArrow, "<down>"
    K.LeftArrow, "<left>"
    K.RightArrow, "<right>"
    K.F1, "<f1>"
    K.F1, "<f1>"
    K.F2, "<f2>"
    K.F3, "<f3>"
    K.F4, "<f4>"
    K.F5, "<f5>"
    K.F6, "<f6>"
    K.F7, "<f7>"
    K.F8, "<f8>"
    K.F9, "<f9>"
    K.F10, "<f10>"
    K.F11, "<f11>"
    K.F12, "<f12>"
    K.Insert, "<insert>"
    K.Delete, "<del>"
    K.Home, "<home>"
    K.End, "<end>"
    K.PageUp, "<pageup>"
    K.PageDown, "<pagedown>"
  ]
  |> Map.ofList


let startMatches (key: string) (keys: Keys) =
  keys
  |> Map.tryFindKey (fun k v -> key.StartsWith(key) )

let TIMEOUT = 2000
let wait (keys: Keys) responsiveness =
  let rec loop prevKeys timeout =
    match Con.nextChar () with
    | Some(ch) ->
      let key =
        if SpecialKeys.ContainsKey(ch.Key) then
          SpecialKeys[ch.Key]
        else
          ch.KeyChar.ToString()
      let key = prevKeys + key

      Logger.write "Input" "wait" $"{key}"

      if keys.ContainsKey(key) then
        Some(keys[key])
      elif (startMatches key keys).IsSome then
        loop key TIMEOUT
      elif timeout > 0 then
        loop key (timeout - responsiveness)
      else
        None
    | None ->
      if timeout > 0 then
        Threading.Thread.Sleep(responsiveness)
        loop prevKeys (timeout - responsiveness)
      else
        None

  loop "" TIMEOUT
