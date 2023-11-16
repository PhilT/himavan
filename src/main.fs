module Himavan.Main

open System
open System.IO

let state = {
  settings = Settings.fetch ()
  folders = Mail.folders ()
  emails = Map.empty
  currentFolder = 0
  currentEmail = 0
}

let emails = Mail.list state.folders[state.currentFolder] 50
let newState = {
  state with
    emails = Map.add state.folders[0] emails Map.empty
}



Renderer.setup ()
Renderer.update newState

let rec loop state =
  let ch = Con.getChar ()

  let newState = State.update ch state

  Renderer.update newState

  if ch <> newState.settings["quit"] then
    loop newState


loop newState
Renderer.teardown ()
