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
Renderer.All.update state Map.empty

let rec agent = MailboxProcessor.Start(fun inbox ->
  let rec loop state =
    async {
      Con.moveTo 0 0

      let! msg = inbox.Receive()
      match msg with
      | Update(ch) ->
        Logger.write "Main" "Update" $"{ch}"
        let newState = State.update ch state agent
        Renderer.All.update newState (State.currentEmails newState)
        return! loop newState
      | NewEmails(folder, emails) ->
        Logger.write "Main" "NewEmails" $"{emails.Count} emails in {folder}"
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
State.fetchEmails (State.currentFolder state) agent |> ignore

let rec keyLoop () =
  let quit =
    match Con.nextChar () with
    | Some(ch) ->
      agent.Post(Update(ch))
      //let newState = agent.PostAndReply((fun channel -> Fetch channel))
      ch = state.settings.keys["quit"]
    | None ->
      Threading.Thread.Sleep(responsiveness)
      false

  if not quit then
    keyLoop ()

keyLoop ()

Renderer.All.teardown ()
