[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Folders
open System

open Himavan


let FolderColors = {
  selected = Color.BLACK, Color.BLUE
  normal = Color.BLUE, Color.DEFAULT
}

let render selected (tabs: string list) =
  Con.moveTo 0 FOLDERS_START_Y

  tabs
  |> List.iteri (fun i tab ->
    let fgColor, bgColor =
      if i = selected then
        FolderColors.selected
      else
        FolderColors.normal

    Con.write $" {tab} " (Con.normalStyle fgColor bgColor)
  )


