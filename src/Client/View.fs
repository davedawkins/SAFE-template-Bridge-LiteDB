module TodoApp.View

open Shared
open TodoApp.Types
open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
            Navbar.Item.IsActive true
        ] [
            img [
                Src "favicon.png"
                Alt "Logo"
            ]
        ]
    ]

let numItemsLeft (model:Model) =
    model.Todos |> List.filter (fun t -> not t.Completed) |> List.length

let OnReturn f =
    DOMAttr.OnKeyPress (fun e ->
        if e.charCode = 13.0 then e.preventDefault(); f(e)
    )

let todoInput model dispatch =
    Field.div [ Field.IsGrouped ] [
        Control.p [ Control.IsExpanded ] [
            Input.text [
              Input.Value model.Input
              Input.Placeholder "What needs to be done?"
              Input.OnChange (fun x -> SetInput x.Value |> dispatch)
              Input.Props [ OnReturn (fun _ -> dispatch AddTodo) ]
              ]
        ]
    ]

let editInput (editState : EditModel) dispatch =
    Field.div [ Field.IsGrouped ] [
        Control.p [ Control.IsExpanded ] [
            Input.text [
              Input.Value editState.Input
              Input.OnChange (fun x -> SetEditInput x.Value |> dispatch)
              Input.Props [
                  OnReturn (fun _ -> dispatch Commit)
                  OnBlur (fun _ -> dispatch Commit )
                  AutoFocus true ]
              ]
        ]
    ]

let filterButton model dispatch label (op : FilterOptions) =
    Radio.radio [
        Props [
            Checked (model.Filter = op)
            OnChange (fun _ -> SetFilter op |> dispatch)
        ]
    ] [
        Radio.input [
            Radio.Input.Name "Filters"
            Radio.Input.Props [ Checked (op = model.Filter) ]
        ]
        str label
    ]

let Centered (items : seq<ReactElement>) =
    div [ Style [
                Display DisplayOptions.Flex
                JustifyContent "center"
            ]
    ] items

let todoOptionBar model (dispatch: Msg -> unit) =
    let plural n label = sprintf "%d %s%s" n label (if n = 1 then "" else "s")
    let n = numItemsLeft model

    Columns.columns [ ] [
        Column.column [ Column.Width ( Screen.All, Column.Is3 ); Column.CustomClass "has-text-left" ] [
            plural n "item" |> sprintf "%s left" |> str
        ]
        Column.column [
            Column.Width ( Screen.All, Column.Is6 )
        ] [
            Centered [
                filterButton model dispatch  " All" FilterOptions.All
                filterButton model dispatch  " Active" FilterOptions.Active
                filterButton model dispatch  " Complete" FilterOptions.Complete
            ]
        ]
        Column.column [ Column.Width ( Screen.All, Column.Is3 ); Column.CustomClass "has-text-right" ] [
            a [ Href "#"; OnClick (fun e -> e.preventDefault(); dispatch ClearCompleted ) ] [str  "Clear completed" ]
        ]
    ]


let todoList model dispatch =
    Content.content [ ] [
        let vis (todo : Todo) =
            match model.Filter with
            | FilterOptions.All -> true
            | Active -> not todo.Completed
            | Complete -> todo.Completed

        for todo in model.Todos |> List.filter vis ->
            let completeClass = if todo.Completed then [ "completed"] else []
            let itemClasses = [ "todo-item" ] @ completeClass

            Columns.columns  [ Columns.CustomClass (String.concat " " itemClasses) ] [
                Column.column [Column.Width ( Screen.All, Column.Is1 )] [
                    Checkbox.checkbox [ ] [
                        Checkbox.input [
                            Modifiers [ Modifier.Spacing (Spacing.MarginRight, Spacing.Is2) ];
                            CustomClass "is-normal"
                            Props [
                                Checked todo.Completed;
                                OnClick (fun _ -> (todo, not todo.Completed) |> SetCompleted |> dispatch)
                            ]
                        ]
                    ]
                ]
                Column.column [Column.Width ( Screen.All, Column.Is10 )] [
                    match model.Editing with
                    | Some edit when edit.Todo.Id = todo.Id ->
                        editInput edit (EditMessage >> dispatch)
                    | _ ->
                        span [ OnDoubleClick (fun _ -> StartEdit todo |> dispatch) ] [ str todo.Description ]
                ]
                Column.column [] [
                    button [
                        ClassName "destroy"
                        OnClick (fun _ -> DeleteTodo todo.Id |> dispatch)
                    ] [ str "✕" ]
                ]
            ]
        ]


let containerBox (model : Model) (dispatch : Msg -> unit) =
    Box.box' [
        ] [
        todoInput model dispatch
        todoList model dispatch
        todoOptionBar model dispatch
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [ navBrand ]
            ]
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.h1 [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "todos" ]
                    containerBox model dispatch
                ]
            ]
        ]
    ]

