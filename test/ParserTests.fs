﻿namespace Waux.Lang.Test

module ParserTests =
    open Xunit
    open Waux.Lang
    let asInteger (e : Ast.Expression) =
        match e with
        | :? Ast.IntegerLiteral as integer -> 
            Some integer
        | _ -> None
    let asLetStatement (s: Ast.Statement) =
        match s with
        | :? Ast.LetStatement as letState ->
            Ok letState
        | _ -> Error "Not an let statement"
    let asIdentifier (s: Ast.Expression) =
        match s with
        | :? Ast.Identifier as iden ->
            Ok iden
        | _ -> Error "Not an identifier"
    let asExpressionStatement (s: Ast.Statement) =
        match s with
        | :? Ast.ExpressionStatement as es -> 
            Ok es
        | _ -> Error "Not an expression statement"
    let asIdentifierFromStatement (s: Ast.Statement) =
        let asExprState = asExpressionStatement s
        match asExprState with
        | Ok es ->
            let iden = asIdentifier es.expression
            match iden with
            | Ok id ->
                Ok id
            | Error msg -> Error msg
        | Error msg -> Error msg
    //let asExpressionStatement (s: Ast.Statement) =
    //    s :?> Ast.ExpressionStatement
    let isInfix (s: Ast.Expression) =
        match s with
        | :? Ast.InfixExpression as ie -> true
        | _ -> false
    let asInfix (e: Ast.Expression) =
        match e with
        | :? Ast.InfixExpression as infix ->
            Ok infix
        | _ -> Error "Not an infix expression"
    let asInfixFromStatement (s: Ast.Statement) =
        let asExprState = asExpressionStatement s
        match asExprState with
        | Ok es ->
            let infixExpr = asInfix es.expression
            match infixExpr with
            | Ok infix ->
                Ok infix
            | Error msg -> Error msg
        | Error msg -> Error msg
    let testResultInfix (infix) (infixFunc) =
        match infix with
        | Ok ie ->
            infixFunc ie
        | Error msg -> Assert.Fail msg

    let testIntegerLiteral (il : Ast.Expression) (value) =
        let integerTest = asInteger il
        match integerTest with
        | Some i ->
            Assert.Equal(i.value, value)

            Assert.Equal(i.token.Literal, (sprintf "%d" value))
        | None ->
            Assert.False(true, $"{il.TokenLiteral()} is not an integer")
    let AssertNoParseErrors (p: Parser.ParserState) =
        //if errors, maybe print to err out
        //TODO - maybe include parser errors in the output
        //TODO - maybe print to stderr
        let errorMessage =  
            if p.errors.Count > 0 then 
                let error = p.errors.ToArray() |> Array.reduce (fun a b -> sprintf "%s\n%s" a b)
                error
            else ""
        Assert.True(0 = p.errors.Count, errorMessage)


    let buildInfixExpressionTest (leftValue: int) (operator: string) (rightValue: int) =
        fun (ie: Ast.InfixExpression) ->
            testIntegerLiteral ie.left leftValue
            Assert.Equal(operator, ie.operator)
            testIntegerLiteral ie.right rightValue

    [<Fact>]
    let ``Can parse integer expression`` () =
        let input = "5"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let es = asExpressionStatement modd.statements.[0]

        match es with
        | Ok exprState ->
            testIntegerLiteral exprState.expression 5
        | Error msg -> Assert.Fail(msg)

    [<Fact>]
    let ``Can parse arithmetic expression`` () =
        let input = "2 + 4"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let infix = asInfixFromStatement modd.statements.[0]

        let infixTest = buildInfixExpressionTest 2 "+" 4
        
        testResultInfix infix infixTest

    [<Fact>]
    let ``Can parse combined arithmetic expression`` () =
        let input = "2 + 4 - 1"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let infix = asInfixFromStatement modd.statements.[0]

        match infix with
        | Ok ie ->

            Assert.True(isInfix ie.left)

            let infix2 = asInfix ie.left
            let infixTest = buildInfixExpressionTest 2 "+" 4
            testResultInfix infix2 infixTest

            Assert.Equal("-", ie.operator)
            testIntegerLiteral ie.right 1

        | Error msg -> Assert.Fail(msg)

    [<Fact>]
    let ``Can parse multiplication expression`` () =
        let input = "2 * 4"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let infix = asInfixFromStatement modd.statements.[0]
        
        let infixTest = buildInfixExpressionTest 2 "*" 4
        testResultInfix infix infixTest

    [<Fact>]
    let ``Can parse division expression`` () =
        let input = "4 / 2"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let infix = asInfixFromStatement modd.statements.[0]
        
        let infixTest = buildInfixExpressionTest 4 "/" 2

        testResultInfix infix infixTest
        

    [<Fact>]
    let ``Can parse parenthesis expression`` () =
        let input = "(2 + 4) / 2"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let infix = asInfixFromStatement modd.statements.[0]

        match infix with
        | Ok ie ->
            Assert.True(isInfix ie.left)

            let infix2 = asInfix ie.left
        
            let infixTest = buildInfixExpressionTest 2 "+" 4
            testResultInfix infix2 infixTest

            Assert.Equal("/", ie.operator)
            testIntegerLiteral ie.right 2
        | Error msg -> Assert.Fail msg

    [<Fact>]
    let ``Can parse simple let statement`` () =
        let input = "let x = 3;"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let letState = asLetStatement modd.statements.[0]

        match letState with
        | Ok ls ->
            Assert.Equal("x", ls.name.value)
            testIntegerLiteral ls.value 3
        | Error msg -> Assert.Fail msg

    [<Fact>]
    let ``Can parse simple let statement with identifier`` () =
        let input = "let x = 3; x"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(2, modd.statements.Length)

        let letState = asLetStatement modd.statements.[0]

        match letState with
        | Ok ls ->
            Assert.Equal("x", ls.name.value)
            testIntegerLiteral ls.value 3
        | Error msg -> Assert.Fail msg

        let identifier = asIdentifierFromStatement modd.statements.[1]
        match identifier with
        | Ok iden ->
            Assert.Equal("x", iden.value)
        | Error msg -> Assert.Fail msg

    [<Fact>]
    let ``Can parse addition let statement`` () =
        let input = "let distance = 3 + 2;"

        let lexer = Lexer.createLexer input
        let parser = Parser.createParser lexer
        let modd = Parser.parseModule parser

        AssertNoParseErrors parser

        Assert.Equal(1, modd.statements.Length)

        let letState = asLetStatement modd.statements.[0]

        match letState with
        | Ok ls ->
            Assert.Equal("distance", ls.name.value)

            let infix2 = asInfix ls.value
            let infixTest = buildInfixExpressionTest 3 "+" 2
            testResultInfix infix2 infixTest
        | Error msg -> Assert.Fail msg


