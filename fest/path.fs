module Path

open System
open System.IO

let separator = Path.DirectorySeparatorChar.ToString()
let project =
  let cwd = Directory.GetCurrentDirectory()
  let root = Directory.GetDirectoryRoot(cwd)
  // Look for .git/ directory
  let rec findRootFolder (parent: DirectoryInfo) =
    parent.GetDirectories()
    |> Array.tryFind (fun info -> info.Name = ".git")
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


let relative (absolutePath: string) =
  absolutePath.Replace(project + separator, "")


let baseName (path: string) =
  Path.GetFileNameWithoutExtension(path)
