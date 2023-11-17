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

let currentFolder = State.currentFolder state
let emails = Mail.list currentFolder (Con.height() - Renderer.FIRST_EMAIL_LINE - 1)
let newState = {
  state with
    emails = Map.add currentFolder emails Map.empty
}

Renderer.setup ()
Renderer.update newState

let rec loop state =
  Con.moveTo 0 0
  let ch = Con.getChar ()
  Logger.write "Main" "module" $"Key pressed {ch}"

  let newState = State.update ch state

  Renderer.update newState

  if ch <> newState.settings.keys["quit"] then
    loop newState


loop newState
Renderer.teardown ()
