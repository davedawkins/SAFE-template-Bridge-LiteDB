module App

open Elmish
open Elmish.React
open Elmish.Bridge

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

open Fable.Remoting.Client
open Shared
open TodoApp.Types

// I need a way to set this true only when building for gh-pages

Program.mkProgram TodoApp.State.init TodoApp.State.update TodoApp.View.view
|> Program.withBridgeConfig (Bridge.endpoint Shared.Remote.endpoint |> Bridge.withMapping Msg.FromServer)
#if DEBUG
|> Program.withConsoleTrace
|> Program.withDebugger
#endif
|> Program.withReactSynchronous "elmish-app"
|> Program.run
