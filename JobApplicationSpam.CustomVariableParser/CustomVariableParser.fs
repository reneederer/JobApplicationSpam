namespace JobApplicationSpam.CustomVariableParser
open FParsec
open FSharp.Core.Result
open System.Collections.Generic;

module Variables =
    open FParsec
    open System.Collections.Generic

    type AssignedVariable = string
    type MatchValue = string
    type MatchCompareVariable = string
    type LiteralString = string

    type Expression =
    | MatchExpression of MatchCompareVariable * list<MatchValue * LiteralString>
    | VariableExpression of LiteralString

    type Definition =
        { assignedVariable : AssignedVariable
          expression : Expression
        }

    let getValue (definition : Definition) (definedVariables : IDictionary<string, string>) =
        match definition.expression with
        | MatchExpression (matchCompareVariable, lines) ->
            let searchMatchValue = 
                let (parsedSuccessfully, value) =
                    definedVariables.TryGetValue(matchCompareVariable)
                if parsedSuccessfully
                then value
                else ""

            lines
            |> List.tryPick
                (fun (variableValue, literalString) ->
                    if variableValue = searchMatchValue
                    then Some literalString
                    else None)
            |> Option.defaultValue ""
        | VariableExpression s ->
            s
        
    let spacedNewline =
        attempt
            (manyChars (anyOf [' '; '\t']))
            .>> newline
            .>> manyChars (anyOf [' '; '\t'])
    
    let spacesFollowedByNewline =
        attempt (many (spacedNewline .>> followedBy newline))
        |>> (fun xs -> System.String.Concat xs)
    
    let spacesNoNewline =
        manyChars (anyOf [' '; '\t'])
    
    let spacesTillNewline =
        spacesFollowedByNewline <|> spacesNoNewline


    let pVariable =
        spaces
        .>> pchar '$'
        >>. manyChars (noneOf [' '; '\t'; '\n'])
        |>> (fun x -> "$" + x)

    let pLiteralString =
        spaces
        >>. pchar '"'
        >>. (manyChars (noneOf ['"']))
        .>> pchar '"'
        .>> spacesTillNewline

    let pMatchCompareVariable =
        spaces
        >>. pstring "match"
        >>. spaces1
        >>. pVariable
        .>> spaces1
        .>> pstring "with"
        .>> spacesTillNewline

    let pMatchLines =
        spaces
        >>. pchar '|'
        >>. spaces
        >>. pLiteralString
        .>> spaces
        .>> pstring "->"
        .>> spaces
        .>>. pLiteralString

    let pConcatenatedStrings1 =
        spaces
        >>. sepBy1
                ((attempt (pVariable .>> spacesTillNewline))
                   <|> (attempt (pLiteralString .>> spacesTillNewline)))
                (attempt (pchar '+' .>> spaces))
        |>> (fun xs -> String.concat "" xs)

    let pAssignedVariable : Parser<AssignedVariable, unit> =
        spaces
        >>. pVariable

    let pMatchDefinition =
        spaces
        >>. pAssignedVariable
        .>> spaces
        .>> pchar '='
        .>> spaces
        .>>. pMatchCompareVariable
        .>> spacesTillNewline
        .>> newline
        .>> spaces
        .>>. (many1 (attempt (pMatchLines .>> spacesTillNewline)))
        |>> (fun ((assignedVariable, matchCompareVariable), kvList) ->
                { assignedVariable = assignedVariable; expression = MatchExpression (matchCompareVariable, kvList) }
            )

    let pVariableDefinition =
        spaces
        >>. pAssignedVariable
        .>> spaces
        .>> pchar '='
        .>> spaces
        .>>. pConcatenatedStrings1
        |>> fun (assignedVariable, literalString) ->
                { assignedVariable = assignedVariable; expression = VariableExpression literalString }

    let pDefinitions =
        (attempt pMatchDefinition <|> attempt pVariableDefinition)

    let parse str =
        match run pDefinitions str with
        | Success (result, _, _) -> FSharp.Core.Result.Ok result
        | Failure (result, _, _) -> FSharp.Core.Result.Error "Couldn't parse."

    let parseMany strs =
        let parsedVariables = 
            [ for str in strs do
                yield parse str
            ]
        let (_, errors, successes) =
            List.fold (fun (i, errors, successes) parsedVariable ->
                match parsedVariable with
                | FSharp.Core.Result.Ok v -> (i + 1, errors, (successes @ [v]))
                | FSharp.Core.Result.Error v -> (i + 1, (errors @ [sprintf "Variable %i: %s" i v]), successes)
                )
                (1, [], [])
                parsedVariables
        if errors = []
        then successes |> Seq.ofList
        else 
            failwith (String.concat "\r\n\r\n" errors)
    
    let addVariablesToDict(variableTexts, dict : IDictionary<string, string>) =
        try
            let definitions = parseMany variableTexts
            for definition in definitions do
                dict.Add(definition.assignedVariable, getValue definition dict)
        with
        | e -> ()











