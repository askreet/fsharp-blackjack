module Blackjack.Main

open Blackjack.Cards

type BettingPhase = { Deck: Card list; PendingBet: int }

type ActionPhase =
    { Deck: Card list
      DealerVisibleCard: Card
      DealerHiddenCard: Card
      PlayerHand: Card list
      Bet: int }

type ResolutionState =
    | RevealingDealerHand
    | ResolvingDealerHand
    | Complete

type ResolutionPhase =
    { Deck: Card list
      DealerHand: Card list
      PlayerHand: Card list
      State: ResolutionState
      Bet: int }

type EndPhase = { MoneyChange: int }

type GamePhase =
    | Betting of BettingPhase
    | Action of ActionPhase
    | Resolution of ResolutionPhase
    | End of EndPhase

type GameState = { Money: int; Phase: GamePhase }

let placeBet (bettingPhase: BettingPhase) : ActionPhase =
    let dealerCards, deck =
        bettingPhase.Deck |> List.splitAt 2

    let playerCards, deck =
        deck |> List.splitAt 2

    { Deck = deck
      DealerVisibleCard = dealerCards |> List.item 0
      DealerHiddenCard = dealerCards |> List.item 1
      PlayerHand = playerCards
      Bet = bettingPhase.PendingBet }

let hit (phase: ActionPhase) : ActionPhase =
    match phase.Deck with
    | [] -> failwith "unexpected end of deck"
    | newCard :: deck ->
        { phase with
            Deck = deck
            PlayerHand = phase.PlayerHand @ [ newCard ] }

let stand (phase: ActionPhase) : ResolutionPhase =
    { Deck = phase.Deck
      DealerHand =
        [ phase.DealerHiddenCard
          phase.DealerVisibleCard ]
      PlayerHand = phase.PlayerHand
      State = ResolvingDealerHand
      Bet = phase.Bet }

let bust (phase: ActionPhase) : EndPhase = { MoneyChange = -phase.Bet }

let tryHit state =
    match state.Phase with
    | Action a ->
        let newPhase = hit a

        if value newPhase.PlayerHand > 21 then
            printfn $"Busted! You lose ${newPhase.Bet}"
            { state with Phase = End(bust newPhase) }
        else
            { state with Phase = Action(hit a) }
    | _ ->
        printfn "cannot hit now!"
        state

let tryStand state =
    match state.Phase with
    | Action a -> { state with Phase = Resolution(stand a) }
    | _ ->
        printfn "cannot stand now!"
        state

let display (state: GameState) =
    match state.Phase with
    | Betting b ->
        printfn "Place Bet!"
        printfn $"Money: %d{state.Money}  Current Bet: %d{b.PendingBet}"
        printfn "a) add 10"
        printfn "b) subtract 10"
        printfn "d) deal cards"
    | Action a ->
        printfn $"Dealer Hand: Xx %s{displayCards [ a.DealerVisibleCard ]}"
        printfn $"Your Hand (%d{value a.PlayerHand}): %s{displayCards a.PlayerHand}"
        printfn "h) hit"
        printfn "s) stand"
    | Resolution _ -> printfn "resolving"
    | End _ -> printfn "end"

let makeDeck: Card list =
    [ for rank in Rank.all do
          for suit in Suit.all do
              Card(suit, rank) ]

type ResolutionStep =
    | DealerHit of Card * Card list
    | DealerBust
    | DealerLoss
    | DealerWin
    | Push

let resolver (phase: ResolutionPhase) : ResolutionStep =
    let dealerHandValue = value phase.DealerHand
    let playerHandValue = value phase.PlayerHand

    if dealerHandValue <= 16 then
        match phase.Deck with
        | [] -> failwith "unexpected end of deck"
        | card :: deck -> DealerHit(card, deck)
    elif dealerHandValue > 21 then
        DealerBust
    elif dealerHandValue > playerHandValue then
        DealerWin
    elif dealerHandValue = playerHandValue then
        Push
    else
        DealerLoss

let rec animateResolution (phase: ResolutionPhase) resolver : EndPhase =
    match resolver phase with
    | DealerHit(newCard, deck) ->
        let dealerHand =
            phase.DealerHand @ [ newCard ]

        printfn $"Dealer hits with a {displayCards [ newCard ]} ({value dealerHand})"

        animateResolution
            { phase with
                DealerHand = dealerHand
                Deck = deck }
            resolver
    | DealerBust ->
        printfn "Dealer busts. You win!"
        { MoneyChange = phase.Bet }
    | DealerLoss ->
        printfn $"Dealer stands at {value phase.DealerHand}, you win ${phase.Bet}!"
        { MoneyChange = phase.Bet }
    | DealerWin ->
        printfn $"Dealer stands at {value phase.DealerHand}, you lose ${phase.Bet}!"
        { MoneyChange = -phase.Bet }
    | Push ->
        printfn $"Dealer stands at {value phase.DealerHand}, bet is refunded."
        { MoneyChange = 0 }

let promptBettingAction (phase: BettingPhase) : GamePhase * bool =
    let key =
        System.Console.ReadKey(true).Key.ToString()

    match key with
    | "A" ->
        // TODO: Bounds
        Betting({ phase with PendingBet = phase.PendingBet + 10 }), false
    | "B" ->
        // TODO: Bounds
        Betting({ phase with PendingBet = phase.PendingBet - 10 }), false
    | "D" -> Action(placeBet phase), false
    | "Q" -> Betting(phase), true
    | _ ->
        printfn "invalid"
        Betting(phase), false

let promptAction state =
    let key =
        System.Console.ReadKey(true).Key.ToString()

    match key with
    | "H" -> tryHit state, false
    | "S" -> tryStand state, false
    | "Q" -> state, true
    | _ ->
        printfn "invalid"
        state, false

[<EntryPoint>]
let main _args =
    let rec loop (state: GameState) =
        display state

        let newState, quit =
            match state.Phase with
            | End e ->
                { state with
                    Money = state.Money + e.MoneyChange
                    Phase =
                        Betting
                            { Deck = ListShuffle.shuffle makeDeck
                              PendingBet = 10 } },
                false
            | Resolution r -> { state with Phase = End(animateResolution r resolver) }, false
            | Betting b ->
                let nextPhase, quit = promptBettingAction b
                { state with Phase = nextPhase }, quit
            | _ -> promptAction state

        if not quit then
            loop newState

    let initState =
        { Money = 100
          Phase =
            Betting
                { Deck = ListShuffle.shuffle makeDeck
                  PendingBet = 10 } }

    loop initState

    0
