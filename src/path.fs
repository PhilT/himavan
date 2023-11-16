module Himavan.Path
open System
open System.IO

let project =
  let cwd = Directory.GetCurrentDirectory()
  let root = Directory.GetDirectoryRoot(cwd)
  // Look for settings.toml
  let rec findRootFolder (parent: DirectoryInfo) =
    parent.GetFiles()
    |> Array.tryFind (fun info -> info.Name = "settings.toml")
    |> function
      | Some _ -> parent
      | None ->
        if parent.FullName = parent.Root.FullName then
          parent // returns root if it can't find config
        else
          findRootFolder parent.Parent

  (findRootFolder (DirectoryInfo(cwd))).FullName


let join relativePath =
  Path.Combine([| project; relativePath |])


