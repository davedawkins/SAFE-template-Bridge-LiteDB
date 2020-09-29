module TodoApp.Types

open Shared

open Shared.Remote

type FilterOptions =
    | All
    | Active
    | Complete

// Editing an existing todo
type EditMsg =
    | SetEditInput of string
    | Commit

// State for editing a todo
type EditModel =
    {
        Todo: Todo
        Input: string
    }

// How updateEdit tells parent (update) what to do
type EditStatus =
    | Continue
    | Finish of EditModel

// Global UI state
type Model =
    {
      Todos: Todo list
      Input: string
      Filter: FilterOptions
      Editing: EditModel option  // Some when editing, None when not.
    }

type Msg =
    | SetInput of string
    | SetCompleted of Todo * bool
    | AddTodo // Create todo from input field
    | UpdateTodo of Todo
    | DeleteTodo of TodoKey
    | ClearCompleted
    | SetFilter of FilterOptions
    | StartEdit of Todo
    | EditMessage of EditMsg
    | FromServer of FromServerMessage
    | ForServer of FromClientMessage

