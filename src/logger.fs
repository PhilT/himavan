module Himavan.Logger
open System.IO

let LOG_FILE = "himavan.log"
File.Delete(LOG_FILE)
let logging = File.ReadLines("settings.toml") |> Seq.contains "logging = on"

let write mo func args =
  if logging then
    File.AppendAllLines(LOG_FILE, [$"[{mo}.{func}] {args}"])

