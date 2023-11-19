[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Folders
open System

open Himavan

let START_Y = 0

let FolderColors = {
  selected = Color.Black, Color.Blue
  normal = Color.Blue, Con.defaultBg
}

let render selected (tabs: string list) =
  Con.moveTo 0 START_Y

  tabs
  |> List.iteri (fun i tab ->
    let fgColor, bgColor =
      if i = selected then
        FolderColors.selected
      else
        FolderColors.normal

    Con.write $" {tab} " fgColor bgColor
  )


