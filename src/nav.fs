[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Nav

let create () = Set.ofList [Nav.LIST]
let onList state = Set.contains Nav.LIST state.nav
let isShowingAddress state = Set.contains Nav.ADDR state.nav
let isQuiting state = Set.contains Nav.QUITING state.nav
let isReading state = Set.contains Nav.OPEN state.nav


let list state =
  { state with
      nav = state.nav
      |> Set.add Nav.LIST
      |> Set.remove Nav.OPEN
  }


let read state =
  { state with
      nav = state.nav
      |> Set.add Nav.OPEN
      |> Set.remove Nav.LIST
  }


let showAddress state =
  { state with nav = state.nav |> Set.add Nav.ADDR }


let hideAddress state =
  { state with nav = state.nav |> Set.remove Nav.ADDR }
