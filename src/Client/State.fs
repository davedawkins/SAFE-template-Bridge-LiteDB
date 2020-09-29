module TodoApp.State
open Types
open Elmish
open Shared
open Shared.Remote
open Elmish.Bridge

let init (): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = ""
          Filter = All
          Editing = None
          }
    // We need to wait until the bridge has connected.
    let cmd = Cmd.none // Cmd.OfAsync.perform api.getTodos () GotTodos
    model, cmd

// Linear searching! There are better ways of doing this in F# I'm sure.
// In C# I'd use a Dictionary<Guid,Todo>. That's still a mutable mindset though.
let replaceTodo todos todo =
    List.map (fun t -> if todo.Id = t.Id then todo else t) todos

let updateEdit (msg : EditMsg) (model : EditModel option) : EditModel option * Cmd<EditMsg> * EditStatus =
    match model with
    | None -> None, Cmd.none, Continue
    | Some edit ->
        match msg with
        | SetEditInput s ->
            Some { edit with Input = s }, Cmd.none, Continue
        | Commit -> // Nothing to do at this level, signal parent to finish
            None, Cmd.none, Finish edit

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with

    // Messages that work on local state

    | EditMessage em ->
        let (editState, cmd, status) = updateEdit em model.Editing
        match status with
        | Continue ->
            { model with Editing = editState }, Cmd.map EditMessage cmd
        | Finish edit ->
            { model with Editing = editState },
            Cmd.batch [
                Cmd.map EditMessage cmd
                Cmd.ofMsg <| Msg.UpdateTodo { edit.Todo with Description = edit.Input }
            ]

    | StartEdit todo ->
        { model with Editing = Some { Todo = todo; Input = todo.Description } }, Cmd.none

    | SetInput value ->
        { model with Input = value }, Cmd.none

    | SetFilter f ->
        { model with Filter = f }, Cmd.none

    // Messages that update storage (and so talk to the server)

    | SetCompleted (todo,completed) ->
        model, Cmd.ofMsg (Msg.UpdateTodo { todo with Completed = completed })

    | Msg.UpdateTodo todo ->
        model, Cmd.ofMsg (todo |> FromClientMessage.UpdateTodo |> ForServer)

    | Msg.AddTodo ->
        let todo = Todo.create model.Input
        { model with Input = "" }, Cmd.ofMsg (todo |> Remote.FromClientMessage.AddTodo |> ForServer)

    | ClearCompleted ->
        model, Cmd.ofMsg (Remote.FromClientMessage.ClearCompletedTodos |> ForServer)

    | Msg.DeleteTodo key ->
        model, Cmd.ofMsg (key |> Remote.FromClientMessage.DeleteTodo |> ForServer)

    // Single message to encapsulate communication to server (via bridge)
    | ForServer clientMsg ->
        model, Cmd.bridgeSend clientMsg

    // Messages from server
    | FromServer msg ->
        match msg with
        | TodoAdded todo ->
            System.Console.WriteLine("Adding todo {0}", todo)
            { model with Todos = model.Todos @ [ todo ] }, Cmd.none
        | AllTodos todos ->
            { model with Todos = todos }, Cmd.none
        | TodoUpdated todo ->
            { model with Todos = (replaceTodo model.Todos todo)}, Cmd.none
        | TodoDeleted key ->
            { model with Todos = model.Todos |> List.filter (fun x -> x.Id <> key) }, Cmd.none


