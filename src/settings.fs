module Himavan.Settings
open System.IO

// FIXME: Very cheap solution to loading a "TOML" file
// Replace with something better
let fetch () =
  File.ReadAllLines(Path.join "settings.toml")
  |> List.ofArray
  |> List.except ["[keys]"]
  |> List.fold (fun (state: Keys) (line: string) ->
    let keyValue =
      line.Split("=")
      |> Array.map(fun (x: string) -> x.Trim())
    Map.add keyValue[0] (keyValue[1].ToCharArray()[0]) state
  ) Map.empty
