﻿namespace Waux.Lang

module Lexer =

    open Models

    type ComplexTokenType =
        | Letter
        | Digit
        | Illegal

    let lookupIdent ident =
        if ident = "func" then
            FUNC
        else if ident = "let" then
            LET
        else if ident = "if" then
            IF
        else if ident = "else" then
            ELSE
        else if ident = "elif" then
            ELIF
        else if ident = "and" then
            AND
        else if ident = "or" then
            OR
        else if ident = "while" then
            WHILE
        //else if ident = "return" then
        //    RETURN
        //else if ident = "true" then
        //    TRUE
        //else if ident = "false" then
        //    FALSE
        else
            IDENT

    let readChar (l: LexerState) =
        let newChar =
            match l.readPosition >= l.input.Length with
            | true -> '\000'
            | false -> l.input.Chars l.readPosition

        l.position <- l.readPosition
        l.readPosition <- l.readPosition + 1
        l.ch <- newChar

    let peekChar (l: LexerState) =
        match l.readPosition >= l.input.Length with
        | true -> '\000'
        | false -> l.input.Chars l.readPosition

    let isLetter (ch: char) =
        let lowerCase = ch.CompareTo('a') >= 0 && ch.CompareTo('z') <= 0
        let upperCase = ch.CompareTo('A') >= 0 && ch.CompareTo('Z') <= 0
        let underscore = ('_' = ch)
        lowerCase || upperCase || underscore

    let isDigit (ch: char) =
        ch.CompareTo('0') >= 0 && ch.CompareTo('9') <= 0

    let canReadLetter (l: LexerState) =
        //ensure I can read next position
        let canReadNextPosition = l.position + 1 < l.input.Length

        canReadNextPosition
        && isLetter (l.input.Chars(l.position + 1))

    let readIdentifier (l: LexerState) =
        let pos = l.position

        while canReadLetter (l) do
            readChar l

        let literal = l.input.Substring(pos, (l.position - pos + 1))
        let tokenType = lookupIdent literal
        (tokenType, literal)

    let canReadDigit (l: LexerState) =
        //ensure I can read next position
        let canReadNextPosition = l.position + 1 < l.input.Length

        canReadNextPosition
        && isDigit (l.input.Chars(l.position + 1))

    let readNumber (l: LexerState) =
        let pos = l.position

        while canReadDigit (l) do
            readChar l

        let literal = l.input.Substring(pos, (l.position - pos + 1))
        (NUMBER, literal)

    let findComplexTokenType l =
        if isLetter (l.ch) then Letter
        else if isDigit (l.ch) then Digit
        else Illegal

    let nextComplexToken (l: LexerState) =
        match findComplexTokenType (l) with
        | Letter -> readIdentifier (l)
        | Digit -> readNumber (l)
        | Illegal -> (Token.ILLEGAL, l.ch.ToString())

    let skipWhitespace (l: LexerState) =
        while l.ch = ' '
              || l.ch = '\t'
              || l.ch = '\n'
              || l.ch = '\r' do
            readChar l

        ()

    let nextTwoCharToken (l: LexerState) (secondChar: char) (matchToken: Token) (nonMatchToken: Token) =
        let nextChar = peekChar l

        match nextChar with
        | nc when nc = '=' ->
            let ch = l.ch
            readChar l
            (matchToken, ch.ToString() + l.ch.ToString())
        | _ -> (nonMatchToken, l.ch.ToString())

    let nextToken (l: LexerState) =
        skipWhitespace l

        let (tokenType, literal) =
            match l.ch with
            | '\000' -> (Token.EOF, "")
            | '+' -> (Token.PLUS, l.ch.ToString())
            | '-' -> (Token.MINUS, l.ch.ToString())
            | '*' -> (Token.ASTERISK, l.ch.ToString())
            | '/' -> (Token.SLASH, l.ch.ToString())
            | '(' -> (Token.LPAREN, l.ch.ToString())
            | ')' -> (Token.RPAREN, l.ch.ToString())
            | ';' -> (Token.SEMICOLON, l.ch.ToString())
            | '{' -> (Token.LBRACE, l.ch.ToString())
            | '}' -> (Token.RBRACE, l.ch.ToString())
            | ',' -> (Token.COMMA, l.ch.ToString())
            | '=' -> nextTwoCharToken l '=' Token.EQ Token.ASSIGN
            | '!' -> nextTwoCharToken l '=' Token.NOT_EQ Token.ILLEGAL
            | '<' -> nextTwoCharToken l '=' Token.LT_EQ Token.LT
            | '>' -> nextTwoCharToken l '=' Token.GT_EQ Token.GT
            | ':' -> nextTwoCharToken l '=' Token.ASSIGNMENT Token.ILLEGAL
            | _ -> nextComplexToken l

        let token = { Token = tokenType; Literal = literal }

        readChar l
        token

    let createLexer input =
        let lexer =
            { input = input
              position = 0
              readPosition = 0
              ch = '\000' }

        readChar lexer
        lexer
