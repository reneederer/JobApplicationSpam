namespace JobApplicationSpam.CustomVariableParser
open FParsec

module Say =
    let hello name =
        printfn "Hello %s" name
