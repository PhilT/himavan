module Himavan.Main

open System
open System.IO

Logger.write "Himvan" "Main" "Logging started"

let folders =
  match MailService.folders () with
  | Ok(folders) -> folders
  | Result.Error(msg) ->
    printfn "Error fetching folders from mail account."
    printfn "Error message from mail agent:"
    printfn "%s" msg
    []

let state = {
  settings = Settings.fetch ()
  folders = folders
  emails = Map.empty
  currentFolder = 0
  currentEmail = 0
  selectedEmailIds = Set.empty
  windowHeight = Con.height ()
  nav = Set.ofList [Nav.LIST]
}

let currentFolder = Email.currentFolder state

Renderer.All.setup ()
Renderer.All.update state []

let responsiveness = Int32.Parse state.settings.general["response_time"]

// Get the first email folder
let agent = Agent.create state
Email.fetchList agent state |> ignore

let rec keyLoop () =
  let state = agent.PostAndReply((fun channel -> Fetch channel))
  let quit =
    match Input.wait state.settings.keys responsiveness with
    | Some(action) ->
      agent.Post(Notice(""))
      if not (Set.contains Nav.QUITING state.nav) then
        let newState = State.update action state agent
        agent.Post(Update(newState))
        false
      else
        true
    | None ->
      Threading.Thread.Sleep(responsiveness)
      Set.contains Nav.QUITING state.nav

  if not quit then
    keyLoop ()

keyLoop ()

