[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Himavan.Renderer.Body
open System

open Himavan

let prepare () =
  Con.clearToBottom BODY_START_Y


let render (body: string) =
  Con.writeAt body 0 BODY_START_Y Color.White Con.defaultBg

