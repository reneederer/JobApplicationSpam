namespace JobApplicationSpam.Translations

open System.Collections.Generic

[<AutoOpen>]
module Translations =
    open Microsoft.FSharp.Reflection
    open System.Text

    type Phrase =
        | Documents
        | Files

        //UserValues
        | UserValues
        | Gender
        | Male
        | Female
        | Unknown
        | WontTell
        | Degree
        | FirstName
        | LastName
        | Street
        | Postcode
        | City
        | Email
        | Phone
        | MobilePhone

        //Email
        | Subject
        | Body
        | AttachmentName

        // Custom Variables
        | CustomVariables
        | AddVariable

        //ApplyNow
        | ApplyAs
        | UserEmail
        | ApplyNow
        | ApplyNowButton
        | Company

    type Variable =
        | MyGender
        | MyFirstName
        | MyLastName
        | MyStreet
        | MyPostcode
        | MyCity
        | MyEmail
        | MyPhone
        | MyMobilePhone

        | BossCompany
        | BossGender
        | BossFirstName
        | BossLastName
        | BossStreet
        | BossPostcode
        | BossCity
        | BossEmail
        | BossPhone
        | BossMobilePhone

        | EmailSubject
        | EmailBody
        | EmailAttachmentName
    

    let rec translate language phrase =
        match language with
        | "en" ->
            match phrase with
                | Documents -> "Documents"
                | Files -> "Files"

                | UserValues -> "User values"
                | Gender -> "Gender"
                | Male -> "male"
                | Female -> "female"
                | Unknown -> "Unknown"
                | WontTell -> "won't tell"
                | Degree -> "Degree"
                | FirstName -> "First name"
                | LastName -> "Last name"
                | Street -> "Street"
                | Postcode -> "Postcode"
                | City -> "City"
                | Email -> "Email"
                | Phone -> "Phone"
                | MobilePhone -> "Mobile phone"

                //Email
                | Subject -> "Subject"
                | Body -> "Body"
                | AttachmentName -> "Attachment name"

                // Custom Variables
                | CustomVariables -> "Custom variables"
                | AddVariable -> "Add variable"

                //ApplyNow
                | ApplyAs -> "Apply as"
                | UserEmail -> "Your email"
                | Company -> "Company"
                | ApplyNow -> "Apply now"
                | ApplyNowButton -> "Apply now!"

        | "de" ->
            match phrase with
                | Documents -> "Dokumente"
                | Files -> "Dateien"

                | UserValues -> "Deine Daten"
                | Gender -> "Geschlect"
                | Degree -> "Titel"
                | Male -> "m\u00e4nnlich"
                | Female -> "weiblich"
                | Unknown -> "unbekannt"
                | WontTell -> "sag ich nicht"
                | FirstName -> "Vorname"
                | LastName -> "Nachname"
                | Street -> "Stra\u00dfe"
                | Postcode -> "Postleitzahl"
                | City -> "Stadt"
                | Email -> "Email"
                | Phone -> "Telefonnr"
                | MobilePhone -> "Mobilnr"

                //Email
                | Subject -> "Betreff"
                | Body -> "Nachricht"
                | AttachmentName -> "Name des Anhangs"

                // Custom Variables
                | CustomVariables -> "Benutzerdefinierte Variablen"
                | AddVariable -> "Neue Variable"

                //ApplyNow
                | ApplyAs -> "Bewerben als"
                | UserEmail -> "Deine Email"
                | Company -> "Firma"
                | ApplyNow -> "Jetzt bewerben"
                | ApplyNowButton -> "Bewerbung abschicken!"
        | _ -> translate "en" phrase
            
    
    let rec getSynonyms language (variable : Variable) =
        match language with
            | "en" ->
                match variable with
                    | MyGender -> [| "$myGender" |]
                    | MyFirstName -> [| "$myFirstName" |]
                    | MyLastName -> [| "$myLastName" |]
                    | MyStreet -> [| "$myStreet" |]
                    | MyPostcode -> [| "$myPostcode" |]
                    | MyCity -> [| "$myCity" |]
                    | MyEmail -> [| "$myEmail" |]
                    | MyPhone -> [| "$myPhone" |]
                    | MyMobilePhone -> [| "$myMobilePhone" |]

                    | BossCompany -> [| "$bossCompany"; "$company" |]
                    | BossGender -> [| "$bossGender" |]
                    | BossFirstName -> [| "$bossFirstName" |]
                    | BossLastName -> [| "$bossLastName" |]
                    | BossStreet -> [| "$bossStreet" |]
                    | BossPostcode -> [| "$bossPostcode" |]
                    | BossCity -> [| "$bossCity" |]
                    | BossEmail -> [| "$bossEmail" |]
                    | BossPhone -> [| "$bossPhone" |]
                    | BossMobilePhone -> [| "$bossMobilePhone" |]

                    | EmailSubject -> [| "$emailSubject" |]
                    | EmailBody -> [| "$emailBody" |]
                    | EmailAttachmentName -> [| "$emailAttachmentName" |]
            | "de" ->
                match variable with
                    | MyGender -> [| "$meinGeschlecht" |]
                    | MyFirstName -> [| "$meinVorname" |]
                    | MyLastName -> [| "$meinNachname" |]
                    | MyStreet -> [| "$meineStrasse" |]
                    | MyPostcode -> [| "$meinePostleitzahl" |]
                    | MyCity -> [| "$meineStadt" |]
                    | MyEmail -> [| "$meineEmail" |]
                    | MyPhone -> [| "$meineTelefonNr" |]
                    | MyMobilePhone -> [| "$meineMobilNr" |]

                    | BossCompany -> [| "$chefFirma"; "$firma" |]
                    | BossGender -> [| "$chefGeschlecht" |]
                    | BossFirstName -> [| "$chefVorname" |]
                    | BossLastName -> [| "$chefNachname" |]
                    | BossStreet -> [| "$chefPostleitzahl" |]
                    | BossPostcode -> [| "chefPostleitzahl" |]
                    | BossCity -> [| "$chefStadt" |]
                    | BossEmail -> [| "$chefEmail" |]
                    | BossPhone -> [| "$chefTelefonNr" |]
                    | BossMobilePhone -> [| "$chefMobilNr" |]

                    | EmailSubject -> [| "$emailBetreff" |]
                    | EmailBody -> [| "$emailNachricht" |]
                    | EmailAttachmentName -> [| "$emailAnhang" |]
            | _ -> getSynonyms "en" variable
    let getTranslationsDict language =
        let cases = FSharpType.GetUnionCases typeof<Phrase>
        let d =
            dict
                [ for case in cases do
                    let phrase = FSharpValue.MakeUnion(case,[||]) :?> Phrase
                    yield case.Name, Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(translate language phrase)))
                ]
        d
    let getVariablesDict language =
        let cases = FSharpType.GetUnionCases typeof<Variable>
        dict
            [ for case in cases do
                let variable = FSharpValue.MakeUnion(case,[||]) :?> Variable
                yield (case.Name, getSynonyms language variable)
            ]


