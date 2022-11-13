# f#-blackjack

A simple CLI implementation of the game Blackjack written in F#, created as a learning exercise.

## Goals

* Write a non-trivial amount of F# and force myself to think "functionally" about the implementation (despite F# supporting mutable flows).
  * The program has zero mutation, currently

## Key Takeaways

* F# stdlib doesn't ship with a shuffle function, I cribbed one from the Internet and updated it a bit (Shuffle.fs).
* Having load order be important is annoying, both for files (.fsproj) and functions within a file. Feels like a step backwards from e.g. Rust.
* F# as a language is _fairly_ approachable, but far more rewarding if you force yourself to think in a functions-first way.
* Documentation around F# is "okay". The Microsoft pages are really tedious, and blog posts can be hit-or-miss.
  * Many of the blogs tends to focus on evangelizing FP rather than solving real problems. It's quite annoying.
* Tooling for F# on Mac is pretty good actually.
* Standalone binaries which embed the CLR are available.
* Cross-compilation appears to be possible, but did not test this.
* C# <-> F# interop stuff is weird. I used Rider and vim for the majority of my development and while most blogs and documentation say that `List<'a>` and `'a list` are equivilent, I got a lot of strange errors when using them interchangably. I eventually settled on only using the `'a list` form and all my problems went away.
* Optional function typing is cool, really feels like best of both worlds:
  * For trivial functions, just skip them. The automatic generalization will make it clear if the function works or not.
  * For other functions, include them as a form of documentation and to aid hints in the IDE.
    * This felt good when I included them for all state-transitioning functions.
* I'm _sure_ my code could be simplified by bringing in some monadic patterns, but I'm very hesitant to go in that direction.
  * I could probably use `Result<'a, 'b>` and `>>` for handling invalid inputs, or signaling a desired quit.
* Overall, I'd be happy working with F# day-to-day. I'm curious about it's place in web services and such. Would love to play around with Fable in the browser.

## Future Ideas

* Learn an F# testing framework.
* Integrate with some sort of UI and animate more of the gameplay.
* Find some succinct model for handling splits, insurance, etc. plays.
* Add AI players to the table, and animate their moves, too.
