module Himavan.Main

open System
open System.IO

Logger.write "Himvan" "Main" "Logging started"

let state = {
  settings = Settings.fetch ()
  folders = Mail.folders ()
  emails = Map.empty
  currentFolder = 0
  currentEmail = 0
}

let currentFolder = State.currentFolder state
//let emails = Mail.list currentFolder (Con.height() - Renderer.FIRST_EMAIL_LINE - 1)
//let newState = {
//  state with
//    emails = Map.add currentFolder emails Map.empty
//}

Renderer.setup ()
Renderer.update state Map.empty

let rec loop state =
  Con.moveTo 0 0
  let ch = Con.getChar ()

  let newState = State.update ch state

  Renderer.update newState (State.currentEmails state)

  if ch <> newState.settings.keys["quit"] then
    loop newState


loop state
Renderer.teardown ()
