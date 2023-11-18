[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer
open System

let FOLDER_LINE = 0
let STATUS_LINE = 1
let HEADER_LINE = 2
let FIRST_EMAIL_LINE = 3


let FolderColors = {
  selected = Color.Black, Color.Blue
  normal = Color.Blue, Con.defaultBg
}

let textWithColors text (fg, bg) = text, fg, bg

let renderFolders selected (tabs: string list) =
  Con.moveTo 0 FOLDER_LINE

  tabs
  |> List.iteri (fun i tab ->
    let fgColor, bgColor =
      if i = selected then
        FolderColors.selected
      else
        FolderColors.normal

    Con.write $" {tab} " fgColor bgColor
  )


let setup () =
  Con.setup ()


let teardown () =
  Con.teardown ()

let update (state: State) emails =
  renderFolders state.currentFolder state.folders

  Renderer.Email.render HEADER_LINE state.currentEmail emails
