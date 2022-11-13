module Blackjack.Cards

type Suit =
    | Heart
    | Diamond
    | Spade
    | Club

    static member all = [ Heart; Diamond; Spade; Club ]

type Rank =
    | Ace
    | Number of int
    | Jack
    | Queen
    | King

    static member all =
        [ Ace ]
        @ [ for value in 2..10 do
                Number(value) ]
          @ [ Jack; Queen; King ]

type Card = Suit * Rank

type Hand = Card list

let displayCards (cards: Hand) : string =
    let prettySuit s =
        match s with
        | Heart -> "♥️"
        | Diamond -> "♦️"
        | Spade -> "♠️"
        | Club -> "♣️"

    let prettyRank r =
        match r with
        | Ace -> "A"
        | Jack -> "J"
        | Queen -> "Q"
        | King -> "K"
        | Number n -> n.ToString()

    let prettyCard (card: Card) : string =
        let s, r = card

        $"%s{prettyRank r}%s{prettySuit s}"

    cards |> List.map prettyCard |> String.concat " "

let rec value' (hand: Hand) (options: int list list) =
    match hand with
    | [] -> options
    | (_, rank) :: rest ->
        let expandedOptions: int list list =
            match rank with
            | Ace ->
                let ifOne =
                    options |> List.map (List.append [ 1 ])

                let ifEleven =
                    options |> List.map (List.append [ 11 ])

                ifOne @ ifEleven
            | King
            | Queen
            | Jack -> options |> List.map (List.append [ 10 ])
            | Number n -> options |> List.map (List.append [ n ])

        value' rest expandedOptions

let value (hand: Hand) =
    let options =
        value' hand [ [ 0 ] ] |> List.map List.sum

    let winnableOptions =
        options |> List.filter (fun v -> v <= 21)

    if List.isEmpty winnableOptions then
        options |> List.min
    else
        winnableOptions |> List.max
