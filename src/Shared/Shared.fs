namespace Shared

open System

type TodoKey = Guid

[<CLIMutable>]
type Todo =
    { Id : TodoKey
      Description : string
      Completed : bool
      }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description
          Completed = false
          }

    let initial = [
            create "Create new SAFE project"
            create "Write your app"
            create "Ship it !!!"
        ]


module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

module Remote =

    let endpoint = "/socket"

    type FromServerMessage =
    | AllTodos of Todo list
    | TodoAdded of Todo
    | TodoUpdated of Todo
    | TodoDeleted of TodoKey

    type FromClientMessage =
    | GetTodos
    | AddTodo of Todo
    | UpdateTodo of Todo
    | ClearCompletedTodos
    | DeleteTodo of TodoKey
