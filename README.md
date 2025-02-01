# WauxLang

WauxLang - A language that compiles to WASM bytecode.

This is currently an experimental language/compiler targeting WASM as it's runtime. Compiler is written in F#.

Should I use this? Probably not.

This is currently alpha software. You'll run into bugs and issues, potentially for no unexplained reason. Here be dragons.

Why Waux? What does that word mean? How do you pronounce it?
  * Pronounced like the English work "walk" [1](https://www.collinsdictionary.com/us/dictionary/english/wauk)
  * "A Scots word for wake"
  * Why Waux?
    * I googled "words that start with wa" and I liked this one the best.

## Installing as Nuget Tool

The fastest way to get started is to install via nuget tool.

* Install a flavor of .NET 8 from [microsoft](https://dotnet.microsoft.com/en-us/download)
* Run the following command: `dotnet tool install --global Waux.Lang.Cli`
  * To update if already installed: `dotnet tool update -g Waux.Lang.Cli`
* You now have access to the `waux` cli tool

## Running

How to compile and use.

* Write a valid `.waux` program
  * See [examples](./examples/waux/) to get started
  * Right now, every waux file needs a parameterless method named `main` that returns an int32 variable.
  * This will be relaxed in the future
* Run `waux compile <.waux file>` to generate a wasm file
* Run `waux run <.wasm file>` to run the generated wasm
  * Can also run in any other wasm compliant runtime.

## Language Overview

> Please see the [examples](./examples/waux/) folder for valid programs

Waux currently supports the following language concepts:
* Functions
* While loops
* If/else blocks
* Most boolean comparisons
* Most integer operations
  * Only supports int32 values at the moment
  * Support operator precedence

### Only Supports int32 operations

Right now, waux only supports int32 operations. Any other number types will lead to undefined behavior.

### Adding

```
5 + 2
```

Compiles into:

```wat
(module
  (type $t0 (func (result i32)))
  (func $main (export "main") (type $t0) (result i32)
    (i32.add
      (i32.const 5)
      (i32.const 2))))
```

### Subtraction

`10 - 2`

Compiles into:

```wat
(module
  (type $t0 (func (result i32)))
  (func $main (export "main") (type $t0) (result i32)
    (i32.sub
      (i32.const 10)
      (i32.const 2))))
```

### Operator Precedence

It also currently handles operator precedence:

`10 + 10 / 5 * 2 - 1`

Compiles into:

```wat
(module
  (type $t0 (func (result i32)))
  (func $main (export "main") (type $t0) (result i32)
    (i32.sub
      (i32.add
        (i32.const 10)
        (i32.mul
          (i32.div_s
            (i32.const 10)
            (i32.const 5))
          (i32.const 2)))
      (i32.const 1))))
```

### Variable Assignments

`let x = 42; x`

```wat
(module
  (type $t0 (func (result i32)))
  (func $main (export "main") (type $t0) (result i32)
    (local $l0 i32)
    (local.set $l0
      (i32.const 42))
    (local.get $l0)))

```

### Function Calls

```
func main() { 
  add(1,2); 
}
func add(x, y) {
  let mul = multi(x, y);
  mul + x + y;
} 
func multi(z, v) {
  z * v; 
}
```

```wat
(module
  (type $t0 (func (result i32)))
  (type $t1 (func (param i32 i32) (result i32)))
  (type $t2 (func (param i32 i32) (result i32)))
  (func $main (export "main") (type $t0) (result i32)
    (call $add
      (i32.const 1)
      (i32.const 2)))
  (func $add (export "add") (type $t1) (param $p0 i32) (param $p1 i32) (result i32)
    (local $l2 i32)
    (local.set $l2
      (call $multi
        (local.get $p0)
        (local.get $p1)))
    (i32.add
      (i32.add
        (local.get $l2)
        (local.get $p0))
      (local.get $p1)))
  (func $multi (export "multi") (type $t2) (param $p0 i32) (param $p1 i32) (result i32)
    (i32.mul
      (local.get $p0)
      (local.get $p1))))
```

## Code Formatting

This repo currently uses [fantomas](https://fsprojects.github.io/fantomas/) in order to format the compiler.

* To install the tool
  * `dotnet tool install fantomas`
  * `dotnet tool restore`
* To run and format everything
  * `./format.bat`
  * Or `dotnet fantomas -r .`