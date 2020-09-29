module Server

open Elmish.Bridge
open Saturn
open LiteDB.FSharp
open LiteDB

open Shared

let database dbFile =
    let mapper = FSharpBsonMapper()
    let dbFile = Environment.databaseFilePath

    let connStr =
        sprintf "Filename=%s;mode=Exclusive" dbFile

    new LiteDatabase(connStr, mapper)

type Storage(db: LiteDatabase) as this =
    let collection = "todos"
    let todos = db.GetCollection<Todo> collection

    do
        if not (db.CollectionExists collection) then
            for todo in Todo.initial do
                this.AddTodo(todo) |> ignore

    member __.ClearCompleted() =
        todos.Delete(fun x -> x.Completed) |> ignore
        this.GetTodos()

    member __.GetTodos() = todos.FindAll() |> List.ofSeq

    member __.AddTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Insert(todo) |> ignore
            todo
        else
            failwith "Invalid todo"

    member __.UpdateTodo(todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Update([ todo ]) |> ignore
            todo
        else
            failwith "Invalid todo"

    member __.Delete(todo: TodoKey) =
        todos.Delete(fun x -> x.Id = todo) |> ignore
        todo


open Shared.Remote
open Elmish

// Server-side top-level messages.
// Messages from clients will arrive wrapped in FromClient
// (see Bridge.register below)
// Other messages are used by the update handler to send responses
// back to clients and to process errors
type ServerMsg =
    | FromClient of Remote.FromClientMessage
    | ForHub of FromServerMessage
    | ForClient of FromServerMessage
    | Error of exn

// Server-side state for each connected client.
// init will be called for each incoming connection
// How do we detect disconnects?
type ServerState = Nothing

// Context for all connected clients. A connection to the API (which handles storage)
// and the hub which manages those clients.
type ServerContext =
    { Api: Storage
      Hub: ServerHub<ServerState, ServerMsg, FromServerMessage> }

let serverContext =
    { Api =
          Environment.databaseFilePath
          |> database
          |> Storage

      // The postsHub keeps track of connected clients and has broadcasting logic
      Hub = ServerHub<ServerState, ServerMsg, FromServerMessage>().RegisterServer(FromClient) }

// New client connected, send the current list of todos
let init ctx clientDispatch arg =
    Nothing, Cmd.OfFunc.perform ctx.Api.GetTodos () (AllTodos >> ForClient)

// react to messages coming from client
let update ctx currentClientDispatch (serverMsg: ServerMsg) currentState: ServerState * Cmd<ServerMsg> =

    match serverMsg with
    // Process message from client
    | FromClient clientMsg ->
        let cmd fn arg msg =
            Cmd.OfFunc.either fn arg (msg >> ForHub) Error
        match clientMsg with
        | AddTodo todo -> currentState, cmd ctx.Api.AddTodo todo TodoAdded
        | GetTodos -> currentState, cmd ctx.Api.GetTodos () AllTodos
        | UpdateTodo todo -> currentState, cmd ctx.Api.UpdateTodo todo TodoUpdated
        | ClearCompletedTodos -> currentState, cmd ctx.Api.ClearCompleted () AllTodos
        | DeleteTodo todo -> currentState, cmd ctx.Api.Delete todo TodoDeleted

    // Send to all connected clients
    | ForHub hubMsg -> currentState, Cmd.OfFunc.attempt ctx.Hub.BroadcastClient hubMsg Error

    // Send to the current client only
    | ForClient hubMsg -> currentState, Cmd.OfFunc.attempt currentClientDispatch hubMsg Error

    | Error exn ->
        System.Console.WriteLine("Caught Error: {0}", exn.Message)
        currentState, Cmd.none

let socketServer ctx =
    Bridge.mkServer Shared.Remote.endpoint (init ctx) (update ctx)
    //|> Bridge.withConsoleTrace
    |> Bridge.withServerHub ctx.Hub
    // register the types we can receive
    |> Bridge.register FromClient
    |> Bridge.run Giraffe.server

let app =
    application {
        use_router (socketServer serverContext)
        app_config Giraffe.useWebSockets
        url "http://0.0.0.0:8085"
        memory_cache
        use_static "public"
        use_gzip
    }

run app
