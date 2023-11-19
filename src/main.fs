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

Renderer.All.setup ()
Renderer.All.update state []

let rec agent = MailboxProcessor.Start(fun inbox ->
  let rec loop state =
    async {
      Con.moveTo 0 0

      let! msg = inbox.Receive()
      match msg with
      | Update(state) ->
        Renderer.All.update state (State.currentEmails state)
        return! loop state
      | Notice(message) ->
        Renderer.StatusLine.notice message
        return! loop state
      | Error(message) ->
        Renderer.StatusLine.error message
        return! loop state
      | NewEmails(folder, emails) ->
        let newState = { state with emails = Map.add folder emails state.emails }
        Renderer.All.update newState (State.currentEmails newState)
        return! loop newState
      | Fetch(channel) ->
        channel.Reply(state)
        return! loop state
    }

  loop state
)

let responsiveness = Int32.Parse state.settings.general["response_time"]

// Get the first email folder
State.fetchEmails agent state |> ignore

let rec keyLoop () =
  let quit =
    match Con.nextChar () with
    | Some(ch) ->
      let state = agent.PostAndReply((fun channel -> Fetch channel))
      let newState = State.update ch state agent
      agent.Post(Update(newState))
      ch = state.settings.keys["quit"]
    | None ->
      Threading.Thread.Sleep(responsiveness)
      false

  if not quit then
    keyLoop ()

keyLoop ()

Renderer.All.teardown ()
