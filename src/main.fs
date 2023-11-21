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
  windowHeight = Con.height ()
  nav = Nav.LIST
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
        if state.nav = Nav.LIST then
          Renderer.All.update state (State.currentEmails state)
        return! loop state
      | Opening(emailId) ->
        Renderer.StatusLine.notice $"Opening email {emailId}"
        Renderer.Body.prepare ()
        Logger.write "Main" "agent" "Done preparing"
        return! loop state
      | ReadEmail(body) ->
        Logger.write "Main" "agent" "about to render"
        Renderer.Body.render body
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
      | Quit ->
        Renderer.All.teardown ()
        return! loop { state with nav = Nav.QUITING }
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
  let state = agent.PostAndReply((fun channel -> Fetch channel))
  let quit =
    match Input.wait state.settings.keys with
    | Some(action) ->
      if state.nav <> Nav.QUITING then
        let newState = State.update action state agent
        agent.Post(Update(newState))
        false
      else
        true
    | None ->
      Threading.Thread.Sleep(responsiveness)
      state.nav = Nav.QUITING

  if not quit then
    keyLoop ()

keyLoop ()

