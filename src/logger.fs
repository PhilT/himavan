module Himavan.Logger
open System.IO

let LOG_FILE = "himavan.log"
File.Delete(LOG_FILE)

let write mo func args =
  File.AppendAllLines(LOG_FILE, [$"[{mo}.{func}] {args}"])

