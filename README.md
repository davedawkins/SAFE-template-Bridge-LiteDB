# SAFE Template using Bridge & LiteDB

This is a fork of SAFE-template-extended, where I've replaced Fable.Remoting with Elmish.Bridge. This gives an Elmish architecture to the client-server communication, and brings with it a websocket layer instead of the HTTP used by Remoting
.

Again, this was learning exercise for me.

Here's the original README for SAFE-template-extended:

This template was created with `dotnet new safe` from [SAFE Stack](https://safe-stack.github.io/). I've then added the following features to approach something closer to the standard TodoMVC example. 

Server:
- replaced the in-memory storage with `LiteDB` using [LiteDB.FSharp](https://github.com/Zaid-Ajaj/LiteDB.FSharp).

Client:
- option to use in-memory storage so that I can run a live demo (see below) on github.io
- added view filters
- return key will add new todo
- double-click to edit existing do
- checkbox to toggle todo complete
- (x) button to delete todo
- (clear completed) button to delete all completed todos
- style.css

Build:
- publish.js, PublishApp target in build.fsx to support publish to gh-pages

[Live Demo](https://davedawkins.github.io/SAFE-template-extended/)

