[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Agent

let rec create state =
  MailboxProcessor.Start(fun inbox ->
    let rec loop state =
      async {
        Con.moveTo 0 0

        let! msg = inbox.Receive()
        match msg with
        | Update(state) ->
          if Set.contains Nav.LIST state.nav then
            Renderer.All.update state (Email.currentList state)
          return! loop state
        | Opening(emailId) ->
          Renderer.StatusLine.notice $"Opening email {emailId}"
          Renderer.Body.prepare ()
          return! loop state
        | ReadEmail(body) ->
          Renderer.Body.render body
          return! loop state
        | Info(message) ->
          Renderer.StatusLine.info message
          return! loop state
        | Notice(message) ->
          Renderer.StatusLine.notice message
          return! loop state
        | Error(message) ->
          Renderer.StatusLine.error message
          return! loop state
        | NewEmails(folder, emails) ->
          let newState = { state with emails = Map.add folder emails state.emails }
          Renderer.All.update newState (Email.currentList newState)
          return! loop newState
        | Quit ->
          Renderer.All.teardown ()
          return! loop { state with nav = Set.ofList [Nav.QUITING] }
        | Fetch(channel) ->
          channel.Reply(state)
          return! loop state
      }

    loop state
  )

