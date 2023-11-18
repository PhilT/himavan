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

let rec agent = MailboxProcessor.Start(fun inbox ->
  let rec loop state =
    async {
      Con.moveTo 0 0

      let! msg = inbox.Receive()
      match msg with
      | Update(ch) ->
        let newState = State.update ch state agent
        Renderer.update newState (State.currentEmails newState)
        return! loop newState
      | NewEmails(folder, emails) ->
        Logger.write "Main" "agent" $"{emails.Count} emails in {folder}"
        let newState = { state with emails = Map.add folder emails state.emails }
        Renderer.update newState (State.currentEmails newState)
        return! loop newState
      | Fetch(channel) ->
        channel.Reply(state)
        return! loop state
    }

  loop state
)

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
      Threading.Thread.Sleep(250)
      false

  if not quit then
    keyLoop ()

keyLoop ()

Renderer.teardown ()
