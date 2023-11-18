[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Settings
open System.IO

// FIXME: Very cheap solution to loading a "TOML" file
// Replace with something better
let fetch () =
  let mutable currentSection = "[global]"

  File.ReadAllLines(Path.join "settings.toml")
  |> List.ofArray
  |> List.fold (fun (settings: Settings) (line: string) ->
    if line = "" then
      settings
    elif line.StartsWith("[") && line.EndsWith("]") then
      currentSection <- line
      settings
    else
      let keyValue =
        line.Split("=")
        |> Array.map(fun (x: string) -> x.Trim())

      match currentSection with
      | "[keys]" ->

        { settings with
            keys = Map.add keyValue[0] (keyValue[1].ToCharArray()[0]) settings.keys
        }
      | "[general]" ->
        { settings with
            general = Map.add keyValue[0] keyValue[1] settings.general
        }
      | "[debug]" ->
        { settings with
            debug = Map.add keyValue[0] keyValue[1] settings.debug
        }
      | _ -> settings
  ) { keys = Map.empty; general = Map.empty; debug = Map.empty }
